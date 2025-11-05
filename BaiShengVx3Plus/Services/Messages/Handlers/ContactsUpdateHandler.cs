using System;
using System.Text.Json;
using System.Threading.Tasks;
using BaiShengVx3Plus.Models;
using BaiShengVx3Plus.Contracts;
using BaiShengVx3Plus.Contracts.Messages;

namespace BaiShengVx3Plus.Services.Messages.Handlers
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

                // 统一调用 ContactDataService 处理
                var contacts = await _contactDataService.ProcessContactsAsync(data);

                _logService.Info("ContactsUpdateHandler", $"✓ 处理完成，共 {contacts.Count} 个联系人");
            }
            catch (Exception ex)
            {
                _logService.Error("ContactsUpdateHandler", "处理联系人更新失败", ex);
            }
        }
    }
}

