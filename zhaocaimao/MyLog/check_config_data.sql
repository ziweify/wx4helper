-- 检查 BetConfig 表结构
PRAGMA table_info(AutoBetConfigs);

-- 查看所有配置数据
SELECT * FROM AutoBetConfigs;

-- 查看账号密码字段（重点）
SELECT Id, ConfigName, Username, Password, Platform, IsEnabled, LastUpdateTime FROM AutoBetConfigs;

-- 查看默认配置
SELECT * FROM AutoBetConfigs WHERE IsDefault = 1;

