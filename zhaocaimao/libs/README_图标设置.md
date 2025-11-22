# 招财猫图标设置指南

## 📋 快速设置步骤

### 1. 获取图标文件

您需要准备一个招财猫图标文件（.ico 格式），命名为 `zhaocaimao.ico`。

**推荐获取方式：**

#### 方式一：在线图标库下载
- 访问 [iconfont.cn](https://www.iconfont.cn) 搜索"招财猫"
- 访问 [flaticon.com](https://www.flaticon.com) 搜索"lucky cat"
- 下载后转换为 .ico 格式

#### 方式二：PNG 转 ICO
1. 找到招财猫的 PNG 图片
2. 使用在线转换工具：
   - [convertio.co](https://convertio.co/zh/png-ico/)
   - [ico-convert.com](https://www.ico-convert.com/)
3. 转换时选择多个尺寸（16x16, 32x32, 48x48, 64x64, 128x128, 256x256）

#### 方式三：使用图标编辑软件
- **IcoFX**（Windows，免费版可用）
- **Greenfish Icon Editor Pro**（免费）
- **GIMP**（免费，需要插件）

### 2. 放置图标文件

将 `zhaocaimao.ico` 文件放置到以下位置：

```
zhaocaimao/libs/zhaocaimao.ico
```

### 3. 验证配置

项目已配置完成：
- ✅ 应用程序图标：已在 `zhaocaimao.csproj` 中配置
- ✅ 窗口图标：已在 `VxMain.cs` 中配置
- ✅ 图标文件路径：`libs/zhaocaimao.ico`

### 4. 编译和测试

1. 确保 `libs/zhaocaimao.ico` 文件存在
2. 编译项目：`dotnet build`
3. 运行程序，检查：
   - ✅ 桌面快捷方式图标
   - ✅ 任务栏图标
   - ✅ 程序窗口标题栏图标

## 🔧 图标要求

- **文件格式**：`.ico`（Windows 图标格式）
- **文件大小**：建议 < 500KB
- **图标尺寸**：建议包含多个尺寸（16x16 到 256x256）
- **背景**：建议透明背景或单色背景

## ⚠️ 注意事项

1. 图标文件必须命名为：`zhaocaimao.ico`（区分大小写）
2. 图标文件必须放在 `libs` 文件夹中
3. 修改图标后需要**重新编译**项目
4. 如果桌面图标未更新，可能需要：
   - 刷新桌面（F5）
   - 删除旧快捷方式，重新创建
   - 清除图标缓存（可能需要重启）

## 📝 项目配置说明

### 应用程序图标（桌面和任务栏）
在 `zhaocaimao.csproj` 中已配置：
```xml
<ApplicationIcon>libs\zhaocaimao.ico</ApplicationIcon>
```

### 窗口图标（程序窗口标题栏）
在 `VxMain.cs` 的 `VxMain_Load` 方法中已配置：
```csharp
this.Icon = new Icon(iconPath);
```

## 🎨 图标设计建议

- 使用招财猫的经典形象（举右手、金色等）
- 保持简洁，在小尺寸下也能清晰识别
- 使用高对比度，确保在各种背景下都清晰可见
- 建议使用透明背景

---

**提示**：如果暂时没有图标文件，程序仍可正常运行，只是会使用默认图标。

