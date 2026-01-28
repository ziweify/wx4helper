using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using MoonSharp.Interpreter;
using Unit.La.Services;

namespace Unit.La.Scripting
{
    /// <summary>
    /// WebView2 桥接类 - 供 Lua 脚本调用
    /// 使用方式: web.Navigate("https://example.com")
    /// 使用 Func 动态获取 WebView2，确保在重新创建时能自动关联
    /// 🔥 使用 MoonSharpUserData 标记，让 MoonSharp 能够识别和转换此类型
    /// </summary>
    [MoonSharpUserData]
    public class WebBridge
    {
        private readonly Func<WebView2?> _webViewProvider;
        private readonly Action<string> _logger;
        private CancellationToken? _cancellationToken;
        
        /// <summary>
        /// 获取当前 WebView2 实例（动态）
        /// </summary>
        private WebView2 WebView
        {
            get
            {
                var webView = _webViewProvider?.Invoke();
                if (webView == null)
                {
                    throw new InvalidOperationException("WebView2 未初始化或已销毁");
                }
                return webView;
            }
        }
        
        /// <summary>
        /// 设置取消令牌（用于停止脚本）
        /// </summary>
        public void SetCancellationToken(CancellationToken token)
        {
            _cancellationToken = token;
        }

        /// <summary>
        /// 构造函数 - 使用 WebView2 提供者（动态引用）
        /// </summary>
        /// <param name="webViewProvider">WebView2 提供者函数，每次调用时获取最新的 WebView2 实例</param>
        /// <param name="logger">日志回调</param>
        public WebBridge(Func<WebView2?> webViewProvider, Action<string>? logger = null)
        {
            _webViewProvider = webViewProvider ?? throw new ArgumentNullException(nameof(webViewProvider));
            _logger = logger ?? (msg => { }); // 默认空日志
        }
        
        /// <summary>
        /// 兼容构造函数 - 直接传入 WebView2 实例
        /// </summary>
        /// <param name="webView">WebView2 实例</param>
        /// <param name="logger">日志回调</param>
        public WebBridge(WebView2 webView, Action<string>? logger = null)
            : this(() => webView, logger)
        {
        }

        #region 导航相关

        /// <summary>
        /// 导航到指定URL并等待页面加载完成
        /// 用法: 
        ///   local success, msg = web.Navigate("https://example.com", 10000)  -- 10秒超时
        ///   local success, msg = web.Navigate("https://example.com", 30000, true)  -- 强制刷新
        ///   local success, msg = web.Navigate("https://example.com", -1)  -- 无限等待（第三个参数可省略）
        ///   local success, msg = web.Navigate("https://example.com")  -- 默认30秒超时，不刷新
        /// </summary>
        /// <param name="url">目标 URL</param>
        /// <param name="timeout">超时时间（毫秒），-1 或 0 表示无限等待，默认 30000</param>
        /// <param name="forceRefresh">如果当前 URL 已是目标 URL，是否强制刷新。当 timeout = -1 时此参数无效，默认 false</param>
        /// <returns>(success: boolean, message: string)</returns>
        public DynValue Navigate(string url, int timeout = 30000, bool forceRefresh = false)
        {
            if (string.IsNullOrEmpty(url))
            {
                return CreateResult(false, "URL不能为空");
            }

            try
            {
                _logger($"🌐 导航到: {url}");
                
                // 🔥 处理 -1 表示无限等待
                var actualTimeout = timeout;
                if (timeout == -1)
                {
                    actualTimeout = 0;  // 0 表示无限等待
                    forceRefresh = false;  // 无限等待时，forceRefresh 失去意义，强制设为 false
                }
                
                // 🔥 检查当前 URL
                var currentUrl = GetCurrentUrl();
                var isSameUrl = IsUrlMatch(currentUrl, url);
                
                if (isSameUrl && !forceRefresh)
                {
                    _logger($"✅ 页面已是目标 URL，无需导航");
                    
                    // 检查页面是否已加载完成
                    if (IsPageLoaded())
                    {
                        return CreateResult(true, "页面已是目标 URL");
                    }
                    else
                    {
                        _logger($"⏳ 页面加载中，等待完成...");
                        // 等待页面加载完成
                        return WaitForPageLoad(actualTimeout);
                    }
                }
                
                if (isSameUrl && forceRefresh)
                {
                    _logger($"🔄 URL 相同，执行刷新");
                }
                
                // 确保在 UI 线程执行
                if (WebView.InvokeRequired)
                {
                    return (DynValue)WebView.Invoke(new Func<DynValue>(() => NavigateInternal(url, actualTimeout, forceRefresh, isSameUrl)));
                }
                else
                {
                    return NavigateInternal(url, actualTimeout, forceRefresh, isSameUrl);
                }
            }
            catch (Exception ex)
            {
                _logger($"❌ 导航异常: {ex.Message}");
                return CreateResult(false, $"异常：{ex.Message}");
            }
        }
        
