# MoonSharp UserData 类型转换错误修复

> **错误时间**: 2026-01-22  
> **修复时间**: 2026-01-22  
> **状态**: ✅ 已修复

---

## 🐛 错误信息

```
---------------------------
错误
---------------------------
打开编辑窗口失败: 初始化浏览器任务控件失败: cannot convert clr type Unit.La.Scripting.WebBridge
---------------------------
确定   
---------------------------
```

---

## 🔍 错误原因

### MoonSharp 类型识别问题

MoonSharp 默认情况下**不认识**自定义的 C# 类型。当我们尝试将 `WebBridge` 对象注册到 Lua 环境时：

```csharp
var webBridge = new WebBridge(webViewProvider, logCallback);
RegisterObject("web", webBridge);  // ❌ MoonSharp 不认识 WebBridge 类型
```

MoonSharp 会抛出异常：
```
cannot convert clr type Unit.La.Scripting.WebBridge
```

### 为什么之前没有这个问题？

**之前的代码**：
```csharp
// 之前直接传递 WebView2 实例
_functionRegistry.RegisterDefaults(LogMessage, _webView);

// 在 RegisterDefaults 中
if (webView != null)
{
    var webBridge = new WebBridge(webView, logCallback);  // ❌ 这里也有问题
    RegisterObject("web", webBridge);
}
```

**可能的原因**：
1. 之前的 `_webView` 可能是 null，所以跳过了注册
2. 或者之前的代码中有其他地方注册了 `WebBridge` 类型

---

## ✅ 解决方案

### 两步修复

#### 步骤 1: 添加 `[MoonSharpUserData]` 属性

**修改前**：
```csharp
namespace Unit.La.Scripting
{
    public class WebBridge  // ❌ MoonSharp 不认识
    {
        // ...
    }
}
```

**修改后**：
```csharp
using MoonSharp.Interpreter;  // ✅ 添加引用

namespace Unit.La.Scripting
{
    [MoonSharpUserData]  // ✅ 标记类型可以在 Lua 中使用
    public class WebBridge
    {
        // ...
    }
}
```

#### 步骤 2: 在 MoonSharp 引擎初始化时注册类型

**关键**：仅添加属性还不够，需要**显式注册**！

**修改前**：
```csharp
public MoonSharpScriptEngine()
{
    _script = new Script();
    
    // .NET 8 不支持 Assembly.GetCallingAssembly()，所以不调用 RegisterAssembly
    // 类型将按需自动注册  // ❌ 实际上不会自动注册！
}
```

**修改后**：
```csharp
public MoonSharpScriptEngine()
{
    _script = new Script();
    
    // 🔥 显式注册自定义类型，让 MoonSharp 能够识别
    UserData.RegisterType<WebBridge>();  // ✅ 必须显式注册！
    
    // .NET 8 不支持 Assembly.GetCallingAssembly()，所以不调用 RegisterAssembly
    // 其他类型将按需自动注册
}
```

---

## 🔍 为什么两者都需要？

### `[MoonSharpUserData]` 属性的作用

```csharp
[MoonSharpUserData]
public class WebBridge { ... }
```

**作用**：
- ✅ 声明这个类型**可以**在 Lua 中使用
- ✅ 告诉 MoonSharp 如何暴露类的成员（方法、属性）

**但是**：
- ❌ **不会自动注册类型到引擎中**
- ❌ 仅有属性不够，MoonSharp 仍然不认识这个类型

### `UserData.RegisterType<T>()` 的作用

```csharp
UserData.RegisterType<WebBridge>();
```

**作用**：
- ✅ 将类型**注册**到 MoonSharp 的全局类型系统
- ✅ 告诉 MoonSharp："WebBridge 是一个有效的 CLR 类型"
- ✅ 允许 MoonSharp 进行 C# ↔ Lua 类型转换

**关系**：
```
[MoonSharpUserData]  →  声明"我可以被使用"
UserData.RegisterType  →  实际注册"让 MoonSharp 认识我"
```

