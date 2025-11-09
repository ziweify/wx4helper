# 消息格式对比报告

## 修复内容

### 1. ✅ 余额不足消息（已修复）

**F5BotV2 格式**（第2430行）：
```
@{m.nickname} 客官你的荷包是否不足!
```

**修复前**：
```
余额不足！当前余额: {member.Balance:F2}，需要: {totalAmount:F2}
```

**修复后**（BinggoOrderService.cs 第112-115行）：
```
@{member.Nickname} 客官你的荷包是否不足!
```

---

## 消息格式对比清单

### 1. 查询消息（查、流水、货单）

**F5BotV2**（第2177-2180行）：
```
@{member.nickname}\r流~~记录\r今日/本轮进货:{member.BetToday}/{member.BetCur}\r今日上/下:{member.CreditToday}/{member.WithdrawToday}\r今日盈亏:{IncomeToday}\r
```

**当前实现**（BinggoLotteryService.cs 第997-1001行）：
```
@{member.Nickname}\r流~~记录\r今日/本轮进货:{member.BetToday}/{member.BetCur}\r今日上/下:{member.CreditToday}/{member.WithdrawToday}\r今日盈亏:{IncomeToday}\r
```

✅ **完全一致**

---

### 2. 上分/下分消息

**F5BotV2**（第2605行）：
```
@{m.nickname}\r[{m.id}]请等待
```

**当前实现**（BinggoLotteryService.cs 第1070行）：
```
@{member.Nickname}\r[{member.Id}]请等待
```

✅ **完全一致**

---

### 3. 下分余额不足消息

**F5BotV2**（第2430行）：
```
@{m.nickname} 客官你的荷包是否不足!
```

**当前实现**（BinggoLotteryService.cs 第1038行）：
```
@{member.Nickname} 客官你的荷包是否不足!
```

✅ **完全一致**

---

### 4. 取消订单消息

**F5BotV2**（第2221行）：
```
@{m.nickname} {ods.BetContentOriginal}\r已取消!\r+{ods.AmountTotal}|留:{(int)m.Balance}
```

**当前实现**（BinggoLotteryService.cs 第1117行）：
```
@{member.Nickname} {ods.BetContentOriginal}\r已取消!\r+{ods.AmountTotal}|留:{(int)member.Balance}
```

✅ **完全一致**

---

### 5. 取消订单失败（时间到）

**F5BotV2**（第2226行）：
```
@{m.nickname} 时间到!不能取消!
```

**当前实现**（BinggoLotteryService.cs 第1085行）：
```
@{member.Nickname} 时间到!不能取消!
```

✅ **完全一致**

---

### 6. 投注成功消息

**F5BotV2**（第2413行）：
```
@{m.nickname}\r已进仓{member_order.Nums}\r{betcontents.ToReplyString()}|扣:{member_order.AmountTotal}|留:{(int)m.Balance}
```

**当前实现**（BinggoOrderService.cs 第192行）：
```
@{member.Nickname}\r已进仓{order.Nums}\r{betContent.ToReplyString()}|扣:{(int)order.AmountTotal}|留:{(int)member.Balance}
```

⚠️ **差异**：`扣:{member_order.AmountTotal}` vs `扣:{(int)order.AmountTotal}`
- F5BotV2 使用 `AmountTotal`（float，可能带小数）
- 当前实现使用 `(int)order.AmountTotal`（整数）

**需要修复**：应该使用 `order.AmountTotal` 而不是 `(int)order.AmountTotal`

---

### 7. 订单重复消息

**F5BotV2**（第2418行）：
```
@{m.nickname}\r未进仓!订单重复!如需重复订单,请稍后一秒后再次下单!{member_order.Nums}\r扣:0|留:{(int)m.Balance}
```

**当前实现**：
❌ **未实现**（订单重复检查在 BinggoOrderService 中，但消息格式未实现）

---

### 8. 时间未到消息

**F5BotV2**（第2425行）：
```
{m.nickname}\r时间未到!不收货!
```

**当前实现**（BinggoLotteryService.cs 第1151行）：
```
已封盘，请等待下期！
```

❌ **不一致**：格式完全不同

**需要修复**：应该使用 `{member.Nickname}\r时间未到!不收货!`

---

### 9. 投注余额不足消息

**F5BotV2**（第2430行）：
```
@{m.nickname} 客官你的荷包是否不足!
```

**当前实现**（BinggoOrderService.cs 第114行）：
```
@{member.Nickname} 客官你的荷包是否不足!
```

