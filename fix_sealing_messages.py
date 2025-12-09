#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""修复封盘提醒和封盘消息的发送逻辑"""

import re

file_path = 'BaiShengVx3Plus/Services/Games/Binggo/BinggoLotteryService.cs'

# 读取文件
with open(file_path, 'r', encoding='utf-8') as f:
    content = f.read()

# 修复 SendSealingReminderAsync 方法（第2412行附近）
# 查找并替换有问题的部分
old_pattern_1 = r'''(private async Task SendSealingReminderAsync\(int issueId, int seconds\)\s*\{\s*try\s*\{)\s*string\? groupWxId = _groupBindingService\?\.CurrentBoundGroup\?\.Wxid;\s*if \(string\.IsNullOrEmpty\(groupWxId\) \|\| _socketClient == null \|\| !_socketClient\.IsConnected\)\s*\{\s*_logService\.Debug\("BinggoLotteryService", "[^"]*"\);\s*return;\s*\}\s*(//[^\n]*\s*bool shouldSend = ShouldSendSystemMessage\(\);)'''

new_code_1 = r'''\1
                // 🔥 检查是否应该发送系统消息
                bool shouldSend = ShouldSendSystemMessage();
                bool isDevMode = _configService.GetIsRunModeDev();
                
                // 🔥 如果收单关闭且不是开发模式，直接返回
                if (!shouldSend && !isDevMode)
                {
                    return;
                }
                
                // 🔥 格式完全按照 F5BotV2：{issueid%1000} 还剩30秒 或 {issueid%1000} 还剩15秒
                int issueShort = issueId % 1000;
                string message = $"{issueShort} 还剩{seconds}秒";
                
                // 🔥 只有在绑定群且微信已登录时才发送到微信群
                string? groupWxId = _groupBindingService?.CurrentBoundGroup?.Wxid;
                if (shouldSend && !string.IsNullOrEmpty(groupWxId) && _socketClient != null && _socketClient.IsConnected)
                {
                    _logService.Info("BinggoLotteryService", $"📢 发送封盘提醒: {groupWxId} - {message}");
                    var response = await _socketClient.SendAsync<object>("SendMessage", groupWxId, message);
                    if (response != null)
                    {
                        _logService.Info("BinggoLotteryService", $"✅ 封盘提醒已发送: {message}");
                    }
                }
                else if (shouldSend)
                {
                    _logService.Debug("BinggoLotteryService", "未绑定群或微信未登录，跳过发送封盘提醒到微信群");
                }
                
                // 🔥 开发模式：无论是否发送到微信群，都通知消息模拟器显示封盘提醒
                if (isDevMode)
                {
                    Views.Dev.MessageSimulatorForm.NotifySystemMessage("封盘提醒", message);
                    _logService.Debug("BinggoLotteryService", $"🔧 开发模式：已通知消息模拟器显示封盘提醒 - {message}");
                }'''

# 尝试替换（使用更简单的模式）
# 方法1：查找方法签名，然后替换整个方法体
lines = content.split('\n')
new_lines = []
i = 0
modified_count = 0

