using System;
using System.Collections.Generic;
using SQLite;
using System.Text.Json;
using System.Threading.Tasks;
using zhaocaimao.Models;
using zhaocaimao.Contracts;

namespace zhaocaimao.Services.Contact
{
    /// <summary>
    /// 联系人数据处理服务实现（使用 ORM）
    /// </summary>
    public class ContactDataService : IContactDataService
    {
        private readonly ILogService _logService;
        private string? _currentWxid;

        public event EventHandler<ContactsUpdatedEventArgs>? ContactsUpdated;

        public ContactDataService(ILogService logService)
        {
            _logService = logService;
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
        /// <param name="contacts">联系人列表</param>
        /// <param name="filterType">过滤类型（默认全部）</param>
        public Task<List<WxContact>> ProcessContactsAsync(List<WxContact> contacts, ContactFilterType filterType = ContactFilterType.全部)
        {
            try
            {
                _logService.Info("ContactDataService", $"开始处理 {contacts.Count} 个联系人数据，过滤类型: {filterType}");

                // 🔥 根据过滤类型过滤联系人
                var filteredContacts = FilterContacts(contacts, filterType);
                _logService.Info("ContactDataService", $"过滤后联系人数量: {filteredContacts.Count} (原始: {contacts.Count})");

                // 触发事件通知 UI（不再保存到数据库，由 UI 层决定如何使用）
                _logService.Info("ContactDataService", $"📢 准备触发 ContactsUpdated 事件，联系人数量: {filteredContacts.Count}");
                ContactsUpdated?.Invoke(this, new ContactsUpdatedEventArgs
                {
                    Contacts = filteredContacts,
                    UpdateTime = DateTime.Now,
                    Source = "Process"
                });
                _logService.Info("ContactDataService", $"✅ ContactsUpdated 事件已触发");

                return Task.FromResult(filteredContacts);
            }
            catch (Exception ex)
            {
                _logService.Error("ContactDataService", "处理联系人数据失败", ex);
                return Task.FromResult(new List<WxContact>());
            }
        }
        
        /// <summary>
        /// 根据过滤类型过滤联系人
        /// </summary>
        /// <param name="contacts">原始联系人列表</param>
        /// <param name="filterType">过滤类型</param>
        /// <returns>过滤后的联系人列表</returns>
        private List<WxContact> FilterContacts(List<WxContact> contacts, ContactFilterType filterType)
        {
            return filterType switch
            {
                ContactFilterType.联系人 => contacts.FindAll(c => !c.Wxid.Contains("@")),  // wxid 不含 @
                ContactFilterType.群组 => contacts.FindAll(c => c.Wxid.Contains("@")),     // wxid 包含 @
                ContactFilterType.全部 => contacts,                                         // 不过滤
                _ => contacts
            };
        }
        
        /// <summary>
        /// 从 JSON 解析联系人列表（静态方法，无需依赖服务实例）
        /// </summary>
        /// <param name="data">JSON 数据</param>
        /// <returns>解析后的联系人列表</returns>
        public static List<WxContact> ParseContactsFromJson(JsonElement data)
        {
            return ParseContacts(data);
        }

        /// <summary>
        /// 解析联系人数据（静态方法）
        /// </summary>
        private static List<WxContact> ParseContacts(JsonElement data)
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
            catch (Exception)
            {
                // 静态方法无法使用 _logService，调用者负责日志
            }

            return contacts;
        }

        /// <summary>
        /// 解析单个联系人（静态方法）
        /// </summary>
        private static WxContact? ParseContactItem(JsonElement item)
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
            catch (Exception)
            {
                // 静态方法无法使用 _logService，调用者负责日志
                return null;
            }
        }

        // 🔥 ContactDataService 不再负责数据库操作
        // 联系人数据由 UI 层（VxMain）决定如何存储和使用
    }
}

