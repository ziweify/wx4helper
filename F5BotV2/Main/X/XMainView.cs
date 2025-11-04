using CCWin;
using CCWin.SkinClass;
using F5BotV2.BetSite.yyz168;
using F5BotV2.CefBrowser;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using F5BotV2.MainOpenLottery;
using System.Diagnostics;
using LxLib.LxNet;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using Newtonsoft.Json.Linq;
using CsQuery.EquationParser.Implementation;
using CCWin.SkinControl;
using System.Web.UI.WebControls;
using F5BotV2.Model;
using CefSharp.DevTools.Page;
using F5BotV2.Boter;
using System.Web;
using System.Text.RegularExpressions;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Button = System.Windows.Forms.Button;
using F5BotV2.Game.BinGou;
using F5BotV2.BetSite;
using LxLib.LxSys;
using CefSharp;
using CsQuery.Utility;
using F5BotV2.BetSite.Boter;
using System.Reflection;
using F5BotV2.Controls;
using F5BotV2.Wx;
using F5Bot.Main.X;
using Newtonsoft.Json;
using CefSharp.DevTools.Network;

namespace F5BotV2.Main
{
    public partial class XMainView : Form
    {
        private V2MemberOrderBindlite _orders_result;
        public XMainView()
        {
            InitializeComponent();
        }

