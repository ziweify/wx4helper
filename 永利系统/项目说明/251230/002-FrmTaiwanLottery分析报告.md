# FrmTaiwanLottery 分析报告

## 📋 概览

**项目**: WinSSSPlan  
**文件**: FrmTaiwanLottery.cs / FrmTaiwanLottery.Designer.cs  
**用途**: 台湾宾果开奖数据采集系统  
**分析日期**: 2025-12-30

---

## 🎨 界面布局分析

### 整体结构

```
┌────────────────────────────────────────────────────────┐
│  FrmTaiwanLottery - BG数据采集                          │
├───────────────────┬────────────────────────────────────┤
│                   │                                    │
│  【任务池.进行中】│     【调试信息】                    │
│  dgv_lottery      │     tbx_debugOutput               │
│  begin            │     (RichTextBox)                 │
│  (上表格)         │                                    │
│                   │                                    │
├───────────────────┤                                    │
│                   │                                    │
│  【任务池.任务    │                                    │
│   结束】          │                                    │
│  dgv_lottery      │                                    │
│  over             │                                    │
│  (下表格)         │                                    │
│                   │                                    │
├───────────────────┴────────────────────────────────────┤
│  【提交地址.多行】                                      │
│  tbx_submitAddress (多行文本框)                        │
│  存储多个API投递地址                                   │
├───────────────────┬────────────────────────────────────┤
│  【测试】         │  【当前/下期期号信息】              │
│  - 通过期号得到   │  - 当期开奖期号: [___]              │
│    时间戳         │  - 当期开奖时间: [___]              │
│  - 提交测试       │  - 下期开奖期号: [___]              │
│  - 得到最新一期   │  - 下期开奖时间: [___]  【倒计时】  │
│    开奖结果       │  - 代理设置                         │
│  - 网机投递       │  - 开启自动任务按钮                 │
│                   │  - 手动投递特定期号数据             │
└───────────────────┴────────────────────────────────────┘
```

### 关键布局特点

1. **左侧：双表格结构 (上下布局)**
   - **上表格**: `dgv_lotterybegin` - 待采集/采集中的任务
   - **下表格**: `dgv_lotteryover` - 已完成的任务

2. **右侧：配置和调试区域**
   - **调试信息面板**: 实时显示采集日志
   - **当前/下期期号信息**: 显示采集状态
   - **提交地址配置**: 多行配置多个投递目标

---

## 📊 数据表格结构

### 1. 上表格 (dgv_lotterybegin) - 待采集任务

**数据源**: `BindingList<lotterybeginData>`

**列结构**:
| 列名 | 字段 | 类型 | 说明 |
|------|------|------|------|
| 期号 | issueId | int | 彩票期号 (如: 113004873) |
| 开奖号码 | opendata | string | 格式: "01,02,03,04,05" (前5个号码) |
| 计次 | acCount | int | 采集尝试次数 |
| 采集时间 | acTime | string | 最后采集时间 |

**用途**:
- 显示需要采集的期号列表
- 当期号采集成功后,移动到下表格
- 支持批量添加待采集任务(如:添加近10天数据)

### 2. 下表格 (dgv_lotteryover) - 已采集完成

**数据源**: `BindingList<lotterybeginData>` (另一个实例)

**列结构**: 与上表格相同

**用途**:
- 显示已成功采集的期号
- 存档已完成的任务
- 可查看历史采集记录

---

## 🔄 核心工作流程

### 流程图

```
┌─────────────────────┐
│ 1. 获取期号信息     │ ← btn_getIssueData
│   (当期/下期)       │
└──────────┬──────────┘
           ↓
┌─────────────────────┐
│ 2. 添加到上表格     │
│   (待采集任务池)    │
└──────────┬──────────┘
           ↓
┌─────────────────────┐
│ 3. 开启自动任务     │ ← btn_StartTask
│   (定时采集)        │
└──────────┬──────────┘
           ↓
┌─────────────────────┐
│ 4. 采集开奖数据     │
│   (从官网抓取)      │
└──────────┬──────────┘
           ↓
┌─────────────────────┐
│ 5. 解析开奖数据     │
│   (正则提取)        │
└──────────┬──────────┘
           ↓
┌─────────────────────┐
│ 6. 投递到多个API    │ ← 使用 submitAddress
│   (W168/WOLD/BOTER) │
└──────────┬──────────┘
           ↓
┌─────────────────────┐
│ 7. 更新表格状态     │
│   (移动到下表格)    │
└─────────────────────┘
```

