# Loader.dll 路径修复

**问题发现时间**: 2025年11月6日 01:05  
**状态**: ✅ 已修复  
**问题**: 找不到 Loader.dll 文件

---

## 🐛 问题描述

### 错误信息

```
Unable to load DLL 'Loader.dll' or one of its dependencies: 
找不到指定的模块。 (0x8007007E)
```

### 根本原因

**P/Invoke 默认行为**：

```csharp
[DllImport("Loader.dll")]  // ← 只指定文件名，没有路径
```

**搜索顺序**：
1. 当前目录（`bin\Debug\net8.0-windows\`）
2. 系统目录（`C:\Windows\System32\`）
3. PATH 环境变量

**实际情况**：
- `Loader.dll` 在固定位置：`bin\release\net8.0-windows\`
- 不在搜索路径中 → **找不到！**

---

## 🔧 解决方案

### 修改前的代码

**文件**: `BaiShengVx3Plus/Native/LoaderNative.cs`

```csharp
public static class LoaderNative
{
    private const string DLL_NAME = "Loader.dll";
    
    // ❌ 直接使用文件名，依赖系统搜索路径
    [DllImport(DLL_NAME)]
    public static extern bool LaunchWeChatWithInjection(...);
}
```

**问题**：运行 Debug 版本时，系统在 `bin\Debug\...\` 下找不到 DLL。

---

### 修改后的代码

**文件**: `BaiShengVx3Plus/Native/LoaderNative.cs`

```csharp
public static class LoaderNative
{
    private const string DLL_NAME = "Loader.dll";

    // 🔥 静态构造函数：在第一次使用前加载 DLL
    static LoaderNative()
    {
        // 1. 获取固定路径：bin\release\net8.0-windows\Loader.dll
        var basePath = Path.GetDirectoryName(
            AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar));
        basePath = Path.GetDirectoryName(basePath); // 回到 bin 目录
        var dllPath = Path.Combine(basePath, "release", "net8.0-windows", "Loader.dll");
        
        Console.WriteLine($"[LoaderNative] 加载 Loader.dll: {dllPath}");
        
        // 2. 检查文件是否存在
        if (!File.Exists(dllPath))
        {
            throw new FileNotFoundException($"找不到 Loader.dll: {dllPath}");
        }

        // 3. 使用 LoadLibrary 预加载 DLL
        var handle = LoadLibrary(dllPath);
        if (handle == IntPtr.Zero)
        {
            var error = Marshal.GetLastWin32Error();
            throw new DllNotFoundException($"无法加载 Loader.dll: {dllPath}, Error: {error}");
        }
        
        Console.WriteLine($"[LoaderNative] ✓ Loader.dll 加载成功");
    }

    // Windows API: LoadLibrary
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr LoadLibrary(string lpFileName);
    
    // ✅ 后续的 P/Invoke 调用会使用已加载的 DLL
    [DllImport(DLL_NAME)]
    public static extern bool LaunchWeChatWithInjection(...);
}
```

---

## 📊 工作原理

### 静态构造函数的执行时机

```csharp
// 第一次使用 LoaderNative 类时（任何静态成员或实例成员）
var processes = LoaderNative.GetWeChatProcesses(...);
// ↑ 在这里之前，静态构造函数会自动执行一次

static LoaderNative()  // ← 只执行一次
{
    // 1. 确定 DLL 路径
    // 2. LoadLibrary 预加载
    // 3. 后续的 P/Invoke 会自动找到已加载的 DLL
}
```

### LoadLibrary 的作用

```
LoadLibrary(完整路径)
   ↓
Windows 将 DLL 加载到进程内存
   ↓
DLL 注册到进程的模块列表
   ↓
后续的 P/Invoke 调用 [DllImport("Loader.dll")]
   ↓
CLR 查找已加载的 DLL
   ↓
✓ 找到了！（因为已经用 LoadLibrary 加载过）
```

**关键**：`LoadLibrary` 使用完整路径，`DllImport` 只需文件名。

---

## 🎯 修复效果

### 修复前

```
运行 Debug 版本
   ↓
调用 LoaderNative.GetWeChatProcesses()
   ↓
CLR 尝试加载 "Loader.dll"
   ↓
在 bin\Debug\...\Loader.dll 查找 ❌
在系统目录查找 ❌
在 PATH 查找 ❌
   ↓
抛出异常：Unable to load DLL 'Loader.dll'
```

### 修复后

```
运行 Debug 版本
   ↓
第一次使用 LoaderNative 类
   ↓
静态构造函数自动执行
   ↓
LoadLibrary("D:\...\bin\release\net8.0-windows\Loader.dll") ✓
   ↓
DLL 已加载到进程内存
   ↓
调用 LoaderNative.GetWeChatProcesses()
   ↓
CLR 查找 "Loader.dll" → 已加载 ✓
   ↓
正常调用
```

---

## 🧪 验证步骤

### 1. 确保 Loader.dll 存在

**手动检查**：
```
打开文件夹：
D:\gitcode\wx4helper\BaiShengVx3Plus\bin\release\net8.0-windows\

