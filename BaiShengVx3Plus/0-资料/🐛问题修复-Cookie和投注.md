# 🐛 问题修复：Cookie回传和投注功能

**问题报告时间：** 2025-11-08 13:18  
**发现问题：**
1. Cookie没有回传到配置
2. 投注命令发送后没有成功

---

## 📋 问题分析

### 问题1：Cookie未回传到配置 ❌

**现象：**
- BrowserClient登录后，日志显示"📤 Cookie已回传到VxMain"
- 但配置管理界面没有显示Cookie
- 数据库BetConfig表的Cookie字段为空

**原因分析：**

**1. AutoBetSocketServer未处理cookie_update消息**
```csharp
// 文件：BaiShengVx3Plus/Services/AutoBet/AutoBetSocketServer.cs:203
while (!cancellationToken.IsCancellationRequested)
{
    var line = await reader.ReadLineAsync(cancellationToken);
    if (string.IsNullOrEmpty(line)) break;
    
    // ❌ 这里只记录了日志，没有处理消息！
    _log.Info("AutoBetServer", $"📩 [{configId}] {line}");
    
    // ⚠️ 缺少消息类型判断和处理逻辑
}
```

**2. 缺少Cookie更新处理方法**
- AutoBetService没有`UpdateCookie`方法
- 没有回调处理器

---

### 问题2：投注未成功 ❌

**现象：**
```
📤 发送命令:投注(1234大10)
📝 命令:投注
   参数:1234大10
✅ 返回:成功=False
   消息:未实现
```

**原因分析：**

**1. 命令格式问题**
```csharp
// BetConfigManagerForm.cs 解析：
ParseCommand("123大10") 
→ cmdName="123大10", cmdParam="" // ❌ 错误！没有括号

ParseCommand("投注(1234大10)") 
→ cmdName="投注", cmdParam="1234大10" // ✅ 正确
```

**2. 投注内容未解析**
```csharp
// 当前代码直接发送："1234大10"
// 但应该解析为："1大10,2大10,3大10,4大10"
```

**3. 期号为"0"**
```csharp
await autoBetService.SendBetCommandAsync(_selectedConfig.Id, "0", cmdParam);
// ❌ 期号硬编码为"0"，BrowserClient可能拒绝投注
```

---

## 🔧 修复方案

### 修复1：实现Cookie更新处理

#### 步骤1：修改AutoBetSocketServer处理消息

**文件：** `BaiShengVx3Plus/Services/AutoBet/AutoBetSocketServer.cs`

```csharp
// 在构造函数添加回调参数
private readonly Action<int, TcpClient> _onBrowserConnected;
private readonly Action<int, JObject>? _onMessageReceived; // 🔥 新增

public AutoBetSocketServer(
    int port, 
    ILogService log, 
    Action<int, TcpClient> onBrowserConnected,
    Action<int, JObject>? onMessageReceived = null) // 🔥 新增
{
    _port = port;
    _log = log;
    _onBrowserConnected = onBrowserConnected;
    _onMessageReceived = onMessageReceived; // 🔥 新增
}

// 修改消息读取循环
while (!cancellationToken.IsCancellationRequested)
{
    var line = await reader.ReadLineAsync(cancellationToken);
    if (string.IsNullOrEmpty(line))
    {
        _log.Warning("AutoBetServer", $"配置 {configId} 连接已断开");
        break;
    }
    
    _log.Info("AutoBetServer", $"📩 [{configId}] {line}");
    
    // 🔥 解析并处理消息
    try
    {
        var message = JsonConvert.DeserializeObject<JObject>(line);
        if (message != null)
        {
            var messageType = message["type"]?.ToString();
            
            // 处理不同类型的消息
            switch (messageType)
            {
                case "cookie_update":
                    _log.Info("AutoBetServer", $"🍪 收到Cookie更新:{configId}");
                    _onMessageReceived?.Invoke(configId, message);
                    break;
                    
                case "login_success":
                    _log.Info("AutoBetServer", $"✅ 登录成功通知:{configId}");
                    _onMessageReceived?.Invoke(configId, message);
                    break;
                    
                default:
                    _log.Info("AutoBetServer", $"📨 收到消息:{messageType}");
                    _onMessageReceived?.Invoke(configId, message);
                    break;
            }
        }
    }
    catch (Exception parseEx)
    {
        _log.Error("AutoBetServer", "解析消息失败", parseEx);
    }
}
```

