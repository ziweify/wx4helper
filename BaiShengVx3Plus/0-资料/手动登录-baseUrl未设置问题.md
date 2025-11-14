# 手动登录 - baseUrl 未设置问题

## 📋 问题描述

用户手动登录后，在高级设置中点击投注，投注失败。浏览器日志显示：

```
🎲 ❌ 未获取到base URL，可能未登录
```

但实际上：
- ✅ UUID、SID、Token 都已获取到（通过拦截 HTTP 请求）
- ✅ 赔率ID映射表已更新（共58项）
- ✅ 投注内容解析成功（ID=5364）
- ❌ `_baseUrl` 是空的，导致投注失败

---

## 🔍 问题根源

### 自动登录 vs 手动登录

```csharp
// 1. 自动登录流程（通过 LoginAsync）
public async Task<bool> LoginAsync(string username, string password)
{
    // 点击登录按钮，输入账号密码
    // ...
    
    // ✅ 登录成功后，设置 _baseUrl
    var currentUrl = _webView.CoreWebView2?.Source ?? "";
    if (!string.IsNullOrEmpty(currentUrl))
    {
        _baseUrl = new Uri(currentUrl).GetLeftPart(UriPartial.Authority);
    }
    
    _logCallback($"✅ 登录成功！UUID: {_uuid}, SID: {_sid.Substring(0, 10)}...");
    return true;
}

// 2. 手动登录流程
// 用户手动在浏览器中输入账号密码，点击登录
// LoginAsync 没有被调用
// ❌ _baseUrl 没有被设置
// ✅ 但 UUID、SID、Token 通过拦截 HTTP 请求获取到了

// 3. 投注时
public async Task<(bool success, string orderId)> PlaceBetAsync(BetOrder order)
{
    // 检查 _baseUrl
    if (string.IsNullOrEmpty(_baseUrl))
    {
        _logCallback("❌ 未获取到base URL，可能未登录");
        return (false, "");  // ← 失败！
    }
    
    // 构造POST URL
    var postUrl = $"{_baseUrl}/Bg28Lottery/Createmainorder.aspx";
    // ...
}
```

### 问题分析

**自动登录**：
```
LoginAsync 被调用
  ├─ 点击登录按钮
  ├─ 输入账号密码
  ├─ 等待登录成功
  └─ ✅ 设置 _baseUrl = https://yb666.fr.win2000.cc
```

**手动登录**：
```
用户手动输入账号密码
  ├─ 浏览器发送 POST 请求到 /getuserinfo
  ├─ OnHttpRequestIntercepted 拦截请求
  │    ├─ 解析 UUID、SID、Token
  │    └─ ✅ 设置 _uuid、_sid、_token
  └─ ❌ _baseUrl 没有被设置
```

**为什么拦截时没有设置 `_baseUrl`？**

原来的代码（第410-416行）：
```csharp
_uuid = Regex.Match(response.PostData, @"uuid=([^&]+)").Groups[1].Value;
_sid = Regex.Match(response.PostData, @"sid=([^&]+)").Groups[1].Value;

if (!string.IsNullOrEmpty(_sid) && !string.IsNullOrEmpty(_uuid))
{
    _logCallback($"✅ 拦截到登录参数 - UUID: {_uuid}, Token: {_token.Substring(0, 10)}...");
    // ← 这里没有设置 _baseUrl！
}
```

---

## ✅ 解决方案

在拦截到登录参数时，同时设置 `_baseUrl`：

