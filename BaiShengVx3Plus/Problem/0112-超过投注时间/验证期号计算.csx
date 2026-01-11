using System;

// 基准常量
const int FIRST_ISSUE_ID = 115000001;
const long FIRST_TIMESTAMP = 1767222300;  // 2025-12-31 07:05:00
const int ISSUES_PER_DAY = 203;
const int MINUTES_PER_ISSUE = 5;

// 测试期号
int issueId = 115002147;

// 计算
int value = issueId - FIRST_ISSUE_ID;
int days = value / ISSUES_PER_DAY;
int number = value % ISSUES_PER_DAY + 1;

var firstTime = DateTimeOffset.FromUnixTimeSeconds(FIRST_TIMESTAMP).LocalDateTime;
var nowDay = firstTime.AddDays(days);
var openTime = nowDay.AddMinutes(MINUTES_PER_ISSUE * (number - 1));

Console.WriteLine($"期号: {issueId}");
Console.WriteLine($"value: {value}");
Console.WriteLine($"days: {days}");
Console.WriteLine($"number: {number}");
Console.WriteLine($"firstTime: {firstTime:yyyy-MM-dd HH:mm:ss}");
Console.WriteLine($"nowDay: {nowDay:yyyy-MM-dd HH:mm:ss}");
Console.WriteLine($"openTime: {openTime:yyyy-MM-dd HH:mm:ss}");
Console.WriteLine($"openTime (HH:mm:ss): {openTime:HH:mm:ss}");

