# 重命名"永利系统"项目为"YongLiSystem"
# 使用方法：在 PowerShell 中执行此脚本

Write-Host "开始重命名项目..." -ForegroundColor Green

# 1. 使用 git mv 重命名文件夹（保持历史记录）
Write-Host "步骤1: 使用 git mv 重命名文件夹..." -ForegroundColor Yellow
git mv "永利系统" "YongLiSystem"
if ($LASTEXITCODE -ne 0) {
    Write-Host "文件夹重命名失败！" -ForegroundColor Red
    exit 1
}

# 2. 重命名项目文件
Write-Host "步骤2: 重命名项目文件..." -ForegroundColor Yellow
cd YongLiSystem
git mv "永利系统.csproj" "YongLiSystem.csproj"
git mv "永利系统.csproj.user" "YongLiSystem.csproj.user"
cd ..

Write-Host "重命名完成！" -ForegroundColor Green
Write-Host "接下来需要更新代码中的命名空间和引用..." -ForegroundColor Yellow
