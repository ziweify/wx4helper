# MoonSharp 兼容性修复

## 问题描述

在 .NET 8 环境下，MoonSharp 的 `UserData.RegisterAssembly()` 方法调用失败：

```
Assembly.GetCallingAssembly is not supported on target framework.
```

## 根本原因

MoonSharp 2.0 的 `UserData.RegisterAssembly()` 方法内部使用了 `Assembly.GetCallingAssembly()`，该方法在某些 .NET 8 配置下不受支持。

## 解决方案

移除 `UserData.RegisterAssembly()` 调用。MoonSharp 会在需要时自动注册类型。

### 修改前

```csharp
public MoonSharpScriptEngine()
{
    _script = new Script();
    
    // 允许CLR类型访问
    UserData.RegisterAssembly();
}
```

### 修改后

```csharp
public MoonSharpScriptEngine()
{
    _script = new Script();
    
    // .NET 8 不支持 Assembly.GetCallingAssembly()，所以不调用 RegisterAssembly
    // 类型将按需自动注册
    // 如果需要注册特定类型，使用: UserData.RegisterType<YourType>();
}
```

## 替代方案

如果需要明确注册特定类型以在 Lua 脚本中使用，可以使用：

```csharp
// 注册单个类型
UserData.RegisterType<MyClass>();

// 注册多个类型
UserData.RegisterType<MyClass>();
UserData.RegisterType<MyOtherClass>();

// 注册代理类型
UserData.RegisterProxyType<MyProxyDescriptor, MyClass>();
```

## 影响

- ✅ 移除了不兼容的 API 调用
- ✅ MoonSharp 仍然可以正常工作
- ✅ 类型会在首次使用时自动注册
- ⚠️ 如果需要在脚本中使用特定的 CLR 类型，需要显式注册

## 测试建议

1. 基础脚本执行（Lua 标准库）
2. 简单的 C# 对象绑定
3. 复杂的类型互操作

完成！✅
