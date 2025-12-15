# ModernHttpHelper 使用说明

## 📦 位置

`BaiShengVx3Plus.Shared/Helpers/ModernHttpHelper.cs`

## 🎯 设计目标

像老的 HttpHelper 一样简单易用，但使用现代 HttpClient 技术。

---

## ✅ 项目引用关系

```
BaiShengVx3Plus.Shared (共享库)
    ├── ModernHttpHelper
    ├── HttpRequestItem
    └── HttpResponseResult

BaiShengVx3Plus (主项目)
    └── 已引用 BaiShengVx3Plus.Shared ✅

zhaocaimao (主项目)
    └── 已引用 BaiShengVx3Plus.Shared ✅
```

---

## 📖 使用示例

### **1. 基本 GET 请求**

```csharp
using BaiShengVx3Plus.Shared.Helpers;

var helper = new ModernHttpHelper();
var result = await helper.GetAsync(new HttpRequestItem
{
    Url = "https://api.example.com/data"
});

if (result.Success)
{
    Console.WriteLine($"响应: {result.Html}");
}
else
{
    Console.WriteLine($"错误: {result.ErrorMessage}");
}
```

### **2. POST JSON 请求**

```csharp
var jsonData = JsonConvert.SerializeObject(new { username = "test", password = "123456" });

var result = await helper.PostAsync(new HttpRequestItem
{
    Url = "https://api.example.com/login",
    PostData = jsonData,
    ContentType = "application/json"
});
```

### **3. 带请求头的 POST 请求（简洁的数组方式）**

```csharp
var result = await helper.PostAsync(new HttpRequestItem
{
    Url = "https://api.example.com/bet",
    PostData = jsonData,
    ContentType = "application/json",
    Timeout = 10,  // 设置超时时间（秒）
    Headers = new[]
    {
        $"Authorization: Bearer {token}",  // 支持字符串插值
        "referer: https://example.com/",
        "sec-fetch-dest: empty",
        "sec-fetch-mode: cors",
        "sec-fetch-site: same-site",
        "accept-language: zh-CN,zh;q=0.9"
    }
});
```

### **4. 复用 HttpClient 连接池**

```csharp
// 在类中定义一个 HttpClient（推荐）
private readonly HttpClient _httpClient = new HttpClient();
private readonly ModernHttpHelper _httpHelper;

public MyClass()
{
    _httpClient.DefaultRequestHeaders.Add("User-Agent", "MyApp/1.0");
    _httpHelper = new ModernHttpHelper(_httpClient);  // 复用连接池
}

// 使用
var result = await _httpHelper.PostAsync(new HttpRequestItem
{
    Url = "https://api.example.com/data",
    PostData = jsonData,
    ContentType = "application/json"
});
```

### **5. 处理超时**

```csharp
var result = await helper.PostAsync(new HttpRequestItem
{
    Url = "https://api.example.com/slow-api",
    PostData = jsonData,
    ContentType = "application/json",
    Timeout = 5  // 5秒超时
});

if (!result.Success)
{
    if (result.ErrorMessage?.Contains("超时") == true)
    {
        Console.WriteLine("⏱️ 请求超时");
    }
    else
    {
        Console.WriteLine($"❌ 请求失败: {result.ErrorMessage}");
    }
}
```

### **6. 发送字节数据**

```csharp
byte[] fileBytes = File.ReadAllBytes("file.jpg");

var result = await helper.PostAsync(new HttpRequestItem
{
    Url = "https://api.example.com/upload",
    PostDataByte = fileBytes,
    ContentType = "image/jpeg",
    Timeout = 30  // 上传文件可能需要更长时间
});
```

---

## 🔧 HttpRequestItem 属性

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `Url` | `string` | - | **必填**，请求URL |
| `Method` | `string` | `"GET"` | 请求方法（自动设置） |
| `PostData` | `string?` | `null` | POST 数据（字符串） |
| `PostDataByte` | `byte[]?` | `null` | POST 数据（字节数组） |
| `ContentType` | `string?` | `"application/x-www-form-urlencoded"` | 内容类型 |
| `Headers` | `string[]?` | `null` | 请求头数组 |
| `Encoding` | `Encoding?` | `UTF8` | 编码 |
| `Timeout` | `int` | `100` | 超时时间（秒） |

