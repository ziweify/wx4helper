using zhaocaimao.Shared.Models;
using zhaocaimao.Services.AutoBet.Browser.Models;
using zhaocaimao.Services.AutoBet.Browser.Services;
using Microsoft.Web.WebView2.WinForms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Linq;
using BrowserOddsInfo = zhaocaimao.Services.AutoBet.Browser.Models.OddsInfo;
using BrowserResponseEventArgs = zhaocaimao.Services.AutoBet.Browser.Services.ResponseEventArgs;

namespace zhaocaimao.Services.AutoBet.Browser.PlatformScripts
{
    /// <summary>
    /// YYDS 平台脚本
    /// 平台地址: https://client.06n.yyds666.me/
    /// 登录页面: https://client.06n.yyds666.me/login?redirect=%2F
    /// </summary>
    public class YydsScript : IPlatformScript
    {
        private readonly WebView2 _webView;
        private readonly Action<string> _logCallback;
        private readonly HttpClient _httpClient = new HttpClient();
        
        // 关键参数（从拦截中获取或cookie中提取）
        private string _token = "";
        private string _sessionId = "";
        private decimal _currentBalance = 0;
        private string _baseUrl = "https://client.06n.yyds666.me";
        
        // 赔率ID映射表
        private readonly Dictionary<string, string> _oddsMap = new Dictionary<string, string>();
        
        // 赔率值映射表
        private readonly Dictionary<string, float> _oddsValues = new Dictionary<string, float>();
        
