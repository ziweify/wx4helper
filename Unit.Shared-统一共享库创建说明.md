# Unit.Shared - 统一共享库创建说明

## 🎯 **优化目标**

将原来分散的共享库（`BaiShengVx3Plus.Shared` 和 `zhaocaimao.Shared`）合并为一个统一的共享库 `Unit.Shared`，实现**代码复用，只维护一份代码**。

---

## 📊 **改造前的问题**

### **原始架构**

```
zhaocaimao 项目
    ├─ 引用: zhaocaimao.Shared
    └─ 引用: BaiShengVx3Plus.Shared  ❌ 交叉引用

BaiShengVx3Plus 项目  
    └─ 引用: BaiShengVx3Plus.Shared

BsBrowserClient 项目
    └─ 引用: BaiShengVx3Plus.Shared
```

### **存在的问题**

1. ❌ **代码重复**
   - ModernHttpHelper 在 BaiShengVx3Plus.Shared 和 zhaocaimao.Shared 中都有副本
   - BinggoTimeHelper 同样重复

2. ❌ **交叉引用**
   - zhaocaimao 同时引用了两个共享库，导致混乱

3. ❌ **维护困难**
   - 修改 ModernHttpHelper 需要在两处同时修改
   - 容易出现版本不一致的情况

4. ❌ **命名空间混乱**
   - 有的文件使用 `using BaiShengVx3Plus.Shared.Helpers;`
   - 有的文件使用 `using zhaocaimao.Shared.Helpers;`

---

## ✅ **解决方案：创建 Unit.Shared**

### **新架构**

```
Unit.Shared (统一共享库)
    ├─ Helpers/
    │   ├─ ModernHttpHelper.cs      ✅ 统一版本
    │   ├─ BinggoTimeHelper.cs      ✅ 统一版本
    │   ├─ HttpHelper.cs             ✅ 旧版兼容
    │   └─ TimestampHelper.cs        ✅ 时间戳工具
    ├─ Models/
    │   ├─ BetStandardOrder.cs       ✅ 标准订单模型
    │   ├─ OddsInfo.cs               ✅ 赔率信息
    │   └─ Games/Binggo/...          ✅ Binggo游戏模型
    ├─ Parsers/
    │   └─ BetContentParser.cs       ✅ 投注内容解析器
    ├─ Platform/
    │   ├─ BetPlatform.cs            ✅ 平台枚举
    │   └─ PlatformUrlManager.cs     ✅ 平台URL管理
    └─ Services/
        └─ BinggoStatisticsService.cs ✅ Binggo统计服务

zhaocaimao 项目
    └─ 引用: Unit.Shared ✅

BaiShengVx3Plus 项目
    └─ 引用: Unit.Shared ✅

BsBrowserClient 项目
    └─ 引用: Unit.Shared ✅
```

---

## 🔧 **实施步骤**

### **1️⃣ 创建 Unit.Shared 项目**

```xml
<!-- Unit.Shared/Unit.Shared.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
  </ItemGroup>
</Project>
```

### **2️⃣ 复制文件从 BaiShengVx3Plus.Shared**

```bash
# 复制所有文件夹
Copy-Item BaiShengVx3Plus.Shared/Helpers → Unit.Shared/Helpers
Copy-Item BaiShengVx3Plus.Shared/Models → Unit.Shared/Models
Copy-Item BaiShengVx3Plus.Shared/Parsers → Unit.Shared/Parsers
Copy-Item BaiShengVx3Plus.Shared/Platform → Unit.Shared/Platform
Copy-Item BaiShengVx3Plus.Shared/Services → Unit.Shared/Services
```

### **3️⃣ 批量修改命名空间**

```powershell
# 将所有 .cs 文件的命名空间从 BaiShengVx3Plus.Shared 改为 Unit.Shared
Get-ChildItem -Path "Unit.Shared" -Filter "*.cs" -Recurse | ForEach-Object {
    $content = Get-Content $_.FullName -Raw -Encoding UTF8
    $newContent = $content -replace 'namespace BaiShengVx3Plus\.Shared', 'namespace Unit.Shared'
    $newContent = $newContent -replace 'using BaiShengVx3Plus\.Shared', 'using Unit.Shared'
    $newContent | Set-Content $_.FullName -Encoding UTF8 -NoNewline
}
```

### **4️⃣ 更新项目引用**

#### **zhaocaimao/zhaocaimao.csproj**

```xml
<!-- 修改前 -->
<ItemGroup>
  <ProjectReference Include="..\zhaocaimao.Shared\zhaocaimao.Shared.csproj" />
  <ProjectReference Include="..\BaiShengVx3Plus.Shared\BaiShengVx3Plus.Shared.csproj" />
</ItemGroup>

<!-- 修改后 -->
<ItemGroup>
  <ProjectReference Include="..\Unit.Shared\Unit.Shared.csproj" />
</ItemGroup>
```

#### **BaiShengVx3Plus/BaiShengVx3Plus.csproj**

