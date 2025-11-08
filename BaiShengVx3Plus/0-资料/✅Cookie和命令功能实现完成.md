# ✅ Cookie回传和命令发送功能实现完成

## 🎯 实施目标

1. **Cookie自动回传**：BrowserClient登录后自动获取Cookie并回传到VxMain
2. **手动命令发送**：配置管理界面支持手动发送命令（投注、获取Cookie、获取额度）

---

## 📋 已实现功能清单

### 1. Cookie自动回传（BrowserClient → VxMain）

**文件：** `BsBrowserClient/Form1.cs`

#### ✅ 实现内容

**1.1 页面加载完成后自动获取Cookie**
```csharp
// 绑定导航事件
_webView.CoreWebView2.NavigationCompleted += async (s, e) =>
{
    if (e.IsSuccess)
    {
        // 触发自动登录
        await TryAutoLoginAsync();
        
        // 🔥 获取Cookie并回传到VxMain
        await GetAndSendCookieToVxMain();
    }
};
```

**1.2 GetAndSendCookieToVxMain 方法**
```csharp
private async Task GetAndSendCookieToVxMain()
{
    // 获取当前页面的所有Cookie
    var cookies = await _webView.CoreWebView2.CookieManager.GetCookiesAsync(_webView.CoreWebView2.Source);
    
    // 将Cookie格式化为字典
    var cookieDict = new Dictionary<string, string>();
    foreach (var cookie in cookies)
    {
        cookieDict[cookie.Name] = cookie.Value;
    }
    
    // 通知VxMain（通过Socket）
    var message = new
    {
        type = "cookie_update",
        configId = _configId,
        url = _webView.CoreWebView2.Source,
        cookies = cookieDict,
        timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
    };
    
    await _socketServer.SendToVxMain(message);
    
    OnLogMessage($"📤 Cookie已回传到VxMain:共{cookies.Count}个Cookie");
}
```

**触发时机：**
- ✅ 页面加载完成（NavigationCompleted事件）
- ✅ 登录成功后
- ✅ 页面跳转后

---

### 2. 获取Cookie命令（VxMain → BrowserClient）

**文件：** `BsBrowserClient/Form1.cs`

#### ✅ 命令处理

```csharp
case "获取Cookie":
    // 获取Cookie命令
    if (_webView?.CoreWebView2 == null)
    {
        response.Message = "WebView2未初始化";
        break;
    }
    
    var allCookies = await _webView.CoreWebView2.CookieManager.GetCookiesAsync(_webView.CoreWebView2.Source);
    var cookieDict = new Dictionary<string, string>();
    
    foreach (var cookie in allCookies)
    {
        cookieDict[cookie.Name] = cookie.Value;
    }
    
    response.Success = true;
    response.Data = new 
    { 
        url = _webView.CoreWebView2.Source,
        cookies = cookieDict,
        count = allCookies.Count
    };
    response.Message = $"获取成功,共{allCookies.Count}个Cookie";
```

---

### 3. 获取盘口额度命令

**文件：** `BsBrowserClient/Form1.cs`

#### ✅ 命令处理

```csharp
case "获取盘口额度":
    // 获取盘口额度命令
    var quotaBalance = await _platformScript!.GetBalanceAsync();
    response.Success = quotaBalance >= 0;
    response.Data = new { balance = quotaBalance, quota = quotaBalance };
    response.Message = response.Success ? $"盘口额度: {quotaBalance}元" : "获取额度失败";
    
    OnLogMessage($"📊 盘口额度:{quotaBalance}元");
```

---

### 4. VxMain端命令发送面板

**文件：** `BaiShengVx3Plus/Views/AutoBet/BetConfigManagerForm.cs`

#### ✅ UI组件（已在Designer中完成）

- ✅ 快捷按钮：投注、获取Cookie、获取盘口额度
- ✅ 命令输入框
- ✅ 发送按钮
- ✅ 执行结果显示区域

#### ✅ 命令发送逻辑

**4.1 发送命令按钮事件**
```csharp
private async void BtnSendCommand_Click(object? sender, EventArgs e)
{
    // 1. 解析命令
    var (cmdName, cmdParam) = ParseCommand(command);
    
    // 2. 通过AutoBetService发送Socket命令
    var result = await SendCommandToBrowserAsync(cmdName, cmdParam);
    
    // 3. 显示结果
    AppendCommandResult($"✅ 返回:成功={result.Success}");
    AppendCommandResult($"   消息:{result.Message}");
    
    if (result.Data != null)
    {
        var dataJson = JsonConvert.SerializeObject(result.Data, Formatting.Indented);
        AppendCommandResult($"   数据:{dataJson}");
    }
}
```

**4.2 命令解析（支持两种格式）**
```csharp
private (string cmdName, string cmdParam) ParseCommand(string command)
{
    // 带参数：投注(1234大10)
    if (openParen > 0 && closeParen > openParen)
    {
        var cmdName = trimmed.Substring(0, openParen).Trim();
        var cmdParam = trimmed.Substring(openParen + 1, closeParen - openParen - 1).Trim();
        return (cmdName, cmdParam);
    }
    // 无参数：获取Cookie
    else
    {
        return (trimmed, "");
    }
}
```

