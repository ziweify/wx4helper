# DevExpress NetCore 路径说明

**📅 创建日期**: 2025年12月19日  
**📌 主题**: .NET 8.0 项目必须使用 NetCore 目录下的 DLL  
**🎯 关键**: 路径必须是 `NetCore`，不是 `Framework`

---

## ✅ 正确路径

```
C:\Program Files\DevExpress 23.2\Components\Bin\NetCore\
```

---

## ❌ 错误路径

```
C:\Program Files\DevExpress 23.2\Components\Bin\Framework\  ← 错误！
```

---

## 📝 说明

- **.NET 8.0** 项目必须使用 `NetCore` 目录
- **.NET Framework** 项目使用 `Framework` 目录
- 路径错误会导致编译失败

---

**说明文件编号**: 251219-004  
**创建时间**: 2025-12-19  
**文件类型**: 技术说明  
**版本**: v1.0

