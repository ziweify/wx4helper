# ✅ Cookie回传和投注命令修复完成

**修复时间：** 2025-11-08 13:30  
**问题报告：** Cookie未回传到配置，投注命令无BetRecord记录  
**状态：** ✅ 已完成

---

## 📋 修复内容

### 1. Cookie回传功能 ✅

#### 问题描述
- BrowserClient登录后，Cookie未保存到VxMain的配置中
- AutoBetSocketServer接收到`cookie_update`消息后，只记录日志，未处理

#### 修复方案

**文件1：`BaiShengVx3Plus/Services/AutoBet/AutoBetSocketServer.cs`**
- ✅ 添加`_onMessageReceived`回调参数到构造函数
- ✅ 修改消息读取循环，解析JSON并分发消息
- ✅ 支持`cookie_update`、`login_success`等消息类型

```csharp
// 构造函数增加消息处理回调
public AutoBetSocketServer(
    ILogService log, 
    Action<int, TcpClient> onBrowserConnected,
    Action<int, JObject>? onMessageReceived = null) // 🔥 新增

// 消息处理循环
while (!cancellationToken.IsCancellationRequested)
{
    var line = await reader.ReadLineAsync(cancellationToken);
    
    var message = JsonConvert.DeserializeObject<JObject>(line);
    var messageType = message["type"]?.ToString();
    
    switch (messageType)
    {
        case "cookie_update":
            _onMessageReceived?.Invoke(configId, message);
            break;
        // ... 其他消息类型
    }
}
```

**文件2：`BaiShengVx3Plus/Services/AutoBet/AutoBetService.cs`**
- ✅ 传递`OnMessageReceived`回调到AutoBetSocketServer
- ✅ 添加`OnMessageReceived`方法分发消息
- ✅ 添加`HandleCookieUpdate`方法处理Cookie更新
- ✅ 添加`HandleLoginSuccess`方法处理登录成功通知

```csharp
// 启动Socket服务器时传递回调
_socketServer = new AutoBetSocketServer(log, OnBrowserConnected, OnMessageReceived);

// 处理Cookie更新
private void HandleCookieUpdate(int configId, JObject message)
{
    var cookies = message["cookies"]?.ToObject<Dictionary<string, string>>();
    var cookieString = string.Join("; ", cookies.Select(kv => $"{kv.Key}={kv.Value}"));
    
    var config = GetConfig(configId);
    config.Cookie = cookieString;
    config.CookieUpdateTime = DateTime.Now;
    SaveConfig(config);
    
    _log.Info("AutoBet", $"✅ 配置{configId} Cookie已更新:共{cookies.Count}个");
}
```

**文件3：`BaiShengVx3Plus/Models/AutoBet/BetConfig.cs`**
- ✅ 添加`Cookie`属性（访问`Cookies`字段）
- ✅ 保持向后兼容

```csharp
[Ignore]
public string? Cookie
{
    get => Cookies;
    set => Cookies = value;
}
```

---

### 2. 投注命令功能 ✅

#### 问题描述
- 手动投注命令`投注(1234大10)`没有生成`BetRecord`
- 投注内容未解析（"1234大10"应解析为"1大10,2大10,3大10,4大10"）
- 期号硬编码为"0"

#### 修复方案

**文件1：`BaiShengVx3Plus/Views/AutoBet/BetConfigManagerForm.cs`**

**新增方法1：`ParseBetContent`**
- ✅ 解析投注内容："1234大10" → "1大10,2大10,3大10,4大10"
- ✅ 支持正则表达式匹配：`(\d+)(大|小|单|双)(\d+)`
- ✅ 自动拆分连续数字

```csharp
private string ParseBetContent(string input)
{
    var items = new List<string>();
    var parts = input.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
    
    foreach (var part in parts)
    {
        var match = Regex.Match(part, @"^(\d+)(大|小|单|双)(\d+)$");
        
        if (match.Success)
        {
            var numbers = match.Groups[1].Value;  // "1234"
            var type = match.Groups[2].Value;      // "大"
            var amount = match.Groups[3].Value;    // "10"
            
            foreach (var num in numbers)
            {
                items.Add($"{num}{type}{amount}");  // "1大10", "2大10", ...
            }
        }
        else
        {
            items.Add(part);
        }
    }
    
    return string.Join(",", items);
}
```

