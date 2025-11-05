using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text.Json;
using System.Threading.Tasks;
using BaiShengVx3Plus.Models;
using BaiShengVx3Plus.Contracts;

namespace BaiShengVx3Plus.Services.Contact
{
    /// <summary>
    /// 联系人数据处理服务实现
    /// </summary>
    public class ContactDataService : IContactDataService
    {
        private readonly ILogService _logService;
        private readonly IDatabaseService _dbService;
        private string? _currentWxid;

        public event EventHandler<ContactsUpdatedEventArgs>? ContactsUpdated;

        public ContactDataService(ILogService logService, IDatabaseService dbService)
        {
            _logService = logService;
            _dbService = dbService;
        }

        /// <summary>
        /// 设置当前登录的微信 ID
        /// </summary>
        public void SetCurrentWxid(string wxid)
        {
            _currentWxid = wxid;
            _logService.Info("ContactDataService", $"设置当前微信 ID: {wxid}");
        }

        /// <summary>
        /// 处理联系人数据（统一入口）
        /// </summary>
        public async Task<List<WxContact>> ProcessContactsAsync(JsonElement data)
        {
            try
            {
                _logService.Info("ContactDataService", "开始处理联系人数据");

                // 1. 解析联系人数据
                var contacts = ParseContacts(data);
                _logService.Info("ContactDataService", $"解析到 {contacts.Count} 个联系人");

                // 2. 保存到数据库
                await SaveContactsAsync(contacts);

                // 3. 触发事件通知 UI
                ContactsUpdated?.Invoke(this, new ContactsUpdatedEventArgs
                {
                    Contacts = contacts,
                    UpdateTime = DateTime.Now,
                    Source = "Process"
                });

                return contacts;
            }
            catch (Exception ex)
            {
                _logService.Error("ContactDataService", "处理联系人数据失败", ex);
                return new List<WxContact>();
            }
        }

        /// <summary>
        /// 解析联系人数据
        /// </summary>
        private List<WxContact> ParseContacts(JsonElement data)
        {
            var contacts = new List<WxContact>();

            try
            {
                // 判断是数组还是对象
                if (data.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in data.EnumerateArray())
                    {
                        var contact = ParseContactItem(item);
                        if (contact != null)
                        {
                            contacts.Add(contact);
                        }
                    }
                }
                else if (data.ValueKind == JsonValueKind.Object)
                {
                    // 单个联系人
                    var contact = ParseContactItem(data);
                    if (contact != null)
                    {
                        contacts.Add(contact);
                    }
                }
            }
            catch (Exception ex)
            {
                _logService.Error("ContactDataService", "解析联系人数据失败", ex);
            }

            return contacts;
        }

        /// <summary>
        /// 解析单个联系人
        /// </summary>
        private WxContact? ParseContactItem(JsonElement item)
        {
            try
            {
                var contact = new WxContact();

                // 微信ID（必填）
                if (item.TryGetProperty("username", out var username))
                {
                    contact.Wxid = username.GetString() ?? string.Empty;
                }
                else if (item.TryGetProperty("wxid", out var wxid))
                {
                    contact.Wxid = wxid.GetString() ?? string.Empty;
                }

                if (string.IsNullOrEmpty(contact.Wxid))
                {
                    return null; // 没有 wxid 的不要
                }

                // 昵称
                if (item.TryGetProperty("nick_name", out var nickName))
                {
                    contact.Nickname = nickName.GetString() ?? string.Empty;
                }
                else if (item.TryGetProperty("nickname", out var nickname))
                {
                    contact.Nickname = nickname.GetString() ?? string.Empty;
                }

                // 微信号
                if (item.TryGetProperty("alias", out var alias))
                {
                    contact.Account = alias.GetString() ?? string.Empty;
                }

                // 备注
                if (item.TryGetProperty("remark", out var remark))
                {
                    contact.Remark = remark.GetString() ?? string.Empty;
                }

                // 头像
                if (item.TryGetProperty("small_head_url", out var avatar))
                {
                    contact.Avatar = avatar.GetString() ?? string.Empty;
                }

                // 个性签名
                if (item.TryGetProperty("description", out var description))
                {
                    // WxContact 没有这个字段，可以扩展
                }

                // 是否群组
                if (item.TryGetProperty("chat_room_type", out var chatRoomType))
                {
                    // 🔥 chat_room_type 可能是字符串或整数
                    if (chatRoomType.ValueKind == JsonValueKind.String)
                    {
                        var typeStr = chatRoomType.GetString() ?? "0";
                        contact.IsGroup = int.TryParse(typeStr, out var typeInt) && typeInt > 0;
                    }
                    else if (chatRoomType.ValueKind == JsonValueKind.Number)
                    {
                        contact.IsGroup = chatRoomType.GetInt32() > 0;
                    }
                }

                return contact;
            }
            catch (Exception ex)
            {
                _logService.Error("ContactDataService", $"解析单个联系人失败: {ex.Message}", ex);
                return null;
            }
        }