```xml
<!-- 修改前 -->
<ItemGroup>
  <ProjectReference Include="..\BaiShengVx3Plus.Shared\BaiShengVx3Plus.Shared.csproj" />
</ItemGroup>

<!-- 修改后 -->
<ItemGroup>
  <ProjectReference Include="..\Unit.Shared\Unit.Shared.csproj" />
</ItemGroup>
```

#### **BsBrowserClient/BsBrowserClient.csproj**

```xml
<!-- 修改前 -->
<ItemGroup>
  <ProjectReference Include="..\BaiShengVx3Plus.Shared\BaiShengVx3Plus.Shared.csproj" />
</ItemGroup>

<!-- 修改后 -->
<ItemGroup>
  <ProjectReference Include="..\Unit.Shared\Unit.Shared.csproj" />
</ItemGroup>
```

### **5️⃣ 批量更新 using 语句**

```powershell
# 更新 zhaocaimao 项目
Get-ChildItem -Path "zhaocaimao" -Filter "*.cs" -Recurse -Exclude obj,bin | ForEach-Object {
    $content = Get-Content $_.FullName -Raw -Encoding UTF8
    $content = $content -replace 'using zhaocaimao\.Shared', 'using Unit.Shared'
    $content = $content -replace 'using BaiShengVx3Plus\.Shared', 'using Unit.Shared'
    $content = $content -replace 'zhaocaimao\.Shared\.', 'Unit.Shared.'
    $content | Set-Content $_.FullName -Encoding UTF8 -NoNewline
}

# 更新 BaiShengVx3Plus 项目
Get-ChildItem -Path "BaiShengVx3Plus" -Filter "*.cs" -Recurse -Exclude obj,bin | ForEach-Object {
    $content = Get-Content $_.FullName -Raw -Encoding UTF8
    $content = $content -replace 'using BaiShengVx3Plus\.Shared', 'using Unit.Shared'
    $content = $content -replace 'BaiShengVx3Plus\.Shared\.', 'Unit.Shared.'
    $content | Set-Content $_.FullName -Encoding UTF8 -NoNewline
}

# 更新 BsBrowserClient 项目
Get-ChildItem -Path "BsBrowserClient" -Filter "*.cs" -Recurse -Exclude obj,bin | ForEach-Object {
    $content = Get-Content $_.FullName -Raw -Encoding UTF8
    $content = $content -replace 'using BaiShengVx3Plus\.Shared', 'using Unit.Shared'
    $content = $content -replace 'BaiShengVx3Plus\.Shared\.', 'Unit.Shared.'
    $content | Set-Content $_.FullName -Encoding UTF8 -NoNewline
}
```

### **6️⃣ 兼容性处理**

#### **BetPlatform 枚举兼容**

```csharp
// Unit.Shared/Platform/BetPlatform.cs
public enum BetPlatform
{
    // ...
    云顶 = 21,
    yyds = 22,      // zhaocaimao 使用
    Yyds666 = 22    // BsBrowserClient 使用（别名）
}
```

### **7️⃣ 修复 BaiShengVx3Plus 的文件复制配置**

```xml
<!-- BaiShengVx3Plus/BaiShengVx3Plus.csproj -->

<!-- 修改前 -->
<OurCodeFiles Include="$(BrowserClientSourcePath)\BaiShengVx3Plus.Shared.dll" />
<OurCodeFiles Include="$(BrowserClientSourcePath)\BaiShengVx3Plus.Shared.pdb" />

<!-- 修改后 -->
<OurCodeFiles Include="$(BrowserClientSourcePath)\Unit.Shared.dll" />
<OurCodeFiles Include="$(BrowserClientSourcePath)\Unit.Shared.pdb" />
```

---

## ✅ **编译验证结果**

| 项目 | 错误数 | 警告数 | 状态 |
|------|--------|--------|------|
| **Unit.Shared** | 0 | 6 | ✅ 成功 |
| **zhaocaimao** | 0 | 107 | ✅ 成功 |
| **BsBrowserClient** | 0 | 0 | ✅ 成功 |
| **BaiShengVx3Plus** | 0 | 56 | ✅ 成功 |

**所有项目编译成功！** 🎊

---

## 📋 **修改的文件统计**

### **新增文件**

- `Unit.Shared/Unit.Shared.csproj`
- `Unit.Shared/Helpers/*.cs`（所有文件）
- `Unit.Shared/Models/*.cs`（所有文件）
- `Unit.Shared/Parsers/*.cs`（所有文件）
- `Unit.Shared/Platform/*.cs`（所有文件）
- `Unit.Shared/Services/*.cs`（所有文件）

### **修改的项目文件**

- `zhaocaimao/zhaocaimao.csproj`
- `BaiShengVx3Plus/BaiShengVx3Plus.csproj`
- `BsBrowserClient/BsBrowserClient.csproj`

### **批量修改的源文件**

- `zhaocaimao/**/*.cs`（所有引用 zhaocaimao.Shared 或 BaiShengVx3Plus.Shared 的文件）
- `BaiShengVx3Plus/**/*.cs`（所有引用 BaiShengVx3Plus.Shared 的文件）
- `BsBrowserClient/**/*.cs`（所有引用 BaiShengVx3Plus.Shared 的文件）

