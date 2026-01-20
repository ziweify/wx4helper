# Bingo 数据控件实现说明

**创建时间**: 2025-12-23  
**功能**: 实现当前期和上期开奖数据显示控件，集成到微信助手页面

---

## 📋 实现内容

### 1. 创建的文件

#### Infrastructure/Helpers/BingoHelper.cs
**功能**: Bingo 游戏辅助类，提供期号计算和时间转换功能。

**核心方法**:
- `GetCurrentIssueId()`: 获取当前期号
- `GetIssueOpenTime()`: 计算期号的开奖时间
- `GetSecondsToSeal()`: 计算距离封盘的秒数
- `GetPreviousIssueId()`: 获取上一期期号
- `FormatCountdown()`: 格式化倒计时显示（MM:SS）

**算法基准**:
- 基准期号: `114000001` (2025-01-01 第1期)
- 基准时间戳: `1735686300` (2025-01-01 07:05:00)
- 每天期数: `203期`
- 每期间隔: `5分钟`

---

#### Views/Wechat/Controls/UcBingoDataCur.cs
**功能**: 当前期开奖数据显示控件。

**显示内容**:
1. **第一行**: 期号 + 开奖时间
2. **第二行**: 倒计时（大字体，居中）
3. **第三行**: 当前状态（开盘中/封盘中/开奖中等）

**UI 特性**:
- 尺寸: `239 × 85` 像素
- 背景色: 淡蓝色 `#F3F9FF`
- 倒计时颜色根据剩余时间动态变化：
  - **绿色** (`#009688`): 正常（>30秒）
  - **橙色** (`#FF9800`): 提示（10-30秒）
  - **红色** (`#F44336`): 警告（1-10秒）
  - **紫色** (`#9C27B0`): 开奖中（≤0秒）

**数据绑定**:
- 订阅 `ILotteryService` 的事件：
  - `IssueChanged`: 期号变更
  - `StatusChanged`: 状态变更
  - `CountdownTick`: 倒计时更新
- 实时更新 UI（线程安全）

---

#### Views/Wechat/Controls/UcBingoDataLast.cs
**功能**: 上期开奖数据显示控件。

**显示内容**:
1. **标题**: "上期开奖"
2. **期号和时间**: 上期期号 + 开奖时间
3. **号码球**: 6个号码球（P1-P5 + 总和）
   - **颜色**: 大=红色，小=绿色
   - **形状**: 单=圆形，双=方形（圆角矩形）
4. **统计信息**: 大小单双 + 龙虎 + 总和

**UI 特性**:
- 尺寸: `239 × 110` 像素
- 背景色: 淡黄色 `#FFF8E1`
- 号码球大小: `32 × 32` 像素
- 间距: `37` 像素

**开奖中动画**:
- 未开奖时，号码显示为 `✱` (星号)
- 背景色闪烁（金黄色 ↔ 亮黄色，0.5秒间隔）
- 状态显示: `🎲 开 奖 中 🎲`

**数据绑定**:
- 订阅 `ILotteryService` 的事件：
  - `IssueChanged`: 期号变更（显示上期期号和时间，号码为 ✱）
  - `LotteryOpened`: 开奖完成（显示实际号码）
- 自动从 API 加载历史数据

---

### 2. 修改的文件

#### Views/Wechat/WechatPage.cs
**修改内容**:
1. **新增字段**:
   ```csharp
   private WechatBingoGameService? _gameService;
   private UcBingoDataCur? _ucBingoDataCur;
   private UcBingoDataLast? _ucBingoDataLast;
   ```

2. **新增方法**:
   - `InitializeBingoDataControls()`: 初始化 Bingo 数据控件
   - `InitializeGameService()`: 初始化游戏服务并绑定到控件

3. **修改逻辑**:
   - 移除 `panelControl_OpenData` 中的原有 Label 控件
   - 添加 `UcBingoDataCur` 和 `UcBingoDataLast` 控件
   - 启动 `WechatBingoGameService` 并绑定到控件
   - 在 `FormClosing` 中停止游戏服务

---

#### 项目说明/项目结构.md
**修改内容**:
- 更新时间: `2025-12-23（添加 Bingo 数据控件和辅助类）`
- 新增目录:
  - `Infrastructure/Helpers/BingoHelper.cs`
  - `Views/Wechat/Controls/UcBingoDataCur.cs`
  - `Views/Wechat/Controls/UcBingoDataLast.cs`

---

## 🎨 UI 设计参考

本次实现完全参考 **BaiShengVx3Plus** 项目中的 `ucBinggoDataCur` 和 `ucBinggoDataLast` 控件，保持了以下设计特点：

1. **F5BotV2 风格**: 紧凑、简洁、信息密集
2. **视觉反馈**: 倒计时颜色变化、号码球颜色/形状区分
3. **动画效果**: 开奖中闪烁动画
4. **实时更新**: 自动订阅服务事件，UI 线程安全更新

