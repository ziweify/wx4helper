using System;
using System.ComponentModel;
using BaiShengVx3Plus.Models;

namespace BaiShengVx3Plus.Contracts
{
    /// <summary>
    /// 订单服务接口（简化版，配合 PropertyChangeTracker 使用）
    /// 注意：修改订单属性后，PropertyChangeTracker 会自动保存单个字段
    /// </summary>
    public interface IOrderService
    {
        /// <summary>
        /// 获取所有订单（从数据库，自动追踪属性变化）
        /// </summary>
        BindingList<V2MemberOrder> GetAllOrders();

        /// <summary>
        /// 根据ID获取订单（自动追踪属性变化）
        /// </summary>
        V2MemberOrder? GetOrderById(long id);

        /// <summary>
        /// 根据会员ID获取订单（自动追踪属性变化）
        /// </summary>
        BindingList<V2MemberOrder> GetOrdersByMemberId(long memberId);

        /// <summary>
        /// 添加订单（立即写入数据库，自动追踪属性变化）
        /// </summary>
        /// <returns>新增的订单ID</returns>
        long AddOrder(V2MemberOrder order);

        /// <summary>
        /// 删除订单（立即从数据库删除，停止追踪）
        /// </summary>
        void DeleteOrder(long id);

        /// <summary>
        /// 订单数据变化事件（通知 UI 更新）
        /// </summary>
        event EventHandler? OrdersChanged;
    }
}

