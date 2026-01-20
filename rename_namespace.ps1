# 批量替换命名空间：永利系统 -> YongLiSystem
# 使用方法：在 PowerShell 中执行此脚本

$oldNamespace = "永利系统"
$newNamespace = "YongLiSystem"
$projectPath = "YongLiSystem"

Write-Host "开始替换命名空间..." -ForegroundColor Green
Write-Host "旧命名空间: $oldNamespace" -ForegroundColor Yellow
Write-Host "新命名空间: $newNamespace" -ForegroundColor Yellow
Write-Host "项目路径: $projectPath" -ForegroundColor Yellow

# 获取所有 .cs 文件
$csFiles = Get-ChildItem -Path $projectPath -Filter "*.cs" -Recurse -File

$totalFiles = $csFiles.Count
$processedFiles = 0
$modifiedFiles = 0

foreach ($file in $csFiles) {
    $processedFiles++
    $content = Get-Content $file.FullName -Raw -Encoding UTF8
    $originalContent = $content
    
    # 替换命名空间声明
    $content = $content -replace "namespace $oldNamespace", "namespace $newNamespace"
    $content = $content -replace "using $oldNamespace", "using $newNamespace"
    
    # 如果内容有变化，保存文件
    if ($content -ne $originalContent) {
        Set-Content -Path $file.FullName -Value $content -Encoding UTF8 -NoNewline
        $modifiedFiles++
        Write-Host "已更新: $($file.FullName)" -ForegroundColor Green
    }
    
    if ($processedFiles % 10 -eq 0) {
        Write-Host "进度: $processedFiles / $totalFiles" -ForegroundColor Cyan
    }
}

Write-Host "`n完成！" -ForegroundColor Green
Write-Host "总文件数: $totalFiles" -ForegroundColor Cyan
Write-Host "修改文件数: $modifiedFiles" -ForegroundColor Cyan
