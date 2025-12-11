# YYDS 平台集成文档

**创建时间**: 2025-12-10  
**平台URL**: https://client.06n.yyds666.me/  
**登录页面**: https://client.06n.yyds666.me/login?redirect=%2F  
**状态**: ✅ 框架已完成，⚠️ 投注API待实现  

---

## 一、概述

YYDS 是一个新的投注平台，本文档描述其在 zhaocaimao 项目中的集成过程和实现细节。

### 1.1 集成内容

1. ✅ 创建 `YydsScript.cs` 平台脚本
2. ✅ 实现自动登录功能（支持自动填充用户名密码）
3. ✅ 实现余额查询功能
4. ⚠️ 投注功能（需要分析平台API后实现）
5. ✅ 添加到平台枚举和URL映射
6. ✅ 集成到浏览器引擎

---

## 二、文件修改清单

### 2.1 新增文件

#### `zhaocaimao/Services/AutoBet/Browser/PlatformScripts/YydsScript.cs`

**功能**: YYDS 平台脚本实现

**主要方法**:
- `LoginAsync(username, password)`: 自动登录
  - 导航到登录页面
  - 等待表单加载
  - 自动填充用户名和密码
  - 聚焦到验证码输入框
  - 等待用户手动输入验证码并登录
  - 监听登录成功（URL跳转或Cookie变化）
  - 提取认证Token

- `GetBalanceAsync()`: 获取余额
  - 从页面DOM中提取余额信息
  - 支持多种常见的余额元素选择器
  - 返回当前余额

- `PlaceBetAsync(orders)`: 投注（待实现）
  - ⚠️ 需要分析平台投注API
  - 当前返回"投注功能尚未实现"错误

- `HandleResponse(response)`: 拦截网络响应
  - 拦截登录响应，提取Token
  - 拦截余额查询响应
  - 拦截投注响应（用于后续开发）
  - 拦截赔率响应（用于后续开发）

- `GetOddsList()`: 获取赔率列表
  - 返回当前赔率信息

#### `zhaocaimao/Shared/Platform/BetPlatformHelper.cs`

**功能**: 平台枚举和辅助工具类

**新增内容**:
- `BetPlatform.yyds` 枚举值
- YYDS 平台URL映射: `https://client.06n.yyds666.me/`
- YYDS 名称映射: `"yyds"` → `BetPlatform.yyds`

**主要方法**:
- `Parse(platformName)`: 解析平台名称为枚举
- `GetDefaultUrl(platform)`: 获取平台默认URL
- `GetAllPlatforms()`: 获取所有平台
- `IsValidPlatform(platformName)`: 检查是否为有效平台

### 2.2 修改文件

#### `zhaocaimao/Services/AutoBet/Browser/BetBrowserEngine.cs`

**修改位置**: 第203-220行

**修改内容**: 添加 YYDS 平台的脚本创建逻辑

```csharp
// InitializePlatformScript 方法中添加:
BetPlatform.yyds => CreateYydsScript(logCallback), // 🔥 YYDS 平台

// 新增方法:
private PlatformScripts.IPlatformScript? CreateYydsScript(Action<string> logCallback)
{
    try
    {
        return new PlatformScripts.YydsScript(_webView, logCallback);
    }
    catch (Exception ex)
    {
        OnLog?.Invoke($"❌ 创建YYDS脚本失败: {ex.Message}");
        return null;
    }
}
```

---

## 三、登录页面分析

### 3.1 页面结构

登录页面是一个基于表格布局的传统Web页面：

```html
<form>
  <table>
    <tr>
      <td>用户名:</td>
      <td>
        <input tabindex="1" class="gaia le val login_input" 
               size="16" type="text" name="username">
      </td>
    </tr>
    <tr>
      <td>密 码:</td>
      <td>
        <input class="gaia le val login_input" type="password" 
               id="txtPass" tabindex="2" size="14" name="password">
      </td>
    </tr>
    <tr>
      <td>验证码:</td>
      <td>
        <input class="login_input" autocomplete="off" tabindex="3" 
               size="5" maxlength="4" name="code">
        <img src="/captcha" alt="验证码">
      </td>
    </tr>
  </table>
</form>
```

