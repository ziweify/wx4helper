# 清理BsBrowserClient日志功能说明

> **功能**: 在"清空数据"操作中增加清理BsBrowserClient日志  
> **位置**: VxMain → btnClearData_Click  
> **清理策略**: 保留24小时之内的日志  
> **日期**: 2025-12-17

---

## 📋 **功能概述**

在 `BaiShengVx3Plus` 的"清空数据"按钮中增加了清理 `BsBrowserClient` 日志的功能。

### **清理位置**

```
%LocalAppData%\BaiShengVx3Plus\log\
```

### **清理对象**

```
BsBrowserClient_*.log
```

**文件名格式**: `BsBrowserClient_yyyyMMdd_HHmmss.log`

**示例**:
```
BsBrowserClient_20251217_021803.log  (2025-12-17 02:18:03 启动)
BsBrowserClient_20251216_153025.log  (2025-12-16 15:30:25 启动)
BsBrowserClient_20251215_091530.log  (2025-12-15 09:15:30 启动)
```

### **清理策略**

- **保留时间**: **24 小时**
- **判断依据**: 文件名中的时间戳（`yyyyMMdd_HHmmss`）
- **删除对象**: 超过 24 小时的日志文件

---

## 🔧 **实现细节**

### **核心代码**

```csharp
// ========================================
// 🔥 步骤8：清理BsBrowserClient日志文件（保留24小时）
// ========================================

try
{
    var logDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "BaiShengVx3Plus",
        "log");
    
    if (Directory.Exists(logDir))
    {
        // 计算24小时之前的时间
        var cutoffTime = DateTime.Now.AddHours(-24);
        
        // 查找所有 BsBrowserClient 日志文件
        var logFiles = Directory.GetFiles(logDir, "BsBrowserClient_*.log");
        int deletedCount = 0;
        long totalSize = 0;
        
        foreach (var logFile in logFiles)
        {
            try
            {
                var fileName = Path.GetFileNameWithoutExtension(logFile);
                
                // 解析文件名：BsBrowserClient_yyyyMMdd_HHmmss
                var parts = fileName.Split('_');
                if (parts.Length >= 3)
                {
                    var dateStr = parts[1];  // yyyyMMdd
                    var timeStr = parts[2];  // HHmmss
                    
                    // 尝试解析时间戳
                    if (DateTime.TryParseExact(
                        $"{dateStr}_{timeStr}",
                        "yyyyMMdd_HHmmss",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out DateTime fileTime))
                    {
                        // 如果文件时间早于截止时间，删除
                        if (fileTime < cutoffTime)
                        {
                            var fileInfo = new FileInfo(logFile);
                            totalSize += fileInfo.Length;
                            File.Delete(logFile);
                            deletedCount++;
                            
                            _logService.Info("VxMain", 
                                $"  删除日志: {Path.GetFileName(logFile)} " +
                                $"({fileInfo.Length / 1024.0:F2} KB, " +
                                $"创建时间: {fileTime:yyyy-MM-dd HH:mm:ss})");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logService.Warning("VxMain", 
                    $"删除日志文件失败: {logFile}, 错误: {ex.Message}");
            }
        }
        
        _logService.Info("VxMain", 
            $"✅ 已清理 {deletedCount} 个BsBrowserClient日志文件 " +
            $"（24小时之前，释放 {totalSize / 1024.0 / 1024.0:F2} MB空间）");
    }
    else
    {
        _logService.Info("VxMain", "BsBrowserClient日志目录不存在，跳过清理");
    }
}
catch (Exception ex)
{
    _logService.Error("VxMain", $"清理BsBrowserClient日志失败: {ex.Message}", ex);
    // 不抛出异常，继续执行
}
```

---

## 📊 **清理逻辑**

### **时间戳解析**

```
文件名: BsBrowserClient_20251217_021803.log

解析:
1. 分割: ["BsBrowserClient", "20251217", "021803"]
2. 日期: parts[1] = "20251217" → 2025-12-17
3. 时间: parts[2] = "021803" → 02:18:03
4. 完整时间戳: 2025-12-17 02:18:03
```

### **清理判断**

```
当前时间: 2025-12-17 18:00:00
截止时间: 2025-12-16 18:00:00（24小时前）

日志文件时间戳 < 截止时间 → 删除
日志文件时间戳 >= 截止时间 → 保留
```

### **清理示例**

