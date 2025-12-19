# 永利系统 - MVVM 架构设计文档

## 🏗️ 架构概述

本项目采用 **MVVM (Model-View-ViewModel)** 设计模式，将界面展示、业务逻辑和数据模型完全分离，实现高内聚、低耦合的现代化架构。

---

## 📊 架构层次

```
┌─────────────────────────────────────────────────────────┐
│                    View Layer (视图层)                   │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  │
│  │  Main.cs     │  │ Dashboard    │  │ DataMgmt     │  │
│  │ (RibbonForm) │  │   Page.cs    │  │   Page.cs    │  │
│  └──────────────┘  └──────────────┘  └──────────────┘  │
└───────────────────────────┬─────────────────────────────┘
                            │ Data Binding & Commands
                            ▼
┌─────────────────────────────────────────────────────────┐
│                 ViewModel Layer (视图模型层)              │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  │
│  │   Main       │  │  Dashboard   │  │  DataMgmt    │  │
│  │ ViewModel    │  │  ViewModel   │  │  ViewModel   │  │
│  └──────────────┘  └──────────────┘  └──────────────┘  │
└───────────────────────────┬─────────────────────────────┘
                            │ Business Logic
                            ▼
┌─────────────────────────────────────────────────────────┐
│                   Model Layer (模型层)                    │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  │
│  │  DataItem    │  │   User       │  │   Config     │  │
│  │    Model     │  │   Model      │  │   Model      │  │
│  └──────────────┘  └──────────────┘  └──────────────┘  │
└───────────────────────────┬─────────────────────────────┘
                            │ Data Access
                            ▼
┌─────────────────────────────────────────────────────────┐
│               Services Layer (服务层)                     │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  │
│  │  Database    │  │  Navigation  │  │    Logger    │  │
│  │  Service     │  │  Service     │  │   Service    │  │
│  └──────────────┘  └──────────────┘  └──────────────┘  │
└─────────────────────────────────────────────────────────┘
```

---

## 🔧 核心组件详解

### 1. View Layer (视图层)

**职责：**
- 负责 UI 渲染和用户交互
- 不包含业务逻辑
- 通过数据绑定与 ViewModel 通信

**关键文件：**
- `Views/Main.cs` - 主窗口（RibbonForm）
- `Views/Pages/DashboardPage.cs` - 首页
- `Views/Pages/DataManagementPage.cs` - 数据管理页

**设计原则：**
```csharp
// ✅ 正确：使用数据绑定
lblTitle.DataBindings.Add("Text", _viewModel, nameof(_viewModel.Title));

// ❌ 错误：在 View 中直接处理业务逻辑
private void btnSave_Click(object sender, EventArgs e)
{
    // 不要在这里写数据库操作等业务逻辑
    database.Save(data); // ❌
}
```

### 2. ViewModel Layer (视图模型层)

**职责：**
- 封装界面所需的数据和命令
- 处理业务逻辑
- 通过 `INotifyPropertyChanged` 通知 View 更新

**关键文件：**
- `ViewModels/MainViewModel.cs`
- `ViewModels/DashboardViewModel.cs`
- `ViewModels/DataManagementViewModel.cs`

**核心特性：**

#### 属性变更通知
```csharp
public class MainViewModel : ViewModelBase
{
    private string _statusMessage;
    
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value); // 自动通知
    }
}
```

#### 命令模式
```csharp
public ICommand SaveCommand { get; private set; }

private void InitializeCommands()
{
    SaveCommand = new RelayCommand(
        execute: _ => SaveData(),
        canExecute: _ => !IsBusy && HasChanges()
    );
}

private void SaveData()
{
    IsBusy = true;
    try
    {
        // 执行保存逻辑
        _dataService.Save(Data);
        StatusMessage = "保存成功";
    }
    catch (Exception ex)
    {
        StatusMessage = $"保存失败: {ex.Message}";
    }
    finally
    {
        IsBusy = false;
    }
}
```

### 3. Model Layer (模型层)

**职责：**
- 定义数据结构
- 纯数据容器，不包含业务逻辑

**示例：**
```csharp
public class DataItem
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreateTime { get; set; }
    public bool IsActive { get; set; }
}
```

### 4. Core Layer (核心框架层)

**职责：**
- 提供 MVVM 基础设施
- 可复用的核心组件

**组件：**

#### ObservableObject
```csharp
public abstract class ObservableObject : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    
    protected bool SetProperty<T>(ref T field, T value, 
        [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;
            
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
```

#### RelayCommand
```csharp
public class RelayCommand : ICommand
{
    private readonly Action<object> _execute;
    private readonly Func<object, bool> _canExecute;
    
    public void Execute(object parameter) => _execute(parameter);
    public bool CanExecute(object parameter) => _canExecute?.Invoke(parameter) ?? true;
}
```

#### NavigationService
```csharp
public class NavigationService
{
    private readonly Panel _contentPanel;
    private readonly Dictionary<string, UserControl> _pages;
    
    public void NavigateTo(string pageKey)
    {
        // 切换页面逻辑
    }
}
```

---

## 🔄 数据流向

### 用户操作流程

```
1. 用户点击按钮
   │
   ▼
2. View 触发命令
   button.Click += (s, e) => _viewModel.SaveCommand.Execute(null);
   │
   ▼
3. ViewModel 执行业务逻辑
   private void SaveData()
   {
       _dataService.Save(Data);
       StatusMessage = "保存成功";  // 修改属性
   }
   │
   ▼
4. 属性变更通知
   SetProperty(ref _statusMessage, value);
   │
   ▼
5. View 自动更新
   lblStatus.Text = _viewModel.StatusMessage;  // 通过数据绑定自动更新
```

