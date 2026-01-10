# 期号变更订单合并BUG - 深入分析

## 问题描述

**严重BUG**：当100期投注失败（网站卡了）时，如果投注命令还在队列中等待执行，当101期开始时，系统可能会把100期的订单和101期的订单合并在一起投注。

## 订单流程分析

### 1. 订单创建流程

```csharp
// BinggoOrderService.CreateOrderAsync (第185-226行)
order = new V2MemberOrder
{
    IssueId = issueId,  // 🔥 订单创建时，期号被设置为当前期号（例如100期）
    OrderStatus = OrderStatus.待处理,  // 🔥 初始状态为待处理，等待自动投注
    OrderType = member.State == MemberState.托 ? OrderType.托 : OrderType.待定,
    // ... 其他字段
};

// 订单被添加到内存表
_ordersBindingList.Insert(0, order);
```

**关键点**：
- 订单创建时，`IssueId` 被设置为当前期号（例如100期）
- 订单状态被设置为 `OrderStatus.待处理`
- 订单被添加到 `_ordersBindingList`（内存表）

### 2. 查询待投注订单

```csharp
// BinggoOrderService.GetPendingOrdersForIssue (第767-797行)
public IEnumerable<V2MemberOrder> GetPendingOrdersForIssue(int issueId)
{
    var allOrders = _ordersBindingList
        .Where(o => o.IssueId == issueId && o.OrderStatus == OrderStatus.待处理)
        .ToList();
    
    // 排除托单
    var validOrders = allOrders
        .Where(o => o.OrderType != OrderType.托)
        .ToList();
    
    return validOrders;
}
```

**关键点**：
- 查询条件是：`o.IssueId == issueId && o.OrderStatus == OrderStatus.待处理`
- 这个查询逻辑是正确的，只会查询指定期号的待处理订单
- 从内存表（`_ordersBindingList`）查询，而不是数据库

### 3. 封盘投注流程

```csharp
// AutoBetCoordinator.LotteryService_StatusChanged (第201-435行)
if (e.NewStatus == BinggoLotteryStatus.封盘中)
{
    // 1. 查询待处理订单
    var pendingOrders = _orderService.GetPendingOrdersForIssue(e.IssueId);
    
    // 2. 合并订单
    var mergeResult = _orderMerger.Merge(pendingOrders);
    
    // 3. 创建投注记录
    var betRecord = new BetRecord
    {
        IssueId = e.IssueId,
        OrderIds = string.Join(",", mergeResult.OrderIds),
        // ...
    };
    
    // 4. 加入投注队列（异步执行）
    _betQueueManager.EnqueueBet(betRecord.Id, async () =>
    {
        // 投注命令执行
        var result = await _autoBetService.SendBetCommandAsync(...);
        
        // 更新订单状态
        foreach (var orderId in mergeResult.OrderIds)
        {
            var order = pendingOrdersList.FirstOrDefault(o => o.Id == orderId);
            if (order != null)
            {
                order.OrderStatus = OrderStatus.待结算;
                order.OrderType = result.Success ? OrderType.盘内 : OrderType.盘外;
                _orderService.UpdateOrder(order);
            }
        }
    });
}
```

**关键点**：
- 封盘时，查询当前期号的待处理订单
- 合并订单，生成投注命令
- 投注命令被加入队列，**异步执行**
- 投注命令执行时，更新订单状态

## 问题根源分析

### 问题场景

1. **100期封盘时**：
   - 查询到100期的订单（例如：订单A、订单B）
   - 生成投注命令，加入队列
   - 投注命令在队列中等待执行

2. **网站卡了**：
   - 投注命令执行超时或失败
   - 订单状态可能仍然是"待处理"（如果投注命令还没执行完）
   - 或者订单状态被更新为"盘外+待结算"（如果投注命令执行失败）

3. **101期开始时**：
   - 期号变更事件触发
   - 调用 `CleanupPendingOrdersForExpiredIssue(100)` 清理100期的待处理订单
   - 但是，如果100期的投注命令还在队列中，它可能还没执行完