#### 步骤2：在AutoBetService添加消息处理

**文件：** `BaiShengVx3Plus/Services/AutoBet/AutoBetService.cs`

```csharp
// 修改StartSocketServer方法
private void StartSocketServer()
{
    try
    {
        var port = 9999;
        _socketServer = new AutoBetSocketServer(
            port, 
            _log, 
            OnBrowserConnected,
            OnMessageReceived // 🔥 新增消息处理回调
        );
        _socketServer.Start();
        _log.Info("AutoBet", $"✅ Socket 服务器已启动，端口: {port}");
    }
    catch (Exception ex)
    {
        _log.Error("AutoBet", "启动 Socket 服务器失败", ex);
    }
}

// 🔥 新增消息处理方法
private void OnMessageReceived(int configId, JObject message)
{
    try
    {
        var messageType = message["type"]?.ToString();
        
        switch (messageType)
        {
            case "cookie_update":
                HandleCookieUpdate(configId, message);
                break;
                
            case "login_success":
                HandleLoginSuccess(configId, message);
                break;
                
            default:
                _log.Info("AutoBet", $"未处理的消息类型:{messageType}");
                break;
        }
    }
    catch (Exception ex)
    {
        _log.Error("AutoBet", "处理消息失败", ex);
    }
}

// 🔥 处理Cookie更新
private void HandleCookieUpdate(int configId, JObject message)
{
    try
    {
        var url = message["url"]?.ToString();
        var cookies = message["cookies"]?.ToObject<Dictionary<string, string>>();
        
        if (cookies == null || cookies.Count == 0)
        {
            _log.Warning("AutoBet", $"配置{configId} Cookie为空");
            return;
        }
        
        // 转换为Cookie字符串
        var cookieString = string.Join("; ", cookies.Select(kv => $"{kv.Key}={kv.Value}"));
        
        // 更新配置
        var config = GetConfig(configId);
        if (config != null)
        {
            config.Cookie = cookieString;
            config.CookieUpdateTime = DateTime.Now;
            UpdateConfig(config);
            
            _log.Info("AutoBet", $"✅ 配置{configId}({config.ConfigName}) Cookie已更新:{cookies.Count}个");
        }
    }
    catch (Exception ex)
    {
        _log.Error("AutoBet", $"更新Cookie失败:配置{configId}", ex);
    }
}

// 🔥 处理登录成功
private void HandleLoginSuccess(int configId, JObject message)
{
    try
    {
        var username = message["username"]?.ToString();
        _log.Info("AutoBet", $"✅ 配置{configId} 登录成功:用户{username}");
        
        // 可以在这里触发其他操作（如刷新配置状态）
    }
    catch (Exception ex)
    {
        _log.Error("AutoBet", "处理登录成功失败", ex);
    }
}
```

---

### 修复2：完善投注命令处理

#### 步骤1：添加投注内容解析方法

**文件：** `BaiShengVx3Plus/Views/AutoBet/BetConfigManagerForm.cs`

```csharp
/// <summary>
/// 解析投注内容："1234大10" → "1大10,2大10,3大10,4大10"
/// </summary>
private string ParseBetContent(string input)
{
    try
    {
        var items = new List<string>();
        
        // 按空格或逗号分割
        var parts = input.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var part in parts)
        {
            var trimmed = part.Trim();
            
            // 检查是否包含连续数字（如："1234大20"）
            var match = System.Text.RegularExpressions.Regex.Match(
                trimmed, 
                @"^(\d+)(大|小|单|双)(\d+)$"
            );
            
            if (match.Success)
            {
                var numbers = match.Groups[1].Value;  // "1234"
                var type = match.Groups[2].Value;      // "大"
                var amount = match.Groups[3].Value;    // "10"
                
                // 拆分为单个投注
                foreach (var num in numbers)
                {
                    items.Add($"{num}{type}{amount}");
                }
            }
            else
            {
                // 已经是标准格式或无法解析，直接添加
                items.Add(trimmed);
            }
        }
        
        return string.Join(",", items);
    }
    catch (Exception ex)
    {
        _logService.Error("CommandPanel", "解析投注内容失败", ex);
        return input; // 解析失败返回原内容
    }
}

/// <summary>
/// 计算总金额："1大10,2大20" → 30
/// </summary>
private decimal CalculateTotalAmount(string standardContent)
{
    try
    {
        decimal total = 0;
        var items = standardContent.Split(',');
        
        foreach (var item in items)
        {
            var match = System.Text.RegularExpressions.Regex.Match(item, @"(\d+)$");
            if (match.Success && decimal.TryParse(match.Groups[1].Value, out var amount))
            {
                total += amount;
            }
        }
        
        return total;
    }
    catch
    {
        return 0;
    }
}
```

