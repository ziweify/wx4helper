# WechatPageV1 逐步重建计划

## 🎯 目标

通过逐步重建 WechatPage，定位设计器无法使用的具体原因。

---

## 📋 步骤规划

### ✅ 步骤1：基础结构（已完成）

**文件**：`WechatPageV1.cs` + `WechatPageV1.Designer.cs`

**包含控件**：
- ✅ Form 基础
- ✅ ToolStrip（工具栏）
- ✅ StatusStrip（状态栏）

**测试项目**：
1. 在设计器中打开 `WechatPageV1.cs`
2. 点击 ToolStrip，查看属性面板是否显示 ToolStrip 的属性
3. 点击 StatusStrip，查看属性面板是否显示 StatusStrip 的属性
4. 尝试在 ToolStrip 上添加按钮

**预期结果**：
- ✅ 设计器应该可以正常打开
- ✅ 应该可以选中 ToolStrip 和 StatusStrip
- ✅ 属性面板应该正确显示控件属性

**如果步骤1失败**：说明基础环境有问题（Visual Studio、DevExpress 安装等）

---

### ✅ 步骤2：添加完整的布局结构（已完成）

**添加内容**：
- DevExpress.XtraEditors.SplitContainerControl（主分隔容器，Dock = Fill）
- 左侧完整布局：
  - panelControl_Left（容器）
  - panelControl_LeftTop（顶部操作区，Dock = Top）
    - textEdit_CurrentContact、simpleButton_BindingContacts、simpleButton_RefreshContacts
  - panelControl_OpenData（开奖数据区，Dock = Top）
    - labelControl_CurrentLottery、labelControl_LastLottery
  - gridControl_Contacts（联系人列表，Dock = Fill）
  - panelControl_FastSetting（快速设置区，Dock = Bottom）
    - labelControl_FastSetting
- 右侧完整布局：
  - panelControl_Right（容器）
  - splitContainerControl_Right（嵌套分隔容器，垂直分割，Dock = Fill）
    - Panel1：panelControl_Members
      - panelControl_MembersTop（顶部标题，Dock = Top）
        - labelControl_MemberInfo
      - gridControl_Members（会员列表，Dock = Fill）
    - Panel2：panelControl_Orders
      - panelControl_OrdersTop（顶部标题，Dock = Top）
        - labelControl_OrderInfo
      - gridControl_Orders（订单列表，Dock = Fill）

**测试项目**：
1. 在设计器中打开 WechatPageV1
2. **测试左侧区域**：
   - 点击左侧的 textEdit_CurrentContact，看能否选中
   - 点击左侧的 simpleButton_BindingContacts、simpleButton_RefreshContacts
   - 点击 labelControl_CurrentLottery、labelControl_LastLottery
   - **重点**：点击 gridControl_Contacts（联系人列表），看能否选中
   - 点击 labelControl_FastSetting
3. **测试右侧区域**：
   - 点击右上方的 labelControl_MemberInfo
   - **重点**：点击 gridControl_Members（会员列表），看能否选中
   - 点击右下方的 labelControl_OrderInfo
   - **重点**：点击 gridControl_Orders（订单列表），看能否选中
4. **测试分隔条**：
   - 拖动主分隔条（左右分割）
   - 拖动右侧垂直分隔条（上下分割）
5. **使用文档大纲窗口**：
   - 展开 splitContainerControl_Main
   - 展开 Panel1、Panel2
   - 尝试在文档大纲中选中各个控件
6. **检查属性面板**：
   - 选中每个控件后，属性面板是否显示正确的控件属性？

**预期结果**：
- ✅ 应该可以选中所有 Label、Button、TextEdit
- ✅ 应该可以选中 GridControl（3个）
- ✅ 应该可以调整分隔条位置
- ⚠️ 点击 Panel 内部空白区域时，可能会选中 PanelControl 或 SplitContainerControl（正常行为）
- ✅ 使用文档大纲窗口应该可以选中任意控件

**如果步骤2失败**：
- **现象1**：GridControl 无法选中 → 说明问题出在 GridControl + Dock 的组合上
- **现象2**：只能选中最外层的 SplitContainerControl → 说明问题出在 Z-Order 或嵌套的 Dock 设置上
- **现象3**：所有控件都无法选中 → 说明问题出在 Form 本身或 DesignMode 检测上

---

### 🔲 步骤3：对比原 WechatPage（如果步骤2成功）