### 3.2 自动登录流程

1. **导航到登录页**
   ```javascript
   window.location.href = "https://client.06n.yyds666.me/login?redirect=%2F"
   ```

2. **等待表单加载**
   - 检测 `input[name="username"]`
   - 检测 `input[name="password"]`
   - 检测 `input[name="code"]`

3. **自动填充**
   ```javascript
   document.querySelector('input[name="username"]').value = username;
   document.querySelector('input[name="password"]').value = password;
   ```

4. **触发事件**
   ```javascript
   usernameInput.dispatchEvent(new Event('input', { bubbles: true }));
   passwordInput.dispatchEvent(new Event('change', { bubbles: true }));
   ```

5. **聚焦验证码**
   ```javascript
   document.querySelector('input[name="code"]').focus();
   ```

6. **等待登录成功**
   - 监听URL变化（从 `/login` 跳转到其他页面）
   - 监听Cookie变化（session、token等）
   - 监听用户信息元素出现

---

## 四、API分析（待完成）

### 4.1 需要分析的接口

⚠️ **投注功能需要以下信息**：

1. **登录接口**
   - URL: 待分析
   - Method: POST
   - Headers: 待分析
   - Body: `{ username, password, code }`
   - Response: `{ token, ... }`

2. **余额接口**
   - URL: 待分析
   - Method: GET/POST
   - Headers: `Authorization: Bearer {token}` ？
   - Response: `{ balance, ... }`

3. **投注接口** ⚠️ **关键**
   - URL: 待分析
   - Method: POST
   - Headers: 待分析
   - Body: 待分析（期号、投注内容、金额等）
   - Response: `{ success, order_id, ... }`

4. **赔率接口**
   - URL: 待分析
   - Method: GET
   - Response: `{ odds: [...] }`

### 4.2 API分析方法

1. **使用浏览器开发者工具**
   - F12 打开开发者工具
   - 切换到 Network 标签
   - 筛选 XHR/Fetch 请求
   - 手动登录并下注，观察请求

2. **关键信息记录**
   - 请求URL
   - 请求Method
   - 请求Headers（特别是 Authorization、Content-Type）
   - 请求Body（JSON格式）
   - 响应Body（JSON格式）

3. **实现投注逻辑**
   - 在 `YydsScript.PlaceBetAsync` 中实现
   - 使用 HttpClient 发送请求
   - 解析响应并返回 `BetResult`

---

## 五、使用指南

### 5.1 添加 YYDS 配置

1. 打开 zhaocaimao 主界面
2. 进入"配置管理"或"飞单配置"
3. 点击"添加配置"
4. 填写配置信息：
   - 配置名称: `YYDS测试`
   - 平台: 选择 `yyds`
   - 平台URL: `https://client.06n.yyds666.me/`
   - 用户名: `你的用户名`
   - 密码: `你的密码`
   - 勾选"自动登录"

### 5.2 启动浏览器

1. 在配置列表中找到 YYDS 配置
2. 点击"启动浏览器"按钮
3. 浏览器窗口会自动打开并导航到登录页
4. 用户名和密码会自动填充
5. **手动输入验证码**并点击登录按钮
6. 登录成功后，系统会自动提取Token

### 5.3 测试余额查询

1. 登录成功后
2. 在命令面板中选择"获取余额"命令
3. 查看日志输出

### 5.4 注意事项

⚠️ **当前限制**:
- 验证码需要手动输入（YYDS平台有验证码，无法自动填充）
- 投注功能尚未实现（需要先分析平台API）
- 如果平台API变化，需要更新脚本

---

## 六、后续开发任务

### 6.1 高优先级

1. **分析投注API** ⚠️
   - 使用开发者工具分析投注请求
   - 记录请求格式（URL、Headers、Body）
   - 记录响应格式

2. **实现投注功能**
   - 在 `YydsScript.PlaceBetAsync` 中实现
   - 参考 `TongBaoScript.PlaceBetAsync` 的实现
   - 处理平台特定的错误码

