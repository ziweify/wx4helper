# config 对象匿名类型 - MoonSharp 不兼容修复

> **问题时间**: 2026-01-22  
> **修复时间**: 2026-01-22  
> **状态**: ✅ 已修复

---

## 🐛 问题描述

### 错误信息

```
启动任务失败: cannot convert clr type <>f__AnonymousType1`5[System.String,System.String,System.String,System.Boolean,System.String]
```

### 触发场景

当尝试启动浏览器任务时：
1. `BrowserTaskControl` 初始化
2. 注册 `config` 对象到 Lua
3. 使用 C# 匿名类型创建 config
4. MoonSharp 尝试转换匿名类型
5. ❌ 转换失败！

---

## 🔍 根本原因

### MoonSharp 的限制

**MoonSharp 不支持 C# 匿名类型！**

匿名类型在编译时生成为：
```csharp
<>f__AnonymousType1`5[System.String, System.String, System.String, System.Boolean, System.String]
```

这是编译器生成的**内部类型**，MoonSharp 无法识别和转换。

### 错误代码

```csharp
// ❌ 使用匿名类型（不支持）
var configObject = new
{
    url = _config.Url,
    username = _config.Username,
    password = _config.Password,
    autoLogin = _config.AutoLogin,
    name = _config.Name
};
_functionRegistry.RegisterObject("config", configObject);
```

---

## ✅ 修复方案

### 使用 Dictionary 替代匿名类型

```csharp
// ✅ 使用 Dictionary（完全兼容）
var configObject = new Dictionary<string, object>
{
    ["url"] = _config.Url ?? "",
    ["username"] = _config.Username ?? "",
    ["password"] = _config.Password ?? "",
    ["autoLogin"] = _config.AutoLogin,
    ["name"] = _config.Name ?? ""
};
_functionRegistry.RegisterObject("config", configObject);
```

### 为什么 Dictionary 可以？

1. **标准类型**: `Dictionary<string, object>` 是.NET 标准类型
2. **MoonSharp 原生支持**: MoonSharp 内置了 Dictionary 到 Lua table 的转换
3. **动态访问**: Lua 可以用 `config.url` 或 `config["url"]` 访问

---

## 🧪 测试验证

### Lua 脚本中使用 config

```lua
function main()
    -- ✅ 可以正常访问
    log('URL: ' .. config.url)
    log('用户名: ' .. config.username)
    log('密码: ' .. config.password)
    log('自动登录: ' .. tostring(config.autoLogin))
    log('任务名: ' .. config.name)
    
    -- ✅ 可以在 web 函数中使用
    web.Navigate(config.url)
    web.Input('#username', config.username)
    web.Input('#password', config.password)
    
    return true
end

function error(errorInfo)
    return false
end

function exit()
    log('清理完成')
end
```

### 预期输出

```
[14:30:00.123] URL: https://yb1s68531569885o.117a.me/
[14:30:00.124] 用户名: admin
[14:30:00.125] 密码: ******
[14:30:00.126] 自动登录: true
[14:30:00.127] 任务名: 任务_143000
[14:30:00.128] 🌐 导航到: https://yb1s68531569885o.117a.me/
```

---

## 📊 MoonSharp 类型兼容性

### ✅ 支持的类型

| C# 类型 | Lua 类型 | 访问方式 |
|---------|---------|---------|
| `Dictionary<string, object>` | `table` | `config.key` 或 `config["key"]` |
| `List<T>` | `table` (数组) | `list[1]` (Lua 从 1 开始) |
| `class` (具名类) | `userdata` | 需要显式绑定属性 |
| `string` | `string` | 直接访问 |
| `int`, `double`, `bool` | `number`, `boolean` | 直接访问 |

### ❌ 不支持的类型

| C# 类型 | 问题 | 解决方案 |
|---------|------|---------|
| **匿名类型** `new { }` | 编译器生成的内部类型 | 使用 `Dictionary` |
| `Tuple<T1, T2>` | 不是标准集合类型 | 使用 `List` 或 `Dictionary` |
| `ValueTuple` `(a, b)` | 同上 | 使用 `List` 或 `Dictionary` |
| `dynamic` | 类型不明确 | 使用具体类型 |

---

## 🎯 最佳实践

### 1. 使用 Dictionary 传递配置

```csharp
// ✅ 推荐
var config = new Dictionary<string, object>
{
    ["key1"] = value1,
    ["key2"] = value2
};
engine.BindObject("config", config);
```

### 2. 使用具名类（如果需要类型安全）

```csharp
// ✅ 也可以，但需要额外配置
public class ConfigData
{
    public string Url { get; set; }
    public string Username { get; set; }
}

var config = new ConfigData { Url = "...", Username = "..." };
engine.BindObject("config", config);
```

### 3. 避免使用匿名类型

```csharp
// ❌ 避免
var config = new { Url = "...", Username = "..." };
engine.BindObject("config", config); // 会失败！
```

---

## 🔧 修改的文件

### Unit.la/Controls/BrowserTaskControl.cs

**位置**: `RegisterDefaultFunctions()` 方法

**修改内容**:

```csharp
// Before (❌ 匿名类型)
var configObject = new
{
    url = _config.Url,
    username = _config.Username,
    password = _config.Password,
    autoLogin = _config.AutoLogin,
    name = _config.Name
};

// After (✅ Dictionary)
var configObject = new Dictionary<string, object>
{
    ["url"] = _config.Url ?? "",
    ["username"] = _config.Username ?? "",
    ["password"] = _config.Password ?? "",
    ["autoLogin"] = _config.AutoLogin,
    ["name"] = _config.Name ?? ""
};
```

**附加改进**:
- 添加了 `?? ""` 空值保护
- 确保不会传递 null 字符串到 Lua

---

## 📝 经验教训

### 1. MoonSharp 的类型系统

MoonSharp 是一个**轻量级**的 Lua 实现，它：
- ✅ 支持基本的 .NET 类型（string, int, bool, List, Dictionary）
- ✅ 支持具名类（通过显式配置）
- ❌ **不支持编译器生成的内部类型**（如匿名类型）

### 2. 优先使用标准集合类型

在与脚本引擎交互时：
- **Dictionary** 是最安全的选择
- **List** 用于数组
- **具名类** 仅在需要类型安全时使用

### 3. 测试类型兼容性

新增绑定时，务必测试：
```csharp
var obj = new YourType();
engine.BindObject("test", obj);
engine.Execute("log(test.property)"); // 验证是否可访问
```

---

## ✅ 修复验证

### 编译状态

```
✅ Unit.la - 编译成功
✅ 匿名类型已替换为 Dictionary
✅ 类型兼容性问题已解决
```

### 运行测试

1. **启动浏览器任务** ✅ 成功
2. **访问 config.url** ✅ 成功
3. **访问 config.username** ✅ 成功
4. **调用 web.Navigate(config.url)** ✅ 成功

---

## 🎉 总结

### 问题
- ❌ 使用 C# 匿名类型创建 config 对象
- ❌ MoonSharp 无法识别编译器生成的内部类型
- ❌ 启动任务时报错

### 修复
- ✅ 改用 `Dictionary<string, object>`
- ✅ 添加空值保护 `?? ""`
- ✅ 完全兼容 MoonSharp

### 效果
- ✅ 脚本可以正常访问 config 对象
- ✅ `config.url`, `config.username` 等全部可用
- ✅ 浏览器任务启动成功

---

**修复完成时间**: 2026-01-22  
**状态**: ✅ 已修复并验证  
**编译状态**: ✅ 成功

---

**© 2026 Unit.la Bug Fix Report**
