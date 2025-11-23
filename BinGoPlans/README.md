# 宾果数据统计系统

## 概述

这是一个现代化的宾果数据统计系统，用于统计和分析宾果开奖数据。系统支持多种统计方式，包括路珠、走势图、连续统计等。

## 功能特性

### 1. 数据模型
- **标准化数据模型**：`BinggoLotteryData` 位于 `BaiShengVx3Plus.Shared` 项目中，作为标准数据源
- 支持多个位置（P1-P5, 总和）
- 支持多种玩法：大小、单双、尾大小、合单双、龙虎

### 2. 统计功能

#### 路珠统计
- 类似百家乐的路珠绘制
- 小方格绘制，不同颜色表示不同结果
- 支持所有位置和玩法

#### 走势图
- 10期走势
- 50期走势
- 100期走势
- 203期走势（日走势）
- 3日走势
- 一周走势
- 月走势
- 5日线（类似K线图，但比例固定）

#### 连续统计
- 统计连续出现次数：2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 11+
- 支持所有位置和玩法

#### 数量统计
- 统计各个结果的出现次数

## 数据接口

### 添加单条数据
```csharp
form1.AddLotteryData(issueId, "7,14,21,8,2", DateTime.Now);
```

### 批量添加数据
```csharp
var dataList = new List<(int issueId, string lotteryData, DateTime openTime)>
{
    (114062884, "7,14,21,8,2", DateTime.Now),
    (114062885, "10,15,22,9,3", DateTime.Now.AddMinutes(1))
};
form1.AddLotteryDataRange(dataList);
```

### 从数据库加载
- 点击"加载数据"按钮
- 选择SQLite数据库文件
- 数据库表结构：
  ```sql
  CREATE TABLE BinggoLotteryData (
      Id INTEGER PRIMARY KEY AUTOINCREMENT,
      IssueId INTEGER,
      LotteryData TEXT,
      OpenTime TEXT
  );
  ```

## 项目结构

```
BinGoPlans/
├── Controls/              # 自定义控件
│   ├── RoadBeadControl.cs      # 路珠绘制控件
│   ├── TrendChartControl.cs    # 走势图控件
│   └── ConsecutiveStatsControl.cs # 连续统计控件
├── Services/              # 服务类
│   └── DataService.cs          # 数据服务（数据入口）
├── Form1.cs               # 主界面
└── README.md              # 说明文档

BaiShengVx3Plus.Shared/
├── Models/
│   └── Games/
│       └── Binggo/
│           ├── BinggoLotteryData.cs  # 标准数据模型
│           ├── LotteryNumber.cs     # 号码模型
│           ├── BallPosition.cs      # 位置枚举
│           ├── GameType.cs          # 玩法类型枚举
│           └── Statistics/          # 统计相关模型
│               ├── GamePlayType.cs
│               ├── PlayResult.cs
│               ├── PositionPlayResult.cs
│               ├── TrendDataPoint.cs
│               └── ConsecutiveStats.cs
└── Services/
    └── BinggoStatisticsService.cs   # 统计服务
```

## 使用方法

1. **启动程序**
   - 运行 `BinGoPlans.exe`

2. **加载数据**
   - 方式1：点击"加载数据"按钮，选择SQLite数据库文件
   - 方式2：通过代码调用 `AddLotteryData` 或 `AddLotteryDataRange` 方法

3. **查看统计**
   - 选择位置（P1-P5, 总和）
   - 选择玩法（大小、单双、尾大小、合单双、龙虎）
   - 在不同选项卡中查看不同的统计结果

4. **查看走势**
   - 切换到"走势图"选项卡
   - 选择走势周期（10期、50期、100期等）
   - 查看走势图

## 注意事项

1. **数据格式**：开奖数据格式为 "7,14,21,8,2"（5个数字，逗号分隔）
2. **时间格式**：开奖时间使用 `DateTime` 类型
3. **数据库**：目前支持SQLite数据库，表结构见上方
4. **数据顺序**：数据会按时间自动排序

## 扩展说明

系统设计为可扩展的架构：
- 数据模型在 `Shared` 项目中，可以被其他项目引用
- 统计服务独立，可以单独使用
- UI控件可复用
- 数据接口清晰，易于集成

## 后续开发

- [ ] 支持更多位置（P6等）
- [ ] 支持更多统计维度
- [ ] 数据导出功能
- [ ] 数据可视化增强
- [ ] 实时数据更新