        private void MainView_Load(object sender, EventArgs e)
        {
            this.Initialzation(); //里面有初始化微信接口

            //修改窗口标题
            //公开版
            string title = $"专业版{Assembly.GetExecutingAssembly().GetName().Version.ToString()} - 仅供测试娱乐! - 有效期 {BoterApi.GetInstance().loginApiResponse.data.c_off_time.ToString()}";
            this.airForm1.Text = title;
            this.Text = title;
            //内部版
            //this.Text = $"小花{Assembly.GetExecutingAssembly().GetName().Version.ToString()} - 仅供测试娱乐! - 有效期 {BoterApi.GetInstance().loginApiResponse.data.c_off_time.ToString()}";

            //tab_order_source.SelectedIndex = 0;
            //this.InitWxContactsView(dgv_WxContacts);
            //this.InitWxGroupsView(dgv_WxContacts);  //联系人,群

            //绑定日志表
            this.InitDataGridView(dgv_log
                , MainConfigure.boterServices.loglite
                , new Func<DataGridView, bool>((p) => {
                    var cell = p.Columns["id"]; if (cell != null){ cell.Width = 45;}
                    cell = p.Columns["User"]; if (cell != null) { cell.Width = 65; } 
                    cell = p.Columns["Action"]; if (cell != null) { cell.Width = 160; } 
                    cell = p.Columns["Context"]; if (cell != null) { cell.Width = 500; } 
                    cell = p.Columns["AtTimestamp"]; if (cell != null) { cell.Visible = false; }
                    cell = p.Columns["ustate"]; cell.Visible = false;
                    cell = p.Columns["uTime"]; cell.Visible = false;
                    cell = p.Columns["AtTimeString"]; cell.Visible = true; cell.Width = 120;
                    return true; 
                }));

            //开奖表
            this.InitDataGridView(dgv_opendata, MainConfigure.boterServices.lotteryDatas,
                new Func<DataGridView, bool>((p) => {

                    return true;
                })
                );

            //初始化会员表
            this.InitDataGridView(dgv_members
                , MainConfigure.boterServices.v2Memberbindlite
                , new Func<DataGridView, bool>((p) =>
                {
                    //p.DataSource = MainConfigure.boterServices.v2Memberbindlite;
                    var cell = p.Columns["id"];
                    if (cell != null)
                    {
                        cell.Width = 45;
                    }
                    cell = p.Columns["account"];
                    if (cell != null)
                    {
                        cell.Visible = false;
                    }
                    //群昵称 display_name
                    cell = p.Columns["city"]; if (cell != null) { cell.Visible = false; } //wxid
                    cell = p.Columns["wxid"]; if (cell != null) { cell.Visible = false; }
                    cell = p.Columns["GroupWxId"]; if (cell != null) { cell.Visible = false; } //State
                    cell = p.Columns["State"]; if (cell != null) { cell.Width = 69; }
                    cell = p.Columns["display_name"]; if (cell != null) { cell.Visible = false; }
                    cell = p.Columns["city"]; if (cell != null) { cell.DefaultCellStyle.Format = "0.00"; } //wxid
                    cell = p.Columns["Balance"]; if (cell != null) { cell.DefaultCellStyle.Format = "0.00"; } //wxid
                    cell = p.Columns["IncomeToday"]; if (cell != null) { cell.DefaultCellStyle.Format = "0.00"; }
                    cell = p.Columns["IncomeTotal"]; if (cell != null) { cell.DefaultCellStyle.Format = "0.00"; }

                    cell = p.Columns["country"];
                    if (cell != null)
                    {
                        cell.Visible = false;
                    }
                    cell = p.Columns["province"];
                    if (cell != null)
                    {
                        cell.Visible = false;
                    }
                    cell = p.Columns["remark"];
                    if (cell != null)
                    {
                        cell.Visible = false;
                    }
                    cell = p.Columns["sex"];
                    if (cell != null)
                    {
                        cell.Visible = false;
                    }
                    cell = p.Columns["avatar"];
                    if (cell != null)
                    {
                        cell.Visible = false; //群昵称
                    }
                    cell = p.Columns["群昵称"];
                    if (cell != null)
                    {
                        cell.Visible = false;
                    }
                    return true;
                }));

            //初始化数据表
            this.InitDataGridView(this.dgv_orders, MainConfigure
                .boterServices.v2memberOderbindlite
                , new Func<DataGridView, bool>((p) =>
                {
                    var cell = p.Columns["id"]; if (cell != null) { cell.Width = 45; }
                    cell = p.Columns["TimeStampBet"]; if (cell != null) { cell.Visible = false; }
                    cell = p.Columns["wxid"]; if (cell != null) { cell.Visible = false; }
                    cell = p.Columns["GroupWxId"]; if (cell != null) { cell.Visible = false; }
                    cell = p.Columns["account"]; if (cell != null) { cell.Visible = false; }
                    cell = p.Columns["IssueId"]; if (cell != null) { cell.Width = 65; }
                    cell = p.Columns["BetFronMoney"]; if (cell != null) { cell.Width = 60; cell.DefaultCellStyle.Format = "0.0"; }
                    cell = p.Columns["BetAfterMoney"]; if (cell != null) { cell.Width = 60; cell.DefaultCellStyle.Format = "0.0"; }
                    cell = p.Columns["Nums"]; if (cell != null) { cell.Width = 26; }
                    cell = p.Columns["Profit"]; if (cell != null) { cell.Width = 50; cell.DefaultCellStyle.Format = "0.0"; }
                    cell = p.Columns["AmountTotal"]; if (cell != null) { cell.Width = 50; cell.DefaultCellStyle.Format = "0.0"; }
                    //AmountTotal

                    cell = p.Columns["avatar"]; if (cell != null) { cell.Visible = false; }
                    cell = p.Columns["city"]; if (cell != null) { cell.Visible = false; }
                    cell = p.Columns["country"]; if (cell != null) { cell.Visible = false; }
                    cell = p.Columns["province"]; if (cell != null) { cell.Visible = false; }
                    cell = p.Columns["remark"]; if (cell != null) { cell.Visible = false; }
                    cell = p.Columns["sex"]; if (cell != null) { cell.Visible = false; }
                    return true;
                }));

           
            //充值表-未处理
            this.InitDataGridView(this.dgv_recharge_unprocessed
               , MainConfigure.boterServices.v2MemberCoinsBuySellbindliteUnProcessed
               , new Func<DataGridView, bool>((p) =>
               {
                   var cell = p.Columns["id"]; if (cell != null) { cell.Width = 0; }
                   cell = p.Columns["nickname"]; cell.Visible = true; cell.Width = 80;
                   cell = p.Columns["PayAction"]; cell.Visible = true; cell.Width = 46;
                   cell = p.Columns["Money"]; cell.Visible = true; cell.Width = 65;
                   cell = p.Columns["Timestring"]; cell.Visible = true; cell.Width = 120;
                   cell = p.Columns["PayStatus"]; cell.Visible = true; cell.Width = 90;
                   //-----------------------------------------
                   p.Columns["wxid"].Visible = false;
                   p.Columns["account"].Visible = false;
                   p.Columns["avatar"].Visible = false;
                   p.Columns["city"].Visible = false;
                   p.Columns["country"].Visible = false;
                   p.Columns["province"].Visible = false;
                   p.Columns["remark"].Visible = false;
                   p.Columns["sex"].Visible = false;
                   p.Columns["Timestamp"].Visible = false;
                   p.Columns["GroupWxId"].Visible = false;
                   return true;
               }));

            




            //微信用户数据
            tbxWxNikeName.DataBindings.Add(new Binding("Text", MainConfigure.boterServices.wxHelper, "nickname"));
            tbxWxId.DataBindings.Add(new Binding("Text", MainConfigure.boterServices.wxHelper, "wxid"));
            tbxWxAccount.DataBindings.Add(new Binding("Text", MainConfigure.boterServices.wxHelper, "account"));
            tbxWxPid.DataBindings.Add(new Binding("Text", MainConfigure.boterServices.wxHelper, "clientid"));

            //绑定群数据
            tbx_BindGroupName.SkinTxt.DataBindings.Add(new Binding("Text", MainConfigure.boterServices.groupBind, "nickname"));

            //盘数据
            btnShowBrowser.DataBindings.Add(new Binding("Text", MainConfigure.appSetting, "showBrowserText"));
            tbx_pan_user.SkinTxt.DataBindings.Add(new Binding("Text", MainConfigure.appSetting, "panUserName", true, DataSourceUpdateMode.OnPropertyChanged));
            btn_pan_address.SkinTxt.DataBindings.Add(new Binding("Text", MainConfigure.appSetting, "panAddress", true, DataSourceUpdateMode.OnPropertyChanged));
            tbx_pan_pwd.SkinTxt.DataBindings.Add(new Binding("Text", MainConfigure.appSetting, "panUserPwd", true, DataSourceUpdateMode.OnPropertyChanged));
            tbxtbx_pan_blance.SkinTxt.DataBindings.Add(new Binding("Text", MainConfigure.boterServices, "Amount", true, DataSourceUpdateMode.OnPropertyChanged));
            lbl_OrderInfo.DataBindings.Add(new Binding("Text", MainConfigure.boterServices, "panDescribe", true, DataSourceUpdateMode.OnPropertyChanged));

            //机器人配置数据
            tbxEndTime.SkinTxt.DataBindings.Add(new Binding("Text", MainConfigure.appSetting, "reduceCloseSeconds", true, DataSourceUpdateMode.OnPropertyChanged));
            tbxMax.SkinTxt.DataBindings.Add(new Binding("Text", MainConfigure.appSetting, "wxMaxBet", true, DataSourceUpdateMode.OnPropertyChanged));
            // tbxMin.SkinTxt.DataBindings.Add(new Binding("Text", MainConfigure.appSetting, "wxMinBet"));
            tbxMin.SkinTxt.DataBindings.Add("Text", MainConfigure.appSetting, "wxMinBet", true, DataSourceUpdateMode.OnPropertyChanged);
            tbxOdds.SkinTxt.DataBindings.Add(new Binding("Text", MainConfigure.appSetting, "wxOdds", true, DataSourceUpdateMode.OnPropertyChanged));

            cmb_panb_platform.DataBindings.Add(new Binding("Text", MainConfigure.appSetting, "platform", true, DataSourceUpdateMode.OnPropertyChanged));
            cmb_panb_platform.Text = MainConfigure.appSetting.platform;

            //结算方式
            chk_zsjs.DataBindings.Add(new Binding("Checked", MainConfigure.appSetting, "Zsjs", false, DataSourceUpdateMode.OnPropertyChanged));
            chk_zsxs.DataBindings.Add(new Binding("Checked", MainConfigure.appSetting, "Zsxs", false, DataSourceUpdateMode.OnPropertyChanged));


            //初始化本地查询表
            _orders_result = new V2MemberOrderBindlite(this);
            dgvOrdersSearch.DataSource = _orders_result;

            //服务注册事件
            //服务事件
            MainConfigure.boterServices.BoterStatusChange += BoterServices_BoterStatusChange;
            MainConfigure.boterServices.wxHelper.WxHelperStatusChange += WxHelperStatusHandle;
            MainConfigure.boterServices.Start();
        }

        private int BoterServices_BoterStatusChange(BoterStatus status, int issueid, BgLotteryData data)
        {
            if (issueid > 0)
            {
                if (ucLotteryDataLast.IssueId == issueid)
                {
                    if (data != null)
                    {
                        this.Invoke(new Action(() =>
                        {
                            ucLotteryDataLast.LotteryUpdata(data);
                        }));
                    }
                }
            }

            switch (status)
            {
                case BoterStatus.等待中:
                    this.Invoke(new Action(() =>
                    {
                        ucLotteryDataCur.SetStatus(status);
                    }));
                    break;
                case BoterStatus.封盘中:
                    this.Invoke(new Action(() =>
                    {
                        ucLotteryDataCur.SetStatus(status);
                    }));
                    break;
                case BoterStatus.开盘中:
                    this.Invoke(new Action(() =>
                    {
                        ucLotteryDataCur.SetStatus(status);
                    }));
                    break;
                case BoterStatus.开奖中:
                    break;
                case BoterStatus.期号变更:
                    var dataCur = new BgLotteryData()
                    {
                        IssueId = issueid,
                        opentime = BinGouHelper.getOpenDatetime(issueid).ToString(),
                    };
                    this.Invoke(new Action(() =>
                    {
                        ucLotteryDataCur.LotteryUpdata(dataCur);
                    }));

                    var dataLast = new BgLotteryData()
                    {
                        IssueId = issueid - 1,
                        opentime = BinGouHelper.getOpenDatetime(issueid - 1).ToString(),
                    };
                    this.Invoke(new Action(() =>
                    {
                        ucLotteryDataLast.LotteryUpdata(dataLast);
                    }));
                    break;
                default:
                    break;
            }

            return 1;
        }

