# 数据存储架构分析 - 数据库 vs JSON

## 📊 当前存储架构

### 1. **数据库存储**（SQLite）
- **文件**: `C:\Users\Administrator\AppData\Local\永利系统\Data\app.db`
- **表名**: `script_tasks`
- **服务**: `DataCollectionService`
- **用途**: 主数据源，存储所有任务列表
- **字段**:
  - `Id`, `Name`, `Url`, `Username`, `Password`, `AutoLogin`
  - `Script` (存储脚本目录路径)
  - `Status`, `CreatedTime`, `LastRunTime`

### 2. **JSON 配置文件**（每个任务一个文件）
- **路径**: `C:\Users\Administrator\AppData\Local\永利系统\*.json`
- **服务**: `ConfigService`
- **用途**: 配置备份/导出，支持远程 HTTP 同步
- **字段**: 与数据库相同的配置信息

## ❌ 问题分析

### 重复存储的问题

1. **数据重复**
   - 数据库和 JSON 存储相同的信息
   - 增加了维护成本
   - 可能导致数据不一致

2. **同步问题**
   - `BrowserTaskControl` 保存配置时，只保存到 JSON
   - 没有自动同步回数据库
   - 需要手动订阅 `ConfigChanged` 事件来同步

3. **数据源混乱**
   - 启动时从数据库加载任务列表
   - 编辑时从 JSON 加载配置
   - 保存时只保存到 JSON，不保存到数据库

## ✅ 解决方案

### 方案1：统一到数据库（推荐）

**优点**：
- 单一数据源，避免不一致
- 简化架构，减少维护成本
- 数据库支持事务，更可靠

**实现**：
1. 移除 JSON 配置文件的自动保存
2. `BrowserTaskControl` 的 `ConfigChanged` 事件直接保存到数据库
3. 保留 JSON 仅用于：
   - 远程同步（HTTP 上传/下载）
   - 备份/导出功能

### 方案2：明确 JSON 的用途

**如果确实需要 JSON**（例如远程同步），应该：
1. **数据库**：主数据源，存储所有任务
2. **JSON**：仅用于远程同步和备份
3. **同步机制**：
   - 编辑时：从数据库加载 → 编辑 → 保存到数据库
   - 远程同步：数据库 ↔ JSON ↔ HTTP

## 🔄 当前数据流

```
启动时：
数据库 (ScriptTask) 
  → ScriptTaskAdapter.ToScriptTaskConfig()
  → BrowserTaskControl (ScriptTaskConfig)
  → 自动加载 JSON（重复！）

编辑时：
用户修改配置
  → BrowserTaskControl 保存到 JSON
  → ConfigChanged 事件（需要手动订阅）
  → 保存到数据库（如果订阅了）

问题：
- JSON 和数据库可能不一致
- 需要手动同步
```

## 💡 建议

**统一到数据库**：
1. 移除 `BrowserTaskControl` 的自动 JSON 保存
2. `ConfigChanged` 事件直接保存到数据库
3. JSON 仅用于远程同步（可选功能）

这样：
- ✅ 单一数据源（数据库）
- ✅ 自动同步
- ✅ 避免数据不一致
- ✅ 简化架构
