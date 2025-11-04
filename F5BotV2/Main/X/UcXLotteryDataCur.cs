using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using F5BotV2.Model;
using CCWin.SkinControl;
using LxLib.LxSys;
using System.Diagnostics;
using System.Threading;
using CefSharp;
using F5BotV2.Boter;

namespace F5Bot.Main.X
{
    public partial class UcXLotteryDataCur : UserControl
    {
        BgLotteryData lotteryData = null;

        public UcXLotteryDataCur()
        {
            InitializeComponent();
        }

        private void UcLotteryData_Load(object sender, EventArgs e)
        {
            Task.Factory.StartNew(() => {
                //计算时间差
                try
                {
                    while(true)
                    {
                        var date = DateTime.Now;
                        this.BeginInvoke(new Action(() => {
                            lblNowTime.Text = date.ToLongTimeString();
                        }));

                        if (lotteryData != null)
                            if (lotteryData.opentime != "")
                            {
                                var openTime = LxTimestampHelper.strtodatetime(lotteryData.opentime);
                                var ts = openTime - date;
                                this.BeginInvoke(new Action(() => {
                                    string msg = "";
                                    if (ts.Hours > 0)
                                        msg = string.Format("{0}小时{1:00}分{2:00}秒", ts.Hours, ts.Minutes, ts.Seconds);
                                    else
                                        msg = String.Format("{0:00}分{1:00}秒", ts.Minutes, ts.Seconds);
                                    lblAlsoSecond.Text = msg;
                                }));
                            }

                        Thread.Sleep(1000);
                    }
                }
                catch(Exception ex)
                {
                    Debug.WriteLine($"::{ex.Message}");
                }
            });
        }

        public void LotteryUpdata(BgLotteryData data)
        {
            try
            {
                ; this.lotteryData = data;
                lblIssue.Text = data.IssueId.ToString();
                lblOpenTime.Text = LxTimestampHelper.strtodatetime(data.opentime).ToLongTimeString();
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"LotteryUpdata::{ex.Message}");
            }
        }


        public void SetStatus(BoterStatus status)
        {
            this.lblStatus.Text = status.ToString();
        }

        private void Bot_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {

        }

        private void Data_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {

        }

        private void preDrawIssue(BgLotteryData data)
        {
            //lblIssue.Text = data.preDrawIssue.ToString();
        }



        private void skinLabel5_Click(object sender, EventArgs e)
        {

        }
    }
}