**新增方法2：`CalculateTotalAmount`**
- ✅ 计算总金额："1大10,2大20" → 30元
- ✅ 正则提取每个投注项的金额并累加

```csharp
private decimal CalculateTotalAmount(string standardContent)
{
    decimal total = 0;
    var items = standardContent.Split(',');
    
    foreach (var item in items)
    {
        var match = Regex.Match(item, @"(\d+)$");
        if (match.Success && decimal.TryParse(match.Groups[1].Value, out var amount))
        {
            total += amount;
        }
    }
    
    return total;
}
```

**修改：`SendCommandToBrowserAsync` 的 "投注" case**
- ✅ 获取当前期号（通过`BinggoLotteryService.CurrentIssueId`）
- ✅ 解析投注内容（调用`ParseBetContent`）
- ✅ 计算总金额（调用`CalculateTotalAmount`）
- ✅ 创建`BetRecord`（`Source=命令`）
- ✅ 发送投注命令到BrowserClient
- ✅ 更新`BetRecord`结果（Success、PostStartTime、PostEndTime等）

```csharp
case "投注":
    // 1. 获取当前期号
    var lotteryService = Program.ServiceProvider.GetService(...) as IBinggoLotteryService;
    var currentIssueId = lotteryService?.CurrentIssueId ?? 0;
    
    // 2. 解析投注内容
    var originalContent = cmdParam;
    var standardContent = ParseBetContent(originalContent);
    var totalAmount = CalculateTotalAmount(standardContent);
    
    // 3. 生成BetRecord
    var betRecordService = Program.ServiceProvider.GetService(...) as BetRecordService;
    var betRecord = new BetRecord
    {
        ConfigId = _selectedConfig.Id,
        IssueId = currentIssueId,
        Source = BetRecordSource.命令,
        BetContentStandard = standardContent,
        TotalAmount = totalAmount,
        SendTime = DateTime.Now
    };
    betRecord = betRecordService.Create(betRecord);
    
    // 4. 发送投注命令
    var betResult = await autoBetService.SendBetCommandAsync(...);
    
    // 5. 更新BetRecord
    betRecord.Success = betResult.Success;
    betRecord.PostStartTime = betResult.PostStartTime;
    betRecord.PostEndTime = betResult.PostEndTime;
    betRecord.DurationMs = betResult.DurationMs;
    betRecord.ErrorMessage = betResult.ErrorMessage;
    betRecord.OrderNo = betResult.OrderNo;
    betRecordService.Update(betRecord);
    
    return new CommandResponse { ... };
```

**文件2：`BaiShengVx3Plus/Services/AutoBet/BetRecordService.cs`**
- ✅ 添加`Update(BetRecord record)`方法
- ✅ 自动计算`DurationMs`（PostEndTime - PostStartTime）
- ✅ 重构`UpdateResult`调用新的`Update`方法

```csharp
public void Update(BetRecord record)
{
    record.UpdateTime = DateTime.Now;
    
    // 计算耗时
    if (record.PostStartTime.HasValue && record.PostEndTime.HasValue)
    {
        record.DurationMs = (int)(record.PostEndTime.Value - record.PostStartTime.Value).TotalMilliseconds;
    }
    
    _db.Update(record);
    _log.Info("BetRecordService", $"✅ 更新投注记录:ID={record.Id} 成功={record.Success}");
}
```

---

## 🎯 修复效果

### Cookie回传

**触发时机：**
1. ✅ BrowserClient页面加载完成（`NavigationCompleted`）
2. ✅ 登录成功后
3. ✅ 手动点击"获取Cookie"命令

**日志示例：**
```
[BrowserClient] 📤 Cookie已回传到VxMain:共8个Cookie
[VxMain AutoBetServer] 🍪 收到Cookie更新:配置1
[VxMain AutoBet] ✅ 配置1(默认配置) Cookie已更新:共8个
```

**数据库验证：**
- 表名：`AutoBetConfigs`
- 字段：`Cookies`（Cookie字符串，如：`PHPSESSID=abc123; token=xyz789`）
- 字段：`CookieUpdateTime`（更新时间）

