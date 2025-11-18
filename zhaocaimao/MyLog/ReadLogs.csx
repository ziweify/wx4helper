#!/usr/bin/env dotnet-script
#r "nuget: System.Data.SQLite.Core, 1.0.118"

using System;
using System.Data.SQLite;
using System.IO;

var dbPath = Path.Combine(Environment.CurrentDirectory, "logs.db");
if (!File.Exists(dbPath))
{
    Console.WriteLine($"数据库文件不存在: {dbPath}");
    return;
}

using var conn = new SQLiteConnection($"Data Source={dbPath}");
conn.Open();

var cmd = conn.CreateCommand();
cmd.CommandText = @"
    SELECT 
        datetime(Timestamp/10000000 - 62135596800, 'unixepoch', 'localtime') as Time,
        Level,
        Source,
        Message
    FROM LogEntry 
    WHERE (Source LIKE '%AutoBet%' OR Source LIKE '%Browser%' OR Message LIKE '%浏览器%')
    ORDER BY Timestamp DESC 
    LIMIT 500
";

using var reader = cmd.ExecuteReader();
while (reader.Read())
{
    Console.WriteLine($"{reader["Time"]}|{reader["Level"]}|{reader["Source"]}|{reader["Message"]}");
}

