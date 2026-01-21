# YongLiSystem 编译和运行脚本

$ErrorActionPreference = "Stop"

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "  YongLiSystem 编译和运行" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# 切换到项目根目录
Set-Location "E:\gitcode\wx4helper"
Write-Host "当前目录: $PWD" -ForegroundColor Green
Write-Host ""

# 编译项目
Write-Host "开始编译 YongLiSystem 项目..." -ForegroundColor Yellow
dotnet build YongLiSystem/YongLiSystem.csproj --no-restore

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "编译失败! ✗" -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host ""
Write-Host "编译成功! ✓" -ForegroundColor Green
Write-Host ""

# 询问是否运行
$run = Read-Host "是否运行程序? (Y/N)"
if ($run -eq "Y" -or $run -eq "y") {
    Write-Host ""
    Write-Host "启动程序..." -ForegroundColor Yellow
    Start-Process "YongLiSystem\bin\Debug\net8.0-windows\YongLiSystem.exe"
    Write-Host "程序已启动!" -ForegroundColor Green
}

Write-Host ""
Write-Host "完成!" -ForegroundColor Cyan