```
当前时间: 2025-12-17 18:00:00

文件列表:
✅ BsBrowserClient_20251217_153025.log  (2025-12-17 15:30) → 保留（3小时前）
✅ BsBrowserClient_20251217_020103.log  (2025-12-17 02:01) → 保留（16小时前）
✅ BsBrowserClient_20251216_195530.log  (2025-12-16 19:55) → 保留（22小时前）
❌ BsBrowserClient_20251216_153025.log  (2025-12-16 15:30) → 删除（26小时前）
❌ BsBrowserClient_20251215_091530.log  (2025-12-15 09:15) → 删除（57小时前）
❌ BsBrowserClient_20251214_120000.log  (2025-12-14 12:00) → 删除（78小时前）
```

---

## 🎯 **清理流程**

### **完整清理步骤**

```
步骤1: 备份当前数据库
  ↓
步骤2: 清空订单表
  ↓
步骤3: 重置会员金额数据
  ↓
步骤4: 清空统计数据
  ↓
步骤5: 清空所有上下分记录
  ↓
步骤6: 清理生成的图片数据（C:\images\）
  ↓
步骤7: 清理28小时之前的日志数据
  ↓
步骤8: 🔥 清理24小时之前的BsBrowserClient日志（新增）
  ↓
步骤9: 刷新UI
```

### **用户交互**

1. **确认对话框**:
   ```
   确定要清空所有数据吗？

   此操作将：
   1. 备份当前数据库（加密压缩）
   2. 清空所有订单数据
   3. 重置会员金额数据
   4. 清空统计数据
   5. 清空48小时之前的上下分记录
   6. 清空24小时之前的BsBrowserClient日志  ← 🔥 新增
   
   会员基础信息（微信ID、昵称等）将保留
   ```

2. **成功提示**:
   ```
   数据清空成功！

   ✓ 订单数据已清空
   ✓ 上下分记录已清空
   ✓ 会员金额数据已重置
   ✓ 统计数据已清空
   ✓ 生成的图片数据已清空
   ✓ 28小时之前的日志数据已清空（保留28小时用于恢复）
   ✓ 24小时之前的BsBrowserClient日志已清空  ← 🔥 新增
   ✓ 会员基础信息已保留
   ✓ 数据库已备份
   ```

---

## 📝 **日志输出示例**

### **清理过程日志**

```
[VxMain] 开始清空数据...
[VxMain] ✅ 数据库已备份: C:\Users\...\Backup\d12171800_BaiSheng.db
[VxMain] ✅ 订单表已清空
[VxMain] ✅ 34 个会员的金额数据已重置
[VxMain] ✅ 统计数据已清空
[VxMain] ✅ 上下分记录已完全清空
[VxMain] ✅ 已清理 5 个生成的图片文件
[VxMain] ✅ 已清理 1234 条日志记录（28小时之前，截止时间: 2025-12-16 14:00:00）
[VxMain] ✅ 日志数据库已优化（VACUUM）
[VxMain]   删除日志: BsBrowserClient_20251216_153025.log (5.12 KB, 创建时间: 2025-12-16 15:30:25)
[VxMain]   删除日志: BsBrowserClient_20251215_091530.log (3.45 KB, 创建时间: 2025-12-15 09:15:30)
[VxMain]   删除日志: BsBrowserClient_20251214_120000.log (4.78 KB, 创建时间: 2025-12-14 12:00:00)
[VxMain] ✅ 已清理 3 个BsBrowserClient日志文件（24小时之前，释放 0.01 MB空间）
[VxMain] ✅ 数据清空完成
```

---

## 🛡️ **错误处理**

### **异常捕获**

```csharp
try
{
    // 清理日志文件
}
catch (Exception ex)
{
    _logService.Error("VxMain", $"清理BsBrowserClient日志失败: {ex.Message}", ex);
    // 不抛出异常，继续执行
}
```

### **单个文件删除失败**

```csharp
foreach (var logFile in logFiles)
{
    try
    {
        // 删除文件
        File.Delete(logFile);
    }
    catch (Exception ex)
    {
        _logService.Warning("VxMain", 
            $"删除日志文件失败: {logFile}, 错误: {ex.Message}");
        // 继续处理下一个文件
    }
}
```

**特点**:
- ✅ 单个文件删除失败不影响其他文件
- ✅ 所有异常都被记录到日志
- ✅ 不会阻止后续清理步骤

---

## 💡 **设计亮点**

### **1. 精确的时间判断**

```csharp
// 根据文件名解析时间戳，而不是文件修改时间
DateTime.TryParseExact(
    $"{dateStr}_{timeStr}",
    "yyyyMMdd_HHmmss",
    CultureInfo.InvariantCulture,
    DateTimeStyles.None,
    out DateTime fileTime)
```

**优势**:
- ✅ 不受文件系统修改时间影响
- ✅ 准确反映程序启动时间
- ✅ 与日志内容时间戳一致

