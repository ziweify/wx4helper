-- ====================================
-- 功能库 (functions.lua)
-- ====================================

log('📚 功能库加载中...')

function login(username, password)
     -- 示例: 获取当前页面信息
    local url = web.GetUrl()
    log('login::当前URL: ' .. url)
    
    local title = web.GetTitle()
    log('login::页面标题: ' .. title)

    -- 1. 导航到目标网站
    log('📍 步骤1: 导航到目标网站')
     local wret, werr = web.Navigate(config.url,-1)
     if  wret then
        log('网站加载成功')
    end

    -- 2. 登录示例
    log('🔐 步骤2: ---登录---')
    local elUsername = 'input.username';
    local elPassword = 'input.password';
    if web.Exists(elUsername) then
        -- 3. 执行业务逻辑
        log('💼 步骤3: 执行业务逻辑')
        web.InputAndTrigger(elUsername, username)
        web.InputAndTrigger(elPassword, password)
        web.Click('#loginBtn')
        web.Wait(2000)
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
