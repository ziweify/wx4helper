# 检查数据库内容
$dbPath = "$env:LOCALAPPDATA\ZhaoCaiMao\Data\business.db"

Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
Write-Host "🔍 数据库检查脚本" -ForegroundColor Yellow
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
Write-Host ""

# 检查数据库文件是否存在
if (Test-Path $dbPath) {
    $file = Get-Item $dbPath
    Write-Host "✅ 数据库文件存在" -ForegroundColor Green
    Write-Host "   路径: $dbPath" -ForegroundColor Gray
    Write-Host "   大小: $($file.Length) 字节" -ForegroundColor Gray
    Write-Host "   修改时间: $($file.LastWriteTime)" -ForegroundColor Gray
    Write-Host ""
    
    # 检查 WAL 文件
    $walPath = "$dbPath-wal"
    $shmPath = "$dbPath-shm"
    
    if (Test-Path $walPath) {
        $walFile = Get-Item $walPath
        Write-Host "⚠️  检测到 WAL 文件！" -ForegroundColor Yellow
        Write-Host "   路径: $walPath" -ForegroundColor Gray
        Write-Host "   大小: $($walFile.Length) 字节" -ForegroundColor Gray
        Write-Host "   说明: 数据可能在 WAL 文件中，主文件未更新！" -ForegroundColor Red
        Write-Host ""
    } else {
        Write-Host "✅ 无 WAL 文件（使用 DELETE 模式）" -ForegroundColor Green
        Write-Host ""
    }
    
    if (Test-Path $shmPath) {
        Write-Host "⚠️  检测到 SHM 文件" -ForegroundColor Yellow
        Write-Host "   路径: $shmPath" -ForegroundColor Gray
        Write-Host ""
    }
    
    # 尝试使用 SQLite 命令（如果安装了）
    $sqliteCmd = Get-Command sqlite3 -ErrorAction SilentlyContinue
    if ($sqliteCmd) {
        Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
        Write-Host "📊 查询数据库内容..." -ForegroundColor Yellow
        Write-Host ""
        
        # 查询表结构
        Write-Host "表结构:" -ForegroundColor Cyan
        sqlite3 $dbPath "PRAGMA table_info(AutoBetConfigs);"
        Write-Host ""
        
        # 查询数据
        Write-Host "配置数据:" -ForegroundColor Cyan
        sqlite3 $dbPath "SELECT Id, ConfigName, Username, Password, Platform, IsEnabled FROM AutoBetConfigs;"
        Write-Host ""
        
        # 检查 WAL 模式
        Write-Host "当前日志模式:" -ForegroundColor Cyan
        sqlite3 $dbPath "PRAGMA journal_mode;"
        Write-Host ""
        
        Write-Host "当前同步模式:" -ForegroundColor Cyan
        sqlite3 $dbPath "PRAGMA synchronous;"
        Write-Host ""
    } else {
        Write-Host "⚠️  未安装 sqlite3 命令行工具" -ForegroundColor Yellow
        Write-Host "   请使用 SQLite 查看器手动查看数据库内容" -ForegroundColor Gray
        Write-Host ""
    }
    
} else {
    Write-Host "❌ 数据库文件不存在！" -ForegroundColor Red
    Write-Host "   路径: $dbPath" -ForegroundColor Gray
    Write-Host ""
}

Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
Write-Host "按任意键退出..." -ForegroundColor Yellow
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

