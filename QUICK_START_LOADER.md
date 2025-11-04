# 🚀 Loader 项目快速上手指南

## 📋 前置要求

- ✅ Visual Studio 2019/2022（C++ 桌面开发工作负载）
- ✅ .NET 8.0 SDK
- ✅ Windows SDK 10
- ✅ Platform Toolset v142

## 🔧 快速编译步骤

### 步骤 1：编译 Loader.dll

#### 方法 A：使用 Visual Studio（推荐）

```bash
1. 双击打开: D:\gitcode\wx4helper\Loader\Loader.vcxproj
2. 顶部工具栏选择: Release x64
3. 右键项目 "Loader" → 生成
4. 等待编译完成（约10-20秒）
5. 查看输出: Loader\x64\Release\Loader.dll
```

#### 方法 B：使用命令行

```powershell
# 打开 "Developer Command Prompt for VS 2022"
cd D:\gitcode\wx4helper
msbuild Loader\Loader.vcxproj /p:Configuration=Release /p:Platform=x64
```

### 步骤 2：复制 DLL 到输出目录

编译成功后，Loader.dll 会自动复制到：
```
D:\gitcode\wx4helper\BaiShengVx3Plus\bin\Release\net8.0-windows\Loader.dll
```

如果没有自动复制，手动复制：
```powershell
copy Loader\x64\Release\Loader.dll BaiShengVx3Plus\bin\Release\net8.0-windows\
```

### 步骤 3：确保 WeixinX.dll 在输出目录

将 WeixinX.dll 复制到相同目录：
```powershell
copy WeixinX\x64\Release\WeixinX.dll BaiShengVx3Plus\bin\Release\net8.0-windows\
```

目录结构应该是：
```
BaiShengVx3Plus\bin\Release\net8.0-windows\
├── BaiShengVx3Plus.exe
├── Loader.dll          ← 微信启动和注入
├── WeixinX.dll         ← 微信Hook功能
└── SunnyUI.dll
```

### 步骤 4：编译 C# 项目

```powershell
cd D:\gitcode\wx4helper
dotnet build BaiShengVx3Plus\BaiShengVx3Plus.csproj -c Release
```

## 🎯 运行和测试

### 测试流程

1. **启动程序**
   ```powershell
   cd BaiShengVx3Plus\bin\Release\net8.0-windows
   .\BaiShengVx3Plus.exe
   ```

2. **登录系统**
   - 输入用户名和密码
   - 点击登录

3. **测试绑定联系人**
   - 主界面左侧有联系人列表（dgvContacts）
   - 点击选择一个联系人
   - 点击 [绑定] 按钮
   - ✅ 成功：txtCurrentContact 显示联系人ID，弹出成功提示

4. **测试获取联系人列表（注入微信）**
   - 点击 [获取列表] 按钮
   - 程序会自动：
     - 检查是否有运行的微信进程
     - 如果有 → 注入 WeixinX.dll 到现有进程
     - 如果没有 → 启动新微信并注入 WeixinX.dll
   - ✅ 成功：弹出成功提示

## 🐛 常见问题

### 问题 1：找不到 Loader.dll

**错误信息**：
```
System.DllNotFoundException: Unable to load DLL 'Loader.dll'
```

**解决方案**：
1. 确认 Loader.dll 已编译（Release x64）
2. 确认 DLL 在程序目录
3. 检查是否缺少依赖（使用 Dependencies.exe 检查）

### 问题 2：找不到 WeixinX.dll

**错误信息**：
```
找不到 WeixinX.dll
路径: D:\gitcode\wx4helper\BaiShengVx3Plus\bin\Release\net8.0-windows\WeixinX.dll
```

**解决方案**：
```powershell
copy WeixinX\x64\Release\WeixinX.dll BaiShengVx3Plus\bin\Release\net8.0-windows\
```

### 问题 3：启动微信失败

**可能原因**：
1. 注册表中找不到微信安装信息
2. 微信路径不正确
3. 没有管理员权限

**解决方案**：
1. 确认微信已正确安装
2. 以管理员身份运行程序
3. 查看错误详细信息

### 问题 4：注入失败

**可能原因**：
1. 微信进程权限不足
2. WeixinX.dll 与微信版本不匹配
3. 杀毒软件拦截

**解决方案**：
1. 以管理员身份运行
2. 检查微信版本是否为 4.1.0.21
3. 暂时关闭杀毒软件

## 📊 验证注入成功

### 方法 1：查看进程模块

使用 Process Explorer：
1. 下载并运行 Process Explorer
2. 找到 Weixin.exe 进程
3. 双击进程 → DLLs 标签页
4. 搜索 "WeixinX.dll"
5. ✅ 如果找到，说明注入成功

