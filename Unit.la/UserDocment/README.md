# Unit.la 库 - 用户文档

> **欢迎使用 Unit.la 库！** 🎉  
> 这是一个功能完整的浏览器自动化和脚本任务库。

---

## 📚 文档导航

### 🚀 核心文档（快速参考）

1. **[使用手册](./使用手册.md)** ⭐⭐⭐ - **从这里开始！**
   - 完整的库使用指南
   - 10个结构化章节
   - 50+ 实战示例
   - 100% API 覆盖

2. **[Web库快速参考](./Web库-快速参考.md)** ⭐⭐ - API速查
   - 常用方法速查
   - 分类整理
   - 示例代码

---

## 🎯 快速开始

### 第一步：创建浏览器任务
```csharp
var config = new BrowserTaskConfig
{
    Name = "我的任务",
    Url = "https://example.com"
};
var task = new BrowserTaskControl(config);
task.Show();
```

### 第二步：编写 Lua 脚本
```lua
-- 导航到网站
web.Navigate("https://example.com")
web.WaitForLoad()

-- 点击按钮
web.Click("#loginBtn")

-- 获取数据
local title = web.GetTitle()
log("标题: " .. title)
```

---

## 📖 主要功能

### Web 库（43个方法）
- ✅ **导航控制** - Navigate, GoBack, GoForward, Reload
- ✅ **DOM操作** - Click, Input, GetText, Exists, IsVisible
- ✅ **等待机制** - Wait, WaitFor, WaitForLoad
- ✅ **JavaScript执行** - Execute, ExecuteJson
- ✅ **表单操作** - Select, Check, Submit
- ✅ **高级功能** - Screenshot, InjectCss, OpenDevTools

### 脚本系统
- ✅ Lua 语法高亮
- ✅ 断点支持
- ✅ 错误检测
- ✅ 自动完成
- ✅ Ctrl+S 保存

---

## 🔍 常用 API 速查

### 导航
```lua
web.Navigate(url)         -- 导航到URL
web.GoBack()              -- 后退
web.GoForward()           -- 前进
web.Reload()              -- 刷新
```

### 元素操作
```lua
web.Click(selector)       -- 点击
web.Input(selector, text) -- 输入
web.GetElementText(sel)   -- 获取文本
web.Exists(selector)      -- 检查存在
```

### 等待
```lua
web.Wait(1000)            -- 等待1秒
web.WaitFor(sel, 5000)    -- 等待元素出现
web.WaitForLoad()         -- 等待页面加载
```

---

## 💡 实战示例

### 自动登录
```lua
web.Navigate("https://example.com/login")
web.WaitForLoad()
web.Input("#username", "admin")
web.Input("#password", "123456")
web.Click("#loginBtn")
```

### 数据采集
```lua
web.Navigate("https://example.com/data")
web.WaitFor(".data-table", 5000)
local titles = web.GetAllText(".item .title")
for i, title in ipairs(titles) do
    log("数据" .. i .. ": " .. title)
end
```

---

## 📞 获取帮助

- 📖 完整指南：查看 [使用手册.md](./使用手册.md)
- ⚡ API速查：查看 [Web库-快速参考.md](./Web库-快速参考.md)
- 🔧 技术文档：参见 `../Notes/` 目录

---

**© 2026 Unit.la Library. All rights reserved.**
