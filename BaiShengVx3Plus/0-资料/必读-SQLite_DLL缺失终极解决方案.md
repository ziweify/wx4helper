# 🔴 必读：SQLite DLL 缺失终极解决方案

## 问题症状

✅ 编译成功  
✅ 登录窗口显示  
✅ 登录成功  
❌ **VxMain 主窗口不显示**  
❌ **输出目录缺少 e_sqlite3.dll**

---

## 🎯 终极解决方案（3步走）

### 第1步：获取 e_sqlite3.dll

运行自动查找脚本：
```bash
cd D:\gitcode\wx4helper\BaiShengVx3Plus
find_and_copy_sqlite_dll.bat
```

**脚本会自动从以下位置查找并复制**：
1. NuGet 缓存 (`%USERPROFILE%\.nuget\packages\`)
2. F5BotV2 的 Build 目录 (`../Build/`)
3. F5BotV2 的 Debug 目录 (`../F5BotV2/bin/Debug/`)

**如果脚本未找到，手动复制**：
```bash
# 方法1: 从 F5BotV2 复制
copy ..\Build\e_sqlite3.dll libs\e_sqlite3.dll

# 方法2: 从 F5BotV2 Debug 复制
copy ..\F5BotV2\bin\Debug\e_sqlite3.dll libs\e_sqlite3.dll
```

### 第2步：编译项目

```bash
cd D:\gitcode\wx4helper\BaiShengVx3Plus
dotnet clean
dotnet restore --force
dotnet build --configuration Debug
```

**项目文件已配置自动复制**：
- `libs\e_sqlite3.dll` → `bin\Debug\net8.0-windows\e_sqlite3.dll`

### 第3步：运行程序

```bash
dotnet run --configuration Debug
```

---

## 🚀 一键解决（推荐）

### 方案A：分步执行（推荐）

```bash
cd D:\gitcode\wx4helper\BaiShengVx3Plus

# 1. 获取 DLL
find_and_copy_sqlite_dll.bat

# 2. 编译和运行
fix_and_run.bat
```

### 方案B：手动操作

```bash
# 1. 手动复制 DLL
cd D:\gitcode\wx4helper\BaiShengVx3Plus
if not exist libs mkdir libs
copy ..\Build\e_sqlite3.dll libs\e_sqlite3.dll

# 2. 清理重新编译
rd /s /q bin obj
dotnet restore --force
dotnet build --configuration Debug

# 3. 验证 DLL 已复制
dir bin\Debug\net8.0-windows\e_sqlite3.dll

# 4. 运行
dotnet run --configuration Debug
```

---

## 📋 验证清单

### ✅ 编译前检查

```bash
cd D:\gitcode\wx4helper\BaiShengVx3Plus
dir libs\e_sqlite3.dll
```

**预期输出**：
```
2025-11-06  ...  1,234,567  e_sqlite3.dll
```

### ✅ 编译后检查

```bash
dir bin\Debug\net8.0-windows\e_sqlite3.dll
```

**预期输出**：
```
2025-11-06  ...  1,234,567  e_sqlite3.dll
```

### ✅ 运行时检查

- ✅ 登录窗口显示
- ✅ 输入用户名密码登录
- ✅ **VxMain 主窗口正常显示**
- ✅ 数据库文件创建成功：
  - `bin\Debug\net8.0-windows\Data\logs.db`
  - `bin\Debug\net8.0-windows\Data\business_{wxid}.db`

---

## 🔍 故障排查

### 问题1: 脚本未找到 DLL

**解决**：手动下载 SQLite DLL
1. 访问 https://www.sqlite.org/download.html
2. 下载 `sqlite-dll-win-x64-*.zip`
3. 解压得到 `sqlite3.dll`
4. 复制到 `libs\e_sqlite3.dll`（注意重命名）

### 问题2: 编译后输出目录仍无 DLL

**检查项目文件**：
```xml
<ItemGroup>
  <None Include="libs\e_sqlite3.dll" Condition="Exists('libs\e_sqlite3.dll')">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
</ItemGroup>
```

**手动复制**：
```bash
copy libs\e_sqlite3.dll bin\Debug\net8.0-windows\e_sqlite3.dll
```

### 问题3: 运行时仍然报错 "无法加载 DLL"

**检查 DLL 位置**：
```bash
cd bin\Debug\net8.0-windows
dir e_sqlite3.dll
```

**检查 DLL 架构**（必须是 x64）：
- 文件大小约 1-2 MB
- 使用 [Dependency Walker](https://dependencywalker.com/) 或 `dumpbin /headers` 检查

### 问题4: VxMain 仍不显示但无错误

**查看日志**：
```bash
# 如果 logs.db 被创建，说明 SQLite 已工作
dir bin\Debug\net8.0-windows\Data\logs.db
```

**添加错误捕获**：
在 `Program.cs` 的 `Main` 方法中添加：
```csharp
try
{
    // 现有代码
}
catch (Exception ex)
{
    MessageBox.Show($"启动失败: {ex.Message}\n\n{ex.StackTrace}", "错误");
}
```

---

## 📂 目录结构

```
BaiShengVx3Plus/
├── libs/
│   └── e_sqlite3.dll                    ← 第1步：放置 DLL
├── bin/Debug/net8.0-windows/
│   ├── e_sqlite3.dll                    ← 第2步：自动复制
│   ├── BaiShengVx3Plus.exe
│   └── Data/
│       ├── logs.db                      ← 第3步：运行时创建
│       └── business_{wxid}.db           ← 登录后创建
├── find_and_copy_sqlite_dll.bat        ← 获取 DLL 脚本
└── fix_and_run.bat                      ← 编译运行脚本
```

---

## 💡 为什么 NuGet 包不自动复制 DLL？

1. **路径问题**：NuGet 将 DLL 放在 `runtimes/win-x64/native/` 子目录
2. **加载问题**：.NET 不会自动在子目录搜索原生 DLL
3. **解决方案**：将 DLL 复制到主输出目录

---

## 🎯 最终确认

执行以下命令验证所有步骤：

```bash
cd D:\gitcode\wx4helper\BaiShengVx3Plus

echo === 步骤1: 检查 libs 目录 ===
dir libs\e_sqlite3.dll

echo === 步骤2: 清理并编译 ===
rd /s /q bin obj
dotnet restore --force
dotnet build --configuration Debug

echo === 步骤3: 检查输出目录 ===
dir bin\Debug\net8.0-windows\e_sqlite3.dll

echo === 步骤4: 运行 ===
dotnet run --configuration Debug
```

---

## 📞 如果问题仍未解决

提供以下信息：

1. **libs 目录内容**：
```bash
dir libs
```

2. **输出目录内容**：
```bash
dir bin\Debug\net8.0-windows\*.dll
```

3. **运行时错误**（如果有）：
   - 截图或完整错误信息
   - 日志文件内容

---

**创建日期**: 2025-11-06  
**状态**: ⚠️ 需要手动获取 e_sqlite3.dll  
**优先级**: 🔴 高（必须先解决才能运行）

