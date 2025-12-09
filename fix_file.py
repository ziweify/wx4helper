#!/usr/bin/env python3
# -*- coding: utf-8 -*-

file_path = 'BaiShengVx3Plus/Services/Games/Binggo/BinggoLotteryService.cs'

# 读取文件
with open(file_path, 'r', encoding='utf-8') as f:
    lines = f.readlines()

# 查找并修复 SendSealingReminderAsync
modified = False
new_lines = []
i = 0

while i < len(lines):
    # 查找方法签名
    if 'private async Task SendSealingReminderAsync(int issueId, int seconds)' in lines[i]:
        print(f"找到 SendSealingReminderAsync 在行 {i+1}")
        # 添加方法签名
        new_lines.append(lines[i])
        i += 1
        new_lines.append(lines[i])  # {
        i += 1
        new_lines.append(lines[i])  # try
        i += 1
        new_lines.append(lines[i])  # {
        i += 1
        
        # 检查下一行是否是旧的 groupWxId 检查
        if 'string? groupWxId = _groupBindingService' in lines[i]:
            print("  发现旧代码，开始替换...")
            
            # 跳过旧代码直到找到 "bool shouldSend"
            while i < len(lines) and 'bool shouldSend = ShouldSendSystemMessage()' not in lines[i]:
                i += 1
            
            # 插入新代码
            new_lines.append('                // 🔥 检查是否应该发送系统消息\n')
            new_lines.append('                bool shouldSend = ShouldSendSystemMessage();\n')
            i += 1  # 跳过旧的 bool shouldSend 行
            new_lines.append('                bool isDevMode = _configService.GetIsRunModeDev();\n')
            i += 1  # 跳过旧的 bool isDevMode 行
            new_lines.append('                \n')
            i += 1  # 跳过空行
            new_lines.append('                // 🔥 如果收单关闭且不是开发模式，直接返回\n')
            new_lines.append('                if (!shouldSend && !isDevMode)\n')
            i += 1  # 跳过旧的 if 行
            new_lines.append('                {\n')
            i += 1  # 跳过 {
            new_lines.append('                    return;\n')
            i += 1  # 跳过return
            new_lines.append('                }\n')
            i += 1  # 跳过 }
            new_lines.append('                \n')
            i += 1  # 跳过空行
            
            # 插入后续代码
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
            
            # 跳过剩余的旧代码直到 catch
            while i < len(lines) and 'catch (Exception ex)' not in lines[i]:
                i += 1
            
            modified = True
            print("  ✅ 代码已替换")
            continue
        else:
            print("  代码已经是新版本")
            continue
    
    new_lines.append(lines[i])
    i += 1

if modified:
    # 保存文件
    with open(file_path, 'w', encoding='utf-8') as f:
        f.writelines(new_lines)
    print('\n✅ 文件修改成功！')
    print(f'   已修复 SendSealingReminderAsync 方法')
else:
    print('\n❌ 未找到需要修改的代码（可能已经修改过）')

