# 任务完成指南

## ✅ 已完成：Loader DLL 项目

Loader 项目已完全创建，位于 `Loader/` 目录。

### 导出的 API 函数

```cpp
// 1. 启动微信并注入
bool LaunchWeChatWithInjection(ip, port, dllPath, errorMessage, size);

// 2. 注入DLL到进程
bool InjectDllToProcess(processId, dllPath, errorMessage, size);

// 3. 获取微信进程列表
int GetWeChatProcesses(processIds[], maxCount);
```

## 🚧 待完成：BaiShengVx3Plus 集成

### 步骤 1：添加 txtCurrentContact 控件

在 `VxMain.Designer.cs` 中：

```csharp
// 在 pnlLeftTop 中添加 txtCurrentContact
txtCurrentContact = new Sunny.UI.UITextBox();
pnlLeftTop.Controls.Add(txtCurrentContact);

// 配置 txtCurrentContact
txtCurrentContact.Dock = DockStyle.Bottom;
txtCurrentContact.ReadOnly = true;
txtCurrentContact.Watermark = "当前绑定联系人ID";
txtCurrentContact.Height = 35;
```

### 步骤 2：创建 P/Invoke 包装

创建 `BaiShengVx3Plus/Native/LoaderNative.cs`:

```csharp
using System.Runtime.InteropServices;
using System.Text;

namespace BaiShengVx3Plus.Native
{
    public static class LoaderNative
    {
        private const string DLL_NAME = "Loader.dll";

        [DllImport(DLL_NAME, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LaunchWeChatWithInjection(
            [MarshalAs(UnmanagedType.LPWStr)] string ip,
            [MarshalAs(UnmanagedType.LPWStr)] string port,
            [MarshalAs(UnmanagedType.LPWStr)] string dllPath,
            [MarshalAs(UnmanagedType.LPWStr)] StringBuilder errorMessage,
            int errorMessageSize
        );

        [DllImport(DLL_NAME, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool InjectDllToProcess(
            uint processId,
            [MarshalAs(UnmanagedType.LPWStr)] string dllPath,
            [MarshalAs(UnmanagedType.LPWStr)] StringBuilder errorMessage,
            int errorMessageSize
        );

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetWeChatProcesses(
            [Out] uint[] processIds,
            int maxCount
        );
    }
}
```

### 步骤 3：创建托管服务

创建 `BaiShengVx3Plus/Services/IWeChatLoaderService.cs`:

```csharp
namespace BaiShengVx3Plus.Services
{
    public interface IWeChatLoaderService
    {
        bool LaunchWeChat(string ip, string port, string dllPath, out string errorMessage);
        bool InjectToProcess(uint processId, string dllPath, out string errorMessage);
        List<uint> GetWeChatProcesses();
    }
}
```

创建 `BaiShengVx3Plus/Services/WeChatLoaderService.cs`:

```csharp
using BaiShengVx3Plus.Native;
using System.Text;

namespace BaiShengVx3Plus.Services
{
    public class WeChatLoaderService : IWeChatLoaderService
    {
        public bool LaunchWeChat(string ip, string port, string dllPath, out string errorMessage)
        {
            var error = new StringBuilder(512);
            bool result = LoaderNative.LaunchWeChatWithInjection(ip, port, dllPath, error, 512);
            errorMessage = error.ToString();
            return result;
        }

        public bool InjectToProcess(uint processId, string dllPath, out string errorMessage)
        {
            var error = new StringBuilder(512);
            bool result = LoaderNative.InjectDllToProcess(processId, dllPath, error, 512);
            errorMessage = error.ToString();
            return result;
        }

        public List<uint> GetWeChatProcesses()
        {
            uint[] pids = new uint[10];
            int count = LoaderNative.GetWeChatProcesses(pids, 10);
            return pids.Take(count).ToList();
        }
    }
}
```

### 步骤 4：创建绑定联系人服务

创建 `BaiShengVx3Plus/Services/IContactBindingService.cs`:

```csharp
using BaiShengVx3Plus.Models;

namespace BaiShengVx3Plus.Services
{
    public interface IContactBindingService
    {
        void BindContact(WxContact contact);
        WxContact? GetCurrentContact();
        void ClearBinding();
    }
}
```

创建 `BaiShengVx3Plus/Services/ContactBindingService.cs`:

```csharp
using BaiShengVx3Plus.Models;

namespace BaiShengVx3Plus.Services
{
    public class ContactBindingService : IContactBindingService
    {
        private WxContact? _currentContact;

        public void BindContact(WxContact contact)
        {
            _currentContact = contact;
            // TODO: 保存到数据库
        }

        public WxContact? GetCurrentContact()
        {
            return _currentContact;
        }

        public void ClearBinding()
        {
            _currentContact = null;
        }
    }
}
```

