# 新增平台对接指南 - Yyds666平台

> **平台名称**: Yyds666 (Mail System)  
> **登录地址**: https://client.06n.yyds666.me/login?redirect=%2F  
> **添加日期**: 2025-12-01  
> **状态**: ✅ 完成

---

## 📋 概述

本文档详细说明如何为 `zhaocaimao` 项目添加一个新的投注平台。以 Yyds666 平台为例，展示完整的对接流程。

---

## 🎯 添加新平台的步骤

### 步骤1: 创建平台脚本类

**文件**: `BsBrowserClient/PlatformScripts/Yyds666Script.cs`

**作用**: 实现平台的登录、下注、余额查询等核心功能。

**关键点**:
1. **实现 `IPlatformScript` 接口**
   - `LoginAsync(string username, string password)` - 登录逻辑
   - `PlaceBetAsync(BetStandardOrderList orders)` - 下注逻辑
   - `GetBalanceAsync()` - 获取余额
   - `HandleResponse(ResponseEventArgs response)` - 拦截HTTP响应
   - `GetOddsList()` - 获取赔率列表

2. **登录实现**（Yyds666示例）
   ```csharp
   public async Task<bool> LoginAsync(string username, string password)
   {
       // 1. 填充用户名: input[name="username"]
       // 2. 填充密码: input[name="password"] or input#txtPass
       // 3. 提示用户输入验证码: input[name="code"]
       // 4. 等待用户点击登录: div.login_submit
       // 5. 检测URL跳转（离开/login页面表示成功）
   }
   ```

3. **验证码处理**
   - **手动输入**: 提示用户在浏览器中输入验证码并登录
   - **自动识别**: 集成OCR服务（如百度OCR、腾讯OCR等）
   
   **Yyds666的验证码元素**:
   - 输入框: `<input name="code" maxlength="4">`
   - 图片: 验证码图片（需要截图OCR）
   - 刷新: "换一张"链接

4. **投注实现**
   ```csharp
   public async Task<(bool, string, string)> PlaceBetAsync(BetStandardOrderList orders)
   {
       // 方法1: 通过HTTP API下注（推荐）
       //   - 抓包分析投注接口
       //   - 构造POST请求
       //   - 附加Cookie和Token
       
       // 方法2: 通过JavaScript在页面中下注
       //   - 调用页面的投注函数
       //   - 填充投注表单并提交
   }
   ```

5. **赔率映射**
   ```csharp
   private void InitializeOddsMap()
   {
       // P1大: CarNum=P1, BetPlay=大, Identify="1_big", Odds=1.97
       _oddsMap["P1大"] = new OddsInfo(CarNumEnum.P1, BetPlayEnum.大, "平码一", "1_big", 1.97f);
       // ... 完整的40个玩法映射
   }
   ```

---

### 步骤2: 更新平台枚举

**文件**: `BaiShengVx3Plus.Shared/Platform/BetPlatform.cs`

**修改1: 添加枚举值**
```csharp
public enum BetPlatform
{
    不使用盘口 = 0,
    // ... 其他平台 ...
    云顶 = 21,
    Yyds666 = 22  // 🔥 新增平台
}
```

**修改2: 添加平台配置**
```csharp
private static readonly Dictionary<BetPlatform, PlatformInfo> _platforms = new()
{
    // ... 其他平台配置 ...
    {
        BetPlatform.Yyds666, new PlatformInfo
        {
            Platform = BetPlatform.Yyds666,
            DefaultUrl = "https://client.06n.yyds666.me/login?redirect=%2F",
            LegacyNames = new[] { "yyds666", "YYDS666", "Yyds" }
        }
    }
};
```

**说明**:
- `Platform`: 枚举值
- `DefaultUrl`: 平台的默认登录地址
- `LegacyNames`: 兼容旧配置的别名（用于数据库中存储的旧名称）

---

### 步骤3: 注册平台脚本

**文件**: `BsBrowserClient/Form1.cs`

**位置**: `InitializePlatformScript()` 方法

