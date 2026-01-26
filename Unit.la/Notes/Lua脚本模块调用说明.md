# Lua 脚本模块调用说明

## 📋 问题

如何在 `main.lua` 中调用 `functions.lua` 中的登录函数？

## ✅ 解决方案

### 方案1：自动加载（推荐）

系统会在执行 `main.lua` 之前自动加载 `functions.lua`，所以你可以直接在 `main.lua` 中调用 `functions.lua` 中的函数。

**前提条件**：
- `main.lua` 和 `functions.lua` 必须在同一个脚本目录中
- 脚本目录路径已正确设置

**示例代码**：

#### functions.lua
```lua
-- ====================================
-- 功能库 (functions.lua)
-- ====================================

log('📚 功能库加载中...')

-- 登录函数
function login(username, password)
    log('🔐 开始登录: ' .. username)
    
    -- 检查登录元素是否存在
    if not web.Exists('#username') then
        log('❌ 未找到用户名输入框')
        return false
    end
    
    if not web.Exists('#password') then
        log('❌ 未找到密码输入框')
        return false
    end
    
    -- 输入用户名和密码
    web.InputAndTrigger('#username', username)
    web.InputAndTrigger('#password', password)
    
    -- 点击登录按钮
    if web.Exists('#loginBtn') then
        web.Click('#loginBtn')
        web.Wait(2000) -- 等待登录完成
        log('✅ 登录操作完成')
        return true
    else
        log('❌ 未找到登录按钮')
        return false
    end
end

-- 其他功能函数...
function getPageTitle()
    return web.GetTitle()
end
```

#### main.lua
```lua
-- ====================================
-- 主脚本 (main.lua)
-- ====================================

log('🚀 主脚本开始执行')

function main()
    -- 1. 导航到目标网站
    log('📍 步骤1: 导航到目标网站')
    local wret, werr = web.Navigate(config.url or 'https://example.com', -1)
    if wret then
        log('✅ 网站加载成功')
    end
    
    -- 2. 🔥 调用 functions.lua 中的登录函数
    log('🔐 步骤2: 登录')
    local loginSuccess = login(config.username or 'admin', config.password or 'password')
    
    if loginSuccess then
        log('✅ 登录成功')
    else
        log('❌ 登录失败')
        return false
    end
    
    -- 3. 执行业务逻辑
    log('💼 步骤3: 执行业务逻辑')
    local title = getPageTitle() -- 调用 functions.lua 中的其他函数
    log('📄 页面标题: ' .. title)
    
    log('✅ 主脚本执行完成')
    return true
end

-- ==============================
-- 异常处理回调函数（可选）
-- ==============================
function error(errorInfo)
    log('⚠️ error() 异常处理回调')
    log('   错误信息: ' .. errorInfo.message)
    log('   错误行号: ' .. tostring(errorInfo.lineNumber))
    return false -- 返回 false = 停止执行脚本
end

-- ==============================
-- 清理函数（可选）
-- ==============================
function exit()
    log('🔚 exit() 清理函数')
    log('   清理完成')
end
```

### 方案2：手动加载（如果方案1不工作）

如果系统没有自动加载 `functions.lua`，你可以在 `main.lua` 开头手动加载：

```lua
-- ====================================
-- 主脚本 (main.lua)
-- ====================================

-- 🔥 手动加载 functions.lua（如果系统没有自动加载）
-- 注意：这需要系统支持 dofile 或 loadfile
-- 如果 MoonSharp 不支持，请使用方案1

function main()
    -- 直接调用函数（假设 functions.lua 已加载）
    login(config.username, config.password)
    
    return true
end

function error(errorInfo)
    return false
end

function exit()
end
```

## 🔧 技术实现

### 当前系统行为

1. **脚本执行流程**：
   - 系统只执行 `main.lua` 的内容
   - `functions.lua` 需要手动加载或合并