---

## 🎯 **核心优势**

### **1. 代码复用**

```
改造前（代码重复）：
- BaiShengVx3Plus.Shared/Helpers/ModernHttpHelper.cs
- zhaocaimao.Shared/Helpers/ModernHttpHelper.cs
→ 需要维护两份代码 ❌

改造后（统一维护）：
- Unit.Shared/Helpers/ModernHttpHelper.cs
→ 只需维护一份代码 ✅
```

### **2. 清晰的依赖关系**

```
改造前：
zhaocaimao → BaiShengVx3Plus.Shared + zhaocaimao.Shared ❌
BaiShengVx3Plus → BaiShengVx3Plus.Shared
BsBrowserClient → BaiShengVx3Plus.Shared

改造后：
zhaocaimao → Unit.Shared ✅
BaiShengVx3Plus → Unit.Shared ✅
BsBrowserClient → Unit.Shared ✅
```

### **3. 统一的命名空间**

```csharp
// 改造前（混乱）
using BaiShengVx3Plus.Shared.Helpers;  // 有的文件这样
using zhaocaimao.Shared.Helpers;      // 有的文件那样

// 改造后（统一）
using Unit.Shared.Helpers;  // 所有文件统一 ✅
```

### **4. 易于维护**

```
改造前：
- 修改 ModernHttpHelper → 需要在两个共享库中都修改
- 容易出现版本不一致

改造后：
- 修改 ModernHttpHelper → 只需在 Unit.Shared 中修改一次
- 自动在所有项目中生效 ✅
```

---

## 📚 **Unit.Shared 包含的模块**

### **Helpers/**

| 模块 | 说明 |
|------|------|
| `ModernHttpHelper.cs` | 现代化HTTP请求包装器（支持超时、重试） |
| `BinggoTimeHelper.cs` | Binggo期号时间计算工具 |
| `HttpHelper.cs` | 旧版HTTP助手（兼容性） |
| `TimestampHelper.cs` | 时间戳转换工具 |

### **Models/**

| 模块 | 说明 |
|------|------|
| `BetStandardOrder.cs` | 标准投注订单模型 |
| `OddsInfo.cs` | 赔率信息模型 |
| `Games/Binggo/...` | Binggo游戏相关模型 |

### **Parsers/**

| 模块 | 说明 |
|------|------|
| `BetContentParser.cs` | 投注内容解析器 |

### **Platform/**

| 模块 | 说明 |
|------|------|
| `BetPlatform.cs` | 平台枚举定义 |
| `PlatformUrlManager.cs` | 平台URL管理器 |

### **Services/**

| 模块 | 说明 |
|------|------|
| `BinggoStatisticsService.cs` | Binggo统计服务 |

---

## 🗑️ **可以删除的旧文件**

完成迁移并验证后，可以删除以下文件夹：

```
❌ BaiShengVx3Plus.Shared/（旧共享库，已被 Unit.Shared 替代）
❌ zhaocaimao.Shared/（旧共享库，已被 Unit.Shared 替代）
```

**注意：** 建议先保留一段时间，确认无问题后再删除。

---

## 📖 **使用示例**

### **在 zhaocaimao 项目中使用**

```csharp
using Unit.Shared.Helpers;
using Unit.Shared.Models;
using Unit.Shared.Platform;

// 使用 ModernHttpHelper
var httpHelper = new ModernHttpHelper();
var result = await httpHelper.PostAsync(new HttpRequestItem
{
    Url = "https://api.example.com/endpoint",
    PostData = "data=value",
    Timeout = 5
});

// 使用 BinggoTimeHelper
var issueId = BinggoTimeHelper.GetCurrentIssueId();
var openTime = BinggoTimeHelper.GetIssueOpenTime(issueId);

// 使用 BetPlatform
var platform = BetPlatform.yyds;
var url = PlatformUrlManager.GetDefaultUrl(platform);
```

### **在 BaiShengVx3Plus 项目中使用**

```csharp
using Unit.Shared.Helpers;
using Unit.Shared.Models;
using Unit.Shared.Services;

// 完全相同的 API，无需改动
var httpHelper = new ModernHttpHelper();
var statsService = new BinggoStatisticsService();
```

---

## 🎉 **总结**

✅ **Unit.Shared 统一共享库已成功创建！**

**核心改进：**
- ✅ **代码复用**：只维护一份代码，避免重复
- ✅ **清晰依赖**：所有项目都引用 Unit.Shared
- ✅ **统一命名空间**：`using Unit.Shared.*`
- ✅ **易于维护**：修改一次，所有项目生效
- ✅ **编译成功**：所有项目 0 个错误
- ✅ **向前兼容**：保持了对旧代码的兼容性

**现在三个项目（zhaocaimao, BaiShengVx3Plus, BsBrowserClient）共享同一个代码库，只需维护 Unit.Shared 一份代码！** 🚀

**这是一个更加清晰、易维护的架构！** 🎊

