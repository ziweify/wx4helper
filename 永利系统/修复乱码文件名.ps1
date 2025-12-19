# Fix incorrectly renamed files
# This script fixes files that were renamed with garbled Chinese characters

[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$OutputEncoding = [System.Text.Encoding]::UTF8

$baseDir = Split-Path -Parent $MyInvocation.MyCommand.Path

# Find target directory by looking for any file starting with "251219"
$targetDir = $null
foreach ($dir in Get-ChildItem -Path $baseDir -Directory) {
    $files = Get-ChildItem -Path $dir.FullName -File | Where-Object { $_.Name -like "251219-*" }
    if ($files.Count -gt 0) {
        $targetDir = $dir.FullName
        Write-Host "Found target directory: $targetDir" -ForegroundColor Cyan
        break
    }
}

if (-not $targetDir) {
    Write-Host "Error: Cannot find target directory!" -ForegroundColor Red
    Write-Host "Please check if the directory exists and contains files starting with '251219-'" -ForegroundColor Yellow
    exit 1
}

# Fix mapping: garbled name -> correct name
# Direct mapping using actual garbled filenames from directory listing
$fixMap = @{
    "251219-002-鏈湴DLL寮曠敤.md" = "251219-002-本地引用说明.md"
    "251219-003-寮曠敤缂哄け瑙ｅ喅.md" = "251219-003-引用缺失解决.md"
    "251219-004-NetCore璺緞.md" = "251219-004-NetCore路径.md"
    "251219-005-缂栬瘧閿欒淇.md" = "251219-005-编译错误修复.md"
    "251219-006-缂栬瘧鎴愬姛璇存槑.md" = "251219-006-编译成功说明.md"
    "251219-007-MVVM鏂规瘮.md" = "251219-007-MVVM方案对比.md"
    "251219-008-妗嗘灦娣卞害瀵规瘮.md" = "251219-008-框架深度对比.md"
    "251219-009-鍒囨崲鎿嶄綔鎸囧崡.md" = "251219-009-切换操作指南.md"
    "251219-010-鍒囨崲瀹屾垚鎶ュ憡.md" = "251219-010-切换完成报告.md"
    "251219-011-Ribbon璁捐鎸囧崡.md" = "251219-011-Ribbon设计指南.md"
}

Write-Host "Fixing garbled filenames..." -ForegroundColor Green
Write-Host "Target directory: $targetDir" -ForegroundColor Cyan

$successCount = 0
$failCount = 0

foreach ($garbledName in $fixMap.Keys) {
    $correctName = $fixMap[$garbledName]
    $oldPath = Join-Path $targetDir $garbledName
    $newPath = Join-Path $targetDir $correctName
    
    if (Test-Path -LiteralPath $oldPath) {
        try {
            Move-Item -LiteralPath $oldPath -Destination $newPath -Force -ErrorAction Stop
            Write-Host "Fixed: $garbledName -> $correctName" -ForegroundColor Green
            $successCount++
        }
        catch {
            Write-Host "Failed: $garbledName - Error: $_" -ForegroundColor Red
            $failCount++
        }
    }
    else {
        Write-Host "Not found: $garbledName" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "Fix completed!" -ForegroundColor Green
Write-Host "Success: $successCount files" -ForegroundColor Green
if ($failCount -gt 0) {
    Write-Host "Failed: $failCount files" -ForegroundColor Red
}

if ($successCount -eq 0 -and $failCount -eq 0) {
    Write-Host "`nAll files are already correctly named!" -ForegroundColor Cyan
}

