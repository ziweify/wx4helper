# 期号计算测试脚本 (PowerShell)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "🎲 BaiShengVx3Plus & zhaocaimao 期号计算测试" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 核心常量
$ISSUES_PER_DAY = 203
$FIRST_ISSUE_ID = 114000001
$FIRST_TIMESTAMP = 1735686300  # 2025-01-01 07:05:00
$MINUTES_PER_ISSUE = 5

# 基准时间
$firstTime = [DateTimeOffset]::FromUnixTimeSeconds($FIRST_TIMESTAMP).LocalDateTime
Write-Host "📌 基准信息：" -ForegroundColor Yellow
Write-Host "   基准日期：$($firstTime.ToString('yyyy-MM-dd HH:mm:ss'))"
Write-Host "   基准期号：$FIRST_ISSUE_ID"
Write-Host "   每天期数：$ISSUES_PER_DAY"
Write-Host "   每期间隔：$MINUTES_PER_ISSUE 分钟"
Write-Host ""

# 当前时间
$now = Get-Date
Write-Host "📅 当前系统时间：$($now.ToString('yyyy-MM-dd HH:mm:ss'))" -ForegroundColor Yellow
Write-Host ""

# 计算天数差
$timeSpan = $now - $firstTime
$days = [Math]::Floor($timeSpan.TotalDays)

# 当天基础期号
$baseDayIssueId = $FIRST_ISSUE_ID + $days * $ISSUES_PER_DAY

Write-Host "🧮 计算过程：" -ForegroundColor Yellow
Write-Host "   天数差：$days 天"
Write-Host "   当天基础期号：$baseDayIssueId"

# 计算当天已过期数
$issueCount = 0
for ($i = 0; $i -lt $ISSUES_PER_DAY; $i++) {
    $issueId = $baseDayIssueId + $i
    $daysDiff = [Math]::Floor(($issueId - $FIRST_ISSUE_ID) / $ISSUES_PER_DAY)
    $number = ($issueId - $FIRST_ISSUE_ID) % $ISSUES_PER_DAY + 1
    
    $issueDate = $firstTime.AddDays($daysDiff)
    $issueOpenTime = $issueDate.AddMinutes($MINUTES_PER_ISSUE * ($number - 1))
    
    if ($now -gt $issueOpenTime) {
        $issueCount++
    } else {
        break
    }
}

Write-Host "   当天已过期数：$issueCount 期"
Write-Host ""

# 当前期号
$currentIssueId = $baseDayIssueId + $issueCount

# 计算当前期号的详细信息
$currentDaysDiff = [Math]::Floor(($currentIssueId - $FIRST_ISSUE_ID) / $ISSUES_PER_DAY)
$currentNumber = ($currentIssueId - $FIRST_ISSUE_ID) % $ISSUES_PER_DAY + 1
$currentDate = $firstTime.AddDays($currentDaysDiff)
$currentOpenTime = $currentDate.AddMinutes($MINUTES_PER_ISSUE * ($currentNumber - 1))
$secondsToOpen = [Math]::Floor(($currentOpenTime - $now).TotalSeconds)

Write-Host "🎯 当前期号信息：" -ForegroundColor Green
Write-Host "   完整期号：$currentIssueId" -ForegroundColor Green
Write-Host "   显示期号：$("{0:D3}" -f ($currentIssueId % 1000)) (后3位)"
Write-Host "   当天第几期：第 $currentNumber 期"
Write-Host "   开奖时间：$($currentOpenTime.ToString('yyyy-MM-dd HH:mm:ss'))"

if ($secondsToOpen -gt 0) {
    $minutes = [Math]::Floor($secondsToOpen / 60)
    $secs = $secondsToOpen % 60
    Write-Host "   距离开奖：$("{0:D2}:{1:D2}" -f $minutes, $secs) ($secondsToOpen 秒)"
} else {
    $absSeconds = [Math]::Abs($secondsToOpen)
    $minutes = [Math]::Floor($absSeconds / 60)
    $secs = $absSeconds % 60
    Write-Host "   已开奖：$("{0:D2}:{1:D2}" -f $minutes, $secs) 前 ($absSeconds 秒)" -ForegroundColor Red
}
Write-Host ""

# 上一期信息
$lastIssueId = $currentIssueId - 1
$lastDaysDiff = [Math]::Floor(($lastIssueId - $FIRST_ISSUE_ID) / $ISSUES_PER_DAY)
$lastNumber = ($lastIssueId - $FIRST_ISSUE_ID) % $ISSUES_PER_DAY + 1
$lastDate = $firstTime.AddDays($lastDaysDiff)
$lastOpenTime = $lastDate.AddMinutes($MINUTES_PER_ISSUE * ($lastNumber - 1))