4. **101期封盘时**：
   - 查询到101期的订单（例如：订单C、订单D）
   - 生成投注命令，加入队列
   - 如果100期的投注命令还在队列中，当它执行时，可能会把100期的订单和101期的订单合并在一起投注

### 根本原因

**问题1：投注命令异步执行，没有期号验证**

投注命令在队列中异步执行，执行时没有验证期号是否仍然有效。如果投注命令在下一期执行，它仍然会投注上一期的订单。

**问题2：期号变更时，投注命令可能还在队列中**

期号变更时，虽然清理了上一期的待处理订单，但是投注命令可能还在队列中等待执行。当投注命令执行时，它可能会把上一期的订单和当前期的订单合并在一起投注。

**问题3：订单状态更新不及时**

如果投注命令执行失败，订单状态可能不会立即更新，导致订单仍然是"待处理"状态。

## 修复方案

### 修复1：投注命令执行前验证期号

在投注命令执行前，验证期号是否仍然有效。如果期号已变更，拒绝投注，并将订单标记为"盘外+待结算"。

```csharp
_betQueueManager.EnqueueBet(betRecord.Id, async () =>
{
    // 🔥 修复BUG：投注命令执行前，验证期号是否仍然有效
    var (currentStatus, currentIssueId, canBet) = _lotteryService.GetStatusSnapshot();
    
    if (currentIssueId != targetIssueId)
    {
        // 期号已变更，拒绝投注，将订单标记为"盘外+待结算"
        // ...
        return new BetResult { Success = false, ErrorMessage = "期号已变更" };
    }
    
    // 执行投注
    // ...
});
```

### 修复2：期号变更时清理过期订单

在期号变更时，清理上一期的待处理订单，将它们标记为"盘外+待结算"。

```csharp
private void LotteryService_IssueChanged(object? sender, BinggoIssueChangedEventArgs e)
{
    // 🔥 修复BUG：期号变更时，清理上一期的待处理订单
    CleanupPendingOrdersForExpiredIssue(e.OldIssueId);
}

private void CleanupPendingOrdersForExpiredIssue(int expiredIssueId)
{
    // 查询上一期的待处理订单
    var expiredOrders = _orderService.GetPendingOrdersForIssue(expiredIssueId).ToList();
    
    // 将过期订单标记为"盘外+待结算"
    foreach (var order in expiredOrders)
    {
        order.OrderStatus = OrderStatus.待结算;
        order.OrderType = OrderType.盘外;
        _orderService.UpdateOrder(order);
    }
}
```

### 修复3：投注命令执行时验证订单期号

在投注命令执行时，验证订单的期号是否仍然匹配。如果订单的期号已过期，跳过该订单。

```csharp
// 更新订单状态时，验证订单期号
foreach (var orderId in mergeResult.OrderIds)
{
    var order = pendingOrdersList.FirstOrDefault(o => o.Id == orderId);
    if (order != null)
    {
        // 🔥 验证订单期号是否仍然匹配
        if (order.IssueId != targetIssueId)
        {
            _log.Warning("AutoBet", $"⚠️ 订单期号已过期: 订单ID={order.Id}, 订单期号={order.IssueId}, 目标期号={targetIssueId}");
            continue;  // 跳过过期订单
        }
        
        order.OrderStatus = OrderStatus.待结算;
        order.OrderType = result.Success ? OrderType.盘内 : OrderType.盘外;
        _orderService.UpdateOrder(order);
    }
}
```

## 总结

**问题根源**：
1. 投注命令异步执行，没有期号验证
2. 期号变更时，投注命令可能还在队列中
3. 订单状态更新不及时

**修复方案**：
1. ✅ 投注命令执行前验证期号
2. ✅ 期号变更时清理过期订单
3. ✅ 投注命令执行时验证订单期号

**修复效果**：
- 期号变更后，上一期的待处理订单不会被投注
- 投注命令执行前会验证期号是否仍然有效
- 过期订单会被正确标记为"盘外+待结算"

