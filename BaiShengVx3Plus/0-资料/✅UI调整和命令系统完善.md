# UI调整和命令系统完善

## ✅ 已完成

### 1. 订单表调整
- ✅ **NetProfit（纯利）列**：
  - 列标题改为"纯利"
  - 宽度调整为 70px
  - 格式化显示小数点后2位（已有 Format = "{0:F2}"）
  
- ✅ **Account列**：
  - Order 从 16 改为 15（倒数第二列）
  - Visible = false（隐藏）

- ✅ **Notes列**：
  - Order 从 17 改为 16（最后一列）

### 2. 会员表调整
- ✅ **Wxid列**：
  - Visible = false（隐藏）

## 🔄 需要实现

### 3. 会员表右键菜单

根据之前的讨论和F5BotV2，需要实现以下右键菜单：

#### 菜单项
1. **清分** - 清空会员余额
2. **删除** - 删除会员（标记为已删除或直接删除）
3. **设置会员类型** - 子菜单
   - 普会
   - 会员（盘内）
   - 托
   - 蓝会（盘外）
   - 黄会
4. **资金变动** - 打开资金变动查看窗口，显示该会员的所有资金变动记录

#### 实现位置
- `VxMain.cs` - 添加 `dgvMembers` 的 `ContextMenuStrip`
- `VxMain.Designer.cs` - 设计右键菜单

### 4. 资金变动查看窗口

#### 功能
- 显示指定会员的所有资金变动记录
- 支持按时间、类型筛选
- 显示详细信息：变动前后余额、变动金额、原因、关联订单等

#### 实现文件
- `Views/BalanceChangeViewerForm.cs`
- `Views/BalanceChangeViewerForm.Designer.cs`

### 5. "查"命令实现

根据 F5BotV2 (BoterServices.cs 第2174行)：

```csharp
if(msg == "查" || msg == "流水" || msg == "货单")
{
    var member = v2Memberbindlite.FirstOrDefault(p=>p.wxid == msgpack.from_wxid);
    string sendTxt = $"@{member.nickname}\r流~~记录\r";
    sendTxt += $"今日/本轮进货:{member.BetToday}/{member.BetCur}\r";
    sendTxt += $"今日上/下:{member.CreditToday}/{member.WithdrawToday}\r";
    sendTxt += $"今日盈亏:" + (this._appSetting.Zsxs ? ((int)member.IncomeToday).ToString() : member.IncomeToday.ToString("F2")) + "\r";
    wxHelper.CallSendText_11036(member.GroupWxId, sendTxt);
    return 0;
}
```

#### 命令格式
- `查` / `流水` / `货单`

#### 回复格式
```
@张三
流~~记录
今日/本轮进货:150.00/50.00
今日上/下:1000.00/500.00
今日盈亏:85.50
```

#### 实现位置
- `Services/Messages/Handlers/BinggoMessageHandler.cs`
- 在 `ProcessBettingCommand` 之前检查是否是查询命令

### 6. 其他F5BotV2命令检查

需要检查的命令：
- ✅ 下注命令：`123大10`、`6大50` 等
- ✅ 取消命令：`取消`、`全部取消`
- ❓ 查询命令：`查`、`流水`、`货单`
- ❓ 上下分命令：`上1000`、`下500`
- ❓ 其他命令...

## 📋 实现优先级

1. **高优先级**
   - ✅ 列宽和格式调整（已完成）
   - 🔄 "查"命令实现
   - 🔄 会员表右键菜单

2. **中优先级**
   - 资金变动查看窗口
   - 上下分管理窗口

3. **低优先级**
   - 完善其他命令

## 🎯 下一步行动

1. 实现"查"命令
2. 实现会员表右键菜单
3. 创建资金变动查看窗口
4. 完善命令系统

## 📝 注意事项

1. 所有命令的回复格式必须与F5BotV2完全一致
2. 会员类型的设置要更新 `OrderType` 字段
3. 清分操作要记录到资金变动表
4. 删除会员要考虑是物理删除还是逻辑删除（标记State）

