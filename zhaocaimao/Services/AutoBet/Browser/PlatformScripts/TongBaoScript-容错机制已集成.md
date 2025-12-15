# TongBaoScript 投注容错重试机制集成说明

## ✅ 完成状态

**zhaocaimao 项目的 TongBaoScript 已成功集成投注容错重试机制！**

编译状态：
- ✅ 0 个错误
- ⚠️ 135 个警告（项目原有警告，非新引入）

---

## 🔧 改造内容

### **1️⃣ 添加必要的引用**

```csharp
using BaiShengVx3Plus.Shared.Helpers;  // 🔥 引入共享库（ModernHttpHelper, BinggoTimeHelper）
```

**用途：**
- `ModernHttpHelper`: 现代化HTTP请求包装器（支持超时、重试）
- `BinggoTimeHelper`: Binggo期号与时间计算工具

---

### **2️⃣ 添加新的字段**

```csharp
private readonly ModernHttpHelper _httpHelper;  // HTTP请求助手
private string DoMainApi = "";  // 动态API域名（从getmoneyinfo自动提取）
```

**说明：**
- `ModernHttpHelper`: 使用共享库的HTTP助手，支持超时和重试
- `DoMainApi`: 投注API域名，从`getmoneyinfo`响应中动态提取，避免硬编码

---

### **3️⃣ 构造函数初始化**

```csharp
public TongBaoScript(WebView2 webView, Action<string> logCallback)
{
    _webView = webView;
    _logCallback = logCallback;
    _httpHelper = new ModernHttpHelper(_httpClient);  // 🔥 初始化 ModernHttpHelper
    // ...
}
```

---

### **4️⃣ PlaceBetAsync 投注方法完全重写**

**核心改造：**

```csharp
// 🎯 计算封盘时间（开奖时间 - 20秒）
var openTime = BinggoTimeHelper.GetIssueOpenTime(issueId);
var sealTime = openTime.AddSeconds(-20);

// 🔥 重试机制：直到成功或超过封盘时间
while (retryCount < maxRetries)
{
    // 检查是否超过封盘时间
    if (now > sealTime) { 停止投注; }
    
    // 发送投注（2秒超时）
    var result = await _httpHelper.PostAsync(...);
    
    if (result.Success)
    {
        // 成功返回 → 解析响应
        // 业务错误 → 判断是否重试
    }
    else
    {
        // 超时 → 查询订单验证是否已成功
        var (success, orderList, ...) = await GetLotMainOrderInfosAsync(...);
        
        // 检查是否有匹配的订单（金额 + 期号）
        if (匹配成功) { 返回成功; }
        
        // 继续重试
    }
}
```

**详细流程：** 参考 `BsBrowserClient/PlatformScripts/TongBaoScript-投注容错机制说明.md`

---

### **5️⃣ 新增 GetLotMainOrderInfosAsync 方法**

```csharp
/// <summary>
/// 获取订单列表（未结算/已结算）
/// 接口: /frclienthall/getlotmainorderinfos
/// </summary>
public async Task<(bool success, List<JObject>? orders, int maxRecordNum, int maxPageNum, string errorMsg)> GetLotMainOrderInfosAsync(
    int state = 0, 
    int pageNum = 1, 
    int pageCount = 20,
    string? beginDate = null,
    string? endDate = null,
    int timeout = 2)
{
    // ... 完整实现 ...
}
```

**用途：**
- 投注超时时查询订单验证是否成功
- 手动查询未结算订单
- 订单历史查询

---

### **6️⃣ HandleResponse 添加 API 域名动态更新**

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
        }
    }
    catch (Exception ex)
    {
        _logCallback($"⚠️ 解析getmoneyinfo域名失败: {ex.Message}");
    }
}
```

**优势：**
- 自动适应域名变化
- 避免硬编码域名
- 支持动态切换API服务器

---

## 🎯 核心特性

### **1. 智能重试机制**

| 特性 | 说明 |
|------|------|
| **2秒超时** | 投注请求2秒无响应即超时 |
| **订单验证** | 超时后查询订单确认是否成功 |
| **时间控制** | 封盘时间 = 开奖时间 - 20秒 |
| **智能重试** | 失败自动重试，成功立即返回 |
| **错误识别** | 区分致命错误和可恢复错误 |
| **防死循环** | 最大重试100次 |

---

### **2. 容错策略**

| 情况 | 处理方式 |
|------|----------|
| **网络超时** | ✅ 查询订单验证后重试 |
| **普通错误** | ✅ 继续重试 |
| **余额不足** | ❌ 立即停止 |
| **已封盘** | ❌ 立即停止 |
| **已结束** | ❌ 立即停止 |
| **超过封盘时间** | ❌ 立即停止 |

---

### **3. 订单验证逻辑**

```csharp
// 🎯 匹配条件：金额相同 && 期号相同
if (orderAmount == (int)totalAmount && orderExpect == issueId.ToString())
{
    // ✅ 找到匹配订单，投注成功
    return (true, orderId, responseJson);
}
```

**为什么需要验证？**
- 网络卡顿时，投注请求可能超时，但服务器可能已处理成功
- 通过查询订单列表，检查是否已有匹配的订单
- 避免重复投注

---

## 📊 对比表格（改造前 vs 改造后）

| 特性 | 改造前 | 改造后 |
|------|--------|--------|
| **HTTP请求方式** | 直接使用HttpClient | 使用ModernHttpHelper |
| **超时控制** | ❌ 无 | ✅ 2秒超时 |
| **重试机制** | ❌ 无 | ✅ 智能重试（直到封盘） |
| **订单验证** | ❌ 无 | ✅ 超时后验证订单 |
| **封盘时间控制** | ❌ 无 | ✅ 精确控制（开奖-20秒） |
| **API域名管理** | ❌ 硬编码 | ✅ 动态提取 |
| **错误识别** | ❌ 简单判断 | ✅ 区分致命/可恢复错误 |
| **日志详细度** | ⚠️ 基础日志 | ✅ 详细日志（重试次数、剩余时间） |

---

## 🔄 完整的重试流程

```
开始投注
    ↓
