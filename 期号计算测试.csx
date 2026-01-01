// 期号计算测试脚本
// 使用方式：在 LINQPad 或 dotnet-script 中运行

using System;

// ========================================
// 🔥 核心常量（与 BinggoHelper 完全相同）
// ========================================
const int ISSUES_PER_DAY = 203;           // 每天期数
const int FIRST_ISSUE_ID = 114000001;     // 基准期号 (2025-01-01 第1期)
const long FIRST_TIMESTAMP = 1735686300;  // 基准时间戳 (2025-01-01 07:05:00)
const int MINUTES_PER_ISSUE = 5;          // 每期间隔（分钟）

// ========================================
// 🔥 期号计算方法（与 BinggoHelper 完全相同）
// ========================================

/// <summary>
/// 获取指定时间的当前期号
/// </summary>
int GetCurrentIssueId(DateTime? time = null)
{
    var currentTime = time ?? DateTime.Now;
    var firstTime = DateTimeOffset.FromUnixTimeSeconds(FIRST_TIMESTAMP).LocalDateTime;
    
    // 计算天数差
    var timeSpan = currentTime - firstTime;
    var days = timeSpan.Days;
    
    // 当天的基础期号
    int baseDayIssueId = FIRST_ISSUE_ID + days * ISSUES_PER_DAY;
    
    // 🔥 关键：计算当天已经过了多少期
    int issueCount = 0;
    for (int i = 0; i < ISSUES_PER_DAY; i++)
    {
        var issueTimestamp = GetIssueOpenTimestamp(baseDayIssueId + i);
        var issueTime = DateTimeOffset.FromUnixTimeSeconds(issueTimestamp).LocalDateTime;
        
        // 🔥 如果当前时间 > 该期开奖时间，说明该期已过
        if (currentTime > issueTime)
        {
            issueCount++;
        }
        else
        {
            break;
        }
    }
    
    return baseDayIssueId + issueCount;
}

/// <summary>
/// 根据期号计算开奖时间戳
/// </summary>
long GetIssueOpenTimestamp(int issueId)
{
    var firstTime = DateTimeOffset.FromUnixTimeSeconds(FIRST_TIMESTAMP).LocalDateTime;
    
    // 计算天数差
    int days = GetDaysDiff(issueId);
    
    // 计算当天第几期（1-203）
    int number = GetIssueNumber(issueId);
    
    // 计算开奖时间
    var nowDay = firstTime.AddDays(days);
    var openTime = nowDay.AddMinutes(MINUTES_PER_ISSUE * (number - 1));
    
    // 转换为 Unix 时间戳
    return new DateTimeOffset(openTime).ToUnixTimeSeconds();
}

/// <summary>
/// 根据期号计算开奖时间
/// </summary>
DateTime GetIssueOpenTime(int issueId)
{
    long timestamp = GetIssueOpenTimestamp(issueId);
    return DateTimeOffset.FromUnixTimeSeconds(timestamp).LocalDateTime;
}

/// <summary>
/// 计算期号相对于基准日期的天数差
/// </summary>
int GetDaysDiff(int issueId)
{
    return (issueId - FIRST_ISSUE_ID) / ISSUES_PER_DAY;
}

/// <summary>
/// 获取期号在当天是第几期（1-203）
/// </summary>
int GetIssueNumber(int issueId)
{
    int result = 0;
    int value = issueId - FIRST_ISSUE_ID;
    
    if (value >= 0)
    {
        // 🔥 关键：result = value % 203 + 1
        result = value % ISSUES_PER_DAY + 1;
    }
    else
    {
        // 处理负数（历史期号）
        result = value % ISSUES_PER_DAY + 1;
        result = ISSUES_PER_DAY - Math.Abs(result);
    }
    
    return result;
}

/// <summary>
/// 计算距离指定期号开奖还有多少秒
/// </summary>
int GetSecondsToOpen(int issueId, DateTime? currentTime = null)
{
    var now = currentTime ?? DateTime.Now;
    var openTime = GetIssueOpenTime(issueId);
    var seconds = (int)(openTime - now).TotalSeconds;
    return seconds;
}

/// <summary>
/// 获取上一期期号
/// </summary>
int GetPreviousIssueId(int issueId)
{
    return issueId - 1;
}

/// <summary>
/// 获取下一期期号
/// </summary>
int GetNextIssueId(int issueId)
{
    return issueId + 1;
}

/// <summary>
/// 格式化倒计时显示（MM:SS）
/// </summary>
string FormatCountdown(int seconds)
{
    if (seconds < 0) return "00:00";
    
    int minutes = seconds / 60;
    int secs = seconds % 60;
    return $"{minutes:D2}:{secs:D2}";
}

// ========================================
// 🔥 测试代码
// ========================================

Console.WriteLine("========================================");
Console.WriteLine("🎲 BaiShengVx3Plus & zhaocaimao 期号计算测试");
Console.WriteLine("========================================");
Console.WriteLine();

// 1. 当前系统时间
var now = DateTime.Now;
Console.WriteLine($"📅 当前系统时间：{now:yyyy-MM-dd HH:mm:ss}");
Console.WriteLine();

// 2. 基准信息
var firstTime = DateTimeOffset.FromUnixTimeSeconds(FIRST_TIMESTAMP).LocalDateTime;
Console.WriteLine("📌 基准信息：");
Console.WriteLine($"   基准日期：{firstTime:yyyy-MM-dd HH:mm:ss}");
Console.WriteLine($"   基准期号：{FIRST_ISSUE_ID}");
Console.WriteLine($"   每天期数：{ISSUES_PER_DAY}");
Console.WriteLine($"   每期间隔：{MINUTES_PER_ISSUE} 分钟");
Console.WriteLine();

