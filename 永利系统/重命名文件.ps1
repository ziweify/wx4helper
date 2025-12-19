# PowerShell File Rename Script
# UTF-8 Encoding

[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$OutputEncoding = [System.Text.Encoding]::UTF8

# Set working directory - find target directory by looking for known files
$baseDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$targetDir = $null

# Try to find the directory by looking for one of the files we need to rename
$knownFile = "251219-002.md"
foreach ($dir in Get-ChildItem -Path $baseDir -Directory) {
    $testPath = Join-Path $dir.FullName $knownFile
    if (Test-Path -LiteralPath $testPath) {
        $targetDir = $dir.FullName
        break
    }
}

if (-not $targetDir) {
    Write-Host "Error: Cannot find target directory!" -ForegroundColor Red
    exit 1
}

# Rename mapping - using Base64 to avoid encoding issues when PowerShell reads the file
function Decode-UTF8Base64 {
    param([string]$base64)
    $bytes = [System.Convert]::FromBase64String($base64)
    return [System.Text.Encoding]::UTF8.GetString($bytes)
}

$renameMap = @{}
$renameMap["251219-002.md"] = "251219-002-" + (Decode-UTF8Base64 "6Y+I7oSA5rm0RExM5a+u5pug5pWk") + ".md"
$renameMap["251219-003.md"] = "251219-003-" + (Decode-UTF8Base64 "5a+u5pug5pWk57yC5ZOE44GR55GZ772F5ZaF") + ".md"
$renameMap["251219-004.md"] = "251219-004-NetCore" + (Decode-UTF8Base64 "TmV0Q29yZeeSuu6Imue3ng==") + ".md"
$renameMap["251219-005.md"] = "251219-005-" + (Decode-UTF8Base64 "57yC5qCs55in6Za/5qyS7oek5reH7oa87piy") + ".md"
$renameMap["251219-006.md"] = "251219-006-" + (Decode-UTF8Base64 "57yC5qCs55in6Y605oSs5aeb55KH5a2Y5qeR") + ".md"
$renameMap["251219-007.md"] = "251219-007-MVVM" + (Decode-UTF8Base64 "TVZWTemPguinhO6UjeeAteinhOeYrg==") + ".md"
$renameMap["251219-008.md"] = "251219-008-" + (Decode-UTF8Base64 "5aaX5ZeY54Gm5aij5Y2e5a6z54C16KeE55iu") + ".md"
$renameMap["251219-009.md"] = "251219-009-" + (Decode-UTF8Base64 "6Y2S5Zuo5bSy6Y6/5baE57aU6Y645Zun5bSh") + ".md"
$renameMap["251219-010.md"] = "251219-010-" + (Decode-UTF8Base64 "6Y2S5Zuo5bSy54C55bG+5Z6a6Y6244Ol5oah") + ".md"
$renameMap["251219-011.md"] = "251219-011-Ribbon" + (Decode-UTF8Base64 "UmliYm9u55KB5o2Q7oW46Y645Zun5bSh") + ".md"

Write-Host "Starting file rename..." -ForegroundColor Green
Write-Host "Target directory: $targetDir" -ForegroundColor Cyan

$successCount = 0
$failCount = 0

foreach ($oldName in $renameMap.Keys) {
    $newName = $renameMap[$oldName]
    $oldPath = Join-Path $targetDir $oldName
    $newPath = Join-Path $targetDir $newName
    
    if (Test-Path -LiteralPath $oldPath) {
        try {
            Move-Item -LiteralPath $oldPath -Destination $newPath -Force -ErrorAction Stop
            Write-Host "Success: $oldName -> $newName" -ForegroundColor Green
            $successCount++
        }
        catch {
            Write-Host "Failed: $oldName - Error: $_" -ForegroundColor Red
            $failCount++
        }
    }
    else {
        Write-Host "Skipped: $oldName (file not found)" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "Rename completed!" -ForegroundColor Green
Write-Host "Success: $successCount files" -ForegroundColor Green
if ($failCount -gt 0) {
    Write-Host "Failed: $failCount files" -ForegroundColor Red
}

Write-Host ""
Write-Host "Press any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
