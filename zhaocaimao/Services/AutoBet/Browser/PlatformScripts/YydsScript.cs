using Microsoft.Web.WebView2.WinForms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sunny.UI.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Unit.Shared.Helpers;  // 🔥 使用共享库中的 ModernHttpHelper
using zhaocaimao.Services.AutoBet.Browser.Models;
using zhaocaimao.Services.AutoBet.Browser.Services;
using Unit.Shared.Models;
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
        private readonly ModernHttpHelper _httpHelper;
        private List<BrowserOddsInfo> _OddsInfo = new List<BrowserOddsInfo>();

        // 关键参数（从拦截中获取或cookie中提取）
        private string _token = "";
        private string _sessionId = "";
        private decimal _currentBalance = 0;
        private string _baseUrl = "https://client.06n.yyds666.me";  // 登录域名
        private string _apiBaseUrl = "";  // API投注域名（从/info接口获取）
        private string _betPlate = "";  // 平台类型（A/B/C/D）
        
        // 赔率更新控制
        private bool _oddsLoaded = false;  // 赔率是否已加载
        private bool _autoUpdateOdds = true;  // 是否允许自动更新赔率
        
        // 🔥 调试日志目录（必须先初始化）
        private static readonly string _debugLogDirectory = GetDebugLogDirectory();
        
        // 🔥 调试日志路径（使用应用程序数据目录，按日期+启动时间命名）
        private static readonly string _debugLogPath = GetDebugLogPath();
        
        /// <summary>
        /// 获取调试日志目录
        /// </summary>
        private static string GetDebugLogDirectory()
        {
            try
            {
                var logDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "zhaocaimao",
                    "Logs",
                    "Debug");
                Directory.CreateDirectory(logDir);
                return logDir;
            }
            catch
            {
                // 如果创建目录失败，使用临时目录
                var tempDir = Path.Combine(Path.GetTempPath(), "zhaocaimao_debug");
                try
                {
                    Directory.CreateDirectory(tempDir);
                }
                catch { }
                return tempDir;
            }
        }
        
        /// <summary>
        /// 获取调试日志路径（按日期+启动时间命名）
        /// </summary>
        private static string GetDebugLogPath()
        {
            try
            {
                // 🔥 清理24小时前的旧日志（在首次调用时执行）
                CleanupOldDebugLogs();
                
                // 🔥 使用日期+启动时间命名：yyds_debug_2026-01-01_11-13-33.log
                var now = DateTime.Now;
                var fileName = $"yyds_debug_{now:yyyy-MM-dd}_{now:HH-mm-ss}.log";
                return Path.Combine(_debugLogDirectory, fileName);
            }
            catch
            {
                // 如果创建目录失败，使用临时目录
                var tempDir = Path.Combine(Path.GetTempPath(), "zhaocaimao_debug");
                try
                {
                    Directory.CreateDirectory(tempDir);
                }
                catch { }
                var now = DateTime.Now;
                var fileName = $"yyds_debug_{now:yyyy-MM-dd}_{now:HH-mm-ss}.log";
                return Path.Combine(tempDir, fileName);
            }
        }
        
        /// <summary>
        /// 清理24小时前的调试日志文件
        /// </summary>
        private static void CleanupOldDebugLogs()
        {
            try
            {
                if (!Directory.Exists(_debugLogDirectory))
                    return;
                
                var cutoffTime = DateTime.Now.AddHours(-24);
                var files = Directory.GetFiles(_debugLogDirectory, "yyds_debug_*.log");
                int deletedCount = 0;
                
                foreach (var file in files)
                {
                    try
                    {
                        var fileName = Path.GetFileNameWithoutExtension(file);
                        // 🔥 从文件名解析日期时间：yyds_debug_2026-01-01_11-13-33
                        // 格式：yyds_debug_yyyy-MM-dd_HH-mm-ss
                        if (fileName.StartsWith("yyds_debug_") && fileName.Length >= 25)
                        {
                            var dateTimeStr = fileName.Substring(11); // 跳过 "yyds_debug_"
                            if (DateTime.TryParseExact(dateTimeStr, "yyyy-MM-dd_HH-mm-ss", 
                                System.Globalization.CultureInfo.InvariantCulture, 
                                System.Globalization.DateTimeStyles.None, out var fileDateTime))
                            {
                                if (fileDateTime < cutoffTime)
                                {
                                    File.Delete(file);
                                    deletedCount++;
                                }
                            }
                            else
                            {
                                // 如果无法解析文件名，使用文件时间作为后备方案
                                var fileInfo = new FileInfo(file);
                                var fileTime = fileInfo.LastWriteTime < fileInfo.CreationTime 
                                    ? fileInfo.LastWriteTime 
                                    : fileInfo.CreationTime;
                                
                                if (fileTime < cutoffTime)
                                {
                                    File.Delete(file);
                                    deletedCount++;
                                }
                            }
                        }
                        else
                        {
                            // 文件名格式不正确，使用文件时间作为后备方案
                            var fileInfo = new FileInfo(file);
                            var fileTime = fileInfo.LastWriteTime < fileInfo.CreationTime 
                                ? fileInfo.LastWriteTime 
                                : fileInfo.CreationTime;
                            
                            if (fileTime < cutoffTime)
                            {
                                File.Delete(file);
                                deletedCount++;
                            }
                        }
                    }
                    catch
                    {
                        // 忽略单个文件删除失败
                    }
                }
                
                if (deletedCount > 0)
                {
                    // 使用静态方法写入日志，但这里可能还没有初始化，所以只记录到系统日志
                    System.Diagnostics.Debug.WriteLine($"[YydsScript] 已清理 {deletedCount} 个24小时前的调试日志文件");
                }
            }
            catch
            {
                // 忽略清理失败，避免影响主流程
            }
        }
        
        /// <summary>
        /// 安全地写入调试日志（带异常处理）
        /// </summary>
        private static void WriteDebugLog(object logData)
        {
            try
            {
                var json = JsonConvert.SerializeObject(logData);
                File.AppendAllText(_debugLogPath, json + "\n");
            }
            catch
            {
                // 忽略写入失败，避免影响主流程
            }
        }
        
        // 赔率ID映射表
        //private readonly Dictionary<string, string> _oddsMap = new Dictionary<string, string>();
        
        // 赔率值映射表
        //private readonly Dictionary<string, float> _oddsValues = new Dictionary<string, float>();
        
        public YydsScript(WebView2 webView, Action<string> logCallback)
        {
            _webView = webView;
            _logCallback = logCallback;
            
            // 🔥 配置 HttpClient 全局默认请求头（所有请求都会携带）
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json, text/javascript, */*; q=0.01");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36");
            
            // 💡 可选：添加更多全局请求头
            // _httpClient.DefaultRequestHeaders.Add("Accept-Language", "zh-CN,zh;q=0.9,en;q=0.8");
            // _httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
            
            // 📝 注意：
            // 1. Authorization 请求头在登录成功后动态添加（HandleResponse 中）
            // 2. Content-Type 由 StringContent/ByteArrayContent 自动设置
            // 3. 单个请求特定的请求头使用 HttpRequestMessage.Headers.Add()
            
            // 🎯 初始化 ModernHttpHelper（复用 HttpClient 连接池）
            _httpHelper = new ModernHttpHelper(_httpClient);
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
                        //await ExtractAuthInfoFromCookies();
                        
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
        //private async Task ExtractAuthInfoFromCookies()
        //{
        //    try
        //    {
        //        var extractScript = @"
        //            (function() {
        //                const cookies = document.cookie.split(';').reduce((acc, cookie) => {
        //                    const [key, value] = cookie.trim().split('=');
        //                    acc[key] = value;
        //                    return acc;
        //                }, {});
        //                return cookies;
        //            })();
        //        ";
                
        //        var result = await _webView.CoreWebView2.ExecuteScriptAsync(extractScript);
        //        var cookies = JObject.Parse(result);
                
        //        // 🔥 修复：只有在 Cookie 中找到 Token 时才更新，避免覆盖已有的 Token
        //        var cookieToken = cookies["token"]?.ToString() ?? 
        //                         cookies["auth_token"]?.ToString() ?? 
        //                         cookies["access_token"]?.ToString() ?? "";
                
        //        if (!string.IsNullOrEmpty(cookieToken))
        //        {
        //            _token = cookieToken;
        //            _logCallback($"✅ 从Cookie提取到 Token: {_token.Substring(0, Math.Min(10, _token.Length))}...");
        //        }
        //        else
        //        {
        //            _logCallback($"ℹ️ Cookie中没有Token，保留现有Token (长度: {_token?.Length ?? 0})");
        //        }
                
        //        // 提取 SessionId
        //        var cookieSessionId = cookies["session"]?.ToString() ?? 
        //                             cookies["PHPSESSID"]?.ToString() ?? 
        //                             cookies["sessionid"]?.ToString() ?? "";
                
        //        if (!string.IsNullOrEmpty(cookieSessionId))
        //        {
        //            _sessionId = cookieSessionId;
        //            _logCallback($"✅ 从Cookie提取到 SessionId: {_sessionId.Substring(0, Math.Min(10, _sessionId.Length))}...");
        //        }
        //        else
        //        {
        //            _logCallback($"ℹ️ Cookie中没有SessionId");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logCallback($"⚠️ 提取Cookie失败: {ex.Message}");
        //    }
        //}
        
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
                
                // 🔥 调试日志：显示实例和Token状态
                _logCallback($"🔍 [DEBUG] YydsScript实例: {this.GetHashCode()}");
                _logCallback($"🔍 [DEBUG] Token状态: {(_token != null ? $"长度={_token.Length}" : "null")}");
                _logCallback($"🔍 [DEBUG] Token是否为空: {string.IsNullOrEmpty(_token)}");
                _logCallback($"🔍 [DEBUG] SessionId是否为空: {string.IsNullOrEmpty(_sessionId)}");
                
                if (!string.IsNullOrEmpty(_token))
                {
                    _logCallback($"🔍 [DEBUG] Token前20位: {_token.Substring(0, Math.Min(20, _token.Length))}...");
                }
                
                // TODO: 需要分析YYDS平台的投注API
                // 以下是通用的投注逻辑模板，需要根据实际API调整
                
                // 1. 检查是否已登录
                if (string.IsNullOrEmpty(_token) && string.IsNullOrEmpty(_sessionId))
                {
                    _logCallback($"❌ 登录检查失败: Token和SessionId都为空");
                    return (false, "", "#未登录，无法下注");
                }

                // 🔥 检查 API 域名是否已设置
                if (string.IsNullOrEmpty(_apiBaseUrl))
                {
                    // 如果未设置，尝试从登录域名推断（通常 API 域名和登录域名在同一域名下）
                    // 例如：登录域名 https://client.06n.yyds666.me，API 域名可能是 https://admin-api.06n.yyds666.me
                    if (!string.IsNullOrEmpty(_baseUrl))
                    {
                        try
                        {
                            var baseUri = new Uri(_baseUrl);
                            // 尝试将 client 替换为 admin-api
                            var host = baseUri.Host;
                            if (host.Contains("client"))
                            {
                                _apiBaseUrl = baseUri.Scheme + "://" + host.Replace("client", "admin-api");
                            }
                            else
                            {
                                // 如果无法推断，使用默认的 API 域名格式
                                _apiBaseUrl = "https://admin-api.06n.yyds666.me";
                            }
                            _logCallback($"⚠️ API域名未设置，已推断为: {_apiBaseUrl}");
                        }
                        catch
                        {
                            // 如果推断失败，使用默认值
                            _apiBaseUrl = "https://admin-api.06n.yyds666.me";
                            _logCallback($"⚠️ API域名未设置，已使用默认值: {_apiBaseUrl}");
                        }
                    }
                    else
                    {
                        // 如果连登录域名都没有，使用默认值
                        _apiBaseUrl = "https://admin-api.06n.yyds666.me";
                        _logCallback($"⚠️ API域名未设置，已使用默认值: {_apiBaseUrl}");
                    }
                }

                // 2. 检查余额
                //var balance = await GetBalanceAsync();
                //var totalAmount = orders.GetTotalAmount();

                //if (balance >= 0 && balance < totalAmount)
                //{
                //    return (false, "", $"#余额不足（余额: {balance}，需要: {totalAmount}）");
                //}

                // 3. 调用投注API（需要根据实际平台实现）
                // 这里提供一个模板，需要通过浏览器开发者工具分析实际API

                //_logCallback("⚠️ YYDS 平台投注API尚未实现");
                //_logCallback("   请联系开发者完成以下工作:");
                //_logCallback("   1. 分析平台投注请求（URL、参数、Headers）");
                //_logCallback("   2. 实现投注API调用");
                //_logCallback("   3. 解析投注响应");

                //合成数据包
                /*
                 *          * 投注
            {"totalAmount":20,
              "gameId":1,
              "periodNo":114069971,
              "addBodyList":[{"betTypeId":5,"dictValue":"DA","dictLabel":"大","amount":10},
                             {"betTypeId":5,"dictValue":"XIAO","dictLabel":"小","amount":10}
                             ]
             }

            //未解析的
                {"totalAmount":20,"gameId":1,"periodNo":114070279,"addBodyList":[{"betTypeId":5,"dictValue":"DA","dictLabel":"大","amount":10},{"betTypeId":5,"dictValue":"XIAO","dictLabel":"小","amount":10}]}
         */
                var issueId = orders.Count > 0 ? orders[0].IssueId : 0;

                List<object> postitems = new List<object>();
                foreach(var order in orders)
                {
                    // 🔥 YYDS平台特殊处理：P总的单/双 需要转换为 合单/合双
                    var actualPlay = order.Play;
                    if (order.Car == CarNumEnum.P总)
                    {
                        if (order.Play == BetPlayEnum.单)
                        {
                            actualPlay = BetPlayEnum.合单;
                            _logCallback($"🔄 自动转换: P总单 → P总合单");
                        }
                        else if (order.Play == BetPlayEnum.双)
                        {
                            actualPlay = BetPlayEnum.合双;
                            _logCallback($"🔄 自动转换: P总双 → P总合双");
                        }
                    }
                    
                    var oddsInfo = _OddsInfo.FirstOrDefault(o => o.Play == actualPlay && o.Car == order.Car);
                    
                    // 🔥 检查赔率信息是否存在
                    if (oddsInfo == null)
                    {
                        _logCallback($"❌ 未找到赔率信息: Play={order.Play}, Car={order.Car}");
                        _logCallback($"   可用赔率列表: {string.Join(", ", _OddsInfo.Select(o => $"{o.Car}-{o.Play}"))}");
                        return (false, "", $"#未找到赔率信息: {order.Play}-{order.Car}");
                    }
                    
                    // 🔥 检查 CarName 是否存在且格式正确
                    if (string.IsNullOrEmpty(oddsInfo.CarName) || !oddsInfo.CarName.Contains("|"))
                    {
                        _logCallback($"❌ 赔率信息格式错误: CarName={oddsInfo.CarName}");
                        return (false, "", $"#赔率信息格式错误: {oddsInfo.CarName}");
                    }
                    
                    string[] param = oddsInfo.CarName.Split('|');
                    if (param.Length < 2)
                    {
                        _logCallback($"❌ 赔率信息格式错误: CarName={oddsInfo.CarName}，无法分割");
                        return (false, "", $"#赔率信息格式错误: {oddsInfo.CarName}");
                    }
                    
                    // 🔥 调试日志：显示投注项详细信息（同时写入调试日志文件）
                    var debugInfo = $"📋 构建投注项:\n" +
                                   $"   - 原始订单: Car={order.Car}, Play={order.Play}, Amount={order.MoneySum}\n" +
                                   $"   - 实际玩法: Play={actualPlay}" + (actualPlay != order.Play ? " (已转换)" : "") + "\n" +
                                   $"   - 赔率: CarName={oddsInfo.CarName}, OddsId={oddsInfo.OddsId}\n" +
                                   $"   - 分割: dictLabel={param[0]}, dictValue={param[1]}";
                    _logCallback(debugInfo);
                    WriteDebugLog(new { 
                        location = "PlaceBetAsync", 
                        message = "构建投注项", 
                        data = new { 
                            car = order.Car.ToString(), 
                            originalPlay = order.Play.ToString(),
                            actualPlay = actualPlay.ToString(),
                            amount = order.MoneySum,
                            carName = oddsInfo.CarName,
                            oddsId = oddsInfo.OddsId,
                            dictLabel = param[0],
                            dictValue = param[1]
                        },
                        timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                    });
                    
                    // 🔥 检查 betTypeId 是否为数字（OddsId 是字符串，需要转换为 int）
                    if (!int.TryParse(oddsInfo.OddsId, out var betTypeIdInt))
                    {
                        var errorMsg = $"❌ betTypeId 格式错误: {oddsInfo.OddsId}，无法转换为整数";
                        _logCallback(errorMsg);
                        WriteDebugLog(new { 
                            location = "PlaceBetAsync", 
                            message = "betTypeId格式错误", 
                            error = errorMsg,
                            oddsId = oddsInfo.OddsId,
                            timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                        });
                        return (false, "", $"#betTypeId 格式错误: {oddsInfo.OddsId}");
                    }
                    
                    // 根据订单信息构建投注项
                    var betItem = new
                    {
                        betTypeId = betTypeIdInt,  // 🔥 转换为整数
                        dictValue = param[1],      // 例如: "DA"
                        dictLabel = param[0],      // 例如: "大"
                        amount = order.MoneySum
                    };
                    
                    var finalInfo = $"   - 最终投注项: betTypeId={betItem.betTypeId}, dictLabel={betItem.dictLabel}, dictValue={betItem.dictValue}, amount={betItem.amount}";
                    _logCallback(finalInfo);
                    WriteDebugLog(new { 
                        location = "PlaceBetAsync", 
                        message = "最终投注项", 
                        data = new { 
                            betTypeId = betItem.betTypeId,
                            dictLabel = betItem.dictLabel,
                            dictValue = betItem.dictValue,
                            amount = betItem.amount
                        },
                        timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                    });

                    postitems.Add(betItem);
                }

                dynamic postData = new ExpandoObject(); 
                postData.totalAmount = orders.GetTotalAmount();
                postData.gameId = 1; 
                postData.periodNo = issueId;
                postData.addBodyList = postitems;

                string postdata = JsonConvert.SerializeObject(postData);

                _logCallback($"📤 投注请求数据:");
                _logCallback($"   URL: {_apiBaseUrl}/system/betOrder/pc_user/order_add");
                _logCallback($"   Body: {postdata}");
                _logCallback($"   Token: {(!string.IsNullOrEmpty(_token) ? _token.Substring(0, Math.Min(20, _token.Length)) + "..." : "未设置")}");
                _logCallback($"   Authorization头: Bearer {(!string.IsNullOrEmpty(_token) ? _token.Substring(0, Math.Min(20, _token.Length)) + "..." : "未设置")}");

                // 🎯 使用 ModernHttpHelper 发送请求（完全匹配抓包数据）
                var result = await _httpHelper.PostAsync(new HttpRequestItem
                {
                    Url = $"{_apiBaseUrl}/system/betOrder/pc_user/order_add",
                    PostData = postdata,
                    ContentType = "application/json",
                    Timeout = 10,  // 🔥 设置超时时间（秒），超过10秒自动返回
                    Headers = new[]
                    {
                        // 🔥 关键：Authorization 必须加 "Bearer " 前缀
                        $"Authorization: Bearer {_token}",
                        
                        // 🔥 关键：referer 头（认证时可能检查来源）
                        $"referer: {_baseUrl}",    //https://client.06n.yyds666.me/",
                        
                        // 🔥 关键：sec-fetch-* 系列头（CORS 安全相关）
                        "sec-fetch-dest: empty",
                        "sec-fetch-mode: cors",
                        "sec-fetch-site: same-site",
                        
                        // sec-ch-ua 系列
                        "sec-ch-ua: \"Microsoft Edge WebView2\";v=\"143\", \"Microsoft Edge\";v=\"143\", \"Chromium\";v=\"143\", \"Not A(Brand\";v=\"24\"",
                        "sec-ch-ua-mobile: ?0",
                        "sec-ch-ua-platform: \"Windows\"",
                        
                        // 其他必要头
                        "accept-language: zh-CN,zh;q=0.9,en;q=0.8,en-GB;q=0.7,en-US;q=0.6",
                        "priority: u=1, i",
                        $"origin: {_baseUrl}",//https://client.06n.yyds666.me",
                        "datasource: master"
                    }
                });

                _logCallback($"📥 投注响应:");
                _logCallback($"   状态码: {result.StatusCode} {result.StatusDescription}");
                _logCallback($"   响应内容: {result.Html}");

                // 4. 解析响应
                if (!result.Success)
                {
                    // 🔥 检查是否是超时错误
                    if (result.ErrorMessage?.Contains("超时") == true || result.ErrorMessage?.Contains("取消") == true)
                    {
                        _logCallback($"⏱️ 投注请求超时: {result.ErrorMessage}");
                        return (false, "", $"#投注超时: {result.ErrorMessage}");
                    }
                    
                    _logCallback($"❌ 请求失败: {result.ErrorMessage}");
                    return (false, "", result.ErrorMessage ?? "请求失败");
                }

                var responseJson = JObject.Parse(result.Html);
                var code = responseJson["code"]?.Value<int>() ?? 0;
                var msg = responseJson["msg"]?.ToString() ?? "";

                if (code == 200)
                {
                    _logCallback($"✅ 投注成功: {msg}");
                    // TODO: 提取订单号等信息
                    return (true, "", msg);
                }
                else
                {
                    _logCallback($"❌ 投注失败: code={code}, msg={msg}");
                    return (false, "", $"{msg} (code:{code})");
                }
            }
            catch (Exception ex)
            {
                _logCallback($"❌ 投注失败: {ex.Message}");
                return (false, "", $"投注异常: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 解析赔率配置信息
        /// </summary>
        private void ParseOddsInfo(JObject json)
        {
            try
            {
                var betTypes = json["data"]?["game"]?["playType"]?["betTypes"] as JArray;
                if (betTypes == null || betTypes.Count == 0)
                {
                    _logCallback("⚠️ 未找到 betTypes 数据");
                    return;
                }
                
                // 🔥 不再清空列表，改为检查并更新已存在的赔率项
                // _logCallback($"📊 开始解析 {betTypes.Count} 个投注类型（当前已有 {_OddsInfo.Count} 个赔率）");
                
                int addedCount = 0;
                int updatedCount = 0;
                int skippedCount = 0;
                
                foreach (var betType in betTypes)
                {
                    try
                    {
                        var betTypeId = betType["betTypeId"]?.Value<int>() ?? 0;
                        var betTypeCname = betType["betTypeCname"]?.ToString() ?? "";
                        var betTypeGroup = betType["betTypeGroup"];
                        var betTypeGroupName = betTypeGroup?["betTypeGroupName"]?.ToString() ?? "";
                        
                        if (string.IsNullOrEmpty(betTypeCname) || string.IsNullOrEmpty(betTypeGroupName))
                        {
                            skippedCount++;
                            continue;
                        }
                        
                        // 🔥 根据 betTypeGroupName 映射 Car（位置）
                        CarNumEnum car = MapBetTypeGroupToCar(betTypeGroupName);
                        if (car == CarNumEnum.未知)
                        {
                            // _logCallback($"⚠️ 未知的投注组: {betTypeGroupName}");
                            skippedCount++;
                            continue;
                        }
                        
                        // 🔥 根据 betTypeCname 构建赔率信息
                        var oddsInfoList = BuildOddsInfoFromBetType(betTypeCname, car, betTypeId);
                        
                        foreach (var oddsInfo in oddsInfoList)
                        {
                            // 🔥 检查是否已存在相同的赔率项（根据 Car + Play 判断唯一性）
                            var existingOdds = _OddsInfo.FirstOrDefault(o => 
                                o.Car == oddsInfo.Car && 
                                o.Play == oddsInfo.Play);
                            
                            if (existingOdds != null)
                            {
                                // 更新现有赔率（包括 OddsId 和 Odds）
                                existingOdds.OddsId = oddsInfo.OddsId;
                                existingOdds.Odds = oddsInfo.Odds;
                                existingOdds.CarName = oddsInfo.CarName;  // 同时更新 CarName（投注用）
                                // _logCallback($"🔄 更新赔率: {oddsInfo.CarName} (ID:{oddsInfo.OddsId})");
                                updatedCount++;
                            }
                            else
                            {
                                // 添加新赔率
                                _OddsInfo.Add(oddsInfo);
                                // _logCallback($"✅ 添加赔率: {oddsInfo.CarName} (ID:{oddsInfo.OddsId})");
                                addedCount++;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logCallback($"❌ 解析单个 betType 失败: {ex.Message}");
                    }
                }
                
                // 🔥 只输出汇总日志，避免频繁刷新UI
                _logCallback($"🎯 赔率解析完成: 新增 {addedCount} 项, 更新 {updatedCount} 项, 跳过 {skippedCount} 项, 共 {_OddsInfo.Count} 个赔率");
            }
            catch (Exception ex)
            {
                _logCallback($"❌ 解析赔率配置失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 映射投注组名称到 Car 枚举
        /// </summary>
        private CarNumEnum MapBetTypeGroupToCar(string betTypeGroupName)
        {
            return betTypeGroupName switch
            {
                "平码一" => CarNumEnum.P1,
                "平码二" => CarNumEnum.P2,
                "平码三" => CarNumEnum.P3,
                "平码四" => CarNumEnum.P4,
                "特码" => CarNumEnum.P5,
                "合值" => CarNumEnum.P总,  // 合值归类到 P5
                "龙虎" => CarNumEnum.P总,  // 龙虎归类到 P5
                _ => CarNumEnum.未知
            };
        }
        
        /// <summary>
        /// 根据投注类型构建赔率信息列表
        /// </summary>
        private List<BrowserOddsInfo> BuildOddsInfoFromBetType(string betTypeCname, CarNumEnum car, int betTypeId)
        {
            var result = new List<BrowserOddsInfo>();
            
            switch (betTypeCname)
            {
                case "大小":
                    result.Add(new BrowserOddsInfo
                    {
                        Car = car,
                        Play = BetPlayEnum.大,
                        CarName = "大|DA",
                        OddsId = betTypeId.ToString(),
                        Odds = 1.97f  // 默认值
                    });
                    result.Add(new BrowserOddsInfo
                    {
                        Car = car,
                        Play = BetPlayEnum.小,
                        CarName = "小|XIAO",
                        OddsId = betTypeId.ToString(),
                        Odds = 1.97f
                    });
                    break;
                    
                case "单双":
                    result.Add(new BrowserOddsInfo
                    {
                        Car = car,
                        Play = BetPlayEnum.单,
                        CarName = "单|DAN",
                        OddsId = betTypeId.ToString(),
                        Odds = 1.97f
                    });
                    result.Add(new BrowserOddsInfo
                    {
                        Car = car,
                        Play = BetPlayEnum.双,
                        CarName = "双|SHUANG",
                        OddsId = betTypeId.ToString(),
                        Odds = 1.97f
                    });
                    break;
                    
                case "尾大尾小":
                    result.Add(new BrowserOddsInfo
                    {
                        Car = car,
                        Play = BetPlayEnum.尾大,
                        CarName = "尾大|WEI_DA",
                        OddsId = betTypeId.ToString(),
                        Odds = 1.97f
                    });
                    result.Add(new BrowserOddsInfo
                    {
                        Car = car,
                        Play = BetPlayEnum.尾小,
                        CarName = "尾小|WEI_XIAO",
                        OddsId = betTypeId.ToString(),
                        Odds = 1.97f
                    });
                    break;
                    
                case "合单合双":
                    result.Add(new BrowserOddsInfo
                    {
                        Car = car,
                        Play = BetPlayEnum.合单,
                        CarName = "合单|HE_DAN",
                        OddsId = betTypeId.ToString(),
                        Odds = 1.97f
                    });
                    result.Add(new BrowserOddsInfo
                    {
                        Car = car,
                        Play = BetPlayEnum.合双,
                        CarName = "合双|HE_SHUANG",
                        OddsId = betTypeId.ToString(),
                        Odds = 1.97f
                    });
                    break;
                    
                case "合值大合值小":
                    // 合值大合值小：使用大小枚举（因为枚举中没有单独的合值大小）
                    // 🔥 注意：对于 betTypeGroupName = "合值" 的类型，dictValue 格式应该是 "HEZHI_DA" 而不是 "HE-ZHI-DA"
                    result.Add(new BrowserOddsInfo
                    {
                        Car = car,
                        Play = BetPlayEnum.大,  // 暂时映射到大
                        CarName = "合值大|HEZHI_DA",
                        OddsId = betTypeId.ToString(),
                        Odds = 1.97f
                    });
                    result.Add(new BrowserOddsInfo
                    {
                        Car = car,
                        Play = BetPlayEnum.小,  // 暂时映射到小
                        CarName = "合值小|HEZHI_XIAO",
                        OddsId = betTypeId.ToString(),
                        Odds = 1.97f
                    });
                    break;
                    
                case "合值单合值双":
                    // 合值单合值双：使用合单合双枚举
                    // 🔥 注意：对于 betTypeGroupName = "合值" 的类型，dictValue 格式应该是 "HEZHI_DAN" 而不是 "HE-ZHI-DAN"
                    result.Add(new BrowserOddsInfo
                    {
                        Car = car,
                        Play = BetPlayEnum.合单,
                        CarName = "合值单|HEZHI_DAN",
                        OddsId = betTypeId.ToString(),
                        Odds = 1.97f
                    });
                    result.Add(new BrowserOddsInfo
                    {
                        Car = car,
                        Play = BetPlayEnum.合双,
                        CarName = "合值双|HEZHI_SHUANG",
                        OddsId = betTypeId.ToString(),
                        Odds = 1.97f
                    });
                    break;
                    
                case "龙虎":
                    result.Add(new BrowserOddsInfo
                    {
                        Car = car,
                        Play = BetPlayEnum.龙,
                        CarName = "龙|LONG",
                        OddsId = betTypeId.ToString(),
                        Odds = 1.97f
                    });
                    result.Add(new BrowserOddsInfo
                    {
                        Car = car,
                        Play = BetPlayEnum.虎,
                        CarName = "虎|HU",
                        OddsId = betTypeId.ToString(),
                        Odds = 1.97f
                    });
                    break;
                    
                default:
                    _logCallback($"⚠️ 未处理的投注类型: {betTypeCname}");
                    break;
            }
            
            return result;
        }
        
        /// <summary>
        /// 处理响应 - 拦截网络请求，提取关键参数
        /// </summary>
        public void HandleResponse(BrowserResponseEventArgs response)
        {
            try
            {
                // 🔥 调试：记录所有拦截到的响应（仅登录相关）
                if (response.Url.Contains("/login") || response.Url.Contains("/auth"))
                {
                    _logCallback($"🔍 [DEBUG] 拦截到响应:");
                    _logCallback($"   - URL: {response.Url}");
                    _logCallback($"   - Method: {response.Method}");
                    _logCallback($"   - Status: {response.StatusCode}");
                    _logCallback($"   - 实例哈希: {this.GetHashCode()}");
                }
                
                // 拦截登录响应
                // 🔥 YYDS平台登录接口: https://admin-api.06n.yyds666.me/login
                // 🔥 修改：使用更宽松的匹配条件，匹配所有包含 /login 的 POST 请求
                if (response.Url.Contains("/login") || response.Url.Contains("/api/auth"))
                {
                    _logCallback($"🔍 [LOGIN-CHECK] URL匹配成功: {response.Url}");
                    _logCallback($"🔍 [LOGIN-CHECK] Method: {response.Method}");
                    _logCallback($"🔍 [LOGIN-CHECK] StatusCode: {response.StatusCode}");
                    _logCallback($"🔍 [LOGIN-CHECK] ContentType: {response.ContentType}");
                    
                    // 🔥 判断请求方法，跳过 OPTIONS 预检请求
                    if (response.Method == "OPTIONS")
                    {
                        _logCallback($"⏭️ [LOGIN-CHECK] 跳过 OPTIONS 预检请求");
                        return;
                    }
                    
                    // 🔥 只处理 POST 请求的响应
                    if (response.Method != "POST")
                    {
                        _logCallback($"⚠️ [LOGIN-CHECK] 非 POST 请求 (Method={response.Method})，跳过处理");
                        return;
                    }
                    
                    // 🔥 只处理 JSON 响应，避免处理图片、CSS 等资源
                    if (!string.IsNullOrEmpty(response.ContentType) && 
                        !response.ContentType.Contains("application/json") && 
                        !response.ContentType.Contains("text/plain"))
                    {
                        _logCallback($"⚠️ [LOGIN-CHECK] 非 JSON 响应 (ContentType={response.ContentType})，跳过处理");
                        return;
                    }
                    
                    _logCallback($"📥 拦截登录响应: {response.Url} [{response.Method}]");
                    
                    _logCallback($"✅ [LOGIN-CHECK] 是 POST 请求，开始解析响应");
                    
                    try
                    {
                        _logCallback($"🔍 [LOGIN-CHECK] 响应内容长度: {response.Context?.Length ?? 0}");
                        _logCallback($"🔍 [LOGIN-CHECK] 响应内容前100字符: {response.Context?.Substring(0, Math.Min(100, response.Context?.Length ?? 0))}");
                        
                        var json = JObject.Parse(response.Context);
                        var code = json["code"]?.Value<int>() ?? 0;
                        
                        _logCallback($"🔍 [LOGIN-CHECK] 响应代码: {code}");
                        
                        if (code == 200)
                        {
                            _logCallback($"✅ [LOGIN-CHECK] 登录成功，开始提取 Token");
                            
                            // 🔥 YYDS平台格式: { "code": 200, "data": { "token": "..." } }
                            var dataObj = json["data"];
                            _logCallback($"🔍 [LOGIN-CHECK] data 对象: {(dataObj != null ? "存在" : "null")}");
                            
                            if (dataObj != null)
                            {
                                var tokenObj = dataObj["token"];
                                _logCallback($"🔍 [LOGIN-CHECK] token 对象: {(tokenObj != null ? "存在" : "null")}");
                                
                                var newToken = tokenObj?.ToString() ?? "";
                                
                                // 🔥 关键修复：只有新 Token 不为空时才更新，避免意外清空
                                if (!string.IsNullOrEmpty(newToken))
                                {
                                    _token = newToken;
                                    _logCallback($"🔍 [LOGIN-CHECK] Token赋值后: Length={_token.Length}");
                                }
                                else
                                {
                                    _logCallback($"⚠️ [LOGIN-CHECK] 新 Token 为空，保留现有 Token (当前长度: {_token?.Length ?? 0})");
                                }
                            }
                            else
                            {
                                // 🔥 关键修复：不清空 Token，只记录警告
                                _logCallback($"⚠️ [LOGIN-CHECK] data 对象为 null，保留现有 Token (当前长度: {_token?.Length ?? 0})");
                            }
                            
                            // 🔥 调试日志：显示实例哈希码，确认是同一个实例
                            _logCallback($"🔍 [DEBUG] YydsScript实例: {this.GetHashCode()}");
                            _logCallback($"🔍 [DEBUG] _token字段地址: {System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(_token)}");
                            
                            if (!string.IsNullOrEmpty(_token))
                            {
                                // 🔥 将 Token 添加到 HttpClient 的请求头中
                                if (_httpClient.DefaultRequestHeaders.Contains("Authorization"))
                                {
                                    _httpClient.DefaultRequestHeaders.Remove("Authorization");
                                }
                                _httpClient.DefaultRequestHeaders.Add("Authorization", _token);
                                
                                _logCallback($"✅ 提取到 Token: {_token.Substring(0, Math.Min(20, _token.Length))}...");
                                _logCallback($"✅ Token已添加到请求头 (完整Token长度: {_token.Length})");
                                _logCallback($"✅ [LOGIN-CHECK] Token 提取和设置完成！");
                            }
                            else
                            {
                                _logCallback($"❌ [LOGIN-CHECK] Token 为空！");
                                _logCallback($"⚠️ 响应结构: {json.ToString(Formatting.None).Substring(0, Math.Min(300, json.ToString(Formatting.None).Length))}");
                            }
                        }
                        else
                        {
                            var msg = json["msg"]?.ToString() ?? "未知错误";
                            _logCallback($"❌ [LOGIN-CHECK] 登录失败: code={code}, msg={msg}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logCallback($"❌ [LOGIN-CHECK] 解析登录响应异常: {ex.Message}");
                        _logCallback($"   StackTrace: {ex.StackTrace?.Substring(0, Math.Min(200, ex.StackTrace?.Length ?? 0))}");
                        _logCallback($"   响应内容: {response.Context?.Substring(0, Math.Min(200, response.Context?.Length ?? 0))}");
                    }
                }
                
                // 拦截余额查询响应
                // 🔥 YYDS平台：拦截 /info 接口（无参数），提取 availableCredit 和 betPlate
                // ⚠️ 注意：/game/game/pc_user/info?gameId=1&playTypeId=1 这样带参数的不是余额接口
                if (response.Url.EndsWith("/info") && !response.Url.Contains("?"))
                {
                    // #region agent log
                    WriteDebugLog(new { sessionId = "debug-session", runId = "run1", hypothesisId = "A", location = "YydsScript.cs:1025", message = "拦截到/info接口", data = new { url = response.Url, hasQueryString = response.Url.Contains("?") }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() });
                    // #endregion
                    
                    try
                    {
                        var json = JObject.Parse(response.Context);
                        var code = json["code"]?.Value<int>() ?? 0;
                        
                        // #region agent log
                        WriteDebugLog(new { sessionId = "debug-session", runId = "run1", hypothesisId = "B", location = "YydsScript.cs:1032", message = "解析/info响应", data = new { code = code, hasData = json["data"] != null, hasUser = json["data"]?["user"] != null, availableCredit = json["data"]?["user"]?["availableCredit"]?.ToString(), betPlate = json["data"]?["user"]?["betPlate"]?.ToString() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() });
                        // #endregion
                        
                        if (code == 200)
                        {
                            // 提取余额: data.user.availableCredit
                            var availableCredit = json["data"]?["user"]?["availableCredit"]?.ToString() ?? "";
                            
                            // #region agent log
                            WriteDebugLog(new { sessionId = "debug-session", runId = "run1", hypothesisId = "B", location = "YydsScript.cs:1040", message = "提取availableCredit", data = new { availableCredit = availableCredit, isEmpty = string.IsNullOrEmpty(availableCredit) }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() });
                            // #endregion
                            
                            if (!string.IsNullOrEmpty(availableCredit) && decimal.TryParse(availableCredit, out var balanceValue))
                            {
                                _currentBalance = balanceValue;
                                _logCallback($"💰 余额更新: {_currentBalance}");
                                
                                // #region agent log
                                WriteDebugLog(new { sessionId = "debug-session", runId = "run1", hypothesisId = "B", location = "YydsScript.cs:1047", message = "余额解析成功", data = new { balanceValue = balanceValue, _currentBalance = _currentBalance }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() });
                                // #endregion
                            }
                            
                            // 提取平台类型: data.user.betPlate
                            var betPlate = json["data"]?["user"]?["betPlate"]?.ToString() ?? "";
                            if (!string.IsNullOrEmpty(betPlate))
                            {
                                _betPlate = betPlate;
                                _logCallback($"📊 平台类型: {_betPlate}");
                                
                                // #region agent log
                                WriteDebugLog(new { sessionId = "debug-session", runId = "run1", hypothesisId = "C", location = "YydsScript.cs:1058", message = "平台类型提取成功", data = new { betPlate = betPlate, _betPlate = _betPlate }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() });
                                // #endregion
                            }
                            
                            // 🔥 更新API投注域名（从/info接口的域名提取，不是登录域名）
                            if (string.IsNullOrEmpty(_apiBaseUrl) && !string.IsNullOrEmpty(response.Url))
                            {
                                try
                                {
                                    var uri = new Uri(response.Url);
                                    _apiBaseUrl = uri.GetLeftPart(UriPartial.Authority);
                                    _logCallback($"✅ API投注域名已设置: {_apiBaseUrl}");
                                    
                                    // #region agent log
                                    WriteDebugLog(new { sessionId = "debug-session", runId = "run1", hypothesisId = "D", location = "YydsScript.cs:1072", message = "API域名设置成功", data = new { responseUrl = response.Url, _apiBaseUrl = _apiBaseUrl }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() });
                                    // #endregion
                                }
                                catch { }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logCallback($"⚠️ 解析/info响应失败: {ex.Message}");
                        
                        // #region agent log
                        WriteDebugLog(new { sessionId = "debug-session", runId = "run1", hypothesisId = "B", location = "YydsScript.cs:1082", message = "解析/info异常", data = new { error = ex.Message, responseContext = response.Context?.Substring(0, Math.Min(200, response.Context?.Length ?? 0)) }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() });
                        // #endregion
                    }
                }
                //https://admin-api.06n.yyds666.me/game/game/pc_user/info?gameId=1&playTypeId=1
                // 🔥 拦截赔率配置接口（info?gameId=1&playTypeId=1）
                else if (response.Url.Contains("/info?gameId=") && response.Url.Contains("playTypeId="))
                {
                    // 🔥 检查是否允许自动更新赔率
                    if (!_autoUpdateOdds && _oddsLoaded)
                    {
                        // 已加载赔率且禁用自动更新，跳过
                        return;
                    }
                    
                    _logCallback($"📊 拦截赔率配置接口: {response.Url}");
                    
                    try
                    {
                        var json = JObject.Parse(response.Context);
                        var code = json["code"]?.Value<int>() ?? 0;
                        
                        if (code == 200)
                        {
                            ParseOddsInfo(json);
                            
                            // 🔥 首次加载成功后，禁用自动更新
                            if (!_oddsLoaded)
                            {
                                _oddsLoaded = true;
                                _autoUpdateOdds = false;  // 禁用自动更新
                                _logCallback($"✅ 赔率首次加载完成，已禁用自动更新（共 {_OddsInfo.Count} 个赔率项）");
                                _logCallback($"💡 如需刷新赔率，请手动点击更新按钮");
                            }
                        }
                        else
                        {
                            _logCallback($"⚠️ 赔率配置接口返回错误: code={code}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logCallback($"❌ 解析赔率配置失败: {ex.Message}");
                    }
                }
                else if (response.Url.Contains("/info"))
                {
                    // #region agent log
                    WriteDebugLog(new { sessionId = "debug-session", runId = "run1", hypothesisId = "E", location = "YydsScript.cs:1089", message = "跳过带参数的/info接口", data = new { url = response.Url, hasQueryString = response.Url.Contains("?") }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() });
                    // #endregion
                }
                
                // 拦截投注响应
                //if (response.Url.Contains("/bet") || response.Url.Contains("/place"))
                //{
                //    _logCallback($"📥 拦截投注响应: {response.Url}");
                //    _logCallback($"   响应: {response.Context}");
                //}
                
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
            //_OddsInfo.Add(new BrowserOddsInfo() {  Car = CarNumEnum.P1, Play = BetPlayEnum.大, CarName="大|DA", OddsId = betTypeId , Odds = 1.7f(默认的)})
            return _OddsInfo;

            //var oddsList = new List<BrowserOddsInfo>();

            //// 根据 _oddsValues 生成赔率列表
            //foreach (var kvp in _oddsValues)
            //{
            //    oddsList.Add(new BrowserOddsInfo
            //    {
            //        CarName = kvp.Key,   // 例如: "平一大"
            //        Odds = kvp.Value     // 例如: 1.97
            //    });
            //}

            //return oddsList;
        }

        /// <summary>
        /// 手动刷新赔率 - 重新启用自动更新并清空缓存
        /// </summary>
        public void RefreshOdds()
        {
            _logCallback("🔄 手动刷新赔率...");
            
            // 重置标志位，允许重新加载赔率
            _autoUpdateOdds = true;
            _oddsLoaded = false;
            
            // 清空现有赔率
            var oldCount = _OddsInfo.Count;
            _OddsInfo.Clear();
            
            _logCallback($"✅ 已清空 {oldCount} 个赔率项，等待平台返回新赔率数据");
            _logCallback($"💡 提示：切换到不同的玩法页面会触发赔率加载");
        }
        
        /// <summary>
        /// 获取赔率加载状态
        /// </summary>
        public (bool loaded, int count) GetOddsStatus()
        {
            return (_oddsLoaded, _OddsInfo.Count);
        }
        
        /// <summary>
        /// 调试：获取认证状态
        /// </summary>
        public void DebugAuthStatus()
        {
            _logCallback($"🔍 [认证状态调试]");
            _logCallback($"   - YydsScript实例哈希: {this.GetHashCode()}");
            _logCallback($"   - Token: {(string.IsNullOrEmpty(_token) ? "空" : $"已设置(长度:{_token.Length})")}");
            _logCallback($"   - SessionId: {(string.IsNullOrEmpty(_sessionId) ? "空" : "已设置")}");
            _logCallback($"   - 余额: {_currentBalance}");
            _logCallback($"   - API域名: {(string.IsNullOrEmpty(_apiBaseUrl) ? "未设置" : _apiBaseUrl)}");
            _logCallback($"   - 平台类型: {(string.IsNullOrEmpty(_betPlate) ? "未设置" : _betPlate)}");
            
            if (!string.IsNullOrEmpty(_token))
            {
                _logCallback($"   - Token预览: {_token.Substring(0, Math.Min(30, _token.Length))}...");
            }
        }
        
        /// <summary>
        /// 手动设置 Token（用于恢复认证状态）
        /// </summary>
        public void SetToken(string token, string? apiBaseUrl = null)
        {
            _token = token;
            
            if (!string.IsNullOrEmpty(token))
            {
                // 添加到请求头
                if (_httpClient.DefaultRequestHeaders.Contains("Authorization"))
                {
                    _httpClient.DefaultRequestHeaders.Remove("Authorization");
                }
                _httpClient.DefaultRequestHeaders.Add("Authorization", token);
                
                _logCallback($"✅ Token已手动设置 (长度: {token.Length})");
            }
            
            if (!string.IsNullOrEmpty(apiBaseUrl))
            {
                _apiBaseUrl = apiBaseUrl;
                _logCallback($"✅ API域名已设置: {apiBaseUrl}");
            }
        }
        
        /// <summary>
        /// 获取当前 Token（用于保存认证状态）
        /// </summary>
        public string? GetToken()
        {
            return _token;
        }

        /*
         * 投注
            {"totalAmount":20,
              "gameId":1,
              "periodNo":114069971,
              "addBodyList":[{"betTypeId":5,"dictValue":"DA","dictLabel":"大","amount":10},
                             {"betTypeId":5,"dictValue":"XIAO","dictLabel":"小","amount":10}
                             ]
             }
         */
        
        /// <summary>
        /// 获取未结算的订单信息（YYDS 平台暂不支持）
        /// </summary>
        public Task<(bool success, List<JObject>? orders, int maxRecordNum, int maxPageNum, string errorMsg)> GetLotMainOrderInfosAsync(
            int state = 0,
            int pageNum = 1,
            int pageCount = 20,
            string? beginDate = null,
            string? endDate = null,
            int timeout = 10)
        {
            return Task.FromResult<(bool, List<JObject>?, int, int, string)>((false, null, 0, 0, "平台暂不支持"));
        }
    }
}

