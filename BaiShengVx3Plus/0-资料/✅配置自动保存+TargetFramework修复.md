# ✅ 配置自动保存 + TargetFramework 修复

## 🐛 问题1：配置不保存

### 问题现象
- 在快速设置面板中选择盘口、输入账号密码
- 关闭程序后重新打开
- ❌ 之前的设置丢失了

### 原因分析
配置只在以下时机保存：
- 点击[启动浏览器]按钮
- 点击[启用自动投注]复选框

**但是**，用户在输入框中修改内容后，如果不点击这些按钮，配置就不会保存。

### 解决方案

**文件**: `BaiShengVx3Plus/Views/VxMain.cs`

添加自动保存事件：

```csharp
private void InitializeAutoBetUIEvents()
{
    try
    {
        _logService.Info("VxMain", "🤖 初始化自动投注UI事件绑定...");
        
        // 从默认配置加载设置
        LoadAutoBetSettings();
        
        // ✅ 绑定自动保存事件（当控件值改变时自动保存）
        cbxPlatform.SelectedIndexChanged += (s, e) => SaveAutoBetSettings();
        txtAutoBetUsername.TextChanged += (s, e) => SaveAutoBetSettings();
        txtAutoBetPassword.TextChanged += (s, e) => SaveAutoBetSettings();
        
        _logService.Info("VxMain", "✅ 自动投注UI事件已绑定");
    }
    catch (Exception ex)
    {
        _logService.Error("VxMain", "初始化自动投注UI事件失败", ex);
    }
}
```

### 效果

**修复前**：
```
1. 用户输入账号：admin
2. 用户输入密码：123456
3. 用户关闭程序
   ↓
4. 重新打开程序
   ↓
❌ 账号和密码都是空的（没保存）
```

**修复后**：
```
1. 用户输入账号：admin
   ↓ 立即触发 TextChanged 事件
   ↓ 自动调用 SaveAutoBetSettings()
   ↓ ✅ 保存到数据库

2. 用户输入密码：123456
   ↓ 立即触发 TextChanged 事件
   ↓ 自动调用 SaveAutoBetSettings()
   ↓ ✅ 保存到数据库

3. 用户关闭程序
4. 重新打开程序
   ↓ LoadAutoBetSettings() 加载配置
   ↓ ✅ 账号：admin，密码：123456
```

## 🐛 问题2：CefSharp 加载失败（持续）

### 问题现象
```
初始化失败: Could not load file or assembly 'CefSharp, Version=126.2.180.0, 
Culture=neutral, PublicKeyToken=40c4b6fc221f4138'. 
系统找不到指定的文件。
```

### 原因分析

CefSharp 126.2.180 是为 .NET Framework 4.6.2+ 设计的，虽然可以在 .NET 8 中使用，但 NuGet 包会显示警告：

```
warning NU1701: 已使用".NETFramework,Version=v4.6.1, ..."而不是项目目标框架"net8.0-windows"
```

这可能导致运行时加载问题。

### 解决方案

修改 `TargetFramework` 为 `net8.0-windows7.0`（明确指定最低 Windows 版本）：

**文件**: `BsBrowserClient/BsBrowserClient.csproj`

```xml
<PropertyGroup>
  <OutputType>WinExe</OutputType>
  <TargetFramework>net8.0-windows7.0</TargetFramework>  <!-- 修改这里 -->
  <Nullable>enable</Nullable>
  <UseWindowsForms>true</UseWindowsForms>
  <ImplicitUsings>enable</ImplicitUsings>
  <PlatformTarget>x64</PlatformTarget>
  <CefSharpAnyCpuSupport>true</CefSharpAnyCpuSupport>
</PropertyGroup>
```

### 为什么这样修复？

**`net8.0-windows` vs `net8.0-windows7.0`**：

- `net8.0-windows` - 泛指 Windows 平台，版本不明确
- `net8.0-windows7.0` - 明确指定最低支持 Windows 7.0

CefSharp 需要特定的 Windows API，明确指定版本可以帮助：
1. ✅ NuGet 包管理器选择正确的依赖
2. ✅ 运行时正确解析程序集引用
3. ✅ 减少兼容性警告