**4.3 发送命令到浏览器客户端**
```csharp
private async Task<CommandResponse> SendCommandToBrowserAsync(string cmdName, string cmdParam)
{
    var autoBetService = Program.ServiceProvider.GetService(typeof(Services.AutoBet.AutoBetService)) as Services.AutoBet.AutoBetService;
    
    switch (cmdName)
    {
        case "投注":
            var betResult = await autoBetService.SendBetCommandAsync(_selectedConfig.Id, "0", cmdParam);
            return new CommandResponse { Success = betResult.Success, ... };
            
        case "获取Cookie":
            var cookieResult = await SendSocketCommandAsync(_selectedConfig.Id, "获取Cookie", null);
            return cookieResult;
            
        case "获取盘口额度":
            var quotaResult = await SendSocketCommandAsync(_selectedConfig.Id, "获取盘口额度", null);
            return quotaResult;
            
        default:
            return new CommandResponse { Success = false, Message = $"未知命令:{cmdName}" };
    }
}
```

**4.4 Socket命令发送（通用方法）**
```csharp
private async Task<CommandResponse> SendSocketCommandAsync(int configId, string command, object? data)
{
    var autoBetService = ...;
    var browserClient = autoBetService.GetBrowserClient(configId);
    
    var result = await browserClient.SendCommandAsync(command, data);
    
    return new CommandResponse
    {
        Success = result.Success,
        Message = result.ErrorMessage ?? (result.Success ? "成功" : "失败"),
        Data = result.Data,
        ErrorMessage = result.ErrorMessage
    };
}
```

---

### 5. AutoBetService 扩展

**文件：** `BaiShengVx3Plus/Services/AutoBet/AutoBetService.cs`

#### ✅ 新增方法

```csharp
/// <summary>
/// 获取浏览器客户端（供命令面板使用）
/// </summary>
public BrowserClient? GetBrowserClient(int configId)
{
    return _browsers.TryGetValue(configId, out var client) ? client : null;
}
```

---

## 🎨 使用方法

### 1. Cookie自动回传

**流程：**
```
1. 启动自动投注 → BrowserClient启动
   ↓
2. 页面加载完成 → 自动登录
   ↓
3. 登录成功 → 获取Cookie
   ↓
4. Socket发送 → VxMain接收
   ↓
5. VxMain保存Cookie到配置
```

**日志示例：**
```
✅ 页面加载完成: https://www.yunding28.com
🔐 开始自动登录: testuser
✅ 自动登录成功！
📤 Cookie已回传到VxMain:共8个Cookie
```

---

### 2. 手动获取Cookie

**步骤：**
1. 打开配置管理 → 选择配置
2. 点击"获取Cookie"按钮（或手动输入"获取Cookie"）
3. 点击"发送"
4. 查看执行结果区域

**结果示例：**
```json
📤 发送命令:获取Cookie
   时间:2025-11-08 13:00:21.923
📝 命令:获取Cookie
✅ 返回:成功=True
   消息:获取成功,共8个Cookie
   数据:{
  "url": "https://www.yunding28.com",
  "cookies": {
    "sessionId": "abc123...",
    "token": "xyz789...",
    "userId": "12345"
  },
  "count": 8
}
```

---

### 3. 获取盘口额度

**步骤：**
1. 点击"获取盘口额度"按钮
2. 点击"发送"
3. 查看余额

**结果示例：**
```json
📤 发送命令:获取盘口额度
   时间:2025-11-08 13:05:45.123
📝 命令:获取盘口额度
✅ 返回:成功=True
   消息:盘口额度: 9856.50元
   数据:{
  "balance": 9856.50,
  "quota": 9856.50
}
```

---

### 4. 手动投注

**步骤：**
1. 点击"投注"按钮（或手动输入"投注(1234大10)"）
2. 修改投注内容（如需要）
3. 点击"发送"
4. 查看投注结果

**结果示例：**
```json
📤 发送命令:投注(12大10)
   时间:2025-11-08 13:10:30.456
📝 命令:投注
   参数:12大10
✅ 返回:成功=True
   消息:投注完成
   数据:{
  "success": true,
  "postStartTime": "2025-11-08 13:10:30.500",
  "postEndTime": "2025-11-08 13:10:30.625",
  "durationMs": 125,
  "orderNo": "ORD20251108131030456"
}
```

---

## 🔍 支持的命令列表

| 命令 | 格式 | 参数 | 说明 |
|-----|------|-----|-----|
| **投注** | `投注(参数)` | 12大10 | 发送投注命令 |
| **获取Cookie** | `获取Cookie` | 无 | 获取当前页面Cookie |
| **获取盘口额度** | `获取盘口额度` | 无 | 获取账户余额 |
| **获取余额** | `获取余额` | 无 | 同"获取盘口额度" |
| **登录** | `登录` | Socket发送 | 仅供内部使用 |
| **显示窗口** | `显示窗口` | 无 | 仅供内部使用 |
| **隐藏窗口** | `隐藏窗口` | 无 | 仅供内部使用 |
| **心跳检测** | `心跳检测` | 无 | 仅供内部使用 |
| **封盘通知** | `封盘通知` | Socket发送 | 仅供内部使用 |