### 详细说明

#### 1. 获取期号信息 (`getLotteryBaseData`)
```csharp
// 从台湾彩票官网获取当期和下期期号
// API: https://www.taiwanlottery.com.tw/...
// 解析返回的JSON,提取:
//   - 当期期号 (issueIdCur)
//   - 当期开奖时间 (issueOpentimeCur)
//   - 下期期号 (issueIdNext)
//   - 下期开奖时间 (issueOpentimeNext)
```

#### 2. 添加到任务池
```csharp
// 将期号添加到 lotteryBeginData (BindingList)
lotteryBeginData.Add(new lotterybeginData() { 
    issueId = issueId,
    opendata = "",  // 初始为空
    acCount = 0,
    acTime = ""
});
```

#### 3. 开启自动任务 (`beginTask`)
```csharp
// 启动定时器,每隔N秒:
//   1. 检查 lotteryBeginData 中的期号
//   2. 尝试采集开奖数据
//   3. 采集成功后投递到API
//   4. 移动到 lotteryOver
```

#### 4. 采集开奖数据 (`getLotteryResult`)
```csharp
// 方法 1: 抓取整天开奖数据
//   URL: https://www.taiwanlottery.com.tw/lotto/BingoBingo/OEHLStatistic.htm
//   返回: HTML页面 (需要解析)

// 方法 2: 获取最新一期数据 (btnGetLastbgData)
//   使用 CsQuery 解析 HTML
//   正则提取: "第(\\d+)期...([^#]+大小顺序)"
//   提取前5个号码
```

#### 5. 解析开奖数据
```csharp
// 正则表达式提取
string strRgx = "第(\\d+)期[^#]+開出順序([^#]+大小順序)";
var rgxMatch = Regex.Match(viewtext, strRgx);
string strIssue = rgxMatch.Groups[1].Value;      // 期号
string strOpenData = rgxMatch.Groups[2].Value;   // 开奖数据
string strOpen11 = Regex.Match(strOpenData, "(\\d+)").Groups[1].Value;

// 提取前5个号码
string strP1 = strOpen11.Substring(0, 2);  // 第1个球
string strP2 = strOpen11.Substring(2, 2);  // 第2个球
string strP3 = strOpen11.Substring(4, 2);  // 第3个球
string strP4 = strOpen11.Substring(6, 2);  // 第4个球
string strP5 = strOpen11.Substring(8, 2);  // 第5个球

// 组装数据
ldata.opendata = $"{strP1},{strP2},{strP3},{strP4},{strP5}";
```

#### 6. 投递到多个API (`PostData`)
```csharp
// 解析提交地址 (支持多个API)
// 格式: [标识]URL,
// 示例:
//   [w168]http://8.134.71.102/api/api/upload_result.do,
//   [boter]http://8.134.71.102:789/api/boter/uploadbg,
//   [wold]http://8.138.183.44/api/task/upload_twbgone?token=xxx,

string[] addresses = model.submitAddress.Split(',');
foreach (var addr in addresses) {
    var tag = Regex.Match(addr, @"\[+([^#\]]+)").Groups[1].Value;
    var url = Regex.Match(addr, @"\[+[^#\]]+.([^#]+)").Groups[1].Value;
    
    // 根据 tag 选择投递策略 (不同API格式不同)
    var site = PostFactory.Create(tag, url);
    var result = site.Post(ldata);
}
```

---

## 🗂️ 数据模型

### lotterybeginData (核心数据类)

```csharp
public class lotterybeginData
{
    [DisplayName("期号")]
    public int issueId { get; set; }              // 期号 (如: 113004873)

    [DisplayName("开奖号码")]
    public string opendata { get; set; }          // 开奖数据 "01,02,03,04,05"

    [DisplayName("计次")]
    public int acCount { get; set; }              // 采集尝试次数

    [DisplayName("采集时间")]
    public string acTime { get; set; }            // 采集时间 (字符串格式)
}
```