2. **MoonSharp 脚本上下文**：
   - 所有脚本在同一个 `Script` 对象中执行
   - 如果先加载 `functions.lua`，再执行 `main.lua`，则 `main.lua` 可以访问 `functions.lua` 中的函数

### 推荐实现方式

**修改 `BrowserTaskControl.ExecuteScript()` 或 `MoonSharpScriptEngine.Execute()`**：

在执行 `main.lua` 之前，先加载同目录下的 `functions.lua`：

```csharp
// 伪代码示例
public ScriptResult Execute(string mainScriptCode, string scriptDirectory)
{
    // 1. 先加载 functions.lua（如果存在）
    var functionsPath = Path.Combine(scriptDirectory, "functions.lua");
    if (File.Exists(functionsPath))
    {
        var functionsCode = File.ReadAllText(functionsPath, Encoding.UTF8);
        _script.DoString(functionsCode); // 先加载 functions.lua
    }
    
    // 2. 再执行 main.lua
    return ExecuteWithLifecycle(mainScriptCode);
}
```

## 📝 注意事项

1. **函数命名冲突**：
   - 如果 `main.lua` 和 `functions.lua` 中有同名函数，后加载的会覆盖先加载的
   - 建议使用命名空间或函数前缀避免冲突

2. **执行顺序**：
   - `functions.lua` 必须在 `main.lua` 之前加载
   - 否则 `main.lua` 无法调用 `functions.lua` 中的函数

3. **错误处理**：
   - 如果 `functions.lua` 加载失败，应该记录错误但继续执行 `main.lua`
   - 或者完全停止执行

## 🎯 最佳实践

1. **函数库设计**：
   - `functions.lua` 只包含函数定义，不包含执行代码
   - 所有函数应该是纯函数或操作函数，不依赖执行顺序

2. **模块化**：
   - 将相关功能组织到不同的函数中
   - 使用清晰的函数命名

3. **文档注释**：
   - 为每个函数添加注释说明参数和返回值
   - 在 `functions.lua` 开头添加使用说明

## 📚 示例：完整的登录流程

### functions.lua
```lua
-- ====================================
-- 功能库：登录相关函数
-- ====================================

-- 执行登录操作
-- @param username 用户名
-- @param password 密码
-- @return boolean 登录是否成功
function login(username, password)
    log('🔐 开始登录: ' .. username)
    
    -- 等待登录页面加载
    if not web.WaitForElement('#username', 5000) then
        log('❌ 登录页面加载超时')
        return false
    end
    
    -- 输入凭据
    web.InputAndTrigger('#username', username)
    web.InputAndTrigger('#password', password)
    
    -- 点击登录
    web.Click('#loginBtn')
    web.Wait(2000)
    
    -- 检查登录结果
    if web.Exists('.user-info') then
        log('✅ 登录成功')
        return true
    else
        log('❌ 登录失败')
        return false
    end
end

-- 检查是否已登录
-- @return boolean 是否已登录
function isLoggedIn()
    return web.Exists('.user-info') or web.Exists('#logoutBtn')
end
```

### main.lua
```lua
-- ====================================
-- 主脚本
-- ====================================

log('🚀 主脚本开始执行')

function main()
    -- 导航
    web.Navigate(config.url, -1)
    web.WaitForLoad(10000)
    
    -- 检查是否已登录
    if not isLoggedIn() then
        -- 调用 functions.lua 中的登录函数
        if not login(config.username, config.password) then
            return false
        end
    else
        log('ℹ️ 已登录，跳过登录步骤')
    end
    
    -- 继续业务逻辑...
    return true
end

function error(errorInfo)
    log('❌ 错误: ' .. errorInfo.message)
    return false
end

function exit()
    log('🔚 清理完成')
end
```

---

**总结**：最简单的方式是确保系统在执行 `main.lua` 之前自动加载 `functions.lua`，这样你就可以直接在 `main.lua` 中调用 `functions.lua` 中的函数，就像调用本地函数一样。
