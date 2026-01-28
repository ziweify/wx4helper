-- ====================================
-- 主脚本 (main.lua)
-- ====================================

-- 🔥 使用 require 加载功能库（支持错误时显示具体行号）
require("functions")

log('🚀 主脚本开始执行')

-- ==============================
-- 🔥 浏览器请求响应拦截示例
-- ==============================
-- 注册响应处理器，拦截所有 HTTP 响应
OnResponse(function(response)
    -- response 对象包含以下字段：
    --   url: 请求URL
    --   statusCode: HTTP状态码（200, 404, 500等）
    --   context: 响应内容（JSON、HTML等）
    --   postData: POST请求的数据（如果有）
    --   contentType: 响应内容类型（application/json, text/html等）
    --   referrerUrl: 来源URL
    
    -- 示例1: 记录所有响应
    log('📡 收到响应: ' .. response.url)
    log('   状态码: ' .. tostring(response.statusCode))
    log('   内容类型: ' .. response.contentType)
    
    -- 示例2: 只处理特定URL的响应
    if string.find(response.url, '/api/') then
        log('🔍 检测到API响应: ' .. response.url)
        log('   响应内容: ' .. string.sub(response.context, 1, 200)) -- 只显示前200个字符
        
        -- 可以在这里解析JSON、提取数据等
        -- local jsonData = parse_json(response.context)
        -- if jsonData then
        --     log('   解析后的数据: ' .. to_json(jsonData))
        -- end
    end
    
    -- 示例3: 处理登录相关的响应
    if string.find(response.url, '/login') or string.find(response.url, '/auth') then
        log('🔐 检测到登录相关响应')
        if response.statusCode == 200 then
            log('   ✅ 登录可能成功')
            -- 可以在这里检查响应内容，确认登录是否成功
            if string.find(response.context, 'success') or string.find(response.context, 'token') then
                log('   ✅ 登录确认成功')
            end
        elseif response.statusCode == 401 or response.statusCode == 403 then
            log('   ❌ 登录失败: 认证错误')
        end
    end
    
    -- 示例4: 处理错误响应
    if response.statusCode >= 400 then
        log('⚠️ 检测到错误响应: ' .. response.url)
        log('   状态码: ' .. tostring(response.statusCode))
        log('   错误内容: ' .. string.sub(response.context, 1, 200))
    end
    
    -- 示例5: 记录POST请求数据
    if string.len(response.postData) > 0 then
        log('📤 POST数据: ' .. string.sub(response.postData, 1, 200))
    end
end)

function main()
     local username = config.username or 'username'
     local password = config.password or 'password'
     local url = config.url or ''
     -- 🔥 调用 login 函数时，传递所有需要的参数（包括 url）
     login(username, password, url)
   
    
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
    
    -- 在这里可以执行异常处理逻辑：
    -- - 记录错误日志
    -- - 发送通知
    -- - 尝试恢复
    -- - 清理资源
    
    -- 示例：根据错误类型决定是否继续
    if string.find(errorInfo.message, '404') then
        log('   页面未找到，尝试重新导航')
        -- web.Navigate('https://backup-url.com')
        return true  -- 返回 true = 忽略异常，继续执行
    end
    
    -- 其他错误，停止执行
    log('   严重错误，停止执行')
    return false  -- 返回 false = 停止执行脚本
end

-- ==============================
-- 清理函数（可选）
-- ==============================
function exit()
    log('🔚 exit() 清理函数')
    
    -- 无论脚本是正常完成还是异常终止，都会执行这里
    -- 在这里可以执行清理工作：
    -- - 关闭连接
    -- - 保存状态
    -- - 释放资源
    -- - 发送完成通知
    
    log('   清理完成')
end

-- ==============================
-- 注意事项
-- ==============================
-- 1. main() 必须存在，这是脚本的入口点
-- 2. error() 可选，用于处理 main() 中的异常
-- 3. exit() 可选，无论如何都会执行，用于清理工作
-- 4. 如果不定义 error()，异常会直接导致脚本停止
-- 5. 如果不定义 exit()，脚本结束后不会执行清理