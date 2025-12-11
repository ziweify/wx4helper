# YYDS 平台集成总结

**完成时间**: 2025-12-10  
**项目**: zhaocaimao  
**平台URL**: https://client.06n.yyds666.me/  

---

## ✅ 已完成的工作

### 1. 核心文件创建

✅ **`YydsScript.cs`** - YYDS 平台脚本
- 位置: `zhaocaimao/Services/AutoBet/Browser/PlatformScripts/YydsScript.cs`
- 功能: 
  - 自动登录（填充用户名密码，等待用户输入验证码）
  - 余额查询（从页面DOM提取）
  - 网络响应拦截（HandleResponse）
  - 投注功能框架（待实现API调用）

✅ **`BetPlatformHelper.cs`** - 平台枚举工具类
- 位置: `zhaocaimao/Shared/Platform/BetPlatformHelper.cs`
- 新增: `BetPlatform.yyds` 枚举值
- 新增: YYDS 平台URL映射和名称映射

### 2. 文件修改

✅ **`BetBrowserEngine.cs`**
- 位置: `zhaocaimao/Services/AutoBet/Browser/BetBrowserEngine.cs`
- 修改: 添加 `CreateYydsScript()` 方法
- 修改: 在平台switch中添加 yyds 分支

### 3. 文档创建

✅ **`YYDS平台集成文档.md`**
- 位置: `zhaocaimao/资料/YYDS平台集成文档.md`
- 内容: 完整的集成指南、API分析方法、使用说明

---

## 📋 功能状态

| 功能 | 状态 | 说明 |
|------|------|------|
| 自动登录 | ✅ 已完成 | 支持自动填充，需手动输入验证码 |
| 余额查询 | ✅ 已完成 | 从页面DOM提取余额信息 |
| 网络拦截 | ✅ 已完成 | 拦截登录、余额、投注响应 |
| 投注功能 | ⚠️ 需实现 | 框架完成，需分析平台API |
| 赔率功能 | ⏸️ 待开发 | 待后续实现 |

---

## 🔧 登录页面元素

根据页面分析，登录表单元素如下：

```html
<!-- 用户名 -->
<input tabindex="1" class="gaia le val login_input" 
       size="16" type="text" name="username">

<!-- 密码 -->
<input class="gaia le val login_input" type="password" 
       id="txtPass" tabindex="2" size="14" name="password">

<!-- 验证码 -->
<input class="login_input" autocomplete="off" tabindex="3" 
       size="5" maxlength="4" name="code">
```

---

## 🚀 使用流程

### 1. 添加配置

1. 打开 zhaocaimao
2. 进入配置管理
3. 添加新配置：
   - 平台: `yyds`
   - URL: `https://client.06n.yyds666.me/`
   - 用户名: `你的用户名`
   - 密码: `你的密码`
   - 勾选"自动登录"

### 2. 启动浏览器

1. 点击"启动浏览器"
2. 等待页面加载
3. 用户名和密码会自动填充
4. **手动输入验证码**
5. 点击登录按钮
6. 等待登录成功（系统会自动检测）

### 3. 查看日志

登录过程中会输出详细日志：
- `🔐 开始登录 YYDS: {username}`
- `📍 导航到登录页面...`
- `✅ 登录表单已加载`
- `✅ 用户名和密码已填充`
- `⏳ 请输入验证码并点击登录按钮...`
- `✅ 登录成功！原因: URL已跳转`

---

## ⚠️ 待完成任务

### 高优先级

1. **分析投注API**
   - 使用浏览器开发者工具（F12）
   - 手动登录并下注
   - 观察 Network 标签中的 XHR/Fetch 请求
   - 记录：
     - 投注URL
     - 请求Method（GET/POST）
     - 请求Headers（Authorization等）
     - 请求Body格式
     - 响应Body格式

2. **实现投注功能**
   - 修改 `YydsScript.PlaceBetAsync` 方法
   - 使用 HttpClient 发送投注请求
   - 解析平台响应
   - 返回 `BetResult` 对象

3. **测试投注**
   - 小额测试
   - 验证订单返回
   - 验证余额扣除

### 中优先级

1. **优化登录流程**
   - 考虑验证码识别（如可行）
   - 优化等待超时
   - 添加重试机制

2. **完善余额查询**
   - 如有专门API，改用API查询
   - 添加缓存机制

3. **添加赔率功能**
   - 分析赔率接口
   - 实现赔率查询
   - 更新赔率映射表

---

## 📊 API分析模板

当你开始分析投注API时，请记录以下信息：

### 登录API
```
URL: https://client.06n.yyds666.me/api/login (示例)
Method: POST
Headers:
  Content-Type: application/json
Body:
  {
    "username": "xxx",
    "password": "xxx",
    "code": "1234"
  }
Response:
  {
    "success": true,
    "token": "xxxxx",
    "user": { ... }
  }
```

### 投注API（待分析）
```
URL: (待填写)
Method: (GET/POST)
Headers:
  Authorization: Bearer {token}
  Content-Type: (待填写)
Body:
  {
    "issueId": "xxx",
    "betContent": "xxx",
    "amount": xxx
  }
Response:
  {
    "success": true,
    "orderId": "xxx",
    "balance": xxx
  }
```

---

## 🔍 技术要点

### WebView2 JavaScript执行

```csharp
var script = @"
    (function() {
        // JavaScript代码
        return { result: 'success' };
    })();
";

var result = await _webView.CoreWebView2.ExecuteScriptAsync(script);
var json = JObject.Parse(result);
```

### 网络请求拦截

```csharp
public void HandleResponse(BrowserResponseEventArgs response)
{
    if (response.Url.Contains("/bet"))
    {
        _logCallback($"📥 拦截投注响应: {response.Body}");
    }
}
```

### HTTP请求发送

```csharp
var betData = new
{
    issueId = orders[0].IssueId,
    betContent = "...",
    amount = orders.GetTotalAmount()
};

var content = new StringContent(
    JsonConvert.SerializeObject(betData),
    Encoding.UTF8,
    "application/json");

_httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_token}");
var response = await _httpClient.PostAsync(betUrl, content);
```

---

## 📝 文件清单

### 新增文件
1. `zhaocaimao/Services/AutoBet/Browser/PlatformScripts/YydsScript.cs` (约500行)
2. `zhaocaimao/Shared/Platform/BetPlatformHelper.cs` (约200行)
3. `zhaocaimao/资料/YYDS平台集成文档.md` (详细文档)
4. `YYDS平台集成总结.md` (本文件)

### 修改文件
1. `zhaocaimao/Services/AutoBet/Browser/BetBrowserEngine.cs` (+18行)

---

## ✨ 总结

YYDS 平台的自动投注框架已完成集成，包括：

- ✅ 完整的登录流程（自动填充+手动验证码）
- ✅ 余额查询功能
- ✅ 网络响应拦截机制
- ✅ 平台脚本框架
- ✅ 详细的集成文档

**下一步工作**:
1. 分析平台投注API
2. 实现 `PlaceBetAsync` 方法
3. 测试投注流程

---

**创建时间**: 2025-12-10  
**作者**: AI Assistant  
**状态**: ✅ 框架完成，⚠️ API待分析