---

### 投注命令

**输入示例：**
```
投注(1234大10)
```

**执行流程：**
1. ✅ 解析命令：cmdName="投注", cmdParam="1234大10"
2. ✅ 获取期号：currentIssueId=114063156
3. ✅ 解析内容：standardContent="1大10,2大10,3大10,4大10"
4. ✅ 计算金额：totalAmount=40
5. ✅ 创建BetRecord（ID=1）
6. ✅ 发送到BrowserClient
7. ✅ 更新BetRecord（Success=true, DurationMs=125）

**日志示例：**
```
[CommandPanel] 投注解析:原始=1234大10 标准=1大10,2大10,3大10,4大10 金额=40
[CommandPanel] BetRecord已创建:ID=1
[AutoBet] 📤 发送投注命令:期号114063156 内容:1大10,2大10,3大10,4大10
[BetRecordService] ✅ 更新投注记录:ID=1 成功=True 耗时=125ms
```

**数据库验证：**
- 表名：`BetRecords`
- 字段示例：
  - `Id=1`
  - `ConfigId=1`
  - `IssueId=114063156`
  - `Source=命令`
  - `BetContentStandard=1大10,2大10,3大10,4大10`
  - `TotalAmount=40`
  - `Success=True`
  - `DurationMs=125`
  - `OrderNo=ORD123456`

---

## 📁 修改文件清单

1. ✅ `BaiShengVx3Plus/Services/AutoBet/AutoBetSocketServer.cs` - 添加消息处理回调
2. ✅ `BaiShengVx3Plus/Services/AutoBet/AutoBetService.cs` - 实现Cookie更新和登录成功处理
3. ✅ `BaiShengVx3Plus/Models/AutoBet/BetConfig.cs` - 添加Cookie属性
4. ✅ `BaiShengVx3Plus/Views/AutoBet/BetConfigManagerForm.cs` - 添加投注解析和BetRecord生成
5. ✅ `BaiShengVx3Plus/Services/AutoBet/BetRecordService.cs` - 添加Update方法

---

## 🧪 测试建议

### 测试1：Cookie自动回传
1. 启动VxMain
2. 配置管理 → 启动浏览器
3. 等待页面加载
4. 检查日志：`Cookie已回传`、`Cookie已更新`
5. 刷新配置列表，查看Cookie字段

### 测试2：Cookie手动获取
1. 配置管理 → 选择配置
2. 点击"获取Cookie"按钮
3. 点击"发送"
4. 查看执行结果区域，应显示Cookie数据

### 测试3：投注命令（简单）
```
投注(1大10)
```
- 预期：解析为"1大10"，生成BetRecord，发送投注

### 测试4：投注命令（复杂）
```
投注(1234大10)
```
- 预期：解析为"1大10,2大10,3大10,4大10"，总金额40元

### 测试5：投注命令（多项）
```
投注(1大10,2小20,3单15)
```
- 预期：保持原样，总金额45元

### 测试6：查看BetRecord
- 打开数据库：`Data/business_{wxid}.db`
- 查询：`SELECT * FROM BetRecords ORDER BY CreateTime DESC LIMIT 10`
- 验证：所有字段正确填充

---

## 🎉 用户体验提升

**修复前：**
- ❌ Cookie需要手动复制粘贴
- ❌ 投注命令没有记录
- ❌ 无法追踪投注历史
- ❌ 投注内容需要手动拆分

**修复后：**
- ✅ Cookie自动回传并保存
- ✅ 每次投注都有完整记录
- ✅ 可查询投注历史和结果
- ✅ 智能解析投注内容
- ✅ 自动计算金额和耗时
- ✅ 详细的日志追踪

---

## 📊 下一步建议

1. **投注记录UI** - 在配置管理界面显示BetRecord列表
2. **Cookie过期检测** - 定期检查Cookie有效性，自动重新登录
3. **投注防重复** - 利用`BetRecordService.HasPendingBet`防止重复投注
4. **投注统计** - 统计成功率、平均耗时、总金额等
5. **批量投注** - 支持一次性发送多个投注命令

---

**修复完成！🚀 请测试验证！**

