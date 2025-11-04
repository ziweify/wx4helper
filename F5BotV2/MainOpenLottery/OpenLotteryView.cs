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

namespace F5BotV2.MainOpenLottery
{
    public partial class OpenLotteryView : CCSkinMain
    {
        //默认可视状态
        bool visual = false;
        private BgLotteryDataBindlite _lotteryDatalite;
        public BgLotteryDataBindlite lotteryDatalite { get { return _lotteryDatalite; } }   //在Boter里面初始化的


        public OpenLotteryView(BgLotteryDataBindlite lotterylite)
        {
            InitializeComponent();
            this._lotteryDatalite = lotterylite;


        }

        private void btn_queryLotteryDataDay_Click(object sender, EventArgs e)
        {
            //应该采集前面5组路子数据, 和单双, 连跳, 数据看趋势。来打下面的连和跳
            //具体下次分析。
            this.getdata(datetimePick.Value, tbxCookie.Text);

        }

        public void GetDgView(Func<SkinDataGridView, bool> func)
        {
            if (dgview.InvokeRequired)
            {
                dgview.Invoke(new Action(() => {
                    func(this.dgview);
                }));
            }
            else
            {
                func(this.dgview);
            }
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
            this.InitDataView(this.dgview, this.lotteryDatalite, new Func<DataGridView, bool>((p) =>
            {
                var cell = p.Columns["IssueId"];
                if (cell != null)
                {
                    cell.Width = 75;
                }
                cell = p.Columns["lotteryData"];
                if (cell != null)
                {
                    //cell.Width = 75;
                    cell.Visible = false;
                }
                cell = p.Columns["P1"];
                if (cell != null)
                {
                    cell.Width = 26;
                }
                cell = p.Columns["P2"];
                if (cell != null)
                {
                    cell.Width = 26;
                }
                cell = p.Columns["P3"];
                if (cell != null)
                {
                    cell.Width = 26;
                }
                cell = p.Columns["P4"];
                if (cell != null)
                {
                    cell.Width = 26;
                }
                cell = p.Columns["P5"];
                if (cell != null)
                {
                    cell.Width = 26;
                }
                cell = p.Columns["P总"];
                if (cell != null)
                {
                    cell.Width = 39;
                }
                cell = p.Columns["P龙虎"];
                if (cell != null)
                {
                    cell.Width = 26;
                }
                return true;
            }));

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

        private async Task<int> UpdateNow(int issue_start)
        {
            return await Task.Factory.StartNew((p) =>
            {
                int issue_index = (int)issue_start;
                string connectionString = "server=8.134.59.220;user=api_bg;database=api_bg;password=twMFWtCrwjGr2ACC;port=32758";
                MySqlConnection connection = new MySqlConnection(connectionString);
                connection.Open();
                try
                {
                    //执行查询
                    string sqlQuery = $"SELECT * FROM eb_kj where qishu > {issue_index}";
                    MySqlCommand command = new MySqlCommand(sqlQuery, connection);
                    MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        try
                        {
                            // 处理每一行数据
                            int id = reader.GetInt32("id");
                            int gid = reader.GetInt32("gid");
                            int dates = reader.GetInt32("dates");
                            int qishu = reader.GetInt32("qishu");
                            int kjtime = reader.GetInt32("kjtime");
                            string m1 = reader.GetString("m1");
                            string m2 = reader.GetString("m2");
                            string m3 = reader.GetString("m3");
                            string m4 = reader.GetString("m4");
                            string m5 = reader.GetString("m5");
                            string m6 = reader.GetString("m6");
                            string m7 = reader.GetString("m7");
                            string m8 = reader.GetString("m8");
                            string m9 = reader.GetString("m9");
                            string m10 = reader.GetString("m10");
                            string m11 = reader.GetString("m11");
                            string m12 = reader.GetString("m12");
                            string m13 = reader.GetString("m13");
                            string m14 = reader.GetString("m14");
                            string m15 = reader.GetString("m15");
                            string m16 = reader.GetString("m16");
                            string m17 = reader.GetString("m17");
                            string m18 = reader.GetString("m18");
                            string m19 = reader.GetString("m19");
                            string m20 = reader.GetString("m20");
                            string m21 = reader.GetString("m21");
                            // 其他字段...
                            //从112期开始录入,也就是今年的数据
                            if (qishu >= BinGouHelper.getNextIssueId(DateTime.Now))
                            {
                                Debug.WriteLine($"更新数据结束: issueid = {qishu}");
                                break;
                            }
                            if (qishu > 112000000)
                            {
                                Debug.WriteLine($"issue_id: {qishu}, data: {m1},{m2},{m3},{m4},{m5}");
                                StringBuilder sbPostData = new StringBuilder(512);
                                sbPostData.Append($"issueid={qishu}");
                                sbPostData.Append($"&lotteryCode={m1},{m2},{m3},{m4},{m5},{m6},{m7},{m8},{m9},{m10},");
                                sbPostData.Append($"{m11},{m12},{m13},{m14},{m15},{m16},{m17},{m18},{m19},{m20},{m21}");
                                sbPostData.Append($"&token=KkoN4bx5Gp7ShJdj");
                                string postdata = sbPostData.ToString();

                                //准备录入新数据库
                                LxHttpHelper http = new LxHttpHelper();
                                HttpItem item = new HttpItem()
                                {
                                    URL = "http://www.lvboter.com/api/boter/uploadbg?" + postdata,
                                    Method = "POST",
                                    Postdata = postdata,
                                };
                                HttpResult hr = http.GetHtml(item);
                                BoterApiResponse<string> response = JsonConvert.DeserializeObject<BoterApiResponse<string>>(hr.Html);
                                if (response.msg == "成功")
                                {
                                    Debug.WriteLine($"更新数据成功: issueid = {qishu}");
                                    issue_index = qishu;
                                }
                                else
                                {
                                    string msg = $"更新数据失败,退出继续: issueid = {qishu}, html={hr.Html}";
                                    Debug.WriteLine(msg);
                                    break;
                                    //MessageBox.Show(msg);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"更新数据失败0:{ex.Message},退出继续");
                            break;
                        }
                    }
                    reader.Close();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"更新数据失败1:{ex.Message}");
                }
                connection.Close();
                return issue_index;
            }, (Object)issue_start);
        }

        private void btn_getall_sql_Click(object sender, EventArgs e)
        {
            string strIndex = LxIniFileHelper.getString("配置", "索引", "112000001", "boter.ini");
            int issue_index = Conversion.ToInt32(strIndex);
            issue_index = 112052494; //112051917
            //获取sqlite所有数据
            Task.Factory.StartNew(async () => { 
                while(true)
                {
                    Debug.WriteLine($"---------------------------------------------------------------------------");
                    Debug.WriteLine($"更新数据开始:{issue_index}");
                    issue_index = await UpdateNow(issue_index);
                    Thread.Sleep(1000);
                }
            });
             
        }

        private void btnOpenLotteryForIssue_Click(object sender, EventArgs e)
        {
            string strIssueid = tbxIssueid.Text.Replace(" ", "");
            int issueid = 0;
            try
            {
                issueid = Convert.ToInt32(strIssueid);
            }
            catch {
                issueid = BinGouHelper.getNextIssueId(DateTime.Now);
                issueid = issueid - 1;
            }
            var response = MainConfigure.getBotApi().getBgdata(issueid);
            if (response.code == 0)
            {
                if (response.data != null)
                {
                    BgLotteryData bgData = new BgLotteryData();



                    _lotteryDatalite.Add(bgData.FillLotteryData(response.data.issueid
                    , response.data.lotteryData
                    , response.data.lottery_time));
                }
            }
        }

        private void btnGetIssueCur_Click(object sender, EventArgs e)
        {
            int issueid =  BinGouHelper.getNextIssueId(DateTime.Now);
            tbxIssueid.Text = issueid.ToString();
        }
    }
}