**如果步骤2的设计器完全可用**：
1. 对比 WechatPageV1.Designer.cs 和 WechatPage.Designer.cs
2. 找出差异点（属性设置、控件顺序、事件处理等）
3. 逐步向 WechatPageV1 添加原 WechatPage 的特性，直到复现问题

**如果步骤2的设计器部分不可用**：
1. 记录哪些控件可以选中，哪些不能
2. 分析不能选中的控件的共同特征
3. 尝试调整这些控件的属性（如 Dock、Z-Order 等）

---

### 🔲 步骤4：废弃（合并到步骤2）

---

### 🔲 步骤5：废弃（合并到步骤2）

---

## 🔍 问题诊断

### 如果步骤1成功，步骤2失败

**结论**：问题出在 DevExpress.XtraEditors.SplitContainerControl + Dock = Fill

**解决方案**：
- 考虑使用标准 WinForms 的 SplitContainer
- 或者使用 Panel + TableLayoutPanel 代替

---

### 如果步骤2成功，步骤3失败

**结论**：问题出在 PanelControl 的多层嵌套 + Dock 组合

**解决方案**：
- 减少嵌套层次
- 使用固定大小而不是 Dock
- 考虑使用 TableLayoutPanel

---

### 如果步骤3成功，步骤4失败

**结论**：问题出在嵌套的 SplitContainerControl

**解决方案**：
- 不使用嵌套的 SplitContainerControl
- 使用单个 SplitContainer + 手动布局

---

### 如果所有步骤都成功

**结论**：原 WechatPage 可能有特定的设置导致问题

**对比检查**：
1. 对比两个文件的 Designer.cs，找出差异
2. 检查是否有特殊的属性设置
3. 检查控件的添加顺序

---

## 📝 测试记录

### 步骤1测试结果

**日期**：2025-12-25  
**测试人**：用户

- [x] 设计器可以正常打开
- [x] 可以选中 ToolStrip
- [x] 可以选中 StatusStrip
- [x] 属性面板显示正确
- [x] 可以添加工具栏按钮

**问题**：
- 无

**备注**：
- ✅ 步骤1完全成功，进入步骤2 

---

### 步骤2测试结果

**日期**：_____  
**测试人**：_____

**基础选择测试**：
- [ ] 可以选中 splitContainerControl_Main
- [ ] 可以拖动主分隔条（左右）
- [ ] 可以拖动右侧垂直分隔条（上下）

**左侧区域测试**：
- [ ] 可以选中 textEdit_CurrentContact
- [ ] 可以选中 simpleButton_BindingContacts
- [ ] 可以选中 simpleButton_RefreshContacts
- [ ] 可以选中 labelControl_CurrentLottery
- [ ] 可以选中 labelControl_LastLottery
- [ ] **可以选中 gridControl_Contacts**（重点）
- [ ] 可以选中 labelControl_FastSetting

**右侧区域测试**：
- [ ] 可以选中 labelControl_MemberInfo
- [ ] **可以选中 gridControl_Members**（重点）
- [ ] 可以选中 labelControl_OrderInfo
- [ ] **可以选中 gridControl_Orders**（重点）

**文档大纲测试**：
- [ ] 可以在文档大纲中看到完整的控件树
- [ ] 可以通过文档大纲选中任意控件
- [ ] 选中后属性面板显示正确

**问题记录**：
- 

**备注**：
- 

---

### 步骤3测试结果

**日期**：_____  
**测试人**：_____

- [ ] 可以在文档大纲中选中左侧控件
- [ ] 属性面板显示正确
- [ ] 可以调整控件大小/位置

**问题**：
- 

**备注**：
- 

---

### 步骤4测试结果

**日期**：_____  
**测试人**：_____

- [ ] 嵌套的 SplitContainerControl 可以工作
- [ ] 可以调整垂直分隔条位置

**问题**：
- 

**备注**：
- 

---

### 步骤5测试结果

**日期**：_____  
**测试人**：_____

- [ ] 所有控件都可以正常操作
- [ ] 与原 WechatPage 功能一致

**问题**：
- 

**备注**：
- 

---

## 🎯 下一步行动

根据测试结果：

**如果某个步骤失败**：
1. 记录失败的具体现象
2. 尝试微调该步骤的实现方式
3. 找出临界点

**如果所有步骤都成功**：
1. 用 WechatPageV1 替换原 WechatPage
2. 或者分析原 WechatPage 的特殊设置

---

**最后更新**: 2025-12-25  
**当前状态**: 步骤2已创建（包含完整布局结构），等待测试

