using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using BaiShengVx3Plus.Models;

namespace BaiShengVx3Plus.Services.Messages
{
    /// <summary>
    /// 消息分发器（负责路由消息到具体处理器）
    /// </summary>
    public class MessageDispatcher
    {
        private readonly Dictionary<ServerMessageType, List<IMessageHandler>> _handlers = new();
        private readonly ILogService _logService;

        public MessageDispatcher(ILogService logService)
        {
            _logService = logService;
        }

        /// <summary>
        /// 注册消息处理器
        /// </summary>
        public void RegisterHandler(IMessageHandler handler)
        {
            if (!_handlers.ContainsKey(handler.MessageType))
            {
                _handlers[handler.MessageType] = new List<IMessageHandler>();
            }

            _handlers[handler.MessageType].Add(handler);
            _logService.Info("MessageDispatcher", $"✓ Registered handler for {handler.MessageType}: {handler.GetType().Name}");
        }

        /// <summary>
        /// 分发消息到对应的处理器
        /// </summary>
        public async Task DispatchAsync(string method, object? data)
        {
            try
            {
                // 将 method 字符串转换为枚举
                if (!Enum.TryParse<ServerMessageType>(method, ignoreCase: true, out var messageType))
                {
                    messageType = ServerMessageType.Unknown;
                    _logService.Warning("MessageDispatcher", $"Unknown message type: {method}");
                }

                // 转换 data 为 JsonElement
                JsonElement jsonData;
                if (data == null)
                {
                    jsonData = JsonDocument.Parse("{}").RootElement;
                }
                else if (data is JsonElement element)
                {
                    jsonData = element;
                }
                else
                {
                    // 其他类型，先序列化再反序列化
                    string json = JsonSerializer.Serialize(data);
                    jsonData = JsonDocument.Parse(json).RootElement;
                }

                // 查找对应的处理器
                if (_handlers.TryGetValue(messageType, out var handlers))
                {
                    _logService.Info("MessageDispatcher", $"📨 Dispatching {method} to {handlers.Count} handler(s)");

                    // 并行执行所有处理器
                    var tasks = handlers.Select(h => h.HandleAsync(jsonData));
                    await Task.WhenAll(tasks);
                }
                else
                {
                    _logService.Warning("MessageDispatcher", $"⚠ No handler registered for {messageType}");
                }
            }
            catch (Exception ex)
            {
                _logService.Error("MessageDispatcher", $"Error dispatching message: {method}", ex);
            }
        }
    }
}

