# Loader 项目实现状态

## ✅ 已完成部分

### 1. Loader DLL 项目创建

#### 项目配置 (`Loader/Loader.vcxproj`)
- ✅ C++ 20 标准
- ✅ Windows SDK 10
- ✅ 平台工具集 v142
- ✅ UTF-8 编码支持
- ✅ DynamicLibrary 类型
- ✅ 自动复制到 BaiShengVx3Plus/bin 目录

#### 核心文件
- ✅ `Loader.h` - DLL 导出函数声明
- ✅ `Loader.cpp` - DLL 实现
- ✅ `Process.h` - 进程管理
- ✅ `Injector.h` - DLL 注入
- ✅ `Parallel.h` - 多进程管理

#### 导出的 API 函数

```cpp
// 1. 启动微信并注入 WeixinX.dll
LOADER_API bool LaunchWeChatWithInjection(
    const wchar_t* ip,           // RabbitMQ IP
    const wchar_t* port,         // RabbitMQ 端口
    const wchar_t* dllPath,      // WeixinX.dll 路径
    wchar_t* errorMessage,       // 错误信息输出
    int errorMessageSize         // 缓冲区大小
);

// 2. 注入 DLL 到指定进程
LOADER_API bool InjectDllToProcess(
    DWORD processId,             // 目标进程ID
    const wchar_t* dllPath,      // DLL 路径
    wchar_t* errorMessage,       // 错误信息输出
    int errorMessageSize         // 缓冲区大小
);

// 3. 获取所有微信进程
LOADER_API int GetWeChatProcesses(
    DWORD* processIds,           // 进程ID数组
    int maxCount                 // 数组容量
);
```

## 🚧 待完成部分

### 2. VxMain 界面修改

#### 需要修改：
- [ ] `lblContactList` 改为 `txtCurrentContact` (只读TextBox)
- [ ] 添加 `btnBindingContacts` 按钮
- [ ] 添加 `btnGetContactList` 按钮

### 3. 服务层实现

#### 需要创建：
- [ ] `IContactBindingService` 接口
- [ ] `ContactBindingService` 实现
- [ ] `IWeChatLoaderService` 接口
- [ ] `WeChatLoaderService` 实现

### 4. C# P/Invoke 包装

#### 需要创建：
- [ ] `LoaderNative.cs` - P/Invoke 声明
- [ ] `LoaderService.cs` - 托管包装类

## 编译说明

### 编译 Loader.dll

```bash
# 使用 Visual Studio
1. 打开 Loader/Loader.vcxproj
2. 选择 Release x64 配置
3. 生成项目

# 输出位置
Loader/x64/Release/Loader.dll
→ 自动复制到 →
BaiShengVx3Plus/bin/Release/net8.0-windows/Loader.dll
```

### 在 C# 中调用

```csharp
// P/Invoke 声明
[DllImport("Loader.dll", CharSet = CharSet.Unicode)]
public static extern bool LaunchWeChatWithInjection(
    string ip,
    string port,
    string dllPath,
    [Out] StringBuilder errorMessage,
    int errorMessageSize
);

// 使用示例
StringBuilder error = new StringBuilder(512);
bool success = LoaderNative.LaunchWeChatWithInjection(
    "127.0.0.1",
    "5672",
    @"D:\path\to\WeixinX.dll",
    error,
    512
);

if (success)
{
    MessageBox.Show("成功启动微信！");
}
else
{
    MessageBox.Show($"失败: {error}");
}
```

## 下一步工作

1. 修改 VxMain.Designer.cs - 添加新控件
2. 创建服务接口和实现
3. 创建 P/Invoke 包装类
4. 实现按钮点击事件
5. 测试完整流程

## 项目结构

```
wx4helper/
├── Loader/                      # ✅ 新建 DLL 项目
│   ├── Loader.vcxproj
│   ├── Loader.h
│   ├── Loader.cpp
│   ├── Process.h
│   ├── Injector.h
│   └── Parallel.h
│
├── BaiShengVx3Plus/
│   ├── Services/                # 🚧 待创建
│   │   ├── IContactBindingService.cs
│   │   ├── ContactBindingService.cs
│   │   ├── IWeChatLoaderService.cs
│   │   └── WeChatLoaderService.cs
│   │
│   ├── Native/                  # 🚧 待创建
│   │   ├── LoaderNative.cs
│   │   └── LoaderService.cs
│   │
│   └── VxMain.cs                # 🚧 待修改
│
└── WeixinX/
    ├── WeixinX/                 # 现有项目
    └── Initiator/               # 参考项目
```

