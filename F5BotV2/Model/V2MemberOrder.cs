using F5BotV2.Boter;
using LxLib.LxSys;
using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace F5BotV2.Model
{
    //订单状态
    public enum OrderStatusEnum { 未知 = -1
            , 待处理 = 0     //订单进来了, 但是没有投递进网盘
            , 待结算 = 1
            , 已完成 = 2     //结算完成。至于怎么结算的，盘内, 盘外, 托, 看订单类型
            , 已取消 = 3,    //取消的订单, 类型那里不需要设定
    }

    //订单类型
    public enum OrderTypeEnum
    {
        待定 = 0,
        盘内 = 1,     //进盘的，就是盘内
        盘外 = 2,     //没进盘的, 就是盘外
        托 = 3,       //就是不打进盘的
    }

    public class V2MemberOrderComparerDefault : IComparer<V2MemberOrder>
    {
        public int Compare(V2MemberOrder x, V2MemberOrder y)
        {
            if (object.ReferenceEquals(x, y))
            {
                return 0;
            }
            //从1号车在前面, 大的在后面
            if (x.IssueId > y.IssueId)
                return 1;
            if (x.IssueId < y.IssueId)
                return -1;

            ////如果等于
            int xHaxi = x.nickname.GetHashCode();
            int yHaxi = y.nickname.GetHashCode();
            if (xHaxi > yHaxi)
                return 1;
            if (xHaxi < yHaxi)
                return -1;

            if (x.TimeStampBet > y.TimeStampBet)
                return 1;
            if (x.TimeStampBet < y.TimeStampBet)
                return -1;


            return 0;
        }
    }

    public class V2MemberOrder
        : IWxContacts
    {
        IWxContacts _iWxContact; //联系人

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

        /// <summary>
        ///     
        /// </summary>
        /// <param name="wxContact">联系人数据</param>
        /// <param name="issueId">下单期号</param>
        /// <param name="TimeStampBet">时间戳</param>
        /// <param name="BetContentOriginal">这个字段仅仅作为记录, 不参与任何计算</param>
        /// <param name="sb">投注上下文</param>
        /// <param name="_msg_origin">注码数量</param>
        /// <param name="betAmountTotal">投注总金额</param>
        public V2MemberOrder(IWxContacts wxContact, int issueId, int TimeStampBet, string BetContentOriginal, string BetContentStandar, int numbers, int amount_total)
        {
            this._iWxContact = wxContact;
            this.IssueId = issueId;
            this.TimeStampBet = TimeStampBet;
            this.TimeString = LxTimestampHelper.GetDateTime(TimeStampBet).ToString("yyyy-MM-dd HH:mm:ss");
            this.BetContentOriginal = BetContentOriginal;
            this.BetContentStandar = BetContentStandar;
            this.Nums = numbers;

            //单式转换.进来前要校验金额
            try
            {
                //OrderConvert = 
                //StringBuilder sb = new StringBuilder(512);
                //int betmoney = 0;
                //try
                //{
                //    betmoney = Convert.ToInt32(str3);
                //}
                //catch { betmoney = 0; };
                //int index = 0;
                //foreach (var pos in str1)
                //{
                //    string strPos = Convert.ToString(pos);
                //    if (sb.Length > 0)
                //    {
                //        sb.Append(";");
                //    }
                //    sb.Append(string.Format("{0},{1},{2}", strPos, str2, betmoney));
                //    index++;
                //}
                this.BetContentStandar = BetContentStandar;
                this.AmountTotal = amount_total;
            }
            catch
            {
                AmountTotal = 0;
            }
        }



        public V2MemberOrder()
        {
            _iWxContact = new WxContacts();
        }


        /// <summary>
        ///     开奖,计算盈利
 
        /// </summary>
        /// <returns></returns>
        public float OpenLottery(BgLotteryData data, float odds = 1.97f, bool isZsjs = false)
        {
            float sum = 0f;
            this.Odds = odds;
            BoterBetContents btc = new BoterBetContents(data.IssueId, this.BetContentStandar);
            foreach (var item in btc.boterItems)
            {
                sum = sum + item.OpenLottery(data, odds, isZsjs);   //开奖错误, 会抛出异常。
            }
            this.OrderStatus = OrderStatusEnum.已完成;
            this.Profit = sum;
            this.NetProfit = this.Profit - this.AmountTotal;
            return sum;
        }

        

        [DisplayName("会员ID")]
        public string wxid
        {
            get => _iWxContact == null ? "未绑定" : _iWxContact.wxid;
            set
            {
                if (_iWxContact == null)
                    return;
                if (_iWxContact.wxid == value)
                    return;
                _iWxContact.wxid = value;
                NotifyPropertyChanged(() => wxid);
            }
        }
        /// <summary>
        ///     下单会员的群号码
        /// </summary>
        [DisplayName("会员号码")]
        public string account
        {
            get => _iWxContact == null ? "未绑定" : _iWxContact.account;
            set
            {
                if (_iWxContact == null)
                    return;
                if (_iWxContact.account == value)
                    return;
                _iWxContact.account = value;
                NotifyPropertyChanged(() => account);
            }
        }

        /// <summary>
        ///     下单会员名
        /// </summary>
        [DisplayName("昵称")]
        public string nickname
        {
            get => _iWxContact == null ? "未绑定" : _iWxContact.nickname;
            set
            {
                if (_iWxContact == null)
                    return;
                if (_iWxContact.nickname == value)
                    return;
                _iWxContact.nickname = value;
                NotifyPropertyChanged(() => nickname);
            }
        }
        public string avatar
        {
            get => _iWxContact == null ? "" : _iWxContact.avatar;
            set
            {
                if (_iWxContact == null)
                    return;
                _iWxContact.avatar = value;
            }
        }

        public string city
        {
            get => _iWxContact == null ? "" : _iWxContact.city;
            set
            {
                if (_iWxContact == null)
                    return;
                _iWxContact.city = value;
            }
        }

        public string country
        {
            get => _iWxContact == null ? "" : _iWxContact.country;
            set
            {
                if (_iWxContact == null)
                    return;
                _iWxContact.country = value;
            }
        }
        public string province
        {
            get => _iWxContact == null ? "" : _iWxContact.province;
            set
            {
                if (_iWxContact == null)
                    return;
                _iWxContact.province = value;
            }
        }

        public string remark
        {
            get => _iWxContact == null ? "" : _iWxContact.remark;
            set
            {
                if (_iWxContact == null)
                    return;
                _iWxContact.remark = value;
            }
        }
        public int sex
        {
            get => _iWxContact == null ? 0 : _iWxContact.sex;
            set
            {
                if (_iWxContact == null)
                    return;
                _iWxContact.sex = value;
            }
        }

        private int _IssueId = 0;
        [DisplayName("期号")]
        public int IssueId
        {
            get { return _IssueId; }
            set
            {
                if (_IssueId == value)
                    return;
                _IssueId = value;
                NotifyPropertyChanged(() => IssueId);
            }
        }

        /// <summary>
        ///     下注内容:玩家的下注语句, 这个字段仅仅用于记录, 不参与任何计算
        /// </summary>
        private string _BetContentOriginal;
        [DisplayName("原始内容")]
        public string BetContentOriginal
        {
            get { return _BetContentOriginal; }
            set
            {
                if (_BetContentOriginal == value)
                    return;
                _BetContentOriginal = value;
                NotifyPropertyChanged(() => BetContentOriginal);
            }
        }

        /// <summary>
        ///     下注内容,转换后的,用于交换的标准化的内容
        /// </summary>
        private string _BetContentStandar;
        [DisplayName("标准内容")]
        public string BetContentStandar
        {
            get { return _BetContentStandar; }
            set
            {
                if (_BetContentStandar == value)
                    return;
                _BetContentStandar = value;
                NotifyPropertyChanged(() => BetContentStandar);
            }
        }

        private float _BetFronMoney;
        [DisplayName("注前金额")]
        public float BetFronMoney
        {
            get { return _BetFronMoney; }
            set
            {
                if (_BetFronMoney == value)
                    return;
                _BetFronMoney = value;
                NotifyPropertyChanged(() => BetFronMoney);
            }
        }

        private float _BetAfterMoney;
        [DisplayName("注后金额")]
        public float BetAfterMoney
        {
            get { return _BetAfterMoney; }
            set
            {
                if (_BetAfterMoney == value)
                    return;
                _BetAfterMoney = value;
                NotifyPropertyChanged(() => BetAfterMoney);
            }
        }


        /// <summary>
        ///    注码数量,投注的条目数量
        /// </summary>
        private int  _Nums;
        [DisplayName("数量")]
        public int Nums
        {
            get { return _Nums; }
            set
            {
                if (_Nums == value)
                    return;
                _Nums = value;
                NotifyPropertyChanged(() => Nums);
            }
        }

        /// <summary>
        ///     盈利.其实应该是返奖金额
        /// </summary>
        private float _Profit = 0;
        [DisplayName("盈利")]
        public float Profit
        {
            get { return _Profit; }
            set
            {
                if (_Profit == value)
                    return;
                _Profit = value;
                NotifyPropertyChanged(() => Profit);
            }
        }


        private float _NetProfit;  //纯利,实际浮盈
        [DisplayName("纯利")]
        public float NetProfit
        {
            get { return _NetProfit; }
            set
            {
                if (_NetProfit == value)
                    return;
                _NetProfit = value;
                NotifyPropertyChanged(() => NetProfit);
            }
        }



        /// <summary>
        ///     总金额
        /// </summary>
        private float _AmountTotal;
        [DisplayName("总金额")]
        public float AmountTotal
        {
            get { return _AmountTotal; }
            set
            {
                if (_AmountTotal == value)
                    return;
                _AmountTotal = value;
                NotifyPropertyChanged(() => AmountTotal);
            }
        }

        private float _Odds;
        [DisplayName("赔率")] //开奖的时候确认赔率.写入赔率, 记录该订单是用什么赔率进行结算的
        public float Odds
        {
            get { return _Odds; }
            set
            {
                if (_Odds == value)
                    return;
                _Odds = value;
                NotifyPropertyChanged(() => Odds);
            }
        }

        private OrderStatusEnum _OrderStatus;
        [DisplayName("状态")]
        public OrderStatusEnum OrderStatus
        {
            get { return _OrderStatus; }
            set
            {
                if (_OrderStatus == value)
                    return;
                _OrderStatus = value;
                NotifyPropertyChanged(() => OrderStatus);
            }
        }


        private OrderTypeEnum _OrderType;
        [DisplayName("类型")]
        public OrderTypeEnum OrderType
        {
            get { return _OrderType; }
            set
            {
                if (_OrderType == value)
                    return;
                _OrderType = value;
                NotifyPropertyChanged(() => OrderType);
            }
        }


        private string _Notes;
        [DisplayName("备注")]
        public string Notes
        {
            get { return _Notes; }
            set
            {
                if (_Notes == value)
                    return;
                _Notes = value;
                NotifyPropertyChanged(() => Notes);
            }
        }

        private int _TimeStampBet;
        [DisplayName("时间戳")]
        public int TimeStampBet
        {
            get { return _TimeStampBet; }
            set
            {
                if (_TimeStampBet == value)
                    return;
                _TimeStampBet = value;
                NotifyPropertyChanged(() => TimeStampBet);
            }
        }

        private string _TimeString;
        [DisplayName("日期时间")]
        public string TimeString
        {
            get { return _TimeString; }
            set
            {
                if (_TimeString == value)
                    return;
                _TimeString = value;
                NotifyPropertyChanged(() => TimeString);
            }
        }


    }
}