        /// <summary>
        /// 获取当前 URL
        /// </summary>
        private string GetCurrentUrl()
        {
            try
            {
                if (WebView.InvokeRequired)
                {
                    return (string)WebView.Invoke(new Func<string>(() =>
                    {
                        return WebView.Source?.ToString() ?? "";
                    }));
                }
                else
                {
                    return WebView.Source?.ToString() ?? "";
                }
            }
            catch
            {
                return "";
            }
        }
        
        /// <summary>
        /// 检查页面是否已加载完成
        /// </summary>
        private bool IsPageLoaded()
        {
            try
            {
                if (WebView.CoreWebView2 == null)
                    return false;
                    
                var task = WebView.CoreWebView2.ExecuteScriptAsync("document.readyState");
                while (!task.IsCompleted)
                {
                    System.Windows.Forms.Application.DoEvents();
                    System.Threading.Thread.Sleep(10);
                }
                
                var readyState = task.Result?.Trim('"');
                return readyState == "complete";
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// 等待页面加载完成
        /// </summary>
        private DynValue WaitForPageLoad(int timeout)
        {
            var startTime = DateTime.Now;
            
            while (true)
            {
                // 检查脚本是否停止
                if (_cancellationToken?.IsCancellationRequested == true)
                {
                    _logger("⏹️ 页面加载被停止");
                    return CreateResult(false, "脚本已停止");
                }
                
                // 检查超时
                if (timeout > 0)
                {
                    var elapsed = (DateTime.Now - startTime).TotalMilliseconds;
                    if (elapsed > timeout)
                    {
                        _logger($"⏱️ 页面加载超时: {elapsed:F0}ms > {timeout}ms");
                        return CreateResult(false, $"超时：页面加载未完成");
                    }
                }
                
                // 检查 readyState
                if (IsPageLoaded())
                {
                    _logger($"✅ 页面加载完成");
                    return CreateResult(true, "页面已是目标 URL");
                }
                
                System.Windows.Forms.Application.DoEvents();
                System.Threading.Thread.Sleep(100);
            }
        }
        
        /// <summary>
        /// 比较两个 URL 是否匹配
        /// 规则：
        /// 1. 规范化 URL（去除末尾斜杠、统一小写）
        /// 2. 比较协议、主机、路径
        /// 3. 如果目标 URL 有查询参数，检查当前 URL 是否包含这些参数（值必须相同）
        /// 4. 当前 URL 多余的参数忽略
        /// </summary>
        private bool IsUrlMatch(string currentUrl, string targetUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(currentUrl) || string.IsNullOrEmpty(targetUrl))
                    return false;
                
                var current = new Uri(currentUrl);
                var target = new Uri(targetUrl);
                
                // 1. 比较协议、主机、路径（忽略大小写、去除末尾斜杠）
                var currentBase = (current.Scheme + "://" + current.Host + current.AbsolutePath.TrimEnd('/')).ToLower();
                var targetBase = (target.Scheme + "://" + target.Host + target.AbsolutePath.TrimEnd('/')).ToLower();
                
                if (currentBase != targetBase)
                    return false;
                
                // 2. 检查查询参数
                // 如果目标 URL 没有参数，忽略当前 URL 的所有参数
                if (string.IsNullOrEmpty(target.Query))
                    return true;
                
                // 解析查询参数
                var currentParams = ParseQueryString(current.Query);
                var targetParams = ParseQueryString(target.Query);
                
                // 检查目标参数是否都存在且值相同
                foreach (var targetParam in targetParams)
                {
                    if (!currentParams.TryGetValue(targetParam.Key, out var currentValue))
                        return false; // 目标参数不存在
                    
                    if (currentValue != targetParam.Value)
                        return false; // 参数值不同
                }
                
                // 所有目标参数都匹配
                return true;
            }
            catch
            {
                // 解析失败，使用简单字符串比较
                return NormalizeUrl(currentUrl) == NormalizeUrl(targetUrl);
            }
        }
        
        /// <summary>
        /// 解析查询参数
        /// </summary>
        private Dictionary<string, string> ParseQueryString(string query)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            
            if (string.IsNullOrEmpty(query))
                return result;
            
            // 去除开头的 '?'
            query = query.TrimStart('?');
            
