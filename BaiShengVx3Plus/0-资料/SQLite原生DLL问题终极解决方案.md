# SQLite 原生 DLL 问题终极解决方案

## 🔴 问题根源

`sqlite-net-pcl` 需要原生 SQLite DLL，但 NuGet 包不总是正确复制到输出目录。

---

## ✅ 终极解决方案

### 方案 1: 手动复制 DLL（最可靠）

1. **下载原生 DLL**
   - 从 https://www.sqlite.org/download.html 下载
   - 或从 F5BotV2 的输出目录复制

2. **复制到项目目录**
```
BaiShengVx3Plus/
├── libs/
│   └── sqlite3.dll  (x64 版本)
```

3. **修改项目文件**
```xml
<ItemGroup>
  <None Include="libs\sqlite3.dll">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
</ItemGroup>
```

### 方案 2: 使用 System.Data.SQLite（传统方案）

**优点**: 自带原生 DLL，不需要额外配置
**缺点**: 包体积较大

```xml
<PackageReference Include="System.Data.SQLite.Core" Version="1.0.118" />
```

### 方案 3: 确保 bundle_green 正确工作（当前方案）

1. **清理并重新安装**
```bash
dotnet clean
rd /s /q bin obj
dotnet restore --force
dotnet build
```

2. **检查输出目录**
```
bin/Debug/net8.0-windows/runtimes/win-x64/native/sqlite3.dll
```

---

## 🚀 立即实施方案 1

我将为你下载并配置好 sqlite3.dll。

