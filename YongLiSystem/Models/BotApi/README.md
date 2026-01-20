# BotApi 命名空间设计说明

**📅 创建日期**: 2025-12-20  
**📌 目的**: 支持 BotApi 多版本，每个版本的数据结构独立

---

## 📁 命名空间结构

```
永利系统.Models.BotApi
├── V1/
│   ├── ApiUser.cs        # V1 版本的 API 用户模型
│   └── ApiResponse.cs    # V1 版本的 API 响应模型
└── V2/
    ├── ApiUser.cs        # V2 版本的 API 用户模型
    └── ApiResponse.cs    # V2 版本的 API 响应模型
```

---

## 🎯 设计原则

### 1. **版本隔离**
- 每个版本的模型类放在独立的命名空间中
- 不同版本可以使用相同的类名（如 `ApiUser`），但通过命名空间区分

### 2. **数据结构独立**
- V1 和 V2 的数据结构可以完全不同
- 每个版本可以有不同的 JSON 字段映射
- 修改一个版本不会影响另一个版本

### 3. **使用方式**

**V1 版本**：
```csharp
using 永利系统.Models.BotApi.V1;

var user = new ApiUser();
var response = new ApiResponse<ApiUser>();
```

**V2 版本**：
```csharp
using 永利系统.Models.BotApi.V2;

var user = new ApiUser();  // 这是 V2 版本的 ApiUser
var response = new ApiResponse<ApiUser>();  // 这是 V2 版本的 ApiResponse
```

**同时使用两个版本**：
```csharp
using V1 = 永利系统.Models.BotApi.V1;
using V2 = 永利系统.Models.BotApi.V2;

var v1User = new V1.ApiUser();
var v2User = new V2.ApiUser();
```

---

## 📋 当前状态

### ✅ V1 版本（已实现）
- **命名空间**: `永利系统.Models.BotApi.V1`
- **类**: `ApiUser`, `ApiResponse<T>`
- **JSON 字段**:
  - `c_soft_name` → `SoftName`
  - `c_sign` → `Token`
  - `c_token_public` → `PublicToken`
  - `c_off_time` → `ValidUntil`
  - `code` → `Code`
  - `msg` → `Msg`
  - `data` → `Data`

### 📝 V2 版本（待完善）
- **命名空间**: `永利系统.Models.BotApi.V2`
- **类**: `ApiUser`, `ApiResponse<T>`
- **状态**: 已创建模板，需要根据 V2 API 的实际字段结构修改 `JsonProperty` 特性

---

## 🔄 迁移说明

### 从旧命名空间迁移

**旧代码**：
```csharp
using 永利系统.Models.Api;

var user = new ApiUser();
var response = new ApiResponse<ApiUser>();
```

**新代码（V1）**：
```csharp
using 永利系统.Models.BotApi.V1;

var user = new ApiUser();
var response = new ApiResponse<ApiUser>();
```

---

## 🚀 添加新版本

如果需要添加 V3 版本：

1. **创建目录和文件**：
   ```
   永利系统/Models/BotApi/V3/
   ├── ApiUser.cs
   └── ApiResponse.cs
   ```

2. **设置命名空间**：
   ```csharp
   namespace 永利系统.Models.BotApi.V3
   {
       public class ApiUser { ... }
       public class ApiResponse<T> { ... }
   }
   ```

3. **根据 V3 API 的实际字段结构修改 JsonProperty 特性**

---

## ⚠️ 注意事项

1. **不要混用版本**：确保一个服务只使用一个版本的 API 模型
2. **更新 JsonProperty**：如果 V2 的 JSON 字段与 V1 不同，必须修改 `JsonProperty` 特性
3. **保持向后兼容**：如果 V2 与 V1 兼容，可以考虑让 V2 继承 V1 或使用适配器模式

---

## 📝 相关文件

- `永利系统/Infrastructure/Api/BoterApi.cs` - 使用 V1 版本的 API 客户端
- `永利系统/Services/Auth/AuthService.cs` - 使用 V1 版本的用户模型
- `永利系统/Services/Auth/AuthGuard.cs` - 使用 V1 版本的用户模型
- `永利系统/ViewModels/LoginViewModel.cs` - 使用 V1 版本的响应模型

---

**版本**: v1.0  
**最后更新**: 2025-12-20

