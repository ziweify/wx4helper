using CCWin;
using CCWin.SkinControl;
using CsQuery.StringScanner.Patterns;
using F5BotV2.BetSite.Boter;
using F5BotV2.Boter;
using F5BotV2.Model;
using F5BotV2.Wx;
using LxLib.LxSys;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Forms;

namespace F5BotV2.Main
{
    public partial class BetOrderManager : CCSkinMain
    {
        private BgLotteryData _lotteryOpenData;   //补分当期的数据
        private BoterServices _boter;
        private V2MemberOrderBindlite _orderslite;
        private BindingList<V2MemberOrder> _orders_result = new BindingList<V2MemberOrder>();

        public BetOrderManager(BoterServices boter, V2MemberOrderBindlite orders)
        {
            InitializeComponent();
            this._orderslite = orders;
            this._boter = boter;
        }

        private void BetOrderManager_Load(object sender, EventArgs e)
        {
            DgvInit(dgv_orders);
        }

        private void BetOrderManager_Shown(object sender, EventArgs e)
        {

        }

        private void DgvInit(SkinDataGridView dgv)
        {
            dgv.DataSource = _orders_result;
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

            var cell = dgv.Columns["id"]; cell.Visible = true; cell.Width = 46;
            cell = dgv.Columns["nickname"]; cell.Visible = true; cell.Width = 80;
            dgv.Columns["wxid"].Visible = false;
            dgv.Columns["account"].Visible = false;
            dgv.Columns["avatar"].Visible = false;
            dgv.Columns["city"].Visible = false;
            dgv.Columns["country"].Visible = false;
            dgv.Columns["province"].Visible = false;
            dgv.Columns["remark"].Visible = false;
            dgv.Columns["sex"].Visible = false;
            dgv.Columns["TimeStampBet"].Visible = false;
            dgv.Columns["GroupWxId"].Visible = false;
            cell = dgv.Columns["BetContentOriginal"]; cell.Visible = true; cell.Width = 120;
            cell = dgv.Columns["BetContentStandar"]; cell.Visible = true; cell.Width = 120;
            cell = dgv.Columns["BetFronMoney"]; cell.Visible = true; cell.Width = 46;
            cell = dgv.Columns["BetAfterMoney"]; cell.Visible = true; cell.Width = 46;
            cell = dgv.Columns["Nums"]; cell.Visible = true; cell.Width = 46;
            cell = dgv.Columns["AmountTotal"]; cell.Visible = true; cell.Width = 46;
            cell = dgv.Columns["Profit"]; cell.Visible = true; cell.Width = 46;
            cell = dgv.Columns["OrderStatus"]; cell.Visible = true; cell.Width = 46;
            cell = dgv.Columns["OrderType"]; cell.Visible = true; cell.Width = 46;
        }

        private void BetOrderManager_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }


        /// <summary>
        ///     查询
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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


            var report = _orderslite.sql.getTabble().Where(x => x.TimeStampBet >= todayStamp && x.TimeStampBet < tomorrowStamp);
            if(issueid != 0)
                report = report.Where(x=>x.IssueId == issueid);
            if (bNotPay)
                report = report.Where(x=>x.OrderStatus == OrderStatusEnum.待处理 || x.OrderStatus == OrderStatusEnum.待结算);


            var response = report.ToList();
            _orders_result.Clear();
            foreach(var res in response)
            {
                _orders_result.Add(res);
            }



