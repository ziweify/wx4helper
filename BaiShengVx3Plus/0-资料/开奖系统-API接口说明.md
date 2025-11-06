# 🌐 开奖系统 - API 接口说明

## 📌 重要提示

**由于您还没有提供实际的 WebAPI 地址和接口规范，我将使用通用的 RESTful API 设计。**

请您提供：
1. API 根地址（例如：`http://api.yourserver.com/`）
2. 登录接口
3. 获取开奖数据接口
4. 其他业务接口

在您提供实际接口后，我会更新 `BsWebApiClient.cs` 中的实现。

---

## 🔧 假设的 API 接口规范

### 1. 登录接口

**请求**:
```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "123456"
}
```

**响应**:
```json
{
  "code": 0,
  "msg": "登录成功",
  "data": {
    "userId": 1,
    "username": "admin",
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "tokenExpiry": "2025-11-07T10:00:00",
    "validUntil": "2026-01-01T00:00:00",
    "isAdmin": true
  }
}
```

### 2. 获取当前期开奖数据

**请求**:
```http
GET /api/binggo/current
Authorization: Bearer {token}
```

**响应**:
```json
{
  "code": 0,
  "msg": "成功",
  "data": {
    "issueId": 20251106001,
    "numbersString": "1,2,3,4,5",
    "issueStartTime": "2025-11-06T10:00:00",
    "openTime": "2025-11-06T10:05:00"
  }
}
```

### 3. 获取指定期号开奖数据

**请求**:
```http
GET /api/binggo/data/{issueId}
Authorization: Bearer {token}
```

**响应**: 同上

### 4. 获取最近 N 期开奖数据

**请求**:
```http
GET /api/binggo/recent?count=10
Authorization: Bearer {token}
```

**响应**:
```json
{
  "code": 0,
  "msg": "成功",
  "data": [
    {
      "issueId": 20251106010,
      "numbersString": "1,2,3,4,5",
      "issueStartTime": "2025-11-06T10:45:00",
      "openTime": "2025-11-06T10:50:00"
    },
    // ... more
  ]
}
```

---

## ⚠️ 请提供实际接口

请您提供实际的接口文档，或者参考 F5BotV2 中的 `BoterApi.cs`，我会据此更新实现。

