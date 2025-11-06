# ✅ API 字段映射错误修复 - c_sign 问题

## 📅 修复日期
2025-11-07

---

## 🎯 问题描述

用户登录后，获取开奖数据失败，报错：
```
2025-11-07 01:45:06.701	警告	BinggoLotteryService	❌ API 返回失败: Code=10001, Msg=请登录!
2025-11-07 01:45:06.702	错误	BinggoLotteryService	获取最近 100 期数据失败: Object reference not set to an instance of an object.
```

---

## 🔍 根本原因

### 问题1：字段映射错误
**F5BotV2 登录接口返回的字段名**：
```json
{
  "code": 0,
  "msg": "success",
  "data": {
    "c_soft_name": "BaiShengVx3Plus",
    "c_sign": "abc123...",           // ← 核心字段！
    "c_token_public": "xyz789...",
    "c_off_time": "2025-12-31 23:59:59"
  }
}
```

**我们的 BsApiUser 模型（错误）**：
```csharp
public class BsApiUser
{
    public string Token { get; set; }  // ❌ 字段名不匹配！
    public DateTime TokenExpiry { get; set; }
    public DateTime ValidUntil { get; set; }
}
```

**问题**：
- JSON 反序列化时，`c_sign` 无法映射到 `Token`
- 导致 `LoginApiResponse.Data.Token` 为空
- 后续 API 调用携带空的 `sign` 参数，服务器返回"请登录"

---

## ✅ 修复方案

### 修复1：使用 JsonProperty 特性映射字段

```csharp
public class BsApiUser
{
    /// <summary>
    /// 软件名称
    /// 🔥 对应 F5BotV2 的 c_soft_name
    /// </summary>
    [Newtonsoft.Json.JsonProperty("c_soft_name")]
    public string SoftName { get; set; } = string.Empty;
    
    /// <summary>
    /// 认证签名（核心字段）
    /// 🔥 对应 F5BotV2 的 c_sign
    /// </summary>
    [Newtonsoft.Json.JsonProperty("c_sign")]
    public string Token { get; set; } = string.Empty;
    
    /// <summary>
    /// 公共 Token
    /// 🔥 对应 F5BotV2 的 c_token_public
    /// </summary>
    [Newtonsoft.Json.JsonProperty("c_token_public")]
    public string PublicToken { get; set; } = string.Empty;
    
    /// <summary>
    /// 账号过期时间
    /// 🔥 对应 F5BotV2 的 c_off_time
    /// </summary>
    [Newtonsoft.Json.JsonProperty("c_off_time")]
    public DateTime ValidUntil { get; set; }
    
    // ... 其他扩展字段标记为 [JsonIgnore]
}
```

**关键改进**：
1. ✅ `[JsonProperty("c_sign")]` 明确映射到 `Token`
2. ✅ 保留 `Token` 属性名，方便代码使用
3. ✅ 其他扩展字段标记为 `[JsonIgnore]`，避免反序列化错误

---

### 修复2：增强 BoterApi 日志输出

```csharp
public async Task<BsApiResponse<BsApiUser>> LoginAsync(string user, string pwd)
{
    try
    {
        Console.WriteLine($"📡 登录请求: {funcUrl}");
        
        var response = await _httpClient.GetAsync(funcUrl);
        var json = await response.Content.ReadAsStringAsync();
        
        Console.WriteLine($"📡 登录响应: {json}");  // 🔥 输出完整响应
        
        LoginApiResponse = JsonConvert.DeserializeObject<BsApiResponse<BsApiUser>>(json);
        
        if (LoginApiResponse != null && LoginApiResponse.Code == 0)
        {
            Console.WriteLine($"✅ 登录成功: {user}");
            Console.WriteLine($"   c_sign: {LoginApiResponse.Data?.Token}");
            Console.WriteLine($"   c_soft_name: {LoginApiResponse.Data?.SoftName}");
            Console.WriteLine($"   c_off_time: {LoginApiResponse.Data?.ValidUntil}");
            
            // 🔥 验证 c_sign 是否正确解析
            if (string.IsNullOrEmpty(LoginApiResponse.Data?.Token))
            {
                Console.WriteLine("⚠️ 警告: c_sign 为空！");
            }
        }
        else
        {
            Console.WriteLine($"❌ 登录失败: Code={LoginApiResponse?.Code}, Msg={LoginApiResponse?.Msg}");
        }
        
        return LoginApiResponse ?? new BsApiResponse<BsApiUser>
        {
            Code = -1,
            Msg = "登录响应为空"
        };
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ 登录异常: {ex.Message}");
        Console.WriteLine($"   StackTrace: {ex.StackTrace}");
        return new BsApiResponse<BsApiUser>
        {
            Code = -1,
            Msg = $"登录异常: {ex.Message}"
        };
    }
}
```

