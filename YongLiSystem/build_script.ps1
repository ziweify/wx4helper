# 编译脚本
$ErrorActionPreference = "Stop"

Write-Host "正在恢复 NuGet 包..." -ForegroundColor Cyan
dotnet restore

Write-Host "正在编译项目..." -ForegroundColor Cyan
dotnet build --no-restore

if ($LASTEXITCODE -eq 0) {
    Write-Host "编译成功！" -ForegroundColor Green
} else {
    Write-Host "编译失败！" -ForegroundColor Red
    exit 1
}
