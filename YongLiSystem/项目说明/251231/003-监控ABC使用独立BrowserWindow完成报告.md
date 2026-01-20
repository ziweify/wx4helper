# 监控ABC使用独立BrowserWindow完成报告

## 日期
2025-12-31

## 需求澄清
用户希望使用我们之前设计的 `Unit.Browser.BrowserWindow` + `BrowserWindowProxy` 通信方案，而不是简单的嵌入式WebView2。

## 核心架构

### 1. 独立浏览器窗口
使用 `Unit.Browser.BrowserWindowProxy` 创建独立的浏览器窗口：
- 浏览器运行在独立的STA线程
- 不会阻塞主UI
- 可以显示/隐藏窗口
- 支持命令式交互

### 2. 监控页面UI
监控ABC页面显示：
- **状态标签**: 显示当前浏览器状态
- **控制按钮**: 
  - 显示浏览器窗口
  - 隐藏浏览器窗口
  - 刷新页面
- **日志区域**: 显示所有操作日志

### 3. 通信机制
通过 `BrowserWindowProxy` 发送命令到独立浏览器窗口：

```csharp
// 登录
await _browserProxy.ExecuteCommandAsync("登录", new { username, password });

// 执行脚本
await _browserProxy.ExecuteCommandAsync("执行脚本", script);

// 获取Cookie
await _browserProxy.ExecuteCommandAsync("获取Cookie");

// 刷新页面
await _browserProxy.ExecuteCommandAsync("刷新页面");

// 重新导航
await _browserProxy.ExecuteCommandAsync("重新导航", url);
```

## 实现细节

### MonitorControlBase
```csharp
public abstract class MonitorControlBase : XtraUserControl
{
    protected BrowserWindowProxy? _browserProxy;  // 浏览器代理
    
    // 初始化独立浏览器窗口
    protected async Task InitializeBrowserAsync()
    {
        _browserProxy = new BrowserWindowProxy();
        _browserProxy.OnLog += (s, msg) => LogMessage($"[浏览器] {msg}");
        await _browserProxy.InitializeAsync(windowTitle, url);
    }
    
    // 执行命令
    public async Task ExecuteMonitorCommand(string commandName)
    {
        var result = await _browserProxy.ExecuteCommandAsync(commandName, parameters);
    }
}
```

### 窗口控制
- `_browserProxy.ShowWindow()` - 显示浏览器窗口
- `_browserProxy.HideWindow()` - 隐藏浏览器窗口
- `_browserProxy.CloseWindow()` - 关闭浏览器窗口

### 日志集成
所有操作日志同时：
1. 显示在监控页面的日志区域
2. 记录到主窗口的 `LoggingService`
3. 浏览器窗口的日志通过 `OnLog` 事件传回

## 使用流程

### 1. 启动应用
打开"数据采集"页面

### 2. 配置监控
在"配置"标签页设置URL、用户名密码、脚本

### 3. 自动初始化
切换到"监控A"标签页：
- 自动创建独立浏览器窗口
- 浏览器窗口在后台打开（可通过按钮显示）
- 如果启用自动登录，自动执行登录

### 4. 查看状态
在监控页面可以看到：
- 浏览器初始化状态
- 操作日志
- 实时命令执行结果

### 5. 控制浏览器
- 点击"显示浏览器窗口" - 弹出独立窗口
- 点击"隐藏浏览器窗口" - 隐藏窗口但保持运行
- 点击"刷新页面" - 刷新浏览器内容

### 6. 执行命令
从配置页面点击命令按钮：
- **登录**: 发送登录命令到浏览器
- **采集**: 执行配置的脚本
- **获取Cookie**: 获取当前Cookie

## 架构优势

### 1. 独立线程运行
- 浏览器在独立STA线程
- 不阻塞主UI
- 支持长时间操作

### 2. 命令式交互
- 通过 `BrowserCommandQueue` 发送命令
- 使用 `TaskCompletionSource` 等待结果
- 线程安全的通信机制

### 3. 可扩展命令系统
- 基础命令: 导航、刷新、执行脚本
- 扩展命令: 登录、投注、采集等
- 通过 `ICommandExecutor` 接口扩展

### 4. 灵活的窗口管理
- 可以显示/隐藏窗口
- 窗口独立运行，不影响主UI
- 可以同时运行多个浏览器窗口（ABC）

## 技术要点

### BrowserWindowProxy 生命周期
```csharp
// 创建
_browserProxy = new BrowserWindowProxy();

// 初始化（在独立线程中创建BrowserWindow）
await _browserProxy.InitializeAsync(title, url);

// 使用
var result = await _browserProxy.ExecuteCommandAsync(command, params);

// 清理
_browserProxy.CloseWindow();
_browserProxy.Dispose();
```

### 命令执行流程
```
1. 调用 ExecuteCommandAsync(commandName, parameters)
2. BrowserWindowProxy 创建 BrowserCommand 对象
3. 将命令加入 BlockingCollection 队列
4. BrowserWindow 线程从队列取出命令
5. CommandExecutor 执行命令并返回结果
6. 通过 TaskCompletionSource 返回结果给调用方
```

## 与嵌入式WebView2的区别

| 特性 | 嵌入式WebView2 | 独立BrowserWindow |
|------|---------------|-------------------|
| 运行方式 | 嵌入在控件中 | 独立窗口 |
| 线程 | 主UI线程 | 独立STA线程 |
| 显示 | 总是可见 | 可显示/隐藏 |
| 通信 | 直接调用API | 命令队列 + 异步等待 |
| 阻塞风险 | 可能阻塞UI | 不会阻塞 |
| 多实例 | 布局复杂 | 独立窗口，易管理 |
| 扩展性 | 有限 | 强（命令系统） |

## 配置示例

### 监控A - 台湾彩票
```csharp
MonitorAConfig.Url = "https://www.taiwanlottery.com.tw/...";
MonitorAConfig.AutoLogin = false;
MonitorAConfig.Script = @"
(function() {
    var issueEl = document.querySelector('#right_overflow_hinet > div');
    return issueEl ? issueEl.innerText : null;
})();
";
```

## 下一步建议

1. **增强命令系统**
   - 添加更多业务命令（如"投注"）
   - 支持自定义命令注册

2. **监控自动化**
   - 定时采集
   - 数据变化通知
   - 自动重连

3. **窗口管理增强**
   - 记住窗口位置和大小
   - 支持多显示器
   - 窗口分组管理

4. **调试工具**
   - 集成WebView2 DevTools
   - 命令历史记录
   - 脚本调试器

---

**状态**: ✅ 完成  
**架构**: Unit.Browser独立窗口 + BrowserWindowProxy通信  
**优势**: 不阻塞UI、灵活控制、易扩展

