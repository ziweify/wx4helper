# Web 库快速参考

## 🚀 快速开始

```lua
-- web 对象已自动注册，直接使用即可！

-- 1. 导航
web.Navigate("https://example.com")
web.WaitForLoad()

-- 2. 点击
web.Click("#loginBtn")

-- 3. 输入
web.Input("#username", "admin")

-- 4. 获取信息
local title = web.GetTitle()
log("标题: " .. title)
```

## 📚 常用方法速查

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
web.IsVisible(selector)   -- 检查可见
```

### 等待
```lua
web.Wait(1000)            -- 等待1秒
web.WaitFor(sel, 5000)    -- 等待元素出现
web.WaitForLoad()         -- 等待页面加载
```

### 获取信息
```lua
web.GetUrl()              -- 当前URL
web.GetTitle()            -- 页面标题
web.GetHtml()             -- 页面HTML
```

### 表单
```lua
web.Select(sel, value)    -- 下拉选择
web.Check(sel, true)      -- 勾选复选框
web.Submit(sel)           -- 提交表单
```

## 💡 实战示例

### 登录
```lua
web.Navigate("https://example.com/login")
web.WaitForLoad()
web.Input("#username", "admin")
web.Input("#password", "123")
web.Click("#loginBtn")
```

### 数据采集
```lua
web.Navigate("https://example.com/data")
web.WaitFor(".data-table", 5000)
local texts = web.GetAllText(".item .title")
for i, text in ipairs(texts) do
    log("项目" .. i .. ": " .. text)
end
```

### 自动填表
```lua
web.Input("#name", "张三")
web.Select("#city", "北京")
web.Check("#agree", true)
web.Click("#submit")
```

---

📖 **完整文档**: 参见 `Web库完整实现-完成报告.md`
