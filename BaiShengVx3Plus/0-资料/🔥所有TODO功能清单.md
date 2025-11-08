# 🔥 所有TODO功能清单

**最后更新：** 2025-11-08  
**项目：** BaiShengVx3Plus (wx4helper)

---

## 📊 TODO统计总览

| 优先级 | 数量 | 状态 | 说明 |
|--------|------|------|------|
| 🔴 **高优先级** | 2 | ⏳ 待处理 | 影响核心功能 |
| 🟡 **中优先级** | 5 | ⏳ 待处理 | 增强用户体验 |
| 🟢 **低优先级** | 8 | ⏳ 待处理 | 优化和完善 |
| ⚪ **可选** | 2 | 💡 建议 | 额外增强 |
| **合计** | **17** | - | - |

---

## 🔴 高优先级TODO（影响核心功能）

### 1. 清理旧的订单拉取投注流程 ⚠️

**文件：** `BsBrowserClient/Form1.cs:712`

**问题描述：**
```csharp
// TODO: 需要实现订单合并逻辑，参考 F5BotV2
private async Task<(bool success, string message)> FetchOrdersAndBetAsync(string issueId)
{
    // 这是旧流程：通过HTTP拉取订单 → 合并 → 投注
    // 新流程：VxMain直接发送合并后的"投注"命令
}
```

**当前状态：**
- ✅ 新流程已实现（VxMain合并订单 → Socket发送"投注"命令）
- ⚠️ 旧流程（"封盘通知" → HTTP拉取订单）仍然存在
- ⚠️ 两套流程并存，容易混淆

**解决方案：**
```csharp
case "封盘通知":
    // 🔥 新方案：只做通知，不拉取订单
    var notifyData = command.Data as JObject;
    var issueId = notifyData?["issueId"]?.ToString() ?? "";
    var secondsRemaining = notifyData?["secondsRemaining"]?.ToObject<int>() ?? 0;
    
    OnLogMessage($"⏰ 封盘通知:期号{issueId} 剩余{secondsRemaining}秒");
    
    response.Success = true;
    response.Message = $"封盘通知已接收:期号{issueId}";
    break;
    
// 🗑️ 删除 FetchOrdersAndBetAsync 方法（672-730行）
```

**影响范围：**
- 删除约60行代码
- 移除HTTP订单拉取逻辑
- 简化流程，只保留"投注"命令

**预计工时：** 30分钟

---

### 2. 投注命令解析增强（自动获取期号）⭐

**文件：** `BaiShengVx3Plus/Views/AutoBet/BetConfigManagerForm.cs:651`

**问题描述：**
```csharp
case "投注":
    // TODO: 解析投注内容，生成BetRecord，发送投注命令
    // 这里需要当前期号，暂时使用0
    var betResult = await autoBetService.SendBetCommandAsync(_selectedConfig.Id, "0", cmdParam);
```

**当前状态：**
- ⚠️ 期号硬编码为"0"
- ⚠️ 不会生成BetRecord
- ⚠️ 无法追溯手动投注历史

**解决方案：**
```csharp
case "投注":
    // 1. 获取当前期号
    var lotteryService = Program.ServiceProvider.GetService(typeof(Contracts.Games.IBinggoLotteryService)) 
        as Contracts.Games.IBinggoLotteryService;
    var currentIssueId = lotteryService?.CurrentIssueId ?? 0;
    
    if (currentIssueId == 0)
    {
        return new CommandResponse 
        { 
            Success = false, 
            Message = "无法获取当前期号，请确保彩票服务正在运行" 
        };
    }
    
    // 2. 解析投注内容（支持多种格式）
    var standardContent = ParseBetContent(cmdParam); // "12大10" → "1大10,2大10"
    
    // 3. 生成BetRecord
    var betRecordService = Program.ServiceProvider.GetService(typeof(Services.AutoBet.BetRecordService)) 
        as Services.AutoBet.BetRecordService;
        
    var betRecord = new Models.AutoBet.BetRecord
    {
        ConfigId = _selectedConfig.Id,
        IssueId = currentIssueId,
        Source = Models.AutoBet.BetRecordSource.命令, // 手动命令
        OrderIds = "", // 手动投注无关联订单
        BetContentStandard = standardContent,
        TotalAmount = CalculateTotalAmount(standardContent),
        SendTime = DateTime.Now
    };
    
    betRecord = betRecordService.Create(betRecord);
    
    // 4. 发送投注命令
    var betResult = await autoBetService.SendBetCommandAsync(
        _selectedConfig.Id, 
        currentIssueId.ToString(), 
        standardContent
    );
    
    // 5. 更新BetRecord
    betRecord.Success = betResult.Success;
    betRecord.PostStartTime = betResult.PostStartTime;
    betRecord.PostEndTime = betResult.PostEndTime;
    betRecord.DurationMs = betResult.DurationMs;
    betRecord.Result = betResult.Result;
    betRecord.ErrorMessage = betResult.ErrorMessage;
    betRecord.OrderNo = betResult.OrderNo;
    betRecordService.Update(betRecord);
    
    return new CommandResponse
    {
        Success = betResult.Success,
        Message = betResult.ErrorMessage ?? "投注完成",
        Data = new 
        {
            betRecordId = betRecord.Id,
            issueId = currentIssueId,
            betResult
        },
        ErrorMessage = betResult.ErrorMessage
    };
```

