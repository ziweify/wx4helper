using CCWin;
using F5BotV2.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using MySql.Data.MySqlClient;
using System.Diagnostics;
using CCWin.SkinClass;
using LxLib.LxNet;
using F5BotV2.Game.BinGou;
using F5BotV2.BetSite.Boter;
using Newtonsoft.Json;
using LxLib.LxFile;
using System.Threading;
using F5BotV2.Boter;
using F5BotV2.Model.BindSqlite;
using CCWin.SkinControl;
using System.Web.UI.WebControls;
using F5BotV2.Main;
using ScottPlot;
using System.Security.RightsManagement;
using System.Runtime.CompilerServices;
using System.Windows.Forms.DataVisualization.Charting;
using System.Reflection;
using CefSharp.DevTools.Accessibility;
using F5BotV2.Model.Attribute;

namespace F5BotV2.MainOpenLottery
{
    public partial class PlanFlowerView : CCSkinMain
    {
        //默认可视状态
        bool visual = false;
        private PlanFlower _plan;


        public PlanFlowerView()
        {
            InitializeComponent();
            _plan = new PlanFlower(this);
        }


        public void GetDgView(Func<SkinDataGridView, bool> func)
        {
            if (dgview.InvokeRequired)
            {
                dgview.Invoke(new Action(() => {
                    func.Invoke(this.dgview);
                }));
            }
            else
            {
                func(this.dgview);
            }
        }

        private void btn_queryLotteryDataDay_Click(object sender, EventArgs e)
        {
            //应该采集前面5组路子数据, 和单双, 连跳, 数据看趋势。来打下面的连和跳
            //具体下次分析。
            _plan.getdata(datetimePick.Value);

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


        private void OpenLotteryView_Load(object sender, EventArgs e)
        {
            this.dgvLotteryDataInit(dgview);
            this.InitDataView(this.dgview, _plan.lotteryDatalite, new Func<DataGridView, bool>((p) =>
            {
                var t = typeof(BgLotteryView);
                foreach (PropertyInfo pi in t.GetProperties())
                {
                    string Name = pi.Name;
                    string DisplayName = pi.GetCustomAttribute<DataGridSettingAttribute>()?.DisplayName;
                    bool? Visable = pi.GetCustomAttribute<DataGridSettingAttribute>()?.Visable;
                    int? Width = pi.GetCustomAttribute<DataGridSettingAttribute>()?.Width;

                    var cell = p.Columns[Name];
                    if (cell != null)
                    {
                        if (!string.IsNullOrEmpty(DisplayName))
                            cell.HeaderText = DisplayName;
                        if (Visable != null)
                            cell.Visible = Visable.Value;
                        if (Width != null)
                            cell.Width = Width.Value;
                    }
                    //Console.WriteLine("属性名称：" + propertyName + "；显示名称：" + displayName + "；显示宽度：" + displayWidth);
                }
                return true;
            }));


            //计划页面


            //dgv_plan
            //this.dgvLotteryDataInit(dgv_plan);
            //this.InitDataView(this.dgv_plan, );
        }


        private void OpenLotteryView_Shown(object sender, EventArgs e)
        {

        }


        private void OpenLotteryView_FormClosing(object sender, FormClosingEventArgs e)
        {
            //调试的时候, 禁用这个。
            e.Cancel = true;
            this.Hide();
        }

        private void dgview_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            this.dgvLotteryDataFormatting(dgview, sender, e);
        }

        private void dgview_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            this.dgvLotteryDataCellPaintting(dgview, sender, e);
        }

       

        private void btnOpenLotteryForIssue_Click(object sender, EventArgs e)
        {
            //string strIssueid = tbxIssueid.Text.Replace(" ", "");
            //int issueid = 0;
            //try
            //{
            //    issueid = Convert.ToInt32(strIssueid);
            //}
            //catch {
            //    issueid = BinGouHelper.getNextIssueId(DateTime.Now);
            //    issueid = issueid - 1;
            //}
            //var response = MainConfigure.getBotApi().getBgdata(issueid);
            //if (response.code == 0)
            //{
            //    if (response.data != null)
            //    {
            //        BgLotteryData bgData = new BgLotteryData();



            //        _lotteryDatalite.Add(bgData.FillLotteryData(response.data.issueid
            //        , response.data.lotteryData
            //        , response.data.lottery_time));
            //    }
            //}
        }

        private void btnGetIssueCur_Click(object sender, EventArgs e)
        {
            int issueid =  BinGouHelper.getNextIssueId(DateTime.Now);
            tbxIssueid.Text = issueid.ToString();
        }

    }
}