### 步骤 5：修改 VxMain.cs

```csharp
using BaiShengVx3Plus.Services;

public partial class VxMain : UIForm
{
    private readonly IContactBindingService _contactBindingService;
    private readonly IWeChatLoaderService _loaderService;

    public VxMain(
        VxMainViewModel viewModel,
        IContactBindingService contactBindingService,
        IWeChatLoaderService loaderService)
    {
        _contactBindingService = contactBindingService;
        _loaderService = loaderService;
        // ...
    }

    private void btnBindingContacts_Click(object sender, EventArgs e)
    {
        if (dgvContacts.CurrentRow?.DataBoundItem is WxContact contact)
        {
            _contactBindingService.BindContact(contact);
            txtCurrentContact.Text = contact.Wxid;
            lblStatus.Text = $"已绑定联系人: {contact.Nickname}";
        }
        else
        {
            UIMessageBox.ShowWarning("请先选择一个联系人");
        }
    }

    private void btnGetContactList_Click(object sender, EventArgs e)
    {
        try
        {
            var currentDir = AppDomain.CurrentDomain.BaseDirectory;
            var dllPath = Path.Combine(currentDir, "WeixinX.dll");

            if (!File.Exists(dllPath))
            {
                UIMessageBox.ShowError($"找不到 WeixinX.dll: {dllPath}");
                return;
            }

            // 获取现有微信进程
            var processes = _loaderService.GetWeChatProcesses();

            if (processes.Count > 0)
            {
                // 注入到现有进程
                if (_loaderService.InjectToProcess(processes[0], dllPath, out string error))
                {
                    UIMessageBox.ShowSuccess("成功注入到微信进程");
                }
                else
                {
                    UIMessageBox.ShowError($"注入失败: {error}");
                }
            }
            else
            {
                // 启动新微信并注入
                if (_loaderService.LaunchWeChat("127.0.0.1", "5672", dllPath, out string error))
                {
                    UIMessageBox.ShowSuccess("成功启动微信并注入");
                }
                else
                {
                    UIMessageBox.ShowError($"启动失败: {error}");
                }
            }
        }
        catch (Exception ex)
        {
            UIMessageBox.ShowError($"发生错误: {ex.Message}");
        }
    }
}
```

### 步骤 6：注册服务

在 `Program.cs` 中添加服务注册：

```csharp
services.AddSingleton<IContactBindingService, ContactBindingService>();
services.AddSingleton<IWeChatLoaderService, WeChatLoaderService>();
```

## 🔨 编译步骤

### 1. 编译 Loader.dll

```bash
# 在 Visual Studio 中
1. 打开 Loader/Loader.vcxproj
2. 选择 Release x64
3. 生成项目
4. 确认 DLL 已复制到 BaiShengVx3Plus/bin/Release/net8.0-windows/
```

### 2. 编译 BaiShengVx3Plus

```bash
cd BaiShengVx3Plus
dotnet build
```

## ✅ 完成检查清单

- [ ] Loader.dll 编译成功
- [ ] Loader.dll 已复制到输出目录
- [ ] 创建 LoaderNative.cs (P/Invoke)
- [ ] 创建 WeChatLoaderService.cs
- [ ] 创建 ContactBindingService.cs
- [ ] 修改 VxMain添加 txtCurrentContact
- [ ] 实现 btnBindingContacts_Click
- [ ] 实现 btnGetContactList_Click
- [ ] 在 Program.cs 注册服务
- [ ] 测试绑定联系人功能
- [ ] 测试获取联系人列表功能

## 📝 测试步骤

1. 启动 BaiShengVx3Plus
2. 登录系统
3. 添加测试联系人数据
4. 选择一个联系人
5. 点击"绑定"按钮 → txtCurrentContact 显示 Wxid
6. 确保 WeixinX.dll 在程序目录
7. 点击"获取列表"按钮 → 启动微信或注入到现有进程
8. 验证微信是否成功注入

## 🎯 最终效果

```
┌──────────────────────────────────────────┐
│ [绑定] [刷新] [获取列表]                  │
│ ┌────────────────────────────────────┐  │
│ │ wxid_001                            │  │ ← txtCurrentContact (只读)
│ └────────────────────────────────────┘  │
│ ┌────────┬──────────────────────────┐  │
│ │   ID   │        昵称               │  │
│ ├────────┼──────────────────────────┤  │
│ │wxid_001│   联系人1                │  │ ← dgvContacts
│ │wxid_002│   联系人2                │  │
│ └────────┴──────────────────────────┘  │
└──────────────────────────────────────────┘
```

