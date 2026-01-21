# 脚本管理器控件 - 完整使用指南

## 📖 概述

`ScriptManagerControl` 是一个完善的脚本管理控件，支持**远程URL**和**本地文件**两种模式，提供了友好的脚本管理界面。

## 🎯 核心功能

### 1. 双模式支持

- **📁 本地文件模式**（默认）
  - 从本地文件夹加载 `.lua` 脚本文件
  - 支持新建、删除、保存脚本
  - 支持创建模板脚本（main.lua, functions.lua）
  - 支持打开文件夹

- **🌐 远程URL模式**
  - 从远程API加载脚本（JSON格式）
  - 支持认证Token
  - 支持测试连接
  - JSON格式: `{"脚本a": "内容", "脚本b": "内容"}`

### 2. 脚本管理

- 脚本列表显示（按类型排序）
- 双击脚本进行选择
- 脚本类型图标：
  - 🚀 主脚本（Main）
  - 📚 功能库（Functions）
  - 🧪 测试脚本（Test）
  - 📄 自定义（Custom）

### 3. 配置管理

- 保存和加载配置
- 自动推断脚本类型
- 元数据支持（来源、URL、加载时间等）

## 📋 使用方法

### 场景1：在 BrowserTaskControl 中集成脚本管理器

```csharp
public partial class BrowserTaskControl : Form
{
    private ScriptManagerControl _scriptManager;
    private ScriptEditorControl _scriptEditor;
    
    public BrowserTaskControl(BrowserTaskConfig config)
    {
        InitializeComponent();
        
        // 创建脚本管理器
        _scriptManager = new ScriptManagerControl
        {
            Dock = DockStyle.Left,
            Width = 300
        };
        
        // 从配置加载脚本源
        _scriptManager.SourceConfig = new ScriptSourceConfig
        {
            Mode = config.ScriptSourceMode,
            LocalDirectory = config.LocalDirectory,
            RemoteUrl = config.RemoteUrl,
            RemoteAuthToken = config.RemoteAuthToken
        };
        
        // 订阅脚本选中事件
        _scriptManager.ScriptSelected += (s, scriptInfo) =>
        {
            if (scriptInfo != null)
            {
                // 在编辑器中显示脚本
                _scriptEditor.ScriptText = scriptInfo.Content;
                LogMessage($"已加载脚本: {scriptInfo.DisplayName}");
            }
        };
        
        // 订阅配置变更事件
        _scriptManager.ConfigChanged += (s, config) =>
        {
            // 保存配置变更
            SaveScriptSourceConfig(config);
        };
        
        // 将脚本管理器添加到窗体
        Controls.Add(_scriptManager);
    }
    
    // 执行脚本
    private async Task ExecuteScriptsAsync()
    {
        try
        {
            // 1. 加载功能库
            var functionsScript = _scriptManager.GetFunctionsScript();
            if (functionsScript != null)
            {
                LogMessage("加载功能库...");
                await _scriptEngine.ExecuteAsync(functionsScript.Content);
            }
            
            // 2. 执行主脚本
            var mainScript = _scriptManager.GetMainScript();
            if (mainScript != null)
            {
                LogMessage("执行主脚本...");
                await _scriptEngine.ExecuteAsync(mainScript.Content);
            }
            
            LogMessage("脚本执行完成");
        }
        catch (Exception ex)
        {
            LogMessage($"脚本执行失败: {ex.Message}");
        }
    }
}
```

### 场景2：本地模式使用

```csharp
// 创建脚本管理器
var scriptManager = new ScriptManagerControl();

// 设置本地模式
scriptManager.SourceConfig = new ScriptSourceConfig
{
    Mode = ScriptSourceMode.Local,
    LocalDirectory = @"E:\MyScripts\Task1"
};

// 加载本地脚本
scriptManager.LoadLocalScripts();

// 创建模板脚本（如果目录为空）
// 会自动创建 main.lua, functions.lua, README.md
LocalScriptLoader.CreateDefaultScripts(@"E:\MyScripts\Task1");

// 获取主脚本
var mainScript = scriptManager.GetMainScript();
if (mainScript != null)
{
    Console.WriteLine($"主脚本内容: {mainScript.Content}");
}

// 保存脚本（修改后）
mainScript.Content = "-- 修改后的内容";
scriptManager.SaveScript(mainScript);
```

