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
        /// 导航到指定URL
        /// 用法: web.Navigate("https://example.com")
        /// </summary>
        public void Navigate(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentException("URL不能为空", nameof(url));
            }

            _logger($"🌐 导航到: {url}");
            
            if (WebView.InvokeRequired)
            {
                WebView.Invoke(new Action(() => WebView.Source = new Uri(url)));
            }
            else
            {
                WebView.Source = new Uri(url);
            }
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
        /// </summary>
        private string ExecuteInternal(string script)
        {
            if (WebView.CoreWebView2 == null)
            {
                throw new InvalidOperationException("WebView2 未初始化");
            }

            try
            {
                // 🔥 使用 GetAwaiter().GetResult() 同步等待
                var result = WebView.CoreWebView2.ExecuteScriptAsync(script).GetAwaiter().GetResult();
                return result;
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
        /// 获取元素文本
        /// 用法: local text = web.GetText("#title")
        /// </summary>
        public string GetElementText(string selector)
        {
            var result = Execute($"document.querySelector('{selector}')?.innerText || ''");
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
        /// 等待指定毫秒
        /// 用法: web.Wait(1000) -- 等待1秒
        /// </summary>
        public void Wait(int milliseconds)
        {
            _logger($"⏱️ 等待 {milliseconds}ms");
            Thread.Sleep(milliseconds);
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
    }
}
