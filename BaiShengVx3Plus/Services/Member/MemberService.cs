using System;
using System.ComponentModel;
using System.Data.SQLite;
using BaiShengVx3Plus.Models;
using BaiShengVx3Plus.Contracts;

namespace BaiShengVx3Plus.Services.Member
{
    /// <summary>
    /// 会员服务实现（简化版，配合 PropertyChangeTracker 使用）
    /// 
    /// 核心机制：
    /// 1. GetAllMembers() 返回 BindingList，自动追踪所有会员
    /// 2. 修改会员属性后，PropertyChangeTracker 自动保存单个字段
    /// 3. 只需要 Add/Delete 方法，Update 由 PropertyChangeTracker 自动处理
    /// </summary>
    public class MemberService : IMemberService
    {
        private readonly IDatabaseService _dbService;
        private readonly ILogService _logService;
        private readonly IPropertyChangeTracker _propertyTracker;

        public event EventHandler? MembersChanged;

        public MemberService(
            IDatabaseService dbService, 
            ILogService logService,
            IPropertyChangeTracker propertyTracker)
        {
            _dbService = dbService;
            _logService = logService;
            _propertyTracker = propertyTracker;
        }

        /// <summary>
        /// 获取所有会员（自动追踪属性变化）
        /// </summary>
        public BindingList<V2Member> GetAllMembers()
        {
            var members = new BindingList<V2Member>();

            try
            {
                using var conn = _dbService.GetConnection();
                using var cmd = new SQLiteCommand(@"
                    SELECT 
                        Id, MemberId, MemberName, MemberAlias, MemberAmount, 
                        MemberState, TimeStampCreate, TimeStampUpdate, 
                        TimeStampBet, Extra
                    FROM members
                    ORDER BY TimeStampCreate DESC", conn);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var member = MapReaderToMember(reader);
                    
                    // 🔥 自动追踪这个会员的属性变化
                    _propertyTracker.Track(member, "members");
                    
                    members.Add(member);
                }

                _logService.Info("MemberService", $"✓ 加载 {members.Count} 个会员（已自动追踪）");
            }
            catch (Exception ex)
            {
                _logService.Error("MemberService", "获取会员列表失败", ex);
            }

            return members;
        }

        /// <summary>
        /// 根据ID获取会员（自动追踪属性变化）
        /// </summary>
        public V2Member? GetMemberById(long id)
        {
            try
            {
                using var conn = _dbService.GetConnection();
                using var cmd = new SQLiteCommand(@"
                    SELECT 
                        Id, MemberId, MemberName, MemberAlias, MemberAmount, 
                        MemberState, TimeStampCreate, TimeStampUpdate, 
                        TimeStampBet, Extra
                    FROM members
                    WHERE Id = @Id", conn);

                cmd.Parameters.AddWithValue("@Id", id);

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    var member = MapReaderToMember(reader);
                    
                    // 🔥 自动追踪这个会员的属性变化
                    _propertyTracker.Track(member, "members");
                    
                    return member;
                }
            }
            catch (Exception ex)
            {
                _logService.Error("MemberService", $"获取会员失败 (ID: {id})", ex);
            }

            return null;
        }

        /// <summary>
        /// 添加会员（立即写入数据库，并自动追踪）
        /// </summary>
        public long AddMember(V2Member member)
        {
            try
            {
                using var conn = _dbService.GetConnection();
                using var transaction = conn.BeginTransaction();

                try
                {
                    using var cmd = new SQLiteCommand(@"
                        INSERT INTO members (
                            MemberId, MemberName, MemberAlias, MemberAmount, 
                            MemberState, TimeStampCreate, TimeStampUpdate, 
                            TimeStampBet, Extra
                        ) VALUES (
                            @MemberId, @MemberName, @MemberAlias, @MemberAmount, 
                            @MemberState, @TimeStampCreate, @TimeStampUpdate, 
                            @TimeStampBet, @Extra
                        );
                        SELECT last_insert_rowid();", conn, transaction);

                    AddMemberParameters(cmd, member);

                    var newId = (long)cmd.ExecuteScalar();
                    member.Id = newId;

                    transaction.Commit();

                    // 🔥 追踪新添加的会员
                    _propertyTracker.Track(member, "members");

                    _logService.Info("MemberService", $"✓ 添加会员成功: {member.MemberName} (ID: {newId})");
                    MembersChanged?.Invoke(this, EventArgs.Empty);

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
                _logService.Error("MemberService", $"添加会员失败: {member.MemberName}", ex);
                throw;
            }
        }

        /// <summary>
        /// 删除会员（立即从数据库删除，停止追踪）
        /// </summary>
        public void DeleteMember(long id)
        {
            try
            {
                // 先找到这个会员，停止追踪
                var member = GetMemberById(id);
                if (member != null)
                {
                    _propertyTracker.Untrack(member);
                }

                using var conn = _dbService.GetConnection();
                using var transaction = conn.BeginTransaction();

                try
                {
                    using var cmd = new SQLiteCommand("DELETE FROM members WHERE Id = @Id", conn, transaction);
                    cmd.Parameters.AddWithValue("@Id", id);

                    var affected = cmd.ExecuteNonQuery();

                    transaction.Commit();

                    if (affected > 0)
                    {
                        _logService.Info("MemberService", $"✓ 删除会员成功 (ID: {id})");
                        MembersChanged?.Invoke(this, EventArgs.Empty);
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
                _logService.Error("MemberService", $"删除会员失败 (ID: {id})", ex);
                throw;
            }
        }

        // ========================================
        // 私有辅助方法
        // ========================================

        private void AddMemberParameters(SQLiteCommand cmd, V2Member member)
        {
            cmd.Parameters.AddWithValue("@MemberId", member.MemberId);
            cmd.Parameters.AddWithValue("@MemberName", member.MemberName ?? string.Empty);
            cmd.Parameters.AddWithValue("@MemberAlias", member.MemberAlias ?? string.Empty);
            cmd.Parameters.AddWithValue("@MemberAmount", member.MemberAmount);
            cmd.Parameters.AddWithValue("@MemberState", (int)member.MemberState);
            cmd.Parameters.AddWithValue("@TimeStampCreate", member.TimeStampCreate);
            cmd.Parameters.AddWithValue("@TimeStampUpdate", member.TimeStampUpdate);
            cmd.Parameters.AddWithValue("@TimeStampBet", member.TimeStampBet);
            cmd.Parameters.AddWithValue("@Extra", member.Extra ?? string.Empty);
        }

        private V2Member MapReaderToMember(SQLiteDataReader reader)
        {
            return new V2Member
            {
                Id = reader.GetInt64(0),
                MemberId = reader.GetInt64(1),
                MemberName = reader.GetString(2),
                MemberAlias = reader.GetString(3),
                MemberAmount = reader.GetDouble(4),
                MemberState = (MemberState)reader.GetInt32(5),
                TimeStampCreate = reader.GetInt64(6),
                TimeStampUpdate = reader.GetInt64(7),
                TimeStampBet = reader.GetInt64(8),
                Extra = reader.GetString(9)
            };
        }
    }
}
