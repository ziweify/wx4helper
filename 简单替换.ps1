$file = "BaiShengVx3Plus\Views\VxMain.cs"
$content = [System.IO.File]::ReadAllText($file, [System.Text.Encoding]::UTF8)

# 旧代码块（从grep的输出精确复制）
$old = @'
                if (contactsData != null)
                {
                    // 🔥 检查是否有错误响应
                    if (contactsData.RootElement.TryGetProperty("error", out var errorElement))
                    {
                        string errorMsg = errorElement.GetString() ?? "";
                        _logService.Warning("VxMain", $"获取联系人失败: {errorMsg}");
                        lblStatus.Text = "获取联系人失败";
                        return;
                    }
                    
                    // 🔥 从响应对象中提取 result 数组（联系人列表）
                    JsonElement contactsArray;
                    if (contactsData.RootElement.TryGetProperty("result", out var resultElement))
                    {
                        contactsArray = resultElement;
                        _logService.Info("VxMain", $"📋 从响应中提取 result 数组，类型: {contactsArray.ValueKind}");
                    }
                    else
                    {
                        // 如果没有 result 字段，假设整个根元素就是数组
                        _logService.Warning("VxMain", "响应中没有 result 字段，假设根元素就是联系人数组");
                        contactsArray = contactsData.RootElement;
                    }
                    
                    // 统一调用 ContactDataService 处理
                    await _contactDataService.ProcessContactsAsync(contactsArray);
                    _logService.Info("VxMain", "✓ 联系人获取成功");
                }
                else
                {
                    _logService.Warning("VxMain", "获取联系人失败");
                    lblStatus.Text = "获取联系人失败";
                }
'@

# 新代码
$new = @'
                if (contactsData != null)
                {
                    // 🔥 SendAsync 已经提取了 result，直接就是联系人数组
                    _logService.Debug("VxMain", $"📦 收到数据，类型: {contactsData.RootElement.ValueKind}");
                    
                    var contacts = await _contactDataService.ProcessContactsAsync(contactsData.RootElement);
                    _logService.Info("VxMain", $"✓ 联系人获取成功，共 {contacts.Count} 个");
                    
                    lblStatus.Text = $"✓ 已获取 {contacts.Count} 个联系人";
                }
                else
                {
                    _logService.Warning("VxMain", "获取联系人失败：响应为空");
                    lblStatus.Text = "获取联系人失败";
                }
'@

if ($content.Contains($old)) {
    $content = $content.Replace($old, $new)
    [System.IO.File]::WriteAllText($file, $content, (New-Object System.Text.UTF8Encoding $false))
    Write-Output "✅ 替换成功！"
} else {
    Write-Output "❌ 未找到旧代码块"
    Write-Output "正在搜索 'TryGetProperty' 的位置..."
    $lines = $content -split "`n"
    for ($i = 2330; $i -lt 2350; $i++) {
        Write-Output "$i : $($lines[$i])"
    }
}