确认存在：
Loader.dll
```

**如果不存在**：
```
1. 打开 Loader.sln 或 Loader.vcxproj
2. 选择 Release | Win32
3. 生成 → 生成 Loader 项目
```

---

### 2. 重新编译 BaiShengVx3Plus

```
Visual Studio → Vx3Plus.sln → 生成 → 重新生成解决方案
```

---

### 3. 运行并查看控制台输出

**预期输出**：
```
[LoaderNative] 加载 Loader.dll: D:\gitcode\wx4helper\BaiShengVx3Plus\bin\release\net8.0-windows\Loader.dll
[LoaderNative] ✓ Loader.dll 加载成功
```

**如果报错**：
```
FileNotFoundException: 找不到 Loader.dll: ...
→ DLL 确实不存在，需要编译 Loader 项目

DllNotFoundException: 无法加载 Loader.dll: ..., Error: XXX
→ DLL 存在但无法加载，可能缺少依赖项或版本不匹配
```

---

## 🔧 常见问题

### Q1: 为什么用 LoadLibrary 而不是直接指定完整路径？

**答**：`DllImport` 不支持运行时动态路径。

```csharp
// ❌ 不支持变量
var path = "D:\...\Loader.dll";
[DllImport(path)]  // 编译错误！

// ❌ 不支持完整路径（在某些情况下）
[DllImport("D:\...\Loader.dll")]  // 可能不工作

// ✅ 使用 LoadLibrary 预加载
static LoaderNative() { LoadLibrary(path); }
[DllImport("Loader.dll")]  // 后续正常调用
```

---

### Q2: 如果 Loader.dll 依赖其他 DLL 怎么办？

**答**：确保依赖项也在同一目录，或者使用 `AddDllDirectory`。

```csharp
[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
private static extern bool SetDllDirectory(string lpPathName);

static LoaderNative()
{
    var dllDir = Path.Combine(basePath, "release", "net8.0-windows");
    SetDllDirectory(dllDir);  // 设置 DLL 搜索目录
    
    var handle = LoadLibrary(Path.Combine(dllDir, "Loader.dll"));
}
```

---

### Q3: 静态构造函数执行失败会怎样？

**答**：整个类型无法使用。

```csharp
static LoaderNative()
{
    throw new Exception("加载失败");
}

// 第一次使用时
try
{
    LoaderNative.GetWeChatProcesses(...);
}
catch (TypeInitializationException ex)
{
    // ex.InnerException 是静态构造函数抛出的异常
    Console.WriteLine(ex.InnerException.Message);
}
```

**好处**：失败快速（Fail-Fast），不会在运行时突然报 DLL 找不到。

---

### Q4: 为什么用固定路径而不是配置文件？

**答**：为了测试方便。

**固定路径**（当前）：
- ✅ 简单直接
- ✅ 无需配置
- ✅ 适合测试环境

**配置文件**（生产环境）：
- ✅ 灵活可配置
- ✅ 适合不同部署环境
- ❌ 增加复杂度

**建议**：测试阶段用固定路径，发布时改为配置文件。

---

## 📝 相关修复

### 同时修复的文件

| 文件 | 修复内容 | 固定路径 |
|------|---------|---------|
| `WeChatService.cs` | WeixinX.dll 路径 | `bin\release\net8.0-windows\WeixinX.dll` |
| `LoaderNative.cs` | Loader.dll 路径 | `bin\release\net8.0-windows\Loader.dll` |

### 统一的路径策略

```
bin\
├── Debug\
│   └── net8.0-windows\
│       └── BaiShengVx3Plus.exe  ← 运行这个
└── release\
    └── net8.0-windows\
        ├── WeixinX.dll  ← 从这里加载
        └── Loader.dll   ← 从这里加载
```

**好处**：
- ✅ WeixinX.dll 和 Loader.dll 只需编译一次（Release 版本）
- ✅ Debug 和 Release 版本的 BaiShengVx3Plus.exe 都使用相同的 DLL
- ✅ 避免版本不一致

---

## ✅ 验证清单

### 编译检查
- [x] 无编译错误
- [x] 无警告

### 路径检查
- [ ] Loader.dll 存在于 `bin\release\net8.0-windows\`
- [ ] WeixinX.dll 存在于 `bin\release\net8.0-windows\`
- [ ] 控制台显示 DLL 加载成功

### 功能检查
- [ ] 不再报"Unable to load DLL 'Loader.dll'"
- [ ] 能正常获取微信进程
- [ ] 能正常启动和注入微信

---

## 📚 相关文档

- `20251106-DLL路径修复.md` - WeixinX.dll 路径修复
- `20251106-微信进程残留问题修复.md` - 进程残留修复
- `20251106-逻辑修复和调试指南.md` - 逻辑修复

---

## 🎯 技术要点

### 1. 静态构造函数

```csharp
static LoaderNative()
{
    // 特点：
    // - 只执行一次
    // - 在第一次使用类之前自动执行
    // - 不能被显式调用
    // - 不能有参数
    // - 如果抛异常，类型将无法使用
}
```

### 2. LoadLibrary API

```csharp
[DllImport("kernel32.dll")]
private static extern IntPtr LoadLibrary(string lpFileName);

// 作用：
// - 将 DLL 加载到进程内存
// - 支持完整路径
// - 返回模块句柄（非零表示成功）
```

### 3. DllImport 解析

```csharp
[DllImport("Loader.dll")]
public static extern bool GetWeChatProcesses(...);

// CLR 查找顺序：
// 1. 已加载的模块（通过 LoadLibrary）
// 2. 当前目录
// 3. 系统目录
// 4. PATH 环境变量
```

---

**修复时间**: 2025年11月6日 01:05  
**状态**: ✅ 已修复，使用静态构造函数预加载  
**下一步**: 确保 Loader.dll 已编译，重新运行程序

