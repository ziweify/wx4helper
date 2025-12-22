# Wechat 模块服务框架设计

## 📁 目录结构

```
永利系统/
├── Services/
│   └── Wechat/                    # 微信模块服务实现
│       ├── LotteryService.cs      # 开奖服务（框架）
│       ├── OrderService.cs        # 订单服务（框架）
│       └── WechatService.cs       # 微信服务（框架）
├── Contracts/
│   └── Wechat/                    # 微信模块服务接口
│       ├── ILotteryService.cs      # 开奖服务接口
│       ├── IOrderService.cs        # 订单服务接口
│       └── IWechatService.cs       # 微信服务接口
└── Models/
    └── Wechat/                    # 微信模块数据模型
        ├── LotteryStatus.cs        # 开奖状态枚举
        ├── WechatConnectionState.cs # 微信连接状态枚举
        ├── Member.cs                # 会员数据模型
        ├── Order.cs                 # 订单数据模型
        ├── Contact.cs               # 联系人数据模型
        ├── LotteryData.cs           # 开奖数据模型
        └── Events/                  # 事件参数类
            ├── LotteryStatusChangedEventArgs.cs
            ├── LotteryIssueChangedEventArgs.cs
            ├── LotteryCountdownEventArgs.cs
            ├── LotteryOpenedEventArgs.cs
            └── WechatConnectionStateChangedEventArgs.cs
```

## 🎯 服务设计

### 1. ILotteryService（开奖服务）

**功能**：
- ✅ 定时获取当前期号和状态
- ✅ 计算倒计时
- ✅ 触发状态变更事件（开奖中、封盘中、等待中、开盘中、即将封盘）
- ✅ 管理开奖数据（本地缓存优先）

**状态枚举**：
- `等待中` - 系统初始化或停止
- `开盘中` - 可以接受下注
- `即将封盘` - 倒计时小于设定秒数
- `封盘中` - 停止接受下注，等待开奖
- `开奖中` - 开奖数据处理中

**事件**：
- `IssueChanged` - 期号变更
- `StatusChanged` - 状态变更
- `CountdownTick` - 倒计时更新（每秒）
- `LotteryOpened` - 开奖数据到达

### 2. IOrderService（订单服务）

**功能**：
- ✅ 创建订单（微信下注）
- ✅ 补单（手动创建）
- ✅ 结算订单（批量+单个）
- ✅ 查询订单
- ✅ 限额验证

**订单状态枚举**：
- `待投注` - 订单已创建，等待投注
- `已投注` - 订单已投注
- `已结算` - 订单已结算
- `已取消` - 订单已取消

**订单类型枚举**：
- `微信下单` - 从微信消息创建
- `手动补单` - 手动创建
- `自动投注` - 自动投注系统创建

### 3. IWechatService（微信服务）

**功能**：
- ✅ 启动微信（完整流程：启动微信→注入DLL→连接Socket）
- ✅ 微信刷新（重新获取微信联系人等数据）
- ✅ 微信绑定（绑定群组）

**连接状态枚举**：
- `Disconnected` - 未连接
- `Connecting` - 连接中
- `Connected` - 已连接
- `Failed` - 连接失败

**事件**：
- `ConnectionStateChanged` - 连接状态变化
- `ContactsUpdated` - 联系人更新
- `GroupBound` - 群组绑定

## 📊 数据模型

### Member（会员）

**核心字段**：
- `Id` - 主键
- `GroupWxId` - 群组ID
- `Wxid` - 微信ID
- `Account` - 微信号
- `Nickname` - 昵称
- `DisplayName` - 群昵称
- `Balance` - 余额
- `State` - 会员状态（普通会员、管理员、代理、已退群）

**统计字段**：
- `BetCur` - 当前投注金额
- `BetWait` - 等待投注金额
- `IncomeToday` - 今日收入
- `CreditToday` - 今日上分
- `BetToday` - 今日投注
- `WithdrawToday` - 今日下分
- `BetTotal` - 总投注
- `CreditTotal` - 总上分
- `WithdrawTotal` - 总下分
- `IncomeTotal` - 总收入

### Order（订单）

**核心字段**：
- `Id` - 主键
- `GroupWxId` - 群组ID
- `Wxid` - 会员微信ID
- `Account` - 会员号码
- `Nickname` - 会员昵称
- `IssueId` - 期号
- `BetContentOriginal` - 原始投注内容
- `BetContentStandard` - 标准投注内容
- `Nums` - 注码数量
- `AmountTotal` - 投注总金额
- `BetAmount` - 投注金额
- `BetBeforeBalance` - 投注前余额
- `BetAfterBalance` - 投注后余额
- `Profit` - 盈利
- `NetProfit` - 纯利
- `Odds` - 赔率
- `OrderStatus` - 订单状态
- `OrderType` - 订单类型
- `MemberState` - 会员状态快照
- `IsSettled` - 是否已结算
- `TimestampBet` - 投注时间戳

### Contact（联系人）

**核心字段**：
- `Wxid` - 微信ID（唯一标识）
- `Account` - 微信号
- `Nickname` - 昵称
- `DisplayName` - 群昵称
- `Avatar` - 头像URL
- `IsGroup` - 是否为群组
- `GroupWxId` - 所属群组ID
- `LastUpdateTime` - 最后更新时间

### LotteryData（开奖数据）

**核心字段**：
- `IssueId` - 期号
- `LotteryNumber` - 开奖号码
- `LotteryTime` - 开奖时间
- `OpenTime` - 开盘时间
- `SealTime` - 封盘时间
- `Status` - 状态
- `SecondsToSeal` - 距离封盘秒数

## 🔧 服务实现框架

所有服务实现都只包含框架代码，不包含实际业务逻辑：

1. **LotteryService** - 开奖服务框架
   - 状态管理（线程安全）
   - 定时器框架
   - 事件触发框架
   - TODO: 实现具体业务逻辑

2. **OrderService** - 订单服务框架
   - 订单创建框架
   - 订单结算框架
   - 订单查询框架
   - TODO: 实现具体业务逻辑

3. **WechatService** - 微信服务框架
   - 连接状态管理（线程安全）
   - 启动流程框架
   - 数据刷新框架
   - 群组绑定框架
   - TODO: 实现具体业务逻辑

## 📝 后续工作

1. 实现具体的业务逻辑
2. 添加数据库支持（SQLite ORM）
3. 添加数据绑定支持（BindingList）
4. 实现消息处理逻辑
5. 实现开奖数据获取逻辑
6. 实现订单验证和结算逻辑

## 🎨 设计特点

1. **模块化** - 按模块分类，便于管理
2. **接口驱动** - 使用接口定义服务，便于测试和扩展
3. **事件驱动** - 使用事件通知状态变化
4. **线程安全** - 关键操作使用锁保护
5. **数据绑定** - 模型实现 INotifyPropertyChanged，支持 UI 自动更新
6. **框架先行** - 先搭建框架，再实现具体逻辑