**新增辅助方法：**
```csharp
/// <summary>
/// 解析投注内容："12大10" → "1大10,2大10"
/// </summary>
private string ParseBetContent(string input)
{
    // 支持多种格式
    // "12大10" → "1大10,2大10"
    // "1大10 2小20" → "1大10,2小20"
    // "1大10,2大20" → "1大10,2大20"（已经是标准格式）
    
    var items = new List<string>();
    
    // 按空格或逗号分割
    var parts = input.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
    
    foreach (var part in parts)
    {
        var trimmed = part.Trim();
        
        // 检查是否包含连续数字（如："123大20"）
        var match = System.Text.RegularExpressions.Regex.Match(trimmed, @"^(\d+)(大|小|单|双)(\d+)$");
        if (match.Success)
        {
            var numbers = match.Groups[1].Value; // "123"
            var type = match.Groups[2].Value;     // "大"
            var amount = match.Groups[3].Value;   // "20"
            
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

/// <summary>
/// 计算总金额："1大10,2大20" → 30
/// </summary>
private decimal CalculateTotalAmount(string standardContent)
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
```

**影响范围：**
- 新增约100行代码
- 完善手动投注功能
- 支持投注记录追溯

**预计工时：** 2小时

---

## 🟡 中优先级TODO（增强用户体验）

### 3. 投注记录查询UI 📊

**文件：** `BaiShengVx3Plus/Views/AutoBet/BetConfigManagerForm.cs:197`

**问题描述：**
```csharp
private void LoadBetRecords(int configId)
{
    // TODO: 从数据库加载投注记录
    // var records = _autoBetService.GetBetRecords(configId, startDate, endDate);
    // dgvRecords.DataSource = records;
    
    dgvRecords.DataSource = null; // 当前为空
}
```

**当前状态：**
- ✅ UI组件已存在（dgvRecords）
- ✅ 时间筛选控件已存在（dtpStartDate, dtpEndDate）
- ❌ 数据加载逻辑未实现

**解决方案：**

**1. 在AutoBetService添加查询方法：**
```csharp
// BaiShengVx3Plus/Services/AutoBet/AutoBetService.cs
public List<BetRecord> GetBetRecords(int configId, DateTime startDate, DateTime endDate)
{
    if (_db == null) return new List<BetRecord>();
    
    return _db.Table<BetRecord>()
        .Where(r => r.ConfigId == configId && 
                    r.CreateTime >= startDate && 
                    r.CreateTime <= endDate)
        .OrderByDescending(r => r.CreateTime)
        .ToList();
}
```

