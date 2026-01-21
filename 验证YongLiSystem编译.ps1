# YongLiSystem 编译脚本
# 用于验证项目编译是否成功

$ErrorActionPreference = "Stop"

Write-Host "开始编译 YongLiSystem 项目..." -ForegroundColor Cyan

# 获取项目路径
$projectPath = "E:\gitcode\wx4helper\YongLiSystem"

if (-not (Test-Path $projectPath)) {
    Write-Host "错误: 找不到项目目录 $projectPath" -ForegroundColor Red
    exit 1
}

# 切换到项目目录
Set-Location $projectPath

Write-Host "当前目录: $PWD" -ForegroundColor Green

# 检查项目文件
$csprojFile = "YongLiSystem.csproj"
if (-not (Test-Path $csprojFile)) {
    Write-Host "错误: 找不到项目文件 $csprojFile" -ForegroundColor Red
    exit 1
}

Write-Host "找到项目文件: $csprojFile" -ForegroundColor Green

# 执行编译
Write-Host "`n开始编译..." -ForegroundColor Yellow
dotnet build --no-restore

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n编译成功! ✓" -ForegroundColor Green
} else {
    Write-Host "`n编译失败! ✗" -ForegroundColor Red
    exit $LASTEXITCODE
}
