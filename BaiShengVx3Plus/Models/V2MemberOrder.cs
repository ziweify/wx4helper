using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BaiShengVx3Plus.Models
{
    public class V2MemberOrder : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private int _id;
        public int Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(); }
        }

        private string _groupWxId = string.Empty;
        [DisplayName("群ID")]
        public string GroupWxId
        {
            get => _groupWxId;
            set { _groupWxId = value; OnPropertyChanged(); }
        }

        private string _wxid = string.Empty;
        [DisplayName("会员ID")]
        public string Wxid
        {
            get => _wxid;
            set { _wxid = value; OnPropertyChanged(); }
        }

        private string _account = string.Empty;
        [DisplayName("会员号码")]
        public string Account
        {
            get => _account;
            set { _account = value; OnPropertyChanged(); }
        }

        private string _nickname = string.Empty;
        [DisplayName("昵称")]
        public string Nickname
        {
            get => _nickname;
            set { _nickname = value; OnPropertyChanged(); }
        }

        private int _issueId;
        [DisplayName("期号")]
        public int IssueId
        {
            get => _issueId;
            set { _issueId = value; OnPropertyChanged(); }
        }

        private string _betContentOriginal = string.Empty;
        [DisplayName("原始内容")]
        public string BetContentOriginal
        {
            get => _betContentOriginal;
            set { _betContentOriginal = value; OnPropertyChanged(); }
        }

        private string _betContentStandar = string.Empty;
        [DisplayName("标准内容")]
        public string BetContentStandar
        {
            get => _betContentStandar;
            set { _betContentStandar = value; OnPropertyChanged(); }
        }

        private decimal _betFronMoney;
        [DisplayName("注前金额")]
        public decimal BetFronMoney
        {
            get => _betFronMoney;
            set { _betFronMoney = value; OnPropertyChanged(); }
        }

        private decimal _betAfterMoney;
        [DisplayName("注后金额")]
        public decimal BetAfterMoney
        {
            get => _betAfterMoney;
            set { _betAfterMoney = value; OnPropertyChanged(); }
        }

        private int _nums;
        [DisplayName("数量")]
        public int Nums
        {
            get => _nums;
            set { _nums = value; OnPropertyChanged(); }
        }

        private decimal _profit;
        [DisplayName("盈利")]
        public decimal Profit
        {
            get => _profit;
            set { _profit = value; OnPropertyChanged(); }
        }

        private decimal _netProfit;
        [DisplayName("纯利")]
        public decimal NetProfit
        {
            get => _netProfit;
            set { _netProfit = value; OnPropertyChanged(); }
        }

        private decimal _amountTotal;
        [DisplayName("总金额")]
        public decimal AmountTotal
        {
            get => _amountTotal;
            set { _amountTotal = value; OnPropertyChanged(); }
        }

        private decimal _odds = 1.97m;
        [DisplayName("赔率")]
        public decimal Odds
        {
            get => _odds;
            set { _odds = value; OnPropertyChanged(); }
        }

        private OrderStatus _orderStatus;
        [DisplayName("状态")]
        public OrderStatus OrderStatus
        {
            get => _orderStatus;
            set { _orderStatus = value; OnPropertyChanged(); }
        }

        private OrderType _orderType;
        [DisplayName("类型")]
        public OrderType OrderType
        {
            get => _orderType;
            set { _orderType = value; OnPropertyChanged(); }
        }

        private string _notes = string.Empty;
        [DisplayName("备注")]
        public string Notes
        {
            get => _notes;
            set { _notes = value; OnPropertyChanged(); }
        }

        private long _timeStampBet;
        [DisplayName("时间戳")]
        public long TimeStampBet
        {
            get => _timeStampBet;
            set { _timeStampBet = value; OnPropertyChanged(); }
        }

        private string _timeString = string.Empty;
        [DisplayName("日期时间")]
        public string TimeString
        {
            get => _timeString;
            set { _timeString = value; OnPropertyChanged(); }
        }

        public V2MemberOrder()
        {
            OrderStatus = OrderStatus.待处理;
            OrderType = OrderType.待定;
            TimeString = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}