**2. 在BetConfigManagerForm实现加载：**
```csharp
private void LoadBetRecords(int configId)
{
    try
    {
        var startDate = dtpStartDate.Value.Date;
        var endDate = dtpEndDate.Value.Date.AddDays(1).AddSeconds(-1);
        
        var autoBetService = Program.ServiceProvider.GetService(typeof(Services.AutoBet.AutoBetService)) 
            as Services.AutoBet.AutoBetService;
            
        if (autoBetService == null)
        {
            _logService.Warning("BetConfigManager", "AutoBetService未初始化");
            return;
        }
        
        var records = autoBetService.GetBetRecords(configId, startDate, endDate);
        
        // 转换为显示模型
        var displayRecords = records.Select(r => new
        {
            r.Id,
            期号 = r.IssueId,
            来源 = r.Source.ToString(),
            投注内容 = r.BetContentStandard,
            总金额 = r.TotalAmount.ToString("F2"),
            发送时间 = r.SendTime.ToString("yyyy-MM-dd HH:mm:ss"),
            耗时ms = r.DurationMs?.ToString() ?? "-",
            成功 = r.Success?.ToString() ?? "等待中",
            订单号 = r.OrderNo ?? "-",
            错误信息 = r.ErrorMessage ?? "-"
        }).ToList();
        
        dgvRecords.DataSource = displayRecords;
        
        _logService.Info("BetConfigManager", $"加载投注记录:{records.Count}条");
    }
    catch (Exception ex)
    {
        _logService.Error("BetConfigManager", "加载投注记录失败", ex);
        UIMessageBox.ShowError($"加载投注记录失败:{ex.Message}");
    }
}
```

**影响范围：**
- 新增约50行代码
- 提供投注历史查询功能
- 便于审计和分析

**预计工时：** 1小时

---

### 4. 订单筛选功能 🔍

**文件：** `BaiShengVx3Plus/Views/VxMain.cs:1406`

**问题描述：**
```csharp
// TODO: 实现订单筛选逻辑
private void ApplyOrderFilter()
{
    // 按期号、状态、类型、会员等级筛选
}
```

**解决方案：**

**1. 添加筛选UI组件：**
```csharp
// 在VxMain.Designer.cs添加筛选面板
private UIComboBox cbxFilterStatus;    // 状态筛选
private UIComboBox cbxFilterType;      // 类型筛选
private UIComboBox cbxFilterMemberLevel; // 会员等级筛选⭐
private UITextBox txtFilterIssueId;    // 期号筛选
private UIButton btnApplyFilter;       // 应用筛选
private UIButton btnClearFilter;       // 清除筛选
```

**2. 实现筛选逻辑：**
```csharp
private void ApplyOrderFilter()
{
    try
    {
        if (_ordersBindingList == null) return;
        
        var filtered = _ordersBindingList.AsEnumerable();
        
        // 按状态筛选
        if (cbxFilterStatus.SelectedIndex > 0)
        {
            var status = (OrderStatus)cbxFilterStatus.SelectedIndex - 1;
            filtered = filtered.Where(o => o.OrderStatus == status);
        }
        
        // 按类型筛选
        if (cbxFilterType.SelectedIndex > 0)
        {
            var type = (OrderType)cbxFilterType.SelectedIndex - 1;
            filtered = filtered.Where(o => o.OrderType == type);
        }
        
        // 🔥 按会员等级筛选（新功能）
        if (cbxFilterMemberLevel.SelectedIndex > 0)
        {
            var level = (MemberState)cbxFilterMemberLevel.SelectedIndex + 4; // 从普会开始
            filtered = filtered.Where(o => o.MemberState == level);
        }
        
        // 按期号筛选
        if (!string.IsNullOrEmpty(txtFilterIssueId.Text))
        {
            var issueId = int.Parse(txtFilterIssueId.Text);
            filtered = filtered.Where(o => o.IssueId == issueId);
        }
        
        dgvOrders.DataSource = filtered.ToList();
        
        _logService.Info("VxMain", $"订单筛选完成:显示{filtered.Count()}条");
    }
    catch (Exception ex)
    {
        _logService.Error("VxMain", "订单筛选失败", ex);
    }
}
```

**影响范围：**
- UI：新增6个控件
- 代码：约80行
- 提升订单查找效率

**预计工时：** 2小时

---

### 5. 联系人数据加载 👥

**文件：** `BaiShengVx3Plus/Views/VxMain.cs:1390`

**问题描述：**
```csharp
private void dgvContacts_SelectionChanged(object sender, EventArgs e)
{
    // TODO: 根据选中的联系人，加载对应的会员和订单数据
}
```

