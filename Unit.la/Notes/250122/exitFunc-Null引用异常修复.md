# exitFunc Null 引用异常修复

> **问题时间**: 2026-01-22  
> **修复时间**: 2026-01-22  
> **状态**: ✅ 已修复

---

## 🐛 问题描述

### 异常信息

```
System.Exception: attempt to call a nil value
exit() 函数执行失败: Object reference not set to an instance of an object.
   at Unit.La.Controls.BrowserTaskControl.ExecuteScriptAsync(String script)
   at Unit.La.Scripting.MoonSharpScriptEngine.ExecuteWithLifecycle(String scriptCode) line 218
```

### 触发场景

当脚本缺少必须的函数（如 main, error, exit）时：
1. 验证失败，提前 return
2. `exitFunc` 还是 null
3. finally 块尝试调用 `exitFunc`
4. 导致 NullReferenceException

---

## 🔍 根本原因

### 代码流程

```csharp
DynValue? exitFunc = null;  // 外层声明

try
{
    // 1. 加载脚本
    _script.DoString(scriptCode);
    
    // 2. 获取函数引用
    var mainFunc = _script.Globals.Get("main");
    var errorFunc = _script.Globals.Get("error");
    exitFunc = _script.Globals.Get("exit");  // 可能获取失败
    
    // 3. 验证函数
    if (缺少函数)
    {
        return new ScriptResult { ... };  // ❌ 提前返回！
    }
    
    // 4. 执行 main()
    // ...
}
finally
{
    // ❌ 这里 exitFunc 可能是 null！
    _script.Call(exitFunc);  // NullReferenceException!
}
```

### 问题核心

**验证失败时**：
- `exitFunc` 可能是 nil（函数不存在）
- `exitFunc` 可能是 null（脚本加载失败）
- finally 块没有检查就直接调用
- 导致二次异常，掩盖了原始错误

---

## ✅ 修复方案

### 添加 null 和 nil 检查

```csharp
finally
{
    // 5. 无论如何，调用 exit() 函数（如果已成功加载）
    try
    {
        // 🔥 检查 exitFunc 是否为 null 且是有效的函数
        if (exitFunc != null && !exitFunc.IsNil() && exitFunc.Type == DataType.Function)
        {
            _script.Call(exitFunc);
        }
    }
    catch (Exception exitEx)
    {
        // exit() 函数出错，记录但不影响最终结果
        if (hasError)
        {
            errorMessage = $"{errorMessage}\nexit() 函数执行失败: {exitEx.Message}";
        }
        else
        {
            hasError = true;
            errorMessage = $"exit() 函数执行失败: {exitEx.Message}";
        }
    }
}
```

### 三重检查

1. **`exitFunc != null`** - C# null 检查
2. **`!exitFunc.IsNil()`** - MoonSharp nil 检查
3. **`exitFunc.Type == DataType.Function`** - 类型检查

---

## 🧪 测试场景

### 场景1: 缺少 exit() 函数

**脚本**:
```lua
function main()
    log('主逻辑')
    return true
end

function error(errorInfo)
    return false
end

-- ❌ 没有 exit() 函数
```

**Before（修复前）**:
```
❌ 脚本不符合规范！必须包含以下3个函数：
  - function exit()

❌ 二次异常：
exit() 函数执行失败: Object reference not set to an instance of an object.
```

**After（修复后）**:
```
❌ 脚本不符合规范！必须包含以下3个函数：
  - function exit()

✅ 只显示原始错误，不会产生二次异常
```

---

### 场景2: 脚本加载失败

**脚本**:
```lua
-- 语法错误
function main(
    -- ❌ 缺少结束括号和 end
```

**Before（修复前）**:
```
❌ 语法错误: unexpected symbol near 'end'

❌ 二次异常：
exit() 函数执行失败: Object reference not set to an instance of an object.
```

**After（修复后）**:
```
❌ 语法错误: unexpected symbol near 'end'

✅ 只显示原始错误，不会产生二次异常
```

---

### 场景3: 正常执行（3个函数齐全）

**脚本**:
```lua
function main()
    log('主逻辑')
    return true
end

function error(errorInfo)
    return false
end

function exit()
    log('清理完成')
end
```

**Before & After（都正常）**:
```
▶️ 开始执行脚本...
[14:30:00.123] 主逻辑
[14:30:00.124] 清理完成
✅ 脚本执行成功
```

---

## 📊 修复效果

| 场景 | Before | After |
|------|--------|-------|
| **缺少 exit()** | ❌ 二次异常 | ✅ 只显示原始错误 |
| **语法错误** | ❌ 二次异常 | ✅ 只显示原始错误 |
| **加载失败** | ❌ 二次异常 | ✅ 只显示原始错误 |
| **正常执行** | ✅ 正常 | ✅ 正常 |

---

## 🎯 修复原则

### 防御性编程

在 finally 块中：
1. **永远不要假设变量已初始化**
2. **永远不要假设对象不是 null**
3. **永远不要假设 DynValue 不是 nil**
4. **永远检查类型是否正确**

### 错误处理优先级

1. **优先显示原始错误** - 不要让二次错误掩盖
2. **记录但不抛出二次错误** - 在 catch 中记录到 errorMessage
3. **保证 finally 块的健壮性** - 不能在清理时产生新错误

---

## ✅ 修改的文件

### Unit.la/Scripting/MoonSharpScriptEngine.cs

**位置**: 第 213-233 行

**修改内容**: 在 finally 块中添加三重检查

```csharp
// Before
finally
{
    try
    {
        _script.Call(exitFunc);  // ❌ 没有检查
    }
    catch (Exception exitEx)
    {
        // ...
    }
}

// After
finally
{
    try
    {
        // ✅ 三重检查
        if (exitFunc != null && !exitFunc.IsNil() && exitFunc.Type == DataType.Function)
        {
            _script.Call(exitFunc);
        }
    }
    catch (Exception exitEx)
    {
        // ...
    }
}
```

---

## 🔧 编译状态

```
✅ Unit.la - 编译成功
✅ YongLiSystem - 编译成功
✅ Null 引用异常已修复
```

---

## 📝 经验教训

### 1. finally 块必须健壮

finally 块用于清理，**绝对不能产生新的异常**，否则会掩盖原始错误。

### 2. DynValue 的双重 null

在 MoonSharp 中：
- C# 层面可能是 `null`
- Lua 层面可能是 `nil`
- 需要同时检查两者

### 3. 验证和执行分离

如果验证失败提前返回：
- 确保 finally 块能正确处理
- 不要假设所有变量都已赋值

---

## 🎉 总结

### 问题
- ❌ 验证失败时，finally 块调用 null 的 exitFunc
- ❌ 产生二次异常，掩盖原始错误

### 修复
- ✅ 添加三重检查：null + nil + type
- ✅ finally 块健壮性提升
- ✅ 原始错误信息清晰显示

### 效果
- ✅ 不再产生二次异常
- ✅ 错误提示更加准确
- ✅ 代码更加健壮

---

**修复完成时间**: 2026-01-22  
**状态**: ✅ 已修复并验证  
**编译状态**: ✅ 成功

---

**© 2026 Unit.la Bug Fix Report**