**修改**:
```csharp
_platformScript = platform switch
{
    BetPlatform.云顶 => new YunDing28Script(_webView!, betLogCallback),
    BetPlatform.通宝 => new TongBaoScript(_webView!, betLogCallback),
    // ... 其他平台 ...
    BetPlatform.Yyds666 => new Yyds666Script(_webView!, betLogCallback),  // 🔥 新增
    BetPlatform.不使用盘口 => new NoneSiteScript(_webView!, betLogCallback),
    _ => new YunDing28Script(_webView!, betLogCallback) // 默认
};
```

---

### 步骤4: 编译测试

```bash
cd E:\gitcode\wx4helper
dotnet build BsBrowserClient/BsBrowserClient.csproj --configuration Debug
```

**检查编译结果**:
- ✅ 0 个错误
- ⚠️ 警告（nullable、async等）可以忽略

---

## 🔍 Yyds666 平台特点

### 登录页面分析

**页面截图**: 
- 标题: "Mail system - 欢迎您 使用邮件系统"
- 表单标题: "登录到 Mail"

**HTML元素**:
```html
<!-- 用户名 -->
<input tabindex="1" 
       class="gaia le val login_input" 
       size="16" 
       type="text" 
       name="username">

<!-- 密码 -->
<input class="gaia le val login_input" 
       type="password" 
       id="txtPass" 
       tabindex="2" 
       size="14" 
       name="password">

<!-- 验证码 -->
<input class="login_input" 
       autocomplete="off" 
       tabindex="3" 
       size="5" 
       maxlength="4" 
       name="code">

<!-- 登录按钮 -->
<div class="login_submit" tabindex="4"></div>
```

**登录流程**:
1. 脚本自动填充用户名和密码
2. **用户手动输入验证码**（4位数字）
3. **用户手动点击登录按钮**
4. 脚本检测URL变化（离开 `/login` 页面）
5. 登录成功 → 获取余额

### 验证码处理方案

#### 方案1: 手动输入（当前实现）✅
- **优点**: 实现简单，100%准确
- **缺点**: 需要用户交互
- **适用**: 初期测试、低频登录

```csharp
Log("⚠️ 请在浏览器中手动输入验证码，然后点击登录按钮！");
Log("💡 验证码输入框: name=\"code\"");
Log("💡 登录按钮: class=\"login_submit\"");

// 等待用户登录
for (int i = 0; i < 60; i++)
{
    await Task.Delay(1000);
    var currentUrl = _webView.CoreWebView2.Source;
    if (!currentUrl.Contains("/login"))
    {
        Log("✅ 登录成功！");
        return true;
    }
}
```

#### 方案2: OCR自动识别（可选）
- **优点**: 全自动，无需用户交互
- **缺点**: 需要集成第三方OCR服务，可能不准确
- **适用**: 高频登录、自动化场景

**实现步骤**:
1. 截取验证码图片
   ```csharp
   var captchaImg = document.querySelector('验证码图片选择器');
   var captchaUrl = captchaImg.src;
   ```

2. 调用OCR服务识别
   ```csharp
   // 示例：使用百度OCR
   var captchaCode = await BaiduOCR.RecognizeAsync(captchaImageBase64);
   ```

3. 自动填充并登录
   ```csharp
   document.querySelector('input[name="code"]').value = captchaCode;
   document.querySelector('.login_submit').click();
   ```