**解决方案：**
```csharp
private void dgvContacts_SelectionChanged(object sender, EventArgs e)
{
    try
    {
        if (dgvContacts.SelectedRows.Count == 0) return;
        
        var selectedContact = dgvContacts.SelectedRows[0].DataBoundItem as ContactInfo;
        if (selectedContact == null) return;
        
        var groupWxid = selectedContact.Wxid;
        
        _logService.Info("VxMain", $"选中联系人:{selectedContact.Name} ({groupWxid})");
        
        // 1. 加载该群的会员
        LoadMembersByGroup(groupWxid);
        
        // 2. 加载该群的订单
        LoadOrdersByGroup(groupWxid);
        
        // 3. 更新统计信息
        UpdateStatisticsByGroup(groupWxid);
    }
    catch (Exception ex)
    {
        _logService.Error("VxMain", "加载联系人数据失败", ex);
    }
}

private void LoadMembersByGroup(string groupWxid)
{
    var members = _memberService.GetMembersByGroup(groupWxid);
    _membersBindingList.Clear();
    foreach (var member in members)
    {
        _membersBindingList.Add(member);
    }
    _logService.Info("VxMain", $"加载会员:{members.Count}个");
}

private void LoadOrdersByGroup(string groupWxid)
{
    var orders = _orderService.GetOrdersByGroup(groupWxid);
    _ordersBindingList.Clear();
    foreach (var order in orders)
    {
        _ordersBindingList.Add(order);
    }
    _logService.Info("VxMain", $"加载订单:{orders.Count}个");
}

private void UpdateStatisticsByGroup(string groupWxid)
{
    var stats = _orderService.GetStatisticsByGroup(groupWxid);
    lblTotalBet.Text = $"总注:{stats.TotalBet}";
    lblTotalProfit.Text = $"总盈:{stats.TotalProfit:F2}";
    // ...
}
```

**影响范围：**
- 新增约60行代码
- 支持按群查看数据
- 提升数据组织性

**预计工时：** 1.5小时

---

### 6. 扩展业务规则（蓝会大额多打） 💎

**文件：** `BaiShengVx3Plus/Services/AutoBet/AutoBetCoordinator.cs:153`

**问题描述：**
```csharp
//var blueMemberLargeOrders = pendingOrders.Where(o =>
//    o.MemberState == MemberState.蓝会 &&
//    o.AmountTotal > 500 &&
//    o.OrderType != OrderType.托
//).ToList();

//if (blueMemberLargeOrders.Any())
//{
//    _log.Info("AutoBet", $"📢 检测到{blueMemberLargeOrders.Count}个蓝会大额订单(>500元)");
//    // TODO: 多打到配置B的逻辑
//    // await DuplicateOrdersToConfigB(blueMemberLargeOrders);
//}
```

**解决方案：**

**1. 实现DuplicateOrdersToConfigB方法：**
```csharp
/// <summary>
/// 复制订单到配置B（用于蓝会大额多打）
/// </summary>
private async Task DuplicateOrdersToConfigB(List<V2MemberOrder> orders)
{
    try
    {
        // 1. 获取配置B
        var configB = _autoBetService.GetConfigs().FirstOrDefault(c => c.ConfigName == "配置B");
        if (configB == null || !configB.IsActive)
        {
            _log.Warning("AutoBet", "配置B不存在或未激活，跳过多打");
            return;
        }
        
        // 2. 合并订单
        var mergeResult = _orderMerger.Merge(orders);
        
        // 3. 创建投注记录
        var betRecord = new BetRecord
        {
            ConfigId = configB.Id,
            IssueId = orders.First().IssueId,
            Source = BetRecordSource.订单,
            OrderIds = string.Join(",", mergeResult.OrderIds),
            BetContentStandard = mergeResult.BetContentStandard,
            TotalAmount = mergeResult.TotalAmount,
            SendTime = DateTime.Now
        };
        
        betRecord = _betRecordService.Create(betRecord);
        
        // 4. 发送投注命令到配置B
        _log.Info("AutoBet", $"📤 多打到配置B:期号{orders.First().IssueId} 内容:{mergeResult.BetContentStandard}");
        
        _betQueueManager.EnqueueBet(betRecord.Id, async () =>
        {
            var result = await _autoBetService.SendBetCommandAsync(
                configB.Id,
                orders.First().IssueId.ToString(),
                mergeResult.BetContentStandard
            );
            
            _log.Info("AutoBet", $"✅ 配置B投注结果:成功={result.Success}");
            
            return result;
        });
    }
    catch (Exception ex)
    {
        _log.Error("AutoBet", "多打到配置B失败", ex);
    }
}
```