            // 分割参数
            var pairs = query.Split('&');
            foreach (var pair in pairs)
            {
                var parts = pair.Split('=');
                if (parts.Length == 2)
                {
                    var key = Uri.UnescapeDataString(parts[0]);
                    var value = Uri.UnescapeDataString(parts[1]);
                    result[key] = value;
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// 规范化 URL（用于简单比较）
        /// </summary>
        private string NormalizeUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return "";
            
            // 转小写、去除末尾斜杠
            return url.ToLower().TrimEnd('/');
        }
        
        /// <summary>
        /// 内部导航实现（假定已在 UI 线程）
        /// </summary>
        private DynValue NavigateInternal(string url, int timeout, bool forceRefresh, bool isSameUrl)
        {
            try
            {
                // 1. 设置导航完成标志
                bool navigationCompleted = false;
                string? navigationError = null;
                
                EventHandler<CoreWebView2NavigationCompletedEventArgs> handler = (s, e) =>
                {
                    navigationCompleted = true;
                    if (!e.IsSuccess)
                    {
                        navigationError = GetNavigationErrorMessage(e.WebErrorStatus);
                    }
                };
                
                WebView.CoreWebView2.NavigationCompleted += handler;
                
                try
                {
                    // 2. 开始导航或刷新
                    if (isSameUrl && forceRefresh)
                    {
                        // 刷新页面
                        WebView.CoreWebView2.Reload();
                        _logger("🔄 刷新页面");
                    }
                    else
                    {
                        // 导航到新 URL
                        WebView.Source = new Uri(url);
                    }
                    
                    // 3. 等待导航完成（带超时和取消检查）
                    var startTime = DateTime.Now;
                    while (!navigationCompleted)
                    {
                        // 检查脚本是否停止
                        if (_cancellationToken?.IsCancellationRequested == true)
                        {
                            _logger("⏹️ 导航被停止");
                            return CreateResult(false, "脚本已停止");
                        }
                        
                        // 检查超时
                        if (timeout > 0)
                        {
                            var elapsed = (DateTime.Now - startTime).TotalMilliseconds;
                            if (elapsed > timeout)
                            {
                                _logger($"⏱️ 导航超时: {elapsed:F0}ms > {timeout}ms");
                                return CreateResult(false, $"超时：导航超过 {timeout} 毫秒");
                            }
                        }
                        
                        System.Windows.Forms.Application.DoEvents();
                        System.Threading.Thread.Sleep(50);
                    }
                    
                    // 4. 检查导航是否成功
                    if (!string.IsNullOrEmpty(navigationError))
                    {
                        _logger($"❌ 导航失败: {navigationError}");
                        return CreateResult(false, navigationError);
                    }
                    
                    _logger("⏳ 等待页面加载完成");
                    
                    // 5. 等待页面完全加载（readyState === 'complete'）
                    startTime = DateTime.Now;
                    int checkCount = 0;
                    
                    while (true)
                    {
                        // 检查脚本是否停止
                        if (_cancellationToken?.IsCancellationRequested == true)
                        {
                            _logger("⏹️ 页面加载被停止");
                            return CreateResult(false, "脚本已停止");
                        }
                        
                        // 检查超时
                        if (timeout > 0)
                        {
                            var elapsed = (DateTime.Now - startTime).TotalMilliseconds;
                            if (elapsed > timeout)
                            {
                                _logger($"⏱️ 页面加载超时: {elapsed:F0}ms > {timeout}ms");
                                return CreateResult(false, $"超时：页面加载未完成");
                            }
                        }
                        
                        // 检查 readyState
                        try
                        {
                            checkCount++;
                            var readyStateScript = "document.readyState";
                            var task = WebView.CoreWebView2.ExecuteScriptAsync(readyStateScript);
                            
                            // 使用 DoEvents 等待
                            while (!task.IsCompleted)
                            {
                                System.Windows.Forms.Application.DoEvents();
                                System.Threading.Thread.Sleep(10);
                            }
                            
                            var readyState = task.Result?.Trim('"');
                            
                            if (checkCount == 1)
                            {
                                _logger($"📜 执行脚本: {readyStateScript}...");
                            }
                            
                            if (readyState == "complete")
                            {
                                _logger($"✅ 页面加载完成");
                                
                                // 根据场景返回不同的成功信息
                                if (isSameUrl && forceRefresh)
                                {
                                    return CreateResult(true, "刷新并加载成功");
                                }
                                else
                                {
                                    return CreateResult(true, "加载成功");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger($"⚠️ 检查页面状态失败: {ex.Message}");
                        }
                        
                        System.Windows.Forms.Application.DoEvents();
                        System.Threading.Thread.Sleep(100);
                    }
                }
                finally
                {
                    WebView.CoreWebView2.NavigationCompleted -= handler;
                }
            }
            catch (Exception ex)
            {
                _logger($"❌ 导航内部错误: {ex.Message}");
                return CreateResult(false, $"异常：{ex.Message}");
            }
        }

        /// <summary>
        /// 创建多返回值结果
        /// </summary>
        private DynValue CreateResult(bool success, string message)
        {
            return DynValue.NewTuple(
                DynValue.NewBoolean(success),
                DynValue.NewString(message)
            );
        }

        /// <summary>
        /// 获取导航错误信息
        /// </summary>
        private string GetNavigationErrorMessage(CoreWebView2WebErrorStatus status)
        {
            return status switch
            {
                CoreWebView2WebErrorStatus.Timeout => "网络错误：连接超时",
                CoreWebView2WebErrorStatus.HostNameNotResolved => "DNS 错误：域名不存在",
                CoreWebView2WebErrorStatus.ConnectionAborted => "网络错误：连接中断",
                CoreWebView2WebErrorStatus.ConnectionReset => "网络错误：连接重置",
                CoreWebView2WebErrorStatus.Disconnected => "网络错误：网络断开",
                CoreWebView2WebErrorStatus.CannotConnect => "网络错误：无法连接",
                CoreWebView2WebErrorStatus.ServerUnreachable => "网络错误：服务器无法访问",
                CoreWebView2WebErrorStatus.ErrorHttpInvalidServerResponse => "HTTP 错误：服务器响应无效",
                _ => $"导航错误：{status}"
            };
        }

        /// <summary>
        /// 后退
        /// 用法: web.GoBack()
        /// </summary>
        public void GoBack()
        {
            _logger("⬅️ 后退");
            if (WebView.InvokeRequired)
            {
                WebView.Invoke(new Action(() =>
                {
                    if (WebView.CoreWebView2?.CanGoBack == true)
                        WebView.CoreWebView2.GoBack();
                }));
            }
            else
            {
                if (WebView.CoreWebView2?.CanGoBack == true)
                    WebView.CoreWebView2.GoBack();
            }
        }

        /// <summary>
        /// 前进
        /// 用法: web.GoForward()
        /// </summary>
        public void GoForward()
        {
            _logger("➡️ 前进");
            if (WebView.InvokeRequired)
            {
                WebView.Invoke(new Action(() =>
                {
                    if (WebView.CoreWebView2?.CanGoForward == true)
                        WebView.CoreWebView2.GoForward();
                }));
            }
            else
            {
                if (WebView.CoreWebView2?.CanGoForward == true)
                    WebView.CoreWebView2.GoForward();
            }
        }

        /// <summary>
        /// 刷新页面
        /// 用法: web.Reload()
        /// </summary>
        public void Reload()
        {
            _logger("🔄 刷新页面");
            if (WebView.InvokeRequired)
            {
                WebView.Invoke(new Action(() => WebView.CoreWebView2?.Reload()));
            }
            else
            {
                WebView.CoreWebView2?.Reload();
            }
        }

        /// <summary>
        /// 停止加载
        /// 用法: web.Stop()
        /// </summary>
        public void Stop()
        {
            _logger("⏹️ 停止加载");
            if (WebView.InvokeRequired)
            {
                WebView.Invoke(new Action(() => WebView.CoreWebView2?.Stop()));
            }
            else
            {
                WebView.CoreWebView2?.Stop();
            }
        }

        #endregion

        #region JavaScript 执行

        /// <summary>
        /// 执行 JavaScript 脚本
        /// 用法: local result = web.Execute("document.title")
        /// 🔥 同步执行，避免 UI 线程死锁
        /// </summary>
        public string Execute(string script)
        {
            if (string.IsNullOrEmpty(script))
            {
                throw new ArgumentException("脚本不能为空", nameof(script));
            }

            _logger($"📜 执行脚本: {script.Substring(0, Math.Min(50, script.Length))}...");

            // 🔥 确保在 UI 线程执行
            if (WebView.InvokeRequired)
            {
                return (string)WebView.Invoke(new Func<string>(() => ExecuteInternal(script)));
            }
            else
            {
                return ExecuteInternal(script);
            }
        }
        
        /// <summary>
        /// 内部执行方法（假定已在 UI 线程，同步执行）
        /// 🔥 使用自旋等待 + DoEvents 保持 UI 响应
        /// </summary>
        private string ExecuteInternal(string script)
        {
            if (WebView.CoreWebView2 == null)
            {
                throw new InvalidOperationException("WebView2 未初始化");
            }

            try
            {
                // 🔥 启动异步操作
                var task = WebView.CoreWebView2.ExecuteScriptAsync(script);
                
                // 🔥 使用自旋等待 + DoEvents 保持 UI 响应，避免死锁
                while (!task.IsCompleted)
                {
                    System.Windows.Forms.Application.DoEvents();
                    System.Threading.Thread.Sleep(10); // 短暂休眠，避免 CPU 100%
                }
                
                // 获取结果
                if (task.IsFaulted)
                {
                    throw task.Exception?.GetBaseException() ?? new Exception("JavaScript 执行失败");
                }
                
                return task.Result;
            }
            catch (Exception ex)
            {
                _logger($"❌ 脚本执行失败: {ex.Message}");
                throw new Exception($"JavaScript 执行失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 执行脚本并返回 JSON 对象
        /// 用法: local data = web.ExecuteJson("JSON.stringify({name: 'test', age: 30})")
        /// </summary>
        public object? ExecuteJson(string script)
        {
            var json = Execute(script);
            if (string.IsNullOrEmpty(json) || json == "null")
                return null;

            try
            {
                return JsonSerializer.Deserialize<object>(json);
            }
            catch
            {
                return json; // 如果不是 JSON，返回原始字符串
            }
        }

        #endregion

        #region 页面信息获取

        /// <summary>
        /// 获取当前 URL
        /// 用法: local url = web.GetUrl()
        /// </summary>
        public string GetUrl()
        {
            if (WebView.InvokeRequired)
            {
                return (string)WebView.Invoke(new Func<string>(() => 
                    WebView.Source?.ToString() ?? ""));
            }
            return WebView.Source?.ToString() ?? "";
        }

        /// <summary>
        /// 获取页面标题
        /// 用法: local title = web.GetTitle()
        /// </summary>
        public string GetTitle()
        {
            return Execute("document.title").Trim('"');
        }

        /// <summary>
        /// 获取页面 HTML
        /// 用法: local html = web.GetHtml()
        /// </summary>
        public string GetHtml()
        {
            return Execute("document.documentElement.outerHTML").Trim('"');
        }

        /// <summary>
        /// 获取页面文本内容
        /// 用法: local text = web.GetText()
        /// </summary>
        public string GetText()
        {
            return Execute("document.body.innerText").Trim('"');
        }

        #endregion

        #region DOM 元素操作

        /// <summary>
        /// 点击元素
        /// 用法: web.Click("#loginBtn")
        /// </summary>
        public void Click(string selector)
        {
            _logger($"🖱️ 点击: {selector}");
            Execute($"document.querySelector('{selector}').click()");
        }

        /// <summary>
        /// 输入文本
        /// 用法: web.Input("#username", "admin")
        /// </summary>
        public void Input(string selector, string text)
        {
            _logger($"⌨️ 输入: {selector} = {text}");
            var escapedText = text.Replace("\\", "\\\\").Replace("'", "\\'");
            Execute($"document.querySelector('{selector}').value = '{escapedText}'");
        }

        /// <summary>
        /// 触发 DOM 事件
        /// 用法: 
        ///   web.TriggerEvent("#username", "input")  -- 触发 input 事件（默认 bubbles=true）
        ///   web.TriggerEvent("#username", "input", true)  -- 触发 input 事件，bubbles=true
        ///   web.TriggerEvent("#username", "change", false)  -- 触发 change 事件，bubbles=false
        ///   web.TriggerEvent("#username", "input", true, true)  -- 触发 input 事件，bubbles=true, cancelable=true
        /// </summary>
        /// <param name="selector">元素选择器</param>
        /// <param name="eventType">事件类型（如 "input", "change", "click" 等）</param>
        /// <param name="bubbles">是否冒泡，默认 true</param>
        /// <param name="cancelable">是否可取消，默认 true</param>
        public void TriggerEvent(string selector, string eventType, bool bubbles = true, bool cancelable = true)
        {
            _logger($"🎯 触发事件: {selector} -> {eventType} (bubbles={bubbles}, cancelable={cancelable})");
            var escapedSelector = selector.Replace("'", "\\'");
            var escapedEventType = eventType.Replace("'", "\\'");
            Execute($@"
                (function() {{
                    var el = document.querySelector('{escapedSelector}');
                    if (el) {{
                        var event = new Event('{escapedEventType}', {{ 
                            bubbles: {bubbles.ToString().ToLower()}, 
                            cancelable: {cancelable.ToString().ToLower()} 
                        }});
                        el.dispatchEvent(event);
                    }}
                }})()
            ");
        }

        /// <summary>
        /// 输入文本并触发 input 事件（常用组合操作）
        /// 用法: web.InputAndTrigger("#username", "admin")
        /// 等同于: web.Input("#username", "admin"); web.TriggerEvent("#username", "input")
        /// </summary>
        public void InputAndTrigger(string selector, string text)
        {
            Input(selector, text);
            TriggerEvent(selector, "input");
        }

        /// <summary>
        /// 获取元素文本
        /// 用法: local text = web.GetText("#title")
        /// </summary>
        public string GetElementText(string selector)
        {
            var result = Execute($"document.querySelector('{selector}')?.innerText || ''");
            return result.Trim('"');
        }

        /// <summary>
        /// 获取输入元素的值（value 属性）
        /// 用法: local value = web.GetValue("#username")
        /// </summary>
        public string GetValue(string selector)
        {
            var escapedSelector = selector.Replace("'", "\\'");
            var result = Execute($"document.querySelector('{escapedSelector}')?.value || ''");
            return result.Trim('"');
        }

        /// <summary>
        /// 获取元素属性
        /// 用法: local href = web.GetAttr("#link", "href")
        /// </summary>
        public string GetAttr(string selector, string attribute)
        {
            var result = Execute($"document.querySelector('{selector}')?.getAttribute('{attribute}') || ''");
            return result.Trim('"');
        }

        /// <summary>
        /// 设置元素属性
        /// 用法: web.SetAttr("#input", "placeholder", "请输入...")
        /// </summary>
        public void SetAttr(string selector, string attribute, string value)
        {
            var escapedValue = value.Replace("\\", "\\\\").Replace("'", "\\'");
            Execute($"document.querySelector('{selector}').setAttribute('{attribute}', '{escapedValue}')");
        }

        /// <summary>
        /// 检查元素是否存在
        /// 用法: if web.Exists("#loginBtn") then ... end
        /// </summary>
        public bool Exists(string selector)
        {
            var result = Execute($"document.querySelector('{selector}') !== null");
            return result.Trim().ToLower() == "true";
        }

        /// <summary>
        /// 检查元素是否可见
        /// 用法: if web.IsVisible("#dialog") then ... end
        /// </summary>
        public bool IsVisible(string selector)
        {
            var script = $@"
                (function() {{
                    var el = document.querySelector('{selector}');
                    if (!el) return false;
                    var style = window.getComputedStyle(el);
                    return style.display !== 'none' && style.visibility !== 'hidden' && style.opacity !== '0';
                }})()
            ";
            var result = Execute(script);
            return result.Trim().ToLower() == "true";
        }

        /// <summary>
        /// 获取元素数量
        /// 用法: local count = web.Count(".item")
        /// </summary>
        public int Count(string selector)
        {
            var result = Execute($"document.querySelectorAll('{selector}').length");
            return int.Parse(result);
        }

        #endregion

        #region 等待操作

        /// <summary>
        /// 等待指定毫秒 - UI 友好版本
        /// 🔥 使用 DoEvents 保持界面响应，避免卡死
        /// 用法: web.Wait(1000) -- 等待1秒，界面不卡
        /// </summary>
        public void Wait(int milliseconds)
        {
            if (milliseconds <= 0) return;

            _logger($"⏱️ 等待 {milliseconds}ms");
            
            var startTime = DateTime.Now;
            var targetTime = startTime.AddMilliseconds(milliseconds);

            // 🔥 使用 DoEvents 循环，保持 UI 响应
            while (DateTime.Now < targetTime)
            {
                // 检查是否已停止
                if (_cancellationToken?.IsCancellationRequested == true)
                {
                    _logger("⏹️ 等待被取消");
                    return; // 提前退出
                }

                // 🔥 处理 UI 消息，保持界面响应
                System.Windows.Forms.Application.DoEvents();

                // 短暂休眠，避免 CPU 100%
                var remaining = (targetTime - DateTime.Now).TotalMilliseconds;
                if (remaining > 0)
                {
                    Thread.Sleep(Math.Min(50, (int)remaining)); // 每次最多休眠 50ms
                }
            }
        }

        /// <summary>
        /// 等待元素出现
        /// 用法: web.WaitFor("#loginBtn", 5000) -- 最多等待5秒
        /// </summary>
        public bool WaitFor(string selector, int timeoutMs = 10000)
        {
            _logger($"⏳ 等待元素: {selector}");
            var endTime = DateTime.Now.AddMilliseconds(timeoutMs);
            
            while (DateTime.Now < endTime)
            {
                if (Exists(selector))
                {
                    _logger($"✅ 元素已出现: {selector}");
                    return true;
                }
                Thread.Sleep(100);
            }
            
            _logger($"⏰ 等待超时: {selector}");
            return false;
        }

        /// <summary>
        /// 等待元素消失
        /// 用法: web.WaitForHidden("#loading", 5000)
        /// </summary>
        public bool WaitForHidden(string selector, int timeoutMs = 10000)
        {
            _logger($"⏳ 等待元素消失: {selector}");
            var endTime = DateTime.Now.AddMilliseconds(timeoutMs);
            
            while (DateTime.Now < endTime)
            {
                if (!IsVisible(selector))
                {
                    _logger($"✅ 元素已消失: {selector}");
                    return true;
                }
                Thread.Sleep(100);
            }
            
            _logger($"⏰ 等待超时: {selector}");
            return false;
        }

        /// <summary>
        /// 等待页面加载完成
        /// 用法: web.WaitForLoad()
        /// </summary>
        public bool WaitForLoad(int timeoutMs = 30000)
        {
            _logger("⏳ 等待页面加载完成");
            var endTime = DateTime.Now.AddMilliseconds(timeoutMs);
            
            while (DateTime.Now < endTime)
            {
                var readyState = Execute("document.readyState").Trim('"');
                if (readyState == "complete")
                {
                    _logger("✅ 页面加载完成");
                    return true;
                }
                Thread.Sleep(100);
            }
            
            _logger("⏰ 页面加载超时");
            return false;
        }

        #endregion

        #region 滚动操作

        /// <summary>
        /// 滚动到顶部
        /// 用法: web.ScrollToTop()
        /// </summary>
        public void ScrollToTop()
        {
            _logger("⬆️ 滚动到顶部");
            Execute("window.scrollTo(0, 0)");
        }

        /// <summary>
        /// 滚动到底部
        /// 用法: web.ScrollToBottom()
        /// </summary>
        public void ScrollToBottom()
        {
            _logger("⬇️ 滚动到底部");
            Execute("window.scrollTo(0, document.body.scrollHeight)");
        }

        /// <summary>
        /// 滚动到指定元素
        /// 用法: web.ScrollTo("#section2")
        /// </summary>
        public void ScrollTo(string selector)
        {
            _logger($"📜 滚动到: {selector}");
            Execute($"document.querySelector('{selector}').scrollIntoView({{behavior: 'smooth', block: 'center'}})");
        }

        /// <summary>
        /// 滚动指定距离
        /// 用法: web.ScrollBy(0, 500) -- 向下滚动500px
        /// </summary>
        public void ScrollBy(int x, int y)
        {
            _logger($"📜 滚动: ({x}, {y})");
            Execute($"window.scrollBy({x}, {y})");
        }

        #endregion

        #region Cookie 操作

        /// <summary>
        /// 获取所有 Cookies
        /// 用法: local cookies = web.GetCookies()
        /// </summary>
        public string GetCookies()
        {
            return Execute("document.cookie").Trim('"');
        }

        /// <summary>
        /// 设置 Cookie
        /// 用法: web.SetCookie("token", "abc123", 7)
        /// </summary>
        public void SetCookie(string name, string value, int days = 7)
        {
            _logger($"🍪 设置Cookie: {name}");
            var expires = DateTime.Now.AddDays(days).ToString("R");
            Execute($"document.cookie = '{name}={value}; expires={expires}; path=/'");
        }

        /// <summary>
        /// 删除 Cookie
        /// 用法: web.DeleteCookie("token")
        /// </summary>
        public void DeleteCookie(string name)
        {
            _logger($"🗑️ 删除Cookie: {name}");
            Execute($"document.cookie = '{name}=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/'");
        }

        /// <summary>
        /// 清除所有 Cookies
        /// 用法: web.ClearCookies()
        /// </summary>
        public void ClearCookies()
        {
            _logger("🗑️ 清除所有Cookies");
            var script = @"
                document.cookie.split(';').forEach(function(c) { 
                    document.cookie = c.replace(/^ +/, '').replace(/=.*/, '=;expires=' + new Date().toUTCString() + ';path=/'); 
                });
            ";
            Execute(script);
        }

        #endregion

        #region 表单操作

        /// <summary>
        /// 选择下拉框选项（按值）
        /// 用法: web.Select("#country", "CN")
        /// </summary>
        public void Select(string selector, string value)
        {
            _logger($"📋 选择: {selector} = {value}");
            Execute($"document.querySelector('{selector}').value = '{value}'");
        }

        /// <summary>
        /// 选择下拉框选项（按索引）
        /// 用法: web.SelectIndex("#country", 0)
        /// </summary>
        public void SelectIndex(string selector, int index)
        {
            _logger($"📋 选择索引: {selector}[{index}]");
            Execute($"document.querySelector('{selector}').selectedIndex = {index}");
        }

        /// <summary>
        /// 勾选/取消复选框
        /// 用法: web.Check("#agree", true)
        /// </summary>
        public void Check(string selector, bool checked_ = true)
        {
            _logger($"☑️ {(checked_ ? "勾选" : "取消")}: {selector}");
            Execute($"document.querySelector('{selector}').checked = {checked_.ToString().ToLower()}");
        }

        /// <summary>
        /// 提交表单
        /// 用法: web.Submit("#loginForm")
        /// </summary>
        public void Submit(string selector)
        {
            _logger($"📤 提交表单: {selector}");
            Execute($"document.querySelector('{selector}').submit()");
        }

        #endregion

        #region 高级操作

        /// <summary>
        /// 注入 CSS 样式
        /// 用法: web.InjectCss("body { background: red; }")
        /// </summary>
        public void InjectCss(string css)
        {
            _logger("🎨 注入CSS");
            var escapedCss = css.Replace("\\", "\\\\").Replace("'", "\\'").Replace("\n", "\\n");
            Execute($@"
                (function() {{
                    var style = document.createElement('style');
                    style.textContent = '{escapedCss}';
                    document.head.appendChild(style);
                }})()
            ");
        }

        /// <summary>
        /// 注入 JavaScript 库
        /// 用法: web.InjectJs("https://cdn.jsdelivr.net/npm/jquery@3.6.0/dist/jquery.min.js")
        /// </summary>
        public void InjectJs(string url)
        {
            _logger($"📦 注入JS: {url}");
            Execute($@"
                (function() {{
                    var script = document.createElement('script');
                    script.src = '{url}';
                    document.head.appendChild(script);
                }})()
            ");
        }

        /// <summary>
        /// 打开开发者工具
        /// 用法: web.OpenDevTools()
        /// </summary>
        public void OpenDevTools()
        {
            _logger("🔧 打开开发者工具");
            if (WebView.InvokeRequired)
            {
                WebView.Invoke(new Action(() => WebView.CoreWebView2?.OpenDevToolsWindow()));
            }
            else
            {
                WebView.CoreWebView2?.OpenDevToolsWindow();
            }
        }

        /// <summary>
        /// 截图并保存
        /// 用法: web.Screenshot("screenshot.png")
        /// 🔥 确保在 UI 线程上执行
        /// </summary>
        public void Screenshot(string filePath)
        {
            _logger($"📸 截图: {filePath}");
            
            if (WebView.InvokeRequired)
            {
                WebView.Invoke(new Action(() => ScreenshotInternal(filePath)));
            }
            else
            {
                ScreenshotInternal(filePath);
            }
        }

        private void ScreenshotInternal(string filePath)
        {
            if (WebView.CoreWebView2 == null)
            {
                throw new InvalidOperationException("WebView2 未初始化");
            }
            
            var task = WebView.CoreWebView2.CapturePreviewAsync(
                CoreWebView2CapturePreviewImageFormat.Png,
                File.OpenWrite(filePath)
            );
            task.Wait();
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 获取元素的所有文本内容（包括子元素）
        /// 用法: local texts = web.GetAllText(".item")
        /// </summary>
        public List<string> GetAllText(string selector)
        {
            var script = $@"
                Array.from(document.querySelectorAll('{selector}'))
                    .map(el => el.innerText)
            ";
            var result = Execute($"JSON.stringify({script})");
            
            try
            {
                return JsonSerializer.Deserialize<List<string>>(result) ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        /// <summary>
        /// 获取元素的所有属性值
        /// 用法: local hrefs = web.GetAllAttr("a", "href")
        /// </summary>
        public List<string> GetAllAttr(string selector, string attribute)
        {
            var script = $@"
                Array.from(document.querySelectorAll('{selector}'))
                    .map(el => el.getAttribute('{attribute}') || '')
            ";
            var result = Execute($"JSON.stringify({script})");
            
            try
            {
                return JsonSerializer.Deserialize<List<string>>(result) ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        #endregion

        #region 响应拦截相关

        private static Action<object>? _responseHandler;

        /// <summary>
        /// 注册响应处理器
        /// 用法: OnResponse(function(response)
        ///     log('响应URL: ' .. response.url)
        ///     log('状态码: ' .. response.statusCode)
        ///     log('内容: ' .. response.context)
        /// end)
        /// </summary>
        public static void OnResponse(DynValue handlerFunc)
        {
            if (handlerFunc == null || handlerFunc.Type != DataType.Function)
            {
                throw new ArgumentException("OnResponse 的参数必须是函数");
            }

            // 获取脚本引擎
            var script = handlerFunc.Function.OwnerScript;
            if (script == null)
            {
                throw new InvalidOperationException("无法获取脚本引擎实例");
            }

            // 注册响应处理器
            _responseHandler = (responseObj) =>
            {
                try
                {
                    // 将响应对象转换为 Lua Table
                    var responseTable = DynValue.NewTable(script);
                    if (responseObj is Services.ResponseEventArgs responseArgs)
                    {
                        responseTable.Table["url"] = DynValue.NewString(responseArgs.Url ?? "");
                        responseTable.Table["statusCode"] = DynValue.NewNumber(responseArgs.StatusCode);
                        responseTable.Table["context"] = DynValue.NewString(responseArgs.Context ?? "");
                        responseTable.Table["postData"] = DynValue.NewString(responseArgs.PostData ?? "");
                        responseTable.Table["contentType"] = DynValue.NewString(responseArgs.ContentType ?? "");
                        responseTable.Table["referrerUrl"] = DynValue.NewString(responseArgs.ReferrerUrl ?? "");
                    }

                    // 调用 Lua 函数
                    script.Call(handlerFunc, responseTable);
                }
                catch (Exception ex)
                {
                    // 记录错误，但不抛出异常（避免影响响应处理流程）
                    System.Diagnostics.Debug.WriteLine($"响应处理器执行错误: {ex.Message}");
                }
            };
        }

        /// <summary>
        /// 触发响应处理器（由 C# 代码调用）
        /// </summary>
        public static void InvokeResponseHandler(Services.ResponseEventArgs args)
        {
            _responseHandler?.Invoke(args);
        }

        #endregion
    }
}
