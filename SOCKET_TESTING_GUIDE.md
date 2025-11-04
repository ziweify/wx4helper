# 🧪 Socket 通信测试指南

## 问题排查

如果收到 `(null)` 响应，可能的原因：

### 1. 使用的是旧版本 DLL

**症状**：收到 `(null)` 响应，但没有报错

**解决方案**：
1. 关闭所有微信进程
2. 重新启动 `BaiShengVx3Plus`
3. 点击"采集"按钮重新注入新编译的 `WeixinX.dll`

### 2. DLL 未正确注入

**症状**：无法连接到 Socket 服务器

**解决方案**：
1. 检查日志窗口，查看注入是否成功
2. 确认端口 `6328` 没有被其他程序占用
3. 使用任务管理器确认微信进程存在

### 3. 命令处理器未注册

**症状**：服务器返回 "Unknown method" 错误

**解决方案**：
1. 确认使用的是最新编译的 `WeixinX.dll` (刚刚编译的版本)
2. 检查 `SocketCommands::RegisterAll` 是否被调用

---

## 测试步骤

### ✅ 完整测试流程

#### 第 1 步：确保使用最新 DLL
```
最新编译时间: 2025/11/4 23:29:53
输出位置: D:\gitcode\wx4helper\WeixinX\bin\release\net8.0-windows\WeixinX.dll
```

#### 第 2 步：启动测试
1. **关闭所有微信进程**（重要！）
2. 启动 `BaiShengVx3Plus.exe`
3. 使用测试账号登录

#### 第 3 步：注入 DLL
1. 点击 **"采集"** 按钮
2. 等待微信启动并注入成功
3. 观察状态栏，应显示：`Socket 连接成功，可以开始采集数据`

#### 第 4 步：打开设置窗口
1. 点击 **"设置"** 按钮
2. 检查连接状态：应显示 `✅ 已连接`

#### 第 5 步：测试命令

##### 测试 1：获取用户信息
```
输入命令: GetUserInfo()
预期响应: 返回当前登录用户的信息 JSON
```

##### 测试 2：获取联系人列表
```
输入命令: GetContacts()
预期响应: 返回联系人数组 JSON（目前是示例数据）
```

##### 测试 3：获取群成员
```
输入命令: GetGroupContacts(wxid_group123)
预期响应: 返回群成员数组 JSON（目前是示例数据）
```

##### 测试 4：发送消息
```
输入命令: SendMessage(wxid_test, Hello World!)
预期响应: {"success": true, "messageId": "msg_xxxxx"}
```

---

## 预期响应格式

### GetUserInfo()
```json
{
  "wxid": "wxid_xxx",
  "nickname": "用户昵称",
  "account": "微信号",
  "mobile": "手机号",
  "avatar": "头像URL",
  "dataPath": "数据路径",
  "currentDataPath": "当前数据路径",
  "dbKey": "数据库密钥"
}
```

### GetContacts()
```json
[
  {
    "wxid": "wxid_example123",
    "nickname": "示例联系人",
    "remark": "备注名",
    "avatar": "http://example.com/avatar.jpg"
  }
]
```

### GetGroupContacts(group_id)
```json
[
  {
    "wxid": "wxid_member123",
    "nickname": "群成员",
    "displayName": "群昵称"
  }
]
```

### SendMessage(wxid, message)
```json
{
  "success": true,
  "messageId": "msg_1730738993"
}
```

---

## 调试技巧

### 1. 查看日志
- 主窗口点击 **"日志"** 按钮
- 筛选 `WeixinSocketClient` 和 `SettingsForm` 相关日志
- 查看请求和响应的详细信息

### 2. 检查服务器日志
由于 `WeixinX.dll` 是注入到微信进程的，日志输出可能在：
- DebugView (使用 Sysinternals DebugView 工具)
- 或者修改 `util::logging::print` 输出到文件

### 3. 使用网络工具
```bash
# 检查端口是否监听
netstat -ano | findstr 6328

# 查看微信进程 PID
tasklist | findstr WeChat.exe
```

---

## 常见错误和解决方案

| 错误信息 | 原因 | 解决方案 |
|---------|------|---------|
| `<<< 响应: (null)` | 使用旧版 DLL | 重启微信，重新注入 |
| `连接失败` | 服务器未启动 | 检查 DLL 注入是否成功 |
| `Unknown method` | 命令名称错误 | 检查命令拼写 |
| `Invalid parameters` | 参数格式错误 | 检查参数数量和类型 |
| `Operator '!=' cannot be applied` | 类型错误（已修复）| 使用最新代码 |

---

## 下一步

### 🎯 当前状态
- ✅ Socket 服务器（C++）
- ✅ Socket 客户端（C#）
- ✅ 命令处理框架
- ✅ 示例命令（GetContacts, GetGroupContacts, SendMessage, GetUserInfo）

### 🚧 待实现
- [ ] 实现真实的联系人获取（从微信数据库）
- [ ] 实现真实的群成员获取
- [ ] 实现真实的消息发送
- [ ] 添加更多命令（获取消息历史、下载图片等）
- [ ] 实现服务器主动推送（新消息通知）

---

## 技术架构

```
BaiShengVx3Plus (C#)          WeixinX.dll (C++)
      │                             │
      │  1. GetContacts()           │
      ├─────────────────────────────>│
      │                             │ SocketCommands::HandleGetContacts
      │                             │  └─> 从微信获取联系人
      │                             │
      │  2. JSON 响应               │
      │<─────────────────────────────┤
      │                             │
      │  [{"wxid":"...", "nickname":"..."}]
      │                             │
```

### 消息格式
```json
// 请求 (C# -> C++)
{
  "id": 1,
  "method": "GetContacts",
  "params": []
}

// 响应 (C++ -> C#)
{
  "id": 1,
  "result": [...],
  "error": null
}
```

---

## 总结

现在您可以：
1. ✅ 关闭所有微信
2. ✅ 启动 BaiShengVx3Plus
3. ✅ 点击"采集"重新注入新 DLL
4. ✅ 打开设置窗口测试命令
5. ✅ 观察响应是否正常

如果仍然收到 `(null)`，请检查：
- 日志窗口中的详细信息
- 是否使用了最新编译的 DLL
- 端口 6328 是否被占用

祝测试顺利！🎉