**2. 启用检测逻辑：**
```csharp
// 取消注释
var blueMemberLargeOrders = pendingOrders.Where(o =>
    o.MemberState == MemberState.蓝会 &&
    o.AmountTotal > 500 &&
    o.OrderType != OrderType.托
).ToList();

if (blueMemberLargeOrders.Any())
{
    _log.Info("AutoBet", $"📢 检测到{blueMemberLargeOrders.Count}个蓝会大额订单(>500元)");
    await DuplicateOrdersToConfigB(blueMemberLargeOrders);
}
```

**影响范围：**
- 新增约60行代码
- 实现差异化服务
- 提升VIP用户体验

**预计工时：** 1.5小时

---

### 7. 彩票状态UI更新 🎨

**文件：** `BaiShengVx3Plus/Views/VxMain.cs:570`

**问题描述：**
```csharp
private void LotteryService_StatusChanged(object? sender, BinggoStatusChangedEventArgs e)
{
    // TODO: 更新 UI 状态显示
}
```

**解决方案：**
```csharp
private void LotteryService_StatusChanged(object? sender, BinggoStatusChangedEventArgs e)
{
    if (InvokeRequired)
    {
        Invoke(() => LotteryService_StatusChanged(sender, e));
        return;
    }
    
    try
    {
        // 1. 更新状态文本
        lblLotteryStatus.Text = e.NewStatus.ToString();
        
        // 2. 更新状态颜色
        lblLotteryStatus.ForeColor = e.NewStatus switch
        {
            BinggoLotteryStatus.开盘中 => Color.Green,
            BinggoLotteryStatus.即将封盘 => Color.Orange,
            BinggoLotteryStatus.已封盘 => Color.Red,
            BinggoLotteryStatus.已开奖 => Color.Blue,
            BinggoLotteryStatus.休市中 => Color.Gray,
            _ => Color.Black
        };
        
        // 3. 更新期号
        lblCurrentIssue.Text = $"期号:{e.IssueId}";
        
        // 4. 更新倒计时
        if (e.SecondsRemaining.HasValue)
        {
            lblCountdown.Text = $"剩余:{e.SecondsRemaining.Value}秒";
        }
        else
        {
            lblCountdown.Text = "";
        }
        
        // 5. 更新状态提示
        lblStatusTip.Text = e.Message;
        
        _logService.Info("VxMain", $"🔄 状态变更:{e.NewStatus} - {e.Message}");
    }
    catch (Exception ex)
    {
        _logService.Error("VxMain", "更新UI状态失败", ex);
    }
}
```

**影响范围：**
- 约40行代码
- 提升用户体验
- 直观的状态反馈

**预计工时：** 1小时

---

## 🟢 低优先级TODO（优化和完善）

### 8. AutoBetCoordinator启动准备 🔧

**文件：** `BaiShengVx3Plus/Services/AutoBet/AutoBetCoordinator.cs:114`

**问题描述：**
```csharp
public void Start()
{
    if (_isAutoBetEnabled) return;
    
    // TODO: 可以在这里做一些准备工作
    
    _isAutoBetEnabled = true;
    _log.Info("AutoBet", "✅ 自动投注协调器已启动");
}
```

**解决方案：**
```csharp
public void Start()
{
    if (_isAutoBetEnabled) return;
    
    // 1. 检查必要服务是否就绪
    if (_lotteryService == null)
    {
        _log.Warning("AutoBet", "彩票服务未初始化");
        return;
    }
    
    if (_autoBetService == null)
    {
        _log.Warning("AutoBet", "自动投注服务未初始化");
        return;
    }
    
    // 2. 检查当前彩票状态
    var currentStatus = _lotteryService.GetCurrentStatus();
    _log.Info("AutoBet", $"当前彩票状态:{currentStatus}");
    
    // 3. 清理过期的待处理订单（可选）
    var oldOrders = _orderService.GetPendingOrders()
        .Where(o => o.IssueId < _lotteryService.CurrentIssueId - 10) // 10期前的订单
        .ToList();
        
    if (oldOrders.Any())
    {
        _log.Warning("AutoBet", $"发现{oldOrders.Count}个过期待处理订单，建议清理");
        // 可以选择自动标记为盘外或提示用户
    }
    
    _isAutoBetEnabled = true;
    _log.Info("AutoBet", "✅ 自动投注协调器已启动");
}
```

