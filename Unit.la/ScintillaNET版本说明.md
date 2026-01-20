# ScintillaNET 版本说明

## 问题

原始配置使用了不存在的版本 `5.3.0`，导致 NuGet 恢复失败。

## 解决方案

已更新为 `Scintilla.NET` 版本 `5.3.2.9`。

## 包名说明

- **NuGet 包名**: `Scintilla.NET`（带点）
- **命名空间**: `ScintillaNET`（不带点）
- **版本**: `5.3.2.9`

## 如果仍然遇到问题

### 选项1：使用 Scintilla5.NET（推荐）

如果 `Scintilla.NET` 仍然有问题，可以改用官方推荐的替代库：

```xml
<PackageReference Include="Scintilla5.NET" Version="5.3.3.10" />
```

然后需要更新代码中的命名空间（可能需要改为 `Scintilla5` 或其他，请参考包文档）。

### 选项2：使用较低版本

如果 5.3.2.9 不可用，可以尝试：
- `3.7.5` - 较旧的稳定版本
- `2.6.0` - 更旧的版本（根据错误信息，这个版本存在）

## 当前配置

```xml
<PackageReference Include="Scintilla.NET" Version="5.3.2.9" />
```

代码中使用：
```csharp
using ScintillaNET;
```

## 验证

编译项目，如果仍然报错，请检查：
1. NuGet 源配置是否正确
2. 网络连接是否正常
3. 是否需要使用代理
