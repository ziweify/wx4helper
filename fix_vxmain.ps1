$file = "BaiShengVx3Plus\Views\VxMain.cs"
$lines = Get-Content $file -Encoding UTF8

$inMethod = $false
$foundStart = $false
$lineNum = 0

$newLines = foreach ($line in $lines) {
    $lineNum++
    
    # 查找方法开始
    if ($line -match 'private async Task RefreshContactsAsync\(\)') {
        $foundStart = $true
        $line
        continue
    }
    
    # 在方法内
    if ($foundStart -and $line -match 'if \(contactsData != null\)') {
        $inMethod = $true
        "                if (contactsData != null)"
        "                {"
        "                    // 🔥 SendAsync 已经提取了 result，直接就是联系人数组"
        '                    _logService.Debug("VxMain", $"📦 收到数据，类型: {contactsData.RootElement.ValueKind}");'
        "                    "
        "                    var contacts = await _contactDataService.ProcessContactsAsync(contactsData.RootElement);"
        '                    _logService.Info("VxMain", $"✓ 联系人获取成功，共 {contacts.Count} 个");'
        "                    "
        '                    lblStatus.Text = $"✓ 已获取 {contacts.Count} 个联系人";'
        "                }"
        continue
    }
    
    # 跳过旧的if块直到else
    if ($inMethod -and $line -match '^\s+else\s*$') {
        $inMethod = $false
        "                else"
        "                {"
        '                    _logService.Warning("VxMain", "获取联系人失败：响应为空");'
        '                    lblStatus.Text = "获取联系人失败";'
        "                }"
        continue
    }
    
    # 跳过旧代码
    if ($inMethod) {
        continue
    }
    
    # 找到else后的右大括号，跳过
    if ($foundStart -and -not $inMethod -and $line -match '^\s+\}\s*$' -and $lineNum -gt 2335) {
        $foundStart = $false
        continue  # 跳过这个}
    }
    
    $line
}

$newLines | Set-Content $file -Encoding UTF8
Write-Output "文件修改完成！"

