# 读取日志数据库
Add-Type -Path "zhaocaimao/bin/Debug/net8.0-windows/SQLite-net.dll"

$dbPath = "zhaocaimao/bin/Debug/net8.0-windows/Data/logs.db"
$conn = New-Object -TypeName SQLite.SQLiteConnection -ArgumentList $dbPath
$conn.Open()

$cmd = $conn.CreateCommand()
$cmd.CommandText = @"
SELECT Timestamp, Level, Source, Message 
FROM LogEntries 
WHERE Source LIKE '%Lottery%' 
   OR Source LIKE '%Binggo%' 
   OR Message LIKE '%开奖%' 
   OR Message LIKE '%LoadLast%'
   OR Message LIKE '%GetRecent%'
   OR Message LIKE '%API%'
   OR Message LIKE '%SetLotteryService%'
ORDER BY Timestamp DESC 
LIMIT 100
"@

$reader = $cmd.ExecuteReader()
$count = 0

Write-Host "========== 开奖相关日志 =========="
while ($reader.Read() -and $count -lt 100) {
    $timestamp = $reader["Timestamp"]
    $level = $reader["Level"]
    $source = $reader["Source"]
    $message = $reader["Message"]
    
    Write-Host "[$timestamp] [$level] [$source] $message"
    $count++
}

$reader.Close()
$conn.Close()

Write-Host "`n共找到 $count 条日志"

