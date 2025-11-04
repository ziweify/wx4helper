using CCWin.SkinControl;
using CefSharp;
using CsQuery.Engine.PseudoClassSelectors;
using F5BotV2.BetSite;
using F5BotV2.BetSite.Boter;
using F5BotV2.BetSite.yyz168;
using F5BotV2.Boter;
using F5BotV2.CefBrowser;
using F5BotV2.MainOpenLottery;
using F5BotV2.Model;
using F5BotV2.Model.BindSqlite;
using F5BotV2.Model.Setting;
using F5BotV2.Properties;
using F5BotV2.Wx;
using LxLib.LxNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.SessionState;
using System.Web.UI;
using System.Windows.Forms;

namespace F5BotV2.Main
{
    public static class MainConfigure
    {
        //窗口
        public static Form _view = new MainView();

        //private static LogView _logView = new LogView();    //日志窗口
        private static IBetBrowserBase _cefBetBrowser;       //cef打单的
        public static IBetBrowserBase cefBetBrowser { get { return _cefBetBrowser; } }
        private static OpenLotteryView _openLotteryView ;   //开奖结果
        private static IBetApi _betSite = null;              //打单的站点
        public static IBetApi betSite { get { return _betSite; } }
        private static BoterApi boterApi = BoterApi.GetInstance();      //机器人webApi
        private static Form _login = null;     //登录窗口

        //机器人主服务,本地服务!
        private static BoterServices _boterServices;
        public static BoterServices boterServices { get { return _boterServices; } }

        //sqllite.公共数据
        //private static BgLotteryDataBindlite bgLotteryDatalite = null;  //移动到BoterServices里面了
        //private static LogBindlite _loglite = null;  //移动到boterservices里面了


        //公共数据类都是公开方法
        public static AppLoginSettingModel appLoginSetting = new AppLoginSettingModel();
        private static AppMainSettingModel _appSetting = new AppMainSettingModel();
        public static AppMainSettingModel appSetting { get { return _appSetting; } }
        

        public static bool cefVisual
        {
            get {
                if (_cefBetBrowser == null)
                    return false;

                return _cefBetBrowser.visual;
            }
        }

        public static IBetApi GetBetApi(this Form view)
        {
            return betSite;
        }

        /// <summary>
        ///     成功返回 "" 失败返回初始化错误信息
        /// </summary>
        /// <returns></returns>
        public static string Initialzation(this Form view)
        {
            //初始化数据库相关
            //_loglite = new LogBindlite();

            //初始化app设定
            AppMainSettingDecorator setting = new AppMainSettingDecorator(appSetting);
            setting.Load();


            _boterServices = new BoterServices(view, _appSetting);       //机器人服务
            _openLotteryView = _boterServices.lotteryView;

            //初始化cef浏览器
            if (_cefBetBrowser == null)
            {
                cefInit();
            }

            //自动监听注册
            MainConfigure._boterServices.wxHelper.PropertyChanged += WxHelper_PropertyChanged;

            return "";
        }


        /// <summary>
        ///     窗口关闭, 程序结束
        /// </summary>
        public static void Closeing(this Form view)
        {
            AppMainSettingDecorator setting = new AppMainSettingDecorator(appSetting);
            setting.Save();
        }


        /// <summary>
        ///     微信助手.数据监听
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private static void WxHelper_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {

             switch(e.PropertyName)
            {
                case "avatar":
                    //下载头像数据, 并且绑定
                    Task.Factory.StartNew(() => {
                        LxHttpHelper http = new LxHttpHelper();
                        Image image = http.GetImage(_boterServices.wxHelper.avatar);
                        boterServices.wxHelper.WxHelperStatusChange?.Invoke(WxHelperStatus.微信自身头像下载完成, image);

                        //getView().setWxImage(new Func<SkinPictureBox, bool>((p) => {
                        //    p.Image = image;
                        //    p.SizeMode = PictureBoxSizeMode.Zoom;
                        //    return true;
                        //}));
                    });
                    break;
                default:
                    break;
            }
        }


