-- 诊断查询：查看时间戳的实际存储格式和转换结果
-- 执行此查询后，请把结果告诉我，我会根据实际格式提供精确的解决方案

SELECT 
    Id,
    Timestamp AS 原始时间戳,
    typeof(Timestamp) AS 数据类型,
    length(Timestamp) AS 字符串长度,
    substr(Timestamp, 1, 30) AS 前30个字符,
    -- 尝试各种转换方式
    datetime(replace(replace(Timestamp, 'T', ' '), '.000', '')) AS 方案1_replace,
    datetime(substr(Timestamp, 1, 19)) AS 方案2_substr19,
    datetime(substr(Timestamp, 1, 10) || ' ' || substr(Timestamp, 12, 8)) AS 方案3_拼接,
    strftime('%Y-%m-%d %H:%M:%S', Timestamp) AS 方案4_strftime,
    datetime(Timestamp, 'unixepoch', 'localtime') AS 方案5_unix,
    -- 如果包含 T，尝试替换
    CASE 
        WHEN Timestamp LIKE '%T%' THEN datetime(replace(Timestamp, 'T', ' '))
        ELSE '无T分隔符'
    END AS 方案6_仅替换T
FROM LogEntry
ORDER BY Id DESC
LIMIT 5;