```csharp
_uuid = Regex.Match(response.PostData, @"uuid=([^&]+)").Groups[1].Value;
_sid = Regex.Match(response.PostData, @"sid=([^&]+)").Groups[1].Value;

if (!string.IsNullOrEmpty(_sid) && !string.IsNullOrEmpty(_uuid))
{
    // 🔥 同时设置 _baseUrl（手动登录时也能获取到）
    if (string.IsNullOrEmpty(_baseUrl) && !string.IsNullOrEmpty(response.Url))
    {
        try
        {
            _baseUrl = new Uri(response.Url).GetLeftPart(UriPartial.Authority);
            _logCallback($"✅ Base URL 已设置: {_baseUrl}");
        }
        catch { }
    }
    
    _logCallback($"✅ 拦截到登录参数 - UUID: {_uuid}, Token: {_token.Substring(0, 10)}...");
}
```

### 为什么从 `response.Url` 获取？

```
response.Url 示例:
  https://yb666.fr.win2000.cc/Bg28Lottery/getuserinfo.aspx

new Uri(response.Url).GetLeftPart(UriPartial.Authority):
  https://yb666.fr.win2000.cc

投注时使用:
  _baseUrl + "/Bg28Lottery/Createmainorder.aspx"
  = https://yb666.fr.win2000.cc/Bg28Lottery/Createmainorder.aspx
```

---

## 🎯 修复后的流程

### 手动登录（修复后）

```
用户手动输入账号密码
  ├─ 浏览器发送 POST 请求到 /getuserinfo
  ├─ OnHttpRequestIntercepted 拦截请求
  │    ├─ 解析 UUID、SID、Token
  │    ├─ ✅ 设置 _uuid、_sid、_token
  │    └─ ✅ 设置 _baseUrl = https://yb666.fr.win2000.cc
  └─ ✅ 准备就绪，可以投注
```

### 投注（修复后）

```
PlaceBetAsync 被调用
  ├─ 检查 _baseUrl
  │    └─ ✅ _baseUrl = https://yb666.fr.win2000.cc
  ├─ 构造 POST URL
  │    └─ https://yb666.fr.win2000.cc/Bg28Lottery/Createmainorder.aspx
  ├─ 组装投注数据
  │    └─ arrbet=[{"id":5364,"money":10}]
  ├─ 发送 POST 请求
  └─ ✅ 投注成功
```

---

## 📝 修改文件清单

### 修改文件

**`BsBrowserClient/PlatformScripts/TongBaoScript.cs`**

- 修改 `OnHttpRequestIntercepted` 方法（第410-427行）
- 在拦截到登录参数时，同时设置 `_baseUrl`

---

## ✅ 修复效果

### 修复前：
❌ 手动登录后，`_baseUrl` 是空的  
❌ 投注失败："未获取到base URL，可能未登录"  
⚠️ 必须通过自动登录（`LoginAsync`）才能投注  

### 修复后：
✅ 手动登录时，通过拦截 HTTP 请求获取 `_baseUrl`  
✅ 投注时 `_baseUrl` 已设置  
✅ 无论自动登录还是手动登录，都能正常投注  

---

## 🎯 关键点

### Base URL 的作用

```csharp
// 投注请求
var postUrl = $"{_baseUrl}/Bg28Lottery/Createmainorder.aspx";

// 其他请求
var balanceUrl = $"{_baseUrl}/Bg28Lottery/getuserinfo.aspx";
var oddsUrl = $"{_baseUrl}/Bg28Lottery/getcommongroupodds.aspx";
```

### 为什么需要 Base URL？

1. **跨域问题**：HTTP 请求必须发送到正确的域名
2. **动态域名**：通宝平台可能有多个域名（如 yb666.fr.win2000.cc）
3. **配置灵活性**：不同配置可能使用不同的平台 URL

### 获取方式

```csharp
// 方式1：自动登录时，从 WebView2 获取
var currentUrl = _webView.CoreWebView2?.Source ?? "";
_baseUrl = new Uri(currentUrl).GetLeftPart(UriPartial.Authority);

// 方式2：拦截 HTTP 请求时，从请求 URL 获取
_baseUrl = new Uri(response.Url).GetLeftPart(UriPartial.Authority);
```

**现在无论自动登录还是手动登录，都能正常投注了！** 🎯

