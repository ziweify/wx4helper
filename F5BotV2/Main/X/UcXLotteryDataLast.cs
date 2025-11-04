using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Linq.Expressions;
using System.Diagnostics;
using F5BotV2.Model;
using LxLib.LxSys;
using CCWin.SkinControl;

namespace F5Bot.Main.X
{
    public partial class UcXLotteryDataLast : UserControl
        //, INotifyPropertyChanged
    {
        //private BotLocalServices bot;
        //private BgLotteryData data;

        #region 
        //public event PropertyChangedEventHandler PropertyChanged;
        //public void NotifyPropertyChanged<T>(Expression<Func<T>> property)
        //{
        //    if (PropertyChanged == null)
        //        return;

        //    var memberExpression = property.Body as MemberExpression;
        //    if (memberExpression == null)
        //        return;

        //    PropertyChanged.Invoke(this, new PropertyChangedEventArgs(memberExpression.Member.Name));
        //}


        //private int _issue = -1;
        //public int issue
        //{
        //    get { return _issue; }
        //    set
        //    {
        //        if (_issue == value)
        //            return;
        //        _issue = value;
        //        NotifyPropertyChanged(() => issue);
        //    }
        //}

        //private string _preTime = "";
        //public string preTime
        //{
        //    get { return _preTime; }
        //    set
        //    {
        //        if (_preTime == value)
        //            return;
        //        _preTime = value;
        //        NotifyPropertyChanged(() => preTime);
        //    }
        //}

        //private int _B1 = -1;
        //public int B1
        //{
        //    get { return _B1; }
        //    set
        //    {
        //        if (_B1 == value)
        //            return;
        //        _B1 = value;
        //        NotifyPropertyChanged(() => B1);
        //    }
        //}

        //private int _B2 = -1;
        //public int B2
        //{
        //    get { return _B2; }
        //    set
        //    {
        //        if (_B2 == value)
        //            return;
        //        _B2 = value;
        //        NotifyPropertyChanged(() => B2);
        //    }

        //}

        //private int _B3 = -1;
        //public int B3
        //{
        //    get { return _B3; }
        //    set
        //    {
        //        if (_B3 == value)
        //            return;
        //        _B3 = value;
        //        NotifyPropertyChanged(() => B3);
        //    }
        //}

        //private int _B4 = -1;
        //public int B4
        //{
        //    get { return _B4; }
        //    set
        //    {
        //        if (_B4 == value)
        //            return;
        //        _B4 = value;
        //        NotifyPropertyChanged(() => B4);
        //    }
        //}
        //private int _B5 = -1;
        //public int B5
        //{
        //    get { return _B5; }
        //    set
        //    {
        //        if (_B5 == value)
        //            return;
        //        _B5 = value;
        //        NotifyPropertyChanged(() => B5);
        //    }
        //}

        //private string _B6dx = "";
        //public string B6dx
        //{
        //    get { return _B6dx; }
        //    set
        //    {
        //        if (_B6dx == value)
        //            return;
        //        _B6dx = value;
        //        NotifyPropertyChanged(() => B6dx);
        //    }
        //}

        //private string _B6ds = "";
        //public string B6ds
        //{
        //    get { return _B6ds; }
        //    set
        //    {
        //        if (_B6ds == value)
        //            return;
        //        _B6ds = value;
        //        NotifyPropertyChanged(() => B6ds);
        //    }
        //}

        //private string _B6lh = "";
        //public string B6lh
        //{
        //    get { return _B6lh; }
        //    set
        //    {
        //        if (_B6lh == value)
        //            return;
        //        _B6lh = value;
        //        NotifyPropertyChanged(() => B6lh);
        //    }
        //}
        #endregion
        private BgLotteryData _data;
        public BgLotteryData data { get { return _data; } }
        private int _IssueId = 0;
        public int IssueId { get { return _IssueId; } set { _IssueId = value; } }

        /// <summary>
        ///     但是传入bot是为了想监听事件, 和消息
        ///     不是为了数据绑定
        /// </summary>
        /// <param name="bot"></param>
        public UcXLotteryDataLast()
        {
            InitializeComponent();

            //this.bot = BotLocalServices.GetInstance();
            //data = BotLocalServices.GetInstance().getLast();
            //data.PropertyChanged += Data_PropertyChanged;
        }

        /// <summary>
        ///     数据升级
        /// </summary>
        public void LotteryUpdata(BgLotteryData data)
        {
            this._data = data;
            lblIssue.Text = data.IssueId.ToString();
            lblOpenTime.Text = DateTime.Parse(data.opentime).ToLongTimeString();
            _IssueId = data.IssueId;
            setLotteryNumber(P1, data.P1);
            setLotteryNumber(P2, data.P2);
            setLotteryNumber(P3, data.P3);
            setLotteryNumber(P4, data.P4);
            setLotteryNumber(P5, data.P5);

            //总数据-------------------------------------------
            if (data.P总 != null)
            {
                Zdx.Text = data.P总.dx.ToString();
                Zds.Text = data.P总.ds.ToString();
                if (data.P1.number > data.P5.number)
                    Zlh.Text = "龙";
                else
                    Zlh.Text = "虎";
            }
            else
            {
                Zdx.Text = "X";
                Zds.Text = "X";
                Zlh.Text = "X";
            }

        }

        private void setLotteryNumber(SkinButton btn, LotteryNumber number)
        {
            if (number == null)
                btn.Text = "*";
            else
            {
                btn.Text = number.number.ToString();
                if (number.dx == NumberDX.大)
                {
                    btn.BackColor = Color.Red;
                }
                else
                {
                    btn.BackColor = Color.Green;
                }
            }
        }

        private void UcLotteryData_Load(object sender, EventArgs e)
        {

            //绑定BotServices数据
            //显示内容: 上一期: 111065586 25,49,38,60,80 大单虎  - 23:56
            //bot.getLast().preDrawIssue = 0;
            //lblIssue.DataBindings.Add("Text", data, "preDrawIssue");

            ////接口数据和这个不一致, 临时在这里转换下。
            //lblOpenTime.DataBindings.Add("Text", this, "preTime");
            //P1.DataBindings.Add("Text", this, "B1");
            //P2.DataBindings.Add("Text", this, "B2");
            //P3.DataBindings.Add("Text", this, "B3");
            //P4.DataBindings.Add("Text", this, "B4");
            //P5.DataBindings.Add("Text", this, "B5");    
        }

        /// <summary>
        ///     
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Data_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //额外处理
            //BgLotteryData data = (BgLotteryData)sender;
            //try
            //{
            //    switch (e.PropertyName)
            //    {
            //        case "preDrawIssue":
            //            {
            //                this.Invoke(new Action(() => { preDrawIssue(data); }));
            //                break;
            //            }
            //        case "preDrawCode":
            //            {
            //                this.Invoke(new Action(() => { preDrawCode(data.preDrawCode); }));
            //                break;
            //            }
            //        case "kjtime":
            //            {
            //                this.Invoke(new Action(() => { kjtime(data.kjtime); }));
            //                break;
            //            }
            //        default:
            //            break;
            //    }
            //}
            //catch(Exception ee)
            //{
            //  // Main.Configure.ShowLog()
            //}
       
        }

