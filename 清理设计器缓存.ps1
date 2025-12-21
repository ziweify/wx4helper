# 清理设计器缓存脚本
Write-Host "正在清理项目缓存..." -ForegroundColor Cyan

# 删除 bin 和 obj 文件夹
$folders = @("bin", "obj")
foreach ($folder in $folders) {
    if (Test-Path $folder) {
        Remove-Item -Path $folder -Recurse -Force
        Write-Host "已删除: $folder" -ForegroundColor Green
    } else {
        Write-Host "不存在: $folder" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "缓存清理完成！" -ForegroundColor Green
Write-Host "请执行以下步骤：" -ForegroundColor Yellow
Write-Host "1. 关闭 Visual Studio" -ForegroundColor White
Write-Host "2. 重新打开 Visual Studio" -ForegroundColor White
Write-Host "3. 重新生成项目 (生成 -> 重新生成解决方案)" -ForegroundColor White
Write-Host "4. 重新打开设计器" -ForegroundColor White