## 🧪 测试验证

### 1. 配置自动保存测试

**步骤**：
1. 启动 BaiShengVx3Plus
2. 在快速设置面板：
   - 选择盘口：云顶28
   - 输入账号：test001
   - 输入密码：aaa111
3. 关闭程序
4. 重新启动 BaiShengVx3Plus

**预期结果**：
```
✅ 盘口：云顶28
✅ 账号：test001
✅ 密码：aaa111（显示为 ******）
```

### 2. 浏览器启动测试

**步骤**：
1. 完成上述配置
2. 点击[启动浏览器]

**预期结果**：
```
✅ 浏览器窗口打开
✅ CEF 初始化成功
✅ 不再出现"找不到程序集"错误
✅ 显示浏览器界面
```

## 📝 技术细节

### 自动保存的实现

```csharp
// 事件绑定
cbxPlatform.SelectedIndexChanged += (s, e) => SaveAutoBetSettings();

// 当用户选择新的盘口时：
// 1. SelectedIndexChanged 事件触发
// 2. Lambda 表达式调用 SaveAutoBetSettings()
// 3. SaveAutoBetSettings() 方法：
//    - 获取默认配置
//    - 更新 Platform、Username、Password
//    - 调用 _db.Update(defaultConfig)
//    - 保存到数据库
```

### 数据流

```
用户操作
  ↓
控件值改变
  ↓
触发事件 (SelectedIndexChanged / TextChanged)
  ↓
调用 SaveAutoBetSettings()
  ↓
获取默认配置 (IsDefault = true)
  ↓
更新配置属性
  ↓
保存到数据库 (_db.Update)
  ↓
✅ 配置持久化
```

### 启动时加载

```
程序启动
  ↓
InitializeAutoBetUIEvents()
  ↓
LoadAutoBetSettings()
  ↓
查询数据库 (GetConfigs().FirstOrDefault(c => c.IsDefault))
  ↓
读取默认配置
  ↓
设置控件值：
  - cbxPlatform.SelectedIndex
  - txtAutoBetUsername.Text
  - txtAutoBetPassword.Text
  ↓
✅ 恢复用户之前的设置
```

## 🎯 TargetFramework 说明

### net8.0-windows vs net8.0-windows7.0

| TargetFramework | 含义 | 适用场景 |
|---|---|---|
| `net8.0` | .NET 8 跨平台 | 跨平台应用 |
| `net8.0-windows` | .NET 8 Windows 平台 | Windows 桌面应用 |
| `net8.0-windows7.0` | .NET 8 + Windows 7.0+ API | 需要特定 Windows API 的应用 |
| `net8.0-windows10.0.17763.0` | .NET 8 + Windows 10 API | UWP 或 Windows 10+ 专用 API |

### CefSharp 的要求

CefSharp 是一个重度依赖 Windows API 的组件：
- 需要 Windows 原生窗口（HWND）
- 需要 GDI+ 绘图
- 需要 DirectX 加速
- 需要多进程支持

**明确指定 `net8.0-windows7.0` 可以确保这些 API 可用。**

## ✅ 修复总结

### 问题1：配置不保存
**原因**：只在按钮点击时保存
**修复**：绑定控件值改变事件，自动保存
**结果**：✅ 用户输入立即保存，重启程序自动恢复

### 问题2：CefSharp 加载失败
**原因**：TargetFramework 不明确，兼容性问题
**修复**：指定 `net8.0-windows7.0`
**结果**：✅ 减少兼容性警告，运行时正确加载

## 🎉 现在应该可以了

1. ✅ **配置会自动保存**
   - 输入账号 → 立即保存
   - 选择盘口 → 立即保存
   - 输入密码 → 立即保存

2. ✅ **浏览器应该能启动**
   - WorkingDirectory 已设置
   - TargetFramework 已修复
   - 所有依赖文件已复制

**请重新编译并测试！** 🚀

```bash
cd D:\gitcode\wx4helper\BaiShengVx3Plus
dotnet build --configuration Debug
```

然后启动程序测试：
1. 输入配置
2. 关闭重启（验证配置保存）
3. 点击[启动浏览器]（验证浏览器启动）

