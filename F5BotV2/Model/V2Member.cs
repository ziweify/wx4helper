using Mysqlx.Crud;
using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace F5BotV2.Model
{
    public enum MemBerState { 已删除 = -1, 非会员 = 0, 会员 = 1, 托 = 2, 管理 = 3, 已退群 = 4 }
    public class V2Member
        : IWxContacts
        , INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged<T>(Expression<Func<T>> property)
        {
            if (PropertyChanged == null)
                return;

            var memberExpression = property.Body as MemberExpression;
            if (memberExpression == null)
                return;

            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(memberExpression.Member.Name));
        }
        private IWxContacts iWxContact;

        public V2Member(IWxContacts wxContacts)
        {
            this.iWxContact = wxContacts;
            IncomeTodayStart = 0;
        }

        public V2Member()
        {
            iWxContact = new WxContacts();
            IncomeTodayStart = 0;
        }

        [PrimaryKey, AutoIncrement]
        public int id { get; set; }

        private string _GroupWxId;
        [DisplayName("群ID")]
        public string GroupWxId
        {
            get { return _GroupWxId; }
            set
            {
                if (_GroupWxId == value)
                    return;
                _GroupWxId = value;
                NotifyPropertyChanged(() => GroupWxId);
            }
        }

        [DisplayName("WxID")]
        public string wxid
        {
            get => iWxContact == null ? "" : iWxContact.wxid;
            set
            {
                if (iWxContact == null)
                    return;
                iWxContact.wxid = value;
                NotifyPropertyChanged(() => wxid);
            }
        }
        [DisplayName("号")]
        public string account
        {
            get => iWxContact == null ? "" : iWxContact.account;
            set
            {
                if (iWxContact == null)
                    return;
                iWxContact.account = value;
            }
        }

        [DisplayName("昵称")]
        public string nickname
        {
            get {
                if (string.IsNullOrEmpty(display_name))
                    return iWxContact == null ? "" : iWxContact.nickname;
                else
                    return display_name;
            }
            set
            {
                if (iWxContact == null)
                    return;
                iWxContact.nickname = value;
                NotifyPropertyChanged(() => nickname);
            }
        }

        public string avatar
        {
            get => iWxContact == null ? "" : iWxContact.avatar;
            set
            {
                if (iWxContact == null)
                    return;
                iWxContact.avatar = value;
            }
        }

        public string city
        {
            get => iWxContact == null ? "" : iWxContact.city;
            set
            {
                if (iWxContact == null)
                    return;
                iWxContact.city = value;
            }
        }

        public string country
        {
            get => iWxContact == null ? "" : iWxContact.country;
            set
            {
                if (iWxContact == null)
                    return;
                iWxContact.country = value;
            }
        }
        public string province
        {
            get => iWxContact == null ? "" : iWxContact.province;
            set
            {
                if (iWxContact == null)
                    return;
                iWxContact.province = value;
            }
        }

        public string remark
        {
            get => iWxContact == null ? "" : iWxContact.remark;
            set
            {
                if (iWxContact == null)
                    return;
                iWxContact.remark = value;
            }
        }
        public int sex
        {
            get => iWxContact == null ? 0 : iWxContact.sex;
            set
            {
                if (iWxContact == null)
                    return;
                iWxContact.sex = value;
            }
        }


        //public string WeHeadUrl
        //{
        //    get => iWxContact==null?"":iWxContact.WeHeadUrl;
        //    set
        //    {
        //        if (iWxContact == null)
        //            return;
        //        iWxContact.WeHeadUrl = value;
        //    }
        //}

        //public string WeHeadUrlBig
        //{
        //    get => iWxContact==null?"":iWxContact.WeHeadUrlBig;
        //    set
        //    {
        //        if (iWxContact == null)
        //            return;
        //        iWxContact.WeHeadUrlBig = value;
        //    }
        //}

        private string _display_name;
        [DisplayName("群昵称")]
        public string display_name
        {
            get { return _display_name; }
            set
            {
                if (_display_name == value)
                    return;
                _display_name = value;
                NotifyPropertyChanged(() => display_name);
            }
        }


        private float _Balance;
        [DisplayName("余额")]
        public float Balance
        {
            get { return _Balance; }
            set
            {
                if (_Balance == value)
                    return;
                _Balance = value;
                NotifyPropertyChanged(() => Balance);
            }
        }


        private MemBerState _State;
        [DisplayName("状态")]
        public MemBerState State
        {
            get { return _State; }
            set
            {
                if (_State == value)
                    return;
                _State = value;
                NotifyPropertyChanged(() => State);
            }
        }


        //今日收益, 今日盈亏
        private float _IncomeToday;
        [DisplayName("今日盈亏")]
        public float IncomeToday
        {
            get { return _IncomeToday; }
            set
            {
                if (_IncomeToday == value)
                    return;
                _IncomeToday = value;
                NotifyPropertyChanged(() => IncomeToday);
            }
        }

        private float _IncomeTodayStart;
        [DisplayName("今日盈亏实时"), Ignore]
        public float IncomeTodayStart
        {
            get { return _IncomeTodayStart; }
            set
            {
                if (_IncomeTodayStart == value)
                    return;
                _IncomeTodayStart = value;
                NotifyPropertyChanged(() => IncomeTodayStart);
            }
        }

        /// <summary>
        ///     当期下注, 本期下注
        /// </summary>
        private float _BetCur;
        [DisplayName("本期下注")]
        public float BetCur
        {
            get { return _BetCur; }
            set
            {
                if (_BetCur == value)
                    return;
                _BetCur = value;
                NotifyPropertyChanged(() => BetCur);
            }
        }

        /// <summary>
        ///     当期下注, 本期下注
        /// </summary>
        private float _BetWait;
        [DisplayName("待结算")]
        public float BetWait
        {
            get { return _BetWait; }
            set
            {
                if (_BetWait == value)
                    return;
                _BetWait = value;
                NotifyPropertyChanged(() => BetWait);
            }
        }

        private float _CreditToday;
        /// <summary>
        ///     今日上分, 今日充值
        /// </summary>
        [DisplayName("今日上分")]
        public float CreditToday
        {
            get { return _CreditToday; }
            set
            {
                if (_CreditToday == value)
                    return;
                _CreditToday = value;
                NotifyPropertyChanged(() => CreditToday);
            }
        }

        //今日下注current period
        private float _BetToday;
        [DisplayName("今日下注")]
        public float BetToday
        {
            get { return _BetToday; }
            set
            {
                if (_BetToday == value)
                    return;
                _BetToday = value;
                NotifyPropertyChanged(() => BetToday);
            }
        }

        private float _WithdrawToday;
        [DisplayName("今日下分")]
        public float WithdrawToday
        {
            get { return _WithdrawToday; }
            set
            {
                if (_WithdrawToday == value)
                    return;
                _WithdrawToday = value;
                NotifyPropertyChanged(() => WithdrawToday);
            }
        }

        /// <summary>
        ///     投注总额,总投注
        /// </summary>
        private float _BetTotal;
        [DisplayName("总下注")]
        public float BetTotal
        {
            get { return _BetTotal; }
            set
            {
                if (_BetTotal == value)
                    return;
                _BetTotal = value;
                NotifyPropertyChanged(() => BetTotal);
            }
        }

        /// <summary>
        ///     总充值
        /// </summary>
        private float _CreditTotal;
        [DisplayName("总上分")]
        public float CreditTotal
        {
            get { return _CreditTotal; }
            set
            {
                if (_CreditTotal == value)
                    return;
                _CreditTotal = value;
                NotifyPropertyChanged(() => CreditTotal);
            }
        }


        private float _WithdrawTotal;
        /// <summary>
        ///     总提现
        /// </summary>
        [DisplayName("总下分")]
        public float WithdrawTotal
        {
            get { return _WithdrawTotal; }
            set
            {
                if (_WithdrawTotal == value)
                    return;
                _WithdrawTotal = value;
                NotifyPropertyChanged(() => WithdrawTotal);
            }
        }




        private float _IncomeTotal;
        [DisplayName("总盈亏")]
        public float IncomeTotal
        {
            get { return _IncomeTotal; }
            set
            {
                if (_IncomeTotal == value)
                    return;
                _IncomeTotal = value;
                NotifyPropertyChanged(() => IncomeTotal);
            }
        }



        /// <summary>
        ///     当期投注清0
        /// </summary>
        /// <returns></returns>
        public void BetCurZero()
        {
            if(this.BetCur != 0)
                this.BetCur = 0;
        }

        /// <summary>
        ///     会员新增加一个订单, 要记录日志
        /// </summary>
        /// <returns></returns>
        public bool AddOrder(V2MemberOrder member_order)
        {
            float money = member_order.AmountTotal;
            this.Balance = this.Balance - money;
            this.BetCur = this.BetCur + money;      //本期投注.. 这个在期号变更时候, 清0
            this.BetToday = this.BetToday + money;  //今日投注
            this.BetTotal = this.BetTotal + money;  //总下注

            return true;
        }

        /// <summary>
        ///     开奖
        /// </summary>
        /// <param name="member_order"></param>
        /// <returns></returns>
        public bool OpenLottery(V2MemberOrder member_order)
        {
            if(member_order.OrderStatus == OrderStatusEnum.已完成)
            {
                //加上订单利润
                this.Balance += member_order.Profit;    //加上返利
                this.IncomeToday = this.IncomeToday + (member_order.Profit - member_order.AmountTotal);  //今日盈亏
                this.IncomeTotal = this.IncomeTotal + (member_order.Profit - member_order.AmountTotal); //总盈亏
                this.IncomeTodayStart = this.IncomeTodayStart + (member_order.Profit - member_order.AmountTotal);
            }
            return true;
        }

        //public void 上分(V2MemberCoinsBuySell order, Func<V2Member, int, bool> OnCreditSuccess)
        //{
        //    this.Balance += money;
        //    this.CreditToday += money;  //今日上分
        //    this.CreditTotal += money;  //总上分
        //    OnCreditSuccess?.Invoke(this, money);
        //}

        //public void 下分(V2MemberCoinsBuySell order, Func<V2Member, int, bool> OnWithdraw)
        //{

        //    if (money > (int)this.Balance)
        //    {
        //        throw new Exception($"#{this.nickname} 存储不足!");
        //    }
        //    this.Balance -= money;
        //    this.WithdrawToday += money;
        //    this.WithdrawTotal += money;
        //    OnWithdraw?.Invoke(this, money);
        //}

    }
}