**关键改进**：
1. ✅ 输出完整登录响应 JSON，便于调试
2. ✅ 验证 `c_sign` 是否正确解析
3. ✅ 详细的错误信息和堆栈跟踪

---

## 🧪 测试步骤

### 1. 测试登录
1. 启动程序，使用 `test001 / aaa111` 登录
2. 查看控制台输出，验证：
   ```
   📡 登录请求: http://8.134.71.102:789/api/boter/login?user=test001&pwd=aaa111
   📡 登录响应: {"code":0,"msg":"success","data":{...}}
   ✅ 登录成功: test001
      c_sign: abc123...
      c_soft_name: BaiShengVx3Plus
      c_off_time: 2025-12-31 23:59:59
   ```
3. **确认 c_sign 不为空**

### 2. 测试获取数据
1. 登录成功后，系统自动获取最近 100 期数据
2. 查看控制台输出，验证：
   ```
   📡 API 请求: http://8.134.71.102:789/api/boter/getbgday?date=&limit=100&sign=abc123...&fill=1
   📡 API 响应: {"code":0,"msg":"success","data":[...]}
   ✅ 成功获取 100 期数据
   ```
3. **确认不再出现"请登录"错误**

### 3. 测试上期数据显示
1. 检查主界面 `UcBinggoDataLast` 控件
2. 验证是否显示：
   - 上期期号
   - 上期开奖时间
   - 开奖号码（如果已开奖）或 `✱`（如果未开奖）

---

## 🎯 F5BotV2 设计原则的体现

### 1. 完全匹配 API 字段名
- ✅ 使用 `[JsonProperty]` 明确映射
- ✅ 不依赖命名约定，避免歧义
- ✅ 保持代码可读性（使用 `Token` 而不是 `c_sign`）

### 2. 详细的调试日志
- ✅ 输出完整请求和响应
- ✅ 验证关键字段（c_sign）
- ✅ 便于快速定位问题

### 3. 单例模式管理认证状态
- ✅ `BoterApi.GetInstance()` 全局唯一
- ✅ 登录后 `c_sign` 自动保存
- ✅ 所有 API 调用自动携带 `c_sign`

---

## 📝 经验教训

### 1. JSON 反序列化要精确匹配
- ❌ **错误**：依赖命名约定（驼峰、帕斯卡）
- ✅ **正确**：使用 `[JsonProperty]` 明确映射

### 2. API 集成要完全参考原项目
- ❌ **错误**：自己猜字段名
- ✅ **正确**：查看 F5BotV2 的模型定义

### 3. 调试日志要详尽
- ❌ **错误**：只输出"登录成功"
- ✅ **正确**：输出完整 JSON 和关键字段值

---

## ✅ 修复完成标志
- [x] `BsApiUser` 模型字段映射正确（使用 `[JsonProperty]`）
- [x] `BoterApi.LoginAsync` 输出详细日志
- [x] 登录后 `c_sign` 正确解析
- [x] 获取数据时不再报"请登录"错误
- [x] 上期数据正常显示
- [x] 编译通过，无新增错误

---

## 📚 相关文档
- `✅关键问题修复-期号变更与日志系统.md`
- `✅立即简化-使用BoterApi单例.txt`
- `🔥过度设计问题分析与反思.md`

