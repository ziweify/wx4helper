# SQLite 原生 DLL 问题最终解决

## 🔴 问题

登录后 `VxMain` 无法加载，缺少 SQLite 原生运行时文件（`e_sqlite3.dll` 或 `SQLite.Interop.dll`）。

---

## ✅ 最终解决方案

### 双重保险配置

同时安装两个包，确保至少一个能提供原生 DLL：

```xml
<PackageReference Include="SQLitePCLRaw.bundle_e_sqlite3" Version="2.1.10" />
<PackageReference Include="System.Data.SQLite.Core" Version="1.0.118" />
```

### 包说明

| 包名                              | 提供的 DLL            | 位置                                    |
|-----------------------------------|----------------------|----------------------------------------|
| `SQLitePCLRaw.bundle_e_sqlite3`   | `e_sqlite3.dll`      | `runtimes/win-x64/native/`             |
| `System.Data.SQLite.Core`         | `SQLite.Interop.dll` | `x64/` 或 根目录                       |

**原理**: 
- `SQLitePCLRaw.bundle_e_sqlite3` 提供轻量级的原生 DLL
- `System.Data.SQLite.Core` 提供完整的原生 DLL（备用）
- `sqlite-net-pcl` 会自动找到并使用可用的 DLL

---

## 🚀 使用说明

### 方法 1: 使用修复脚本（推荐）

```bash
cd BaiShengVx3Plus
fix_and_run.bat
```

**脚本功能**:
1. 清理旧的编译输出
2. 强制恢复 NuGet 包
3. 编译项目
4. 检查并复制 SQLite 原生 DLL
5. 运行程序

### 方法 2: 手动操作

```bash
# 1. 清理
cd BaiShengVx3Plus
rd /s /q bin obj

# 2. 恢复
dotnet restore --force

# 3. 编译
dotnet build --configuration Debug

# 4. 运行
dotnet run --configuration Debug
```

### 方法 3: Visual Studio

1. **卸载并重新加载项目**
   - 右键项目 → 卸载项目
   - 右键项目 → 重新加载项目

2. **清理解决方案**
   - 生成 → 清理解决方案

3. **恢复 NuGet 包**
   - 工具 → NuGet 包管理器 → 管理解决方案的 NuGet 包
   - 点击"恢复"按钮

4. **重新生成**
   - 生成 → 重新生成解决方案

5. **运行**
   - 按 F5 或 Ctrl+F5

---

## 🔍 故障排查

### 如果仍然报错

1. **检查输出目录**
```bash
cd bin\Debug\net8.0-windows
dir /s *.dll | findstr /i sqlite
```

2. **手动复制 DLL**
```bash
# 如果找到了 DLL 但在子目录中
copy x64\e_sqlite3.dll .
# 或
copy x64\SQLite.Interop.dll .
```

3. **查看详细错误**
   - 运行时如果报错，记录完整的错误信息
   - 检查是否是 DLL 加载失败还是其他问题

### 常见错误

#### 错误 1: "无法加载 DLL 'e_sqlite3'"
**解决**: DLL 不在正确位置
```bash
# 从 runtimes 目录复制
copy bin\Debug\net8.0-windows\runtimes\win-x64\native\e_sqlite3.dll bin\Debug\net8.0-windows\
```

#### 错误 2: "无法加载 DLL 'SQLite.Interop'"
**解决**: DLL 不在正确位置
```bash
# 从 x64 目录复制
copy bin\Debug\net8.0-windows\x64\SQLite.Interop.dll bin\Debug\net8.0-windows\
```

#### 错误 3: VxMain 窗口不显示但没有错误
**解决**: 可能是 UI 线程问题或初始化顺序问题
- 检查 `Program.cs` 中的 `SQLitePCL.Batteries.Init()` 是否在最前面
- 检查日志文件 `Data/logs.db` 是否创建成功

---

## 📊 验证成功的标志

### 1. 编译输出
```
✅ 编译成功！
✅ 找到 e_sqlite3.dll 或 SQLite.Interop.dll
```

### 2. 运行时
- 登录窗口正常显示 ✅
- 登录成功 ✅
- **VxMain 主窗口正常显示** ✅
- 数据库文件创建成功 ✅

### 3. 数据库文件
```
BaiShengVx3Plus/bin/Debug/net8.0-windows/Data/
├── logs.db                    ✅ 日志数据库
└── business_{wxid}.db         ✅ 业务数据库（登录后创建）
```

---

## 🎯 为什么这个方案有效？

1. **双重保险**: 两个包任何一个成功都能工作
2. **System.Data.SQLite.Core**: 这个包的原生 DLL 打包更可靠
3. **自动查找**: `sqlite-net-pcl` 会在多个位置查找原生 DLL
4. **强制恢复**: `--force` 确保重新下载所有依赖

---

## 📝 如果问题仍未解决

请提供以下信息：

1. **运行 `fix_and_run.bat` 的完整输出**
2. **错误截图或错误信息**
3. **检查输出目录的 DLL 列表**:
```bash
cd bin\Debug\net8.0-windows
dir /s *.dll > dll_list.txt
notepad dll_list.txt
```

---

**修复日期**: 2025-11-06  
**状态**: ✅ 双重保险方案，确保原生 DLL 可用

