using F5BotV2.Boter;
using F5BotV2.Main;
using F5BotV2.Model;
using LxLib.LxSys;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Forms;

namespace F5Bot.Main.X
{
    public partial class UcXBuySellCoins : UserControl
    {
        private V2MemberCoinsBuySell _data;
        public V2MemberCoinsBuySell data { get { return _data; } }
        public UcXBuySellCoins()
        {
            InitializeComponent();
            SetMemberCoinsBuySell(null);
        }

        private void lbl_note_Click(object sender, EventArgs e)
        {

        }


        /// <summary>
        ///     设置数据
        /// </summary>
        /// <param name="data"></param>
        public void SetMemberCoinsBuySell(V2MemberCoinsBuySell data)
        {
            if (data != null)
                if (_data != null)
                    return;

            this._data = data;
            if(data == null)
            {
                btn_NikeName.Text = "";
                lbl_Datetime.Text = "";
                btn_PayAction.Text = "";
                lbl_money.Text = "";
                btn_agree.Visible = false;
                btn_ignore.Visible = false;
            }
            else
            {
                this._data = data;
                btn_NikeName.Text =$"会员({this.data.id}):{this.data.nickname}";
                string datetimeStr = "";
                try
                {
                    datetimeStr = LxTimestampHelper.strtodatetime(this.data.Timestring).ToString("HH:mm::ss");
                }
                catch { datetimeStr = "错误"; }
                lbl_Datetime.Text = datetimeStr;
                btn_PayAction.Text = this.data.PayAction.ToString();
                lbl_money.Text = this.data.Money.ToString();
                btn_agree.Visible = true;
                btn_ignore.Visible = true;
            }

        }

        private void btn_agree_Click(object sender, EventArgs e)
        {
            try
            {
                if (data.Money == 0)
                    throw new Exception("金额不能是0");
                if (data == null)
                    throw new Exception("错误:400");
                var boter = MainConfigure.boterServices;
                
                if(_data.PayAction == V2MemberPayAction.上分)
                {
                    boter.OnActionMemberCredit(_data);   //上分
                }
                else if(_data.PayAction == V2MemberPayAction.下分)
                {
                    boter.OnActionMemberWithdraw(_data);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btn_ignore_Click(object sender, EventArgs e)
        {
            var boter = MainConfigure.boterServices;
            var member = boter.v2Memberbindlite.FirstOrDefault(p => p.wxid == data.wxid);
            boter.wxHelper.CallSendText_11036(data.GroupWxId, $"@{member.nickname}\r[{member.id}]{data.PayAction.ToString().Replace("分","")}{data.Money}|已取消!");
            this.data.PayStatus = V2MemberPayStatus.忽略;
        }

        private void btn_AllOrder_Click(object sender, EventArgs e)
        {
            //打开所有订单UI
            var view = MainConfigure.boterServices.coninsView;
            BoterServices.ShowWindows(view);
        }
    }
}