        /// <summary>
        ///     初始化联系人显示
        /// </summary>
        /// <returns></returns>
        public static string InitWxContactsView(this Form view, SkinDataGridView dgv)
        {
            dgv.DataSource = _boterServices.wxHelper.wxContacts;
            dgv.AllowUserToAddRows = false;
            //dgv.ReadOnly = true;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;    //整行选择
            dgv.AllowUserToResizeColumns = false;
            dgv.AllowUserToResizeRows = false;
            dgv.RowHeadersVisible = false;
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            //dgv.RowsDefaultCellStyle.BackColor = Color.FromArgb(100, 52, 152, 219); //修改选择行背景色有问题
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(128, 128, 128);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.DarkBlue;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("宋体", 9, FontStyle.Bold);

            //禁止自动生成列，以下场景会用到：数据源的列超过需要展示的列
            dgv.AutoGenerateColumns = false;
            //右键DataGridView->编辑列->添加列->选择列的DataPropertyName属性，在此属性上指定需要绑定的列名，通过HeaderText设置控件上的标题。
            //比如，我需要绑定"Address"属性，那么在列的DataPropertyName上设置为"Address"就行了，HeaderText设置为"地址"

            dgv.Columns["nickname"].Visible = true;
            dgv.Columns["account"].Visible = true;         
            dgv.Columns["wxid"].Visible = true;
            dgv.Columns["sex"].Visible = true;

            dgv.Columns["id"].Visible = false;
            dgv.Columns["WeHeadUrl"].Visible = false;
            dgv.Columns["WeHeadUrlBig"].Visible = false;
            dgv.Columns["WeMainId"].Visible = false;
            dgv.Columns["WeMainNikeName"].Visible = false;
            dgv.Columns["WeHeadUrlBig"].Visible = false;
            dgv.Columns["city"].Visible = false;
            dgv.Columns["country"].Visible = false;
            dgv.Columns["remark"].Visible = false;
            dgv.Columns["AtTimestamp"].Visible = false;

            return "";
        }

        public static string InitDataGridView(this Form view, DataGridView dgv, object datasource, Func<DataGridView, bool> customization)
        {
            dgv.DataSource = datasource;
            dgv.AllowUserToAddRows = false;
            
            dgv.ReadOnly = true;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;    //整行选择
            dgv.AllowUserToResizeColumns = false;
            dgv.AllowUserToResizeRows = false;
            dgv.RowHeadersVisible = false;
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            //dgv.RowsDefaultCellStyle.BackColor = Color.FromArgb(100, 52, 152, 219); //修改选择行背景色有问题
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(128, 128, 128);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.DarkBlue;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("宋体", 9, FontStyle.Bold);

            //禁止自动生成列，以下场景会用到：数据源的列超过需要展示的列
            dgv.AutoGenerateColumns = false;

            customization?.Invoke(dgv);
            return "";
        }

        public static string InitWxGroupsView(this Form view, DataGridView dgv)
        {
            dgv.DataSource = _boterServices.wxHelper.wxGroups;
            dgv.AllowUserToAddRows = false;
            dgv.ReadOnly = true;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;    //整行选择
            dgv.AllowUserToResizeColumns = false;
            dgv.AllowUserToResizeRows = false;
            dgv.RowHeadersVisible = false;
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            //dgv.RowsDefaultCellStyle.BackColor = Color.FromArgb(100, 52, 152, 219); //修改选择行背景色有问题
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(128, 128, 128);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.DarkBlue;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("宋体", 9, FontStyle.Bold);

            //禁止自动生成列，以下场景会用到：数据源的列超过需要展示的列
            dgv.AutoGenerateColumns = false;
            //右键DataGridView->编辑列->添加列->选择列的DataPropertyName属性，在此属性上指定需要绑定的列名，通过HeaderText设置控件上的标题。
            //比如，我需要绑定"Address"属性，那么在列的DataPropertyName上设置为"Address"就行了，HeaderText设置为"地址"

            dgv.Columns["id"].Visible = false;
            dgv.Columns["wxid"].Visible = false;
            dgv.Columns["WeMainId"].Visible = false;
            var cell = dgv.Columns["nickname"]; cell.Width = 130;
            dgv.Columns["avatar"].Visible = false;
            dgv.Columns["manager_wxid"].Visible = false;
            dgv.Columns["AtTimestamp"].Visible = false;
            cell = dgv.Columns["is_manager"]; cell.Width = 46;
           // dgv.Columns["member_list"].Visible = false; //因为这里设置了没有保存, 所以没有这个字段
            return "";
        }

        public static LogBindlite GetLogBindLite(this Form view)
        {
            return boterServices.loglite;
        }

        //public static void OpenWx(this Form view)
        //{
           
        //    //Task.Factory.StartNew(() => { 
        //    //    Thread.Sleep(2000);
        //    //    _boterServices.wxHelper.FixWxVersion();
        //    //});
            