### 场景3：远程模式使用

```csharp
// 创建脚本管理器
var scriptManager = new ScriptManagerControl();

// 设置远程模式
scriptManager.SourceConfig = new ScriptSourceConfig
{
    Mode = ScriptSourceMode.Remote,
    RemoteUrl = "https://api.example.com/scripts/task1",
    RemoteAuthToken = "your_token_here" // 可选
};

// 加载远程脚本
await scriptManager.LoadRemoteScripts();

// 获取所有脚本
var scripts = scriptManager.Scripts;
foreach (var script in scripts)
{
    Console.WriteLine($"脚本: {script.Name}");
    Console.WriteLine($"  类型: {script.Type}");
    Console.WriteLine($"  来源: {script.Metadata["source"]}");
    Console.WriteLine($"  URL: {script.Metadata.GetValueOrDefault("url", "N/A")}");
}
```

### 场景4：服务端API示例（ASP.NET Core）

```csharp
// 服务端API返回脚本列表
[HttpGet("scripts/{taskId}")]
public IActionResult GetScripts(string taskId)
{
    // 从数据库或文件系统加载脚本
    var scripts = new Dictionary<string, string>
    {
        ["main.lua"] = @"
-- 主脚本
log('主脚本开始')
function main()
    login('user', 'pass')
    getData()
end
main()
",
        ["functions.lua"] = @"
-- 功能库
function login(user, pass)
    log('登录: ' .. user)
    return true
end

function getData()
    log('获取数据')
    return 'data'
end
",
        ["test.lua"] = @"
-- 测试脚本
log('测试开始')
login('test', 'test')
log('测试完成')
"
    };
    
    return Ok(scripts);
}

// 保存脚本（远程模式）
[HttpPost("scripts/{taskId}")]
public IActionResult SaveScripts(string taskId, [FromBody] Dictionary<string, string> scripts)
{
    // 保存到数据库或文件系统
    // ...
    
    return Ok(new { message = "脚本保存成功" });
}
```

## 🔧 配置模型

### ScriptSourceConfig

```csharp
public class ScriptSourceConfig
{
    /// <summary>
    /// 脚本源模式（Local / Remote）
    /// </summary>
    public ScriptSourceMode Mode { get; set; } = ScriptSourceMode.Local;
    
    /// <summary>
    /// 本地文件夹路径（本地模式）
    /// </summary>
    public string LocalDirectory { get; set; } = string.Empty;
    
    /// <summary>
    /// 远程URL（远程模式）
    /// </summary>
    public string RemoteUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// 远程认证Token（可选）
    /// </summary>
    public string? RemoteAuthToken { get; set; }
    
    /// <summary>
    /// 自动刷新间隔（秒，0=不自动刷新）
    /// </summary>
    public int AutoRefreshInterval { get; set; } = 0;
    
    /// <summary>
    /// 验证配置是否有效
    /// </summary>
    public bool IsValid()
    {
        return Mode switch
        {
            ScriptSourceMode.Local => !string.IsNullOrEmpty(LocalDirectory),
            ScriptSourceMode.Remote => !string.IsNullOrEmpty(RemoteUrl),
            _ => false
        };
    }
}
```

### ScriptInfo

```csharp
public class ScriptInfo
{
    public string Id { get; set; }              // 唯一标识
    public string Name { get; set; }            // 文件名（如 main.lua）
    public string DisplayName { get; set; }     // 显示名称
    public string Content { get; set; }         // 脚本内容
    public string? FilePath { get; set; }       // 文件路径（本地模式）
    public ScriptType Type { get; set; }        // 脚本类型
    public bool IsMemoryMode { get; }           // 是否为内存模式
    public DateTime CreatedAt { get; set; }     // 创建时间
    public DateTime ModifiedAt { get; set; }    // 修改时间
    public bool IsModified { get; set; }        // 是否已修改
    public Dictionary<string, string> Metadata { get; set; }  // 元数据
}

public enum ScriptType
{
    Main,       // 主脚本
    Functions,  // 功能库
    Test,       // 测试脚本
    Custom      // 自定义
}
```