        public int WxHelperStatusHandle(WxHelperStatus status, object value)
        {
            switch (status)
            {
                case WxHelperStatus.微信登入成功:
                    btnOpenWx.Enabled = true;
                    break;
                case WxHelperStatus.微信自身头像下载完成:
                    //getView().setWxImage(new Func<SkinPictureBox, bool>((p) => {
                    //    p.Image = image;
                    //    p.SizeMode = PictureBoxSizeMode.Zoom;
                    //    return true;
                    //}));
                    picWxImage.Image = (System.Drawing.Image)value;
                    picWxImage.SizeMode = PictureBoxSizeMode.Zoom;
                    break;
            }
            return 1;
        }


        public void setLotteryLast(Func<UcXLotteryDataLast, bool> func)
        {
            if (ucLotteryDataLast.InvokeRequired)
            {
                this.BeginInvoke(new Action(() =>
                {
                    func(this.ucLotteryDataLast);
                }));
            }
            else
            {
                func(this.ucLotteryDataLast);
            }
        }


        //public void setBuySellConis(Func<UcBuySellCoins, bool> func)
        //{
        //    if (ucBuySellCoins1.InvokeRequired)
        //    {
        //        this.BeginInvoke(new Action(() =>
        //        {
        //            func(this.ucBuySellCoins1);
        //        }));
        //    }
        //    else
        //    {
        //        func(this.ucBuySellCoins1);
        //    }
        //}


        private void btnBegin_Click(object sender, EventArgs e)
        {
            //选择哪个接口来登录

        }

        private void btnEnd_Click(object sender, EventArgs e)
        {
            //cef.LoadAsynUserFunction("https://www.qq.com", new Func<bool>(() => {
            //    MessageBox.Show("异步加载完成");
            //    btnBegin.Enabled = false;
            //    return true;
            //}));

            //测试
            YYZ2Member bet = new YYZ2Member();
            bool result = bet.SetRootUrl("https://111.com/");
        }

        private void btnShowBrowser_Click(object sender, EventArgs e)
        {
            //显示. 隐藏.cef
            this.cefShow(!MainConfigure.cefVisual);
        }

        private void btnOcrTest_Click(object sender, EventArgs e)
        {

            //查找进程是否有F5Browser.exe
            var pros = Process.GetProcessesByName("F5Browser.exe");
            if (pros.Count() > 0)
            {

            }
            else
            {
                Process p = new Process();
                p.StartInfo.FileName = "F5Browser.exe";
                //p.StartInfo.Arguments 
                p.Start();
            }
        }

        private void btnOpenLotteryResult_Click(object sender, EventArgs e)
        {
            //打开.开奖结果页面
            this.openResultShow();
        }

        private void btnPost_Click(object sender, EventArgs e)
        {
            LxHttpHelper http = new LxHttpHelper();

            //创建json post数据
            var postJson = new JObject();
            postJson["lotteryType"] = "";
            postJson["search"] = "username";
            postJson["like"] = "jkj115";
            postJson["betType"] = -1;
            postJson["status"] = 0;
            postJson["startDate"] = "2023-08-09";
            postJson["endDate"] = "2023-08-09";
            postJson["installments"] = "";

            HttpItem item = new HttpItem()
            {
                URL = "http://yyz2.666.macao-lottery.com/ServiceAPI/Betlist/QueryBetingByCondiction?uid=bNWC3ATbviO61eQpVOk9j&page=1",
                Method = "POST",
                Cookie = "ssid1=7166523040959493; PHPSESSION=MR6vWMINQtbU3jhaKoZPYp3lY3HYxrRX",
                Referer = "http://yyz2.666.macao-lottery.com/d1.html",
                ContentType = "application/json;charset=UTF-8",
                Postdata = postJson.ToString()
            };
            HttpResult hr = http.GetHtml(item);

        }

        private void btn_pan_login_Click(object sender, EventArgs e)
        {
            this.cefShow(true);
            this.LoginBetSite(cmb_panb_platform.Text);
        }

        private void btn_pan_end_Click(object sender, EventArgs e)
        {
            //确认提示框
            var ret = MessageBox.Show("重置后, 需要重新登录! 是否要操作!", "确认", MessageBoxButtons.OKCancel);
            if (ret == DialogResult.OK)
            {
                this.cefShow(true);
                MainConfigure.cefBetBrowser.ReSetBrowser();
                if (this.GetBetApi() != null)
                    this.GetBetApi().Cancel();
            }
        }

        private void btnOpenWx_Click(object sender, EventArgs e)
        {
            btnOpenWx.Enabled = false;
            MainConfigure.boterServices.wxHelper.OpenWx(new Action(() => {
                this.Invoke(new Action(() => {
                    btnOpenWx.Enabled = true;
                }));
            }));
        }

        private void btnLog_Click(object sender, EventArgs e)
        {
            var view = MainConfigure.boterServices.logView;
            BoterServices.ShowWindows(view);
        }

        private void 绑定ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //得到列表索引
            btnBoterStart_Click(sender, e);
        }



        [System.Reflection.ObfuscationAttribute(Feature = "Virtualization", Exclude = false)]
        private void btnBoterStart_Click(object sender, EventArgs e)
        {
            
        }

