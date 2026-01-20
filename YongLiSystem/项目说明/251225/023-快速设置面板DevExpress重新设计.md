# 快速设置面板 - DevExpress 控件重新设计

## 📝 需求

将 BaiShengVx3Plus 主界面中的"快速设置"部分，使用 DevExpress 控件重新设计，放入到 `panelControl_FastSetting` 中。

---

## ✅ 已完成的修改

### 1. 控件映射表

| 原控件（SunnyUI） | DevExpress 控件 | 说明 |
|------------------|----------------|------|
| `UISwitch` | `ToggleSwitch` | 开关控件 |
| `UIIntegerUpDown` | `SpinEdit` | 整数输入（带上下箭头） |
| `UIDoubleUpDown` | `SpinEdit` | 小数输入（带上下箭头） |
| `UIComboBox` | `ComboBoxEdit` | 下拉列表 |
| `UITextBox` | `TextEdit` | 文本输入框 |
| `UIButton` | `SimpleButton` | 按钮 |
| `Label` | `LabelControl` | 标签 |

### 2. 快速设置控件清单

#### 开关控件
1. **toggleSwitch_OrdersTasking** - 收单开关
   - OnText: "收单中"
   - OffText: "收单停"

2. **toggleSwitch_AutoOrdersBet** - 飞单开关
   - OnText: "飞单中"
   - OffText: "飞单停"

#### 数值输入控件
3. **spinEdit_SealSeconds** - 封盘提前(秒)
   - 范围：10-300
   - 默认值：49

4. **spinEdit_MinBet** - 最小投注
   - 范围：1-10000
   - 默认值：1

5. **spinEdit_MaxBet** - 最大投注
   - 范围：1-1000000
   - 默认值：10000

6. **spinEdit_Odds** - 赔率
   - 范围：0-1000
   - 小数位数：2位
   - 默认值：0

#### 下拉列表
7. **comboBoxEdit_Platform** - 盘口选择
   - 下拉列表样式（不可编辑）
   - 待填充平台列表

#### 文本输入控件
8. **textEdit_AutoBetUsername** - 投注账号
   - 水印提示："投注账号"

9. **textEdit_AutoBetPassword** - 投注密码
   - 密码字符：`*`
   - 水印提示："投注密码"

#### 按钮控件
10. **simpleButton_StartBrowser** - 启动浏览器
    - 文本："启动浏览器"

11. **simpleButton_ConfigManager** - 配置管理
    - 文本："配置管理"

---

## 📐 布局结构

```
panelControl_FastSetting
├── labelControl_FastSetting (标题："快速设置")
├── labelControl_SealSeconds + spinEdit_SealSeconds (封盘提前)
├── labelControl_MinBet + spinEdit_MinBet (最小投注)
├── labelControl_MaxBet + spinEdit_MaxBet (最大投注)
├── labelControl_Platform + comboBoxEdit_Platform (盘口)
├── labelControl_AutoBetUsername + textEdit_AutoBetUsername (账号)
├── labelControl_AutoBetPassword + textEdit_AutoBetPassword (密码)
├── labelControl_Odds + spinEdit_Odds (赔率)
├── toggleSwitch_OrdersTasking (收单开关)
├── toggleSwitch_AutoOrdersBet (飞单开关)
├── simpleButton_StartBrowser (启动浏览器)
└── simpleButton_ConfigManager (配置管理)
```

---

## 🎨 控件位置（Y坐标）

| 控件 | Y坐标 | 说明 |
|-----|------|------|
| labelControl_FastSetting | 5 | 标题 |
| labelControl_SealSeconds | 30 | 封盘提前标签 |
| spinEdit_SealSeconds | 28 | 封盘提前输入 |
| labelControl_MinBet | 55 | 最小投注标签 |
| spinEdit_MinBet | 53 | 最小投注输入 |
| labelControl_MaxBet | 80 | 最大投注标签 |
| spinEdit_MaxBet | 78 | 最大投注输入 |
| labelControl_Platform | 105 | 盘口标签 |
| comboBoxEdit_Platform | 103 | 盘口下拉 |
| labelControl_AutoBetUsername | 130 | 账号标签 |
| textEdit_AutoBetUsername | 128 | 账号输入 |
| labelControl_AutoBetPassword | 155 | 密码标签 |
| textEdit_AutoBetPassword | 153 | 密码输入 |
| labelControl_Odds | 180 | 赔率标签 |
| spinEdit_Odds | 178 | 赔率输入 |
| toggleSwitch_OrdersTasking | 205 | 收单开关 |
| toggleSwitch_AutoOrdersBet | 235 | 飞单开关 |
| simpleButton_StartBrowser | 265 | 启动浏览器按钮 |
| simpleButton_ConfigManager | 265 | 配置管理按钮 |

---

## 📋 文件修改清单

### 修改的文件
- `永利系统/Views/Wechat/WechatPage.Designer.cs`
  - 添加了11个新控件的声明
  - 添加了所有控件的初始化代码
  - 添加了 SuspendLayout/ResumeLayout 调用
  - 添加了字段声明

---

## 🔧 后续工作

### 1. 事件处理
需要在 `WechatPage.cs` 中添加事件处理程序：
- `toggleSwitch_OrdersTasking.Toggled` - 收单开关切换
- `toggleSwitch_AutoOrdersBet.Toggled` - 飞单开关切换
- `spinEdit_SealSeconds.EditValueChanged` - 封盘秒数改变
- `spinEdit_MinBet.EditValueChanged` - 最小投注改变
- `spinEdit_MaxBet.EditValueChanged` - 最大投注改变
- `comboBoxEdit_Platform.SelectedIndexChanged` - 盘口选择改变
- `textEdit_AutoBetUsername.EditValueChanged` - 账号改变
- `textEdit_AutoBetPassword.EditValueChanged` - 密码改变
- `spinEdit_Odds.EditValueChanged` - 赔率改变
- `simpleButton_StartBrowser.Click` - 启动浏览器
- `simpleButton_ConfigManager.Click` - 配置管理

### 2. 数据绑定
- 从配置系统加载初始值
- 保存用户修改到配置系统
- 填充平台下拉列表

### 3. 布局优化
- 可以在设计器中调整控件位置和大小
- 可以调整 `panelControl_FastSetting` 的高度以适应所有控件

---

## ✅ 完成状态

- ✅ 所有控件已添加到 Designer.cs
- ✅ 控件属性已正确设置
- ✅ 布局位置已设置
- ✅ 编译无错误
- ⏳ 事件处理待实现
- ⏳ 数据绑定待实现


