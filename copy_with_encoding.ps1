# PowerShell脚本：正确复制文件并修改命名空间
param(
    [string]$SourceDir = "BaiShengVx3Plus",
    [string]$TargetDir = "zhaocaimao"
)

Write-Host "开始复制项目文件..." -ForegroundColor Green

# 创建目录结构
$dirs = @("Contracts", "Models", "Core", "Utils", "Services", "Views", "ViewModels")
foreach ($dir in $dirs) {
    $targetPath = Join-Path $TargetDir $dir
    if (!(Test-Path $targetPath)) {
        New-Item -ItemType Directory -Path $targetPath -Force | Out-Null
    }
}

# 复制并转换文件的函数
function Copy-AndConvert {
    param([string]$SourcePath, [string]$TargetPath)
    
    Write-Host "Processing: $SourcePath"
    
    # 读取文件内容（使用UTF8编码）
    $content = Get-Content -Path $SourcePath -Raw -Encoding UTF8
    
    # 替换命名空间
    $content = $content -replace 'namespace BaiShengVx3Plus', 'namespace zhaocaimao'
    $content = $content -replace 'using BaiShengVx3Plus\.', 'using zhaocaimao.'
    
    # 写入目标文件（使用UTF8编码，无BOM）
    $utf8NoBom = New-Object System.Text.UTF8Encoding $false
    [System.IO.File]::WriteAllText($TargetPath, $content, $utf8NoBom)
}

# 复制Contracts
Write-Host "`n复制 Contracts..." -ForegroundColor Cyan
Get-ChildItem -Path "$SourceDir\Contracts\*.cs" | ForEach-Object {
    $targetFile = Join-Path "$TargetDir\Contracts" $_.Name
    Copy-AndConvert -SourcePath $_.FullName -TargetPath $targetFile
}

# 复制Models（递归）
Write-Host "`n复制 Models..." -ForegroundColor Cyan
Get-ChildItem -Path "$SourceDir\Models" -Filter "*.cs" -Recurse | ForEach-Object {
    $relativePath = $_.FullName.Substring((Resolve-Path "$SourceDir\Models").Path.Length + 1)
    $targetFile = Join-Path "$TargetDir\Models" $relativePath
    $targetDir = Split-Path $targetFile -Parent
    if (!(Test-Path $targetDir)) {
        New-Item -ItemType Directory -Path $targetDir -Force | Out-Null
    }
    Copy-AndConvert -SourcePath $_.FullName -TargetPath $targetFile
}

# 复制Core
if (Test-Path "$SourceDir\Core") {
    Write-Host "`n复制 Core..." -ForegroundColor Cyan
    Get-ChildItem -Path "$SourceDir\Core" -Filter "*.cs" -Recurse | ForEach-Object {
        $relativePath = $_.FullName.Substring((Resolve-Path "$SourceDir\Core").Path.Length + 1)
        $targetFile = Join-Path "$TargetDir\Core" $relativePath
        $targetDir = Split-Path $targetFile -Parent
        if (!(Test-Path $targetDir)) {
            New-Item -ItemType Directory -Path $targetDir -Force | Out-Null
        }
        Copy-AndConvert -SourcePath $_.FullName -TargetPath $targetFile
    }
}

# 复制Utils
if (Test-Path "$SourceDir\Utils") {
    Write-Host "`n复制 Utils..." -ForegroundColor Cyan
    Get-ChildItem -Path "$SourceDir\Utils" -Filter "*.cs" -Recurse | ForEach-Object {
        $relativePath = $_.FullName.Substring((Resolve-Path "$SourceDir\Utils").Path.Length + 1)
        $targetFile = Join-Path "$TargetDir\Utils" $relativePath
        $targetDir = Split-Path $targetFile -Parent
        if (!(Test-Path $targetDir)) {
            New-Item -ItemType Directory -Path $targetDir -Force | Out-Null
        }
        Copy-AndConvert -SourcePath $_.FullName -TargetPath $targetFile
    }
}

# 复制Services（递归）
Write-Host "`n复制 Services..." -ForegroundColor Cyan
Get-ChildItem -Path "$SourceDir\Services" -Filter "*.cs" -Recurse | ForEach-Object {
    $relativePath = $_.FullName.Substring((Resolve-Path "$SourceDir\Services").Path.Length + 1)
    $targetFile = Join-Path "$TargetDir\Services" $relativePath
    $targetDir = Split-Path $targetFile -Parent
    if (!(Test-Path $targetDir)) {
        New-Item -ItemType Directory -Path $targetDir -Force | Out-Null
    }
    Copy-AndConvert -SourcePath $_.FullName -TargetPath $targetFile
}

# 复制ViewModels
if (Test-Path "$SourceDir\ViewModels") {
    Write-Host "`n复制 ViewModels..." -ForegroundColor Cyan
    Get-ChildItem -Path "$SourceDir\ViewModels" -Filter "*.cs" -Recurse | ForEach-Object {
        $relativePath = $_.FullName.Substring((Resolve-Path "$SourceDir\ViewModels").Path.Length + 1)
        $targetFile = Join-Path "$TargetDir\ViewModels" $relativePath
        $targetDir = Split-Path $targetFile -Parent
        if (!(Test-Path $targetDir)) {
            New-Item -ItemType Directory -Path $targetDir -Force | Out-Null
        }
        Copy-AndConvert -SourcePath $_.FullName -TargetPath $targetFile
    }
}

# 复制Views（递归，排除.resx文件）
Write-Host "`n复制 Views..." -ForegroundColor Cyan
Get-ChildItem -Path "$SourceDir\Views" -Filter "*.cs" -Recurse | ForEach-Object {
    $relativePath = $_.FullName.Substring((Resolve-Path "$SourceDir\Views").Path.Length + 1)
    $targetFile = Join-Path "$TargetDir\Views" $relativePath
    $targetDir = Split-Path $targetFile -Parent
    if (!(Test-Path $targetDir)) {
        New-Item -ItemType Directory -Path $targetDir -Force | Out-Null
    }
    Copy-AndConvert -SourcePath $_.FullName -TargetPath $targetFile
}

Write-Host "`n✅ 复制完成！" -ForegroundColor Green

# 统计
$totalFiles = (Get-ChildItem -Path $TargetDir -Filter "*.cs" -Recurse).Count
Write-Host "总共复制了 $totalFiles 个 C# 文件" -ForegroundColor Yellow