---

## 📊 HttpResponseResult 属性

| 属性 | 类型 | 说明 |
|------|------|------|
| `Success` | `bool` | 是否成功 |
| `Html` | `string` | 响应内容（字符串） |
| `ResponseByte` | `byte[]?` | 响应内容（字节数组） |
| `StatusCode` | `int` | HTTP 状态码 |
| `StatusDescription` | `string` | 状态描述 |
| `Headers` | `Dictionary<string, string>?` | 响应头字典 |
| `ErrorMessage` | `string?` | 错误消息 |

---

## 💡 请求头数组特性

### **1. 自动处理重复**

```csharp
Headers = new[]
{
    "Authorization: old_token",
    "X-Custom: value1",
    "Authorization: new_token",  // ✅ 会覆盖 old_token
    "X-Custom: value2"           // ✅ 会覆盖 value1
}

// 最终生效：
// Authorization: new_token
// X-Custom: value2
```

### **2. 自动过滤 Content-Type**

```csharp
// ❌ 不需要在 Headers 中添加 Content-Type
Headers = new[]
{
    "Content-Type: application/json"  // 会被自动忽略
}

// ✅ 正确做法：使用 ContentType 属性
ContentType = "application/json"
```

### **3. 格式容错**

```csharp
// 以下格式都可以：
"Authorization: Bearer xxx"      // ✅ 正常格式
"Authorization:Bearer xxx"       // ✅ 无空格也行
"  Authorization  :  Bearer xxx" // ✅ 多余空格会自动去除
```

---

## 🎯 实战案例：YYDS 投注接口

```csharp
using BaiShengVx3Plus.Shared.Helpers;

// 1. 登录获取 Token
var loginResult = await _httpHelper.PostAsync(new HttpRequestItem
{
    Url = "https://admin-api.06n.yyds666.me/login",
    PostData = JsonConvert.SerializeObject(new { username, password, code }),
    ContentType = "application/json",
    Timeout = 5
});

if (loginResult.Success)
{
    var loginJson = JObject.Parse(loginResult.Html);
    var token = loginJson["data"]?["token"]?.ToString();
    
    // 2. 使用 Token 投注
    var betResult = await _httpHelper.PostAsync(new HttpRequestItem
    {
        Url = "https://admin-api.06n.yyds666.me/system/betOrder/pc_user/order_add",
        PostData = betData,
        ContentType = "application/json",
        Timeout = 10,
        Headers = new[]
        {
            $"Authorization: Bearer {token}",
            "referer: https://client.06n.yyds666.me/",
            "sec-fetch-dest: empty",
            "sec-fetch-mode: cors",
            "sec-fetch-site: same-site",
            "origin: https://client.06n.yyds666.me",
            "datasource: master"
        }
    });
    
    if (betResult.Success)
    {
        var betJson = JObject.Parse(betResult.Html);
        var code = betJson["code"]?.Value<int>() ?? 0;
        
        if (code == 200)
        {
            Console.WriteLine("✅ 投注成功");
        }
    }
}
```

---

## ⚡ 性能优势

| 特性 | 老 HttpHelper | ModernHttpHelper |
|------|---------------|------------------|
| **底层技术** | ❌ HttpWebRequest | ✅ HttpClient |
| **异步支持** | ❌ 同步阻塞 | ✅ async/await |
| **连接池** | ❌ 手动管理 | ✅ 自动复用 |
| **HTTP/2** | ❌ 不支持 | ✅ 支持 |
| **超时控制** | ⚠️ 基础 | ✅ 精确控制 |
| **易用性** | ✅ 简单 | ✅ 更简单 |

---

## 🎉 总结

**ModernHttpHelper** 提供了：

✅ **像老 HttpHelper 一样简单的封装**  
✅ **使用现代 HttpClient 技术**  
✅ **请求头数组（极致简洁）**  
✅ **自动超时控制**  
✅ **自动处理 Content-Type**  
✅ **支持连接池复用**  
✅ **完整的错误处理**  

**两个项目（BaiShengVx3Plus 和 zhaocaimao）都可以使用！** 🎊

