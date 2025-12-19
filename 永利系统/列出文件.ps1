# List files to see actual garbled names
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$OutputEncoding = [System.Text.Encoding]::UTF8

$baseDir = Split-Path -Parent $MyInvocation.MyCommand.Path

# Find target directory
$targetDir = $null
foreach ($dir in Get-ChildItem -Path $baseDir -Directory) {
    $files = Get-ChildItem -Path $dir.FullName -File | Where-Object { $_.Name -like "251219-*" }
    if ($files.Count -gt 0) {
        $targetDir = $dir.FullName
        break
    }
}

if ($targetDir) {
    Write-Host "Target directory: $targetDir" -ForegroundColor Green
    Write-Host "`nFiles to fix:" -ForegroundColor Cyan
    Write-Host "=" * 60
    
    $files = Get-ChildItem -Path $targetDir -File | Where-Object { $_.Name -like "251219-*" } | Sort-Object Name
    foreach ($file in $files) {
        Write-Host $file.Name
    }
    
    Write-Host "`nTotal: $($files.Count) files" -ForegroundColor Yellow
} else {
    Write-Host "Directory not found!" -ForegroundColor Red
}