        /// <summary>
        ///     
        /// </summary>
        /// <param name="data"></param>
        private void preDrawIssue(BgLotteryData data)
        {
            lblIssue.Text = data.IssueId.ToString();

            ////检查更新开奖数据
            ////每期只会出发一次, 如果没开, 就一直获取
            //Task.Factory.StartNew(()=> {
            //    //var li = bot.botServices.GetLottoryInfo();
            //});
        }

        private void preDrawCode(string code)
        {
            try
            {
                string[] p = code.Split(',');
                if(p.Count() == 5)
                {
                    if (p[0] == "*")
                        throw new Exception("");
                    P1.Text = p[0];
                    P2.Text = p[1];
                    P3.Text = p[2];
                    P4.Text = p[3];
                    P5.Text = p[4];
                    int zong = 0;
                    foreach(var v in p)
                    {
                        zong += Convert.ToInt32(v.ToString());
                    }
                    if(zong>0)
                    {
                        if (zong >= 203)
                        {
                            Zdx.Text = "大";
                        }
                        else
                        {
                            Zdx.Text = "小";
                        }
                        if(zong%2==0)
                        {
                            Zds.Text = "双";
                        }
                        else
                        {
                            Zds.Text = "单";
                        }
                        var one = Convert.ToInt32(p[0]);
                        var two = Convert.ToInt32(p[4]);
                        if(one>two)
                        {
                            Zlh.Text = "龙";
                        }
                        else
                        {
                            Zlh.Text = "虎";
                        }
                    }
                   
                }
                else
                {
                    throw new Exception("");
                }

            }
            catch
            {
                P1.Text = "*";
                P2.Text = "*";
                P3.Text = "*";
                P4.Text = "*";
                P5.Text = "*";
                Zdx.Text = "开";
                Zds.Text = "奖";
                Zlh.Text = "中";
            }
        }

        private void kjtime(int timeStamp)
        {
            try
            {
                var date = LxTimestampHelper.GetDateTime(timeStamp);
                lblOpenTime.Text = date.ToString("HH:mm:ss");
            }
            catch
            {
                lblOpenTime.Text = "??:??:??";
            }
            
        }
    }
}
