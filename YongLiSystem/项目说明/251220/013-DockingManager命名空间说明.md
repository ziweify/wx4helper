# DockingManager 命名空间说明

**📅 日期**: 2025-12-20  
**📌 主题**: DockingManager 和 DockPanel 的正确命名空间  
**📄 文件编号**: 251220-013

---

## 🔍 问题发现

通过反射检查 `DevExpress.XtraBars.v23.2.dll`，发现：

- ✅ `DockPanel` 在命名空间：`DevExpress.XtraBars.Docking.DockPanel`
- ✅ `DockingManager` 在命名空间：`DevExpress.XtraBars.Helpers.Docking.DockingManager`

**重要**：`DockingManager` **不在** `DevExpress.XtraBars.Docking` 命名空间中，而是在 `DevExpress.XtraBars.Helpers.Docking` 命名空间中！

---

## ✅ 正确的 using 语句

### 需要同时使用两个命名空间

```csharp
using DevExpress.XtraBars.Docking;        // 用于 DockPanel
using DevExpress.XtraBars.Helpers.Docking; // 用于 DockingManager
```

### 为什么需要两个命名空间？

- **`DevExpress.XtraBars.Docking`** - 包含 `DockPanel`、`DockLayout` 等停靠面板相关类
- **`DevExpress.XtraBars.Helpers.Docking`** - 包含 `DockingManager` 等停靠管理器相关类

这是 DevExpress 的设计，将停靠功能分为两个命名空间。

---

## 📝 代码示例

### Main.cs

```csharp
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraBars;
using DevExpress.XtraBars.Docking;        // DockPanel
using DevExpress.XtraBars.Helpers.Docking; // DockingManager
using DevExpress.XtraBars.Ribbon;

namespace 永利系统.Views
{
    public partial class Main : RibbonForm
    {
        private DockingManager? _dockingManager; // 来自 Helpers.Docking
        private LogWindow? _logWindow;            // 继承自 DockPanel（来自 Docking）
        
        // ...
    }
}
```

### LogWindow.cs

```csharp
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraBars.Docking;        // DockPanel（基类）
using DevExpress.XtraBars.Helpers.Docking; // DockingManager（构造函数参数）
using DevExpress.XtraEditors;

namespace 永利系统.Views
{
    public partial class LogWindow : DockPanel // 来自 Docking
    {
        public LogWindow(DockingManager dockingManager) // 来自 Helpers.Docking
        {
            // ...
        }
    }
}
```

---

## 🔧 已修复的文件

1. ✅ `永利系统/Views/Main.cs` - 已添加 `using DevExpress.XtraBars.Helpers.Docking;`
2. ✅ `永利系统/Views/LogWindow.cs` - 已添加 `using DevExpress.XtraBars.Helpers.Docking;`

---

## 💡 为什么 VS 提示了这两个命名空间？

Visual Studio 的智能提示可能显示了：
1. `DevExpress.Utils.CodedUISupport` - 这是错误的提示（用于 CodedUI 测试）
2. `DevExpress.XtraBars.Helpers.Docking` - **这是正确的！** 应该使用这个

VS 可能因为找不到 `DockingManager` 而给出了模糊的建议，但 `DevExpress.XtraBars.Helpers.Docking` 是正确的命名空间。

---

## 📋 验证方法

### 使用反射验证（PowerShell）

```powershell
$assembly = [System.Reflection.Assembly]::LoadFrom("C:\Program Files\DevExpress 23.2\Components\Bin\NetCore\DevExpress.XtraBars.v23.2.dll")
$types = $assembly.GetTypes() | Where-Object { $_.Name -eq "DockingManager" -or $_.Name -eq "DockPanel" }
$types | ForEach-Object { Write-Host $_.FullName }
```

**输出**：
```
DevExpress.XtraBars.Docking.DockPanel
DevExpress.XtraBars.Helpers.Docking.DockingManager
```

---

## 🎯 总结

### 关键点

1. ✅ **`DockPanel`** 使用：`using DevExpress.XtraBars.Docking;`
2. ✅ **`DockingManager`** 使用：`using DevExpress.XtraBars.Helpers.Docking;`
3. ✅ **两个命名空间都需要**，因为它们包含不同的类

### 为什么会有两个命名空间？

这是 DevExpress 的设计：
- **`Docking`** - 停靠面板相关（UI 组件）
- **`Helpers.Docking`** - 停靠管理器相关（管理逻辑）

---

**说明文件编号**: 251220-013-DockingManager命名空间说明  
**创建时间**: 2025-12-20  
**文件类型**: 命名空间说明  
**版本**: v1.0

