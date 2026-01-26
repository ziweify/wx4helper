-- ====================================
-- 主脚本 (main.lua)
-- ====================================

log('🚀 主脚本开始执行')

function main()
     local username = config.username or 'username'
     local password = config.password or 'password'
     login(username, password);
   
    
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