**说明**:
- `issueId`: 唯一标识一期彩票
- `opendata`: 只存储前5个号码 (台湾宾果有20个号码,但只需要前5个)
- `acCount`: 用于统计采集失败次数,超过阈值可能需要人工介入
- `acTime`: 记录采集时间,用于审计和调试

### TaiwanLotteryModel (配置模型)

```csharp
public class TaiwanLotteryModel : INotifyPropertyChanged
{
    public string issueIdCur { get; set; }        // 当期期号
    public string issueOpentimeCur { get; set; }  // 当期开奖时间
    public string issueIdNext { get; set; }       // 下期期号
    public string issueOpentimeNext { get; set; } // 下期开奖时间
    public bool isProxy { get; set; }             // 是否使用代理
    public string proxyIp { get; set; }           // 代理地址 (如: 127.0.0.1:7890)
    public string djs { get; set; }               // 倒计时
    public string submitAddress { get; set; }     // 提交地址(多行)
    public string debuginfo { get; set; }         // 调试信息
}
```

---

## 🎯 关键功能点

### 1. 双表格管理 (上下布局)

**优势**:
- **上表格**: 清晰显示待处理任务
- **下表格**: 历史记录存档
- **状态流转**: 从上到下的视觉流程

**实现要点**:
```csharp
// 数据绑定
dgv_lotterybegin.DataSource = lotteryBeginData;  // BindingList 自动更新
dgv_lotteryover.DataSource = lotteryOver;

// 移动数据 (采集完成后)
var completed = lotteryBeginData.FirstOrDefault(x => x.issueId == targetIssue);
if (completed != null) {
    lotteryBeginData.Remove(completed);   // 从上表格移除
    lotteryOver.Insert(0, completed);     // 添加到下表格顶部
}
```

### 2. 多API投递支持

**配置格式**:
```
[标识]URL,
[标识]URL,
...
```

**支持的投递目标**:
- `[w168]`: 机器人老版本API
- `[boter]`: 机器人新版本API
- `[wold]`: 蓝盘API
- `[netboter]`: 网络版本机器人API

**实现策略**:
```csharp
// PostFactory.Create 工厂模式
// 根据不同的标识,创建不同的投递策略类
// 每个策略类实现 Post 方法,适配不同的API格式
```

### 3. 代理支持

**用途**: 
- 绕过IP限制
- 提高采集成功率
- 支持境外数据源

**配置**:
```csharp
if (model.isProxy) {
    httpItem.ProxyIp = model.proxyIp;  // "127.0.0.1:7890"
}
```

### 4. 调试信息输出

**特点**:
- **颜色分级**: 普通(黑)/提醒(紫)/警告(橙)/错误(红)
- **自动清理**: 超过1500行自动清空
- **时间戳**: 每条日志带时间戳
- **自动滚动**: 始终显示最新日志

```csharp
public void OutPutDebugString(string msg, DebugLv lv)
{
    Color color = GetColorByLevel(lv);
    
    if (tbx_debugOutput.Lines.Count >= 1500) {
        tbx_debugOutput.Text = "";
    }
    
    tbx_debugOutput.SelectionColor = color;
    tbx_debugOutput.AppendText($"{DateTime.Now.ToLongTimeString()} {msg}\r\n");
    tbx_debugOutput.ScrollToCaret();
}
```

### 5. 手动投递特定期号

**用途**:
- 补采集历史数据
- 测试投递功能
- 手动修正错误数据

**界面**:
```
期号:      [113004873]
开奖数据:  [01,02,03,04,05]
          【投递特定期号数据】
```

**实现**:
```csharp
private void btnPostManual_Click(object sender, EventArgs e)
{
    string issueId = tbxIssueIdManual.Text;
    string[] datas = tbxOpenDataManual.Text.Split(',');
    
    if (datas.Length == 5) {
        this.PostData(new lotterybeginData() {
            issueId = Convert.ToInt32(issueId),
            opendata = $"{datas[0]},{datas[1]},{datas[2]},{datas[3]},{datas[4]}",
            acTime = DateTime.Now.ToString()
        });
    }
}
```

---

## 📐 表格样式配置