✅ **完全一致**（已修复）

---

### 10. 开盘提示消息

**F5BotV2**（第1159行）：
```
第{issueid % 1000}队\r{Reply_开盘提示}
```
其中 `Reply_开盘提示 = "---------线下开始---------"`

**当前实现**（BinggoLotteryService.cs 第1205行）：
```
第{issueShort}队\r---------线下开始---------
```

✅ **完全一致**

---

### 11. 封盘提醒消息（30秒/15秒）

**F5BotV2**（第1008/1013行）：
```
{issueid%1000} 还剩30秒
{issueid%1000} 还剩15秒
```

**当前实现**（BinggoLotteryService.cs 第1248行）：
```
{issueShort} 还剩{seconds}秒
```

✅ **完全一致**

---

### 12. 封盘消息

**F5BotV2**（第1227-1238行）：
```
{issueid % 1000} {Reply_回合停止}\r
{nickname}[{(int)BetFronMoney}]:{BetContentStandar}|计:{AmountTotal}\r
...
------线下无效------
```
其中 `Reply_回合停止 = "时间到! 停止进仓! 以此为准!"`

**当前实现**（BinggoLotteryService.cs 第1285-1303行）：
```
{issueShort} 时间到! 停止进仓! 以此为准!\r
{nickname}[{(int)BetFronMoney}]:{BetContentStandar}|计:{AmountTotal}\r
...
------线下无效------
```

✅ **完全一致**

---

### 13. 中奖名单消息

**F5BotV2**（第1415-1462行）：
```
第{issueid_lite}队\r{data.ToLotteryString()}\r----中~名单----\r
{nickname}[{(int)balance}] {(int)order.Profit- order.AmountTotal}\r
...
```

**当前实现**（BinggoLotteryService.cs 第918-930行）：
```
第{issueidLite}队\r{lotteryData.ToLotteryString()}\r----中~名单----\r
{nickname}[{(int)balance}] {(int)netProfit}\r
...
```

✅ **完全一致**

---

### 14. 留分名单消息

**F5BotV2**（第1464-1474行）：
```
第{issueid_lite}队\r{data.ToLotteryString()}\r----留~名单----\r
{nickname} {(int)Balance}\r
...
```

**当前实现**（BinggoLotteryService.cs 第942-956行）：
```
第{issueidLite}队\r{lotteryData.ToLotteryString()}\r----留~名单----\r
{nickname} {(int)Balance}\r
...
```

✅ **完全一致**

---

## 已修复的问题

### 1. ✅ 余额不足消息格式（已修复）

**位置**：`BinggoOrderService.cs` 第112-115行

**修复**：将 `余额不足！当前余额: {member.Balance:F2}，需要: {totalAmount:F2}` 改为 `@{member.Nickname} 客官你的荷包是否不足!`

---

### 2. ✅ 投注成功消息 - 扣款格式（已修复）

**位置**：`BinggoOrderService.cs` 第201行

**修复**：将 `扣:{(int)order.AmountTotal}` 改为 `扣:{order.AmountTotal}`

**原因**：F5BotV2 使用 `AmountTotal`（float），不使用 `(int)` 转换

---

### 3. ✅ 时间未到消息格式（已修复）

**位置**：`BinggoLotteryService.cs` 第1152行

**修复**：将 `已封盘，请等待下期！` 改为 `{member.Nickname}\r时间未到!不收货!`

**原因**：F5BotV2 使用 `{m.nickname}\r时间未到!不收货!` 格式

---

### 4. ⚠️ 订单重复消息 - 待实现

**位置**：`BinggoOrderService.cs` 中需要添加订单重复检查

**F5BotV2 格式**（第2418行）：
```
@{m.nickname}\r未进仓!订单重复!如需重复订单,请稍后一秒后再次下单!{member_order.Nums}\r扣:0|留:{(int)m.Balance}
```

**说明**：F5BotV2 在 `v2memberOderbindlite.Add(member_order, OnMemberOrderCreate)` 中检查订单重复，如果返回 false，说明订单重复。当前实现中，`V2OrderBindingList` 的 `Add` 方法可能没有实现重复检查逻辑，需要后续实现。

---

## 总结

- ✅ **已修复**：余额不足消息格式
- ✅ **已修复**：投注成功消息的扣款格式（去掉 `(int)` 转换）
- ✅ **已修复**：时间未到消息格式
- ⚠️ **待实现**：订单重复消息（需要检查 `V2OrderBindingList.Add` 是否实现了重复检查）

