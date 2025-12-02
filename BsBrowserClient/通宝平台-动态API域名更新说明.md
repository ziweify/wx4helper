# 通宝平台 - 动态API域名更新功能说明

> **更新日期**: 2025-12-01  
> **修改文件**: `BsBrowserClient/PlatformScripts/TongBaoScript.cs`  
> **功能**: 自动检测和更新投注API域名

---

## 📋 功能概述

通宝平台的投注API域名可能会动态变化（如 `https://api.fr.win2000.vip`）。本次更新实现了**自动检测和更新**投注API域名的功能，确保投注请求始终使用最新的API地址。

---

## 🔧 实现原理

### 1. 新增成员变量

```csharp
// 🔥 动态API域名（从getmoneyinfo请求中自动提取和更新）
private string DoMainApi = "";  // 投注使用的API域名，如 https://api.fr.win2000.vip
```

**作用**: 存储当前有效的投注API域名。

---

### 2. 初始化逻辑

**位置**: 构造函数 `TongBaoScript(...)`

```csharp
// 🔥 初始化DoMainApi为配置的盘口地址（后续会从getmoneyinfo请求中动态更新）
try
{
    if (_webView?.CoreWebView2 != null && !string.IsNullOrEmpty(_webView.CoreWebView2.Source))
    {
        var uri = new Uri(_webView.CoreWebView2.Source);
        DoMainApi = $"{uri.Scheme}://{uri.Host}";
        _logCallback($"🌐 初始化API域名: {DoMainApi}");
    }
}
catch { }
```

**说明**:
- 从配置管理中的"盘口地址"提取初始域名
- 仅提取域名部分（如 `https://tbfowenb.fr.cvv66.top`）
- 输出初始化日志便于观察

---

### 3. 动态拦截和更新

**位置**: `HandleResponse()` 方法

**拦截目标**: `https://api.fr.win2000.vip/frclienthall/getmoneyinfo`

```csharp
// 🔥 拦截 getmoneyinfo - 动态提取并更新API域名
if (response.Url.Contains("/getmoneyinfo"))
{
    try
    {
        var uri = new Uri(response.Url);
        var currentDomain = $"{uri.Scheme}://{uri.Host}";
        
        // 如果域名和DoMainApi不一致，更新DoMainApi
        if (currentDomain != DoMainApi)
        {
            var oldDomain = DoMainApi;
            DoMainApi = currentDomain;
            _logCallback($"🔄 API域名已更新: {oldDomain} → {DoMainApi}");
            _logCallback($"🌐 投注将使用新域名: {DoMainApi}/frcomgame/createmainorder");
        }
        else
        {
            // 域名一致，输出确认日志（便于观察）
            _logCallback($"✅ API域名确认: {DoMainApi}");
        }
    }
    catch (Exception ex)
    {
        _logCallback($"⚠️ 解析getmoneyinfo域名失败: {ex.Message}");
    }
}
```

**工作流程**:
1. 拦截 `getmoneyinfo` 请求
2. 提取请求的域名（如 `https://api.fr.win2000.vip`）
3. 对比当前域名和 `DoMainApi`
4. **如果不一致**: 更新 `DoMainApi` 并输出更新日志
5. **如果一致**: 输出确认日志

---

### 4. 投注使用新域名

**位置**: `PlaceBetAsync()` 方法

```csharp
// 🔥 优先使用DoMainApi（从getmoneyinfo动态获取），fallback到_baseUrl
var apiDomain = !string.IsNullOrEmpty(DoMainApi) ? DoMainApi : _baseUrl;

if (string.IsNullOrEmpty(apiDomain))
{
    _logCallback("❌ 未获取到API域名，可能未登录");
    return (false, "", "#未获取到API域名，可能未登录");
}

// 发送POST请求（使用DoMainApi动态域名）
var url = $"{apiDomain}/frcomgame/createmainorder";

_logCallback($"🌐 投注API域名: {apiDomain}");
```

**说明**:
- **优先使用** `DoMainApi`（从 `getmoneyinfo` 动态获取的最新域名）
- **Fallback** 到 `_baseUrl`（初始配置的域名）
- 投注URL: `{DoMainApi}/frcomgame/createmainorder`
- 输出投注域名日志便于观察

---

## 📊 日志输出示例

### 情况1: 初始化

```
[通宝] 🌐 初始化API域名: https://tbfowenb.fr.cvv66.top
```

---

### 情况2: 拦截到 getmoneyinfo，域名不变

```
[通宝] ✅ API域名确认: https://api.fr.win2000.vip
```

---

### 情况3: 拦截到 getmoneyinfo，域名已变化

