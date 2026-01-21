using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unit.La.Models;

namespace Unit.La.Scripting
{
    /// <summary>
    /// 本地脚本加载器 - 从文件夹加载脚本
    /// </summary>
    public class LocalScriptLoader
    {
        private readonly ScriptSourceConfig _config;

        public LocalScriptLoader(ScriptSourceConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// 从本地文件夹加载所有Lua脚本
        /// </summary>
        public List<ScriptInfo> LoadScripts()
        {
            if (string.IsNullOrEmpty(_config.LocalDirectory))
            {
                throw new InvalidOperationException("本地目录未设置");
            }

            if (!Directory.Exists(_config.LocalDirectory))
            {
                throw new DirectoryNotFoundException($"目录不存在: {_config.LocalDirectory}");
            }

            var scripts = new List<ScriptInfo>();

            // 搜索所有 .lua 文件
            var luaFiles = Directory.GetFiles(_config.LocalDirectory, "*.lua", SearchOption.TopDirectoryOnly);

            foreach (var filePath in luaFiles)
            {
                try
                {
                    var fileName = Path.GetFileName(filePath);
                    var content = File.ReadAllText(filePath, Encoding.UTF8);

                    var scriptInfo = new ScriptInfo
                    {
                        Name = fileName,
                        DisplayName = Path.GetFileNameWithoutExtension(fileName),
                        Content = content,
                        FilePath = filePath,
                        Type = InferScriptType(fileName),
                        CreatedAt = File.GetCreationTime(filePath),
                        ModifiedAt = File.GetLastWriteTime(filePath),
                        Metadata = new Dictionary<string, string>
                        {
                            ["source"] = "local",
                            ["directory"] = _config.LocalDirectory,
                            ["file_size"] = new FileInfo(filePath).Length.ToString()
                        }
                    };

                    scripts.Add(scriptInfo);
                }
                catch (Exception ex)
                {
                    // 跳过无法读取的文件，记录错误
                    System.Diagnostics.Debug.WriteLine($"无法加载脚本 {filePath}: {ex.Message}");
                }
            }

            return scripts;
        }

        /// <summary>
        /// 保存脚本到本地文件
        /// </summary>
        public void SaveScript(ScriptInfo script)
        {
            if (string.IsNullOrEmpty(_config.LocalDirectory))
            {
                throw new InvalidOperationException("本地目录未设置");
            }

            if (!Directory.Exists(_config.LocalDirectory))
            {
                Directory.CreateDirectory(_config.LocalDirectory);
            }

            var filePath = string.IsNullOrEmpty(script.FilePath)
                ? Path.Combine(_config.LocalDirectory, script.Name)
                : script.FilePath;

            File.WriteAllText(filePath, script.Content, Encoding.UTF8);

            script.FilePath = filePath;
            script.ModifiedAt = DateTime.Now;
            script.IsModified = false;
        }

        /// <summary>
        /// 删除本地脚本文件
        /// </summary>
        public void DeleteScript(ScriptInfo script)
        {
            if (!string.IsNullOrEmpty(script.FilePath) && File.Exists(script.FilePath))
            {
                File.Delete(script.FilePath);
            }
        }

        /// <summary>
        /// 刷新脚本内容（从文件重新加载）
        /// </summary>
        public void RefreshScript(ScriptInfo script)
        {
            if (string.IsNullOrEmpty(script.FilePath) || !File.Exists(script.FilePath))
            {
                throw new FileNotFoundException($"脚本文件不存在: {script.FilePath}");
            }

            script.Content = File.ReadAllText(script.FilePath, Encoding.UTF8);
            script.ModifiedAt = File.GetLastWriteTime(script.FilePath);
            script.IsModified = false;
        }

        /// <summary>
        /// 根据文件名推断脚本类型
        /// </summary>
        private ScriptType InferScriptType(string fileName)
        {
            var lowerName = fileName.ToLower();

            if (lowerName == "main.lua")
                return ScriptType.Main;
            else if (lowerName == "functions.lua" || lowerName == "lib.lua")
                return ScriptType.Functions;
            else if (lowerName.Contains("test"))
                return ScriptType.Test;
            else
                return ScriptType.Custom;
        }

        /// <summary>
        /// 创建默认脚本模板
        /// </summary>
        public static void CreateDefaultScripts(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // 创建 main.lua
            var mainPath = Path.Combine(directory, "main.lua");
            if (!File.Exists(mainPath))
            {
                File.WriteAllText(mainPath, GetMainScriptTemplate(), Encoding.UTF8);
            }

            // 创建 functions.lua
            var functionsPath = Path.Combine(directory, "functions.lua");
            if (!File.Exists(functionsPath))
            {
                File.WriteAllText(functionsPath, GetFunctionsScriptTemplate(), Encoding.UTF8);
            }

            // 创建 README.md
            var readmePath = Path.Combine(directory, "README.md");
            if (!File.Exists(readmePath))
            {
                File.WriteAllText(readmePath, GetReadmeTemplate(), Encoding.UTF8);
            }
        }

        private static string GetMainScriptTemplate()
        {
            return @"-- ====================================
-- 主脚本 (main.lua)
-- 控制业务逻辑流程
-- ====================================

log('主脚本开始执行')

-- 主要业务逻辑
function main()
    -- 1. 登录
    log('步骤1: 登录')
    local loginSuccess = login(config.username or 'test', config.password or 'pass')
    if not loginSuccess then
        log('登录失败，终止执行')
        return false
    end

    -- 2. 执行业务逻辑
    log('步骤2: 执行业务')
    -- 在这里调用 functions.lua 中定义的功能函数

    log('执行完成')
    return true
end

-- 执行主逻辑
local success = main()
if success then
    log('✅ 主脚本执行成功')
else
    log('❌ 主脚本执行失败')
end
";
        }

        private static string GetFunctionsScriptTemplate()
        {
            return @"-- ====================================
-- 功能库 (functions.lua)
-- 定义所有业务功能函数
-- ====================================

log('功能库加载中...')

-- 示例：登录功能
function login(username, password)
    log('登录: ' .. username)
    
    -- 在这里实现登录逻辑
    navigate(config.url or 'https://example.com/login')
    wait(2000)
    
    -- 填写表单
    -- ...
    
    return true
end

-- 示例：获取数据功能
function getData()
    log('获取数据')
    
    -- 在这里实现数据获取逻辑
    
    return 'data'
end

-- 示例：查询订单
function queryOrder(orderId)
    log('查询订单: ' .. orderId)
    
    -- 在这里实现订单查询逻辑
    
    return {}
end

-- 示例：投注功能
function placeBet(betData)
    log('投注')
    
    -- 在这里实现投注逻辑
    
    return true
end

log('功能库加载完成')
";
        }

        private static string GetReadmeTemplate()
        {
            return @"# 脚本任务说明

## 文件说明

- `main.lua`: 主脚本，控制业务逻辑流程
- `functions.lua`: 功能库，定义所有业务功能函数
- `test.lua`: 测试脚本（可选）

## 使用方法

1. 在 `functions.lua` 中定义功能函数
2. 在 `main.lua` 中调用功能函数，控制业务流程
3. 在浏览器任务配置中指定此目录

## 配置参数

可以在任务配置中设置参数，通过 `config` 对象访问：

```lua
local username = config.username
local password = config.password
local url = config.url
```

## 可用函数

- `login(username, password)` - 登录
- `getData()` - 获取数据
- `queryOrder(orderId)` - 查询订单
- `placeBet(betData)` - 投注
";
        }
    }
}
