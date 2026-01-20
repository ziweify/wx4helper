# Unit.la - 脚本编辑器控件库

## 📦 简介

`Unit.la` 是一个可复用的脚本编辑器控件库，提供完整的 Lua 脚本编辑、调试和功能绑定能力。

**设计理念**：开箱即用，即拿即用，高度封装，可在多个项目中复用。

## ✨ 特性

- ✅ **语法高亮**：Lua 语法高亮显示
- ✅ **断点调试**：点击左边距设置/清除断点
- ✅ **实时验证**：自动检测语法错误
- ✅ **错误标记**：错误行自动标记和提示
- ✅ **自动完成**：智能代码补全
- ✅ **代码折叠**：支持代码块折叠
- ✅ **行号显示**：可配置的行号显示
- ✅ **查找替换**：内置查找替换功能
- ✅ **功能绑定**：轻松绑定 C# 函数和对象到脚本

## 🚀 快速开始

### 1. 添加项目引用

在项目文件中添加：

```xml
<ProjectReference Include="..\Unit.la\Unit.la.csproj" />
```

### 2. 在设计器中使用

1. 打开 Visual Studio 设计器
2. 在工具箱中找到 `ScriptEditorControl`
3. 拖放到窗体或用户控件
4. 完成！所有功能已自动初始化

### 3. 代码中使用

```csharp
using Unit.La.Controls;
using Unit.La.Scripting;

// 获取控件（如果在设计器中已添加）
var editor = scriptEditorControl1;

// 设置脚本内容
editor.ScriptText = @"
function hello(name)
    return 'Hello, ' .. name
end
";

// 绑定函数
editor.BindFunction("print", new Action<string>(Console.WriteLine));

// 执行脚本
var result = editor.ExecuteScript(new Dictionary<string, object>
{
    { "name", "World" }
});

if (result.Success)
{
    Console.WriteLine($"执行成功: {result.Output}");
}
else
{
    Console.WriteLine($"执行失败: {result.Error}");
}
```

## 📚 API 文档

### ScriptEditorControl 控件

#### 属性

- `ScriptText` - 获取或设置脚本内容
- `EnableRealTimeValidation` - 是否启用实时验证（默认：true）
- `ValidationDelay` - 验证延迟时间（毫秒，默认：500）
- `ShowLineNumbers` - 是否显示行号（默认：true）
- `EnableBreakpoints` - 是否启用断点（默认：true）
- `FontSize` - 字体大小（默认：10）

#### 方法

- `ExecuteScript(context)` - 执行脚本
- `ValidateScript()` - 验证脚本语法
- `BindFunction(name, function)` - 绑定函数
- `BindObject(name, obj)` - 绑定对象
- `SetBreakpoint(lineNumber)` - 设置断点
- `ClearBreakpoint(lineNumber)` - 清除断点
- `ClearAllBreakpoints()` - 清除所有断点
- `FindText(text, matchCase, wholeWord)` - 查找文本
- `ReplaceText(findText, replaceText, matchCase)` - 替换文本
- `SetScriptEngine(engine)` - 设置自定义脚本引擎

#### 事件

- `ScriptTextChanged` - 脚本内容变更
- `OnValidationError` - 验证错误
- `OnValidationSuccess` - 验证成功
- `OnError` - 执行错误
- `OnBreakpointHit` - 断点命中

### IScriptEngine 接口

脚本引擎接口，支持自定义实现：

```csharp
public interface IScriptEngine
{
    ScriptResult Execute(string script, Dictionary<string, object>? context = null);
    ScriptValidationResult Validate(string script);
    void BindFunction(string name, Delegate function);
    void BindObject(string name, object obj);
    void SetBreakpoint(int lineNumber);
    void ClearBreakpoint(int lineNumber);
    event EventHandler<ScriptDebugEventArgs>? OnBreakpoint;
    event EventHandler<ScriptErrorEventArgs>? OnError;
}
```

### MoonSharpScriptEngine

默认的 Lua 脚本引擎实现（基于 MoonSharp）。

### ScriptFunctionRegistry

功能注册表，用于管理可绑定到脚本的功能：

```csharp
// 注册功能
ScriptFunctionRegistry.Instance.RegisterFunction(
    "print",
    new Action<string>(Console.WriteLine),
    "打印文本到控制台",
    "print('Hello')"
);

// 绑定到脚本引擎
var engine = new MoonSharpScriptEngine();
ScriptFunctionRegistry.Instance.BindToEngine(engine);
```

## 🏗️ 项目结构

```
Unit.la/
├── Unit.la.csproj          # 项目文件
├── Scripting/
│   ├── IScriptEngine.cs           # 脚本引擎接口
│   ├── MoonSharpScriptEngine.cs   # MoonSharp 实现
│   └── ScriptFunctionRegistry.cs  # 功能注册表
├── Controls/
│   ├── ScriptEditorControl.cs     # 脚本编辑器控件
│   └── ScriptEditorControl.Designer.cs  # 设计器文件
└── README.md               # 本文档
```

## 📦 依赖

- **MoonSharp** (2.0.0) - Lua 脚本引擎
- **ScintillaNET** (5.3.0) - 代码编辑器控件
- **.NET 8.0** - 目标框架
- **Windows Forms** - UI 框架

## 🔧 使用场景

1. **脚本任务配置**：在配置对话框中编辑脚本
2. **自动化脚本**：编写和执行自动化任务脚本
3. **规则引擎**：定义和执行业务规则
4. **插件系统**：允许用户编写自定义插件脚本

## 📝 注意事项

1. **设计器支持**：控件完全支持 Visual Studio 设计器，建议在设计器中添加和配置
2. **线程安全**：脚本执行应在后台线程进行，UI 更新应在 UI 线程
3. **错误处理**：建议订阅 `OnError` 和 `OnValidationError` 事件处理错误
4. **性能**：实时验证有延迟机制，避免频繁验证影响性能

## 🔄 版本历史

- **v1.0.0** - 初始版本
  - 基础脚本编辑器功能
  - Lua 语法支持
  - 断点调试
  - 实时验证

## 📄 许可证

本项目为内部库，仅供项目内使用。
