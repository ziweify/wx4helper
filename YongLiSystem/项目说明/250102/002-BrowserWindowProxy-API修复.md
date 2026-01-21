# API 修复说明

## 问题
编译时出现以下错误：
- `BrowserWindowProxy` 未包含 `CreateBrowserWindow` 的定义
- `BrowserWindowProxy` 未包含 `Navigate` 的定义
- `BrowserWindowProxy` 未包含 `Close` 的定义
- `flowLayoutTasks` 字段未赋值警告

## 原因
1. `BrowserWindowProxy` 的 API 设计不同，使用的是：
   - `InitializeAsync(windowTitle, initialUrl)` - 初始化并导航
   - `CloseWindow()` - 关闭窗口
   - `Dispose()` - 释放资源

2. `flowLayoutTasks` 在 Designer 的 `InitializeComponent` 方法中未创建实例

## 修复

### 1. 更新 DataCollectionPage.cs

**原代码**:
```csharp
private void StartTask(ScriptTask task, ScriptTaskCardControl card)
{
    var proxy = new BrowserWindowProxy();
    proxy.CreateBrowserWindow();  // ❌ 错误：方法不存在
    
    card.SetBrowserProxy(proxy);
    
    if (!string.IsNullOrWhiteSpace(task.Url))
    {
        proxy.Navigate(task.Url);  // ❌ 错误：方法不存在
    }
    // ...
}

private void StopTask(ScriptTask task)
{
    control.proxy?.Close();  // ❌ 错误：方法不存在
}
```

**修复后**:
```csharp
private async void StartTask(ScriptTask task, ScriptTaskCardControl card)
{
    try
    {
        var proxy = new BrowserWindowProxy();
        
        // ✅ 使用 InitializeAsync，自动创建窗口并导航到 URL
        var windowTitle = $"{task.Name} - {task.Id}";
        await proxy.InitializeAsync(windowTitle, task.Url);
        
        card.SetBrowserProxy(proxy);
        
        task.IsRunning = true;
        task.Status = "运行中";
        // ...
    }
    catch (Exception ex)
    {
        // 错误处理
        task.IsRunning = false;
        task.Status = "启动失败";
    }
}

private void StopTask(ScriptTask task)
{
    if (_taskControls.TryGetValue(task.Id, out var control))
    {
        // ✅ 使用 CloseWindow() 和 Dispose()
        control.proxy?.CloseWindow();
        control.proxy?.Dispose();
        
        task.IsRunning = false;
        task.Status = "已停止";
        // ...
    }
}
```

### 2. 更新 DataCollectionPage.Designer.cs

**在 InitializeComponent 方法开始处添加**:
```csharp
flowLayoutTasks = new System.Windows.Forms.FlowLayoutPanel();
```

这确保了字段被正确初始化。

## BrowserWindowProxy API 说明

```csharp
// 初始化浏览器窗口（在独立 STA 线程中运行）
Task InitializeAsync(string windowTitle, string initialUrl)

// 执行命令
Task<BrowserCommandResult> ExecuteCommandAsync(string commandName, object? parameters)

// 显示/隐藏/关闭窗口
void ShowWindow()
void HideWindow()
void CloseWindow()

// 获取窗口句柄
IntPtr WindowHandle { get; }

// 释放资源
void Dispose()
```

## 编译结果
修复后，项目应该可以成功编译。

## 测试步骤
1. 运行 `.\编译并运行YongLiSystem.ps1`
2. 进入"数据采集"页面
3. 点击"➕ 增加脚本任务"
4. 填写配置并保存
5. 点击任务卡片上的"▶ 启动"按钮
6. 浏览器窗口应该自动打开并导航到指定 URL
7. 缩略图应该每秒自动更新

完成！✅
