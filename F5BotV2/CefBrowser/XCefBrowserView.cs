using CCWin.SkinClass;
using CefSharp;
using CefSharp.Handler;
using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace F5BotV2.CefBrowser
{
    public partial class XCefBrowserView : Form
        , INotifyPropertyChanged
        , IBetBrowserBase
    {
        //浏览器对象
        public ChromiumWebBrowserExtend chromeBroser { get; set; }
        public bool isBrowserInit = false;  //每个进程cef只能初始化一次, 是进程
        public string Cookies { get {
                try
                {
                    StringBuilder sb = new StringBuilder(128);
                    foreach(var ck in cookies_dic)
                    {
                        if (sb.Length > 0)
                            sb.Append(";");
                        sb.Append($"{ck.Key}={ck.Value}");
                    }
                    return sb.ToString();
                }
                catch(Exception ex)
                {
                    Debug.WriteLine($"Cookies::异常={ex.Message}");
                }
                return "";
            } }
        private Dictionary<string, string> cookies_dic = new Dictionary<string, string>();
        private bool _visual = false;   //当前窗口是否可视

        public event PropertyChangedEventHandler PropertyChanged;


        public bool visual
        {
            get { return _visual; }
            set
            {
                if (_visual == value)
                    return;
                _visual = value;
                NotifyPropertyChanged(() => visual);
            }
        }


        public XCefBrowserView()
        {
            InitializeComponent();

            initCrf();
        }


        private void initCrf()
        {
            //参数设置
            if(!isBrowserInit)
            {
                CefSettings settings = new CefSettings();
                //settings.Locale = "en-US";
                //settings.CefCommandLineArgs.Add("disable-gpu", "1");    //去掉gpu, 否则可能显示有问题
                Cef.Initialize(settings);
                isBrowserInit = true;
            }


            //创建实例
            chromeBroser = new ChromiumWebBrowserExtend("");

            //chromeBroser = new ChromiumWebBrowser("") {
            //    //注册自定义事件
            //    JsDialogHandler = new F5JsDialogHandler(),  //自定义弹窗事件.这里要做个委托。把弹窗内容输出到外部检测
            //};
            chromeBroser.FrameLoadEnd += ChromeBroser_FrameLoadEnd;
            chromeBroser.LifeSpanHandler = new OpenPageSelf();

            //不需要释放的
            chromeBroser.OnUrlChange = OnUrlChangeHandler;

            //把浏览器放入容器中
            this.pnl_chromeBrowser.Controls.Add(chromeBroser);
            chromeBroser.Dock = DockStyle.Fill;

            //隐藏加载窗口
            //this.Opacity = 0;
            //this.ShowInTaskbar = false;
            //base.Show();
            //_visual = false;
        }

        private void OnUrlChangeHandler(string url)
        {
            tbx_url.Invoke(new Action(() => {
                tbx_url.Text = url;
                tbx_url.Enabled = true;
            }));
        }

        public void Show()
        {
            this.Opacity = 100;
            this.ShowInTaskbar = true;
            base.Show();
            visual = true;
        }

        public void Hide()
        {
            base.Hide();
            visual = false;
        }


        /// <summary>
        ///     重置浏览器
        /// </summary>
        public void ReSetBrowser()
        {
            if(chromeBroser != null)
            {
                chromeBroser.FrameLoadEnd -= ChromeBroser_FrameLoadEnd;
                chromeBroser.LifeSpanHandler = null;
                this.pnl_chromeBrowser.Controls.Remove(chromeBroser);
            }
            chromeBroser = new ChromiumWebBrowserExtend("");
            initCrf();
        }


        private void ChromeBroser_FrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
            try
            {
                //这个是错误的，页面应该不是这个
                //tbx_url.Invoke(new Action(() => { 
                //   tbx_url.Text =  e.Url;
                //    tbx_url.Enabled = true;
                //}));

                var cookieManager = CefSharp.Cef.GetGlobalCookieManager();
                CefCookieVisitor visitor = new CefCookieVisitor();
                visitor.SendCookie += Visitor_SendCookie; ;  //  注册获取cookie回调事件
                cookieManager.VisitAllCookies(visitor);

            }
            catch (Exception ex)
            {
                Debug.WriteLine("CefBrowserView::ChromeBroser_FrameLoadEnd => " + ex.Message);
            }
            Debug.WriteLine("CefBrowserView::FrameLoadEnd");
        }

        private void Visitor_SendCookie(Cookie obj)
        {
            // Cookies += obj.Name + "=" + obj.Value + ";";
            Debug.WriteLine($"CefBrowserView::cookie updata => {obj.Name} : {obj.Value}");
            cookies_dic[obj.Name] = obj.Value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="actionFinish">任务结束后的动作</param>
        public async Task<int> LoadUrlAsyn(string url, Func<ChromiumWebBrowserExtend, bool> finshAct)
        {
            tbx_url.Invoke(new Action(() => {
                tbx_url.Text = url;
                tbx_url.Enabled = false;
            }));
            //chromeBroser.Load(url);
            var loadresult = await chromeBroser.LoadUrlAsync(url);
            Debug.WriteLine("LoadUrlAsyn::load finish");
            try
            {
                Debug.WriteLine("LoadUrlAsyn::页面加载完成,执行委托!");
                finshAct?.Invoke(chromeBroser);
            }
            catch(Exception ex)
            {

            }
            Debug.WriteLine("LoadUrlAsyn::exit");
            return loadresult.ToInt32();
        }


        //开始
        private void btn_go_Click(object sender, EventArgs e)
        {
             chromeBroser.LoadUrlAsync(tbx_url.Text);
        }

        /// <summary>
        ///     弹出验证码窗口
        /// </summary>
        /// <param name="img"></param>
        /// <param name="updateYzmImage">刷新验证码</param>
        /// <returns></returns>
        //public DialogResult ShowCodeInput(Image img, out string ocrCode, Func<Image> updateYzmImage)
        //{
        //    //显示验证码窗口
        //    CefCodeInput codeView = new CefCodeInput(img, updateYzmImage);
        //    var result = codeView.ShowDialog();
        //    ocrCode = codeView.codeText;
        //    return result;
        //}

        /// <summary>
        ///     获取浏览器对象
        ///     返回值: 执行异常 返回-1, 其他异常, 自定义
        /// </summary>
        public int GetBrowser(Func<int> func)
        {
            try
            {
                return func.Invoke();
            }
            catch(Exception ex)
            {
                return -1;
            }
        }

        private void btnGetCookie_Click(object sender, EventArgs e)
        {
            MessageBox.Show(this.Cookies);
        }

        /// <summary>
        ///     禁止窗口关闭。销毁窗口, 必须要调用自定义的其他函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CefBrowserView_FormClosing(object sender, FormClosingEventArgs e)
        {
            //取消关闭操作
            e.Cancel = true;
            this.Hide();
        }

        public void NotifyPropertyChanged<T>(Expression<Func<T>> property)
        {
            if (PropertyChanged == null)
                return;

            var memberExpression = property.Body as MemberExpression;
            if (memberExpression == null)
                return;

            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(memberExpression.Member.Name));
        }

        private void skinButton1_Click(object sender, EventArgs e)
        {

        }

        private void skinButton2_Click(object sender, EventArgs e)
        {

        }
    }
}