3. **测试投注流程**
   - 小额测试
   - 验证订单ID返回
   - 验证余额扣除

### 6.2 中优先级

1. **优化登录流程**
   - 考虑验证码自动识别（如果可行）
   - 优化等待超时时间
   - 添加登录失败重试机制

2. **完善余额查询**
   - 如果有专门的余额API，改用API查询
   - 添加余额缓存机制

3. **添加赔率功能**
   - 分析赔率接口
   - 实现赔率查询
   - 更新 `_oddsMap` 和 `_oddsValues`

### 6.3 低优先级

1. **性能优化**
   - 减少页面DOM查询次数
   - 优化网络请求

2. **错误处理**
   - 添加更详细的错误日志
   - 改进错误提示信息

3. **文档完善**
   - 补充API文档
   - 添加常见问题解答

---

## 七、技术要点

### 7.1 WebView2 使用

```csharp
// 执行JavaScript获取页面信息
var result = await _webView.CoreWebView2.ExecuteScriptAsync(script);
var json = JObject.Parse(result);
```

### 7.2 网络请求拦截

```csharp
// 在 HandleResponse 中拦截所有网络请求
public void HandleResponse(BrowserResponseEventArgs response)
{
    if (response.Url.Contains("/login"))
    {
        // 处理登录响应
        var json = JObject.Parse(response.Body);
        _token = json["token"]?.ToString() ?? "";
    }
}
```

### 7.3 Cookie管理

```csharp
// 从Document.cookie提取Cookie
var extractScript = @"
    (function() {
        const cookies = document.cookie.split(';').reduce((acc, cookie) => {
            const [key, value] = cookie.trim().split('=');
            acc[key] = value;
            return acc;
        }, {});
        return cookies;
    })();
";

var result = await _webView.CoreWebView2.ExecuteScriptAsync(extractScript);
var cookies = JObject.Parse(result);
```

### 7.4 HTTP请求发送

```csharp
// 使用HttpClient发送投注请求
_httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_token}");

var content = new StringContent(
    JsonConvert.SerializeObject(betData),
    Encoding.UTF8,
    "application/json");

var response = await _httpClient.PostAsync(betUrl, content);
var responseBody = await response.Content.ReadAsStringAsync();
```

---

## 八、参考代码

### 8.1 TongBaoScript

YYDS 的实现参考了 `TongBaoScript.cs`：
- 登录流程（自动填充+手动验证码）
- 网络拦截机制
- 余额查询逻辑

### 8.2 F5BotV2

F5BotV2 的相关代码：
- `TongBaoMember.cs`: 通宝平台会员管理
- 投注API调用
- 错误处理

---

## 九、常见问题

### Q1: 登录失败怎么办？

**A**: 检查以下几点：
1. 用户名和密码是否正确
2. 验证码是否输入正确
3. 网络是否畅通
4. 查看日志中的错误信息

### Q2: 为什么投注功能不可用？

**A**: 投注功能需要先分析平台API才能实现。当前只完成了登录和余额查询功能。

### Q3: 如何分析平台API？

**A**: 
1. 打开浏览器开发者工具（F12）
2. 切换到 Network 标签
3. 手动登录并执行操作
4. 观察 XHR/Fetch 请求
5. 记录请求URL、Headers、Body

### Q4: 能自动识别验证码吗？

**A**: 当前不支持。需要用户手动输入验证码。如果平台验证码较简单，可以考虑集成OCR识别。

---

## 十、版本历史

| 版本 | 日期 | 作者 | 说明 |
|------|------|------|------|
| 1.0 | 2025-12-10 | AI Assistant | 创建文档，完成登录功能 |

---

## 十一、联系方式

如有问题或需要协助，请联系开发团队。

**开发状态**: 
- ✅ 登录功能: 已完成
- ✅ 余额查询: 已完成
- ⚠️ 投注功能: 需要分析API
- ⏸️ 赔率功能: 待开发

---

**最后更新**: 2025-12-10  
**文档作者**: AI Assistant