        /// <summary>
        /// 保存联系人到数据库
        /// </summary>
        public async Task SaveContactsAsync(List<WxContact> contacts)
        {
            if (string.IsNullOrEmpty(_currentWxid))
            {
                _logService.Warning("ContactDataService", "当前微信 ID 为空，无法保存联系人");
                return;
            }

            // 使用 Task.Run 在后台线程执行同步数据库操作
            await Task.Run(() =>
            {
                try
                {
                    var conn = _dbService.GetConnection();

                // 创建表（如果不存在）
                var createTableSql = $@"
                    CREATE TABLE IF NOT EXISTS contacts_{_currentWxid} (
                        wxid TEXT PRIMARY KEY,
                        account TEXT,
                        nickname TEXT,
                        remark TEXT,
                        avatar TEXT,
                        sex INTEGER DEFAULT 0,
                        province TEXT,
                        city TEXT,
                        country TEXT,
                        is_group INTEGER DEFAULT 0,
                        update_time INTEGER DEFAULT 0
                    )";

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = createTableSql;
                    cmd.ExecuteNonQuery();
                }

                // 清空旧数据
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $"DELETE FROM contacts_{_currentWxid}";
                    cmd.ExecuteNonQuery();
                }

                // 批量插入新数据
                using (var transaction = conn.BeginTransaction())
                {
                    foreach (var contact in contacts)
                    {
                        using var cmd = conn.CreateCommand();
                        cmd.Transaction = transaction;
                        cmd.CommandText = $@"
                            INSERT INTO contacts_{_currentWxid} 
                            (wxid, account, nickname, remark, avatar, sex, province, city, country, is_group, update_time)
                            VALUES 
                            (@wxid, @account, @nickname, @remark, @avatar, @sex, @province, @city, @country, @is_group, @update_time)";

                        cmd.Parameters.AddWithValue("@wxid", contact.Wxid);
                        cmd.Parameters.AddWithValue("@account", contact.Account);
                        cmd.Parameters.AddWithValue("@nickname", contact.Nickname);
                        cmd.Parameters.AddWithValue("@remark", contact.Remark);
                        cmd.Parameters.AddWithValue("@avatar", contact.Avatar);
                        cmd.Parameters.AddWithValue("@sex", contact.Sex);
                        cmd.Parameters.AddWithValue("@province", contact.Province);
                        cmd.Parameters.AddWithValue("@city", contact.City);
                        cmd.Parameters.AddWithValue("@country", contact.Country);
                        cmd.Parameters.AddWithValue("@is_group", contact.IsGroup ? 1 : 0);
                        cmd.Parameters.AddWithValue("@update_time", DateTimeOffset.Now.ToUnixTimeSeconds());

                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }

                    _logService.Info("ContactDataService", $"成功保存 {contacts.Count} 个联系人到数据库");
                }
                catch (Exception ex)
                {
                    _logService.Error("ContactDataService", "保存联系人到数据库失败", ex);
                }
            });
        }

        /// <summary>
        /// 从数据库加载联系人
        /// </summary>
        public async Task<List<WxContact>> LoadContactsAsync()
        {
            if (string.IsNullOrEmpty(_currentWxid))
            {
                _logService.Warning("ContactDataService", "当前微信 ID 为空，无法加载联系人");
                return new List<WxContact>();
            }

            // 使用 Task.Run 在后台线程执行同步数据库操作
            return await Task.Run(() =>
            {
                var contacts = new List<WxContact>();
                
                try
                {
                    var conn = _dbService.GetConnection();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = $@"
                    SELECT wxid, account, nickname, remark, avatar, sex, province, city, country, is_group
                    FROM contacts_{_currentWxid}
                    ORDER BY nickname";

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var contact = new WxContact
                    {
                        Wxid = reader.GetString(0),
                        Account = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                        Nickname = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                        Remark = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                        Avatar = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                        Sex = reader.IsDBNull(5) ? 0 : reader.GetInt32(5),
                        Province = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                        City = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
                        Country = reader.IsDBNull(8) ? string.Empty : reader.GetString(8),
                        IsGroup = reader.IsDBNull(9) ? false : reader.GetInt32(9) == 1
                    };

                    contacts.Add(contact);
                }

                    _logService.Info("ContactDataService", $"从数据库加载 {contacts.Count} 个联系人");
                }
                catch (Exception ex)
                {
                    _logService.Error("ContactDataService", "从数据库加载联系人失败", ex);
                }

                return contacts;
            });
        }
    }
}

