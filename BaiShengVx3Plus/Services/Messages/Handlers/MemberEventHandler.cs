using System;
using System.Text.Json;
using System.Threading.Tasks;
using BaiShengVx3Plus.Models;

namespace BaiShengVx3Plus.Services.Messages.Handlers
{
    /// <summary>
    /// 群成员加入处理器
    /// </summary>
    public class MemberJoinHandler : IMessageHandler
    {
        private readonly ILogService _logService;

        public ServerMessageType MessageType => ServerMessageType.OnMemberJoin;

        public MemberJoinHandler(ILogService logService)
        {
            _logService = logService;
        }

        public async Task HandleAsync(JsonElement data)
        {
            try
            {
                var memberData = JsonSerializer.Deserialize<MemberEventData>(data.GetRawText());
                if (memberData == null) 
                {
                    _logService.Error("MemberJoinHandler", "Failed to deserialize member data");
                    return;
                }

                _logService.Info("MemberJoinHandler", 
                    $"👋 新成员加入 | 群: {memberData.GroupId} | 成员: {memberData.MemberNickname} ({memberData.MemberWxid})");

                // TODO: 处理成员加入事件
                // 1. 更新群成员列表
                // 2. 发送欢迎消息
                // 3. 通知 UI 更新

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logService.Error("MemberJoinHandler", "Error handling member join", ex);
            }
        }
    }

    /// <summary>
    /// 群成员退出处理器
    /// </summary>
    public class MemberLeaveHandler : IMessageHandler
    {
        private readonly ILogService _logService;

        public ServerMessageType MessageType => ServerMessageType.OnMemberLeave;

        public MemberLeaveHandler(ILogService logService)
        {
            _logService = logService;
        }

        public async Task HandleAsync(JsonElement data)
        {
            try
            {
                var memberData = JsonSerializer.Deserialize<MemberEventData>(data.GetRawText());
                if (memberData == null) 
                {
                    _logService.Error("MemberLeaveHandler", "Failed to deserialize member data");
                    return;
                }

                _logService.Info("MemberLeaveHandler", 
                    $"👋 成员退出 | 群: {memberData.GroupId} | 成员: {memberData.MemberNickname} ({memberData.MemberWxid})");

                // TODO: 处理成员退出事件
                // 1. 更新群成员列表
                // 2. 记录退群日志
                // 3. 通知 UI 更新

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logService.Error("MemberLeaveHandler", "Error handling member leave", ex);
            }
        }
    }
}

