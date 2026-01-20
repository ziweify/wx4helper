# 编译脚本
$projectPath = Join-Path $PSScriptRoot "永利系统.csproj"
Write-Host "正在恢复 NuGet 包..." -ForegroundColor Cyan
dotnet restore $projectPath

Write-Host "正在编译项目..." -ForegroundColor Cyan
dotnet build $projectPath --no-restore

if ($LASTEXITCODE -eq 0) {
    Write-Host "编译成功！" -ForegroundColor Green
} else {
    Write-Host "编译失败！" -ForegroundColor Red
    exit 1
}