### 全局属性
```csharp
// 禁止用户调整大小
dgv.AllowUserToResizeColumns = false;
dgv.AllowUserToResizeRows = false;

// 隐藏行头
dgv.RowHeadersVisible = false;

// 表头居中
dgv.ColumnHeadersDefaultCellStyle.Alignment = 
    DataGridViewContentAlignment.MiddleCenter;

// 内容居中
dgv.RowsDefaultCellStyle.Alignment = 
    DataGridViewContentAlignment.MiddleCenter;

// 允许自定义表头颜色
dgv.EnableHeadersVisualStyles = false;
dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(128, 128, 128);
dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.DarkBlue;
dgv.ColumnHeadersDefaultCellStyle.Font = new Font("宋体", 9, FontStyle.Bold);
```

### 列宽配置
```csharp
// issueId 列
dgv.Columns["issueId"].Width = 60;

// opendata 列
dgv.Columns["opendata"].Width = 68;

// acCount 列
dgv.Columns["acCount"].Width = 28;

// acTime 列
dgv.Columns["acTime"].Width = 70;
```

---

## 🔧 技术栈

### HTTP请求
- **库**: HttpHelper (自定义)
- **功能**: 
  - 支持代理
  - 支持Cookie管理
  - 支持自定义Headers

### HTML解析
- **库**: CsQuery (类jQuery选择器)
- **用法**:
```csharp
var jquery = CQ.CreateDocument(html);
var viewdata = jquery["#right_overflow_hinet > div"];
var viewtext = viewdata.Text();
```

### 正则表达式
- **场景**: 提取开奖数据
- **模式**: `第(\d+)期[^#]+開出順序([^#]+大小順序)`

### 数据绑定
- **类型**: `BindingList<T>`
- **优势**: 自动更新UI

---

## 💡 设计亮点

### 1. 任务池设计
- **上表格** = 待办任务
- **下表格** = 已完成任务
- **清晰的状态流转**

### 2. 多目标投递
- 一次采集,多处投递
- 支持不同API格式
- 工厂模式扩展

### 3. 可视化调试
- 实时日志显示
- 颜色分级
- 自动清理

### 4. 灵活配置
- 多行提交地址配置
- 代理开关
- 手动投递功能

### 5. 倒计时提醒
- 显示下期开奖倒计时
- 自动刷新

---

## 🎨 UI设计规范

### 布局原则
1. **左侧**: 数据表格 (主要内容)
2. **右侧**: 配置和调试 (辅助功能)
3. **底部**: 提交地址和测试功能

### 颜色方案
- **当期数据**: 绿色 (`Color.ForestGreen`)
- **下期数据**: 红色 (`Color.Crimson`)
- **表头背景**: 灰色 (`Color.FromArgb(128, 128, 128)`)
- **表头文字**: 深蓝 (`Color.DarkBlue`)

### 面板使用
- **SkinCaptionPanel**: 带标题的面板容器
- **优势**: 清晰的功能分组

---

## 🔄 适配到永利系统的建议

### 1. 保留核心结构
- ✅ **双表格上下布局** (待采集/已采集)
- ✅ **右侧配置面板**
- ✅ **调试信息输出**

### 2. 使用 DevExpress 控件
- **DataGridView** → **GridControl**
- **RichTextBox** → **MemoEdit** 或保留 **RichTextBox**
- **Panel** → **PanelControl**
- **SkinCaptionPanel** → **GroupControl**

### 3. 架构优化
- **Model**: `DataCollectionTask` (任务模型)
- **Service**: `DataCollectionService` (采集服务)
- **ViewModel**: `DataCollectionViewModel` (MVVM)

### 4. 功能增强
- **任务队列管理**
- **重试机制**
- **失败通知**
- **采集统计**
- **历史查询**

---

## 📝 总结

FrmTaiwanLottery 是一个**成熟的数据采集系统**,具有以下特点:

1. **清晰的任务流转**: 上表格(待采集) → 下表格(已完成)
2. **多目标投递**: 支持同时投递到多个API
3. **可视化调试**: 实时日志,颜色分级
4. **灵活配置**: 代理,多行地址,手动投递
5. **用户友好**: 倒计时,自动刷新,表格样式

这个设计非常适合作为我们**永利系统数据采集页面**的参考蓝本。

---

**分析完成日期**: 2025-12-30  
**分析人**: AI Assistant

