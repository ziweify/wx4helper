# PowerShell 脚本：清理并重新生成项目
# 用于解决 DockingManager 引用问题

# 设置编码
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "清理并重新生成项目" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 获取项目路径
$projectPath = Join-Path $PSScriptRoot ".."
$projectPath = Resolve-Path $projectPath

Write-Host "项目路径: $projectPath" -ForegroundColor Yellow
Write-Host ""

# 清理 bin 和 obj 文件夹
Write-Host "正在清理 bin 和 obj 文件夹..." -ForegroundColor Green

$binPath = Join-Path $projectPath "bin"
$objPath = Join-Path $projectPath "obj"

if (Test-Path $binPath) {
    Remove-Item -Path $binPath -Recurse -Force
    Write-Host "  ✓ 已删除 bin 文件夹" -ForegroundColor Green
} else {
    Write-Host "  - bin 文件夹不存在" -ForegroundColor Gray
}

if (Test-Path $objPath) {
    Remove-Item -Path $objPath -Recurse -Force
    Write-Host "  ✓ 已删除 obj 文件夹" -ForegroundColor Green
} else {
    Write-Host "  - obj 文件夹不存在" -ForegroundColor Gray
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "清理完成！" -ForegroundColor Green
Write-Host ""
Write-Host "请在 Visual Studio 中执行以下操作：" -ForegroundColor Yellow
Write-Host "1. 关闭 Visual Studio（如果已打开）" -ForegroundColor White
Write-Host "2. 重新打开 Visual Studio" -ForegroundColor White
Write-Host "3. 打开项目" -ForegroundColor White
Write-Host "4. 生成 -> 重新生成解决方案 (Ctrl+Shift+B)" -ForegroundColor White
Write-Host ""
Write-Host "或者直接在 Visual Studio 中：" -ForegroundColor Yellow
Write-Host "1. 生成 -> 清理解决方案" -ForegroundColor White
Write-Host "2. 生成 -> 重新生成解决方案" -ForegroundColor White
Write-Host "========================================" -ForegroundColor Cyan