**推荐OCR服务**:
- 百度OCR (https://cloud.baidu.com/product/ocr)
- 腾讯OCR (https://cloud.tencent.com/product/ocr)
- 讯飞OCR (https://www.xfyun.cn/)

---

## 📊 投注接口分析

### 如何分析平台的投注接口

**工具**: Chrome DevTools → Network

**步骤**:
1. 打开 Chrome DevTools (F12)
2. 切换到 **Network** 标签页
3. 在平台上手动下注一次
4. 查找投注相关的请求（通常名称包含 `bet`, `place`, `order` 等）
5. 查看请求详情:
   - **Request URL**: 投注接口地址
   - **Request Method**: POST / GET
   - **Request Headers**: Cookie, Token, Content-Type 等
   - **Request Payload**: 投注数据格式（JSON/FormData）
   - **Response**: 返回结果格式

**示例**: 假设Yyds666的投注接口如下

```http
POST https://client.06n.yyds666.me/api/bet/place
Content-Type: application/json
Cookie: PHPSESSID=xxx; token=yyy

{
  "issueId": "114067797",
  "items": [
    { "playId": "1_big", "amount": 100, "odds": 1.97 },
    { "playId": "3_small", "amount": 50, "odds": 1.97 }
  ]
}
```

**对应的C#实现**:
```csharp
public async Task<(bool, string, string)> PlaceBetAsync(BetStandardOrderList orders)
{
    // 1. 构造投注数据
    var betItems = new List<object>();
    foreach (var order in orders.Orders)
    {
        var key = $"P{(int)order.CarNum}{order.BetPlay}";
        if (_oddsMap.TryGetValue(key, out var oddsInfo))
        {
            betItems.Add(new
            {
                playId = oddsInfo.Identify,  // "1_big"
                amount = order.Money,
                odds = oddsInfo.Odds
            });
        }
    }
    
    var betData = new { issueId = orders.IssueId, items = betItems };
    var jsonContent = JsonConvert.SerializeObject(betData);
    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
    
    // 2. 获取Cookie
    var cookies = await GetCookiesAsync();
    _httpClient.DefaultRequestHeaders.Clear();
    _httpClient.DefaultRequestHeaders.Add("Cookie", cookies);
    
    // 3. 发送请求
    var response = await _httpClient.PostAsync($"{_baseUrl}/api/bet/place", content);
    var responseText = await response.Content.ReadAsStringAsync();
    
    // 4. 解析结果
    var json = JObject.Parse(responseText);
    if (json["success"]?.ToObject<bool>() == true)
    {
        var orderId = json["orderId"]?.ToString() ?? "";
        return (true, orderId, responseText);
    }
    
    return (false, "", responseText);
}
```

---

## 🚀 使用新平台

### 在BaiShengVx3Plus中配置

1. **打开配置管理**
   - 菜单: **配置 → 配置管理**

2. **添加新配置**
   - 点击 **新增配置**
   - 配置名: `Yyds666测试`
   - 端口: `9601`（或其他空闲端口）
   - 平台: 选择 **Yyds666**
   - 平台URL: `https://client.06n.yyds666.me/login?redirect=%2F`（默认已填）

3. **配置账号**
   - 用户名: `your_username`
   - 密码: `your_password`

4. **启动投注**
   - 点击 **启动**
   - 等待浏览器打开登录页面
   - **手动输入验证码** → **点击登录按钮**
   - 登录成功后，系统会自动获取余额

5. **测试下注**
   - 在群里发送投注消息（如 `1大100`）
   - 系统会自动解析并调用平台的投注接口
   - 查看日志确认投注结果

---

## 🔧 调试技巧

### 1. 查看日志

**BsBrowserClient 日志**:
- 位置: BsBrowserClient窗口的日志面板
- 过滤: 搜索 `[Yyds666]`

**关键日志**:
```
[Yyds666] 🔐 开始登录 Yyds666...
[Yyds666] ✅ 用户名已填充: test_user
[Yyds666] ✅ 密码已填充
[Yyds666] ⚠️ 请在浏览器中手动输入验证码，然后点击登录按钮！
[Yyds666] ⏳ 等待用户登录...
[Yyds666] ✅ 登录成功！当前URL: https://client.06n.yyds666.me/home
[Yyds666] 💰 正在获取账户余额...
[Yyds666] ✅ 账户余额: 1000.00
```

### 2. 调试登录失败

**问题1: 用户名/密码输入框未找到**
- 检查元素选择器是否正确
- 使用Chrome DevTools → Elements 查看实际的HTML结构
- 更新脚本中的选择器

**问题2: 登录超时**
- 确认验证码已输入
- 确认登录按钮已点击
- 检查是否有其他弹窗阻止跳转

**问题3: 登录后获取余额失败**
- 登录成功后，打开Chrome DevTools
- 查看页面结构，找到余额显示的元素
- 更新 `GetBalanceAsync()` 中的选择器

### 3. 调试投注失败

**步骤**:
1. 在Chrome中手动下注一次
2. 打开 DevTools → Network
3. 找到投注请求，查看:
   - 请求URL
   - 请求方法（POST/GET）
   - 请求头（Cookie, Token等）
   - 请求体（JSON格式）
   - 响应结果
4. 根据实际情况修改 `PlaceBetAsync()` 的实现

**常见问题**:
- **403 Forbidden**: Cookie或Token失效，需要重新登录
- **400 Bad Request**: 请求数据格式错误，检查JSON结构
- **500 Server Error**: 服务器内部错误，可能是赔率ID不正确

---

## 📚 参考资料

### 现有平台脚本参考

| 平台 | 脚本文件 | 特点 |
|------|---------|------|
| QT | `QtScript.cs` | 标准HTTP API投注 |
| 红海 | `HongHaiScript.cs` | Token认证 + API投注 |
| 通宝 | `TongBaoScript.cs` | Cookie认证 + 表单投注 |
| 茅台 | `Mt168Script.cs` | 复杂的加密参数 |

### 关键接口

**IPlatformScript**:
```csharp
public interface IPlatformScript
{
    Task<bool> LoginAsync(string username, string password);
    Task<decimal> GetBalanceAsync();
    Task<(bool success, string orderId, string platformResponse)> PlaceBetAsync(BetStandardOrderList orders);
    void HandleResponse(ResponseEventArgs response);
    List<OddsInfo> GetOddsList();
}
```

**BetStandardOrderList**:
```csharp
public class BetStandardOrderList
{
    public int IssueId { get; set; }  // 期号
    public List<BetStandardOrder> Orders { get; set; }  // 订单列表
}

public class BetStandardOrder
{
    public CarNumEnum CarNum { get; set; }  // P1/P2/P3/P4/P5/P总
    public BetPlayEnum BetPlay { get; set; }  // 大/小/单/双/尾大/尾小/合单/合双/龙/虎
    public double Money { get; set; }  // 金额
}
```

---

## ✅ 检查清单

### 添加平台完成后，请检查：

- [ ] **步骤1**: `BsBrowserClient/PlatformScripts/Yyds666Script.cs` 已创建
  - [ ] 实现 `IPlatformScript` 接口的所有方法
  - [ ] `LoginAsync` 能够正常填充用户名和密码
  - [ ] `InitializeOddsMap` 包含40个玩法映射
  - [ ] `PlaceBetAsync` 有基本的实现框架（可以后续完善）

- [ ] **步骤2**: `BaiShengVx3Plus.Shared/Platform/BetPlatform.cs` 已更新
  - [ ] 枚举中添加了 `Yyds666 = 22`
  - [ ] `_platforms` 字典中添加了 `PlatformInfo` 配置

- [ ] **步骤3**: `BsBrowserClient/Form1.cs` 已更新
  - [ ] `InitializePlatformScript()` 中添加了 `Yyds666Script` 映射

- [ ] **步骤4**: 编译测试
  - [ ] 编译成功，0个错误
  - [ ] 警告（如果有）不影响功能

- [ ] **步骤5**: 功能测试
  - [ ] 能够在配置管理中选择 `Yyds666` 平台
  - [ ] 能够打开登录页面
  - [ ] 能够填充用户名和密码
  - [ ] 能够手动登录
  - [ ] 登录成功后能够获取余额（如果实现）

---

## 🎉 完成

恭喜！您已经成功为 `zhaocaimao` 项目添加了一个新的投注平台！

**下一步**:
1. 测试登录功能
2. 抓包分析投注接口
3. 完善 `PlaceBetAsync()` 实现
4. 测试完整的下注流程
5. 如需自动化验证码，集成OCR服务

**如有问题**:
- 查看日志输出
- 使用Chrome DevTools调试
- 参考其他平台脚本的实现

---

## 📞 技术支持

如有疑问，请联系开发团队或查看项目文档。

**相关文档**:
- `BsBrowserClient/PlatformScripts/IPlatformScript.cs` - 平台脚本接口
- `BaiShengVx3Plus.Shared/Models/` - 数据模型
- `BaiShengVx3Plus.Shared/Platform/` - 平台配置

---

**文档版本**: v1.0  
**最后更新**: 2025-12-01

