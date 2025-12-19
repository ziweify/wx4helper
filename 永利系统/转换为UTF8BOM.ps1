# Convert PowerShell script to UTF-8 with BOM
# This script will add UTF-8 BOM to .ps1 files

param(
    [string]$FilePath = ""
)

if ([string]::IsNullOrEmpty($FilePath)) {
    Write-Host "Usage: .\ConvertToUTF8BOM.ps1 -FilePath 'path\to\file.ps1'" -ForegroundColor Yellow
    Write-Host "Or: .\ConvertToUTF8BOM.ps1  (will convert all .ps1 files in current directory)" -ForegroundColor Yellow
    exit
}

function Convert-FileToUTF8BOM {
    param([string]$file)
    
    if (-not (Test-Path $file)) {
        Write-Host "File not found: $file" -ForegroundColor Red
        return $false
    }
    
    # Read file content
    $content = Get-Content $file -Raw -Encoding UTF8
    
    # Check if already has BOM
    $bytes = [System.IO.File]::ReadAllBytes($file)
    $hasBOM = ($bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF)
    
    if ($hasBOM) {
        Write-Host "File already has UTF-8 BOM: $file" -ForegroundColor Green
        return $true
    }
    
    # Write with UTF-8 BOM
    $utf8WithBOM = New-Object System.Text.UTF8Encoding $true
    [System.IO.File]::WriteAllText($file, $content, $utf8WithBOM)
    
    Write-Host "Converted to UTF-8 with BOM: $file" -ForegroundColor Green
    return $true
}

# Convert single file or all .ps1 files
if ($FilePath -ne "") {
    Convert-FileToUTF8BOM -file $FilePath
} else {
    $files = Get-ChildItem -Path . -Filter "*.ps1" -File
    foreach ($file in $files) {
        Convert-FileToUTF8BOM -file $file.FullName
    }
}

Write-Host "`nDone!" -ForegroundColor Cyan