                //this.lblShow.Text = $"下注：{bet2} | 盈亏：{profit} | 未结算：{mm2}";

        }

        /// <summary>
        ///     补分结算
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPay_Click(object sender, EventArgs e)
        {
            //foreach(var order in _orders_result)
            //{
            //    //得到最新状态
            //    if(order.OrderStatus == OrderStatusEnum.待结算)
            //    {

            //    }
            //}
        }

        private void dgv_orders_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0)
                {
                    if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right)
                    {
                        var ss = dgv_orders.CurrentCell.RowIndex;
                        if (ss != e.RowIndex)
                        {
                            dgv_orders.ClearSelection();
                            dgv_orders.CurrentCell = dgv_orders.Rows[e.RowIndex].Cells[e.ColumnIndex];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("选择异常! \r\n请左键点击再次选择后, 再操作!");
            }
        }


        private void 补分ToolStripMenuItem_Click(object sender, EventArgs e)
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
                            _boter.loglite.Add(Log.Create($"补分失败::获取开奖数据错误::{order.IssueId}", errMsg));
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


                    if (_boter.groupBind == null)
                    {
                        MessageBox.Show("没有绑定群组! 不能补分!");
                        return;
                    }

                    if (_boter.groupBind.wxid != order.GroupWxId)
                    {
                        var msgret = MessageBox.Show("该订单与目前绑定的群组不一致! \r\n订单不是这个群的\r\n您确定要补该订单吗?", "警告", MessageBoxButtons.YesNo);
                        if (msgret != DialogResult.No)
                            return;
                    }


                    var m = _boter.v2Memberbindlite.FirstOrDefault(p => p.wxid == order.wxid && p.GroupWxId == order.GroupWxId);
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

                    var member_order = _boter.v2memberOderbindlite.FirstOrDefault(p=>p.id == order.id && p.BetContentOriginal == order.BetContentOriginal);

                    //if (OnAction补分(m, member_order, _lotteryOpenData))
                    
                    if (_boter.OnMemberOrderFinish(order, _lotteryOpenData))
                    {
                        string shengStrList = "----补分名单----\r";
                        shengStrList += $"{member_order.nickname}|{member_order.IssueId % 1000}|{_lotteryOpenData.ToLotteryString()}|{member_order.BetContentOriginal}|{member_order.Profit- member_order.AmountTotal}\r";

                        string shengStrList2 = "------补完留分------\r";
                        shengStrList2 += $"{member_order.nickname} | {m.Balance}";

                        _boter.wxHelper.CallSendText_11036(member_order.GroupWxId, shengStrList);
                        _boter.wxHelper.CallSendText_11036(member_order.GroupWxId, shengStrList2);
                    }
                    //_orderslite.Clear();
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show($"出现了小小的意外:{ex.Message}");
            }

        }

        private void 全部补分ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("禁止使用该功能! 请使用 补分, 一单单结算!");
            //string bufen_list = "";
            //string liufen_list = "";
            //List<string> item_member = new List<string>();  //补过分的会员

            //try
            //{
            //    foreach (var order in _orderslite)
            //    {
            //        if (_lotteryOpenData == null || _lotteryOpenData.IssueId != order.IssueId)
            //        {
            //            var response = MainConfigure.getBotApi().getBgdata(order.IssueId);
            //            if (response.code == 0)
            //            {
            //                if (response.data != null)
            //                {
            //                    //这里要重新封装下，要能一步到位得到BgLotteryData对象,因为开奖结果页面，也有这个方法, 也需要获取某期开奖数据
            //                    _lotteryOpenData = new BgLotteryData();
            //                    _lotteryOpenData.FillLotteryData(response.data.issueid
            //                    , response.data.lotteryData
            //                    , response.data.lottery_time);
            //                }
            //            }
            //            else
            //            {
            //                string errMsg = $"获取开奖数据失败! {JsonConvert.SerializeObject(response)}";
            //                _boter.loglite.Add(Log.Create($"补分失败::获取开奖数据错误::{order.IssueId}", errMsg));
            //                MessageBox.Show(errMsg);
            //                return;
            //            }
            //        }


            //        if (_lotteryOpenData == null)
            //        {
            //            MessageBox.Show("未获取到开奖数据! 请稍后重试!");
            //            return;
            //        }
            //        if (_lotteryOpenData.IssueId != order.IssueId)
            //        {
            //            MessageBox.Show("开奖结果期号校验失败! 请重试!");
            //            _lotteryOpenData = null;
            //            return;
            //        }


            //        if (_boter.groupBind == null)
            //        {
            //            MessageBox.Show("没有绑定群组! 不能补分!");
            //            return;
            //        }

            //        if (_boter.groupBind.wxid != order.GroupWxId)
            //        {
            //            var msgret = MessageBox.Show("该订单与目前绑定的群组不一致! \r\n订单不是这个群的\r\n您确定要补该订单吗?", "警告", MessageBoxButtons.YesNo);
            //            if (msgret != DialogResult.No)
            //                return;
            //        }


            //        var m = _boter.v2Memberbindlite.FirstOrDefault(p => p.wxid == order.wxid && p.GroupWxId == order.GroupWxId);
            //        if (m == null)
            //        {
            //            MessageBox.Show("没有在目前绑定的群中找到该会员数据! \r\n无法补分!");
            //            return;
            //        }

            //        if(_boter.OnMemberOrderFinish(order, _lotteryOpenData))
            //        {
            //            bufen_list += $"{order.nickname}|{order.IssueId % 1000}|{_lotteryOpenData.ToLotteryString()}|{order.BetContentOriginal}|{(int)order.Profit - (int)order.AmountTotal}\r";

            //            //留分名单那
            //            if (item_member.FirstOrDefault(p => p == order.wxid) == null)
            //            {
            //                item_member.Add(order.wxid);
            //            }
            //        }

            //        //if (OnAction补分(m, order, _lotteryOpenData))
            //        //{

                        
            //        //}

            //    }

            //    //补分, 留分
            //    if (!string.IsNullOrEmpty(bufen_list))
            //    {
            //        _boter.wxHelper.CallSendText_11036(_boter.groupBind.wxid, $"----补分名单----\r{bufen_list}\r");
            //        foreach (var member_wxid in item_member)
            //        {
            //            var m = _boter.v2Memberbindlite.FirstOrDefault(p => p.wxid == member_wxid && p.GroupWxId == _boter.groupBind.wxid);
            //            {
            //                liufen_list += $"{m.nickname} | {(int)m.Balance}\r";
            //            }
            //        }
            //        _boter.wxHelper.CallSendText_11036(_boter.groupBind.wxid, $"------补完留分------\r{liufen_list}\r");
            //    }
            //}
            //catch(Exception ex)
            //{
            //    MessageBox.Show($"出现了小小的意外:{ex.Message}");
            //}
        }

        /// <summary>
        ///     
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        private bool OnAction补分(V2Member member, V2MemberOrder order, BgLotteryData bgData)
        {
            bool response = true;
            try
            {
                if(!(MainConfigure.appSetting.wxOdds >= 1.00f  && MainConfigure.appSetting.wxOdds <= 2.00f))
                {
                    MessageBox.Show("赔率错误! 必须 1-2 之间");
                    return false;
                }

                if(order.OrderStatus == OrderStatusEnum.已完成)
                {
                    return false;
                }

                order.OpenLottery(bgData, MainConfigure.appSetting.wxOdds, MainConfigure.appSetting.Zsjs);
                member.OpenLottery(order);
                //机器人盈亏，是相反的，用总金额-会员盈利
                _boter.IncomeTotal += (order.AmountTotal - order.Profit);
                _boter.IncomeToday += (order.AmountTotal - order.Profit);
            }
            catch
            {
                return false;
            }
            return response;
        }


    }
}
