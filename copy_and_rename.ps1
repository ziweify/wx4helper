# 拷贝 BaiShengVx3Plus 到 zhaocaimao 并修改命名空间

$sourceDir = "BaiShengVx3Plus"
$targetDir = "zhaocaimao"

# 排除的目录和文件
$excludeDirs = @("bin", "obj", "0-资料", "packages", "x64", "MyLog", "sound", "tools")
$excludeFiles = @("LoginForm.Designer.cs")

# 获取所有文件
$files = Get-ChildItem -Path $sourceDir -Recurse -File | Where-Object {
    $relativePath = $_.FullName.Substring((Resolve-Path $sourceDir).Path.Length + 1)
    $pathParts = $relativePath.Split([IO.Path]::DirectorySeparatorChar)
    
    # 检查是否在排除的目录中
    $inExcludeDir = $false
    foreach ($part in $pathParts) {
        if ($excludeDirs -contains $part) {
            $inExcludeDir = $true
            break
        }
    }
    
    # 检查是否是排除的文件
    $isExcludeFile = $excludeFiles -contains $_.Name
    
    # 排除 libs 目录中的特定文件类型
    $inLibs = $relativePath -like "libs\*"
    $isLibsFile = $inLibs -and ($_.Extension -in @(".exe", ".dll", ".png", ".ico", ".txt"))
    
    return -not $inExcludeDir -and -not $isExcludeFile -and -not $isLibsFile
}

Write-Host "找到 $($files.Count) 个文件需要拷贝..."

# 拷贝文件
foreach ($file in $files) {
    $relativePath = $file.FullName.Substring((Resolve-Path $sourceDir).Path.Length + 1)
    $targetPath = Join-Path $targetDir $relativePath
    $targetFileDir = Split-Path $targetPath -Parent
    
    # 创建目标目录
    if (-not (Test-Path $targetFileDir)) {
        New-Item -ItemType Directory -Path $targetFileDir -Force | Out-Null
    }
    
    # 拷贝文件
    Copy-Item -Path $file.FullName -Destination $targetPath -Force
    Write-Host "已拷贝: $relativePath"
}

Write-Host "文件拷贝完成！"
