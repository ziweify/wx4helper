# 修复所有平台脚本中 OddsInfo 构造函数参数顺序错误
# 问题：第4个参数应该是 float (odds)，第5个参数应该是 string (oddsId)
# 但当前传的是反的

$scriptPath = "E:\gitcode\wx4helper\zhaocaimao\Services\AutoBet\Browser\PlatformScripts"
$files = Get-ChildItem -Path $scriptPath -Filter "*.cs" -Recurse

$pattern = 'new OddsInfo\(([^,]+),\s*([^,]+),\s*"([^"]+)",\s*"([^"]+)",\s*([0-9.]+)f?\)'
$replacement = 'new OddsInfo($1, $2, "$3", $5f, "$4")'

$totalFixed = 0

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw -Encoding UTF8
    $originalContent = $content
    
    # 匹配模式：new OddsInfo(car, play, "name", "id", 1.97f)
    # 应该改为：new OddsInfo(car, play, "name", 1.97f, "id")
    
    $matches = [regex]::Matches($content, $pattern)
    
    if ($matches.Count -gt 0) {
        Write-Host "修复文件: $($file.Name) - 找到 $($matches.Count) 处错误" -ForegroundColor Yellow
        
        $content = [regex]::Replace($content, $pattern, $replacement)
        
        Set-Content -Path $file.FullName -Value $content -Encoding UTF8 -NoNewline
        $totalFixed += $matches.Count
    }
}

Write-Host "`n✅ 总共修复了 $totalFixed 处错误" -ForegroundColor Green
Write-Host "请重新编译项目" -ForegroundColor Cyan

