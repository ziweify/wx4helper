# 🚀 快速入门指南

## 运行项目

### 方法1: 使用 Visual Studio 2022
1. 双击打开 `BaiShengVx3Plus.csproj`
2. 按 `F5` 或点击"启动"按钮
3. 登录窗口将自动打开

### 方法2: 使用命令行
```bash
cd BaiShengVx3Plus
dotnet run
```

## 默认登录信息
- **用户名**: `admin`
- **密码**: `admin`

## 界面说明

### 登录界面
- 输入用户名和密码
- 可选择"记住密码"
- 点击"登录"按钮或按回车键登录

### 主界面布局（980 x 762）

```
┌─────────────────────────────────────────────────────────┐
│  百胜VX3Plus - 管理系统                                   │
├───────┬─────────────────────────────────────────────────┤
│       │ [开发测试中] [日志]                              │
│ 用户  ├─────────────────────────────────────────────────┤
│ 列表  │ [添加] [微信数据卡] [订单] [密码] [充值] [转分]  │
│       ├─────────────────────────────────────────────────┤
│ ┌───┐ │ ┌─ InsUser - 真号VIP ────────────────────┐    │
│ │刷新│ │ │ ID: 111065741    名称: 开发测试中      │    │
│ └───┘ │ │ 账号: wwwww11    密码: ******           │    │
│       │ │ 地址: ___________                      │    │
│  [用  │ │ 上一次: 2024-01-01  当前: 2024-01-01    │    │
│   户  │ │ 余额: 2354.00      秒数: 3000          │    │
│   1]  │ │ [提交]                                 │    │
│       │ └────────────────────────────────────────┘    │
│  [用  │                                               │
│   户  │ ┌─进度条─────────────────────────────────┐    │
│   2]  │ │ 上一次: 123456789 当前: 13:19:23 剩3000│    │
│       │ │ ████████████░░░░░░░░░░░░░░░░░░░░      │    │
│       │ └────────────────────────────────────────┘    │
│       │                                      [设置]    │
├───────┴─────────────────────────────────────────────────┤
│ 状态: 就绪                                               │
└─────────────────────────────────────────────────────────┘
```

### 功能按钮
- **➕ 添加**: 添加新用户
- **🔧 设置**: 系统设置
- **📊 微信数据卡管理**: 管理微信数据
- **📋 订单管理**: 查看和管理订单
- **🔑 修改密码**: 修改当前用户密码
- **💰 充值**: 账户充值
- **🔄 转分**: 积分转账
- **🔄 刷新**: 刷新用户列表

## 在设计器中编辑界面

### 编辑登录界面
1. 在解决方案资源管理器中，找到 `Views/LoginForm.cs`
2. 右键点击 -> **查看设计器** (或双击)
3. 从工具箱拖放SunnyUI控件
4. 使用属性窗口调整控件属性

### 编辑主界面
1. 在解决方案资源管理器中，找到 `VxMain.cs`
2. 右键点击 -> **查看设计器**
3. 调整控件位置、大小、颜色等

## 自定义开发

### 1. 修改主题颜色
在 `LoginForm.Designer.cs` 或 `VxMain.Designer.cs` 中调整：
```csharp
// 修改按钮颜色
btnLogin.FillColor = Color.FromArgb(80, 160, 255);

// 修改面板背景色
pnlMain.BackColor = Color.FromArgb(243, 249, 255);
```

### 2. 添加新功能
在 `VxMainViewModel.cs` 中添加命令：
```csharp
[RelayCommand]
private void MyNewFeature()
{
    StatusMessage = "新功能执行中...";
    // 实现你的逻辑
}
```

在 `VxMain.Designer.cs` 中添加按钮并绑定：
```csharp
// 在设计器中添加UIButton
private Sunny.UI.UIButton btnNewFeature;

// 在InitializeComponent中初始化
btnNewFeature.Click += (s, e) => _viewModel.MyNewFeatureCommand.Execute(null);
```

### 3. 连接真实数据库
修改 `Services/AuthService.cs` 和 `Services/InsUserService.cs`:
```csharp
// 替换模拟数据为真实数据库调用
public async Task<(bool Success, string Message, User? User)> LoginAsync(string username, string password)
{
    // TODO: 连接数据库验证
    using var connection = new SqlConnection(connectionString);
    // ... 数据库操作
}
```

## SunnyUI 控件说明

项目使用了以下SunnyUI控件：

- `UIForm`: 窗体基类
- `UIPanel`: 面板容器
- `UIButton`: 按钮
- `UITextBox`: 文本框
- `UILabel`: 标签
- `UICheckBox`: 复选框
- `UIDataGridView`: 数据表格
- `UITabControl`: 选项卡
- `UIGroupBox`: 分组框
- `UIIntegerUpDown`: 数字输入框
- `UIProcessBar`: 进度条
- `UIRichTextBox`: 富文本框
- `UISplitContainer`: 分割容器

详细文档: [SunnyUI 官方文档](https://gitee.com/yhuse/SunnyUI)

## MVVM 数据绑定示例

### 在ViewModel中定义属性
```csharp
[ObservableProperty]
private string _userName = string.Empty;

// 自动生成: public string UserName { get; set; }
```

### 在View中绑定
```csharp
// 单向绑定: VM -> View
_viewModel.PropertyChanged += (s, e) =>
{
    if (e.PropertyName == nameof(_viewModel.UserName))
    {
        txtUserName.Text = _viewModel.UserName;
    }
};

// 双向绑定: View -> VM
txtUserName.TextChanged += (s, e) => _viewModel.UserName = txtUserName.Text;
```

### 命令绑定
```csharp
// 在ViewModel中
[RelayCommand]
private async Task SaveAsync()
{
    // 异步操作
}

// 在View中
btnSave.Click += async (s, e) => await _viewModel.SaveCommand.ExecuteAsync(null);
```

## 常见问题

### Q: 如何更改窗口大小？
A: 在 `VxMain.Designer.cs` 中修改：
```csharp
ClientSize = new Size(980, 762);
```

### Q: 如何添加图标？
A: 在设计器中选择窗体，在属性窗口中设置 `Icon` 属性

### Q: 登录失败怎么办？
A: 检查 `Services/AuthService.cs` 中的验证逻辑，默认用户名密码都是 `admin`

### Q: 如何调试MVVM绑定？
A: 在 `PropertyChanged` 事件处理中设置断点：
```csharp
_viewModel.PropertyChanged += (s, e) =>
{
    // 在这里设置断点查看属性变化
    Debug.WriteLine($"Property changed: {e.PropertyName}");
};
```

## 下一步

1. ✅ 连接真实数据库
2. ✅ 实现完整的用户管理功能
3. ✅ 添加更多业务功能
4. ✅ 优化UI/UX
5. ✅ 添加日志记录
6. ✅ 实现权限管理

## 技术支持

遇到问题？
1. 查看 `README.md` 了解项目结构
2. 查看 SunnyUI 官方文档
3. 查看 .NET 8.0 官方文档

祝开发愉快！🎉