```
[通宝] 🔄 API域名已更新: https://api.old.domain.com → https://api.fr.win2000.vip
[通宝] 🌐 投注将使用新域名: https://api.fr.win2000.vip/frcomgame/createmainorder
```

---

### 情况4: 投注时输出当前域名

```
[通宝] 🎲 开始投注: 期号114067797 共3项 500元
[通宝] 🌐 投注API域名: https://api.fr.win2000.vip
[通宝] 📤 发送投注请求: https://api.fr.win2000.vip/frcomgame/createmainorder
```

---

## 🔍 如何观察域名变化

### 1. 在 BsBrowserClient 日志中查看

**位置**: BsBrowserClient窗口 → 日志面板

**筛选**: 搜索 `[通宝]` 或 `API域名`

**关键日志**:
- `🌐 初始化API域名:` - 启动时的初始域名
- `✅ API域名确认:` - 每次 `getmoneyinfo` 请求确认域名一致
- `🔄 API域名已更新:` - 域名发生变化
- `🌐 投注API域名:` - 每次投注使用的域名

---

### 2. 日志示例（完整流程）

```
[通宝] 🔐 开始登录通宝: test_user
[通宝] ✅ 用户名已填充: test_user
[通宝] ✅ 密码已填充
[通宝] 🌐 初始化API域名: https://tbfowenb.fr.cvv66.top
[通宝] ✅ 拦截到登录参数 - UUID: 10014139, Token: 640006705...
[通宝] 💰 余额更新: 1000.00
[通宝] ✅ API域名确认: https://api.fr.win2000.vip
[通宝] ✅ API域名确认: https://api.fr.win2000.vip
[通宝] 🎲 开始投注: 期号114067797 共3项 500元
[通宝] 🌐 投注API域名: https://api.fr.win2000.vip
[通宝] 📤 发送投注请求: https://api.fr.win2000.vip/frcomgame/createmainorder
[通宝] ✅ 投注成功: TB1733123456
```

---

## 🎯 技术细节

### 1. 域名提取

**提取方式**: 使用 `Uri` 类解析URL

```csharp
var uri = new Uri(response.Url);
var currentDomain = $"{uri.Scheme}://{uri.Host}";
```

**示例**:
- 输入URL: `https://api.fr.win2000.vip/frclienthall/getmoneyinfo?uuid=123&sid=456`
- 提取域名: `https://api.fr.win2000.vip`

---

### 2. 域名对比

**对比逻辑**: 字符串完全匹配

```csharp
if (currentDomain != DoMainApi)
{
    // 域名不一致，更新
}
```

**说明**:
- 区分大小写
- 包含协议（http/https）
- 不包含端口（如果域名相同但端口不同，会被视为不同域名）

---

### 3. Fallback机制

**优先级**: `DoMainApi` > `_baseUrl`

```csharp
var apiDomain = !string.IsNullOrEmpty(DoMainApi) ? DoMainApi : _baseUrl;
```

**说明**:
- 优先使用动态获取的 `DoMainApi`
- 如果 `DoMainApi` 为空，fallback到初始的 `_baseUrl`
- 保证在未拦截到 `getmoneyinfo` 时仍能投注

---

## ✅ 优势

1. **自动适应**: 平台域名变化时自动更新，无需手动修改配置
2. **实时监控**: 日志实时输出域名变化，便于观察和调试
3. **向后兼容**: Fallback机制保证在未拦截到新域名时仍能使用旧域名
4. **透明化**: 每次投注都输出使用的域名，便于问题排查

---

## 🔧 调试技巧

### 1. 模拟域名变化

**方法**: 修改通宝平台的DNS或hosts文件，使 `getmoneyinfo` 请求指向不同的域名。

---

### 2. 查看拦截日志

**位置**: BsBrowserClient → 日志面板

**筛选**: 搜索 `getmoneyinfo`

---

### 3. 确认投注域名

**位置**: BsBrowserClient → 日志面板

**筛选**: 搜索 `投注API域名`

---

## 📝 注意事项

1. **首次使用**: 初始化时会使用配置的"盘口地址"作为默认域名
2. **域名更新时机**: 在拦截到 `getmoneyinfo` 请求时更新
3. **日志观察**: 建议在投注前查看日志，确认域名是否正确
4. **异常处理**: 如果域名提取失败，会输出警告日志但不影响投注（使用fallback）

---

## 🎉 总结

通过本次更新，通宝平台的投注脚本现在能够：
- ✅ 自动检测API域名变化
- ✅ 实时更新投注域名
- ✅ 输出详细日志便于观察
- ✅ 保证投注始终使用最新域名

**建议**: 在使用通宝平台时，注意观察BsBrowserClient的日志输出，确认域名更新情况。

---

**更新日期**: 2025-12-01  
**版本**: v1.0