        public YydsScript(WebView2 webView, Action<string> logCallback)
        {
            _webView = webView;
            _logCallback = logCallback;
            
            // 配置HttpClient
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json, text/javascript, */*; q=0.01");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36");
        }
        
        /// <summary>
        /// 登录 - 自动填充表单，用户输入验证码后点击登录
        /// </summary>
        public async Task<bool> LoginAsync(string username, string password)
        {
            try
            {
                _logCallback($"🔐 开始登录 YYDS: {username}");
                
                // 🔥 等待 WebView2 初始化
                var initWaitCount = 0;
                while (_webView.CoreWebView2 == null && initWaitCount < 30)
                {
                    _logCallback($"⏳ 等待 WebView2 初始化... ({initWaitCount + 1}/30)");
                    await Task.Delay(1000);
                    initWaitCount++;
                }
                
                if (_webView.CoreWebView2 == null)
                {
                    _logCallback("❌ WebView2 初始化超时");
                    return false;
                }
                
                _logCallback("✅ WebView2 已初始化，开始登录流程");
                
                // #region agent log
                // 🔥 DEBUG: 检查当前URL和页面状态（假设F）
                var checkInitialUrlScript = @"
                    (function() {
                        fetch('http://127.0.0.1:7242/ingest/9756b6bb-934b-4f2a-9616-4fac9cf9b59f',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({location:'YydsScript.cs:78',message:'初始URL检查',data:{url:window.location.href,readyState:document.readyState,bodyHTML:(document.body?.innerHTML || 'no body').substring(0, 200)},timestamp:Date.now(),sessionId:'debug-session',hypothesisId:'F'})}).catch(()=>{});
                        return {url: window.location.href};
                    })();
                ";
                await _webView.CoreWebView2.ExecuteScriptAsync(checkInitialUrlScript);
                
                // 🔥 DEBUG: 检查 div.login_submit 的可点击性
                var checkLoginButtonClickableScript = @"
                    (function() {
                        try {
                            const loginBtn = document.querySelector('.login_submit');
                            if (loginBtn) {
                                const rect = loginBtn.getBoundingClientRect();
                                const computed = window.getComputedStyle(loginBtn);
                                const elemAtCenter = document.elementFromPoint(rect.left + rect.width/2, rect.top + rect.height/2);
                                
                                fetch('http://127.0.0.1:7242/ingest/9756b6bb-934b-4f2a-9616-4fac9cf9b59f',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({location:'YydsScript.cs:85',message:'登录按钮可点击性检查',data:{rect:{top:rect.top,left:rect.left,width:rect.width,height:rect.height},display:computed.display,visibility:computed.visibility,pointerEvents:computed.pointerEvents,zIndex:computed.zIndex,opacity:computed.opacity,isObscured:elemAtCenter !== loginBtn,obscuringElement:elemAtCenter?.tagName + '.' + elemAtCenter?.className},timestamp:Date.now(),sessionId:'debug-session',hypothesisId:'H'})}).catch(()=>{});
                            }
                            return {checked: true};
                        } catch(e) {
                            return {error: e.message};
                        }
                    })();
                ";
                await _webView.CoreWebView2.ExecuteScriptAsync(checkLoginButtonClickableScript);
                
                // 🔥 DEBUG: 检查页面上的JavaScript框架和库
                var checkFrameworksScript = @"
                    (function() {
                        const frameworks = {
                            hasVue: typeof Vue !== 'undefined' || !!document.querySelector('[data-v-]') || !!document.querySelector('[v-]'),
                            hasReact: typeof React !== 'undefined' || !!document.querySelector('[data-reactroot]') || !!document.querySelector('[data-reactid]'),
                            hasAngular: typeof angular !== 'undefined' || !!document.querySelector('[ng-app]') || !!document.querySelector('[ng-controller]'),
                            hasJQuery: typeof jQuery !== 'undefined' || typeof $ !== 'undefined',
                            scriptsCount: document.querySelectorAll('script').length,
                            scriptsSrc: Array.from(document.querySelectorAll('script[src]')).map(s => s.src.substring(s.src.lastIndexOf('/') + 1))
                        };
                        fetch('http://127.0.0.1:7242/ingest/9756b6bb-934b-4f2a-9616-4fac9cf9b59f',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({location:'YydsScript.cs:100',message:'JavaScript框架检测',data:frameworks,timestamp:Date.now(),sessionId:'debug-session',hypothesisId:'H'})}).catch(()=>{});
                        return frameworks;
                    })();
                ";
                await _webView.CoreWebView2.ExecuteScriptAsync(checkFrameworksScript);
                // #endregion
                
                // 1. 先导航到登录页面（如果尚未在登录页）（现在可以安全访问 CoreWebView2）
                var currentUrl = await _webView.CoreWebView2.ExecuteScriptAsync("window.location.href");
                currentUrl = currentUrl?.Trim('"') ?? "";
                
                _logCallback($"📍 当前URL: {currentUrl}");
                
                if (!currentUrl.Contains("/login"))
                {
                    _logCallback("📍 导航到登录页面...");
                    _webView.CoreWebView2.Navigate($"{_baseUrl}/login?redirect=%2F");
                    await Task.Delay(3000);  // 🔥 增加等待时间到3秒，等待重定向完成
                    
                    // 🔥 验证导航是否成功
                    currentUrl = await _webView.CoreWebView2.ExecuteScriptAsync("window.location.href");
                    currentUrl = currentUrl?.Trim('"') ?? "";
                    _logCallback($"📍 导航后URL: {currentUrl}");
                }
                
                // 2. 等待页面完全加载
                _logCallback("⏳ 等待页面加载...");
                var waitCount = 0;
                bool pageReady = false;
                
                while (!pageReady && waitCount < 50)  // 最多等待5秒
                {
                    try
                    {
                        var checkPageScript = @"document.readyState === 'complete' ? 'ready' : document.readyState";
                        var result = await _webView.CoreWebView2.ExecuteScriptAsync(checkPageScript);
                        result = result?.Trim('"') ?? "";
                        
                        if (result == "ready")
                        {
                            pageReady = true;
                            _logCallback("✅ 页面已加载完成");
                        }
                        else
                        {
                            if (waitCount % 10 == 0)
                            {
                                _logCallback($"⏳ 页面加载中... 状态: {result}");
                            }
                            await Task.Delay(100);
                            waitCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logCallback($"⚠️ 页面状态检测异常: {ex.Message}");
                        await Task.Delay(100);
                        waitCount++;
                    }
                }
                
                // 3. 等待登录表单加载
                _logCallback("⏳ 等待登录表单加载...");
                waitCount = 0;
                bool formReady = false;
                
                while (!formReady && waitCount < 100)  // 最多等待10秒
                {
                    try
                    {
                        var checkFormScript = @"
                            (function() {
                                try {
                                    const usernameInput = document.querySelector('input[name=""username""]');
                                    const passwordInput = document.querySelector('input[name=""password""]');
                                    const codeInput = document.querySelector('input[name=""code""]');
                                    
                                    return JSON.stringify({
                                        url: window.location.href,
                                        hasUsername: !!usernameInput,
                                        hasPassword: !!passwordInput,
                                        hasCode: !!codeInput,
                                        ready: !!(usernameInput && passwordInput && codeInput),
                                        allInputs: document.querySelectorAll('input').length,
                                        bodyText: document.body ? document.body.innerText.substring(0, 100) : 'no body'
                                    });
                                } catch (e) {
                                    return JSON.stringify({ error: e.message });
                                }
                            })();
                        ";
                        
                        var result = await _webView.CoreWebView2.ExecuteScriptAsync(checkFormScript);
                        
                        // 🔥 处理 null 或空字符串
                        if (string.IsNullOrWhiteSpace(result) || result == "null")
                        {
                            if (waitCount % 20 == 0)
                            {
                                _logCallback($"📊 表单检测状态 ({waitCount * 0.1:F1}s): JavaScript返回null，页面可能未加载");
                            }
                            await Task.Delay(100);
                            waitCount++;
                            continue;
                        }
                        
                        result = result.Trim('"').Replace("\\\"", "\"");
                        
                        // 🔥 每2秒输出一次调试信息
                        if (waitCount % 20 == 0)
                        {
                            _logCallback($"📊 表单检测状态 ({waitCount * 0.1:F1}s): {result}");
                        }
                        
                        var checkResult = Newtonsoft.Json.Linq.JObject.Parse(result);
                        
                        // 检查是否有错误
                        if (checkResult["error"] != null)
                        {
                            _logCallback($"⚠️ JavaScript执行错误: {checkResult["error"]}");
                            await Task.Delay(100);
                            waitCount++;
                            continue;
                        }
                        
                        formReady = checkResult["ready"]?.Value<bool>() ?? false;
                        
                        if (!formReady)
                        {
                            await Task.Delay(100);
                            waitCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        // 只在关键时刻输出异常
                        if (waitCount % 20 == 0)
                        {
                            _logCallback($"⚠️ 表单检测异常 ({waitCount * 0.1:F1}s): {ex.Message}");
                        }
                        await Task.Delay(100);
                        waitCount++;
                    }
                }
                
                if (!formReady)
                {
                    _logCallback("❌ 登录表单加载超时");
                    return false;
                }
                
                _logCallback("✅ 登录表单已加载");
                
                // 3. 自动填充用户名和密码
                var fillFormScript = $@"
                    (function() {{
                        try {{
                            // 用户名: <input tabindex=""1"" class=""gaia le val login_input"" size=""16"" type=""text"" name=""username"">
                            const usernameInput = document.querySelector('input[name=""username""]');
                            
                            // 密码: <input class=""gaia le val login_input"" type=""password"" id=""txtPass"" tabindex=""2"" size=""14"" name=""password"">
                            const passwordInput = document.querySelector('input[name=""password""]');
                            
                            // 验证码: <input class=""login_input"" autocomplete=""off"" tabindex=""3"" size=""5"" maxlength=""4"" name=""code"">
                            const codeInput = document.querySelector('input[name=""code""]');
                            
                            if (usernameInput && passwordInput && codeInput) {{
                                // 填充用户名和密码
                                usernameInput.value = '{username}';
                                passwordInput.value = '{password}';
                                
                                // 触发事件（可能有Vue/React监听）
                                usernameInput.dispatchEvent(new Event('input', {{ bubbles: true }}));
                                usernameInput.dispatchEvent(new Event('change', {{ bubbles: true }}));
                                passwordInput.dispatchEvent(new Event('input', {{ bubbles: true }}));
                                passwordInput.dispatchEvent(new Event('change', {{ bubbles: true }}));
                                
                                // 聚焦到验证码输入框（提示用户输入）
                                codeInput.focus();
                                
                                return {{ success: true, message: '用户名和密码已填充' }};
                            }} else {{
                                return {{ success: false, message: '未找到登录表单元素' }};
                            }}
                        }} catch (error) {{
                            return {{ success: false, message: error.message }};
                        }}
                    }})();
                ";
                
                var fillResult = await _webView.CoreWebView2.ExecuteScriptAsync(fillFormScript);
                var fillJson = JObject.Parse(fillResult);
                
                var success = fillJson["success"]?.Value<bool>() ?? false;
                var message = fillJson["message"]?.ToString() ?? "";
                
                if (!success)
                {
                    _logCallback($"❌ 填充表单失败: {message}");
                    return false;
                }
                
                _logCallback($"✅ {message}");
                
                // #region agent log
                // 🔥 DEBUG: 检查填充后的字段值和状态（假设A）
                var checkFieldsAfterFillScript = @"
                    (function() {
                        try {
                            const usernameInput = document.querySelector('input[name=""username""]');
                            const passwordInput = document.querySelector('input[name=""password""]');
                            const codeInput = document.querySelector('input[name=""code""]');
                            
                            fetch('http://127.0.0.1:7242/ingest/9756b6bb-934b-4f2a-9616-4fac9cf9b59f',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({location:'YydsScript.cs:266',message:'填充后字段状态',data:{usernameValue:usernameInput?.value,usernameLength:usernameInput?.value?.length,passwordValue:'******',passwordLength:passwordInput?.value?.length,codeValue:codeInput?.value,codeLength:codeInput?.value?.length,usernameDisabled:usernameInput?.disabled,passwordDisabled:passwordInput?.disabled,codeDisabled:codeInput?.disabled},timestamp:Date.now(),sessionId:'debug-session',hypothesisId:'A'})}).catch(()=>{});
                            
                            return {logged: true};
                        } catch(e) {
                            return {error: e.message};
                        }
                    })();
                ";
                await _webView.CoreWebView2.ExecuteScriptAsync(checkFieldsAfterFillScript);
                // #endregion
                
                // 🔥 添加控制台错误监听
                var setupConsoleScript = @"
                    (function() {
                        window.__yyds_console_errors = [];
                        const originalError = console.error;
                        console.error = function(...args) {
                            window.__yyds_console_errors.push(args.join(' '));
                            originalError.apply(console, args);
                        };
                        return 'Console监听已设置';
                    })();
                ";
                await _webView.CoreWebView2.ExecuteScriptAsync(setupConsoleScript);
                
                // #region agent log
                // 🔥 DEBUG: 详细检测所有可能的登录按钮和可点击元素
                var detectAllButtonsScript = @"
(function() {
    try {
        // 查找所有可能的按钮
        const allButtons = Array.from(document.querySelectorAll('button, input[type=""button""], input[type=""submit""], div[class*=""btn""], div[class*=""button""], a[class*=""btn""]'));
        const allClickableElements = Array.from(document.querySelectorAll('[onclick], [class*=""login""], [class*=""submit""]'));
        
        const buttonInfo = allButtons.map((btn, idx) => ({
            index: idx,
            tagName: btn.tagName,
            type: btn.type || 'none',
            className: btn.className,
            id: btn.id,
            text: (btn.innerText || btn.value || '').substring(0, 30),
            disabled: btn.disabled,
            onclick: btn.onclick ? 'has onclick' : 'no onclick'
        }));
        
        const clickableInfo = allClickableElements.map((el, idx) => ({
            index: idx,
            tagName: el.tagName,
            className: el.className,
            id: el.id,
            text: (el.innerText || el.textContent || '').substring(0, 30)
        }));
        
        fetch('http://127.0.0.1:7242/ingest/9756b6bb-934b-4f2a-9616-4fac9cf9b59f',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({location:'YydsScript.cs:282',message:'所有按钮和可点击元素',data:{buttonsCount:buttonInfo.length,buttons:buttonInfo,clickableCount:clickableInfo.length,clickable:clickableInfo},timestamp:Date.now(),sessionId:'debug-session',hypothesisId:'C'})}).catch(()=>{});
        
        return {success: true, found: buttonInfo.length};
    } catch (e) {
        return { error: e.message };
    }
})();
";
                await _webView.CoreWebView2.ExecuteScriptAsync(detectAllButtonsScript);
                // #endregion
                
                // 🔥 添加登录按钮检测和辅助点击功能（修复：支持 div.login_submit）
                var detectLoginButtonScript = @"
(function() {
    try {
        // 🔥 优先查找 div.login_submit（YYDS 平台使用的按钮）
        const loginButton = document.querySelector('.login_submit') ||
                           document.querySelector('div.login_submit') ||
                           document.querySelector('button[type=""submit""]') ||
                           document.querySelector('input[type=""submit""]') ||
                           document.querySelector('button[class*=""login""]') ||
                           document.querySelector('button[class*=""btn""]') ||
                           document.querySelector('.login-button') ||
                           document.getElementById('loginBtn') ||
                           document.querySelector('[onclick*=""login""]');
        
        if (loginButton) {
            const isDisabled = loginButton.disabled || loginButton.classList.contains('disabled');
            const buttonText = loginButton.innerText || loginButton.textContent || loginButton.value || '未知';
            
            window.__yyds_login_button = loginButton;
            
            // 🔥 F8 快捷键支持
            window.addEventListener('keydown', function(e) {
                if (e.key === 'F8' && window.__yyds_login_button) {
                    console.log('F8触发登录');
                    window.__yyds_login_button.click();
                }
            });
            
            // 🔥 添加调试日志：记录找到的按钮信息
            fetch('http://127.0.0.1:7242/ingest/9756b6bb-934b-4f2a-9616-4fac9cf9b59f',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({location:'YydsScript.cs:320',message:'找到登录按钮',data:{tagName:loginButton.tagName,className:loginButton.className,id:loginButton.id,text:buttonText,disabled:isDisabled},timestamp:Date.now(),sessionId:'debug-session',hypothesisId:'H'})}).catch(()=>{});
            
            return {
                found: true,
                disabled: isDisabled,
                text: buttonText,
                tagName: loginButton.tagName,
                className: loginButton.className
            };
        }
        
        return { found: false };
    } catch (e) {
        return { error: e.message };
    }
})();
";
                
                var btnResult = await _webView.CoreWebView2.ExecuteScriptAsync(detectLoginButtonScript);
                var btnJson = JObject.Parse(btnResult);
                
                if (btnJson["found"]?.Value<bool>() == true)
                {
                    var isDisabled = btnJson["disabled"]?.Value<bool>() ?? false;
                    var btnText = btnJson["text"]?.ToString() ?? "未知";
                    var tagName = btnJson["tagName"]?.ToString() ?? "未知";
                    
                    _logCallback($"🔘 检测到登录按钮: [{tagName}] {btnText} (禁用:{isDisabled})");
                    _logCallback("💡 提示: 输入验证码后，可以按 F8 键自动点击登录按钮");
                }
                else
                {
                    _logCallback("⚠️ 未检测到登录按钮，请手动点击");
                }
                
                // #region agent log  
                // 🔥 DEBUG: 读取 globalConfig.js 的内容来理解登录逻辑
                var readGlobalConfigScript = @"
                    (function() {
                        return new Promise((resolve) => {
                            try {
                                const scripts = Array.from(document.querySelectorAll('script'));
                                const globalConfigScript = scripts.find(s => s.src && s.src.includes('globalConfig.js'));
                                
                                if (globalConfigScript) {
                                    fetch(globalConfigScript.src)
                                        .then(r => r.text())
                                        .then(content => {
                                            fetch('http://127.0.0.1:7242/ingest/9756b6bb-934b-4f2a-9616-4fac9cf9b59f',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({location:'YydsScript.cs:355',message:'globalConfig.js内容',data:{scriptContent:content.substring(0, 2000)},timestamp:Date.now(),sessionId:'debug-session',hypothesisId:'H'})}).catch(()=>{});
                                            resolve({success: true});
                                        })
                                        .catch(e => resolve({error: e.message}));
                                } else {
                                    resolve({error: 'globalConfig.js not found'});
                                }
                            } catch(e) {
                                resolve({error: e.message});
                            }
                        });
                    })();
                ";
                try
                {
                    await _webView.CoreWebView2.ExecuteScriptAsync(readGlobalConfigScript);
                }
                catch { }
                
                // 🔥 DEBUG: 直接检查 div.login_submit 上绑定的事件和属性
                var inspectLoginButtonScript = @"
                    (function() {
                        try {
                            const loginBtn = document.querySelector('.login_submit');
                            if (!loginBtn) {
                                return {error: 'login_submit not found'};
                            }
                            
                            // 获取所有属性
                            const attrs = {};
                            for (let i = 0; i < loginBtn.attributes.length; i++) {
                                const attr = loginBtn.attributes[i];
                                attrs[attr.name] = attr.value;
                            }
                            
                            // 检查是否有onclick属性
                            const hasOnClick = !!loginBtn.onclick || !!loginBtn.getAttribute('onclick');
                            const onclickStr = loginBtn.getAttribute('onclick') || 'none';
                            
                            // 获取所有内联样式
                            const styles = loginBtn.style.cssText;
                            
                            // 检查computed样式
                            const computed = window.getComputedStyle(loginBtn);
                            const computedStyles = {
                                display: computed.display,
                                cursor: computed.cursor,
                                pointerEvents: computed.pointerEvents
                            };
                            
                            const data = {
                                attributes: attrs,
                                hasOnClick: hasOnClick,
                                onclickAttribute: onclickStr.substring(0, 200),
                                inlineStyles: styles,
                                computedStyles: computedStyles,
                                innerHTML: loginBtn.innerHTML,
                                textContent: loginBtn.textContent
                            };
                            
                            fetch('http://127.0.0.1:7242/ingest/9756b6bb-934b-4f2a-9616-4fac9cf9b59f',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({location:'YydsScript.cs:365',message:'登录按钮详细检查',data:data,timestamp:Date.now(),sessionId:'debug-session',hypothesisId:'H'})}).catch(()=>{});
                            
                            return data;
                        } catch(e) {
                            return {error: e.message};
                        }
                    })();
                ";
                await _webView.CoreWebView2.ExecuteScriptAsync(inspectLoginButtonScript);
                // #endregion
                
                _logCallback("⏳ 请输入验证码并点击登录按钮（或按 F8）...");
                
                // #region agent log
                // 🔥 DEBUG: 添加验证码输入框监听（假设B）和按钮状态监听（假设C、H）
                var setupFieldMonitoringScript = @"
                    (function() {
                        try {
                            const codeInput = document.querySelector('input[name=""code""]');
                            // 🔥 优先查找 div.login_submit
                            const loginButton = document.querySelector('.login_submit') ||
                                               document.querySelector('div.login_submit') ||
                                               document.querySelector('button[type=""submit""]') ||
                                               document.querySelector('input[type=""submit""]') ||
                                               document.querySelector('button[class*=""login""]') ||
                                               window.__yyds_login_button;
                            
                            // 监听验证码输入
                            if (codeInput) {
                                ['input', 'change', 'blur', 'focus'].forEach(eventType => {
                                    codeInput.addEventListener(eventType, function(e) {
                                        fetch('http://127.0.0.1:7242/ingest/9756b6bb-934b-4f2a-9616-4fac9cf9b59f',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({location:'YydsScript.cs:337',message:'验证码输入框事件',data:{eventType:eventType,codeValue:e.target.value,codeLength:e.target.value?.length},timestamp:Date.now(),sessionId:'debug-session',hypothesisId:'B'})}).catch(()=>{});
                                    });
                                });
                            }
                            
                            // 🔥 特别监听 div.login_submit（假设H）
                            const loginSubmitDiv = document.querySelector('.login_submit');
                            if (loginSubmitDiv) {
                                // 🔥 检查这个div上已有的事件监听器
                                const listeners = getEventListeners ? getEventListeners(loginSubmitDiv) : {};
                                fetch('http://127.0.0.1:7242/ingest/9756b6bb-934b-4f2a-9616-4fac9cf9b59f',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({location:'YydsScript.cs:370',message:'登录按钮已有监听器',data:{hasGetEventListeners:!!getEventListeners,listenerKeys:Object.keys(listeners)},timestamp:Date.now(),sessionId:'debug-session',hypothesisId:'H'})}).catch(()=>{});
                                
                                // 监听多种事件 - 同时使用捕获和冒泡阶段
                                ['click', 'mousedown', 'mouseup', 'touchstart', 'touchend', 'pointerdown', 'pointerup'].forEach(eventType => {
                                    // 捕获阶段
                                    loginSubmitDiv.addEventListener(eventType, function(e) {
                                        fetch('http://127.0.0.1:7242/ingest/9756b6bb-934b-4f2a-9616-4fac9cf9b59f',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({location:'YydsScript.cs:375',message:'登录按钮事件-捕获阶段',data:{eventType:eventType,phase:'capture',className:loginSubmitDiv.className,defaultPrevented:e.defaultPrevented,propagationStopped:e.cancelBubble,isTrusted:e.isTrusted},timestamp:Date.now(),sessionId:'debug-session',hypothesisId:'H'})}).catch(()=>{});
                                    }, true);
                                    
                                    // 冒泡阶段
                                    loginSubmitDiv.addEventListener(eventType, function(e) {
                                        fetch('http://127.0.0.1:7242/ingest/9756b6bb-934b-4f2a-9616-4fac9cf9b59f',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({location:'YydsScript.cs:380',message:'登录按钮事件-冒泡阶段',data:{eventType:eventType,phase:'bubble',className:loginSubmitDiv.className,defaultPrevented:e.defaultPrevented,propagationStopped:e.cancelBubble,isTrusted:e.isTrusted},timestamp:Date.now(),sessionId:'debug-session',hypothesisId:'H'})}).catch(()=>{});
                                    }, false);
                                });
                                
                                // 🔥 使用 Object.defineProperty 劫持 onclick
                                const originalOnClick = loginSubmitDiv.onclick;
                                Object.defineProperty(loginSubmitDiv, 'onclick', {
                                    get: function() {
                                        return originalOnClick;
                                    },
                                    set: function(fn) {
                                        fetch('http://127.0.0.1:7242/ingest/9756b6bb-934b-4f2a-9616-4fac9cf9b59f',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({location:'YydsScript.cs:390',message:'登录按钮onclick被设置',data:{hasFn:!!fn},timestamp:Date.now(),sessionId:'debug-session',hypothesisId:'H'})}).catch(()=>{});
                                        originalOnClick = fn;
                                    }
                                });
                            }
                            
                            // 🔥 监听所有按钮和可点击元素的点击
                            const allClickable = document.querySelectorAll('button, input[type=""button""], input[type=""submit""], div[class*=""btn""], div[class*=""button""], div[class*=""submit""], a[class*=""btn""], [onclick], [class*=""login""]');
                            allClickable.forEach((el, idx) => {
                                el.addEventListener('click', function(e) {
                                    const elInfo = {
                                        index: idx,
                                        tagName: el.tagName,
                                        className: el.className,
                                        id: el.id,
                                        text: (el.innerText || el.textContent || el.value || '').substring(0, 30),
                                        disabled: el.disabled,
                                        defaultPrevented: e.defaultPrevented,
                                        isTrusted: e.isTrusted
                                    };
                                    fetch('http://127.0.0.1:7242/ingest/9756b6bb-934b-4f2a-9616-4fac9cf9b59f',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({location:'YydsScript.cs:380',message:'元素被点击',data:elInfo,timestamp:Date.now(),sessionId:'debug-session',hypothesisId:'C_H'})}).catch(()=>{});
                                }, true); // 使用捕获阶段确保能捕获到事件
                            });
                            
                            // 监听按钮状态变化
                            if (loginButton) {
                                const observer = new MutationObserver(function(mutations) {
                                    mutations.forEach(function(mutation) {
                                        if (mutation.type === 'attributes' && (mutation.attributeName === 'disabled' || mutation.attributeName === 'class')) {
                                            fetch('http://127.0.0.1:7242/ingest/9756b6bb-934b-4f2a-9616-4fac9cf9b59f',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({location:'YydsScript.cs:390',message:'登录按钮状态变化',data:{disabled:loginButton.disabled,className:loginButton.className,attributeName:mutation.attributeName},timestamp:Date.now(),sessionId:'debug-session',hypothesisId:'C'})}).catch(()=>{});
                                        }
                                    });
                                });
                                observer.observe(loginButton, {attributes: true});
                            }
                            
                            return {success: true, monitoredElements: allClickable.length, hasLoginSubmit: !!loginSubmitDiv};
                        } catch(e) {
                            return {error: e.message};
                        }
                    })();
                ";
                await _webView.CoreWebView2.ExecuteScriptAsync(setupFieldMonitoringScript);
                // #endregion
                
                // #region agent log
                // 🔥 DEBUG: 检查表单提交机制、隐藏字段和网络请求监听（假设D、E、G）
                var checkFormMechanismScript = @"
                    (function() {
                        try {
                            const form = document.querySelector('form');
                            const allInputs = Array.from(document.querySelectorAll('input'));
                            const hiddenInputs = allInputs.filter(i => i.type === 'hidden');
                            const usernameInput = document.querySelector('input[name=""username""]');
                            const passwordInput = document.querySelector('input[name=""password""]');
                            const codeInput = document.querySelector('input[name=""code""]');
                            
                            // 检查隐藏字段
                            const hiddenFields = hiddenInputs.map(i => ({name: i.name, value: i.value?.substring(0, 20)}));
                            
                            // 检查表单验证状态（假设D）
                            const formValid = form?.checkValidity ? form.checkValidity() : 'unknown';
                            const usernameValid = usernameInput?.checkValidity ? usernameInput.checkValidity() : 'unknown';
                            const passwordValid = passwordInput?.checkValidity ? passwordInput.checkValidity() : 'unknown';
                            const codeValid = codeInput?.checkValidity ? codeInput.checkValidity() : 'unknown';
                            
                            // 🔥 检查表单的 action 和 method（假设G）
                            const formAction = form?.action || 'no action';
                            const formMethod = form?.method || 'no method';
                            
                            fetch('http://127.0.0.1:7242/ingest/9756b6bb-934b-4f2a-9616-4fac9cf9b59f',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({location:'YydsScript.cs:420',message:'表单验证状态和隐藏字段',data:{formValid:formValid,usernameValid:usernameValid,passwordValid:passwordValid,codeValid:codeValid,hiddenFieldsCount:hiddenFields.length,hiddenFields:hiddenFields,allInputsCount:allInputs.length,formAction:formAction,formMethod:formMethod,currentUrl:window.location.href},timestamp:Date.now(),sessionId:'debug-session',hypothesisId:'D_E_G'})}).catch(()=>{});
                            
                            // 🔥 监听表单提交事件 - 捕获和冒泡两个阶段（假设D）
                            if (form) {
                                // 捕获阶段
                                form.addEventListener('submit', function(e) {
                                    const formData = {
                                        username: document.querySelector('input[name=""username""]')?.value,
                                        password: '******',
                                        code: document.querySelector('input[name=""code""]')?.value
                                    };
                                    fetch('http://127.0.0.1:7242/ingest/9756b6bb-934b-4f2a-9616-4fac9cf9b59f',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({location:'YydsScript.cs:435',message:'表单提交-捕获阶段',data:{phase:'capture',defaultPrevented:e.defaultPrevented,formValid:form.checkValidity(),formAction:form.action,formData:formData},timestamp:Date.now(),sessionId:'debug-session',hypothesisId:'D'})}).catch(()=>{});
                                }, true);
                                
                                // 冒泡阶段
                                form.addEventListener('submit', function(e) {
                                    fetch('http://127.0.0.1:7242/ingest/9756b6bb-934b-4f2a-9616-4fac9cf9b59f',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({location:'YydsScript.cs:440',message:'表单提交-冒泡阶段',data:{phase:'bubble',defaultPrevented:e.defaultPrevented,formValid:form.checkValidity()},timestamp:Date.now(),sessionId:'debug-session',hypothesisId:'D'})}).catch(()=>{});
                                }, false);
                                
                                // 🔥 劫持表单的submit方法
                                const originalSubmit = form.submit;
                                form.submit = function() {
                                    fetch('http://127.0.0.1:7242/ingest/9756b6bb-934b-4f2a-9616-4fac9cf9b59f',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({location:'YydsScript.cs:445',message:'表单submit方法被调用',data:{},timestamp:Date.now(),sessionId:'debug-session',hypothesisId:'D'})}).catch(()=>{});
                                    return originalSubmit.apply(this, arguments);
                                };
                            }
                            
                            // 🔥 注意：不再劫持fetch/XHR，因为会导致网站的验证码刷新等请求失败
                            
                            return {success: true, formExists: !!form};
                        } catch(e) {
                            return {error: e.message};
                        }
                    })();
                ";
                await _webView.CoreWebView2.ExecuteScriptAsync(checkFormMechanismScript);
                // #endregion
                
                // 4. 等待登录成功（监听页面跳转或Cookie变化）
                _logCallback("⏳ 等待登录完成（超时时间：60秒）...");
                
                waitCount = 0;
                while (waitCount < 600)  // 60秒超时（给用户充足的时间输入验证码）
                {
                    await Task.Delay(100);
                    waitCount++;
                    
                    // #region agent log
                    // 🔥 DEBUG: 每5秒检查一次字段状态（所有假设）
                    if (waitCount % 50 == 0)  // 每5秒
                    {
                        var checkCurrentStateScript = @"
                            (function() {
                                try {
                                    const usernameInput = document.querySelector('input[name=""username""]');
                                    const passwordInput = document.querySelector('input[name=""password""]');
                                    const codeInput = document.querySelector('input[name=""code""]');
                                    const loginButton = document.querySelector('button[type=""submit""]') ||
                                                       document.querySelector('input[type=""submit""]') ||
                                                       document.querySelector('button[class*=""login""]') ||
                                                       window.__yyds_login_button;
                                    const form = document.querySelector('form');
                                    
                                    const state = {
                                        usernameValue: usernameInput?.value || '',
                                        usernameLength: usernameInput?.value?.length || 0,
                                        passwordLength: passwordInput?.value?.length || 0,
                                        codeValue: codeInput?.value || '',
                                        codeLength: codeInput?.value?.length || 0,
                                        buttonDisabled: loginButton?.disabled,
                                        buttonClassName: loginButton?.className,
                                        formValid: form?.checkValidity ? form.checkValidity() : 'unknown',
                                        usernameValid: usernameInput?.checkValidity ? usernameInput.checkValidity() : 'unknown',
                                        passwordValid: passwordInput?.checkValidity ? passwordInput.checkValidity() : 'unknown',
                                        codeValid: codeInput?.checkValidity ? codeInput.checkValidity() : 'unknown',
                                        url: window.location.href
                                    };
                                    
                                    fetch('http://127.0.0.1:7242/ingest/9756b6bb-934b-4f2a-9616-4fac9cf9b59f',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({location:'YydsScript.cs:405',message:'定期状态检查',data:state,timestamp:Date.now(),sessionId:'debug-session',hypothesisId:'ALL'})}).catch(()=>{});
                                    
                                    return state;
                                } catch(e) {
                                    return {error: e.message};
                                }
                            })();
                        ";
                        await _webView.CoreWebView2.ExecuteScriptAsync(checkCurrentStateScript);
                    }
                    // #endregion
                    
                    // 检查是否已跳转离开登录页（登录成功的标志）
                    var checkLoginScript = @"
                        (function() {
                            // 检查URL是否已跳转
                            if (!window.location.href.includes('/login')) {
                                return { loggedIn: true, reason: 'URL已跳转' };
                            }
                            
                            // 检查是否有Session/Token Cookie
                            const cookies = document.cookie;
                            if (cookies.includes('session') || cookies.includes('token') || cookies.includes('PHPSESSID')) {
                                return { loggedIn: true, reason: 'Cookie已设置' };
                            }
                            
                            // 检查是否有登录成功的元素（例如用户信息显示）
                            const userInfo = document.querySelector('[class*=""user""]') ||
                                           document.querySelector('[class*=""profile""]') ||
                                           document.querySelector('[class*=""account""]');
                            
                            if (userInfo && !window.location.href.includes('/login')) {
                                return { loggedIn: true, reason: '找到用户信息元素' };
                            }
                            
                            return { loggedIn: false };
                        })();
                    ";
                    
                    var checkResult = await _webView.CoreWebView2.ExecuteScriptAsync(checkLoginScript);
                    var checkJson = JObject.Parse(checkResult);
                    
                    var loggedIn = checkJson["loggedIn"]?.Value<bool>() ?? false;
                    var reason = checkJson["reason"]?.ToString() ?? "";
                    
                    if (loggedIn)
                    {
                        _logCallback($"✅ 登录成功！原因: {reason}");
                        
                        // 提取Cookie中的Token/SessionId
                        await ExtractAuthInfoFromCookies();
                        
                        return true;
                    }
                }
                
                _logCallback("❌ 登录超时（60秒内未完成登录）");
                return false;
            }
            catch (Exception ex)
            {
                _logCallback($"❌ 登录失败: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 从Cookie中提取认证信息
        /// </summary>
        private async Task ExtractAuthInfoFromCookies()
        {
            try
            {
                var extractScript = @"
                    (function() {
                        const cookies = document.cookie.split(';').reduce((acc, cookie) => {
                            const [key, value] = cookie.trim().split('=');
                            acc[key] = value;
                            return acc;
                        }, {});
                        return cookies;
                    })();
                ";
                
                var result = await _webView.CoreWebView2.ExecuteScriptAsync(extractScript);
                var cookies = JObject.Parse(result);
                
                // 尝试提取常见的认证Cookie
                _token = cookies["token"]?.ToString() ?? 
                        cookies["auth_token"]?.ToString() ?? 
                        cookies["access_token"]?.ToString() ?? "";
                
                _sessionId = cookies["session"]?.ToString() ?? 
                            cookies["PHPSESSID"]?.ToString() ?? 
                            cookies["sessionid"]?.ToString() ?? "";
                
                if (!string.IsNullOrEmpty(_token))
                {
                    _logCallback($"✅ 提取到 Token: {_token.Substring(0, Math.Min(10, _token.Length))}...");
                }
                
                if (!string.IsNullOrEmpty(_sessionId))
                {
                    _logCallback($"✅ 提取到 SessionId: {_sessionId.Substring(0, Math.Min(10, _sessionId.Length))}...");
                }
            }
            catch (Exception ex)
            {
                _logCallback($"⚠️ 提取Cookie失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 获取余额
        /// </summary>
        public async Task<decimal> GetBalanceAsync()
        {
            try
            {
                // 方法1: 从页面DOM中提取余额
                var extractBalanceScript = @"
                    (function() {
                        try {
                            // 尝试多种常见的余额元素选择器
                            const balanceSelectors = [
                                '[class*=""balance""]',
                                '[class*=""money""]',
                                '[class*=""amount""]',
                                '[id*=""balance""]',
                                '[id*=""money""]'
                            ];
                            
                            for (const selector of balanceSelectors) {
                                const elements = document.querySelectorAll(selector);
                                for (const el of elements) {
                                    const text = el.textContent || el.innerText;
                                    // 匹配数字（支持小数和负数）
                                    const match = text.match(/[-]?\d+\.?\d*/);
                                    if (match) {
                                        const value = parseFloat(match[0]);
                                        if (!isNaN(value) && value >= 0) {
                                            return { success: true, balance: value, source: selector };
                                        }
                                    }
                                }
                            }
                            
                            return { success: false, message: '未找到余额信息' };
                        } catch (error) {
                            return { success: false, message: error.message };
                        }
                    })();
                ";
                
                var result = await _webView.CoreWebView2.ExecuteScriptAsync(extractBalanceScript);
                var json = JObject.Parse(result);
                
                var success = json["success"]?.Value<bool>() ?? false;
                
                if (success)
                {
                    _currentBalance = json["balance"]?.Value<decimal>() ?? 0;
                    var source = json["source"]?.ToString() ?? "";
                    _logCallback($"✅ 余额: {_currentBalance} (来源: {source})");
                    return _currentBalance;
                }
                else
                {
                    var message = json["message"]?.ToString() ?? "";
                    _logCallback($"⚠️ 获取余额失败: {message}");
                    return -1;
                }
            }
            catch (Exception ex)
            {
                _logCallback($"❌ 获取余额异常: {ex.Message}");
                return -1;
            }
        }
        
        /// <summary>
        /// 下注 - 需要根据实际平台API实现
        /// </summary>
        public async Task<(bool success, string orderId, string platformResponse)> PlaceBetAsync(BetStandardOrderList orders)
        {
            try
            {
                if (orders == null || orders.Count == 0)
                {
                    return (false, "", "❌ 订单列表为空");
                }
                
                _logCallback($"📤 准备投注: {orders.Count} 项");
                
                // TODO: 需要分析YYDS平台的投注API
                // 以下是通用的投注逻辑模板，需要根据实际API调整
                
                // 1. 检查是否已登录
                if (string.IsNullOrEmpty(_token) && string.IsNullOrEmpty(_sessionId))
                {
                    return (false, "", "#未登录，无法下注");
                }
                
                // 2. 检查余额
                var balance = await GetBalanceAsync();
                var totalAmount = orders.GetTotalAmount();
                
                if (balance >= 0 && balance < totalAmount)
                {
                    return (false, "", $"#余额不足（余额: {balance}，需要: {totalAmount}）");
                }
                
                // 3. 调用投注API（需要根据实际平台实现）
                // 这里提供一个模板，需要通过浏览器开发者工具分析实际API
                
                _logCallback("⚠️ YYDS 平台投注API尚未实现");
                _logCallback("   请联系开发者完成以下工作:");
                _logCallback("   1. 分析平台投注请求（URL、参数、Headers）");
                _logCallback("   2. 实现投注API调用");
                _logCallback("   3. 解析投注响应");
                
                return (false, "", "#投注功能尚未实现，请先分析平台API");
            }
            catch (Exception ex)
            {
                _logCallback($"❌ 投注失败: {ex.Message}");
                return (false, "", $"投注异常: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 处理响应 - 拦截网络请求，提取关键参数
        /// </summary>
        public void HandleResponse(BrowserResponseEventArgs response)
        {
            try
            {
                // 拦截登录响应
                if (response.Url.Contains("/login") || response.Url.Contains("/api/auth"))
                {
                    _logCallback($"📥 拦截登录响应: {response.Url}");
                    
                    try
                    {
                        var json = JObject.Parse(response.Context);
                        
                        // 尝试提取Token
                        _token = json["token"]?.ToString() ?? 
                                json["access_token"]?.ToString() ?? 
                                json["data"]?["token"]?.ToString() ?? "";
                        
                        if (!string.IsNullOrEmpty(_token))
                        {
                            _logCallback($"✅ 提取到 Token: {_token.Substring(0, Math.Min(10, _token.Length))}...");
                        }
                        
                        // 提取余额
                        var balance = json["balance"]?.ToString() ?? 
                                     json["data"]?["balance"]?.ToString() ?? "";
                        
                        if (!string.IsNullOrEmpty(balance) && decimal.TryParse(balance, out var balanceValue))
                        {
                            _currentBalance = balanceValue;
                            _logCallback($"✅ 余额: {_currentBalance}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logCallback($"⚠️ 解析登录响应失败: {ex.Message}");
                    }
                }
                
                // 拦截余额查询响应
                if (response.Url.Contains("/balance") || response.Url.Contains("/userinfo"))
                {
                    try
                    {
                        var json = JObject.Parse(response.Context);
                        
                        var balance = json["balance"]?.ToString() ?? 
                                     json["data"]?["balance"]?.ToString() ?? 
                                     json["amount"]?.ToString() ?? "";
                        
                        if (!string.IsNullOrEmpty(balance) && decimal.TryParse(balance, out var balanceValue))
                        {
                            _currentBalance = balanceValue;
                            _logCallback($"💰 余额更新: {_currentBalance}");
                        }
                    }
                    catch { }
                }
                
                // 拦截投注响应
                if (response.Url.Contains("/bet") || response.Url.Contains("/place"))
                {
                    _logCallback($"📥 拦截投注响应: {response.Url}");
                    _logCallback($"   响应: {response.Context}");
                }
                
                // 拦截赔率响应
                if (response.Url.Contains("/odds") || response.Url.Contains("/rates"))
                {
                    try
                    {
                        var json = JObject.Parse(response.Context);
                        _logCallback($"📊 拦截赔率响应: {json.ToString(Formatting.None)}");
                        
                        // TODO: 解析赔率列表并更新 _oddsMap 和 _oddsValues
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                _logCallback($"❌ 处理响应失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 获取赔率列表
        /// </summary>
        public List<BrowserOddsInfo> GetOddsList()
        {
            var oddsList = new List<BrowserOddsInfo>();
            
            // 根据 _oddsValues 生成赔率列表
            foreach (var kvp in _oddsValues)
            {
                oddsList.Add(new BrowserOddsInfo
                {
                    CarName = kvp.Key,   // 例如: "平一大"
                    Odds = kvp.Value     // 例如: 1.97
                });
            }
            
            return oddsList;
        }
    }
}

