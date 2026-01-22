# UserDocment 文档目录说明

## 📁 目录结构

```
Unit.la/
└── UserDocment/              ← 用户文档目录
    ├── README.md             ← 文档导航（从这里开始）
    ├── 使用手册.md            ← 完整使用指南（主文档）
    ├── Web库-快速参考.md      ← Web库快速查询
    ├── Web库完整实现-完成报告.md
    ├── 库功能文档体系-完成报告.md
    ├── Web库与文档系统-最终完成报告.md
    ├── 脚本管理系统-完成报告.md
    ├── 脚本保存功能-完成报告.md
    ├── 自动创建默认脚本-完成说明.md
    ├── VS风格布局-最终版.md
    ├── 浏览器任务控件库重构完成报告.md
    ├── README_ScriptManager.md
    ├── 脚本管理器集成说明.md
    ├── MoonSharp兼容性修复.md
    ├── ScintillaNET版本说明.md
    └── 编译错误修复说明.md
```

## 🎯 目录用途

### 为什么创建 UserDocment 目录？

1. **集中管理文档** - 所有用户文档集中在一个目录，便于查找和维护
2. **随编译分发** - 文档自动复制到输出目录，用户可以直接访问
3. **结构清晰** - 与源代码分离，保持项目结构清晰

### 编译配置

在 `Unit.la.csproj` 中配置：

```xml
<!-- 用户文档 - 随编译复制到输出目录 -->
<ItemGroup>
  <None Include="UserDocment\**\*.md">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
</ItemGroup>
```

**效果**：
- 编译后，所有 `.md` 文档会自动复制到 `bin/Debug/net8.0-windows/UserDocment/`
- 引用 Unit.la 的项目（如 YongLiSystem）也会自动获得这些文档

## 📖 文档分类

### 核心文档（必读）
- **README.md** - 文档导航和快速入门
- **使用手册.md** - 完整的使用指南（10章节）
- **Web库-快速参考.md** - 常用API速查

### 功能文档
- **Web库完整实现-完成报告.md** - Web库详细说明
- **脚本管理系统-完成报告.md** - 脚本系统文档
- **浏览器任务控件库重构完成报告.md** - 控件库说明

### 技术文档
- **VS风格布局-最终版.md** - 编辑器布局设计
- **脚本保存功能-完成报告.md** - 保存功能实现
- **自动创建默认脚本-完成说明.md** - 脚本初始化

### 系统文档
- **库功能文档体系-完成报告.md** - 文档维护说明
- **Web库与文档系统-最终完成报告.md** - 完整总结

### 技术修复文档
- **MoonSharp兼容性修复.md** - Lua引擎兼容性
- **ScintillaNET版本说明.md** - 编辑器组件版本
- **编译错误修复说明.md** - 编译问题解决

## 🚀 使用指南

### 开发者
编译项目后，在输出目录 `bin/Debug/net8.0-windows/UserDocment/` 中可以找到所有文档。

### 最终用户
程序发布时，UserDocment 文件夹会随程序一起分发，用户可以直接查看文档。

### 推荐阅读顺序
1. **README.md** - 了解文档结构
2. **使用手册.md - 第2章** - 快速开始
3. **Web库-快速参考.md** - 常用API
4. **使用手册.md - 第5章** - Web库完整API

## 🔄 文档更新

### 添加新文档
1. 在 `Unit.la/UserDocment/` 目录下创建新的 `.md` 文件
2. 编译项目，新文档会自动复制到输出目录
3. 无需修改 `.csproj`，已配置 `**\*.md` 通配符

### 修改现有文档
1. 直接在 `Unit.la/UserDocment/` 中修改文档
2. 重新编译，修改会自动同步到输出目录

### 文档维护规则
参考：`AI工作规则/库功能文档维护规则.md`

**核心原则**：代码变更必须同步更新文档！

## 📊 编译输出结构

```
bin/Debug/net8.0-windows/
├── Unit.la.dll
├── Unit.la.pdb
├── UserDocment/                  ← 文档目录（自动生成）
│   ├── README.md
│   ├── 使用手册.md
│   └── ...（所有文档）
├── MoonSharp.dll
├── Scintilla.NET.dll
└── ...（其他依赖）
```

## ✅ 验证

### 检查文档是否正确复制

**PowerShell**:
```powershell
# 检查 Unit.la 输出目录
Test-Path "Unit.la/bin/Debug/net8.0-windows/UserDocment"
Get-ChildItem "Unit.la/bin/Debug/net8.0-windows/UserDocment"

# 检查 YongLiSystem 输出目录
Test-Path "YongLiSystem/bin/Debug/net8.0-windows/UserDocment"
Get-ChildItem "YongLiSystem/bin/Debug/net8.0-windows/UserDocment"
```

### 预期结果
- ✅ UserDocment 目录存在
- ✅ 包含 16 个 `.md` 文件
- ✅ 文件时间戳与源文件一致

## 🎉 总结

- ✅ 所有文档已移动到 `Unit.la/UserDocment/`
- ✅ 配置了自动复制到输出目录
- ✅ 编译成功，文档已正确复制
- ✅ 引用项目也能自动获得文档
- ✅ 结构清晰，便于维护和分发

---

**完成时间**: 2026-01-22  
**目录位置**: `Unit.la/UserDocment/`  
**文档数量**: 16 个 Markdown 文件
