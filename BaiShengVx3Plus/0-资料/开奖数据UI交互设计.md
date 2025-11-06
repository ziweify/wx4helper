# 🎨 开奖数据 UI 交互设计

## 📋 设计目标

1. ✅ **线程安全**：开奖服务定时器更新数据时，不阻塞 UI 线程
2. ✅ **双向绑定**：DataGridView 与 BindingList 双向绑定，自动刷新
3. ✅ **自动保存**：任何数据变更（包括手动输入）自动保存到数据库
4. ✅ **手动输入**：支持用户在 UI 上手动添加/修改开奖数据
5. ✅ **缓存优先**：显示本地数据，定时从网络同步

---

## 🏗️ 架构设计

### 数据流向

```
┌─────────────────────────────────────────────────────────────────┐
│  定时器线程 (BinggoLotteryService)                               │
│  ├─ 每秒获取 API 数据                                            │
│  ├─ 检测到新开奖数据                                             │
│  └─ 调用 AddOrUpdate()                                          │
└─────────────────────┬───────────────────────────────────────────┘
                      ↓
┌─────────────────────────────────────────────────────────────────┐
│  BinggoLotteryDataBindingList (线程安全)                         │
│  ├─ lock (_lock) { 添加/更新数据 }                              │
│  ├─ 保存到 SQLite 数据库                                        │
│  ├─ ResetBindings() 触发 UI 刷新                                │
│  └─ PropertyChanged 事件监听（手动修改时自动保存）               │
└─────────────────────┬───────────────────────────────────────────┘
                      ↓
┌─────────────────────────────────────────────────────────────────┐
│  UI 线程 (DataGridView)                                         │
│  ├─ 自动显示最新数据                                            │
│  ├─ 用户可编辑单元格（手动输入开奖号码）                         │
│  └─ 编辑后自动触发 PropertyChanged → 自动保存                   │
└─────────────────────────────────────────────────────────────────┘
```

---

## 💻 代码实现

### 1. VxMain 中的初始化

```csharp
public partial class VxMain : Sunny.UI.UIForm
{
    private BinggoLotteryDataBindingList? _lotteryDataBindingList;
    private IBinggoLotteryService _lotteryService;
    
    private void InitializeBinggoLottery()
    {
        // 创建 BindingList
        _lotteryDataBindingList = new BinggoLotteryDataBindingList(_db, _logService);
        
        // 加载最近 100 期数据
        _lotteryDataBindingList.LoadFromDatabase(100);
        
        // 绑定到 DataGridView
        dgvLotteryData.DataSource = _lotteryDataBindingList;
        
        // 配置列显示
        ConfigureLotteryDataGridView();
        
        // 订阅开奖服务事件
        _lotteryService.LotteryOpened += OnLotteryOpened;
    }
    
    // 开奖事件处理
    private void OnLotteryOpened(object? sender, BinggoLotteryOpenedEventArgs e)
    {
        // 线程安全地更新 UI
        UpdateUIThreadSafe(() =>
        {
            _lotteryDataBindingList?.AddOrUpdate(e.LotteryData);
            
            // 更新当前期控件
            ucBinggoDataCur.SetLotteryData(e.LotteryData);
        });
    }
}
```

### 2. DataGridView 配置

```csharp
private void ConfigureLotteryDataGridView()
{
    // 基础设置
    dgvLotteryData.AutoGenerateColumns = false;
    dgvLotteryData.AllowUserToAddRows = true;   // ✅ 允许手动添加
    dgvLotteryData.AllowUserToDeleteRows = false; // ❌ 不允许删除
    dgvLotteryData.EditMode = DataGridViewEditMode.EditOnEnter;
    
    // 列配置
    dgvLotteryData.Columns.Clear();
    dgvLotteryData.Columns.Add(new DataGridViewTextBoxColumn
    {
        DataPropertyName = "IssueId",
        HeaderText = "期号",
        Width = 100,
        ReadOnly = false  // ✅ 可编辑
    });
    dgvLotteryData.Columns.Add(new DataGridViewTextBoxColumn
    {
        DataPropertyName = "NumbersString",
        HeaderText = "开奖号码 (格式:1,2,3,4,5)",
        Width = 150,
        ReadOnly = false  // ✅ 可编辑
    });
    dgvLotteryData.Columns.Add(new DataGridViewTextBoxColumn
    {
        DataPropertyName = "Sum",
        HeaderText = "总和",
        Width = 60,
        ReadOnly = true  // 自动计算
    });
    dgvLotteryData.Columns.Add(new DataGridViewTextBoxColumn
    {
        DataPropertyName = "BigSmall",
        HeaderText = "大小",
        Width = 50,
        ReadOnly = true
    });
    dgvLotteryData.Columns.Add(new DataGridViewTextBoxColumn
    {
        DataPropertyName = "OddEven",
        HeaderText = "单双",
        Width = 50,
        ReadOnly = true
    });
    dgvLotteryData.Columns.Add(new DataGridViewTextBoxColumn
    {
        DataPropertyName = "DragonTiger",
        HeaderText = "龙虎",
        Width = 50,
        ReadOnly = true
    });
    dgvLotteryData.Columns.Add(new DataGridViewTextBoxColumn
    {
        DataPropertyName = "OpenTime",
        HeaderText = "开奖时间",
        Width = 140,
        DefaultCellStyle = new DataGridViewCellStyle { Format = "yyyy-MM-dd HH:mm:ss" },
        ReadOnly = false
    });
    
    // 样式
    dgvLotteryData.RowHeadersVisible = false;
    dgvLotteryData.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
    dgvLotteryData.MultiSelect = false;
    
    // 交替行颜色
    dgvLotteryData.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
}
```