## 📊 事件

### ScriptSelected

```csharp
_scriptManager.ScriptSelected += (sender, scriptInfo) =>
{
    if (scriptInfo != null)
    {
        Console.WriteLine($"选中脚本: {scriptInfo.DisplayName}");
        // 在编辑器中加载脚本
        _scriptEditor.ScriptText = scriptInfo.Content;
    }
};
```

### ScriptsUpdated

```csharp
_scriptManager.ScriptsUpdated += (sender, e) =>
{
    Console.WriteLine($"脚本列表已更新，共 {_scriptManager.Scripts.Count} 个脚本");
};
```

### ConfigChanged

```csharp
_scriptManager.ConfigChanged += (sender, config) =>
{
    Console.WriteLine($"配置已更改: {config.Mode}");
    // 保存配置到数据库或文件
    SaveConfig(config);
};
```

## 🎨 UI布局

```
┌─────────────────────────────────────────┐
│ 📁 本地文件  ○ 🌐 远程URL              │ ← 模式切换
├─────────────────────────────────────────┤
│ 本地目录: [_____] [浏览] [创建模板] [刷新] │ ← 本地模式配置
│ 💡 提示: 选择包含 .lua 脚本的文件夹       │
├─────────────────────────────────────────┤
│ 脚本列表:                                │
│ ┌─────────────────────────────────────┐ │
│ │ 🚀 main (主脚本)                     │ │
│ │ 📚 functions (功能库)                │ │
│ │ 🧪 test (测试脚本)                   │ │
│ │ 📄 custom1 (自定义)                  │ │
│ └─────────────────────────────────────┘ │
├─────────────────────────────────────────┤
│ [➕ 新建脚本] [🗑 删除] [📂 打开文件夹]   │ ← 操作按钮
└─────────────────────────────────────────┘
```

## 🚀 完整集成示例

### 在 YongLiSystem 的数据采集页面中使用

```csharp
public partial class DataCollectionPage : XtraUserControl
{
    private void OnAddTask(object? sender, EventArgs e)
    {
        // 创建浏览器任务配置
        var config = new BrowserTaskConfig
        {
            Url = "https://example.com",
            Username = "user",
            Password = "pass",
            
            // 脚本源配置
            ScriptSourceMode = ScriptSourceMode.Local,
            LocalDirectory = $@"E:\Scripts\Task_{Guid.NewGuid():N}"
        };
        
        // 创建脚本目录和模板
        Directory.CreateDirectory(config.LocalDirectory);
        LocalScriptLoader.CreateDefaultScripts(config.LocalDirectory);
        
        // 创建浏览器任务控件
        var taskControl = new BrowserTaskControl(config);
        taskControl.Show();
    }
}
```

## ✅ 优势总结

1. **双模式切换**：本地/远程无缝切换，满足不同场景
2. **友好UI**：清晰的界面布局，易于使用
3. **模板支持**：一键创建标准脚本模板
4. **类型推断**：自动识别脚本类型（main/functions/test）
5. **元数据扩展**：支持存储额外信息（来源、版本、URL等）
6. **事件驱动**：完善的事件机制，易于集成
7. **错误处理**：友好的错误提示和异常处理
8. **未来扩展**：架构设计支持网络保存等高级功能

## 🔮 未来扩展

1. **网络保存**: 实现远程脚本的保存功能（需服务端API支持）
2. **版本控制**: 脚本版本历史和回滚
3. **脚本搜索**: 根据名称、内容搜索脚本
4. **脚本分组**: 支持脚本分组管理
5. **在线编辑**: 直接在管理器中编辑脚本（集成ScriptEditorControl）
6. **脚本市场**: 从脚本市场下载公共脚本模板

---

**文档版本**: 1.0  
**创建日期**: 2026-01-22  
**作者**: AI Assistant  
**状态**: ✅ 已完成并通过编译
