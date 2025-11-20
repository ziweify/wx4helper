# Sync BaiShengVx3Plus to zhaocaimao (exclude login forms)
param(
    [string]$SourceDir = "BaiShengVx3Plus",
    [string]$TargetDir = "zhaocaimao"
)

Write-Host "Starting project sync..." -ForegroundColor Green

# 排除的目录
$excludeDirs = @("bin", "obj", "0-资料", "packages", "x64", "MyLog", "sound", "tools", "资料")

# 排除的文件（登录相关）
$excludeFiles = @("LoginForm.cs", "LoginForm.Designer.cs", "LoginEventHandler.cs")

# 需要同步的目录
$syncDirs = @(
    "Services",
    "Models", 
    "Core",
    "Helpers",
    "Utils",
    "Extensions",
    "Contracts",
    "ViewModels",
    "UserControls",
    "Views",
    "Attributes",
    "Native"
)

# 需要同步的根目录文件
$rootFiles = @("Program.cs", "ARCHITECTURE.md", "DATA_ACCESS_ARCHITECTURE.md", "DATA_BINDING_AUDIT.md")

function Sync-File {
    param(
        [string]$SourcePath,
        [string]$TargetPath
    )
    
    $fileName = Split-Path $SourcePath -Leaf
    
    # Check if file is excluded
    if ($excludeFiles -contains $fileName) {
        Write-Host "Skipping excluded file: $fileName" -ForegroundColor Yellow
        return
    }
    
    # Check if source file exists
    if (-not (Test-Path $SourcePath)) {
        Write-Host "Source file not found: $SourcePath" -ForegroundColor Red
        return
    }
    
    # Create target directory
    $targetDir = Split-Path $TargetPath -Parent
    if (-not (Test-Path $targetDir)) {
        New-Item -ItemType Directory -Path $targetDir -Force | Out-Null
    }
    
    # Read source file content
    $content = Get-Content -Path $SourcePath -Raw -Encoding UTF8
    
    # Replace namespace
    $content = $content -replace 'namespace BaiShengVx3Plus', 'namespace zhaocaimao'
    $content = $content -replace 'using BaiShengVx3Plus\.', 'using zhaocaimao.'
    
    # Write target file (UTF8 no BOM)
    $utf8NoBom = New-Object System.Text.UTF8Encoding $false
    [System.IO.File]::WriteAllText($TargetPath, $content, $utf8NoBom)
    
    Write-Host "Synced: $TargetPath" -ForegroundColor Green
}

# 同步目录
foreach ($dir in $syncDirs) {
    $sourcePath = Join-Path $SourceDir $dir
    $targetPath = Join-Path $TargetDir $dir
    
    if (-not (Test-Path $sourcePath)) {
        Write-Host "Source directory not found: $sourcePath" -ForegroundColor Yellow
        continue
    }
    
    Write-Host "`nSyncing directory: $dir" -ForegroundColor Cyan
    
    # 获取所有 .cs, .Designer.cs, .resx 文件
    $files = Get-ChildItem -Path $sourcePath -Include "*.cs", "*.Designer.cs", "*.resx" -Recurse | Where-Object {
        $relativePath = $_.FullName.Substring((Resolve-Path $sourcePath).Path.Length + 1)
        $pathParts = $relativePath.Split([IO.Path]::DirectorySeparatorChar)
        
        # 检查是否在排除的目录中
        $inExcludeDir = $false
        foreach ($part in $pathParts) {
            if ($excludeDirs -contains $part) {
                $inExcludeDir = $true
                break
            }
        }
        
        return -not $inExcludeDir
    }
    
    foreach ($file in $files) {
        $relativePath = $file.FullName.Substring((Resolve-Path $sourcePath).Path.Length + 1)
        $targetFile = Join-Path $targetPath $relativePath
        
        Sync-File -SourcePath $file.FullName -TargetPath $targetFile
    }
}

# Sync root files
Write-Host "`nSyncing root files..." -ForegroundColor Cyan
foreach ($file in $rootFiles) {
    $sourceFile = Join-Path $SourceDir $file
    $targetFile = Join-Path $TargetDir $file
    
    if (Test-Path $sourceFile) {
        if ($file -like "*.cs") {
            Sync-File -SourcePath $sourceFile -TargetPath $targetFile
        } else {
            # Copy non-.cs files directly
            Copy-Item -Path $sourceFile -Destination $targetFile -Force
            Write-Host "Copied: $file" -ForegroundColor Green
        }
    }
}

Write-Host "`nSync completed!" -ForegroundColor Green

