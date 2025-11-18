# 百胜VX3Plus 管理系统

基于 .NET 8.0 和 SunnyUI 的现代化WinForms应用程序，采用MVVM架构模式。

## 🎯 项目特点

- **现代化UI**: 使用SunnyUI 3.6.9 UI库，界面美观现代
- **MVVM架构**: 完整的Model-View-ViewModel设计模式
- **依赖注入**: 使用Microsoft.Extensions.DependencyInjection
- **可设计器编辑**: 所有界面支持Visual Studio设计器可视化编辑
- **窗口尺寸**: 980 x 762 (根据原始设计)

## 📁 项目结构

```
BaiShengVx3Plus/
├── Core/                      # 核心基础设施
│   ├── ViewModelBase.cs       # ViewModel基类
│   └── RelayCommand.cs        # 命令实现
├── Models/                    # 数据模型
│   ├── User.cs                # 用户模型
│   └── InsUser.cs             # InsUser模型
├── Services/                  # 服务层
│   ├── IAuthService.cs        # 认证服务接口
│   ├── AuthService.cs         # 认证服务实现
│   ├── IInsUserService.cs     # 数据服务接口
│   └── InsUserService.cs      # 数据服务实现
├── ViewModels/                # 视图模型
│   ├── LoginViewModel.cs      # 登录页面ViewModel
│   └── VxMainViewModel.cs     # 主界面ViewModel
├── Views/                     # 视图
│   ├── LoginForm.cs           # 登录窗体
│   ├── LoginForm.Designer.cs  # 登录窗体设计器
│   └── LoginForm.resx         # 登录窗体资源
├── VxMain.cs                  # 主窗体
├── VxMain.Designer.cs         # 主窗体设计器
├── VxMain.resx                # 主窗体资源
└── Program.cs                 # 程序入口
```

## 🚀 技术栈

- **.NET 8.0** - 最新的.NET平台
- **WinForms** - Windows桌面应用框架
- **SunnyUI 3.6.9** - 现代化UI组件库
- **CommunityToolkit.Mvvm 8.2.2** - MVVM工具包
- **Microsoft.Extensions.DependencyInjection** - 依赖注入
- **Microsoft.Extensions.Hosting** - 主机支持

## 🎨 功能特性

### 登录系统
- 用户名/密码验证
- 记住密码功能
- 异步登录处理
- 错误提示

### 主界面
- **左侧面板**: 用户列表显示（支持刷新）
- **右侧标签页**:
  - **开发测试中**: 
    - InsUser详细信息编辑
    - 实时进度显示
    - 功能按钮区（添加、设置、订单管理、微信数据卡管理等）
  - **日志**: 系统日志显示
- **状态栏**: 实时状态信息

### 按钮功能
- ➕ 添加用户
- 🔧 设置
- 📊 微信数据卡管理
- 📋 订单管理
- 🔑 修改密码
- 💰 充值
- 🔄 转分

## 💻 开发指南

### 前置要求
- Visual Studio 2022 或更高版本
- .NET 8.0 SDK

### 运行项目

1. 还原NuGet包:
```bash
dotnet restore
```

2. 编译项目:
```bash
dotnet build
```

3. 运行项目:
```bash
dotnet run
```

### 默认登录凭据
- 用户名: `admin`
- 密码: `admin`

## 🎯 MVVM模式说明

### ViewModel
- 继承自 `ViewModelBase`（使用CommunityToolkit.Mvvm的ObservableObject）
- 使用 `[ObservableProperty]` 特性自动生成属性
- 使用 `[RelayCommand]` 特性自动生成命令

### 数据绑定
```csharp
// 在ViewModel中定义可观察属性
[ObservableProperty]
private string _username = string.Empty;

// 在View中绑定
txtUsername.TextChanged += (s, e) => _viewModel.Username = txtUsername.Text;

// 监听ViewModel变化
_viewModel.PropertyChanged += (s, e) =>
{
    if (e.PropertyName == nameof(_viewModel.Username))
    {
        // 更新UI
    }
};
```

### 依赖注入
```csharp
// 在Program.cs中注册服务
services.AddSingleton<IAuthService, AuthService>();
services.AddTransient<LoginViewModel>();
services.AddTransient<LoginForm>();

// 在构造函数中注入
public LoginForm(LoginViewModel viewModel)
{
    _viewModel = viewModel;
}
```

## 📝 界面编辑

所有窗体都支持Visual Studio设计器编辑：

1. 在解决方案资源管理器中双击 `.Designer.cs` 文件
2. 或右键点击窗体文件 -> 查看设计器
3. 使用工具箱拖放SunnyUI控件
4. 使用属性窗口调整控件属性

## 🔧 扩展功能

### 添加新页面

1. 创建Model (如果需要)
```csharp
public class MyModel 
{
    public string Name { get; set; }
}
```

2. 创建ViewModel
```csharp
public partial class MyViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _title = string.Empty;
    
    [RelayCommand]
    private void DoSomething()
    {
        // 逻辑处理
    }
}
```

3. 创建View (WinForms窗体)
```csharp
public partial class MyForm : UIForm
{
    private readonly MyViewModel _viewModel;
    
    public MyForm(MyViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindViewModel();
    }
}
```

4. 在Program.cs中注册
```csharp
services.AddTransient<MyViewModel>();
services.AddTransient<MyForm>();
```

### 添加新服务

1. 定义接口
```csharp
public interface IMyService
{
    Task<string> GetDataAsync();
}
```

2. 实现服务
```csharp
public class MyService : IMyService
{
    public async Task<string> GetDataAsync()
    {
        // 实现逻辑
        return await Task.FromResult("Data");
    }
}
```

3. 注册服务
```csharp
services.AddSingleton<IMyService, MyService>();
```

## 📄 许可证

本项目仅供学习和参考使用。

## 👥 贡献

欢迎提交Issue和Pull Request！

## 📮 联系方式

如有问题，请创建Issue。