### 3. 手动输入功能

```csharp
// 添加"手动录入"按钮
private void btnManualInput_Click(object sender, EventArgs e)
{
    try
    {
        // 创建新数据
        var newData = new BinggoLotteryData
        {
            IssueId = 0,  // 用户需要输入
            NumbersString = "",
            IssueStartTime = DateTime.Now,
            OpenTime = DateTime.Now
        };
        
        // 添加到 BindingList（会自动显示在最后一行）
        _lotteryDataBindingList?.Add(newData);
        
        // 定位到新行并进入编辑模式
        dgvLotteryData.CurrentCell = dgvLotteryData.Rows[dgvLotteryData.Rows.Count - 1].Cells[0];
        dgvLotteryData.BeginEdit(true);
        
        UIMessageTip.ShowOk("请输入期号和开奖号码（格式：1,2,3,4,5）");
    }
    catch (Exception ex)
    {
        _logService.Error("VxMain", $"手动录入失败: {ex.Message}", ex);
        UIMessageBox.ShowError($"手动录入失败：{ex.Message}");
    }
}

// 添加"刷新数据"按钮
private void btnRefreshLottery_Click(object sender, EventArgs e)
{
    try
    {
        // 从网络刷新最近 10 期数据
        var dataList = await _lotteryService.GetRecentLotteryDataAsync(10);
        
        foreach (var data in dataList)
        {
            _lotteryDataBindingList?.AddOrUpdate(data);
        }
        
        UIMessageTip.ShowOk($"刷新成功，获取 {dataList.Count} 期数据");
    }
    catch (Exception ex)
    {
        _logService.Error("VxMain", $"刷新数据失败: {ex.Message}", ex);
        UIMessageBox.ShowError($"刷新失败：{ex.Message}");
    }
}

// 数据验证（用户输入后验证）
private void dgvLotteryData_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
{
    if (dgvLotteryData.Columns[e.ColumnIndex].DataPropertyName == "NumbersString")
    {
        var value = e.FormattedValue?.ToString();
        if (!string.IsNullOrEmpty(value))
        {
            // 验证格式：必须是 5 个数字，用逗号分隔
            var parts = value.Split(',');
            if (parts.Length != 5 || !parts.All(p => int.TryParse(p.Trim(), out _)))
            {
                e.Cancel = true;
                UIMessageTip.ShowError("开奖号码格式错误！\n请输入 5 个数字，用逗号分隔，例如：1,2,3,4,5");
            }
        }
    }
}
```

---

## 🎨 UI 布局建议

### VxMain.Designer.cs 中添加

