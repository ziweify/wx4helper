using System;
using System.ComponentModel;
using System.Data.SQLite;
using BaiShengVx3Plus.Models;
using BaiShengVx3Plus.Contracts;
using BaiShengVx3Plus.Core;

namespace BaiShengVx3Plus.Services.Order
{
    /// <summary>
    /// 订单服务实现（简化版，配合 PropertyChangeTracker 使用）
    /// 
    /// 核心机制：
    /// 1. GetAllOrders() 返回 TrackableBindingList，自动追踪所有订单
    /// 2. 修改订单属性后，PropertyChangeTracker 自动保存单个字段
    /// 3. 只需要 Add/Delete 方法，Update 由 PropertyChangeTracker 自动处理
    /// </summary>
    public class OrderService : IOrderService
    {
        private readonly IDatabaseService _dbService;
        private readonly ILogService _logService;
        private readonly IPropertyChangeTracker _propertyTracker;

        public event EventHandler? OrdersChanged;

        public OrderService(
            IDatabaseService dbService, 
            ILogService logService,
            IPropertyChangeTracker propertyTracker)
        {
            _dbService = dbService;
            _logService = logService;
            _propertyTracker = propertyTracker;
        }

        /// <summary>
        /// 获取所有订单（自动追踪属性变化）
        /// </summary>
        public TrackableBindingList<V2MemberOrder> GetAllOrders()
        {
            var orders = new TrackableBindingList<V2MemberOrder>();

            try
            {
                using var conn = _dbService.GetConnection();
                using var cmd = new SQLiteCommand(@"
                    SELECT 
                        Id, MemberId, MemberName, OrderId, OrderStatus, 
                        OrderType, OrderAmountPlan, OrderAmount, OrderResult, 
                        OrderTarget, OrderPlace, TimeStampCreate, TimeStampUpdate, 
                        TimeStampBet, Extra
                    FROM orders
                    ORDER BY TimeStampCreate DESC", conn);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var order = MapReaderToOrder(reader);
                    
                    // 🔥 自动追踪这个订单的属性变化
                    _propertyTracker.Track(order, "orders");
                    
                    orders.Add(order);
                }

                _logService.Info("OrderService", $"✓ 加载 {orders.Count} 个订单（已自动追踪）");
            }
            catch (Exception ex)
            {
                _logService.Error("OrderService", "获取订单列表失败", ex);
            }

            return orders;
        }

        /// <summary>
        /// 根据ID获取订单（自动追踪属性变化）
        /// </summary>
        public V2MemberOrder? GetOrderById(long id)
        {
            try
            {
                using var conn = _dbService.GetConnection();
                using var cmd = new SQLiteCommand(@"
                    SELECT 
                        Id, MemberId, MemberName, OrderId, OrderStatus, 
                        OrderType, OrderAmountPlan, OrderAmount, OrderResult, 
                        OrderTarget, OrderPlace, TimeStampCreate, TimeStampUpdate, 
                        TimeStampBet, Extra
                    FROM orders
                    WHERE Id = @Id", conn);

                cmd.Parameters.AddWithValue("@Id", id);

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    var order = MapReaderToOrder(reader);
                    
                    // 🔥 自动追踪这个订单的属性变化
                    _propertyTracker.Track(order, "orders");
                    
                    return order;
                }
            }
            catch (Exception ex)
            {
                _logService.Error("OrderService", $"获取订单失败 (ID: {id})", ex);
            }

            return null;
        }

        /// <summary>
        /// 根据会员ID获取订单（自动追踪属性变化）
        /// </summary>
        public BindingList<V2MemberOrder> GetOrdersByMemberId(long memberId)
        {
            var orders = new BindingList<V2MemberOrder>();

            try
            {
                using var conn = _dbService.GetConnection();
                using var cmd = new SQLiteCommand(@"
                    SELECT 
                        Id, MemberId, MemberName, OrderId, OrderStatus, 
                        OrderType, OrderAmountPlan, OrderAmount, OrderResult, 
                        OrderTarget, OrderPlace, TimeStampCreate, TimeStampUpdate, 
                        TimeStampBet, Extra
                    FROM orders
                    WHERE MemberId = @MemberId
                    ORDER BY TimeStampCreate DESC", conn);

                cmd.Parameters.AddWithValue("@MemberId", memberId);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var order = MapReaderToOrder(reader);
                    
                    // 🔥 自动追踪这个订单的属性变化
                    _propertyTracker.Track(order, "orders");
                    
                    orders.Add(order);
                }

                _logService.Debug("OrderService", $"会员 {memberId} 的订单数: {orders.Count}");
            }
            catch (Exception ex)
            {
                _logService.Error("OrderService", $"获取会员订单失败 (MemberId: {memberId})", ex);
            }

            return orders;
        }