---

## 📊 数据流图

### Cookie回传流程

```
BrowserClient                          VxMain
    |                                    |
    | 1. 页面加载完成                    |
    |--------------------------------->  |
    |                                    |
    | 2. 获取Cookie                      |
    | (CoreWebView2.CookieManager)       |
    |                                    |
    | 3. Socket发送                      |
    | {"type":"cookie_update"}           |
    |--------------------------------->  |
    |                                    |
    |                              4. 保存Cookie
    |                              到BetConfig
```

### 命令发送流程

```
VxMain UI                  AutoBetService              BrowserClient
    |                            |                            |
    | 1. 输入命令"获取Cookie"    |                            |
    |---------------------------->                            |
    |                            |                            |
    |                      2. 获取BrowserClient              |
    |                            |                            |
    |                      3. SendCommandAsync               |
    |                            |--------------------------->|
    |                            |                            |
    |                            |                      4. 执行命令
    |                            |                      (获取Cookie)
    |                            |                            |
    |                            |    5. 返回结果              |
    |                            |<---------------------------|
    |                            |                            |
    |    6. 显示结果             |                            |
    |<----------------------------                            |
```

---

## ✅ 编译状态

### BsBrowserClient
- ✅ 编译成功（0个错误，2个警告）

### BaiShengVx3Plus
- ✅ 语法检查通过（0个错误）
- ⚠️ 文件锁定（BsBrowserClient进程运行中，无法复制文件）

**解决方案：**
- 关闭所有BsBrowserClient进程后重新编译
- 或者直接运行测试（文件已在运行目录）

---

## 🎯 测试清单

### Cookie功能测试
- [ ] 启动浏览器后自动获取Cookie
- [ ] 登录成功后Cookie立即回传
- [ ] 页面跳转后Cookie更新
- [ ] VxMain正确接收并保存Cookie
- [ ] 手动发送"获取Cookie"命令成功

### 命令功能测试
- [ ] 快捷按钮正确填充命令
- [ ] 手动输入命令可以解析
- [ ] "获取Cookie"命令执行成功
- [ ] "获取盘口额度"命令执行成功
- [ ] "投注"命令执行成功
- [ ] 执行结果正确显示
- [ ] JSON格式化显示正确

### 异常处理测试
- [ ] WebView2未初始化时提示正确
- [ ] 浏览器未连接时提示正确
- [ ] 网络异常时错误提示正确
- [ ] 命令格式错误时提示正确

---

## 📝 注意事项

### 1. Cookie安全
- Cookie包含敏感信息，应加密存储
- Socket通信应使用SSL/TLS
- 不要在日志中明文输出Cookie

### 2. 命令权限
- 投注命令应有权限验证
- 敏感操作应二次确认
- 防止命令注入攻击

### 3. 性能优化
- Cookie回传避免频繁触发（使用防抖）
- 大量Cookie应分批发送
- Socket通信应设置超时

### 4. 错误处理
- 所有异常都应捕获并记录
- 用户友好的错误提示
- 详细的日志便于排查

---

## 🚀 后续优化方向

### 1. Cookie管理增强
```csharp
// 支持Cookie过滤（只回传关键Cookie）
var importantCookies = cookies.Where(c => 
    c.Name == "sessionId" || 
    c.Name == "token" || 
    c.Name == "userId"
).ToList();

// 支持Cookie加密
var encryptedCookies = EncryptCookies(cookieDict);

// 支持Cookie持久化到数据库
await SaveCookiesToDatabase(configId, cookieDict);
```

### 2. 命令历史记录
```csharp
// 保存命令历史
var commandHistory = new List<CommandRecord>();

// 支持历史命令快速填充
txtCommand.AutoCompleteSource = AutoCompleteSource.CustomSource;
txtCommand.AutoCompleteCustomSource.AddRange(commandHistory.ToArray());
```

### 3. 批量命令执行
```csharp
// 支持多个命令批量执行
var commands = new[]
{
    "获取Cookie",
    "获取盘口额度",
    "投注(1大10)"
};

foreach (var cmd in commands)
{
    await ExecuteCommandAsync(cmd);
}
```

### 4. 命令调度
```csharp
// 支持定时执行命令
var scheduler = new CommandScheduler();
scheduler.Schedule("获取盘口额度", TimeSpan.FromMinutes(5));

// 支持条件触发命令
scheduler.When(balance < 1000)
    .Execute("通知余额不足");
```

---

**实施完成时间：** 2025-11-08

**实施状态：** ✅ 完成

**测试状态：** ⏳ 待测试（需要关闭运行中的BsBrowserClient进程后重新编译）

---

**现在可以使用命令面板手动控制浏览器客户端了！** 🎉