计算封盘时间 (开奖时间 - 20秒)
    ↓
┌──────────────────────────────┐
│  检查是否超过封盘时间        │
│  ├─ ✅ 未超过 → 继续          │
│  └─ ❌ 超过 → 停止投注        │
└──────────────────────────────┘
    ↓
发送投注请求（2秒超时）
    ↓
┌──────────────────────────────┐
│  请求是否成功返回？          │
│  ├─ ✅ 是：                   │
│  │   ├─ status=true → 成功   │
│  │   └─ status=false         │
│  │       ├─ 致命错误 → 退出  │
│  │       └─ 普通错误 → 重试  │
│  │                            │
│  └─ ⏰ 否（超时）：           │
│      ├─ 查询未结算订单        │
│      ├─ 检查金额&期号是否匹配│
│      │   ├─ ✅ 匹配 → 成功   │
│      │   └─ ❌ 不匹配 → 重试 │
└──────────────────────────────┘
    ↓
返回结果
```

---

## 📖 典型场景示例

### **场景1：网络卡顿，投注实际已成功**

```
23:14:30 | 🔄 第1次投注尝试 (距封盘还有10秒)
23:14:30 | 📤 发送投注请求
23:14:32 | ⏰ 投注请求超时（2秒无响应）
23:14:32 | 🔍 查询未结算订单 (金额:20元)...
23:14:33 | 📋 查询到 1 条未结算订单，开始匹配...
23:14:33 | ✅ 找到匹配订单！金额:20元, 期号:114070636
23:14:33 | 🎉 投注已成功（通过订单验证确认，第1次尝试）
```

**结果：** ✅ 成功（虽然投注请求超时，但通过订单验证确认成功）

---

### **场景2：第一次失败，重试成功**

```
23:14:30 | 🔄 第1次投注尝试 (距封盘还有10秒)
23:14:32 | ⏰ 投注请求超时
23:14:32 | 🔍 查询未结算订单...
23:14:33 | ⚠️ 未找到订单，等待1秒后继续投注...
23:14:34 | 🔄 第2次投注尝试 (距封盘还有6秒)
23:14:35 | 📥 投注响应: {"status":true...
23:14:35 | ✅ 投注成功: 25121423143510029526020 (第2次尝试)
```

**结果：** ✅ 成功（第2次重试成功）

---

### **场景3：超过封盘时间**

```
23:14:38 | 🔄 第3次投注尝试 (距封盘还有2秒)
23:14:40 | ⏰ 投注请求超时
23:14:40 | 🔍 查询未结算订单...
23:14:41 | ⚠️ 未找到订单，等待1秒后继续投注...
23:14:42 | ⏰ 已超过封盘时间(23:14:40)，停止投注
```

**结果：** ❌ 失败（超过封盘时间）

---

## 🎉 总结

### **✅ 已完成**

1. ✅ 引入共享库（ModernHttpHelper, BinggoTimeHelper）
2. ✅ 添加 ModernHttpHelper 字段和初始化
3. ✅ 添加 DoMainApi 动态域名管理
4. ✅ PlaceBetAsync 完全重写（智能重试 + 订单验证）
5. ✅ 新增 GetLotMainOrderInfosAsync 方法
6. ✅ HandleResponse 添加 DoMainApi 动态更新逻辑
7. ✅ 编译成功（0个错误）

### **🎯 核心优势**

✅ **网络容错**：超时后不放弃，验证订单确认是否成功  
✅ **智能重试**：失败自动重试，直到成功或封盘  
✅ **时间控制**：精确计算封盘时间，避免封盘后投注  
✅ **订单验证**：通过金额和期号双重匹配确认成功  
✅ **致命错误识别**：余额不足等错误立即停止，不浪费时间  
✅ **防死循环**：最大重试100次保护  
✅ **详细日志**：每步都有日志，便于调试  

### **📚 参考文档**

- `BsBrowserClient/PlatformScripts/TongBaoScript-投注容错机制说明.md`（详细机制说明）
- `BsBrowserClient/PlatformScripts/TongBao-BettingNumber字段来源说明.md`（字段参考来源）
- `BsBrowserClient/PlatformScripts/TongBaoScript-认证机制对比分析.md`（认证机制分析）

---

## 🚀 使用说明

**该容错机制已自动集成到 zhaocaimao 项目的 TongBaoScript 中，无需额外配置！**

只需正常调用 `PlaceBetAsync` 方法即可享受完整的容错保护：

```csharp
var result = await _platformScript.PlaceBetAsync(orders);
// 容错机制会自动处理：超时重试、订单验证、封盘控制等
```

**投注将更加稳定可靠！** 🎊