        /// <summary>
        /// 添加订单（立即写入数据库，并自动追踪）
        /// </summary>
        public long AddOrder(V2MemberOrder order)
        {
            try
            {
                using var conn = _dbService.GetConnection();
                using var transaction = conn.BeginTransaction();

                try
                {
                    // 设置时间戳
                    var now = DateTimeOffset.Now.ToUnixTimeSeconds();
                    order.TimeStampCreate = now;
                    order.TimeStampUpdate = now;

                    using var cmd = new SQLiteCommand(@"
                        INSERT INTO Orders (
                            MemberId, OrderNo, Amount, Status, OrderType, TimeStampBet, Remark,
                            CreatedAt, UpdatedAt
                        ) VALUES (
                            @MemberId, @OrderNo, @Amount, @Status, @OrderType, @TimeStampBet, @Remark,
                            datetime('now'), datetime('now')
                        );
                        SELECT last_insert_rowid();", conn, transaction);

                    cmd.Parameters.AddWithValue("@MemberId", order.MemberId);
                    cmd.Parameters.AddWithValue("@OrderNo", order.IssueId.ToString());
                    cmd.Parameters.AddWithValue("@Amount", order.AmountTotal);
                    cmd.Parameters.AddWithValue("@Status", (int)order.OrderStatus);
                    cmd.Parameters.AddWithValue("@OrderType", (int)order.OrderType);
                    cmd.Parameters.AddWithValue("@TimeStampBet", order.TimeStampBet);
                    cmd.Parameters.AddWithValue("@Remark", order.Notes ?? "");

                    var newId = (long)cmd.ExecuteScalar()!;
                    order.Id = newId;

                    transaction.Commit();

                    // 🔥 追踪新添加的订单
                    _propertyTracker.Track(order, "Orders");

                    _logService.Info("OrderService", $"✓ 添加订单成功: 期号{order.IssueId} (ID: {newId})");
                    OrdersChanged?.Invoke(this, EventArgs.Empty);

                    return newId;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logService.Error("OrderService", $"添加订单失败: 期号{order.IssueId}", ex);
                throw;
            }
        }

        /// <summary>
        /// 删除订单（立即从数据库删除，停止追踪）
        /// </summary>
        public void DeleteOrder(long id)
        {
            try
            {
                using var conn = _dbService.GetConnection();
                using var transaction = conn.BeginTransaction();

                try
                {
                    using var cmd = new SQLiteCommand("DELETE FROM Orders WHERE Id = @Id", conn, transaction);
                    cmd.Parameters.AddWithValue("@Id", id);

                    var affected = cmd.ExecuteNonQuery();

                    transaction.Commit();

                    if (affected > 0)
                    {
                        _logService.Info("OrderService", $"✓ 删除订单成功 (ID: {id})");
                        OrdersChanged?.Invoke(this, EventArgs.Empty);
                    }
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logService.Error("OrderService", $"删除订单失败 (ID: {id})", ex);
                throw;
            }
        }

        // ========================================
        // 私有辅助方法
        // ========================================

        private void AddOrderParameters(SQLiteCommand cmd, V2MemberOrder order)
        {
            cmd.Parameters.AddWithValue("@MemberId", order.MemberId);
            cmd.Parameters.AddWithValue("@MemberName", order.MemberName ?? string.Empty);
            cmd.Parameters.AddWithValue("@OrderId", order.OrderId ?? string.Empty);
            cmd.Parameters.AddWithValue("@OrderStatus", (int)order.OrderStatus);
            cmd.Parameters.AddWithValue("@OrderType", (int)order.OrderType);
            cmd.Parameters.AddWithValue("@OrderAmountPlan", order.OrderAmountPlan);
            cmd.Parameters.AddWithValue("@OrderAmount", order.OrderAmount);
            cmd.Parameters.AddWithValue("@OrderResult", order.OrderResult ?? string.Empty);
            cmd.Parameters.AddWithValue("@OrderTarget", order.OrderTarget ?? string.Empty);
            cmd.Parameters.AddWithValue("@OrderPlace", order.OrderPlace ?? string.Empty);
            cmd.Parameters.AddWithValue("@TimeStampCreate", order.TimeStampCreate);
            cmd.Parameters.AddWithValue("@TimeStampUpdate", order.TimeStampUpdate);
            cmd.Parameters.AddWithValue("@TimeStampBet", order.TimeStampBet);
            cmd.Parameters.AddWithValue("@Extra", order.Extra ?? string.Empty);
        }

        private V2MemberOrder MapReaderToOrder(SQLiteDataReader reader)
        {
            return new V2MemberOrder
            {
                Id = reader.GetInt64(0),
                MemberId = reader.GetInt64(1),
                MemberName = reader.GetString(2),
                OrderId = reader.GetString(3),
                OrderStatus = (OrderStatus)reader.GetInt32(4),
                OrderType = (OrderType)reader.GetInt32(5),
                OrderAmountPlan = reader.GetDouble(6),
                OrderAmount = reader.GetDouble(7),
                OrderResult = reader.GetString(8),
                OrderTarget = reader.GetString(9),
                OrderPlace = reader.GetString(10),
                TimeStampCreate = reader.GetInt64(11),
                TimeStampUpdate = reader.GetInt64(12),
                TimeStampBet = reader.GetInt64(13),
                Extra = reader.GetString(14)
            };
        }
    }
}
