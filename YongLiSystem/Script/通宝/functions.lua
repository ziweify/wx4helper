-- ====================================
-- 功能库 (functions.lua)
-- ====================================

log('📚 功能库加载中...')

function login(username, password, url)
    -- 🔥 参数说明：
    -- username: 用户名
    -- password: 密码
    -- url: 目标网站URL（从 config.url 传递）
    
    -- 示例: 获取当前页面信息
    local initialUrl = web.GetUrl()
    log('login::当前URL: ' .. initialUrl)
    
    local title = web.GetTitle()
    log('login::页面标题: ' .. title)

    -- 1. 导航到目标网站
    log('📍 步骤1: 导航到目标网站')
    local wret, werr = web.Navigate(url, -1)
    if wret then
        log('网站加载成功')
    end
    
    -- 2. 准备登录相关元素选择器
    log('🔐 步骤2: ---登录---')
    local elUsername = 'input.username'
    local elPassword = 'input.password'
    local elImgcode = 'input.imgcode'
    local loginButton = 'li.l4.huiyuan'
    
    -- 3. 构建登录页面的 URL 模式（用于判断是否还在登录页）
    local loginUrlPattern = url
    -- 智能处理 URL：去掉末尾的 /，然后添加 /#/ 或 /
    if string.sub(loginUrlPattern, -1) == '/' then
        loginUrlPattern = string.sub(loginUrlPattern, 1, -2) -- 去掉末尾的 /
    end
    local loginUrl1 = loginUrlPattern .. '/#/'
    local loginUrl2 = loginUrlPattern .. '/'
    local loginUrl3 = loginUrlPattern -- 不包含 / 的情况
    
    log('🔍 登录页面 URL 模式: ' .. loginUrl1 .. ' 或 ' .. loginUrl2 .. ' 或 ' .. loginUrl3)
    
    -- 4. 循环登录，直到登录成功（URL 改变）
    log('🔄 步骤3: 开始登录循环（条件恒为真，直到登录成功）')
    local attempt = 0
    
    -- 🔥 将所有局部变量定义移到循环外，避免 goto 跳转到作用域内的问题
    local imgcodeValue = ''
    local maxWaitTime = 30000 -- 最多等待30秒
    local waitInterval = 200 -- 每200ms检查一次
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
        
        -- 检查登录表单是否存在
        if not web.Exists(elUsername) then
            log('⚠️ 用户名输入框不存在，等待页面加载...')
            web.Wait(1000)
            goto continue -- 继续下一次循环
        end
        
        -- 检查并写入用户名和密码（有相等判断，不会重复输入）
        currentUsername = web.GetValue(elUsername) or ''
        currentPassword = web.GetValue(elPassword) or ''
        
        if currentUsername ~= username then
            log('📝 写入用户名: ' .. username)
            web.InputAndTrigger(elUsername, username)
            web.Wait(500) -- 等待输入完成
        else
            log('✓ 用户名已正确，无需修改')
        end
        
        if currentPassword ~= password then
            log('📝 写入密码: ' .. string.rep('*', #password))
            web.InputAndTrigger(elPassword, password)
            web.Wait(500) -- 等待输入完成
        else
            log('✓ 密码已正确，无需修改')
        end
        
        -- 等待验证码输入完成（字符个数 == 4）
        log('⏳ 等待验证码输入完成（4个字符）')
        -- 🔥 重置变量（已在循环外定义）
        elapsedTime = 0
        imgcodeValue = ''
        
        while elapsedTime < maxWaitTime do
            imgcodeValue = web.GetValue(elImgcode) or ''
            if #imgcodeValue == 4 then
                log('✅ 验证码已输入完成: ' .. imgcodeValue)
                break
            end
            web.Wait(waitInterval)
            elapsedTime = elapsedTime + waitInterval
        end
        
        if #imgcodeValue ~= 4 then
            log('⚠️ 警告: 验证码未在30秒内输入完成，当前长度: ' .. #imgcodeValue)
            -- 继续尝试，不退出循环
        end
        
        -- 点击登录按钮（使用 class 选择器，避免使用动态的 data-v-* 属性）
        log('🖱️ 点击登录按钮')
        if web.Exists(loginButton) then
            web.Click(loginButton)
            log('✅ 已点击登录按钮')
            web.Wait(2000) -- 等待登录处理
            
            -- 再次检查 URL 是否改变
            currentUrl = web.GetUrl() or ''
            isLoginPage = (currentUrl == loginUrl1) or (currentUrl == loginUrl2) or (currentUrl == loginUrl3)
            if not isLoginPage then
                log('✅ 登录成功！当前页面已不在登录页: ' .. currentUrl)
                break
            else
                log('⏳ 仍在登录页，继续等待或重试...')
                web.Wait(1000) -- 等待一下再继续
            end
        else
            log('❌ 登录按钮未找到: ' .. loginButton)
            web.Wait(1000) -- 等待一下再继续
        end
        
        ::continue::
    end
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
