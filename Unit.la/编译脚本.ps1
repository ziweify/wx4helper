# Unit.la 编译脚本 - 处理中文路径
# 使用方法：在 PowerShell 中执行此脚本

# 获取脚本所在目录
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectPath = Join-Path $scriptPath "Unit.la.csproj"

Write-Host "项目路径: $projectPath" -ForegroundColor Green

# 恢复 NuGet 包
Write-Host "正在恢复 NuGet 包..." -ForegroundColor Yellow
dotnet restore $projectPath
if ($LASTEXITCODE -ne 0) {
    Write-Host "NuGet 包恢复失败！" -ForegroundColor Red
    exit 1
}

# 编译项目
Write-Host "正在编译项目..." -ForegroundColor Yellow
dotnet build $projectPath --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "编译失败！" -ForegroundColor Red
    exit 1
}

Write-Host "编译成功！" -ForegroundColor Green
