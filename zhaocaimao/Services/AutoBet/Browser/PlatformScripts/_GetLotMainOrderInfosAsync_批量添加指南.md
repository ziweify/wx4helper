# GetLotMainOrderInfosAsync 批量添加指南

## ✅ 已完成的项目

1. **BsBrowserClient** - 所有 17 个平台脚本已实现 ✅
   - TongBaoScript: 完整实现 ✅
   - 其他 16 个脚本: 空实现（返回 "平台暂不支持"） ✅

2. **IPlatformScript 接口** - 已添加方法签名 ✅
   - `BsBrowserClient/PlatformScripts/IPlatformScript.cs` ✅
   - `zhaocaimao/Services/AutoBet/Browser/PlatformScripts/IPlatformScript.cs` ✅

## ⚠️ 待处理

**zhaocaimao 项目** - 17 个平台脚本需要实现

---

## 📋 需要添加的方法模板

为每个 zhaocaimao 脚本添加以下方法（在 `GetOddsList()` 方法后、类结束前）：

```csharp
/// <summary>
/// 获取未结算的订单信息（平台暂不支持）
/// </summary>
public Task<(bool success, List<JObject>? orders, int maxRecordNum, int maxPageNum, string errorMsg)> GetLotMainOrderInfosAsync(
    int state = 0,
    int pageNum = 1,
    int pageCount = 20,
    string? beginDate = null,
    string? endDate = null)
{
    return Task.FromResult<(bool, List<JObject>?, int, int, string)>((false, null, 0, 0, "平台暂不支持"));
}
```

---

## 📝 需要修改的文件列表（zhaocaimao）

1. ✅ **YydsScript.cs** - YYDS 平台
2. ⏳ **TongBaoScript.cs** - 通宝平台（空实现）
3. ⏳ **QtScript.cs**
4. ⏳ **Mt168Script.cs**
5. ⏳ **ADKScript.cs**
6. ⏳ **Hy168bingoScript.cs**
7. ⏳ **HongHaiWuMingScript.cs**
8. ⏳ **S880Script.cs**
9. ⏳ **LanAScript.cs**
10. ⏳ **AcScript.cs**
11. ⏳ **HongHaiScript.cs**
12. ⏳ **HaiXiaScript.cs**
13. ⏳ **YYZ2Script.cs**
14. ⏳ **Kk888Script.cs**
15. ⏳ **YunDing28Script.cs**
16. ⏳ **NoneSiteScript.cs**
17. ⏳ **TongBaoPcScript.cs**

---

## 🎯 下一步操作建议

由于 zhaocaimao 有 17 个脚本需要逐个添加，建议采用以下方式之一：

### 方案 A：手动逐个添加（推荐，最安全）
逐个打开文件，在 `GetOddsList()` 方法后添加上述模板。

### 方案 B：使用 IDE 批量重构
1. 在 IDE 中打开 `IPlatformScript.cs`
2. 使用"实现接口"功能
3. 为每个类自动生成方法签名
4. 然后修改实现为返回空值

### 方案 C：继续使用工具（需要测试）
创建一个更精确的脚本来处理每个文件。

---

## ✅ 完成检查清单

编译通过标志：
```bash
dotnet build zhaocaimao/zhaocaimao.csproj
# 应该显示: 0 个错误
```

所有平台脚本都实现了 `GetLotMainOrderInfosAsync` 方法 ✓