---

## 🔄 数据流程

### 当前期控件 (UcBingoDataCur)

```
WechatBingoGameService
  ├─ IssueChanged 事件
  │   └─> 更新期号和开奖时间
  ├─ StatusChanged 事件
  │   └─> 更新状态文字和颜色
  └─ CountdownTick 事件
      └─> 更新倒计时和颜色
```

### 上期控件 (UcBingoDataLast)

```
WechatBingoGameService
  ├─ IssueChanged 事件
  │   └─> 显示上期期号、时间、号码（✱）
  ├─ LotteryOpened 事件
  │   └─> 显示实际号码和统计信息
  └─ GetRecentLotteryDataAsync()
      └─> 初始化时加载历史数据
```

---

## 🚀 使用方式

### 在 WechatPage 中集成

```csharp
// 1. 创建控件
_ucBingoDataCur = new UcBingoDataCur { Dock = DockStyle.Top };
_ucBingoDataLast = new UcBingoDataLast { Dock = DockStyle.Top };

// 2. 添加到容器
panelControl_OpenData.Controls.Add(_ucBingoDataCur);
panelControl_OpenData.Controls.Add(_ucBingoDataLast);

// 3. 创建游戏服务
_gameService = new WechatBingoGameService(null, _loggingService);

// 4. 绑定服务到控件
_ucBingoDataCur.SetLotteryService(_gameService);
_ucBingoDataLast.SetLotteryService(_gameService);

// 5. 启动服务
_gameService.StartAsync();
```

### 清理资源

```csharp
private void WechatPage_FormClosing(object? sender, FormClosingEventArgs e)
{
    // 停止游戏服务
    _gameService?.StopAsync();
    
    // 控件会自动取消事件订阅（在 Dispose 中）
}
```

---

## 📊 技术要点

### 1. 线程安全 UI 更新

所有 UI 更新都通过 `UpdateUIThreadSafe()` 方法确保在 UI 线程执行：

```csharp
private void UpdateUIThreadSafe(Action action)
{
    if (InvokeRequired)
    {
        BeginInvoke(action);
    }
    else
    {
        action();
    }
}
```

### 2. 自定义绘制号码球

使用 `Label.Paint` 事件自定义绘制：
- **单数**: 圆形背景（`FillEllipse`）
- **双数**: 圆角矩形背景（`FillPath` + `GraphicsPath`）

### 3. 闪烁动画实现

使用 `System.Windows.Forms.Timer` 实现 0.5 秒间隔的闪烁：

```csharp
_blinkTimer.Interval = 500;
_blinkTimer.Tick += (s, e) => {
    bool showBright = (_blinkCount % 2 == 0);
    // 切换背景色
};
```

### 4. 期号计算算法

基于基准时间戳和期号，精确计算任意期号的开奖时间：

```csharp
// 计算天数差
int days = (issueId - FIRST_ISSUE_ID) / ISSUES_PER_DAY;
// 计算当天第几期
int number = (issueId - FIRST_ISSUE_ID) % ISSUES_PER_DAY + 1;
// 计算开奖时间
var openTime = firstTime.AddDays(days).AddMinutes(MINUTES_PER_ISSUE * (number - 1));
```

---

## ✅ 测试要点

1. **期号计算准确性**: 验证 `BingoHelper` 计算的期号和时间是否正确
2. **倒计时更新**: 验证倒计时每秒更新，颜色变化正确
3. **期号切换**: 验证期号变更时，当前期和上期控件都正确更新
4. **开奖动画**: 验证未开奖时显示 ✱ 并闪烁
5. **号码显示**: 验证开奖后号码颜色/形状正确（大小单双）
6. **线程安全**: 验证从后台线程更新 UI 不会抛出异常
7. **资源释放**: 验证关闭页面后定时器停止，事件取消订阅

---

## 🔮 后续优化

1. **性能优化**:
   - 号码球绘制可以缓存 `GraphicsPath` 避免重复创建
   - 闪烁动画可以使用 `DoubleBuffering` 减少闪烁

2. **功能扩展**:
   - 支持点击号码球查看详细信息
   - 支持配置倒计时警告阈值（10秒、30秒）
   - 支持自定义颜色主题

3. **错误处理**:
   - 添加网络请求失败的提示
   - 添加数据解析异常的处理

---

## 📚 参考资料

- **原项目**: `BaiShengVx3Plus/UserControls/UcBinggoDataCur.cs`
- **原项目**: `BaiShengVx3Plus/UserControls/UcBinggoDataLast.cs`
- **辅助类**: `BaiShengVx3Plus/Helpers/BinggoHelper.cs`
- **设计风格**: F5BotV2 简洁风格

---

**🎉 实现完成！** 现在微信助手页面已经可以实时显示当前期和上期的开奖数据了！