        //}


        private static void _cefBet_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch(e.PropertyName)
            {
                case "visual":
                    {
                        if(_cefBetBrowser.visual)
                        {
                            _view.BeginInvoke(new Action(() =>
                            {
                                appSetting.showBrowserText = "隐藏";
                            }));
                        }
                        else
                        {
                            _view.BeginInvoke(new Action(() =>
                            {
                                appSetting.showBrowserText = "显示";
                            }));
                        }
                        break;
                    }
                default:
                    break;
            }
        }

        public static Form getView(Form view = null)
        {
            if (view != null)
                _view = view;
            return _view;
        }

        public static Form getLogin(Form view = null)
        {
            if (view != null)
                _login = view;
            return _login;
        }

        public static BoterApi getBotApi()
        {
            return boterApi;
        }

        /// <summary>
        ///     登录盘口
        /// </summary>
        /// <param name="view"></param>
        /// <param name="siteName"></param>
        public static void LoginBetSite(this Form view, string siteName)
        {
            try
            {
                _boterServices.loglite.Add(Log.Create("盘口登录::准备", $"{siteName}"));
                var bs = (BetSiteType)Enum.Parse(typeof(BetSiteType), siteName);
                if (betSite == null) 
                {
                    _betSite = BetSiteFactory.Create(bs, _cefBetBrowser);
                }
               
                if (betSite != null)
                {
                    if (!string.IsNullOrEmpty(appSetting.panAddress))
                        betSite.SetRootUrl(appSetting.panAddress);
                    if (betSite.betSiteType.ToString() != siteName)
                    {
                        _betSite = BetSiteFactory.Create(bs, _cefBetBrowser);
                        if (!string.IsNullOrEmpty(appSetting.panAddress))
                            betSite.SetRootUrl(appSetting.panAddress);
                    }
                        
                    _boterServices.loglite.Add(Log.Create("盘口登录::开始", $"{siteName}::{betSite.urlRoot}::{appSetting.panUserName}::{appSetting.panUserPwd}"));
                    betSite.LoginAsync(appSetting.panUserName
                        , appSetting.panUserPwd
                        , _cefBetBrowser);
                }
            }
            catch (Exception ex)
            {
                _boterServices.loglite.Add(Log.Create("盘口登录::异常", $"ex.Message"));
                DebugOutput(view, $"LoginBetSite::异常::{ex.Message}");
            }
            finally
            {
                DebugOutput(view, $"LoginBetSite::完成");
            }
        }

        public static void DebugOutput(this Form view, string message)
        {
            Debug.WriteLine(message);
        }

        ///// <summary>
        /////     
        ///// </summary>
        ///// <param name="view"></param>
        ///// <returns>成功返回0, 失败返回错误代码!</returns>
        //public static async Task<int> loginAsync(this Form view)
        //{
        //    cefInit();

        //    //网站登录
        //    if(string.IsNullOrEmpty(appSetting.panUserName) || string.IsNullOrEmpty(appSetting.panUserPwd))
        //    {
        //        //601账密相关错误
        //        return 1601;
        //    }

        //    var retCOde = await betSite.LoginAsync(appSetting.panUserName, appSetting.panUserPwd, _cefBetBrowser);
        //    Debug.WriteLine("begin::退出");
        //    return retCOde;
        //}

        /// <summary>
        ///     
        /// </summary>
        /// <param name="'6iew"></param>
        /// <param name="show">当前状态</param>
        /// <returns></returns>
        public static bool cefShow(this Form view, bool show = true)
        {
            if(_cefBetBrowser == null)
            {
                cefInit();
            }
            

            if (show)
            {
                _cefBetBrowser.Show();
                
            }
            else
            {
                _cefBetBrowser.Hide();
                
            }
            return cefVisual;
        }

        public static void openResultShow(this Form view, bool show = true)
        {
            if(show)
            {
                _openLotteryView.Show();
            }
            else
            {
                _openLotteryView.Hide();
            }
        }

        private static void cefInit()
        {
            if (_cefBetBrowser == null)
            {
                if (Program.appMode == AppMode.普通模式)
                    _cefBetBrowser = new CefBrowserView(); //里面有隐藏加载代码
                else if(Program.appMode == AppMode.X模式)
                    _cefBetBrowser = new XCefBrowserView();
                _cefBetBrowser.PropertyChanged += _cefBet_PropertyChanged;
            }
        }
    }
}