```csharp
// 开奖数据面板
private Sunny.UI.UIPanel pnlLotteryData;
private Sunny.UI.UIDataGridView dgvLotteryData;
private Sunny.UI.UIButton btnManualInput;
private Sunny.UI.UIButton btnRefreshLottery;
private Sunny.UI.UILabel lblLotteryTitle;

private void InitializeComponent()
{
    // ... 其他控件初始化
    
    // 开奖数据面板
    pnlLotteryData = new Sunny.UI.UIPanel();
    pnlLotteryData.Text = "开奖数据";
    pnlLotteryData.Location = new Point(10, 450);
    pnlLotteryData.Size = new Size(960, 300);
    
    // 标题
    lblLotteryTitle = new Sunny.UI.UILabel();
    lblLotteryTitle.Text = "📊 开奖数据（最近 100 期）";
    lblLotteryTitle.Font = new Font("微软雅黑", 12F, FontStyle.Bold);
    lblLotteryTitle.Location = new Point(10, 10);
    lblLotteryTitle.Size = new Size(300, 30);
    
    // 手动录入按钮
    btnManualInput = new Sunny.UI.UIButton();
    btnManualInput.Text = "✏️ 手动录入";
    btnManualInput.Location = new Point(700, 10);
    btnManualInput.Size = new Size(120, 35);
    btnManualInput.Click += btnManualInput_Click;
    
    // 刷新按钮
    btnRefreshLottery = new Sunny.UI.UIButton();
    btnRefreshLottery.Text = "🔄 刷新数据";
    btnRefreshLottery.Location = new Point(830, 10);
    btnRefreshLottery.Size = new Size(120, 35);
    btnRefreshLottery.Click += btnRefreshLottery_Click;
    
    // DataGridView
    dgvLotteryData = new Sunny.UI.UIDataGridView();
    dgvLotteryData.Location = new Point(10, 50);
    dgvLotteryData.Size = new Size(940, 240);
    dgvLotteryData.CellValidating += dgvLotteryData_CellValidating;
    
    // 添加到面板
    pnlLotteryData.Controls.Add(lblLotteryTitle);
    pnlLotteryData.Controls.Add(btnManualInput);
    pnlLotteryData.Controls.Add(btnRefreshLottery);
    pnlLotteryData.Controls.Add(dgvLotteryData);
    
    // 添加到主窗体
    this.Controls.Add(pnlLotteryData);
}
```

---

## 🔒 线程安全要点

### 1. BindingList 内部使用 `lock`
```csharp
lock (_lock)
{
    // 所有数据操作都在锁内进行
    var existing = this.FirstOrDefault(d => d.IssueId == data.IssueId);
    // ...
}
```

### 2. UI 更新使用 `UpdateUIThreadSafe`
```csharp
private void UpdateUIThreadSafe(Action action)
{
    if (InvokeRequired)
    {
        Invoke(action);
    }
    else
    {
        action();
    }
}
```

### 3. 避免死锁
- ❌ 不要在 UI 线程中调用 `lock` 内的耗时操作
- ✅ 数据库操作尽可能快
- ✅ 网络请求在单独的线程/Task 中

---

## 📝 用户操作流程

### 正常流程（自动）
1. 开奖服务定时器每秒运行
2. 检测到新开奖数据
3. `BinggoLotteryDataBindingList.AddOrUpdate()`
4. 自动保存到数据库
5. DataGridView 自动刷新显示

### 手动录入流程
1. 用户点击"手动录入"按钮
2. 新增空行到 DataGridView
3. 用户输入期号和号码
4. 失去焦点时验证格式
5. `PropertyChanged` 事件触发
6. 自动保存到数据库
7. 自动计算大小单双龙虎

### 手动修改流程
1. 用户双击单元格
2. 修改期号或号码
3. 按回车或失去焦点
4. `PropertyChanged` 事件触发
5. 自动保存到数据库

---

## ✅ 优势

1. **零冲突**：线程安全的 `lock` 确保数据一致性
2. **实时性**：定时器 + 事件驱动，数据实时更新
3. **可靠性**：所有修改自动保存，不会丢失数据
4. **易用性**：用户可以像 Excel 一样直接编辑
5. **容错性**：网络断开时，仍可查看和编辑本地数据

---

## 🎯 与订单服务的集成

### 补单时自动获取开奖数据

```csharp
// 在 BinggoOrderService 中
public async Task<V2MemberOrder?> CreateManualOrderAsync(
    V2Member member, int issueId, string betContent, float amount)
{
    // 1. 先从 BindingList 查找（UI 缓存）
    var lotteryData = _lotteryDataBindingList?.GetByIssueId(issueId);
    
    // 2. 如果 UI 缓存没有，从服务查询（会查本地数据库 + 网络）
    if (lotteryData == null || !lotteryData.IsOpened)
    {
        lotteryData = await _lotteryService.GetLotteryDataAsync(issueId, forceRefresh: false);
    }
    
    // 3. 如果还是没有，提示用户手动录入
    if (lotteryData == null || !lotteryData.IsOpened)
    {
        throw new Exception($"期号 {issueId} 未开奖，请先在开奖页面手动录入开奖数据！");
    }
    
    // 4. 创建订单并结算
    // ...
}
```

---

**设计完成！现在开奖数据可以：**
- ✅ 自动从网络同步
- ✅ 显示在 DataGridView
- ✅ 用户手动编辑
- ✅ 自动保存到数据库
- ✅ 线程安全
- ✅ 支持补单查询