// 3. 当前期号计算
var currentIssueId = GetCurrentIssueId();
var currentIssueNumber = GetIssueNumber(currentIssueId);
var currentOpenTime = GetIssueOpenTime(currentIssueId);
var secondsToOpen = GetSecondsToOpen(currentIssueId);

Console.WriteLine("🎯 当前期号信息：");
Console.WriteLine($"   完整期号：{currentIssueId}");
Console.WriteLine($"   显示期号：{(currentIssueId % 1000):D3} (后3位)");
Console.WriteLine($"   当天第几期：第{currentIssueNumber}期");
Console.WriteLine($"   开奖时间：{currentOpenTime:yyyy-MM-dd HH:mm:ss}");

if (secondsToOpen > 0)
{
    Console.WriteLine($"   距离开奖：{FormatCountdown(secondsToOpen)} ({secondsToOpen}秒)");
}
else
{
    Console.WriteLine($"   已开奖：{FormatCountdown(-secondsToOpen)} 前 ({-secondsToOpen}秒)");
}
Console.WriteLine();

// 4. 上一期信息
var lastIssueId = GetPreviousIssueId(currentIssueId);
var lastIssueNumber = GetIssueNumber(lastIssueId);
var lastOpenTime = GetIssueOpenTime(lastIssueId);

Console.WriteLine("⬅️ 上一期信息：");
Console.WriteLine($"   完整期号：{lastIssueId}");
Console.WriteLine($"   显示期号：{(lastIssueId % 1000):D3} (后3位)");
Console.WriteLine($"   当天第几期：第{lastIssueNumber}期");
Console.WriteLine($"   开奖时间：{lastOpenTime:yyyy-MM-dd HH:mm:ss}");
Console.WriteLine();

// 5. 下一期信息
var nextIssueId = GetNextIssueId(currentIssueId);
var nextIssueNumber = GetIssueNumber(nextIssueId);
var nextOpenTime = GetIssueOpenTime(nextIssueId);
var secondsToNext = GetSecondsToOpen(nextIssueId);

Console.WriteLine("➡️ 下一期信息：");
Console.WriteLine($"   完整期号：{nextIssueId}");
Console.WriteLine($"   显示期号：{(nextIssueId % 1000):D3} (后3位)");
Console.WriteLine($"   当天第几期：第{nextIssueNumber}期");
Console.WriteLine($"   开奖时间：{nextOpenTime:yyyy-MM-dd HH:mm:ss}");
Console.WriteLine($"   距离开奖：{FormatCountdown(secondsToNext)} ({secondsToNext}秒)");
Console.WriteLine();

// 6. 今日期号范围
var today = now.Date;
var firstIssueToday = GetCurrentIssueId(today.AddHours(7).AddMinutes(5)); // 7:05:00
var lastIssueToday = GetCurrentIssueId(today.AddHours(23).AddMinutes(59).AddSeconds(59)); // 23:59:59

Console.WriteLine("📆 今日期号范围：");
Console.WriteLine($"   第一期：{firstIssueToday} ({GetIssueOpenTime(firstIssueToday):HH:mm:ss})");
Console.WriteLine($"   最后期：{lastIssueToday} ({GetIssueOpenTime(lastIssueToday):HH:mm:ss})");
Console.WriteLine($"   总期数：{lastIssueToday - firstIssueToday + 1} 期");
Console.WriteLine();

// 7. 测试特定时间的期号计算
Console.WriteLine("🧪 测试特定时间的期号计算：");
var testTime1 = new DateTime(2025, 1, 1, 7, 5, 0);
var testIssue1 = GetCurrentIssueId(testTime1);
Console.WriteLine($"   {testTime1:yyyy-MM-dd HH:mm:ss} → 期号 {testIssue1} (第{GetIssueNumber(testIssue1)}期)");

var testTime2 = new DateTime(2025, 1, 1, 10, 15, 0);
var testIssue2 = GetCurrentIssueId(testTime2);
Console.WriteLine($"   {testTime2:yyyy-MM-dd HH:mm:ss} → 期号 {testIssue2} (第{GetIssueNumber(testIssue2)}期)");

var testTime3 = new DateTime(2025, 1, 2, 7, 5, 0);
var testIssue3 = GetCurrentIssueId(testTime3);
Console.WriteLine($"   {testTime3:yyyy-MM-dd HH:mm:ss} → 期号 {testIssue3} (第{GetIssueNumber(testIssue3)}期)");
Console.WriteLine();

// 8. 最近5期信息
Console.WriteLine("📊 最近5期信息：");
Console.WriteLine("┌────────────┬──────────┬──────────┬───────────────────┐");
Console.WriteLine("│ 完整期号   │ 显示期号 │ 当天期数 │ 开奖时间          │");
Console.WriteLine("├────────────┼──────────┼──────────┼───────────────────┤");
for (int i = -2; i <= 2; i++)
{
    var issueId = currentIssueId + i;
    var issueNumber = GetIssueNumber(issueId);
    var openTime = GetIssueOpenTime(issueId);
    var displayIssue = (issueId % 1000).ToString("D3");
    
    string marker = i == 0 ? " ← 当前" : "";
    Console.WriteLine($"│ {issueId} │   {displayIssue}    │ 第{issueNumber,3}期 │ {openTime:HH:mm:ss}          │{marker}");
}
Console.WriteLine("└────────────┴──────────┴──────────┴───────────────────┘");
Console.WriteLine();

Console.WriteLine("========================================");
Console.WriteLine("✅ 测试完成");
Console.WriteLine("========================================");