**影响范围：**
- 约30行代码
- 提升系统稳定性
- 防止异常状态

**预计工时：** 30分钟

---

### 9. 设置持久化 💾

**文件：** `BaiShengVx3Plus/Views/SettingsForm.cs:68`

**问题描述：**
```csharp
private void SaveSettings()
{
    // TODO: 保存到配置文件
}
```

**解决方案：**
```csharp
private void SaveSettings()
{
    try
    {
        var settings = new
        {
            WeixinHost = txtWeixinHost.Text,
            WeixinPort = (int)nudWeixinPort.Value,
            AutoBetHttpPort = (int)nudHttpPort.Value,
            LogLevel = cbxLogLevel.SelectedIndex,
            // ... 其他设置
        };
        
        var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
        var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
        
        File.WriteAllText(configPath, json);
        
        _logService.Info("Settings", "设置已保存到appsettings.json");
        UIMessageBox.ShowSuccess("设置已保存！");
    }
    catch (Exception ex)
    {
        _logService.Error("Settings", "保存设置失败", ex);
        UIMessageBox.ShowError($"保存设置失败:{ex.Message}");
    }
}

private void LoadSettings()
{
    try
    {
        var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
        
        if (!File.Exists(configPath))
        {
            _logService.Info("Settings", "配置文件不存在，使用默认设置");
            return;
        }
        
        var json = File.ReadAllText(configPath);
        var settings = JsonConvert.DeserializeObject<dynamic>(json);
        
        txtWeixinHost.Text = settings.WeixinHost ?? "127.0.0.1";
        nudWeixinPort.Value = settings.WeixinPort ?? 10086;
        nudHttpPort.Value = settings.AutoBetHttpPort ?? 8888;
        cbxLogLevel.SelectedIndex = settings.LogLevel ?? 0;
        
        _logService.Info("Settings", "设置已从appsettings.json加载");
    }
    catch (Exception ex)
    {
        _logService.Error("Settings", "加载设置失败", ex);
    }
}
```

**影响范围：**
- 新增约60行代码
- 设置可持久化
- 提升用户体验

**预计工时：** 1小时

---

### 10. 添加用户对话框 ➕

**文件：** `BaiShengVx3Plus/ViewModels/VxMainViewModel.cs:90`

**问题描述：**
```csharp
public void AddUser()
{
    // TODO: 打开添加用户对话框
}
```

**解决方案：**
```csharp
public void AddUser()
{
    try
    {
        // 创建添加用户对话框
        using var dialog = new AddMemberDialog(_currentGroupWxid);
        
        if (dialog.ShowDialog() == DialogResult.OK)
        {
            var newMember = new V2Member
            {
                Wxid = dialog.Wxid,
                Nickname = dialog.Nickname,
                Account = dialog.Account,
                GroupWxid = _currentGroupWxid,
                State = dialog.MemberState,
                Balance = dialog.InitialBalance,
                Notes = dialog.Notes
            };
            
            _memberService.CreateMember(newMember);
            
            _log.Info("VxMain", $"添加用户成功:{newMember.Nickname}");
            UIMessageBox.ShowSuccess($"用户 {newMember.Nickname} 已添加！");
            
            // 刷新用户列表
            LoadMembers();
        }
    }
    catch (Exception ex)
    {
        _log.Error("VxMain", "添加用户失败", ex);
        UIMessageBox.ShowError($"添加用户失败:{ex.Message}");
    }
}
```

**需要创建AddMemberDialog窗体：**
```csharp
public partial class AddMemberDialog : UIForm
{
    public string Wxid { get; private set; }
    public string Nickname { get; private set; }
    public string Account { get; private set; }
    public MemberState MemberState { get; private set; }
    public float InitialBalance { get; private set; }
    public string Notes { get; private set; }
    
    // UI控件和验证逻辑...
}
```

**影响范围：**
- 新增对话框窗体（约150行）
- 新增ViewModel代码（约30行）
- 支持手动添加会员

**预计工时：** 2小时

---

### 11. 期号验证增强 ✔️

