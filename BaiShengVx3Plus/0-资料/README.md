# 📚 项目学习资料

> **说明**：由于 PowerShell 中文路径编码问题，架构设计文档目前在 `ziliao` 文件夹中。  
> **请手动将 `ziliao` 文件夹中的 4 个 .md 文件移动到这里（`0-资料` 文件夹），然后删除 `ziliao` 文件夹。**

---

## 📄 应该包含的文档

1. **20251105-ModernArchitecture.md** - 现代化架构设计详解
2. **20251105-ServiceGranularity.md** - 服务粒度与边界设计指南
3. **20251105-ServiceQuickRef.md** - 服务划分快速参考表
4. **20251105-架构设计学习资料索引.md** - 学习资料索引

---

## 🎯 快速操作指南

### 方法1：手动移动（推荐）

1. 在文件资源管理器中打开 `BaiShengVx3Plus` 文件夹
2. 打开 `ziliao` 文件夹
3. 选中所有 `.md` 文件（Ctrl+A）
4. 剪切（Ctrl+X）
5. 返回上一级，进入 `0-资料` 文件夹
6. 粘贴（Ctrl+V）
7. 删除空的 `ziliao` 文件夹

### 方法2：使用命令行（在 CMD 中执行）

```cmd
cd /d D:\gitcode\wx4helper\BaiShengVx3Plus
move ziliao\*.md 0-资料\
rmdir ziliao
```

---

## 📖 文档说明

这些文档记录了项目架构设计的核心概念和实战经验：

- ✅ 为什么要封装服务
- ✅ 如何控制服务粒度
- ✅ 服务如何分层
- ✅ 如何避免代码冗余
- ✅ 实际重构案例

---

**移动完成后，请删除此 README.md 和 `00-请移动ziliao文件夹中的文档到这里.txt`** 📝

