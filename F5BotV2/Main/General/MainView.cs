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
using F5Bot.Main.X;
using F5Bot.Main.General;
using Org.BouncyCastle.Asn1.X509;
using CefSharp.DevTools.Database;

namespace F5BotV2.Main
{
    public partial class MainView : CCSkinMain
    {
        public MainView()
        {
            InitializeComponent();
        }

        private void MainView_Load(object sender, EventArgs e)
        {
            this.Initialzation(); //里面有初始化微信接口

            //修改窗口标题
            this.Text = $"小花{Assembly.GetExecutingAssembly().GetName().Version.ToString()} - 仅供测试娱乐! - 有效期 {BoterApi.GetInstance().loginApiResponse.data.c_off_time.ToString()}";

            tab_order_source.SelectedIndex = 0;
            //this.InitWxContactsView(dgv_WxContacts);
            this.InitWxGroupsView(dgv_WxContacts);  //联系人,群

            //初始化会员表
            //dgv_members
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
                    cell = p.Columns["city"];if (cell != null){cell.Visible = false; } //wxid
                    cell = p.Columns["wxid"]; if (cell != null) { cell.Visible = false; }
                    cell = p.Columns["GroupWxId"]; if (cell != null) { cell.Visible = false; } //State
                    cell = p.Columns["State"]; if (cell != null) { cell.Width = 69; }
                    cell = p.Columns["display_name"]; if (cell != null) { cell.Visible = false; }
                    cell = p.Columns["BetWait"]; if(cell != null) { cell.Visible = false; } //不显示待结算
                    cell = p.Columns["city"]; if (cell != null) { cell.DefaultCellStyle.Format = "0.00"; } //wxid
                    cell = p.Columns["Balance"]; if (cell != null) { cell.DefaultCellStyle.Format = "0.00"; } //wxid
                    cell = p.Columns["IncomeToday"]; if (cell != null) { cell.DefaultCellStyle.Format = "0.00"; }
                    cell = p.Columns["IncomeTotal"]; if (cell != null) { cell.DefaultCellStyle.Format = "0.00"; }
                    
                    cell = p.Columns["country"];
                    if (cell != null)
                    {
                        cell.Visible = false;
                    }
                    cell = p.Columns["IncomeTodayStart"];
                    if(cell != null)
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
                    var cell = p.Columns["id"];if (cell != null){ cell.Width = 45; }
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
                    cell = p.Columns["TimeString"]; if (cell != null) { cell.Width = 90; cell.DefaultCellStyle.Format = "0.0"; }//TimeString
                    //AmountTotal

                    cell = p.Columns["avatar"]; if (cell != null) { cell.Visible = false; }
                    cell = p.Columns["city"]; if (cell != null) { cell.Visible = false; }
                    cell = p.Columns["country"]; if (cell != null) { cell.Visible = false; }
                    cell = p.Columns["province"]; if (cell != null) { cell.Visible = false; }
                    cell = p.Columns["remark"]; if (cell != null) { cell.Visible = false; }
                    cell = p.Columns["sex"]; if (cell != null) { cell.Visible = false; }
                    

                    return true;
                }));


            


            //微信用户数据
            tbxWxNikeName.DataBindings.Add(new Binding("Text", MainConfigure.boterServices.wxHelper, "nickname"));
            tbxWxId.DataBindings.Add(new Binding("Text", MainConfigure.boterServices.wxHelper, "wxid"));
            tbxWxAccount.DataBindings.Add(new Binding("Text", MainConfigure.boterServices.wxHelper, "account"));
            tbxWxPid.DataBindings.Add(new Binding("Text", MainConfigure.boterServices.wxHelper, "clientid"));

            //绑定群数据
            tbx_BindGroupName.SkinTxt.DataBindings.Add(new Binding("Text", MainConfigure.boterServices.groupBind, "nickname"));

            //其他数据
            tbxClearPath.SkinTxt.DataBindings.Add(new Binding("Text", MainConfigure.appSetting, "wxClearPath"));

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

            //获取默认盘口
            cmb_panb_platform.DataBindings.Add(new Binding("Text", MainConfigure.appSetting, "platform", true, DataSourceUpdateMode.OnPropertyChanged));
            cmb_panb_platform.Text = MainConfigure.appSetting.platform;

            MainConfigure.boterServices.BoterStatusChange += BoterServices_BoterStatusChange;
            MainConfigure.boterServices.Start();
        }

        public void setWxImage(Func<SkinPictureBox, bool> func)
        {
            if (picWxImage.InvokeRequired)
            {
                picWxImage.Invoke(new Action(() =>
                {
                    func(this.picWxImage);
                }));
            }
            else
            {
                func(this.picWxImage);
            }
        }

        public void setLotteryLast(Func<UcLotteryDataLast, bool> func)
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

        public void setLotteryCur(Func<UcLotteryDataCur, bool> func)
        {
            if (ucLotteryDataCur.InvokeRequired)
            {
                this.BeginInvoke(new Action(() =>
                {
                    func(this.ucLotteryDataCur);
                }));
            }
            else
            {
                func(this.ucLotteryDataCur);
            }
        }


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
            if(ret == DialogResult.OK)
            {
                this.cefShow(true);
                MainConfigure.cefBetBrowser.ReSetBrowser();
                if(this.GetBetApi() != null)
                    this.GetBetApi().Cancel();
            }
        }

        private void btnOpenWx_Click(object sender, EventArgs e)
        {
            btnOpenWx.Enabled = false;
            MainConfigure.boterServices.wxHelper.OpenWx(new Action(() => {
                this.Invoke(new Action(() => {
                    btnOpenWx.Enabled = true ;
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
            //开启服务
            //得到列表索引
            try
            {
                WxGroup wxgroup = null;
                int index = dgv_WxContacts.CurrentRow.Index;
                if (index >= 0)
                {
                    wxgroup = dgv_WxContacts.CurrentRow.DataBoundItem as WxGroup;
                }
                MainConfigure.boterServices.userServicesBegin(wxgroup, callback_RunningStatus);
            }
            catch
            {
                MessageBox.Show("绑定错误! 请重新选择后再次绑定!");
            }
        }

        //离线绑定
        private void 离线绑定管理ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InputBox input = new InputBox();
            var dr = input.ShowDialog();
            if(dr == DialogResult.OK)
            {
                MainConfigure.boterServices.userServicesBegin(new WxGroup() { 
                     wxid = input.GroupWxid
                }, callback_RunningStatus, true);
            }
        }

        private bool callback_RunningStatus(bool status)
        {
            if(status)
            {
                btnBoterStart.Enabled = false;
                btnStop.Enabled = true;
            }
            else
            {
                btnBoterStart.Enabled = true;
                btnStop.Enabled = false;
            }
            return true;
        }

        private int BoterServices_BoterStatusChange(BoterStatus status, int issueid, BgLotteryData data)
        {
            if(issueid > 0)
            {
                if (ucLotteryDataLast.IssueId == issueid)
                {
                    if(data != null)
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
            if(dgv_members != null)
            {
                if(dgv_members.CurrentRow != null)
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
            }
        }


        private void 设为管理ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool isMamager = false;
            if (设为管理ToolStripMenuItem.Text == "设为管理")
                isMamager = true;
            else
                isMamager = false;
            if(dgv_members.CurrentRow == null)
            {
                MessageBox.Show("什么都没做!");
                return;
            }
            var m = (V2Member)dgv_members.CurrentRow.DataBoundItem;
            if(m != null)
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
            if (dgv_members.CurrentRow == null)
            {
                MessageBox.Show("什么都没做!");
                return;
            }
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
           var items =  BoterApi.GetInstance().getbgday(DateTime.Now.ToString("yyyy-MM-dd"), 32, true);
        }

        private void btnClearData_Click(object sender, EventArgs e)
        {
            try
            {
                //string path = System.Environment.CurrentDirectory;
                //File.Delete($"{path}\\v2.bat");
                DialogResult dr = MessageBox.Show("该操作会删除绑定群的所有订单数据!\r\n且不可以恢复\r\n您确定要删除吗?", "提示", MessageBoxButtons.YesNo);
                if(dr == DialogResult.Yes)
                {
                    string groupWxid = MainConfigure.boterServices.groupBind.wxid;
                    List<V2Member> members_del = new List<V2Member>();
                    var members = MainConfigure.boterServices.v2Memberbindlite;
                    foreach(var m in members)
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
                        if(m.State == MemBerState.已退群)
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
                    var loger = MainConfigure.boterServices.loglite.sql.getTabble().Delete(p=>p.AtTimestamp <= timestamp_del_after);

                    var moneyOrder = MainConfigure.boterServices.v2MemberCoinsBuySellbindlite;
                    for (int i = (moneyOrder.Count-1); i >=0 ; i--)
                    {
                        moneyOrder.Remove(moneyOrder[i]);
                    }

                    var memberOrder = MainConfigure.boterServices.v2memberOderbindlite;
                    for(int i = (memberOrder.Count-1); i >=0 ; i--)
                    {
                        memberOrder.Remove(memberOrder[i]);
                    }

                    //删除退群的人员
                    foreach (var v in members_del)
                    {
                        MainConfigure.boterServices.v2Memberbindlite.Remove(v);
                    }



                    //string fileName = @"v2.bat";
                    //if (File.Exists(fileName))
                    //{
                    //    try
                    //    {
                    //        //File.Delete(fileName);
                    //        File.Move(fileName, $"{fileName}-{LxTimestampHelper.GetTimeStamp()}");
                    //    }
                    //    catch (Exception exx)
                    //    {
                    //        //Console.WriteLine("The deletion failed: {0}", e.Message);
                    //    }
                    //}
                    //else
                    //{
                    //   // Console.WriteLine("Specified file doesn't exist");
                    //}
                }
            }
            catch (Exception ex)
            {
                // 异常处理及相关逻辑
            }
        }

        private void btnClearAllData_Click(object sender, EventArgs e)
        {
            var mr = MessageBox.Show("该操作将删除所有数据！\r\n并且不能恢复\r\n您确认要进行删除操作吗?", "警告", MessageBoxButtons.YesNo);
            if(mr == DialogResult.Yes)
            {
                btnClearData_Click(sender, e);
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

        private void dgv_WxContacts_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0)
                {
                    if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right)
                    {
                        var ss = dgv_WxContacts.CurrentCell.RowIndex;
                        if (ss != e.RowIndex)
                        {
                            dgv_WxContacts.ClearSelection();
                            dgv_WxContacts.CurrentCell = dgv_WxContacts.Rows[e.RowIndex].Cells[e.ColumnIndex];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("选择异常! \r\n请左键点击再次选择后, 再操作!");
            }
        }

        private void btnFlowser2_Click(object sender, EventArgs e)
        {
            BoterServices.ShowWindows(PlanFlowerConfigure.view);
        }

        private void MainView_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Closeing();
        }

        private void btnBetAction_Click(object sender, EventArgs e)
        {
            //int issueid = MainConfigure.boterServices.IssueidCur;
            //var data = new List<V2MemberOrder>();
            //data.Add(new V2MemberOrder(null, issueid, LxTimestampHelper.GetTimeStampToInt32(), "1大10", "1大10", 1, 10));
            //MainConfigure.boterServices.BetOrder(issueid, data);
            MainConfigure.boterServices.BetingTest();
        }

        void DeleteDirectory(string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            if (dir.Exists)
            {
                DirectoryInfo[] childs = dir.GetDirectories();
                foreach (DirectoryInfo child in childs)
                {
                    child.Delete(true);
                }
                dir.Delete(true);
            }
        }

        /// <summary>
        ///     设置微信清理目录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClearWx_Click(object sender, EventArgs e)
        {
            try
            {
                string path = tbxClearPath.Text;
                if (Directory.Exists(path))
                {
                    DeleteDirectory(path);
                    MessageBox.Show("成功!");
                }
                else
                {
                    FolderBrowserDialog dialog = new FolderBrowserDialog();
                    dialog.Description = "请选择文件路径";
                    DialogResult result = dialog.ShowDialog();
                    if (result == System.Windows.Forms.DialogResult.Cancel)
                    {
                        return;
                    }
                    string folderPath = dialog.SelectedPath.Trim();
                    DirectoryInfo theFolder = new DirectoryInfo(folderPath);
                    if (theFolder.Exists)
                    {
                        //操作
                        MainConfigure.appSetting.wxClearPath = folderPath;
                        //保存目录信息
                        return;
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        ///     重新结算订单, 有可能补分错误的。从这里结算
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 从本期重新结算ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //启动重新结算流程
            //只能单选开始--
            try
            {
                V2MemberOrder order_select = null;
                var loger = MainConfigure.boterServices.loglite;
                var wxhelper = MainConfigure.boterServices.wxHelper;
                var v2Orders = MainConfigure.boterServices.v2memberOderbindlite;
                var v2Members = MainConfigure.boterServices.v2Memberbindlite;
                var boterservices = MainConfigure.boterServices;

                //要检测绑定群了, 才给结算..
                //代码暂无

                int index = dgv_orders.CurrentRow.Index;
                if (index >= 0)
                {
                    order_select = dgv_orders.CurrentRow.DataBoundItem as V2MemberOrder;
                    string msg_begin = $"系统: {order_select.nickname} 从 {order_select.IssueId} 起重新结算!";
                    loger.Add(Log.Create(msg_begin, order_select.BetContentOriginal));
                    wxhelper.CallSendText_11036(order_select.GroupWxId, msg_begin);
                    //把这个人订单全部找出来
                    bool isBegin = false;
                    V2MemberOrder orderLast = null;
                    string wxid = "";
                    string groupid = "";
                    for (int i = 0; i < v2Orders.Count(); i++)
                    {
                        var order = v2Orders[i];
                        if (order.GetHashCode() == order_select.GetHashCode())
                        {
                            //开始结算-第一期
                            isBegin = true;
                            //1、得到开奖数据
                            var response = MainConfigure.getBotApi().getBgdata(order.IssueId);
                            if (response.code == 0)
                            {
                                if (response.data != null)
                                {
                                    var lotterydata = new BgLotteryData();
                                    lotterydata.FillLotteryData(response.data.issueid, response.data.lotteryData, response.data.lottery_time);
                                    string outmsg;
                                    wxid = order.wxid;
                                    groupid = order.GroupWxId;
                                    if (boterservices.On补分(order.wxid, order.id, lotterydata, MainConfigure.appSetting.wxOdds, out outmsg)) //成功后的补分信息
                                    {
                                        orderLast = order;
                                    }
                                    wxhelper.CallSendText_11036(order_select.GroupWxId, outmsg);
                                }
                            }
                            continue;
                        }
                        if(isBegin)
                        {
                            if(order.wxid == order_select.wxid && order.GroupWxId == order_select.GroupWxId)
                            {
                                if(orderLast != null)
                                {
                                    var response = MainConfigure.getBotApi().getBgdata(order.IssueId);
                                    if (response.code == 0)
                                    {
                                        if (response.data != null)
                                        {
                                            //修正数据。结算。
                                            var lotterydata = new BgLotteryData();
                                            lotterydata.FillLotteryData(response.data.issueid, response.data.lotteryData, response.data.lottery_time);
                                            string outmsg;
                                            int member_last_money = (int)orderLast.BetAfterMoney + (int)orderLast.Profit;
                                            if ((int)order.BetFronMoney != member_last_money)
                                            {
                                               // wxhelper.CallSendText_11036(order_select.GroupWxId, $"余额校正:{order.nickname}, 进货前:{order.BetFronMoney} => {member_last_money}, 进货后:{order.BetAfterMoney} => {member_last_money - order.AmountTotal}");
                                                order.BetFronMoney = member_last_money;
                                                order.BetAfterMoney = member_last_money - order.AmountTotal;
                                            }
                                            if (boterservices.On补分(order.wxid, order.id, lotterydata, MainConfigure.appSetting.wxOdds, out outmsg)) //成功后的补分信息
                                            {
                                                orderLast = order;
                                            }
                                            //wxhelper.CallSendText_11036(order_select.GroupWxId, outmsg);
                                            loger.Add(Log.Create($"重新结算:{order.nickname}", outmsg));
                                        }
                                    }

                                }
                            }
                        }
                    }

                    if(orderLast != null)
                    {
                        var member = v2Members.FirstOrDefault(p => p.wxid == wxid && p.GroupWxId == groupid);
                        var orginBalance = member.Balance;
                        string member_name = "空";
                        if(member != null)
                        {
                            member.Balance = (int)orderLast.BetAfterMoney + (int)orderLast.Profit;
                            member_name = member.nickname;
                        }
                        
                        //查询总上下分.计算盈利

                        wxhelper.CallSendText_11036(groupid, $"{member_name}余额: {orginBalance} => {member.Balance}");
                    }
                }
            }
            catch(Exception ex)
            {

            }
        }

        private void 重校余额盈亏ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int index = dgv_orders.CurrentRow.Index;
            if (index >= 0)
            {
                //得到会员ID
                //order_select = dgv_orders.CurrentRow.DataBoundItem as V2MemberOrder;
                foreach(var mem in MainConfigure.boterServices.v2Memberbindlite)
                {
                    int ye_result = (int)(mem.CreditToday - mem.WithdrawToday + mem.IncomeTodayStart - mem.BetWait);
                    //得到会员订单盈利
                    if (ye_result != (int)mem.Balance)
                    {
                       var mret = MessageBox.Show($"会员:{mem.nickname}\r\n余额多维校验不正确\r\n校验金额={ye_result},盈利={mem.IncomeTodayStart}\r\n记录金额={mem.Balance},盈利={mem.IncomeToday}\r\n是否用校验金额修正余额！", "警告!",MessageBoxButtons.YesNo);
                        if(mret == DialogResult.Yes)
                        {
                            mem.Balance = ye_result;
                            mem.IncomeToday = mem.IncomeTodayStart;
                        }
                    }
                }
                MessageBox.Show("处理完成!");
            }
        }

        private void 设置待结算ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //设置
            var loger = MainConfigure.boterServices.loglite;
            if (dgv_orders.CurrentRow != null)
            {
                var order = (V2MemberOrder)dgv_orders.CurrentRow.DataBoundItem;
                if (order != null)
                {
                    if (order.OrderStatus == OrderStatusEnum.已完成 ||
                        order.OrderStatus == OrderStatusEnum.已取消)
                    {
                        MessageBox.Show("已完成|已取消,的订单不能设置!");
                        return;
                    }
                    loger.Add(Log.Create($"设置待结算:{order.nickname}/{order.id}", $"原订单设置前数据:{order.id}/{order.BetContentOriginal}/{order.OrderStatus}"));
                    order.OrderStatus = OrderStatusEnum.待结算;
                }
            }
        }

        private void dgv_orders_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (e.RowIndex >= 0)
                {
                    //若行已是选中状态就不再进行设置
                    if (dgv_orders.Rows[e.RowIndex].Selected == false)
                    {
                        dgv_orders.ClearSelection();
                        dgv_orders.Rows[e.RowIndex].Selected = true;
                    }
                    //只选中一行时设置活动单元格，同时切换当前行
                    if (dgv_orders.SelectedRows.Count == 1)
                    {
                        dgv_orders.CurrentCell = dgv_orders.Rows[e.RowIndex].Cells[e.ColumnIndex];
                    }
                    //弹出操作菜单
                    //contextMenuStrip1.Show(MousePosition.X, MousePosition.Y);
                }
            }
        }

        private void dgv_members_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (e.RowIndex >= 0)
                {
                    //若行已是选中状态就不再进行设置
                    if (dgv_members.Rows[e.RowIndex].Selected == false)
                    {
                        dgv_members.ClearSelection();
                        dgv_members.Rows[e.RowIndex].Selected = true;
                    }
                    //只选中一行时设置活动单元格，同时切换当前行
                    if (dgv_members.SelectedRows.Count == 1)
                    {
                        dgv_members.CurrentCell = dgv_members.Rows[e.RowIndex].Cells[e.ColumnIndex];
                    }
                    //弹出操作菜单
                    //contextMenuStrip1.Show(MousePosition.X, MousePosition.Y);
                }
            }
        }

        private void 设置已结算ToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void btnBetString_Click(object sender, EventArgs e)
        {
            //
            //解析词法
            string command = "12大345小125单34双10";
            var boter = MainConfigure.boterServices;
            BoterBetContents betcontents = new BoterBetContents(boter.IssueidCur, command);
            if (betcontents.code != 0)
            {
                if (betcontents.code != -1)
                {
                    MessageBox.Show("code:-1", "收单失败");
                    return;
                }
            }

            List<V2MemberOrder> orders = new List<V2MemberOrder>();
            orders.Add(new V2MemberOrder(
                                   new V2Member()
                , boter.IssueidCur
                , LxTimestampHelper.GetTimeStamp13().ToInt32()
                , betcontents.msg_origin
                , betcontents.ToStandarString()
                , betcontents.GetCount()
                , betcontents.GetAmountTatol()) {
            }); ;
            //投递订单
            var status = boter.BetOrder(boter.IssueidCur, orders);
        }
    }
}
