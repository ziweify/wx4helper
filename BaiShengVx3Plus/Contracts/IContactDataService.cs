using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using BaiShengVx3Plus.Models;

namespace BaiShengVx3Plus.Contracts
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
        /// 只负责解析数据并触发事件，不再保存到数据库（由 UI 层决定如何存储）
        /// </summary>
        /// <param name="data">联系人 JSON 数据</param>
        /// <returns>解析后的联系人列表</returns>
        Task<List<WxContact>> ProcessContactsAsync(JsonElement data);

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

