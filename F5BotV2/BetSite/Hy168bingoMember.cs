using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CefSharp;
using F5BotV2.BetSite.HongHai;
using F5BotV2.BetSite.yyz168;
using F5BotV2.CefBrowser;
using F5BotV2.Ext;
using F5BotV2.Model;
using LxLib.LxNet;
using LxLib.LxSys;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace F5BotV2.BetSite
{
    public class Hy168bingoMember
        : IBetApi
        , INotifyPropertyChanged
    {
        CancellationTokenSource cts = null;
        private Hy168bingoOdds _Odds = new Hy168bingoOdds();
        public Hy168bingoOdds Odds { get { return _Odds; } }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged<T>(Expression<Func<T>> property)
        {
            if (PropertyChanged == null)
                return;

            var memberExpression = property.Body as MemberExpression;
            if (memberExpression == null)
                return;

            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(memberExpression.Member.Name));
        }

        private string abcd = "";
        private string ab = "";
        private string gid = "";

        private IBetBrowserBase browser;
        IBetApi betApi { get; set; }

        public string urlRoot => betApi.urlRoot;

        public float amount => betApi.amount;

        public string cookie => betApi.cookie;

        public BetSiteType betSiteType => betApi.betSiteType;

        public bool isLoginSuccess { get => betApi.isLoginSuccess; set => betApi.isLoginSuccess = value; }

        public Hy168bingoMember(IBetBrowserBase browser, BetSiteType bstype)
        {
            betApi = new BetApi(bstype);
            betApi.SetRootUrl("http://hy.168bingo.top/");
            this.browser = browser;
            this.browser.chromeBroser.FrameLoadEnd += ChromeBroser_FrameLoadEnd;
            this.browser.chromeBroser.RequestHandler = new CefRequestHandler(ChromeBroser_ResponseComplete);
        }

        private void ChromeBroser_FrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
            //if (e.Url.IndexOf($"/Member/Agreement") >= 0)
            //{
            //    //自动点击同意
            //    //document.querySelectorAll('.submit_btn')[0].text == 同意
            //    //document.querySelectorAll('.submit_btn')[0].click()
            //    Task.Factory.StartNew(() => {
            //        var p = browser.chromeBroser;
            //        var js_tongyi = p.GetBrowser().MainFrame.EvaluateScriptAsync($"document.querySelectorAll('.submit_btn')[0].text");
            //        js_tongyi.Wait();
            //        var jsText = (string)js_tongyi.Result.Result;
            //        if (jsText != null)
            //            if (jsText == "同意")
            //                p.GetBrowser().MainFrame.ExecuteJavaScriptAsync($"document.querySelectorAll('.submit_btn')[0].click()");
            //    });
            //}
            //else if (e.Url.IndexOf($"{urlRoot}/Home/Index") >= 0)
            //{
            //    //处理弹窗
            //    Task.Factory.StartNew(() => {
            //        var p = browser.chromeBroser;
            //        for (int i = 1; i <= 4; i++)
            //        {
            //            //document.querySelectorAll('#ui-id-4')[0].parentNode.childNodes[1].textContent;
            //            string jsquery_textContent = $"document.querySelectorAll('#ui-id-{i}')[0].parentNode.childNodes[1].textContent";
            //            string jsquery_click = $"document.querySelectorAll('#ui-id-{i}')[0].parentNode.childNodes[1].click()";
            //            var js_tongyi = p.GetBrowser().MainFrame.EvaluateScriptAsync(jsquery_textContent);
            //            js_tongyi.Wait();
            //            var jsText = (string)js_tongyi.Result.Result;
            //            if (jsText != null)
            //                if (jsText == "Close")
            //                    p.GetBrowser().MainFrame.ExecuteJavaScriptAsync(jsquery_click);
            //        }


            //    });
            //}
            //else if (e.Url.IndexOf($"?ReturnUrl=/Home/Index") >= 0)
            //{
            //    //被挤下线了，需要重新登录

            //}
        }

        private void ChromeBroser_ResponseComplete(object sender, ResponseEventArgs args)
        {
            //处理资源
            try
            {
                //https://frhrewobvower.da16888.top/frclienthall/getnoticeinfo
                //https://frhrewobvower.da16888.top/frclienthall/gettodaywinlost
                if (args.Url.IndexOf("/gettodaywinlost") != -1)
                {
                    //sid = Regex.Match(args.PostData, "sid=([^&]+)").Groups[1].Value;
                    //Uuid = Regex.Match(args.PostData, "uuid=([^&]+)").Groups[1].Value;
                    //token = Regex.Match(args.PostData, "token=([^&]+)").Groups[1].Value;
                }
                else if(args.Url.IndexOf("/make.php") != -1)
                {
                    //postdata:xtype=lib&stype=1-6&bid=1-6&abcd=A&ab=A&sid=
                    //更新赔率
                }
                else if(args.Url.IndexOf("/makelib.php") != -1)
                {
                    //xtype=getatt&abcd=A&ab=A&gid=300
                    try
                    {
                        var argsString = args.PostData;
                        var parameters = new Dictionary<string, string>();

                        // 分割参数
                        string[] pairs = argsString.Split('&');
                        foreach (string pair in pairs)
                        {
                            string[] keyValue = pair.Split('=');
                            if (keyValue.Length == 2)
                            {
                                string key = keyValue[0].Trim().ToLower();
                                string value = keyValue[1].Trim();
                                parameters[key] = value;
                            }
                        }
                        if(parameters.ContainsKey("xtype"))
                        {
                            if (parameters["xtype"] == "getatt")
                            {
                                if (parameters.ContainsKey("abcd"))
                                    abcd = parameters["abcd"];
                                if (parameters.ContainsKey("ab"))
                                    ab = parameters["ab"];
                            }
                        }
                    }
                    catch(Exception ex) 
                    {

                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ERROR::ChromeBroser_ResponseComplete::" + ex.Message);
            }
        }

     

        public bool SetRootUrl(string url)
        {
            return betApi.SetRootUrl(url);
        }

        public void SetCookie(string cookie)
        {
            betApi.SetCookie(cookie);
        }

        public void Cancel()
        {
            betApi.Cancel();
        }

        public int LoginAsync(string name, string pass, IBetBrowserBase browser)
        {

            Debug.WriteLine("LoginAsync::进入");
            if (cts == null)
            {
                cts = new CancellationTokenSource();

                Task.Factory.StartNew(async (object hx666) => {
                    await browser.LoadUrlAsyn(urlRoot, new Func<ChromiumWebBrowserExtend, bool>((p) =>
                    {
                        HongHaiWuMing hx = hx666 as HongHaiWuMing;
                        bool response = true;
                        try
                        {
                            p.OnJSDialog = OnJSDialogLogin;
                            //由于加载了验证页面, 判断是否是正确登录页
                            using (cts.Token.Register((Thread.CurrentThread.Abort)))
                            {
                                //这个线程就一直给他跑, 不返回了
                                bool hasFilledLoginForm = false; // 标志位：是否已经填写过登录表单
                                while (true)
                                {
                                    try
                                    {
                                        // 检查是否已经在登录页面
                                        var currentUrl = p.UrlCurrent;
                                        if (currentUrl != null && currentUrl.Contains("Member/Login"))
                                        {
                                            // 等待页面完全加载
                                            Thread.Sleep(500);
                                            
                                            // 检测页面使用的前端框架
                                            var frameworkDetectionScript = @"
                                                (function() {
                                                    var frameworks = [];
                                                    
                                                    // 检测Vue.js
                                                    if (window.Vue || document.querySelector('[data-v-]') || document.querySelector('*[v-]')) {
                                                        frameworks.push('Vue.js');
                                                    }
                                                    
                                                    // 检测React
                                                    if (window.React || document.querySelector('*[data-reactroot]') || document.querySelector('*[data-react-]')) {
                                                        frameworks.push('React');
                                                    }
                                                    
                                                    // 检测Angular
                                                    if (window.angular || window.ng || document.querySelector('*[ng-]') || document.querySelector('*[data-ng-]')) {
                                                        frameworks.push('Angular');
                                                    }
                                                    
                                                    // 检测jQuery
                                                    if (window.jQuery || window.$) {
                                                        frameworks.push('jQuery');
                                                    }
                                                    
                                                    // 检查DOM元素是否有Vue相关属性
                                                    var hasVueElements = false;
                                                    var elements = document.querySelectorAll('*');
                                                    for (var i = 0; i < Math.min(elements.length, 100); i++) {
                                                        var el = elements[i];
                                                        if (el._vei || el.__vue__ || el.__VUE__) {
                                                            hasVueElements = true;
                                                            break;
                                                        }
                                                    }
                                                    
                                                    if (hasVueElements && frameworks.indexOf('Vue.js') === -1) {
                                                        frameworks.push('Vue.js (detected via DOM)');
                                                    }
                                                    
                                                    return {
                                                        frameworks: frameworks,
                                                        hasVue: frameworks.some(f => f.indexOf('Vue') > -1),
                                                        hasReact: frameworks.some(f => f.indexOf('React') > -1),
                                                        hasAngular: frameworks.some(f => f.indexOf('Angular') > -1),
                                                        count: frameworks.length
                                                    };
                                                })();
                                            ";
                                            
                                            var frameworkTask = p.GetBrowser().MainFrame.EvaluateScriptAsync(frameworkDetectionScript);
                                            frameworkTask.Wait(2000);
                                            
                                            if (frameworkTask.IsCompleted && frameworkTask.Result != null && frameworkTask.Result.Success)
                                            {
                                                dynamic frameworkInfo = frameworkTask.Result.Result;
                                                if (frameworkInfo != null)
                                                {
                                                    Debug.WriteLine($"前端框架检测: 使用Vue={frameworkInfo.hasVue}, 使用React={frameworkInfo.hasReact}, 使用Angular={frameworkInfo.hasAngular}");
                                                    
                                                    if (frameworkInfo.frameworks != null && frameworkInfo.count > 0)
                                                    {
                                                        var frameworks = frameworkInfo.frameworks;
                                                        Debug.WriteLine("检测到的框架:");
                                                        for (int i = 0; i < frameworks.Count; i++)
                                                        {
                                                            Debug.WriteLine($"  - {frameworks[i]}");
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Debug.WriteLine("未检测到主流前端框架，可能使用原生JavaScript");
                                                    }
                                                }
                                            }
                                            
                                            // 检查登录表单是否存在
                                            var checkFormScript = @"
                                                (function() {
                                                    var loginForm = document.querySelector('form');
                                                    var usernameInput = document.querySelector('input[name=""username""], input[type=""text""], input[placeholder*=""帐""], input[placeholder*=""账""], input[placeholder*=""用户""]');
                                                    var passwordInput = document.querySelector('input[name=""password""], input[type=""password""], input[placeholder*=""密码""]');
                                                    var submitBtn = document.querySelector('input[type=""submit""], button[type=""submit""], .btn-login, #loginBtn');
                                                    
                                                    // 获取页面上所有可能的按钮信息用于调试
                                                    var allButtons = document.querySelectorAll('button, input[type=""button""], input[type=""submit""], a.btn, .btn, [onclick]');
                                                    var buttonInfo = [];
                                                    for(var i = 0; i < allButtons.length; i++) {
                                                        var btn = allButtons[i];
                                                        buttonInfo.push({
                                                            tag: btn.tagName,
                                                            type: btn.type || 'none',
                                                            className: btn.className || 'none',
                                                            id: btn.id || 'none',
                                                            text: (btn.textContent || btn.value || btn.innerText || '').trim(),
                                                            hasOnclick: !!btn.onclick
                                                        });
                                                    }
                                                    
                                                    return {
                                                        hasForm: !!loginForm,
                                                        hasUsername: !!usernameInput,
                                                        hasPassword: !!passwordInput,
                                                        hasSubmitBtn: !!submitBtn,
                                                        formAction: loginForm ? loginForm.action : 'none',
                                                        formMethod: loginForm ? loginForm.method : 'none',
                                                        buttonCount: allButtons.length,
                                                        buttons: buttonInfo,
                                                        url: window.location.href
                                                    };
                                                })();
                                            ";

                                            var formCheckTask = p.GetBrowser().MainFrame.EvaluateScriptAsync(checkFormScript);
                                            formCheckTask.Wait(3000);

                                            if (formCheckTask.IsCompleted && formCheckTask.Result != null && formCheckTask.Result.Success)
                                            {
                                                dynamic formInfo = formCheckTask.Result.Result;
                                                if (formInfo != null)
                                                {
                                                    Debug.WriteLine($"页面检测结果: URL={formInfo.url}, 表单={formInfo.hasForm}, 用户名={formInfo.hasUsername}, 密码={formInfo.hasPassword}, 提交按钮={formInfo.hasSubmitBtn}");
                                                    Debug.WriteLine($"表单信息: Action={formInfo.formAction}, Method={formInfo.formMethod}, 按钮数量={formInfo.buttonCount}");

                                                    // 输出所有找到的按钮信息
                                                    if (formInfo.buttons != null)
                                                    {
                                                        var buttons = formInfo.buttons;
                                                        Debug.WriteLine("页面上的按钮列表:");
                                                        for (int i = 0; i < buttons.Count; i++)
                                                        {
                                                            var btn = buttons[i];
                                                            Debug.WriteLine($"  按钮{i}: {btn.tag}, type={btn.type}, class={btn.className}, id={btn.id}, text='{btn.text}', hasOnclick={btn.hasOnclick}");
                                                        }
                                                    }

                                                    if (formInfo.hasForm && (formInfo.hasUsername || formInfo.hasPassword))
                                                    {
                                                        // 只在还没有填写过表单时才进行填写，避免重复聚焦影响用户输入验证码
                                                        if (!hasFilledLoginForm)
                                                        {
                                                            Debug.WriteLine("检测到登录表单，开始自动填写");

                                                            // 填写用户名 - Vue.js兼容版本
                                                            var fillUsernameScript = $@"
                                                                (function() {{
                                                                    var usernameInput = document.querySelector('input[name=""username""], input[type=""text""], input[placeholder*=""帐""], input[placeholder*=""账""], input[placeholder*=""用户""]');
                                                                    if (usernameInput) {{
                                                                        // 先聚焦到输入框
                                                                        usernameInput.focus();
                                                                        
                                                                        // 清空现有值
                                                                        usernameInput.value = '';
                                                                        
                                                                        // 触发Vue.js兼容的事件序列
                                                                        usernameInput.dispatchEvent(new Event('focus', {{ bubbles: true }}));
                                                                        usernameInput.dispatchEvent(new Event('click', {{ bubbles: true }}));
                                                                        
                                                                        // 模拟逐字符输入
                                                                        var username = '{name}';
                                                                        for (var i = 0; i < username.length; i++) {{
                                                                            usernameInput.value += username[i];
                                                                            
                                                                            // 为每个字符触发键盘事件
                                                                            var keyCode = username.charCodeAt(i);
                                                                            usernameInput.dispatchEvent(new KeyboardEvent('keydown', {{ 
                                                                                bubbles: true, 
                                                                                keyCode: keyCode,
                                                                                which: keyCode,
                                                                                key: username[i]
                                                                            }}));
                                                                            
                                                                            usernameInput.dispatchEvent(new Event('input', {{ 
                                                                                bubbles: true, 
                                                                                data: username[i]
                                                                            }}));
                                                                            
                                                                            usernameInput.dispatchEvent(new KeyboardEvent('keyup', {{ 
                                                                                bubbles: true, 
                                                                                keyCode: keyCode,
                                                                                which: keyCode,
                                                                                key: username[i]
                                                                            }}));
                                                                        }}
                                                                        
                                                                        // 完成输入后的事件
                                                                        usernameInput.dispatchEvent(new Event('change', {{ bubbles: true }}));
                                                                        usernameInput.dispatchEvent(new Event('blur', {{ bubbles: true }}));
                                                                        
                                                                        // 检查Vue.js或其他框架的特殊属性
                                                                        if (usernameInput._vei || usernameInput.__vue__ || usernameInput.__reactInternalInstance) {{
                                                                            // 如果检测到Vue或React，触发额外的事件
                                                                            var customEvent = new CustomEvent('vue-input', {{ 
                                                                                bubbles: true, 
                                                                                detail: {{ value: username }}
                                                                            }});
                                                                            usernameInput.dispatchEvent(customEvent);
                                                                        }}
                                                                        
                                                                        return 'filled username with Vue compatibility: ' + (usernameInput.placeholder || usernameInput.name);
                                                                    }}
                                                                    return 'username input not found';
                                                                }})();
                                                            ";

                                                            var usernameTask = p.GetBrowser().MainFrame.EvaluateScriptAsync(fillUsernameScript);
                                                            usernameTask.Wait(3000);

                                                            string usernameResult = "unknown";
                                                            if (usernameTask.IsCompleted && usernameTask.Result != null && usernameTask.Result.Success)
                                                            {
                                                                usernameResult = usernameTask.Result.Result?.ToString() ?? "no result";
                                                            }
                                                            Debug.WriteLine($"用户名填写结果: {usernameResult}");

                                                            Thread.Sleep(500);

                                                            // 填写密码 - Vue.js兼容版本
                                                            var fillPasswordScript = $@"
                                                                (function() {{
                                                                    var passwordInput = document.querySelector('input[name=""password""], input[type=""password""], input[placeholder*=""密码""]');
                                                                    if (passwordInput) {{
                                                                        // 先聚焦到输入框
                                                                        passwordInput.focus();
                                                                        
                                                                        // 清空现有值
                                                                        passwordInput.value = '';
                                                                        
                                                                        // 触发Vue.js兼容的事件序列
                                                                        passwordInput.dispatchEvent(new Event('focus', {{ bubbles: true }}));
                                                                        passwordInput.dispatchEvent(new Event('click', {{ bubbles: true }}));
                                                                        
                                                                        // 模拟逐字符输入
                                                                        var password = '{pass}';
                                                                        for (var i = 0; i < password.length; i++) {{
                                                                            passwordInput.value += password[i];
                                                                            
                                                                            // 为每个字符触发键盘事件
                                                                            var keyCode = password.charCodeAt(i);
                                                                            passwordInput.dispatchEvent(new KeyboardEvent('keydown', {{ 
                                                                                bubbles: true, 
                                                                                keyCode: keyCode,
                                                                                which: keyCode,
                                                                                key: password[i]
                                                                            }}));
                                                                            
                                                                            passwordInput.dispatchEvent(new Event('input', {{ 
                                                                                bubbles: true, 
                                                                                data: password[i]
                                                                            }}));
                                                                            
                                                                            passwordInput.dispatchEvent(new KeyboardEvent('keyup', {{ 
                                                                                bubbles: true, 
                                                                                keyCode: keyCode,
                                                                                which: keyCode,
                                                                                key: password[i]
                                                                            }}));
                                                                        }}
                                                                        
                                                                        // 完成输入后的事件
                                                                        passwordInput.dispatchEvent(new Event('change', {{ bubbles: true }}));
                                                                        passwordInput.dispatchEvent(new Event('blur', {{ bubbles: true }}));
                                                                        
                                                                        // 检查Vue.js或其他框架的特殊属性
                                                                        if (passwordInput._vei || passwordInput.__vue__ || passwordInput.__reactInternalInstance) {{
                                                                            // 如果检测到Vue或React，触发额外的事件
                                                                            var customEvent = new CustomEvent('vue-input', {{ 
                                                                                bubbles: true, 
                                                                                detail: {{ value: password }}
                                                                            }});
                                                                            passwordInput.dispatchEvent(customEvent);
                                                                        }}
                                                                        
                                                                        return 'filled password with Vue compatibility: ' + (passwordInput.placeholder || passwordInput.name);
                                                                    }}
                                                                    return 'password input not found';
                                                                }})();
                                                            ";

                                                            var passwordTask = p.GetBrowser().MainFrame.EvaluateScriptAsync(fillPasswordScript);
                                                            passwordTask.Wait(2000);

                                                            string passwordResult = "unknown";
                                                            if (passwordTask.IsCompleted && passwordTask.Result != null && passwordTask.Result.Success)
                                                            {
                                                                passwordResult = passwordTask.Result.Result?.ToString() ?? "no result";
                                                            }
                                                            Debug.WriteLine($"密码填写结果: {passwordResult}");

                                                            // 标记表单已填写完成，避免重复填写和聚焦
                                                            hasFilledLoginForm = true;
                                                            Debug.WriteLine("用户名和密码填写完成，设置标志位防止重复填写");
                                                        }
                                                        
                                                        // 检查是否有验证码（每次循环都检测，不管表单是否已填写）
                                                        Thread.Sleep(500);

                                                        // 检查是否有验证码
                                                        var captchaCheckScript = @"
                                                            (function() {
                                                                var captchaInput = document.querySelector('input[name=""captcha""], input[name=""verifycode""], input[placeholder*=""验证""], input[placeholder*=""code""]');
                                                                var captchaImg = document.querySelector('img[src*=""captcha""], img[src*=""verify""], .captcha-img');
                                                                
                                                                // 检查所有可能的验证码相关元素
                                                                var allInputs = document.querySelectorAll('input');
                                                                var possibleCaptcha = [];
                                                                var actualCaptchaInput = null;
                                                                var actualCaptchaValue = '';
                                                                
                                                                for(var i = 0; i < allInputs.length; i++) {
                                                                    var input = allInputs[i];
                                                                    var placeholder = input.placeholder || '';
                                                                    var name = input.name || '';
                                                                    var id = input.id || '';
                                                                    
                                                                    // 检查是否可能是验证码字段
                                                                    var isCaptcha = placeholder.indexOf('验证') > -1 || 
                                                                                   placeholder.indexOf('code') > -1 || 
                                                                                   placeholder.indexOf('Code') > -1 ||
                                                                                   name.indexOf('captcha') > -1 || 
                                                                                   name.indexOf('verify') > -1 ||
                                                                                   name.indexOf('code') > -1 ||
                                                                                   id.indexOf('captcha') > -1 ||
                                                                                   id.indexOf('verify') > -1 ||
                                                                                   id.indexOf('code') > -1;
                                                                    
                                                                    if (isCaptcha) {
                                                                        var fieldInfo = {
                                                                            name: name,
                                                                            id: id,
                                                                            placeholder: placeholder,
                                                                            value: input.value,
                                                                            type: input.type,
                                                                            length: input.value ? input.value.length : 0
                                                                        };
                                                                        possibleCaptcha.push(fieldInfo);
                                                                        
                                                                        // 如果还没找到主验证码输入框，或者当前输入框有值，则设为主输入框
                                                                        if (!actualCaptchaInput || (input.value && input.value.length > 0)) {
                                                                            actualCaptchaInput = input;
                                                                            actualCaptchaValue = input.value || '';
                                                                        }
                                                                    }
                                                                }
                                                                
                                                                // 如果没有找到特定的验证码输入框，但找到了主验证码输入框
                                                                if (!captchaInput && actualCaptchaInput) {
                                                                    captchaInput = actualCaptchaInput;
                                                                }
                                                                
                                                                // 获取最终的验证码值
                                                                var finalCaptchaValue = '';
                                                                if (captchaInput) {
                                                                    finalCaptchaValue = captchaInput.value || '';
                                                                } else if (actualCaptchaValue) {
                                                                    finalCaptchaValue = actualCaptchaValue;
                                                                }
                                                                
                                                                return {
                                                                    hasCaptcha: !!captchaInput || possibleCaptcha.length > 0,
                                                                    hasCaptchaImg: !!captchaImg,
                                                                    captchaValue: finalCaptchaValue,
                                                                    captchaLength: finalCaptchaValue.length,
                                                                    possibleCaptchaCount: possibleCaptcha.length,
                                                                    possibleCaptcha: possibleCaptcha,
                                                                    primaryCaptchaField: actualCaptchaInput ? {
                                                                        name: actualCaptchaInput.name,
                                                                        id: actualCaptchaInput.id,
                                                                        placeholder: actualCaptchaInput.placeholder,
                                                                        value: actualCaptchaInput.value
                                                                    } : null
                                                                };
                                                            })();
                                                        ";

                                                        var captchaTask = p.GetBrowser().MainFrame.EvaluateScriptAsync(captchaCheckScript);
                                                        captchaTask.Wait(2000);

                                                        bool canSubmit = true;
                                                        if (captchaTask.IsCompleted && captchaTask.Result != null && captchaTask.Result.Success)
                                                        {
                                                            dynamic captchaInfo = captchaTask.Result.Result;
                                                            if (captchaInfo != null)
                                                            {
                                                                Debug.WriteLine($"验证码检测: 有验证码={captchaInfo.hasCaptcha}, 有验证码图片={captchaInfo.hasCaptchaImg}, 可能的验证码字段数={captchaInfo.possibleCaptchaCount}");
                                                                Debug.WriteLine($"验证码值='{captchaInfo.captchaValue}', 长度={captchaInfo.captchaLength}");
                                                                
                                                                // 显示主验证码字段信息
                                                                if (captchaInfo.primaryCaptchaField != null)
                                                                {
                                                                    var primary = captchaInfo.primaryCaptchaField;
                                                                    Debug.WriteLine($"主验证码字段: name='{primary.name}', id='{primary.id}', placeholder='{primary.placeholder}', value='{primary.value}'");
                                                                }
                                                                
                                                                if (captchaInfo.possibleCaptcha != null && captchaInfo.possibleCaptchaCount > 0)
                                                                {
                                                                    var possibleCaptcha = captchaInfo.possibleCaptcha;
                                                                    Debug.WriteLine("所有可能的验证码字段:");
                                                                    for (int i = 0; i < possibleCaptcha.Count; i++)
                                                                    {
                                                                        var field = possibleCaptcha[i];
                                                                        Debug.WriteLine($"  验证码字段{i}: name={field.name}, id={field.id}, placeholder={field.placeholder}, value='{field.value}', length={field.length}, type={field.type}");
                                                                    }
                                                                }

                                                                if (captchaInfo.hasCaptcha)
                                                                {
                                                                    Debug.WriteLine("检测到验证码，等待用户输入...");
                                                                    // 检查验证码是否已填写完整（必须恰好4个字符）
                                                                    string captchaValue = captchaInfo.captchaValue?.ToString();
                                                                    Debug.WriteLine($"当前验证码值: '{captchaValue}' (长度: {captchaValue?.Length ?? 0})");
                                                                    
                                                                    if (string.IsNullOrEmpty(captchaValue))
                                                                    {
                                                                        canSubmit = false;
                                                                        Debug.WriteLine("验证码为空，等待用户输入");
                                                                    }
                                                                    else if (captchaValue.Length != 4)
                                                                    {
                                                                        canSubmit = false;
                                                                        Debug.WriteLine($"验证码长度不正确（当前{captchaValue.Length}位，需要4位），等待用户输入完整验证码");
                                                                    }
                                                                    else
                                                                    {
                                                                        Debug.WriteLine("验证码已填写完整（4位），可以提交");
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    Debug.WriteLine("未检测到验证码字段，可以直接提交");
                                                                }
                                                            }
                                                        }
                                                        else
                                                        {
                                                            Debug.WriteLine("验证码检测失败，假设可以提交");
                                                        }

                                                        // 如果所有字段都已填写，点击登录按钮
                                                        if (canSubmit)
                                                        {
                                                            var submitScript = @"
                                                            (function() {
                                                                // 优先查找常见的提交按钮
                                                                var submitBtn = document.querySelector('input[type=""submit""], button[type=""submit""]');
                                                                if (submitBtn) {
                                                                    submitBtn.click();
                                                                    return 'clicked submit input/button';
                                                                }
                                                                
                                                                // 查找包含登录文字的按钮
                                                                var buttons = document.querySelectorAll('button, input[type=""button""], a.btn, .btn');
                                                                for (var i = 0; i < buttons.length; i++) {
                                                                    var btn = buttons[i];
                                                                    var text = btn.textContent || btn.value || btn.innerText || '';
                                                                    if (text.indexOf('登录') > -1 || text.indexOf('Login') > -1 || text.indexOf('登陆') > -1) {
                                                                        btn.click();
                                                                        return 'clicked text button: ' + text;
                                                                    }
                                                                }
                                                                
                                                                // 查找特定类名的按钮
                                                                var classButtons = document.querySelectorAll('.btn-login, #loginBtn, .login-btn, .submit-btn, .login-submit');
                                                                if (classButtons.length > 0) {
                                                                    classButtons[0].click();
                                                                    return 'clicked class button';
                                                                }
                                                                
                                                                // 查找表单中的最后一个按钮（通常是提交按钮）
                                                                var form = document.querySelector('form');
                                                                if (form) {
                                                                    var formButtons = form.querySelectorAll('button, input[type=""button""], input[type=""submit""]');
                                                                    if (formButtons.length > 0) {
                                                                        var lastBtn = formButtons[formButtons.length - 1];
                                                                        lastBtn.click();
                                                                        return 'clicked last form button';
                                                                    }
                                                                }
                                                                
                                                                // 尝试直接提交表单
                                                                if (form) {
                                                                    form.submit();
                                                                    return 'submitted form directly';
                                                                }
                                                                
                                                                // 查找任何可点击的元素，包含登录相关文字
                                                                var allElements = document.querySelectorAll('*');
                                                                for (var i = 0; i < allElements.length; i++) {
                                                                    var el = allElements[i];
                                                                    var text = el.textContent || el.innerText || '';
                                                                    if ((text.indexOf('登录') > -1 || text.indexOf('Login') > -1) && 
                                                                        (el.onclick || el.style.cursor === 'pointer' || el.tagName === 'A')) {
                                                                        el.click();
                                                                        return 'clicked element with login text: ' + el.tagName;
                                                                    }
                                                                }
                                                                
                                                                return 'no submit button found';
                                                            })();
                                                        ";

                                                            var submitTask = p.GetBrowser().MainFrame.EvaluateScriptAsync(submitScript);
                                                            submitTask.Wait(3000);

                                                            string clickResult = "unknown";
                                                            if (submitTask.IsCompleted && submitTask.Result != null && submitTask.Result.Success)
                                                            {
                                                                clickResult = submitTask.Result.Result?.ToString() ?? "no result";
                                                            }

                                                            Debug.WriteLine($"登录按钮点击结果: {clickResult}");
                                                            Thread.Sleep(2000);
                                                        }
                                                    }
                                                }
                                            }

                                            // 检查登录是否成功
                                            if (p.UrlCurrent != null)
                                            {
                                                if (p.UrlCurrent.IndexOf("Home/Index") != -1 ||
                                                    p.UrlCurrent.IndexOf("index") != -1 ||
                                                    p.UrlCurrent.IndexOf("main") != -1)
                                                {
                                                    Debug.WriteLine("登录成功！");
                                                    isLoginSuccess = true;
                                                    break;
                                                }
                                            }

                                            // 检查是否有登录错误信息
                                            var errorCheckScript = @"
                                            (function() {
                                                var errorMsg = document.querySelector('.error, .alert-error, .login-error, [class*=""error""]');
                                                return errorMsg ? errorMsg.textContent || errorMsg.innerText : '';
                                            })();
                                        ";

                                            var errorTask = p.GetBrowser().MainFrame.EvaluateScriptAsync(errorCheckScript);
                                            errorTask.Wait(1000);

                                            if (errorTask.IsCompleted && errorTask.Result != null && errorTask.Result.Success)
                                            {
                                                string errorText = errorTask.Result.Result?.ToString();
                                                if (!string.IsNullOrEmpty(errorText) &&
                                                    (errorText.Contains("错误") || errorText.Contains("失败") || errorText.Contains("invalid")))
                                                {
                                                    Debug.WriteLine($"登录失败: {errorText}");
                                                }
                                            }
                                        }
                                        else if (p.UrlCurrent.IndexOf("Member/Agreement") != -1)
                                        {
                                            Debug.WriteLine("检测到协议页面，开始自动点击同意按钮");
                                            
                                            // 等待页面完全加载
                                            Thread.Sleep(1000);
                                            
                                            // 检测并点击同意按钮
                                            var agreeButtonScript = @"
                                                (function() {
                                                    var agreeButton = null;
                                                    
                                                    var submitButtons = document.querySelectorAll('.submit_btn, a.submit_btn');
                                                    for (var i = 0; i < submitButtons.length; i++) {
                                                        var btn = submitButtons[i];
                                                        var text = btn.textContent || btn.innerText || '';
                                                        if (text.trim() === '同意') {
                                                            agreeButton = btn;
                                                            break;
                                                        }
                                                    }
                                                    
                                                    if (!agreeButton) {
                                                        var links = document.querySelectorAll('a[href]');
                                                        for (var i = 0; i < links.length; i++) {
                                                            var link = links[i];
                                                            var href = link.getAttribute('href') || '';
                                                            var text = link.textContent || link.innerText || '';
                                                            if (href === '/Home/Index' && text.trim() === '同意') {
                                                                agreeButton = link;
                                                                break;
                                                            }
                                                        }
                                                    }
                                                    
                                                    if (!agreeButton) {
                                                        var pageBtns = document.querySelector('.page_btns');
                                                        if (pageBtns) {
                                                            var firstBtn = pageBtns.querySelector('a, button');
                                                            if (firstBtn) {
                                                                var text = firstBtn.textContent || firstBtn.innerText || '';
                                                                if (text.indexOf('同意') > -1) {
                                                                    agreeButton = firstBtn;
                                                                }
                                                            }
                                                        }
                                                    }
                                                    
                                                    var allButtons = document.querySelectorAll('a, button, .btn, .submit_btn');
                                                    var buttonInfo = [];
                                                    for (var i = 0; i < allButtons.length; i++) {
                                                        var btn = allButtons[i];
                                                        buttonInfo.push({
                                                            tag: btn.tagName,
                                                            className: btn.className || 'none',
                                                            href: btn.href || 'none',
                                                            text: (btn.textContent || btn.innerText || '').trim()
                                                        });
                                                    }
                                                    
                                                    var result = {
                                                        found: !!agreeButton,
                                                        buttonCount: allButtons.length,
                                                        buttons: buttonInfo
                                                    };
                                                    
                                                    if (agreeButton) {
                                                        agreeButton.click();
                                                        result.clicked = true;
                                                        result.clickedButton = {
                                                            tag: agreeButton.tagName,
                                                            className: agreeButton.className,
                                                            href: agreeButton.href || 'none',
                                                            text: agreeButton.textContent || agreeButton.innerText || ''
                                                        };
                                                    } else {
                                                        result.clicked = false;
                                                    }
                                                    
                                                    return result;
                                                })();
                                            ";
                                            
                                            var agreeTask = p.GetBrowser().MainFrame.EvaluateScriptAsync(agreeButtonScript);
                                            agreeTask.Wait(3000);
                                            
                                            if (agreeTask.IsCompleted && agreeTask.Result != null && agreeTask.Result.Success)
                                            {
                                                dynamic agreeResult = agreeTask.Result.Result;
                                                if (agreeResult != null)
                                                {
                                                    Debug.WriteLine($"同意按钮检测结果: 找到={agreeResult.found}, 已点击={agreeResult.clicked}, 按钮总数={agreeResult.buttonCount}");
                                                    
                                                    // 输出所有找到的按钮信息
                                                    if (agreeResult.buttons != null)
                                                    {
                                                        var buttons = agreeResult.buttons;
                                                        Debug.WriteLine("协议页面按钮列表:");
                                                        for (int i = 0; i < buttons.Count; i++)
                                                        {
                                                            var btn = buttons[i];
                                                            Debug.WriteLine($"  按钮{i}: {btn.tag}, class={btn.className}, href={btn.href}, text='{btn.text}'");
                                                        }
                                                    }
                                                    
                                                    if (agreeResult.clicked && agreeResult.clickedButton != null)
                                                    {
                                                        var clickedBtn = agreeResult.clickedButton;
                                                        Debug.WriteLine($"成功点击同意按钮: {clickedBtn.tag}, class={clickedBtn.className}, href={clickedBtn.href}, text='{clickedBtn.text}'");
                                                        
                                                        // 等待页面跳转
                                                        Thread.Sleep(2000);
                                                    }
                                                    else
                                                    {
                                                        Debug.WriteLine("未找到或无法点击同意按钮");
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                Debug.WriteLine("同意按钮检测脚本执行失败");
                                            }
                                        }
                                        else if(p.UrlCurrent.IndexOf("Home/Index") != -1)
                                        {
                                            Debug.WriteLine($"登录成功");
                                            break;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Debug.WriteLine($"自动登录过程中出现异常: {ex.Message}");
                                    }
                                    Thread.Sleep(1000);
                                    Debug.Write("等待登录完成!");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.Write("LoginAsync::异常!退出");
                            response = false;
                        }
                        finally
                        {
                            cts = null;
                            //p.OnJSDialog = null;
                            Debug.Write("登录完成!");
                        }

                        return response;
                    }));

                    //释放线程参数
                    cts = null;
                }, this);
            }
            Debug.WriteLine("LoginAsync::退出");
            return 0;
        }

        private void OnJSDialogLogin(IWebBrowser chromiumWebBrowser, IBrowser browser, string originUrl, CefJsDialogType dialogType, string messageText, string defaultPromptText)
        {
            if (dialogType == CefJsDialogType.Alert)
            {
                if (messageText.IndexOf("验证码错误") != -1)
                {

                }
                else if (messageText.IndexOf("账号或密码错误") != -1)
                {

                }
            }
        }

        public bool GetCurrentOpenLotteryData()
        {
            return betApi.GetCurrentOpenLotteryData();
        }

        public bool GetUserInfoUpdata()
        {
            return betApi.GetUserInfoUpdata();
        }

        public BetStatus Bet(BetStandardOrderList items)
        {
            BetStatus status = BetStatus.未知;
            if (items.Count <= 0)
                return BetStatus.没有数据;

            int issueid = items[0].IssueId;
            Debug.WriteLine($"::Bet-honghai::begin::{issueid}");
            Debug.WriteLine($"Bet::Sort::{JsonConvert.SerializeObject(items)}");
            items.Sort(new BetStandardOrderComparer());

            //合并订单
            BetStandardOrderList betitems = new BetStandardOrderList();
            foreach (var bsOrder in items)
            {
                var last = betitems.LastOrDefault();
                if (last == null)
                {
                    betitems.Add(bsOrder);
                }
                else
                {
                    if (last.car == bsOrder.car && last.play == bsOrder.play)
                    {
                        last.moneySum += bsOrder.moneySum;
                    }
                    else
                    {
                        betitems.Add(bsOrder);
                    }
                }
            }
            Debug.WriteLine($"Bet::合成包");
            int sum = betitems.GetAmountTatol();
            //dynamic postPackect = new ExpandoObject();
            StringBuilder postPackect = new StringBuilder(128);

            //合成发送包
            try
            {
                postPackect.Append($"xtype=make");
                //postPackect.Append($"&pstr={this.sid}");


                List<dynamic> bets = new List<dynamic>();
               // StringBuilder userdata_buf = new StringBuilder(128);
                for (int i = 0; i < betitems.Count; i++)
                {
                    var ibettem = betitems[i];
                    var ods = _Odds.GetOdds(ibettem.car, ibettem.play);     //填充赔率

                    //动态生成对象.
                    dynamic betdata = new ExpandoObject();
                    betdata.pid = ods.oddsName;
                    betdata.je = ibettem.moneySum.ToString();
                    betdata.name = ibettem.play.ToString().ToUnicodeCodes();
                    betdata.peilv1 = ods.odds.ToString("F2").ToUnicodeCodes();
                    betdata.classx = ods.carName.ToUnicodeCodes();
                    betdata.con = "";
                    betdata.bz = "";
                    bets.Add(betdata);

                    //userdata_buf.Append(ods.carName + " ");
                }

                string arrbet = JsonConvert.SerializeObject(bets);
                postPackect.Append($"&pstr={arrbet}");
                postPackect.Append($"&abcd={abcd}");
                postPackect.Append($"&ab={ab}");
                postPackect.Append($"&bid=1-6");

                string postdata = postPackect.ToString();
                //http://hy.168bingo.top/uxj/makelib.php
                string url = $"{urlRoot}/uxj/makelib.php";
                LxHttpHelper http = new LxHttpHelper();
                HttpItem item = new HttpItem()
                {
                    URL = url,
                    Method = "POST",
                    Cookie = browser.Cookies,
                    Postdata = postdata,
                    Accept = "application/json, text/javascript, */*; q=0.01", //"application/json, text/javascript, */*; q=0.01",
                    UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36",
                    ContentType = "application/x-www-form-urlencoded;charset=UTF-8",   //这个很重要, 没这个, 服务器就无法解析注单
                };

                HttpResult hr = null;
                try
                {
                    hr = http.GetHtml(item);
                    JObject jResult = JObject.Parse(hr.Html);
                    string succeed = jResult["status"].ToString();
                    if (succeed == "success")
                    {
                        if(jResult.ContainsKey("message"))
                        {
                            var msgs = jResult["message"].ToArray();
                            if(msgs.Count() > 0)
                            {
                                var msg = msgs[0];
                                JObject jmsg = JObject.FromObject(msg);
                                if(jmsg.ContainsKey("err"))
                                {
                                    status = BetStatus.失败;
                                    string err = jmsg["err"].ToString().UnicodeCodesToString();
                                    loger(Log.Create($"Bet::POST::{betSiteType}失败::{err}", $"hr={hr.Html}"));
                                    return status;
                                }
                            }
                        }

                        loger(Log.Create($"Bet::POST::{betSiteType}成功", $"hr={hr.Html}"));
                        status = BetStatus.成功;
                    }
                    else
                    {
                        loger(Log.Create($"Bet::POST::{betSiteType}失败", $"hr={hr.Html}"));
                        status = BetStatus.失败;
                    }
                }
                catch (Exception ex)
                {
                    loger(Log.Create($"Bet::POST::{betSiteType}异常", $"ex={ex.Message}"));
                    status = BetStatus.异常;
                    string html = "";
                    if (hr != null)
                        html = hr.Html;
                    loger(Log.Create($"Bet::POST::{betSiteType}POST返回异常", $"hr={html},errmsg={ex.Message}"));
                }
            }
            catch (Exception ex)
            {
                //记录异常信息
                _Odds.isUpdata = false;
                string jsonitem = JsonConvert.SerializeObject(items);
                loger(Log.Create($"Bet::错误::{betSiteType}赔率获取失败", $"BetStandardOrderList={jsonitem},errmsg={ex.Message}"));
                return BetStatus.赔率获取失败;
            }


            return status;
        }

        public OddsBingo GetOdds(CarNumEnum car, BetPlayEnum play)
        {
            return betApi.GetOdds(car, play);
        }

        public void loger(Log log)
        {
            betApi.loger(log);
        }

        void IBetApi.ChromeBroser_FrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
            betApi.ChromeBroser_FrameLoadEnd(sender, e);
        }
    }
}
