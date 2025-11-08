# 测试脚本：关闭进程并重新编译

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  关闭所有进程并重新编译" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# 1. 关闭 BaiShengVx3Plus 进程
Write-Host "[1/3] 关闭 BaiShengVx3Plus 进程..." -ForegroundColor Yellow
$process1 = Get-Process -Name "BaiShengVx3Plus" -ErrorAction SilentlyContinue
if ($process1) {
    Stop-Process -Name "BaiShengVx3Plus" -Force
    Write-Host "  ✅ 已关闭 BaiShengVx3Plus" -ForegroundColor Green
} else {
    Write-Host "  ℹ️  BaiShengVx3Plus 未运行" -ForegroundColor Gray
}

# 2. 关闭 BsBrowserClient 进程
Write-Host "[2/3] 关闭 BsBrowserClient 进程..." -ForegroundColor Yellow
$process2 = Get-Process -Name "BsBrowserClient" -ErrorAction SilentlyContinue
if ($process2) {
    Stop-Process -Name "BsBrowserClient" -Force
    Write-Host "  ✅ 已关闭 BsBrowserClient" -ForegroundColor Green
} else {
    Write-Host "  ℹ️  BsBrowserClient 未运行" -ForegroundColor Gray
}

# 等待进程完全退出
Start-Sleep -Seconds 1

# 3. 重新编译
Write-Host "[3/3] 重新编译项目..." -ForegroundColor Yellow
Write-Host ""
dotnet build BaiShengVx3Plus/BaiShengVx3Plus.csproj

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "============================================" -ForegroundColor Green
    Write-Host "  ✅ 编译成功！" -ForegroundColor Green
    Write-Host "============================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "📋 测试步骤：" -ForegroundColor Cyan
    Write-Host "  1. 启动 BaiShengVx3Plus.exe" -ForegroundColor White
    Write-Host "  2. 在快速设置面板输入账号密码" -ForegroundColor White
    Write-Host "  3. 等待 2 秒（确保防抖完成）" -ForegroundColor White
    Write-Host "  4. 观察日志，应该看到：" -ForegroundColor White
    Write-Host "     ✅ 自动投注设置已保存" -ForegroundColor Gray
    Write-Host "        - 用户名: xxx" -ForegroundColor Gray
    Write-Host "        - 密码: ******" -ForegroundColor Gray
    Write-Host "  5. 点击'启动浏览器'按钮" -ForegroundColor White
    Write-Host "  6. 观察日志，重点关注：" -ForegroundColor White
    Write-Host "     📩 GET /api/config?configId=1" -ForegroundColor Gray
    Write-Host "     ✅ 返回配置: 默认配置" -ForegroundColor Gray
    Write-Host "        - 用户名: xxx" -ForegroundColor Gray
    Write-Host "        - 密码: ******" -ForegroundColor Gray
    Write-Host "  7. 查看浏览器日志，确认账号密码是否正确获取" -ForegroundColor White
    Write-Host ""
} else {
    Write-Host ""
    Write-Host "============================================" -ForegroundColor Red
    Write-Host "  ❌ 编译失败！" -ForegroundColor Red
    Write-Host "============================================" -ForegroundColor Red
    Write-Host ""
}

