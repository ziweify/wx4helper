# SpinEdit 控件格式和步进设置修复

## 🐛 问题描述

1. **整数控件显示小数点问题**：
   - 封盘提前秒、最小投注、最大投注等整数控件
   - 点击上下按钮时，值后面会显示一个点（.），例如 38 变成 37.
   - 直到焦点移开才恢复正常

2. **步进设置缺失**：
   - 赔率需要步进 0.01
   - 飞单倍率需要步进 0.1

---

## ✅ 修复方案

### 1. 整数控件 - 禁用浮点数模式

为所有整数控件设置 `IsFloatValue = false`，确保不显示小数点：

**修复的控件**：
- `spinEdit_SealSeconds` - 封盘提前秒
- `spinEdit_MinBet` - 最小投注
- `spinEdit_MaxBet` - 最大投注

**修改内容**：
```csharp
spinEdit_SealSeconds.Properties.IsFloatValue = false;
spinEdit_MinBet.Properties.IsFloatValue = false;
spinEdit_MaxBet.Properties.IsFloatValue = false;
```

### 2. 赔率控件 - 设置步进 0.01

**控件**：`spinEdit_Odds`

**修改内容**：
```csharp
spinEdit_Odds.Properties.IsFloatValue = true;
spinEdit_Odds.Properties.Increment = new decimal(new int[] { 1, 0, 0, 131072 }); // 0.01
spinEdit_Odds.Properties.DisplayFormat.FormatString = "F2"; // 2位小数
spinEdit_Odds.Properties.EditFormat.FormatString = "F2";
```

### 3. 飞单倍率控件 - 设置步进 0.1，格式改为1位小数

**控件**：`spinEdit_FlyBetMultiplier`

**修改内容**：
```csharp
spinEdit_FlyBetMultiplier.Properties.IsFloatValue = true;
spinEdit_FlyBetMultiplier.Properties.Increment = new decimal(new int[] { 1, 0, 0, 65536 }); // 0.1
spinEdit_FlyBetMultiplier.Properties.DisplayFormat.FormatString = "F1"; // 1位小数（从F2改为F1）
spinEdit_FlyBetMultiplier.Properties.EditFormat.FormatString = "F1";
```

---

## 📋 控件设置总结

| 控件 | 类型 | IsFloatValue | Increment | 格式 | 说明 |
|-----|------|--------------|-----------|------|------|
| `spinEdit_SealSeconds` | 整数 | `false` | 1（默认） | 整数 | 封盘提前秒 |
| `spinEdit_MinBet` | 整数 | `false` | 1（默认） | 整数 | 最小投注 |
| `spinEdit_MaxBet` | 整数 | `false` | 1（默认） | 整数 | 最大投注 |
| `spinEdit_Odds` | 浮点数 | `true` | 0.01 | F2 | 赔率 |
| `spinEdit_FlyBetMultiplier` | 浮点数 | `true` | 0.1 | F1 | 飞单倍率 |
| `spinEdit_Balance` | 浮点数 | `true` | 1（默认） | F2 | 余额（只读） |

---

## 🔧 技术细节

### DevExpress SpinEdit 属性说明

1. **IsFloatValue**：
   - `false` = 整数模式，不显示小数点
   - `true` = 浮点数模式，可以显示小数

2. **Increment**：
   - 设置点击上下按钮时的步进值
   - 格式：`new decimal(new int[] { 分子, 0, 0, 分母标志 })`
   - 0.01 = `new decimal(new int[] { 1, 0, 0, 131072 })`
   - 0.1 = `new decimal(new int[] { 1, 0, 0, 65536 })`

3. **FormatString**：
   - `F0` = 整数格式
   - `F1` = 1位小数
   - `F2` = 2位小数

---

## ✅ 修复效果

### 修复前
- ❌ 封盘提前秒：点击下按钮，38 → 37.（显示小数点）
- ❌ 赔率：步进为1，无法精确调整
- ❌ 飞单倍率：步进为1，格式为2位小数

### 修复后
- ✅ 封盘提前秒：点击下按钮，38 → 37（不显示小数点）
- ✅ 最小/最大投注：不显示小数点
- ✅ 赔率：步进为0.01，可以精确调整
- ✅ 飞单倍率：步进为0.1，格式为1位小数

---

## 📝 文件修改

- **文件**：`永利系统/Views/Wechat/WechatPage.Designer.cs`
- **修改内容**：
  1. 为整数控件添加 `IsFloatValue = false`
  2. 为赔率控件添加 `Increment = 0.01`
  3. 为飞单倍率控件添加 `Increment = 0.1` 并修改格式为 `F1`