### 数据加载流程

```
1. ViewModel 初始化
   │
   ▼
2. 调用 Service 获取数据
   var data = await _dataService.GetDataAsync();
   │
   ▼
3. 更新 ViewModel 属性
   DataItems = new ObservableCollection<DataItem>(data);
   │
   ▼
4. View 自动更新
   gridControl.DataSource = _viewModel.DataItems;
```

---

## 🎯 设计模式

### 1. MVVM Pattern
- **Model**: 数据模型
- **View**: UI 视图
- **ViewModel**: 视图模型（桥接 Model 和 View）

### 2. Command Pattern
- 使用 `ICommand` 接口封装用户操作
- 支持 `Execute` 和 `CanExecute` 逻辑分离

### 3. Observer Pattern
- 通过 `INotifyPropertyChanged` 实现观察者模式
- ViewModel 变更自动通知 View

### 4. Service Locator Pattern
- 导航服务统一管理页面
- 可扩展为依赖注入（DI）

---

## 📋 命名规范

### ViewModel
```csharp
// 格式: {功能}ViewModel
public class MainViewModel : ViewModelBase { }
public class DashboardViewModel : ViewModelBase { }
```

### View
```csharp
// 格式: {功能}Page 或 {功能}Form
public partial class DashboardPage : UserControl { }
public partial class Main : RibbonForm { }
```

### 属性
```csharp
// 私有字段: _camelCase
private string _statusMessage;

// 公共属性: PascalCase
public string StatusMessage { get; set; }
```

### 命令
```csharp
// 格式: {动作}Command
public ICommand SaveCommand { get; private set; }
public ICommand DeleteCommand { get; private set; }
public ICommand NavigateToCommand { get; private set; }
```

---

## 🚀 扩展指南

### 添加新功能模块

#### Step 1: 创建 Model
```csharp
// Models/Product.cs
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}
```

#### Step 2: 创建 ViewModel
```csharp
// ViewModels/ProductViewModel.cs
public class ProductViewModel : ViewModelBase
{
    private ObservableCollection<Product> _products;
    
    public ObservableCollection<Product> Products
    {
        get => _products;
        set => SetProperty(ref _products, value);
    }
    
    public ICommand LoadProductsCommand { get; private set; }
    
    public ProductViewModel()
    {
        LoadProductsCommand = new RelayCommand(_ => LoadProducts());
        LoadProducts();
    }
    
    private void LoadProducts()
    {
        // 加载数据逻辑
    }
}
```

#### Step 3: 创建 View
```csharp
// Views/Pages/ProductPage.cs
public partial class ProductPage : UserControl
{
    private readonly ProductViewModel _viewModel;
    
    public ProductPage()
    {
        InitializeComponent();
        _viewModel = new ProductViewModel();
        gridControl.DataSource = _viewModel.Products;
    }
}
```

#### Step 4: 注册导航
```csharp
// Views/Main.cs - InitializeNavigation()
_navigationService.RegisterPage("Products", new ProductPage());
```

#### Step 5: 添加 Ribbon 按钮
在设计器中添加按钮，并绑定事件：
```csharp
private void barButtonItemProducts_ItemClick(object sender, ItemClickEventArgs e)
{
    _navigationService?.NavigateTo("Products");
}
```

---

## 🔐 最佳实践

### 1. View 层
✅ **应该做的：**
- 只负责 UI 展示
- 使用数据绑定
- 响应用户交互并调用 ViewModel 命令

❌ **不应该做的：**
- 不要包含业务逻辑
- 不要直接访问数据库
- 不要在代码中硬编码数据

### 2. ViewModel 层
✅ **应该做的：**
- 封装业务逻辑
- 提供数据和命令供 View 绑定
- 使用 `SetProperty` 通知属性变更

❌ **不应该做的：**
- 不要引用 View 类型
- 不要使用 MessageBox（应该通过事件或服务）
- 不要直接操作 UI 控件

### 3. Model 层
✅ **应该做的：**
- 纯数据结构
- 简单的验证逻辑（如字段长度）

❌ **不应该做的：**
- 不要包含业务逻辑
- 不要引用 ViewModel 或 View

---

## 📈 性能优化建议

### 1. 使用异步操作
```csharp
public async Task LoadDataAsync()
{
    IsBusy = true;
    try
    {
        var data = await _dataService.GetDataAsync();
        DataItems = new ObservableCollection<DataItem>(data);
    }
    finally
    {
        IsBusy = false;
    }
}
```

### 2. 懒加载
```csharp
private ProductViewModel _productViewModel;
public ProductViewModel ProductViewModel => 
    _productViewModel ??= new ProductViewModel();
```

### 3. 虚拟化大数据集
```csharp
// 在 GridControl 中启用虚拟化
gridView.OptionsView.EnableVirtualScrolling = true;
```

---

## 🎓 学习资源

- **MVVM Pattern**: https://learn.microsoft.com/en-us/dotnet/architecture/maui/mvvm
- **DevExpress WinForms**: https://docs.devexpress.com/WindowsForms/
- **C# 最佳实践**: https://learn.microsoft.com/en-us/dotnet/csharp/

---

## ✅ 架构优势

1. **可维护性**: 代码结构清晰，易于理解和修改
2. **可测试性**: ViewModel 可以独立于 UI 进行单元测试
3. **可扩展性**: 新功能模块化添加，不影响现有代码
4. **代码复用**: Core 层组件可在多个模块中复用
5. **团队协作**: 前端和后端可以并行开发

---

**本架构设计遵循 SOLID 原则，为复杂数据管理系统提供坚实的基础。** 🚀

