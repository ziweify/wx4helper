-- ✅ 正确方案：时间戳存储为 .NET DateTime.Ticks（整数格式）
-- Ticks 是从 0001-01-01 00:00:00 开始的 100 纳秒间隔数
-- DateTime.Now 返回的是本地时间（北京时间），Ticks 已经是本地时间的绝对时间值
-- 因此转换时不需要使用 'localtime'，否则会重复加8小时

DROP VIEW IF EXISTS LogEntryFormatted;

CREATE VIEW LogEntryFormatted AS
SELECT 
    Id,
    -- 将 Ticks 转换为日期时间
    -- Ticks / 10000000 = 秒数（从 0001-01-01 开始）
    -- 减去 62135596800 = 从 0001-01-01 到 1970-01-01 的秒数差
    -- 然后使用 'unixepoch' 转换为日期时间
    -- ⚠️ 注意：不使用 'localtime'，因为 Ticks 已经是本地时间的绝对时间值
    datetime((Timestamp / 10000000.0) - 62135596800, 'unixepoch') AS 时间,
    Level,
    Source,
    Message,
    Exception,
    ThreadId,
    UserId,
    ExtraData
FROM LogEntry;

-- 如果方案1不行，尝试方案2：直接截取前19个字符（YYYY-MM-DD HH:MM:SS）
-- DROP VIEW IF EXISTS LogEntryFormatted;
-- CREATE VIEW LogEntryFormatted AS
-- SELECT 
--     Id,
--     substr(Timestamp, 1, 10) || ' ' || substr(Timestamp, 12, 8) AS 时间,
--     Level,
--     Source,
--     Message,
--     Exception,
--     ThreadId,
--     UserId,
--     ExtraData
-- FROM LogEntry;

-- 如果方案2不行，尝试方案3：使用 strftime（适用于各种格式）
-- DROP VIEW IF EXISTS LogEntryFormatted;
-- CREATE VIEW LogEntryFormatted AS
-- SELECT 
--     Id,
--     strftime('%Y-%m-%d %H:%M:%S', Timestamp) AS 时间,
--     Level,
--     Source,
--     Message,
--     Exception,
--     ThreadId,
--     UserId,
--     ExtraData
-- FROM LogEntry;