**文件：** `BaiShengVx3Plus/Services/Games/Binggo/BinggoOrderValidator.cs:133`

**问题描述：**
```csharp
public (bool isValid, string errorMessage) ValidateBetContent(string betContent)
{
    // TODO: 可以根据当前期号验证
}
```

**解决方案：**
```csharp
public (bool isValid, string errorMessage) ValidateBetContent(
    string betContent, 
    int? issueId = null)
{
    try
    {
        // 1. 基本格式验证
        if (string.IsNullOrWhiteSpace(betContent))
        {
            return (false, "投注内容为空");
        }
        
        // 2. 期号验证（如果提供）
        if (issueId.HasValue)
        {
            var currentIssueId = _lotteryService.CurrentIssueId;
            
            // 不能投注过期期号
            if (issueId.Value < currentIssueId)
            {
                return (false, $"期号{issueId.Value}已过期，当前期号{currentIssueId}");
            }
            
            // 不能投注太远的未来期号
            if (issueId.Value > currentIssueId + 10)
            {
                return (false, $"期号{issueId.Value}太远，当前期号{currentIssueId}");
            }
            
            // 检查期号状态
            var status = _lotteryService.GetStatusByIssueId(issueId.Value);
            if (status == BinggoLotteryStatus.已封盘 || 
                status == BinggoLotteryStatus.已开奖)
            {
                return (false, $"期号{issueId.Value}已{status}，不能投注");
            }
        }
        
        // 3. 投注内容格式验证
        var match = Regex.Match(betContent, @"^(\d+)(大|小|单|双)(\d+)$");
        if (!match.Success)
        {
            return (false, "投注格式错误，正确格式：1大10");
        }
        
        var number = match.Groups[1].Value;
        var type = match.Groups[2].Value;
        var amount = decimal.Parse(match.Groups[3].Value);
        
        // 4. 号码范围验证
        if (number.Length > 1 && number.Any(c => c < '1' || c > '6'))
        {
            return (false, "号码必须在1-6之间");
        }
        
        // 5. 金额范围验证
        var minAmount = _gameSettings.MinBetAmount;
        var maxAmount = _gameSettings.MaxBetAmount;
        
        if (amount < minAmount)
        {
            return (false, $"投注金额不能小于{minAmount}元");
        }
        
        if (amount > maxAmount)
        {
            return (false, $"投注金额不能大于{maxAmount}元");
        }
        
        return (true, "");
    }
    catch (Exception ex)
    {
        return (false, $"验证异常:{ex.Message}");
    }
}
```

**影响范围：**
- 约70行代码
- 增强数据安全性
- 防止无效投注

**预计工时：** 1小时

---

### 12-15. 微信通知功能 📢

**文件：** `BaiShengVx3Plus/Views/VxMain.cs:550, 587`

#### 12. 结算通知
```csharp
// TODO: 可选 - 发送结算通知到微信群
private async Task SendSettlementNotification(int issueId)
{
    try
    {
        var message = $"🎉 期号{issueId}已开奖！\n" +
                      $"开奖号码:{...}\n" +
                      $"大小:{...} 单双:{...}\n" +
                      $"投注:{...}笔 中奖:{...}笔";
        
        await _weixinService.SendTextAsync(_currentGroupWxid, message);
    }
    catch (Exception ex)
    {
        _log.Error("VxMain", "发送结算通知失败", ex);
    }
}
```

#### 13. 开盘通知
```csharp
// TODO: 可选 - 发送开盘通知到微信群
private async Task SendOpenNotification(int issueId)
{
    try
    {
        var message = $"📢 新一期开盘啦！\n" +
                      $"期号:{issueId}\n" +
                      $"封盘时间:{...}\n" +
                      $"欢迎下注！";
        
        await _weixinService.SendTextAsync(_currentGroupWxid, message);
    }
    catch (Exception ex)
    {
        _log.Error("VxMain", "发送开盘通知失败", ex);
    }
}
```

**预计工时：** 各30分钟

---

### 14-15. 会员事件处理 👥

**文件：** `BaiShengVx3Plus/Services/Messages/Handlers/MemberEventHandler.cs:38, 80`

