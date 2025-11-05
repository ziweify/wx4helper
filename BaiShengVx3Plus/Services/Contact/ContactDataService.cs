using System;
using System.Collections.Generic;
using SQLite;
using System.Text.Json;
using System.Threading.Tasks;
using BaiShengVx3Plus.Models;
using BaiShengVx3Plus.Contracts;

namespace BaiShengVx3Plus.Services.Contact
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
        public async Task<List<WxContact>> ProcessContactsAsync(JsonElement data)
        {
            try
            {
                _logService.Info("ContactDataService", "开始处理联系人数据");

                // 1. 解析联系人数据
                var contacts = ParseContacts(data);
                _logService.Info("ContactDataService", $"✓ 解析到 {contacts.Count} 个联系人");

                // 2. 触发事件通知 UI（不再保存到数据库，由 UI 层决定如何使用）
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

        // 🔥 ContactDataService 不再负责数据库操作
        // 联系人数据由 UI 层（VxMain）决定如何存储和使用
    }
}

