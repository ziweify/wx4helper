-- ====================================
-- 功能库 (functions.lua)
-- ====================================

log('📚 功能库加载中...')

function login(username, password, url)
    -- 参数说明:
    -- username: 用户名（从 config.username 传递）
    -- password: 密码（从 config.password 传递）
    -- url: 目标网站URL（从 config.url 传递）
    
    -- 示例: 获取当前页面信息
    local initialUrl = web.GetUrl()
    log('login::当前URL: ' .. initialUrl)
    
    local title = web.GetTitle()
    log('login::页面标题: ' .. title)

    -- 1. 导航到目标网站
    log('📍 步骤1: 导航到目标网站')
    local loginUrl1 = url:gsub('/+$', '')  -- 移除尾部斜杠
    local loginUrl2 = url:gsub('/+$', '') .. '/'  -- 确保有尾部斜杠
    local loginUrl3 = url:gsub('/+$', '') .. '/#/'  -- 登录页面URL
    
    log('🔗 目标URL: ' .. loginUrl3)
    local navRet, navErr = web.Navigate(loginUrl3, -1)
    if navRet then
        log('✅ 网站加载成功')
    else
        log('❌ 网站加载失败: ' .. (navErr or '未知错误'))
        return false
    end
    
    -- 等待页面加载
    web.WaitForLoad(10000)
    
    -- 2. 登录示例
    log('🔐 步骤2: 开始登录流程')
    local elUsername = 'input.username'
    local elPassword = 'input.password'
    local elImgcode = 'input.imgcode'
    
    -- 等待登录元素出现
    if not web.WaitFor(elUsername, 5000) then
        log('❌ 未找到用户名输入框')
        return false
    end
    
    log('✅ 登录元素已找到')
    
    -- 3. 开始登录循环（条件恒为真，直到登录成功）
    log('🔄 步骤3: 开始登录循环（条件恒为真，直到登录成功）')
    local attempt = 0
    
    -- 🔥 将所有局部变量定义移到循环外，避免 goto 跳转到作用域内的问题
    local imgcodeValue = ''
    local maxWaitTime = 30000 -- 最多等待30秒
    local waitInterval = 200 -- 200ms检查一次
    local elapsedTime = 0
    local currentUrl = ''
    local isLoginPage = false
    local currentUsername = ''
    local currentPassword = ''
    
    while true do
        attempt = attempt + 1
        log('📋 登录尝试 #' .. attempt)
        
        -- 获取当前 URL
        currentUrl = web.GetUrl() or ''
        log('📍 当前 URL: ' .. currentUrl)
        
        -- 检查是否已经登录成功（URL 不在登录页面）
        isLoginPage = (currentUrl == loginUrl1) or (currentUrl == loginUrl2) or (currentUrl == loginUrl3)
        if not isLoginPage then
            log('✅ 登录成功！当前页面已不在登录页: ' .. currentUrl)
            break
        end
        
        -- 等待验证码输入（最多等待30秒）
        log('⏳ 等待验证码输入...')
        imgcodeValue = web.GetValue(elImgcode) or ''
        if string.len(imgcodeValue) == 4 then
             -- 点击登录按钮
            log('🖱️ 点击登录按钮')
            local loginBtn = 'li.huiyuan > span'
            if web.Exists(loginBtn) then
                web.Click(loginBtn)
                log('✅ 登录按钮已点击')
            else
                log('❌ 未找到登录按钮: ' .. loginBtn)
            end
        end
        
        -- 检查并写入用户名和密码（有相等判断，不会重复输入）
        currentUsername = web.GetValue(elUsername) or ''
        currentPassword = web.GetValue(elPassword) or ''
        
        if currentUsername ~= username then
            log('📝 写入用户名: ' .. username)
            web.InputAndTrigger(elUsername, username)
            sleep(500)
        else
            log('✓ 用户名已正确: ' .. currentUsername)
        end
        
        if currentPassword ~= password then
            log('📝 写入密码: ***')
            web.InputAndTrigger(elPassword, password)
            sleep(500)
        else
            log('✓ 密码已正确')
        end
        
        -- 等待登录结果
        sleep(2000)
    end
    
    log('🎉 登录流程完成')
    return true
end

function getData()
    log('📊 获取数据')
    if not web.WaitFor('.data-table', 5000) then
        log('⚠️ 数据表格未找到')
        return nil
    end
    local texts = web.GetAllText('.data-row .title')
    return texts
end

function queryOrder(orderId)
    log('🔍 查询订单: ' .. orderId)
    web.Input('#orderId', orderId)
    web.Click('#searchBtn')
    web.Wait(1000)
    if web.WaitFor('.order-result', 3000) then
        return web.GetElementText('.order-result')
    end
    return nil
end

function placeBet(betData)
    log('💰 投注')
    web.Input('#betAmount', tostring(betData.amount))
    web.Select('#betType', betData.type)
    web.Click('#betBtn')
    web.Wait(1000)
    return web.Exists('.bet-success')
end

log('✅ 功能库加载完成')
