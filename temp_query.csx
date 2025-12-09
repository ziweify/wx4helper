using SQLite;
var db = new SQLiteConnection(@"BaiShengVx3Plus/BugReport/20251205-32.7.1-封盘还能进单/logs.db");
var logs = db.Query<dynamic>("SELECT * FROM LogEntry WHERE Message LIKE '%封盘%' OR Message LIKE '%进单%' OR Message LIKE '%下注%' ORDER BY Timestamp DESC LIMIT 50");
foreach(var log in logs) Console.WriteLine(log);
