using System;
using System.Text.Json;
using System.Threading.Tasks;
using zhaocaimao.Models;
using zhaocaimao.Contracts;
using zhaocaimao.Contracts.Messages;

namespace zhaocaimao.Services.Messages.Handlers
{
    /// <summary>
    /// 联系人更新处理器（处理服务器推送的联系人数据）
    /// </summary>
    public class ContactsUpdateHandler : IMessageHandler
    {
        private readonly ILogService _logService;
        private readonly IContactDataService _contactDataService;

        public ServerMessageType MessageType => ServerMessageType.Unknown; // 暂时用 Unknown，可以扩展枚举

        public ContactsUpdateHandler(ILogService logService, IContactDataService contactDataService)
        {
            _logService = logService;
            _contactDataService = contactDataService;
        }

        public async Task HandleAsync(JsonElement data)
        {
            try
            {
                _logService.Info("ContactsUpdateHandler", "📇 收到联系人更新推送");

                // 🔥 使用静态方法解析 JSON
                var contacts = Services.Contact.ContactDataService.ParseContactsFromJson(data);
                _logService.Info("ContactsUpdateHandler", $"✓ 联系人解析成功，共 {contacts.Count} 个");

                // 🔥 统一调用 ContactDataService 处理并触发事件
                await _contactDataService.ProcessContactsAsync(contacts);
                _logService.Info("ContactsUpdateHandler", $"✓ 处理完成，共 {contacts.Count} 个联系人");
            }
            catch (Exception ex)
            {
                _logService.Error("ContactsUpdateHandler", "处理联系人更新失败", ex);
            }
        }
    }
}