### **2. 详细的清理日志**

```csharp
_logService.Info("VxMain", 
    $"  删除日志: {Path.GetFileName(logFile)} " +
    $"({fileInfo.Length / 1024.0:F2} KB, " +
    $"创建时间: {fileTime:yyyy-MM-dd HH:mm:ss})");
```

**信息包含**:
- ✅ 文件名
- ✅ 文件大小
- ✅ 创建时间

### **3. 统计信息**

```csharp
_logService.Info("VxMain", 
    $"✅ 已清理 {deletedCount} 个BsBrowserClient日志文件 " +
    $"（24小时之前，释放 {totalSize / 1024.0 / 1024.0:F2} MB空间）");
```

**统计数据**:
- ✅ 删除文件数量
- ✅ 释放空间大小
- ✅ 清理时间范围

---

## 🔍 **与自动清理的对比**

### **自动清理（BsBrowserClient 启动时）**

| 特性 | 值 |
|------|-----|
| **触发时机** | BsBrowserClient 启动时 |
| **保留时间** | 7 天 |
| **清理对象** | 所有 `BsBrowserClient_*.log` |
| **判断依据** | 文件的最后修改时间 |

### **手动清理（清空数据按钮）**

| 特性 | 值 |
|------|-----|
| **触发时机** | 用户点击"清空数据"按钮 |
| **保留时间** | 24 小时 |
| **清理对象** | 所有 `BsBrowserClient_*.log` |
| **判断依据** | 文件名中的时间戳 |

### **为什么不同？**

1. **自动清理（7天）**:
   - 定期清理，保留足够的历史日志用于问题排查
   - 不需要用户干预
   - 占用空间可控

2. **手动清理（24小时）**:
   - 用户主动清理，更激进
   - 与其他数据清理操作一起执行
   - 只保留最近的日志

---

## 🧪 **测试方法**

### **测试步骤**

1. **准备测试数据**
   - 创建多个不同时间的日志文件（修改文件名）
   ```
   BsBrowserClient_20251217_180000.log  (当前)
   BsBrowserClient_20251216_200000.log  (22小时前)
   BsBrowserClient_20251216_150000.log  (27小时前)
   BsBrowserClient_20251215_100000.log  (56小时前)
   ```

2. **执行清理**
   - 启动 BaiShengVx3Plus
   - 点击"清空数据"按钮
   - 确认操作

3. **查看结果**
   - 检查日志文件夹：`%LocalAppData%\BaiShengVx3Plus\log`
   - 验证保留/删除是否正确
   - 查看程序日志中的清理记录

### **预期结果**

```
保留:
✅ BsBrowserClient_20251217_180000.log  (当前)
✅ BsBrowserClient_20251216_200000.log  (22小时前)

删除:
❌ BsBrowserClient_20251216_150000.log  (27小时前)
❌ BsBrowserClient_20251215_100000.log  (56小时前)
```

---

## 📚 **相关功能**

### **BsBrowserClient 日志系统**

1. **日志持久化**: 自动将运行日志保存到磁盘
2. **文件命名**: 按"日期+时间"命名（`yyyyMMdd_HHmmss`）
3. **自动清理**: 启动时清理 7 天前的日志
4. **手动清理**: 通过"清空数据"清理 24 小时前的日志

### **相关文档**

- `BsBrowserClient/日志持久化设计方案.md`
- `BsBrowserClient/日志持久化使用说明.md`
- `BsBrowserClient/日志文件命名更新说明.md`
- `BsBrowserClient/日志丢失根本原因修复.md`

---

## ✅ **编译验证**

```bash
dotnet build BaiShengVx3Plus/BaiShengVx3Plus.csproj
```

**结果**: ✅ **0 个错误**（56 个警告为原有）

---

## 🎉 **总结**

### **功能完成**

- ✅ 在"清空数据"操作中增加清理 BsBrowserClient 日志
- ✅ 保留 24 小时之内的日志
- ✅ 根据文件名时间戳精确判断
- ✅ 详细的清理日志和统计信息
- ✅ 完善的错误处理
- ✅ 更新确认对话框和成功提示

### **用户体验**

- ✅ 一键清理所有旧数据
- ✅ 清晰的操作提示
- ✅ 详细的清理日志
- ✅ 失败不影响其他步骤

### **维护友好**

- ✅ 代码结构清晰
- ✅ 异常处理完善
- ✅ 日志输出详细

**现在用户可以通过"清空数据"按钮一键清理 BsBrowserClient 的旧日志，释放磁盘空间！** 🚀

