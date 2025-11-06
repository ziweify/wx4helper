# 🎮 开奖系统使用指南

## 📖 快速开始

### 1. 启动应用
```
1. 编译项目 (Ctrl+Shift+B)
2. 运行 BaiShengVx3Plus
3. 登录账号
```

### 2. 连接微信
```
1. 点击"连接"按钮
2. 等待微信启动和注入
3. 等待 UserInfo 显示
```

### 3. 绑定群组
```
1. 在联系人列表中选择一个群组
2. 点击"绑定"按钮
3. 等待群成员加载到会员表
```

### 4. 测试下注
```
在微信群中发送消息:
- "123大100" (简单下注)
- "1大50,2小60" (多个下注)
- "123大4小5单龙100" (组合下注)

查看:
- 应用日志中的处理记录
- 会员余额是否扣除
- 订单表中是否新增订单
- 微信群中的回复消息
```

### 5. 测试开奖结算
```
方式1: 等待自动开奖 (如果 API 可用)
- 定时器会自动获取开奖数据
- 自动触发结算
- 查看日志和订单表

方式2: 手动录入开奖 (如果 API 不可用)
- TODO: 实现开奖数据手动录入 UI
- 或直接在数据库中插入开奖数据
```

---

## 💬 支持的下注格式

### 基础格式
```
车号 + 玩法 + 金额
```

### 示例

| 格式 | 说明 | 解析结果 |
|------|------|----------|
| `123大100` | P1、P2、P3 都下大，各100 | 3个下注项 |
| `1大50` | P1 下大50 | 1个下注项 |
| `总和大100` | 总和下大100 | 1个下注项 |
| `龙100` | 龙100 | 1个下注项 |
| `1大50,2小60` | P1大50，P2小60 | 2个下注项 |
| `123大4小5单龙100` | P1/2/3大，P4小，P5单，龙，各100 | 7个下注项 |
| `一二三大100` | P1/2/3大，各100 (中文数字) | 3个下注项 |

### 支持的玩法

| 玩法 | 说明 |
|------|------|
| `大` | 大于等于41 |
| `小` | 小于等于40 |
| `单` | 奇数 |
| `双` | 偶数 |
| `尾大` | 个位数 >= 5 |
| `尾小` | 个位数 < 5 |
| `合单` | 个位+十位 为奇数 |
| `合双` | 个位+十位 为偶数 |
| `龙` | P1 > P5 |
| `虎` | P1 < P5 |

---

## 🔧 配置说明

### 游戏配置
编辑 `BinggoGameSettings` (在 `Program.cs` 中注册)：

```csharp
var settings = new BinggoGameSettings
{
    // 赔率
    Odds = new Dictionary<string, float>
    {
        { "大", 1.95f },
        { "小", 1.95f },
        { "单", 1.95f },
        { "双", 1.95f },
        { "龙", 1.95f },
        { "虎", 1.95f }
    },
    
    // 限额
    MinBet = 1.0f,          // 最小单注
    MaxBet = 10000.0f,      // 最大单注
    MaxBetPerIssue = 50000.0f, // 单期最大总投注
    
    // 封盘
    SealSecondsAhead = 30,  // 提前30秒封盘
    
    // 回复消息
    ReplySuccess = "已进仓！",
    ReplyFailed = "客官我有点不明白！",
    ReplyInsufficientBalance = "客官你的荷包是否不足！",
    ReplySealed = "已封盘，请等待下期！"
};
```

### WebAPI 配置
编辑 `BsWebApiClient.cs`:

```csharp
private string _baseUrl = "http://8.134.71.102:789"; // API 地址
```

---

## 📊 数据库说明

### 数据库文件
```
Data/
├── logs.db                    // 日志数据库 (公用)
├── business.db                // 默认业务数据库 (空)
└── business_{wxid}.db         // 微信专属业务数据库
```

### 表结构

#### V2Member (会员表)
```sql
CREATE TABLE V2Member (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Wxid TEXT,
    Nickname TEXT,
    Account TEXT,
    DisplayName TEXT,
    Balance REAL,
    State INTEGER,  -- MemberState enum
    GroupWxId TEXT,
    CreatedAt DATETIME
);
```

#### V2MemberOrder (订单表)
```sql
CREATE TABLE V2MemberOrder (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Wxid TEXT,
    Nickname TEXT,
    GroupWxId TEXT,
    IssueId INTEGER,
    BetContent TEXT,  -- "1大100,2小50"
    BetAmount REAL,
    Profit REAL,
    IsSettled INTEGER,  -- 0=未结算, 1=已结算
    CreatedAt DATETIME
);
```

#### BinggoLotteryData (开奖数据表)
```sql
CREATE TABLE BinggoLotteryData (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    IssueId INTEGER UNIQUE,
    NumbersString TEXT,  -- "1,2,3,4,5"
    IssueStartTime DATETIME,
    OpenTime DATETIME,
    IsOpened INTEGER,  -- 0=未开奖, 1=已开奖
    CreatedAt DATETIME
);
```

---

## 🐛 常见问题

### 1. 下注消息没有反应
**原因**:
- 消息格式不正确
- 封盘状态
- 当前期号未初始化

**解决**:
- 查看日志，检查解析错误
- 确认当前状态不是"封盘中"
- 确认开奖服务已启动

### 2. 余额没有扣除
**原因**:
- 会员是"托"或"管理"角色 (不扣除余额)
- 数据库未正确保存

**解决**:
- 查看会员状态
- 查看日志中的保存记录

### 3. 开奖后没有自动结算
**原因**:
- 开奖服务未启动
- 事件订阅失败
- 订单期号与开奖期号不匹配

**解决**:
- 查看日志: "🎮 初始化炳狗服务..."
- 查看日志: "🎲 开奖: xxx"
- 查看日志: "✅ 结算完成: xxx 单"

### 4. API 无法连接
**原因**:
- API 地址错误
- 网络问题
- API 服务未启动

**解决**:
- 检查 `BsWebApiClient` 中的 `_baseUrl`
- 检查网络连接
- 联系 API 提供方

---

## 📝 日志查看

### 查看实时日志
```
1. 点击"日志"按钮
2. 查看实时日志窗口
```

### 日志级别
- `Info`: 正常信息
- `Warning`: 警告信息
- `Error`: 错误信息

### 关键日志
```
🎮 初始化炳狗服务...
✅ 炳狗服务初始化完成
📨 收到可能的下注消息: xxx
✅ 下注成功: xxx
🎲 开奖: xxx
✅ 结算完成: xxx 单
```

---

## 🎯 开发建议

### 添加 UI 控件
1. 创建 `UcBinggoDataCur.cs` (当前期控件)
2. 创建 `UcBinggoDataLast.cs` (上期控件)
3. 在 `VxMain.Designer.cs` 中添加控件
4. 在 `VxMain.cs` 中绑定数据

### 添加开奖数据表
1. 在 `VxMain.Designer.cs` 中添加 `dgvLotteryData`
2. 绑定到 `_lotteryDataBindingList`
3. 配置列显示

### 添加自动通知
```csharp
// 在 OnLotteryOpened 中
if (_binggoSettings.AutoSendSettlementNotice)
{
    await SendWeChatMessageAsync(summary);
}
```

---

## 📞 技术支持

如有问题，请查看：
1. `0-资料/开奖系统实现完成-100percent.md` (完整文档)
2. `0-资料/开奖数据UI交互设计.md` (UI 设计)
3. `0-资料/批次1-下注核心完成.md` (下注解析)
4. `0-资料/批次2-订单服务完成.md` (订单服务)

---

**祝使用愉快！** 🎉

