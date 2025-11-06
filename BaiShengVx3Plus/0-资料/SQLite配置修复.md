# SQLite 配置修复

## 问题描述

登录后 `VxMain` 无法加载，原因是缺少 SQLite 原生运行时文件。

---

## 🔧 修复方案

### 1. 更换 SQLite Bundle 包

**原配置**（有问题）：
```xml
<PackageReference Include="SQLitePCLRaw.bundle_e_sqlite3" Version="2.1.10" />
```

**新配置**（参考 F5BotV2）：
```xml
<PackageReference Include="SQLitePCLRaw.bundle_green" Version="2.1.10" />
```

### 2. 初始化 SQLite（Program.cs）

在 `Main()` 方法最开始添加：
```csharp
// 🔥 初始化 SQLite 原生库（必须在最前面）
SQLitePCL.Batteries.Init();
```

---

## 📦 SQLite Bundle 包说明

### bundle_green
- **特点**: 包含预编译的原生 SQLite 库
- **平台**: Windows、Linux、macOS
- **文件**: 自动复制到输出目录
- **推荐**: ✅ 用于 Windows 桌面应用

### bundle_e_sqlite3
- **特点**: 使用系统自带的 SQLite 库
- **平台**: 依赖系统环境
- **文件**: 需要系统提供 `e_sqlite3.dll`
- **推荐**: ❌ 不推荐用于 Windows 应用

---

## 🚀 编译和运行

### 方法 1: 使用批处理脚本

```bash
cd BaiShengVx3Plus
restore_and_build.bat
```

### 方法 2: Visual Studio

1. **卸载并重新加载项目**
   - 右键项目 → 卸载项目
   - 右键项目 → 重新加载项目

2. **恢复 NuGet 包**
   - 右键解决方案 → 还原 NuGet 包

3. **清理并重新生成**
   - 生成 → 清理解决方案
   - 生成 → 重新生成解决方案

4. **运行**
   - 按 F5 或 Ctrl+F5

### 方法 3: 命令行

```bash
cd BaiShengVx3Plus
dotnet clean
dotnet restore
dotnet build --configuration Debug
dotnet run --configuration Debug
```

---

## ✅ 验证修复

### 检查原生 DLL

编译成功后，检查输出目录：

```
bin/Debug/net8.0-windows/
├── runtimes/
│   ├── win-x64/
│   │   └── native/
│   │       └── sqlite3.dll  ✅ (bundle_green)
│   └── ...
```

**注意**: `bundle_green` 会生成 `sqlite3.dll`，而不是 `e_sqlite3.dll`。

### 运行测试

1. 启动程序
2. 登录成功
3. **VxMain 正常加载** ✅
4. 数据库文件正常创建：
   - `Data/logs.db`
   - `Data/business_{wxid}.db`

---

## 🎯 修复总结

| 项目                | 修复前                          | 修复后                        |
|---------------------|--------------------------------|------------------------------|
| SQLite Bundle       | `bundle_e_sqlite3`             | `bundle_green` ✅            |
| 原生 DLL            | `e_sqlite3.dll` (缺失)         | `sqlite3.dll` (自动)         |
| 初始化代码          | ❌ 无                          | `SQLitePCL.Batteries.Init()` ✅ |
| VxMain 加载         | ❌ 失败                        | ✅ 成功                      |

---

## 📚 参考

- **F5BotV2**: 使用 `SQLitePCLRaw.bundle_green` (Version 2.1.7)
- **BaiShengVx3Plus**: 使用 `SQLitePCLRaw.bundle_green` (Version 2.1.10)

---

**修复日期**: 2025-11-06  
**状态**: ✅ 已修复