#### 步骤2：修改投注命令发送逻辑

```csharp
case "投注":
    // 1. 获取当前期号
    var lotteryService = Program.ServiceProvider.GetService(typeof(Contracts.Games.IBinggoLotteryService)) 
        as Contracts.Games.IBinggoLotteryService;
    var currentIssueId = lotteryService?.CurrentIssueId ?? 0;
    
    if (currentIssueId == 0)
    {
        AppendCommandResult("⚠️ 警告:无法获取当前期号，将使用期号0");
    }
    
    // 2. 解析投注内容
    var originalContent = cmdParam; // "1234大10"
    var standardContent = ParseBetContent(originalContent); // "1大10,2大10,3大10,4大10"
    
    AppendCommandResult($"   原始:{originalContent}");
    AppendCommandResult($"   解析:{standardContent}");
    
    // 3. 发送投注命令
    var betResult = await autoBetService.SendBetCommandAsync(
        _selectedConfig.Id, 
        currentIssueId.ToString(), 
        standardContent
    );
    
    return new CommandResponse
    {
        Success = betResult.Success,
        Message = betResult.ErrorMessage ?? (betResult.Success ? "投注成功" : "投注失败"),
        Data = new 
        {
            issueId = currentIssueId,
            originalContent = originalContent,
            standardContent = standardContent,
            betResult
        },
        ErrorMessage = betResult.ErrorMessage
    };
```

---

## ✅ 测试步骤

### 测试1：Cookie回传

1. 启动VxMain
2. 启动浏览器客户端（配置管理 → 启动浏览器）
3. 等待页面加载完成
4. 检查日志：
   ```
   [BrowserClient] 📤 Cookie已回传到VxMain:共8个Cookie
   [VxMain] 🍪 收到Cookie更新:1
   [VxMain] ✅ 配置1(默认配置) Cookie已更新:8个
   ```
5. 刷新配置列表，查看Cookie字段是否有值

### 测试2：手动获取Cookie

1. 配置管理 → 选择配置
2. 点击"获取Cookie"按钮
3. 点击"发送"
4. 查看执行结果区域是否显示Cookie数据

### 测试3：投注命令

1. 确保BrowserClient已登录
2. 输入："投注(1234大10)"
3. 点击"发送"
4. 查看执行结果：
   ```
   📤 发送命令:投注(1234大10)
   📝 命令:投注
      参数:1234大10
      原始:1234大10
      解析:1大10,2大10,3大10,4大10
   ✅ 返回:成功=True
      消息:投注成功
   ```

---

## 🎯 修改文件清单

1. **BaiShengVx3Plus/Services/AutoBet/AutoBetSocketServer.cs**
   - 添加消息处理回调参数
   - 修改消息读取循环，解析并分发消息

2. **BaiShengVx3Plus/Services/AutoBet/AutoBetService.cs**
   - 添加`OnMessageReceived`方法
   - 添加`HandleCookieUpdate`方法
   - 添加`HandleLoginSuccess`方法
   - 修改`StartSocketServer`传递回调

3. **BaiShengVx3Plus/Views/AutoBet/BetConfigManagerForm.cs**
   - 添加`ParseBetContent`方法
   - 添加`CalculateTotalAmount`方法
   - 修改"投注"命令处理逻辑

---

## 📝 预计工时

- Cookie回传修复：1小时
- 投注命令修复：1小时
- 测试验证：30分钟
- **合计：2.5小时**

---

**修复后用户体验：**
- ✅ 登录后自动更新Cookie到配置
- ✅ 手动"获取Cookie"命令可用
- ✅ 投注命令自动解析格式
- ✅ 投注命令自动获取期号
- ✅ 详细的命令执行日志

**立即开始修复吗？** 🚀

