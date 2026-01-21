# 编译 YongLiSystem 项目

$ErrorActionPreference = "Stop"

Write-Host "开始编译 YongLiSystem 项目..." -ForegroundColor Cyan

Set-Location "E:\gitcode\wx4helper"

Write-Host "当前目录: $PWD" -ForegroundColor Green

# 执行编译
Write-Host "`n开始编译..." -ForegroundColor Yellow
dotnet build YongLiSystem/YongLiSystem.csproj --no-restore

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n编译成功! ✓" -ForegroundColor Green
} else {
    Write-Host "`n编译失败! ✗" -ForegroundColor Red
    exit $LASTEXITCODE
}