while i < len(lines):
    line = lines[i]
    
    # 查找 SendSealingReminderAsync 方法
    if 'private async Task SendSealingReminderAsync(int issueId, int seconds)' in line:
        new_lines.append(line)
        i += 1
        new_lines.append(lines[i])  # {
        i += 1
        new_lines.append(lines[i])  # try
        i += 1
        new_lines.append(lines[i])  # {
        i += 1
        
        # 跳过旧代码直到找到 bool shouldSend 或遇到第一个有效代码
        # 删除旧的 groupWxId 检查和提前返回
        while i < len(lines):
            if 'bool shouldSend = ShouldSendSystemMessage();' in lines[i]:
                break
            if 'string? groupWxId = _groupBindingService' in lines[i]:
                # 跳过这个旧的检查块
                while i < len(lines) and 'bool shouldSend' not in lines[i]:
                    i += 1
                break
            else:
                i += 1
        
        # 插入新代码
        new_lines.append('                // 🔥 检查是否应该发送系统消息\n')
        new_lines.append('                bool shouldSend = ShouldSendSystemMessage();\n')
        new_lines.append('                bool isDevMode = _configService.GetIsRunModeDev();\n')
        new_lines.append('                \n')
        new_lines.append('                // 🔥 如果收单关闭且不是开发模式，直接返回\n')
        new_lines.append('                if (!shouldSend && !isDevMode)\n')
        new_lines.append('                {\n')
        new_lines.append('                    return;\n')
        new_lines.append('                }\n')
        new_lines.append('                \n')
        new_lines.append('                // 🔥 格式完全按照 F5BotV2：{issueid%1000} 还剩30秒 或 {issueid%1000} 还剩15秒\n')
        new_lines.append('                int issueShort = issueId % 1000;\n')
        new_lines.append('                string message = $"{issueShort} 还剩{seconds}秒";\n')
        new_lines.append('                \n')
        new_lines.append('                // 🔥 只有在绑定群且微信已登录时才发送到微信群\n')
        new_lines.append('                string? groupWxId = _groupBindingService?.CurrentBoundGroup?.Wxid;\n')
        new_lines.append('                if (shouldSend && !string.IsNullOrEmpty(groupWxId) && _socketClient != null && _socketClient.IsConnected)\n')
        new_lines.append('                {\n')
        new_lines.append('                    _logService.Info("BinggoLotteryService", $"📢 发送封盘提醒: {groupWxId} - {message}");\n')
        new_lines.append('                    var response = await _socketClient.SendAsync<object>("SendMessage", groupWxId, message);\n')
        new_lines.append('                    if (response != null)\n')
        new_lines.append('                    {\n')
        new_lines.append('                        _logService.Info("BinggoLotteryService", $"✅ 封盘提醒已发送: {message}");\n')
        new_lines.append('                    }\n')
        new_lines.append('                }\n')
        new_lines.append('                else if (shouldSend)\n')
        new_lines.append('                {\n')
        new_lines.append('                    _logService.Debug("BinggoLotteryService", "未绑定群或微信未登录，跳过发送封盘提醒到微信群");\n')
        new_lines.append('                }\n')
        new_lines.append('                \n')
        new_lines.append('                // 🔥 开发模式：无论是否发送到微信群，都通知消息模拟器显示封盘提醒\n')
        new_lines.append('                if (isDevMode)\n')
        new_lines.append('                {\n')
        new_lines.append('                    Views.Dev.MessageSimulatorForm.NotifySystemMessage("封盘提醒", message);\n')
        new_lines.append('                    _logService.Debug("BinggoLotteryService", $"🔧 开发模式：已通知消息模拟器显示封盘提醒 - {message}");\n')
        new_lines.append('                }\n')
        
        # 跳过旧代码直到 catch
        if i < len(lines):
            # 跳过已经存在的 shouldSend 行（如果有）
            if 'bool shouldSend' in lines[i]:
                i += 1
            if 'bool isDevMode' in lines[i]:
                i += 1
            
            # 继续跳过直到找到剩余代码的开始（int issueShort或者后续的代码）
            while i < len(lines) and 'catch' not in lines[i]:
                # 如果找到了任何旧的实现代码，跳过它们
                if ('int issueShort' in lines[i] or 
                    'string message' in lines[i] or
                    'string? groupWxId' in lines[i] or
                    'if (shouldSend' in lines[i] or
                    '_logService.Info' in lines[i] or
                    '_logService.Debug' in lines[i] or
                    'var response' in lines[i] or
                    'Views.Dev.MessageSimulatorForm' in lines[i]):
                    i += 1
                    continue
                elif lines[i].strip() in ['', '{', '}']:
                    i += 1
                    continue
                else:
                    break
        
        modified_count += 1
        continue
    
    # 查找 SendSealingMessageAsync 方法（类似的修复）
    elif 'private async Task SendSealingMessageAsync(int issueId)' in line:
        # 这个方法的逻辑类似，也需要修复
        new_lines.append(line)
        i += 1
        # ... 类似的处理 ...
        # 暂时保持不变，先修复SendSealingReminderAsync
        continue
    
    new_lines.append(line)
    i += 1

if modified_count > 0:
    with open(file_path, 'w', encoding='utf-8') as f:
        f.write('\n'.join(new_lines))
    print(f'✅ 成功修复了 {modified_count} 个方法')
else:
    print('❌ 未找到需要修改的方法')

