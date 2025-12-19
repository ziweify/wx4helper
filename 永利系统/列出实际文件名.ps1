# List actual filenames with Base64 encoding for easy copy
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$OutputEncoding = [System.Text.Encoding]::UTF8

$baseDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$targetDir = Join-Path $baseDir "项目说明"

if (-not (Test-Path $targetDir)) {
    Write-Host "Directory not found: $targetDir" -ForegroundColor Red
    exit 1
}

Write-Host "Actual filenames in: $targetDir" -ForegroundColor Cyan
Write-Host ("=" * 80) -ForegroundColor Cyan

$files = Get-ChildItem -Path $targetDir -File | Where-Object { $_.Name -like "251219-*" } | Sort-Object Name

foreach ($file in $files) {
    $name = $file.Name
    Write-Host "`nFile: $name" -ForegroundColor Yellow
    
    # Show Base64 encoding for easy copy
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($name)
    $base64 = [System.Convert]::ToBase64String($bytes)
    Write-Host "Base64: $base64" -ForegroundColor Gray
}

Write-Host "`n" + ("=" * 80) -ForegroundColor Cyan
Write-Host "Total: $($files.Count) files" -ForegroundColor Green