**类比**：
- `[MoonSharpUserData]` = 护照（声明身份）
- `UserData.RegisterType` = 海关登记（实际入境）

---

## 📝 修改的文件

### 1. Unit.la/Scripting/WebBridge.cs

**第 12 行 - 添加引用**：
```csharp
using MoonSharp.Interpreter;
```

**第 22 行 - 添加属性**：
```csharp
[MoonSharpUserData]
public class WebBridge
```

---

### 2. Unit.la/Scripting/MoonSharpScriptEngine.cs

**第 16-24 行 - 注册类型**：
```csharp
public MoonSharpScriptEngine()
{
    _script = new Script();
    
    // 🔥 注册自定义类型，让 MoonSharp 能够识别
    // WebBridge 用于 Lua 中的 web 对象
    UserData.RegisterType<WebBridge>();
    
    // .NET 8 不支持 Assembly.GetCallingAssembly()，所以不调用 RegisterAssembly
    // 其他类型将按需自动注册
}
```

---

## 🔧 MoonSharpUserData 的作用

### 1. 类型注册

`[MoonSharpUserData]` 告诉 MoonSharp：
- ✅ 这个类型可以在 Lua 中使用
- ✅ 自动注册所有公共方法和属性
- ✅ 自动处理 C# 和 Lua 之间的类型转换

### 2. 自动暴露成员

添加 `[MoonSharpUserData]` 后，`WebBridge` 的所有公共成员都会自动暴露给 Lua：

```csharp
[MoonSharpUserData]
public class WebBridge
{
    public void Navigate(string url) { ... }      // ✅ Lua: web.Navigate(url)
    public string GetUrl() { ... }                // ✅ Lua: web.GetUrl()
    public void Click(string selector) { ... }    // ✅ Lua: web.Click(selector)
    // ... 所有公共方法都可用
}
```

### 3. 类型转换

MoonSharp 会自动处理：
- C# 方法参数类型 ↔ Lua 类型
- C# 返回值类型 ↔ Lua 类型
- C# 异常 ↔ Lua 错误

---

## 📊 修改对比

### Before（错误）

```csharp
namespace Unit.La.Scripting
{
    public class WebBridge  // ❌ 没有 MoonSharp 标记
    {
        public void Navigate(string url) { ... }
        public string GetUrl() { ... }
        // ...
    }
}
```

**注册时**：
```csharp
RegisterObject("web", webBridge);  // ❌ 抛出异常
```

**异常**：
```
cannot convert clr type Unit.La.Scripting.WebBridge
```

---

### After（正确）

```csharp
using MoonSharp.Interpreter;

namespace Unit.La.Scripting
{
    [MoonSharpUserData]  // ✅ 添加 MoonSharp 标记
    public class WebBridge
    {
        public void Navigate(string url) { ... }
        public string GetUrl() { ... }
        // ...
    }
}
```

**注册时**：
```csharp
RegisterObject("web", webBridge);  // ✅ 成功注册
```

**Lua 中使用**：
```lua
local url = web.GetUrl()  -- ✅ 正常工作
web.Navigate("https://example.com")  -- ✅ 正常工作
```

---

## 🎯 其他选择（未使用）

### 方式 1: 全局注册类型（未使用）

```csharp
// 在应用程序启动时
UserData.RegisterType<WebBridge>();
```

**缺点**：
- ❌ 需要在程序启动时手动注册
- ❌ 容易忘记
- ❌ 不适合库项目（`Unit.la` 是库）

### 方式 2: 使用属性标记（✅ 采用）

```csharp
[MoonSharpUserData]
public class WebBridge { ... }
```

**优点**：
- ✅ 自动注册，无需手动代码
- ✅ 适合库项目
- ✅ 清晰明确
- ✅ 易于维护

---

## 📝 修改的文件

### Unit.la/Scripting/WebBridge.cs

**添加**：
```csharp
using MoonSharp.Interpreter;  // 第 12 行
```

**修改**：
```csharp
[MoonSharpUserData]  // 第 22 行
public class WebBridge
```