### 方法 2：查看微信日志

如果 WeixinX.dll 有日志功能，检查日志文件：
```
%TEMP%\WeixinX.log
```

### 方法 3：使用程序的状态栏

注入成功后，状态栏会显示：
```
状态栏：成功注入到微信进程 (PID: 12345)
```

## 🎨 界面说明

```
┌─────────────────────────────────────────────┐
│ 联系人区域                                   │
│ ┌─────────────────────────────────────────┐ │
│ │ [绑定] [刷新] [获取列表]                 │ │ ← 操作按钮
│ ├─────────────────────────────────────────┤ │
│ │ wxid_12345678                           │ │ ← 当前绑定ID（只读）
│ ├──────────┬──────────────────────────────┤ │
│ │   ID     │      昵称                     │ │
│ ├──────────┼──────────────────────────────┤ │
│ │wxid_001  │   张三                        │ │ ← 联系人列表
│ │wxid_002  │   李四                        │ │
│ └──────────┴──────────────────────────────┘ │
└─────────────────────────────────────────────┘

操作步骤：
1. 点击 "张三" 这一行（选中）
2. 点击 [绑定] 按钮
   → 顶部文本框显示 "wxid_001"
   → 弹出提示 "成功绑定联系人: 张三"

3. 点击 [获取列表] 按钮
   → 自动检查微信进程
   → 注入或启动微信
   → 弹出成功/失败提示
```

## 🔍 调试技巧

### 启用详细日志

在 `VxMain.cs` 的 `btnGetContactList_Click` 方法开始处添加：

```csharp
private void btnGetContactList_Click(object sender, EventArgs e)
{
    // 添加调试信息
    var currentDir = AppDomain.CurrentDomain.BaseDirectory;
    var dllPath = Path.Combine(currentDir, "WeixinX.dll");
    
    lblStatus.Text = $"当前目录: {currentDir}";
    Application.DoEvents();
    
    lblStatus.Text = $"DLL路径: {dllPath}";
    Application.DoEvents();
    
    lblStatus.Text = $"DLL存在: {File.Exists(dllPath)}";
    Application.DoEvents();
    
    // ... 原有代码
}
```

### 测试 Loader.dll 加载

在程序启动时测试：

```csharp
// 在 VxMain 构造函数中添加
try
{
    uint[] pids = new uint[1];
    int count = Native.LoaderNative.GetWeChatProcesses(pids, 1);
    lblStatus.Text = $"Loader.dll 加载成功！找到 {count} 个微信进程";
}
catch (Exception ex)
{
    lblStatus.Text = $"Loader.dll 加载失败: {ex.Message}";
}
```

## 📝 配置文件

### RabbitMQ 配置

默认配置在代码中：
```csharp
_loaderService.LaunchWeChat("127.0.0.1", "5672", dllPath, out string error)
```

如需修改，可以：
1. 添加配置文件 `appsettings.json`
2. 从配置读取 IP 和端口
3. 或在 UI 添加设置界面

## ✅ 完整检查清单

编译前：
- [ ] 安装了 Visual Studio（C++ 工具）
- [ ] 安装了 .NET 8.0 SDK
- [ ] 下载了 Windows SDK 10

编译 Loader.dll：
- [ ] 编译 Release x64 成功
- [ ] DLL 已复制到输出目录
- [ ] 使用 Dependencies.exe 检查依赖

准备运行：
- [ ] WeixinX.dll 在输出目录
- [ ] Loader.dll 在输出目录
- [ ] 微信已安装
- [ ] RabbitMQ 服务运行中（可选）

运行测试：
- [ ] 程序启动成功
- [ ] 登录成功
- [ ] 绑定联系人成功
- [ ] 注入微信成功（或启动微信成功）

## 🎉 成功标志

✅ **绑定功能成功**
- txtCurrentContact 显示联系人ID
- 状态栏显示 "已绑定联系人: xxx"
- 弹出成功提示框

✅ **注入功能成功**
- 状态栏显示 "成功注入到微信进程"
- 微信正常运行
- Process Explorer 中可以看到 WeixinX.dll

## 📚 相关文档

- `IMPLEMENTATION_COMPLETE.md` - 完整实现文档
- `TASK_COMPLETION_GUIDE.md` - 任务完成指南
- `LOADER_IMPLEMENTATION_STATUS.md` - Loader 实现状态

## 🆘 获取帮助

如果遇到问题：
1. 查看上述常见问题
2. 检查状态栏的错误信息
3. 查看 Windows 事件查看器
4. 使用 Process Explorer 检查进程状态

祝你使用愉快！🚀