        //离线绑定
        private void 离线绑定管理ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InputBox input = new InputBox();
            var dr = input.ShowDialog();
            if (dr == DialogResult.OK)
            {
                MainConfigure.boterServices.userServicesBegin(new WxGroup()
                {
                    wxid = input.GroupWxid
                }, callback_RunningStatus);
            }
        }

        private bool callback_RunningStatus(bool status)
        {
            if (status)
            {
                //btnBoterStart.Enabled = false;
                btnStop.Enabled = true;
            }
            else
            {
                //btnBoterStart.Enabled = true;
                btnStop.Enabled = false;
            }
            return true;
        }


        [System.Reflection.ObfuscationAttribute(Feature = "Virtualization", Exclude = false)]
        private void btnStop_Click(object sender, EventArgs e)
        {
            MainConfigure.boterServices.userServicesStop(callback_RunningStatus);
        }

        private void btnRegex_Click(object sender, EventArgs e)
        {
            // string regexStr = @"([\u4e00-\u9fa5]+)(\d*)([^#]*)";
            //string regexStr = "([上]|[下]){1}(\\d*)([^#]*)";
            //string content = "上1000.0";
            //bool bret = Regex.IsMatch(content, regexStr, RegexOptions.IgnoreCase);
            //var items = Regex.Match(content, regexStr, RegexOptions.IgnoreCase);
            //var st0 = items.Groups[0].Value;
            //var st1 = items.Groups[1].Value;    //中文(玩法)  只有上或者下
            //var st2 = items.Groups[2].Value;    //金额
            //var st3 = items.Groups[3].Value;

            //BoterBetContents bet = new BoterBetContents("123大100");
            //BoterBetContents bet = new BoterBetContents(1, "总总和大100"); //解析错误, 逻辑OK
            //bet = new BoterBetContents(1, "总和大100"); //解析错误, 逻辑OK

            // string msg = "@飞扬🐠 上10000";
            //string msg = "@飞 扬  下1000.6667";
            //var buff = Encoding.Default.GetBytes(msg);
            //string regexStr = @"@([^ ]+).(上|下){1}(\d+)";
            //bool brgx = Regex.IsMatch(msg, regexStr);
            //var ss = Regex.Match(msg, regexStr);
            //string s1 = ss.Groups[1].Value;
            //string s2 = ss.Groups[2].Value;
            //string s3 = ss.Groups[3].Value;
            //string s4 = ss.Groups[4].Value;
            //MainConfigure.boterServices.wxHelper.CallSendText_11036("38969656589@chatroom", $"@{s1} 你好啊");
            //格式二测试
            //@"(\d+)(上|下){1}(\d+)(.*)"
            string regexStr = @"(\d+)(上|下){1}(\d+)(.*)";
            string msg = "7上10000";
            bool brgx = Regex.IsMatch(msg, regexStr);
            var ss = Regex.Match(msg, regexStr);
            string s1 = ss.Groups[1].Value;
            string s2 = ss.Groups[2].Value;
            string s3 = ss.Groups[3].Value;
            string s4 = ss.Groups[4].Value;
        }

        private void btn_set_more_Click(object sender, EventArgs e)
        {
            //int aa = MainConfigure.appSetting.wxMaxBet = 26000;
            MessageBox.Show("暂时没有更多的设置选项!");
        }

        /// <summary>
        ///     编辑框输入校验
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbxMin_Validating(object sender, CancelEventArgs e)
        {
            string strPattem = "[0-9]";
            Regex rgx = new Regex(strPattem);
            if (rgx.IsMatch(tbxMin.Text) == false)
            {
                tbxMin.BackColor = Color.Red;
                e.Cancel = true; //就不会触发后面的 Validated 事件
            }
            //成功可以在 Validated 事件中处理其他代码
        }

        private void cms_member_Opening(object sender, CancelEventArgs e)
        {
            var m = (V2Member)dgv_members.CurrentRow.DataBoundItem;
            if (m != null)
            {
                if (m.State == MemBerState.管理)
                {
                    设为管理ToolStripMenuItem.Text = "取消管理";
                }
                else
                {
                    设为管理ToolStripMenuItem.Text = "设为管理";

                }

                if (m.State == MemBerState.托)
                {
                    设为托ToolStripMenuItem.Text = "取消托";
                }
                else
                {
                    设为托ToolStripMenuItem.Text = "设为托";
                }
            }
        }


        private void 设为管理ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool isMamager = false;
            if (设为管理ToolStripMenuItem.Text == "设为管理")
                isMamager = true;
            else
                isMamager = false;
            var m = (V2Member)dgv_members.CurrentRow.DataBoundItem;
            if (m != null)
            {
                DialogResult dr = MessageBox.Show($"是否对 [{m.nickname}]  {设为管理ToolStripMenuItem.Text}", "提示", MessageBoxButtons.YesNo);
                if (dr == DialogResult.Yes)
                {
                    if (!isMamager)
                    {
                        if (m.wxid == MainConfigure.boterServices.wxHelper.wxid)
                        {
                            MessageBox.Show("机器人账号不能取消管理!");
                        }
                        else
                        {
                            m.State = MemBerState.会员;   //取消管理后, 就变成普通会员了
                            MainConfigure.boterServices.loglite.Add(Log.Create("取消管理", $"{m.wxid}-{m.nickname}-成为会员了"));
                        }
                    }
                    else
                    {
                        m.State = MemBerState.管理;
                        MainConfigure.boterServices.loglite.Add(Log.Create("设为管理", $"{m.wxid}-{m.nickname}-成为管理了"));
                    }
                }
            }
        }

        private void 设为托ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool isMamager = false;
            if (设为托ToolStripMenuItem.Text == "设为托")
                isMamager = true;
            else
                isMamager = false;
            var m = (V2Member)dgv_members.CurrentRow.DataBoundItem;
            if (m != null)
            {
                DialogResult dr = MessageBox.Show($"是否对 [{m.nickname}]  {设为托ToolStripMenuItem.Text}", "提示", MessageBoxButtons.YesNo);
                if (dr == DialogResult.Yes)
                {
                    if (isMamager)
                    {
                        if (m.wxid == MainConfigure.boterServices.wxHelper.wxid)
                        {
                            MessageBox.Show("机器人账号不能设置为托!");
                        }
                        else
                        {
                            m.State = MemBerState.托;   //取消管理后, 就变成普通会员了
                            MainConfigure.boterServices.loglite.Add(Log.Create("设置托", $"{m.wxid}-{m.nickname}-成为托了"));
                        }
                    }
                    else
                    {
                        m.State = MemBerState.会员;
                        MainConfigure.boterServices.loglite.Add(Log.Create("取消托", $"{m.wxid}-{m.nickname}-成为会员了"));
                    }
                }
            }
        }

        private void 清分ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("禁止清理! 请使用全部清除命令\r\n或者下分命令清除\r\n");
        }

        private void 删除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("禁止删除!");
        }


        private void cbb_sxf_ListImgClick(object sender, int index)
        {
            MessageBox.Show("你点了关闭");
        }

        private void ucBuySellCoins1_Load(object sender, EventArgs e)
        {

        }

        private void btnOrderView_Click(object sender, EventArgs e)
        {
            var view = MainConfigure.boterServices.betOrderView;
            BoterServices.ShowWindows(view);
        }

        private void btn_data_test_Click(object sender, EventArgs e)
        {

            //var browser = MainConfigure.cefBetBrowser;
            //var betsite = (HX666)MainConfigure.GetBetApi();
            //betsite.GetOdds();
            /*
             *        //jsScript.Append("hxp.open('POST', 'https://4921031761-cj.mm666.co/PlaceBet/Loaddata?lotteryType=TWBINGO');");
            //jsScript.Append("hxp.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');");
            //jsScript.Append($"hxp.send('itype=-1&settingCode=LM%2CWH%2CFLSX%2CLH&oddstype=A&lotteryType=TWBINGO&install={issueid}');");
             */
            int issueid = BinGouHelper.getNextIssueId(DateTime.Now);
            LxHttpHelper http = new LxHttpHelper();
            HttpItem item = new HttpItem()
            {
                URL = "https://4921031761-cj.mm666.co/PlaceBet/Loaddata?lotteryType=TWBINGO",
                Method = "POST",
                Postdata = $"itype=-1&settingCode=LM%2CWH%2CFLSX%2CLH&oddstype=A&lotteryType=TWBINGO&install={issueid}",
                Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7",
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36",
                Cookie = MainConfigure.cefBetBrowser.Cookies,
                //Allowautoredirect = true
            };
            item.Header.Add("Sec-Ch-Ua:\"Chromium\";v=\"116\", \"Not)A;Brand\";v=\"24\", \"Google Chrome\";v=\"116\"");
            item.Header.Add("Sec-Ch-Ua-Mobile:?0");
            item.Header.Add("Sec-Ch-Ua-Platform:\"Windows\"");
            item.Header.Add("Sec-Fetch-Dest:document");
            item.Header.Add("Sec-Fetch-Mode:navigate");
            item.Header.Add("Sec-Fetch-Site:none");
            item.Header.Add("Sec-Fetch-User:?1");
            item.Header.Add("Upgrade-Insecure-Requests:1");
            var hr = http.GetHtml(item);
        }

        private void btn_pan_refresh_Click(object sender, EventArgs e)
        {
            //var betsite = this.GetBetApi();
            //if(betsite.GetUserInfoUpdata())
            //{
            //    tbxtbx_pan_blance.Text = betsite.amount.ToString("#.##");
            //}

            MainConfigure.boterServices.UpdataUserInfo();

            //betsite.GetOdds();
        }

        private void btnOdds_Click(object sender, EventArgs e)
        {
            OddsBingo odds = new OddsBingo(CarNumEnum.P1, BetPlayEnum.大, "", "B1DS_D", 0f);
        }

        private void btnBettest_Click(object sender, EventArgs e)
        {
            string urlroot = "https://8575517633-cj.mm666.co";
            string url = $"{urlroot}/PlaceBet/Confirmbet?lotteryType=TWBINGO";
            //1大
            string postdata = $"betdata%5B0%5D%5BAmount%5D=10&betdata%5B0%5D%5BKeyCode%5D=B1DS_D&betdata%5B0%5D%5BOdds%5D=1.97&lotteryType=TWBINGO&betNum=10{LxTimestampHelper.GetTimeStamp13()}&prompt=true&gt=A&qf=0";
            string cookie = "cf_clearance=.oQQDBK48xIqJuqu1B8qoYopym9Q1gDmAB9enCPc4_8-1692479821-0-1-c10be91d.76c17a16.960ab6fd-160.0.0; usid=CfDJ8JTgj6Z5UcZLohgMbPzsNFrbgS%2B3OcclEb1pui3jEMUgrpq%2F%2FuYs4IP%2FScFNJDl0GlRG00vp1PsjFNZmcmdIz6GkXjC68oiFX1IJOvvBXiTo2WlMfu5UDFEQWDCoGPT8EuVhLo7Fo4g5Q3wjuVDsW6IqSr3yxSaMCqkyNacNbIvo; csrf=CfDJ8JTgj6Z5UcZLohgMbPzsNFq4KSVAOEKzhJw5M_hiZuzGtWslSfcxxhBiWsb_2jdAVlwUxHEUk4sbm9Zmk1BGW0VskVZbRHcsNMgAWoz7hvnmASPhREKo8OnpXXxGnzMrMvhlju0g5tqxz86Lo9Cojro; ssid_token=kWkwKViWePhL/nhJHSTgXGXG2yU=; random=2.7431956605149e+14";
            LxHttpHelper http = new LxHttpHelper();
            HttpItem item = new HttpItem()
            {
                URL = url,
                Method = "POST",
                Cookie = cookie,
                Postdata = postdata,
                Accept = "application/json, text/javascript, */*; q=0.01",
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36",
                ContentType = "application/x-www-form-urlencoded; charset=UTF-8",
                Referer = "https://8575517633-cj.mm666.co/PlaceBet/Index?lotteryType=TWBINGO&page=zp",
            };
            var hr = http.GetHtml(item);
        }

        private void btn_api_getday_Click(object sender, EventArgs e)
        {
            var items = BoterApi.GetInstance().getbgday(DateTime.Now.ToString("yyyy-MM-dd"), 32, true);
        }

        private void btnClearData_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult dr = DialogResult.Cancel;
                if (sender != null)
                    dr = MessageBox.Show("该操作会删除绑定群的所有订单数据!\r\n且不可以恢复\r\n您确定要删除吗?", "提示", MessageBoxButtons.YesNo);
                else
                    dr = DialogResult.Yes;
                if (dr == DialogResult.Yes)
                {
                    string groupWxid = MainConfigure.boterServices.groupBind.wxid;
                    List<V2Member> members_del = new List<V2Member>();
                    var members = MainConfigure.boterServices.v2Memberbindlite;
                    foreach (var m in members)
                    {
                        m.BetToday = 0; //今日流水
                        m.BetTotal = 0; //总流水
                        m.CreditToday = 0;  //今日上分
                        m.CreditTotal = 0;  //总上分
                        m.IncomeToday = 0;  //今日盈亏
                        m.IncomeTotal = 0;  //总盈亏
                        m.WithdrawToday = 0;//今日下分
                        m.WithdrawTotal = 0;    //总下分
                        m.Balance = 0;  //余额
                        m.BetCur = 0;   //当期投注
                        if (m.State == MemBerState.已退群)
                        {
                            m.State = MemBerState.已删除;
                            members_del.Add(m);
                        }
                    }
                    //直接操作数据库,删除数据库中有分的人员
                    var membersDb = MainConfigure.boterServices.v2Memberbindlite.sql.getTabble().ToList();
                    foreach (var m in membersDb)
                    {
                        m.BetToday = 0; //今日流水
                        m.BetTotal = 0; //总流水
                        m.CreditToday = 0;  //今日上分
                        m.CreditTotal = 0;  //总上分
                        m.IncomeToday = 0;  //今日盈亏
                        m.IncomeTotal = 0;  //总盈亏
                        m.WithdrawToday = 0;//今日下分
                        m.WithdrawTotal = 0;    //总下分
                        m.Balance = 0;  //余额
                        m.BetCur = 0;   //当期投注
                    }
                    var membersConnect = MainConfigure.boterServices.v2Memberbindlite.sql.getConnect().UpdateAll(membersDb);

                    //删除48小时之前的所有数据
                    var timestamp_del_after = LxTimestampHelper.ConvertDateTimeInt(DateTime.Now.AddDays(-2));
                    var loger = MainConfigure.boterServices.loglite.sql.getTabble().Delete(p => p.AtTimestamp <= timestamp_del_after);

                    var moneyOrder = MainConfigure.boterServices.v2MemberCoinsBuySellbindlite;
                    for (int i = (moneyOrder.Count - 1); i >= 0; i--)
                    {
                        moneyOrder.Remove(moneyOrder[i]);
                    }

                    var memberOrder = MainConfigure.boterServices.v2memberOderbindlite;
                    for (int i = (memberOrder.Count - 1); i >= 0; i--)
                    {
                        memberOrder.Remove(memberOrder[i]);
                    }

                    //删除退群的人员
                    foreach (var v in members_del)
                    {
                        MainConfigure.boterServices.v2Memberbindlite.Remove(v);
                    }

                    //刷新总流水
                    MessageBox.Show("数据清理完成! 请重新打开软件!");
                }
            }
            catch (Exception ex)
            {
                // 异常处理及相关逻辑
            }
        }

        private void btnClearAllData_Click(object sender, EventArgs e)
        {
            var mr = MessageBox.Show("您确认要进行删除操作吗？", "警告", MessageBoxButtons.YesNo);
            if (mr == DialogResult.Yes)
            {
                btnClearData_Click(null, e);
                MainConfigure.boterServices.v2MemberCoinsBuySellbindlite.RemoveAll();
                MainConfigure.boterServices.v2memberOderbindlite.RemoveAll();
            }
        }

        private void 刷新ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //发送diamagnetic1
            MainConfigure.boterServices.Refresh();
        }



        private void pnl_order_Paint(object sender, PaintEventArgs e)
        {

        }

        private void lbl_OrderInfo_Click(object sender, EventArgs e)
        {

        }


        private void btnFlowser2_Click(object sender, EventArgs e)
        {
            BoterServices.ShowWindows(PlanFlowerConfigure.view);
        }

        private void MainView_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Closeing();
            //强制关闭
            Process currentProcess = Process.GetCurrentProcess();
            currentProcess.Kill(); // 关闭当前进程
        }

        private void btnBetAction_Click(object sender, EventArgs e)
        {
            int issueid = MainConfigure.boterServices.IssueidCur;
            var data = new List<V2MemberOrder>();
            data.Add(new V2MemberOrder(null, issueid, LxTimestampHelper.GetTimeStampToInt32(), "1大10", "1大10", 1, 10));
            MainConfigure.boterServices.BetOrder(issueid, data);
        }

        private void btnWxGroupSelect_Click(object sender, EventArgs e)
        {
            //把群数据拷贝进去。
            var items = MainConfigure.boterServices.wxHelper.wxGroups;
            XWxGroupView view = new XWxGroupView(items);
            var result = view.ShowDialog();
            if (result == DialogResult.OK)
            {
                MainConfigure.boterServices.userServicesBegin(view.WxGroup, callback_RunningStatus);
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            var isseuId = this.tbxIssueid.Text;
            Show(this.dpckSearch.Value, isseuId, chkNotPay.Checked);
        }


        private void Show(DateTime time, string strIsseueId, bool bNotPay)
        {
            var t = $"{time.Year}-{time.Month}-{time.Day} 00:00:00";
            var today = Convert.ToDateTime(t);
            var time1 = time.AddDays(1);
            var tt = $"{time1.Year}-{time1.Month}-{time1.Day} 00:00:00";
            var tomorrow = Convert.ToDateTime(tt);

            //&& x.TimeStampBet < totomorrowStamp
            var todayStamp = LxTimestampHelper.ConvertDateTimeInt(today);
            var tomorrowStamp = LxTimestampHelper.ConvertDateTimeInt(tomorrow);
            List<V2MemberOrder> orders = new List<V2MemberOrder>();

            int bet2 = 0;   //下注统计
            int profit = 0; //盈亏统计
            int mm2 = 0;    //未结算
            int issueid = 0;
            if (string.IsNullOrEmpty(strIsseueId))
            {
                try
                {
                    issueid = Convert.ToInt32(strIsseueId);
                }
                catch
                {
                    issueid = 0;
                }
            }

            var ordersLite = MainConfigure.boterServices.v2memberOderbindlite;
            var report = ordersLite.sql.getTabble().Where(x => x.TimeStampBet >= todayStamp && x.TimeStampBet < tomorrowStamp);
            if (issueid != 0)
                report = report.Where(x => x.IssueId == issueid);
            if (bNotPay)
                report = report.Where(x => x.OrderStatus == OrderStatusEnum.待处理 || x.OrderStatus == OrderStatusEnum.待结算);


            var response = report.ToList();
            _orders_result.Clear();
            foreach (var res in response)
            {
                _orders_result.Add(res);
            }



            //this.lblShow.Text = $"下注：{bet2} | 盈亏：{profit} | 未结算：{mm2}";

        }

        private void dgv_recharge_unprocessed_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0)
                {
                    if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right)
                    {
                        var ss = dgv_recharge_unprocessed.CurrentCell.RowIndex;
                        if (ss != e.RowIndex)
                        {
                            dgv_recharge_unprocessed.ClearSelection();
                            dgv_recharge_unprocessed.CurrentCell = dgv_recharge_unprocessed.Rows[e.RowIndex].Cells[e.ColumnIndex];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("选择异常! \r\n请左键点击再次选择后, 再操作!");
            }
        }

        //上下分剥离出来
        private void 同意ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                ToolStripMenuItem mi = (ToolStripMenuItem)sender;
                ContextMenuStrip ms = (ContextMenuStrip)mi.GetCurrentParent();
                DataGridView tb = (DataGridView)ms.SourceControl;
                string name = tb.Name;
                if (name == "dgv_recharge_unprocessed")
                {
                    //未处理列表
                    if (dgv_recharge_unprocessed.CurrentRow != null)
                    {
                        int index = dgv_recharge_unprocessed.CurrentRow.Index;
                        if (index >= 0)
                        {
                            var order = dgv_recharge_unprocessed.CurrentRow.DataBoundItem as V2MemberCoinsBuySell;
                            MainConfigure.boterServices.On上下分同意操作(order);
                            //try
                            //{
                            //    if (order.PayAction == V2MemberPayAction.上分)
                            //        MainConfigure.boterServices.OnActionMemberCredit(order);
                            //    else if (order.PayAction == V2MemberPayAction.下分)
                            //        MainConfigure.boterServices.OnActionMemberWithdraw(order);

                            //    //清理掉。已经处理过的上下分命令
                            //    var dd = MainConfigure.boterServices.v2MemberCoinsBuySellbindliteUnProcessed.Where(p => p.PayStatus != V2MemberPayStatus.等待处理).ToList();
                            //    for (int i = dd.Count - 1; i >= 0; i--)
                            //    {
                            //        MainConfigure.boterServices.v2MemberCoinsBuySellbindliteUnProcessed.Remove(dd[i]);
                            //    }
                            //}
                            //catch (Exception ex)
                            //{
                            //    MessageBox.Show(ex.Message);
                            //}
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void 忽略ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                ToolStripMenuItem mi = (ToolStripMenuItem)sender;
                ContextMenuStrip ms = (ContextMenuStrip)mi.GetCurrentParent();
                DataGridView tb = (DataGridView)ms.SourceControl;
                string name = tb.Name;
                //判断来自哪个dgv的操作
                if (name == "dgv_recharge_unprocessed")
                {
                    //未处理列表
                    if (dgv_recharge_unprocessed.CurrentRow != null)
                    {
                        int index = dgv_recharge_unprocessed.CurrentRow.Index;
                        if (index >= 0)
                        {
                            var order = dgv_recharge_unprocessed.CurrentRow.DataBoundItem as V2MemberCoinsBuySell;
                            MainConfigure.boterServices.On上下分忽略操作(order);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void dgv_recharge_unprocessed_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            RechargePainting(dgv_recharge_unprocessed, e);
        }

        private void RechargePainting(DataGridView dgview, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex > -1)
            {
                var column = dgview.Columns[e.ColumnIndex];
                var rowIndex = e.RowIndex;
                var cellName = column.DataPropertyName;
                if (column != null)
                {
                    switch (column.DataPropertyName)
                    {
                        case "PayAction":
                            {
                                var data = (V2MemberPayAction)dgview.Rows[rowIndex].Cells[cellName].Value;
                                if (data == V2MemberPayAction.上分)
                                {
                                    dgview.Rows[rowIndex].Cells[cellName].Style.BackColor = Color.Green;   //设置单元格颜色
                                    dgview.Rows[rowIndex].Cells[cellName].Style.ForeColor = Color.Black;
                                }
                                else if (data == V2MemberPayAction.下分)
                                {
                                    dgview.Rows[rowIndex].Cells[cellName].Style.BackColor = Color.Red;   //设置单元格颜色
                                    dgview.Rows[rowIndex].Cells[cellName].Style.ForeColor = Color.White;
                                }
                                break;
                            }
                        case "PayStatus":
                            {
                                var data = (V2MemberPayStatus)dgview.Rows[rowIndex].Cells[cellName].Value;
                                if (data == V2MemberPayStatus.等待处理)
                                {
                                    dgview.Rows[rowIndex].Cells[cellName].Style.BackColor = Color.Red;   //设置单元格颜色
                                    dgview.Rows[rowIndex].Cells[cellName].Style.ForeColor = Color.White;
                                }
                                else if (data == V2MemberPayStatus.同意)
                                {
                                    dgview.Rows[rowIndex].Cells[cellName].Style.BackColor = Color.Green;   //设置单元格颜色
                                    dgview.Rows[rowIndex].Cells[cellName].Style.ForeColor = Color.White;
                                }
                                else if (data == V2MemberPayStatus.忽略)
                                {
                                    dgview.Rows[rowIndex].Cells[cellName].Style.BackColor = Color.LightGray;   //设置单元格颜色
                                    dgview.Rows[rowIndex].Cells[cellName].Style.ForeColor = Color.Black;
                                }
                                break;
                            }
                        case "Money":
                            {
                                //百元:白色
                                //千元:绿色
                                //万元:橙色
                                int data = (int)dgview.Rows[rowIndex].Cells[cellName].Value;
                                if (data > 0)
                                {
                                    if (data >= 10000)
                                    {
                                        dgview.Rows[rowIndex].Cells[cellName].Style.BackColor = Color.Orange;   //设置单元格颜色
                                        dgview.Rows[rowIndex].Cells[cellName].Style.ForeColor = Color.Black;
                                    }
                                    else if (data >= 1000)
                                    { //说明是千
                                        dgview.Rows[rowIndex].Cells[cellName].Style.BackColor = Color.Green;   //设置单元格颜色
                                        dgview.Rows[rowIndex].Cells[cellName].Style.ForeColor = Color.Black;
                                    }
                                    else if (data >= 100)
                                    {
                                        dgview.Rows[rowIndex].Cells[cellName].Style.BackColor = Color.LightGray;   //设置单元格颜色
                                        dgview.Rows[rowIndex].Cells[cellName].Style.ForeColor = Color.Black;
                                    }
                                }

                                break;
                            }
                        default:
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     补分 -- 列表可见的补分
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private BgLotteryData _lotteryOpenData;   //补分当期的数据
        private void btnUpMoney_Click(object sender, EventArgs e)
        {
            try
            {
                V2MemberOrder order = null;
                int index = dgv_orders.CurrentRow.Index;
                if (index >= 0)
                {
                    order = dgv_orders.CurrentRow.DataBoundItem as V2MemberOrder;

                    //查询订单开奖数据, 并且插入到队列
                    if (_lotteryOpenData == null || _lotteryOpenData.IssueId != order.IssueId)
                    {
                        var response = MainConfigure.getBotApi().getBgdata(order.IssueId);
                        if (response.code == 0)
                        {
                            if (response.data != null)
                            {
                                //这里要重新封装下，要能一步到位得到BgLotteryData对象,因为开奖结果页面，也有这个方法, 也需要获取某期开奖数据
                                _lotteryOpenData = new BgLotteryData();
                                _lotteryOpenData.FillLotteryData(response.data.issueid
                                , response.data.lotteryData
                                , response.data.lottery_time);
                            }
                        }
                        else
                        {
                            string errMsg = $"获取开奖数据失败! {JsonConvert.SerializeObject(response)}";
                            MainConfigure.boterServices.loglite.Add(Log.Create($"补分失败::获取开奖数据错误::{order.IssueId}", errMsg));
                            MessageBox.Show(errMsg);
                            return;
                        }
                    }


                    if (_lotteryOpenData == null)
                    {
                        MessageBox.Show("未获取到开奖数据! 请稍后重试!");
                        return;
                    }
                    if (_lotteryOpenData.IssueId != order.IssueId)
                    {
                        MessageBox.Show("开奖结果期号校验失败! 请重试!");
                        _lotteryOpenData = null;
                        return;
                    }


                    if (MainConfigure.boterServices.groupBind == null)
                    {
                        MessageBox.Show("没有绑定群组! 不能补分!");
                        return;
                    }

                    if (MainConfigure.boterServices.groupBind.wxid != order.GroupWxId)
                    {
                        var msgret = MessageBox.Show("该订单与目前绑定的群组不一致! \r\n订单不是这个群的\r\n您确定要补该订单吗?", "警告", MessageBoxButtons.YesNo);
                        if (msgret != DialogResult.No)
                            return;
                    }


                    var m = MainConfigure.boterServices.v2Memberbindlite.FirstOrDefault(p => p.wxid == order.wxid && p.GroupWxId == order.GroupWxId);
                    if (m == null)
                    {
                        MessageBox.Show("没有在目前绑定的群中找到该会员数据! \r\n无法补分!");
                        return;
                    }

                   ;
                    if (MainConfigure.boterServices.OnMemberOrderFinish(order, _lotteryOpenData))
                    {
                        string shengStrList = "----补分名单----\r";
                        shengStrList += $"{order.nickname}|{order.IssueId % 1000}|{_lotteryOpenData.ToLotteryString()}|{order.BetContentOriginal}|{order.AmountTotal - order.Profit}\r";

                        string shengStrList2 = "------补完留分------\r";
                        shengStrList2 += $"{order.nickname} | {m.Balance}";

                        MainConfigure.boterServices.wxHelper.CallSendText_11036(order.GroupWxId, shengStrList);
                        MainConfigure.boterServices.wxHelper.CallSendText_11036(order.GroupWxId, shengStrList2);
                    }
                    _orders_result.Clear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"出现了小小的意外:{ex.Message}");
            }
        }

        private void btn_recharge_all_Click(object sender, EventArgs e)
        {
            var view = MainConfigure.boterServices.coninsView;
            BoterServices.ShowWindows(view);
        }

        private void btnBettingTest_Click(object sender, EventArgs e)
        {
            MainConfigure.boterServices.BetingTest();
        }

        private void btnGetContacts_Click(object sender, EventArgs e)
        {
            var result = MainConfigure.boterServices.wxHelper.wxServices.GetContacts();
            Debug.WriteLine(JsonConvert.SerializeObject(result));
        }

        private void skinButton1_Click(object sender, EventArgs e)
        {
            string wxid = tbx_wxgrp_id.Text.Replace(" ", "");
            if(wxid.IndexOf("@chatroom") != -1 )
            {
                var result = MainConfigure.boterServices.wxHelper.wxServices.GetMemberList(wxid);
                Debug.WriteLine("群成员="+JsonConvert.SerializeObject(result));
            }
            else
            {
                MessageBox.Show("请输入正确的群ID");
            }
        }

        private void btnGetNikeName_Click(object sender, EventArgs e)
        {
            string wxid = tbx_wxid.Text.Replace(" ", "");
            var result = MainConfigure.boterServices.wxHelper.wxServices.GetMemberNickname(wxid);
            Debug.WriteLine("昵称="+JsonConvert.SerializeObject(result));
        }

        private void btnBufenDantiao_Click(object sender, EventArgs e)
        {
            try
            {
                V2MemberOrder order = null;
                int index = dgvOrdersSearch.CurrentRow.Index;
                Debug.WriteLine($"补分::index={index}");
                if (index >= 0)
                {
                    order = dgvOrdersSearch.CurrentRow.DataBoundItem as V2MemberOrder;

                    //查询订单开奖数据, 并且插入到队列
                    if (_lotteryOpenData == null || _lotteryOpenData.IssueId != order.IssueId)
                    {
                        var response = MainConfigure.getBotApi().getBgdata(order.IssueId);
                        Debug.WriteLine($"补分::getdatas={JsonConvert.SerializeObject(response)}");
                        if (response.code == 0)
                        {
                            if (response.data != null)
                            {
                                //这里要重新封装下，要能一步到位得到BgLotteryData对象,因为开奖结果页面，也有这个方法, 也需要获取某期开奖数据
                                _lotteryOpenData = new BgLotteryData();
                                _lotteryOpenData.FillLotteryData(response.data.issueid
                                , response.data.lotteryData
                                , response.data.lottery_time);
                            }
                        }
                        else
                        {
                            Debug.WriteLine($"补分::获取开奖数据失败={response}");
                            string errMsg = $"获取开奖数据失败! {JsonConvert.SerializeObject(response)}";
                            MainConfigure.boterServices.loglite.Add(Log.Create($"补分失败::获取开奖数据错误::{order.IssueId}", errMsg));
                            MessageBox.Show(errMsg);
                            return;
                        }
                    }


                    if (_lotteryOpenData == null)
                    {
                        MessageBox.Show("未获取到开奖数据! 请稍后重试!");
                        return;
                    }
                    if (_lotteryOpenData.IssueId != order.IssueId)
                    {
                        MessageBox.Show("开奖结果期号校验失败! 请重试!");
                        _lotteryOpenData = null;
                        return;
                    }


                    if (MainConfigure.boterServices.groupBind == null)
                    {
                        MessageBox.Show("没有绑定群组! 不能补分!");
                        return;
                    }

                    if (MainConfigure.boterServices.groupBind.wxid != order.GroupWxId)
                    {
                        var msgret = MessageBox.Show("该订单与目前绑定的群组不一致! \r\n订单不是这个群的\r\n您确定要补该订单吗?", "警告", MessageBoxButtons.YesNo);
                        if (msgret != DialogResult.No)
                            return;
                    }


                    var m = MainConfigure.boterServices.v2Memberbindlite.FirstOrDefault(p => p.wxid == order.wxid && p.GroupWxId == order.GroupWxId);
                    if (m == null)
                    {
                        MessageBox.Show("没有在目前绑定的群中找到该会员数据! \r\n无法补分!");
                        return;
                    }
                    //if(order.OrderStatus != OrderStatusEnum.待结算)
                    //{
                    //    MessageBox.Show("补分失败! 要求订单状态必须是:待结算");
                    //    return;
                    //}

                    var member_order = MainConfigure.boterServices.v2memberOderbindlite.FirstOrDefault(p => p.id == order.id && p.BetContentOriginal == order.BetContentOriginal);

                    //if (OnAction补分(m, member_order, _lotteryOpenData))

                    if (MainConfigure.boterServices.OnMemberOrderFinish(order, _lotteryOpenData))
                    {
                        Debug.WriteLine($"补分::发送补分信息");
                        string shengStrList = "----补分名单----\r";
                        shengStrList += $"{member_order.nickname}|{member_order.IssueId % 1000}|{_lotteryOpenData.ToLotteryString()}|{member_order.BetContentOriginal}|{member_order.Profit - member_order.AmountTotal}\r";

                        string shengStrList2 = "------补完留分------\r";
                        shengStrList2 += $"{member_order.nickname} | {m.Balance}";

                        MainConfigure.boterServices.wxHelper.CallSendText_11036(member_order.GroupWxId, shengStrList);
                        MainConfigure.boterServices.wxHelper.CallSendText_11036(member_order.GroupWxId, shengStrList2);
                    }
                    //_orderslite.Clear();
                    Debug.WriteLine($"补分::结束");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"出现了小小的意外:{ex.Message}");
            }

        }
    }
}