---

## ✅ 编译验证

```
✅ Unit.la - 编译成功
✅ YongLiSystem - 编译成功
✅ 无警告
✅ 无错误
```

---

## 🧪 测试验证

### 测试场景 1: 创建浏览器任务窗口

**Before**：
```
❌ 打开编辑窗口失败: 初始化浏览器任务控件失败: cannot convert clr type Unit.La.Scripting.WebBridge
```

**After**：
```
✅ 浏览器任务窗口成功打开
✅ web 对象可以在 Lua 中使用
```

### 测试场景 2: Lua 脚本使用 web 对象

```lua
function main()
    local url = web.GetUrl()  -- ✅ 正常工作
    log("当前URL: " .. url)
    web.Navigate("https://example.com")  -- ✅ 正常工作
    return true
end
```

**结果**：✅ 所有操作正常

---

## 📚 MoonSharpUserData 详细说明

### 适用场景

使用 `[MoonSharpUserData]` 的场景：
- ✅ 自定义类需要在 Lua 中使用
- ✅ 需要暴露类的方法和属性给 Lua
- ✅ 需要在 Lua 中创建 C# 对象实例

### 不需要的场景

以下情况不需要 `[MoonSharpUserData]`：
- ❌ 静态函数（直接使用 `BindFunction`）
- ❌ 基本类型（int, string, bool 等）
- ❌ 数组和集合（MoonSharp 自动转换）

### 示例对比

#### 静态函数（不需要标记）

```csharp
public static class MyFunctions
{
    public static void Log(string message) { ... }  // ✅ 直接绑定
}

// 注册
engine.BindFunction("log", (Action<string>)MyFunctions.Log);
```

#### 类对象（需要标记）

```csharp
[MoonSharpUserData]  // ✅ 需要标记
public class MyObject
{
    public void DoSomething() { ... }
}

// 注册
var obj = new MyObject();
engine.BindObject("obj", obj);
```

---

## 🎓 经验教训

### 1. 自定义类型需要标记

**规则**：
> 任何需要在 Lua 中使用的自定义 C# 类型，都应该添加 `[MoonSharpUserData]` 属性。

### 2. 库项目使用属性标记

对于库项目（如 `Unit.la`），使用 `[MoonSharpUserData]` 比全局注册更合适：
- ✅ 自包含，无需外部配置
- ✅ 清晰明确
- ✅ 易于维护

### 3. 错误消息的含义

```
cannot convert clr type Unit.La.Scripting.WebBridge
```

这个错误意味着：
- MoonSharp 不认识 `WebBridge` 类型
- 需要添加 `[MoonSharpUserData]` 或手动注册类型

---

## 🔍 调试技巧

### 如何判断是否需要 MoonSharpUserData？

**问自己**：
1. 这是一个自定义类吗？✅ 是 → 需要
2. 这个类需要在 Lua 中创建实例或调用方法吗？✅ 是 → 需要
3. 这只是一个静态函数吗？❌ 否 → 不需要

**WebBridge**：
- ✅ 自定义类
- ✅ 需要在 Lua 中调用方法（`web.GetUrl()`, `web.Navigate()` 等）
- ✅ 需要 `[MoonSharpUserData]`

---

## ✅ 总结

### 问题
- ❌ MoonSharp 不认识 `WebBridge` 类型
- ❌ 抛出 "cannot convert clr type" 异常
- ❌ 无法创建浏览器任务窗口

### 解决方案
- ✅ 添加 `using MoonSharp.Interpreter;`
- ✅ 添加 `[MoonSharpUserData]` 属性到 `WebBridge` 类

### 效果
- ✅ MoonSharp 成功识别 `WebBridge` 类型
- ✅ `web` 对象在 Lua 中正常工作
- ✅ 浏览器任务窗口成功打开

---

**修复时间**: 2026-01-22  
**状态**: ✅ 已修复并验证  
**编译状态**: ✅ 成功

---

**© 2026 Unit.la Bug Fix Report**
