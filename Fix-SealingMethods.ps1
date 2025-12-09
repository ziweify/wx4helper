# 修复封盘提醒和封盘消息方法
$filePath = "BaiShengVx3Plus\Services\Games\Binggo\BinggoLotteryService.cs"

# 读取所有行
$lines = Get-Content $filePath -Encoding UTF8

# 修复标志
$modified = $false

# 查找并修复 SendSealingReminderAsync (大约第2412行)
for ($i = 0; $i -lt $lines.Count; $i++) {
    if ($lines[$i] -match 'private async Task SendSealingReminderAsync') {
        Write-Host "找到 SendSealingReminderAsync 在行 $($i+1)"
        
        # 检查是否需要修复（查找旧的致命检查）
        $checkLine = $i + 4  # try { 后的第一行
        if ($lines[$checkLine] -match 'string\? groupWxId.*CurrentBoundGroup') {
            Write-Host "  发现旧代码，开始修复..."
            
            # 跳过 {, try, {
            $startReplace = $i + 4
            
            # 找到需要替换的结束位置（找到 "int issueShort" 之前）
            $endReplace = $startReplace
            while ($endReplace -lt $lines.Count -and $lines[$endReplace] -notmatch 'int issueShort') {
                $endReplace++
            }
            
            # 创建新代码
            $newCode = @(
                '                // 🔥 检查是否应该发送系统消息',
                '                bool shouldSend = ShouldSendSystemMessage();',
                '                bool isDevMode = _configService.GetIsRunModeDev();',
                '                ',
                '                // 🔥 如果收单关闭且不是开发模式，直接返回',
                '                if (!shouldSend && !isDevMode)',
                '                {',
                '                    return;',
                '                }',
                '                ',
                '                // 🔥 格式完全按照 F5BotV2：{issueid%1000} 还剩30秒 或 {issueid%1000} 还剩15秒'
            )
            
            # 替换
            $newLines = @()
            $newLines += $lines[0..($startReplace-1)]
            $newLines += $newCode
            $newLines += $lines[$endReplace..($lines.Count-1)]
            
            $lines = $newLines
            $modified = $true
            Write-Host "  ✅ SendSealingReminderAsync 修复完成"
            break
        }
        else {
            Write-Host "  已经是修复后的版本，跳过"
        }
        break
    }
}

if (-not $modified) {
    Write-Host "❌ 未找到需要修复的代码或已经修复过"
    exit 1
}

# 保存文件
$lines | Set-Content $filePath -Encoding UTF8
Write-Host "✅ 文件已保存"

# 验证修复
$content = Get-Content $filePath -Raw
if ($content -match 'SendSealingReminderAsync.*\{.*try.*\{.*bool shouldSend = ShouldSendSystemMessage') {
    Write-Host "✅ 验证成功：SendSealingReminderAsync 已正确修复"
}
else {
    Write-Host "⚠️ 警告：验证失败，请手动检查文件"
}

