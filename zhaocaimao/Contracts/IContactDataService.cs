using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using zhaocaimao.Models;

namespace zhaocaimao.Contracts
{
    /// <summary>
    /// 联系人数据处理服务接口
    /// </summary>
    public interface IContactDataService
    {
        /// <summary>
        /// 设置当前登录的微信 ID（用于数据库表名）
        /// </summary>
        /// <param name="wxid">微信 ID</param>
        void SetCurrentWxid(string wxid);

        /// <summary>
        /// 处理联系人数据（统一入口，无论是主动请求还是服务器推送）
        /// 触发事件通知 UI 层更新
        /// </summary>
        /// <param name="contacts">已解析的联系人列表</param>
        /// <param name="filterType">过滤类型（默认全部）</param>
        /// <returns>过滤后的联系人列表</returns>
        Task<List<WxContact>> ProcessContactsAsync(List<WxContact> contacts, ContactFilterType filterType = ContactFilterType.全部);

        /// <summary>
        /// 联系人数据更新事件
        /// </summary>
        event EventHandler<ContactsUpdatedEventArgs>? ContactsUpdated;
    }

    /// <summary>
    /// 联系人更新事件参数
    /// </summary>
    public class ContactsUpdatedEventArgs : EventArgs
    {
        public List<WxContact> Contacts { get; set; } = new();
        public DateTime UpdateTime { get; set; } = DateTime.Now;
        public string Source { get; set; } = string.Empty; // "Push" or "Refresh"
    }
}

