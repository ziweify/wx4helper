<Query Kind="Program">
  <NuGetReference>sqlite-net-pcl</NuGetReference>
  <Namespace>SQLite</Namespace>
</Query>

void Main()
{
	var dbPath = @"D:\gitcode\wx4helper\BaiShengVx3Plus\bin\Debug\net8.0-windows\Data\logs.db";
	
	using (var db = new SQLiteConnection(dbPath))
	{
		var logs = db.Query<LogEntry>(@"
			SELECT * FROM LogEntries 
			WHERE Source LIKE '%Lottery%' 
			   OR Source LIKE '%Binggo%' 
			   OR Message LIKE '%开奖%' 
			   OR Message LIKE '%LoadLast%'
			   OR Message LIKE '%GetRecent%'
			   OR Message LIKE '%SetLotteryService%'
			   OR Message LIKE '%ucBinggo%'
			ORDER BY Timestamp DESC 
			LIMIT 100
		");
		
		Console.WriteLine($"========== 找到 {logs.Count} 条相关日志 ==========\n");
		
		foreach (var log in logs)
		{
			Console.WriteLine($"[{log.Timestamp:yyyy-MM-dd HH:mm:ss}] [{log.Level}] [{log.Source}]");
			Console.WriteLine($"  {log.Message}");
			if (!string.IsNullOrEmpty(log.Exception))
			{
				Console.WriteLine($"  异常: {log.Exception}");
			}
			Console.WriteLine();
		}
	}
}

public class LogEntry
{
	public int Id { get; set; }
	public DateTime Timestamp { get; set; }
	public string Level { get; set; }
	public string Source { get; set; }
	public string Message { get; set; }
	public string Exception { get; set; }
}

