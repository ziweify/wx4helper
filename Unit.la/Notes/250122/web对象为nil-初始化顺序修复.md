# web 对象为 nil 问题修复

> **问题时间**: 2026-01-22  
> **修复时间**: 2026-01-22  
> **状态**: ✅ 已修复

---

## 🐛 问题描述

### 错误信息

```
chunk_1:(9,4-28): attempt to index a nil value

脚本第9行: local url = web.GetUrl()
```

### 错误原因

**`web` 对象是 nil！**

脚本尝试调用 `web.GetUrl()`，但 `web` 对象根本不存在（为 nil）。

---

## 🔍 根本原因

### 初始化顺序错误

```csharp
public BrowserTaskControl(BrowserTaskConfig config)
{
    InitializeComponent();
    
    // ❌ 错误顺序
    RegisterDefaultFunctions();  // 64行: 注册函数（包括创建 WebBridge）
    InitializeControls();        // 67行: 绑定函数到引擎
    InitializeWebView();         // 70行: 初始化 _webView
}
```

**问题**：

1. **第64行** - `RegisterDefaultFunctions()` 被调用
2. 在这个方法中，尝试创建 `WebBridge(webView, logCallback)`
3. **但是！**`_webView` 在第70行才初始化，现在还是 **null**！
4. `ScriptFunctionRegistry.RegisterDefaults(LogMessage, _webView)` 收到 `null`
5. 所以 `web` 对象没有被注册到 Lua 环境
6. 脚本中调用 `web.GetUrl()` → **nil value!**

### 代码证据

```csharp
// Unit.la/Scripting/ScriptFunctionRegistry.cs
public void RegisterDefaults(Action<string>? logCallback = null, Microsoft.Web.WebView2.WinForms.WebView2? webView = null)
{
    // ... 注册其他函数 ...
    
    // 🌐 注册 WebView2 桥接对象
    if (webView != null)  // ❌ webView 是 null，所以这个块不执行！
    {
        var webBridge = new WebBridge(webView, logCallback);
        RegisterObject("web", webBridge);
    }
}
```

---

## ✅ 修复方案

### 调整初始化顺序

```csharp
public BrowserTaskControl(BrowserTaskConfig config)
{
    InitializeComponent();
    
    // ✅ 正确顺序
    InitializeWebView();         // 1️⃣ 先初始化 WebView2
    RegisterDefaultFunctions();  // 2️⃣ 再注册函数（这时 _webView 已经不是 null）
    InitializeControls();        // 3️⃣ 最后绑定函数到引擎
}
```

**修复后的流程**：

1. **InitializeWebView()** - `_webView` 被创建 ✅
2. **RegisterDefaultFunctions()** - 传入有效的 `_webView` ✅
3. `ScriptFunctionRegistry.RegisterDefaults(LogMessage, _webView)` - `webView != null` ✅
4. `WebBridge` 被成功创建 ✅
5. `web` 对象被注册到 Lua 环境 ✅
6. 脚本可以正常调用 `web.GetUrl()` ✅

---

## 🧪 测试验证

### Before（修复前）

**脚本**:
```lua
function main()
    local url = web.GetUrl()  -- ❌ 第9行
    log(url)
    return true
end
```

**结果**:
```
❌ chunk_1:(9,4-28): attempt to index a nil value
原因: web 对象为 nil
```

### After（修复后）

**脚本**:
```lua
function main()
    local url = web.GetUrl()  -- ✅ 正常工作
    log(url)
    return true
end
```

**结果**:
```
✅ 脚本执行成功
📤 输出: https://yb1s68531569885o.117a.me/
```

---

## 📊 初始化顺序对比

### Before（错误）

```
┌──────────────────────────────┐
│ InitializeComponent()        │
├──────────────────────────────┤
│ RegisterDefaultFunctions()   │  ❌ _webView = null
│   ├─ RegisterDefaults()      │     webView = null
│   │   └─ if (webView != null)│     跳过！
│   └─ web 对象未注册          │
├──────────────────────────────┤
│ InitializeControls()         │
│   └─ 绑定函数到引擎          │     web 对象不存在
├──────────────────────────────┤
│ InitializeWebView()          │  ✅ _webView 才被创建（太晚了！）
└──────────────────────────────┘
```

### After（正确）

```
┌──────────────────────────────┐
│ InitializeComponent()        │
├──────────────────────────────┤
│ InitializeWebView()          │  ✅ _webView 被创建
├──────────────────────────────┤
│ RegisterDefaultFunctions()   │  ✅ _webView 有效
│   ├─ RegisterDefaults()      │     webView 有效
│   │   ├─ if (webView != null)│     执行！
│   │   ├─ new WebBridge()     │     创建成功
│   │   └─ RegisterObject("web")│    注册成功
│   └─ web 对象已注册          │
├──────────────────────────────┤
│ InitializeControls()         │
│   └─ 绑定函数到引擎          │     web 对象存在 ✅
└──────────────────────────────┘
```

---

## 🎯 经验教训

### 1. 依赖关系很重要

初始化顺序必须遵循依赖关系：

```
_webView (基础)
    ↓
WebBridge (依赖 _webView)
    ↓
ScriptEngine (依赖 WebBridge)
```

### 2. null 检查是有原因的

代码中的 `if (webView != null)` 不是装饰，而是**真的会遇到 null 的情况**！

### 3. 异步初始化要小心

WebView2 初始化可能是异步的，但函数注册必须在之后进行。

---

## ✅ 修改的文件

### Unit.la/Controls/BrowserTaskControl.cs

**位置**: 构造函数（第 57-82 行）

**修改内容**: 调整初始化顺序

```csharp
// Before
InitializeComponent();
RegisterDefaultFunctions();  // ❌ _webView 还是 null
InitializeControls();
InitializeWebView();         // 太晚了

// After
InitializeComponent();
InitializeWebView();         // ✅ 先初始化
RegisterDefaultFunctions();  // ✅ 再注册（_webView 有效）
InitializeControls();
```

---

## 🔧 编译状态

```
✅ Unit.la - 编译成功
✅ 初始化顺序已修复
✅ web 对象现在可以正常使用
```

---

## 🎉 总结

### 问题
- ❌ `web.GetUrl()` 报错 "attempt to index a nil value"
- ❌ `web` 对象为 nil
- ❌ 初始化顺序错误

### 原因
- `RegisterDefaultFunctions()` 在 `InitializeWebView()` 之前调用
- `_webView` 还是 null
- `WebBridge` 没有被创建
- `web` 对象没有被注册

### 修复
- ✅ 调整初始化顺序：先 WebView，再注册，最后绑定
- ✅ `_webView` 在注册函数时已经有效
- ✅ `WebBridge` 成功创建
- ✅ `web` 对象成功注册到 Lua

### 效果
- ✅ 脚本可以正常调用 `web.GetUrl()`
- ✅ 所有 web 函数都可用
- ✅ config 对象也正常工作

---

**修复完成时间**: 2026-01-22  
**状态**: ✅ 已修复并验证  
**编译状态**: ✅ 成功

---

**© 2026 Unit.la Bug Fix Report**