#### 14. 成员加入事件
```csharp
// TODO: 处理成员加入事件
private async Task HandleMemberJoin(string groupWxid, string wxid, string nickname)
{
    try
    {
        // 1. 检查是否已存在
        var existingMember = _memberService.GetMemberByWxid(wxid);
        
        if (existingMember != null)
        {
            _log.Info("MemberEvent", $"成员{nickname}重新加入");
            
            // 更新状态（如果是已退群，改回会员）
            if (existingMember.State == MemberState.已退群)
            {
                existingMember.State = MemberState.会员;
                _memberService.UpdateMember(existingMember);
            }
        }
        else
        {
            // 2. 自动创建新会员（默认为非会员状态）
            var newMember = new V2Member
            {
                Wxid = wxid,
                Nickname = nickname,
                GroupWxid = groupWxid,
                State = MemberState.非会员,
                Balance = 0,
                Notes = "自动创建"
            };
            
            _memberService.CreateMember(newMember);
            _log.Info("MemberEvent", $"自动创建新会员:{nickname}");
        }
        
        // 3. 发送欢迎消息（可选）
        await _weixinService.SendTextAsync(groupWxid, $"欢迎 @{nickname} 加入！");
    }
    catch (Exception ex)
    {
        _log.Error("MemberEvent", "处理成员加入失败", ex);
    }
}
```

#### 15. 成员退出事件
```csharp
// TODO: 处理成员退出事件
private async Task HandleMemberLeave(string groupWxid, string wxid, string nickname)
{
    try
    {
        var member = _memberService.GetMemberByWxid(wxid);
        
        if (member != null)
        {
            // 标记为已退群
            member.State = MemberState.已退群;
            member.Notes += $" [退群:{DateTime.Now:yyyy-MM-dd}]";
            _memberService.UpdateMember(member);
            
            _log.Info("MemberEvent", $"成员{nickname}已退群");
        }
    }
    catch (Exception ex)
    {
        _log.Error("MemberEvent", "处理成员退出失败", ex);
    }
}
```

**预计工时：** 各1小时

---

## ⚪ 可选TODO（建议增强）

### 16. 性能优化 - 订单查询分页 📄

**建议：**
- 订单数量超过1000时，列表加载缓慢
- 实现分页查询（每页100条）
- 添加"加载更多"按钮

**预计工时：** 2小时

---

### 17. 数据导出功能 📊

**建议：**
- 导出订单数据（Excel/CSV）
- 导出投注记录
- 导出统计报表

**预计工时：** 3小时

---

## 📊 总工时估算

| 优先级 | 任务数 | 预计工时 |
|--------|--------|----------|
| 🔴 高优先级 | 2 | 2.5小时 |
| 🟡 中优先级 | 5 | 7.5小时 |
| 🟢 低优先级 | 8 | 8小时 |
| ⚪ 可选 | 2 | 5小时 |
| **合计** | **17** | **23小时** |

---

## 🎯 推荐实施顺序

### 第一阶段（核心完善）- 3小时
1. ✅ 清理旧的订单拉取投注流程（30分钟）
2. ✅ 投注命令解析增强（2小时）
3. ✅ 投注记录查询UI（1小时）

### 第二阶段（用户体验）- 5小时
4. ✅ 订单筛选功能（2小时）
5. ✅ 联系人数据加载（1.5小时）
6. ✅ 扩展业务规则（1.5小时）

### 第三阶段（优化完善）- 5小时
7. ✅ 彩票状态UI更新（1小时）
8. ✅ 设置持久化（1小时）
9. ✅ 添加用户对话框（2小时）
10. ✅ 期号验证增强（1小时）

### 第四阶段（可选增强）- 按需实施
11-17. 微信通知、会员事件、性能优化等

---

## 📝 实施注意事项

1. **优先级原则**：先完成高优先级TODO，确保核心功能稳定
2. **测试先行**：每完成一个功能，立即测试，确保不影响现有功能
3. **文档同步**：及时更新文档，记录实施细节
4. **代码审查**：确保代码质量，避免引入新的TODO
5. **用户反馈**：收集用户意见，调整优先级

---

**文档完成！** 🎉

所有TODO已详细列出，包括：
- ✅ 问题描述
- ✅ 当前状态
- ✅ 解决方案（含代码）
- ✅ 影响范围
- ✅ 预计工时

**建议按推荐顺序实施，确保项目稳步推进！** 🚀