Write-Host "⬅️ 上一期信息：" -ForegroundColor Yellow
Write-Host "   完整期号：$lastIssueId"
Write-Host "   显示期号：$("{0:D3}" -f ($lastIssueId % 1000)) (后3位)"
Write-Host "   当天第几期：第 $lastNumber 期"
Write-Host "   开奖时间：$($lastOpenTime.ToString('yyyy-MM-dd HH:mm:ss'))"
Write-Host ""

# 下一期信息
$nextIssueId = $currentIssueId + 1
$nextDaysDiff = [Math]::Floor(($nextIssueId - $FIRST_ISSUE_ID) / $ISSUES_PER_DAY)
$nextNumber = ($nextIssueId - $FIRST_ISSUE_ID) % $ISSUES_PER_DAY + 1
$nextDate = $firstTime.AddDays($nextDaysDiff)
$nextOpenTime = $nextDate.AddMinutes($MINUTES_PER_ISSUE * ($nextNumber - 1))
$secondsToNext = [Math]::Floor(($nextOpenTime - $now).TotalSeconds)

Write-Host "➡️ 下一期信息：" -ForegroundColor Yellow
Write-Host "   完整期号：$nextIssueId"
Write-Host "   显示期号：$("{0:D3}" -f ($nextIssueId % 1000)) (后3位)"
Write-Host "   当天第几期：第 $nextNumber 期"
Write-Host "   开奖时间：$($nextOpenTime.ToString('yyyy-MM-dd HH:mm:ss'))"
$nextMinutes = [Math]::Floor($secondsToNext / 60)
$nextSecs = $secondsToNext % 60
Write-Host "   距离开奖：$("{0:D2}:{1:D2}" -f $nextMinutes, $nextSecs) ($secondsToNext 秒)"
Write-Host ""

# 今日期号范围
$today = $now.Date
$firstIssueTodayTime = $today.AddHours(7).AddMinutes(5)
$lastIssueTodayTime = $today.AddHours(23).AddMinutes(59).AddSeconds(59)

# 计算今日第一期
$firstTimeSpan = $firstIssueTodayTime - $firstTime
$firstDays = [Math]::Floor($firstTimeSpan.TotalDays)
$firstIssueToday = $FIRST_ISSUE_ID + $firstDays * $ISSUES_PER_DAY

# 计算今日最后一期
$lastTimeSpan = $lastIssueTodayTime - $firstTime
$lastDays = [Math]::Floor($lastTimeSpan.TotalDays)
$lastBaseDayIssueId = $FIRST_ISSUE_ID + $lastDays * $ISSUES_PER_DAY
$lastIssueCount = 0
for ($i = 0; $i -lt $ISSUES_PER_DAY; $i++) {
    $issueId = $lastBaseDayIssueId + $i
    $daysDiff = [Math]::Floor(($issueId - $FIRST_ISSUE_ID) / $ISSUES_PER_DAY)
    $number = ($issueId - $FIRST_ISSUE_ID) % $ISSUES_PER_DAY + 1
    
    $issueDate = $firstTime.AddDays($daysDiff)
    $issueOpenTime = $issueDate.AddMinutes($MINUTES_PER_ISSUE * ($number - 1))
    
    if ($lastIssueTodayTime -gt $issueOpenTime) {
        $lastIssueCount++
    } else {
        break
    }
}
$lastIssueToday = $lastBaseDayIssueId + $lastIssueCount

Write-Host "📆 今日期号范围：" -ForegroundColor Yellow
Write-Host "   第一期：$firstIssueToday (07:05:00)"
Write-Host "   最后期：$lastIssueToday"
Write-Host "   总期数：$($lastIssueToday - $firstIssueToday + 1) 期"
Write-Host ""

# 最近5期信息
Write-Host "📊 最近5期信息：" -ForegroundColor Yellow
Write-Host "┌────────────┬──────────┬──────────┬───────────────────┐"
Write-Host "│ 完整期号   │ 显示期号 │ 当天期数 │ 开奖时间          │"
Write-Host "├────────────┼──────────┼──────────┼───────────────────┤"

for ($i = -2; $i -le 2; $i++) {
    $issueId = $currentIssueId + $i
    $daysDiff = [Math]::Floor(($issueId - $FIRST_ISSUE_ID) / $ISSUES_PER_DAY)
    $number = ($issueId - $FIRST_ISSUE_ID) % $ISSUES_PER_DAY + 1
    $issueDate = $firstTime.AddDays($daysDiff)
    $issueOpenTime = $issueDate.AddMinutes($MINUTES_PER_ISSUE * ($number - 1))
    
    $displayIssue = "{0:D3}" -f ($issueId % 1000)
    $marker = if ($i -eq 0) { " ← 当前" } else { "" }
    
    Write-Host ("│ {0} │   {1}    │ 第{2,3}期 │ {3}          │{4}" -f $issueId, $displayIssue, $number, $issueOpenTime.ToString('HH:mm:ss'), $marker)
}
Write-Host "└────────────┴──────────┴──────────┴───────────────────┘"
Write-Host ""

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "✅ 测试完成" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan


