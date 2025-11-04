using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BaiShengVx3Plus.Models
{
    public class V2Member : INotifyPropertyChanged
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
        [DisplayName("WxID")]
        public string Wxid
        {
            get => _wxid;
            set { _wxid = value; OnPropertyChanged(); }
        }

        private string _account = string.Empty;
        [DisplayName("号")]
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

        private string _displayName = string.Empty;
        [DisplayName("群昵称")]
        public string DisplayName
        {
            get => _displayName;
            set { _displayName = value; OnPropertyChanged(); }
        }

        private decimal _balance;
        [DisplayName("余额")]
        public decimal Balance
        {
            get => _balance;
            set { _balance = value; OnPropertyChanged(); }
        }

        private MemberState _state;
        [DisplayName("状态")]
        public MemberState State
        {
            get => _state;
            set { _state = value; OnPropertyChanged(); }
        }

        private decimal _incomeToday;
        [DisplayName("今日盈亏")]
        public decimal IncomeToday
        {
            get => _incomeToday;
            set { _incomeToday = value; OnPropertyChanged(); }
        }

        private decimal _betCur;
        [DisplayName("本期下注")]
        public decimal BetCur
        {
            get => _betCur;
            set { _betCur = value; OnPropertyChanged(); }
        }

        private decimal _betWait;
        [DisplayName("待结算")]
        public decimal BetWait
        {
            get => _betWait;
            set { _betWait = value; OnPropertyChanged(); }
        }

        private decimal _creditToday;
        [DisplayName("今日上分")]
        public decimal CreditToday
        {
            get => _creditToday;
            set { _creditToday = value; OnPropertyChanged(); }
        }

        private decimal _betToday;
        [DisplayName("今日下注")]
        public decimal BetToday
        {
            get => _betToday;
            set { _betToday = value; OnPropertyChanged(); }
        }

        private decimal _withdrawToday;
        [DisplayName("今日下分")]
        public decimal WithdrawToday
        {
            get => _withdrawToday;
            set { _withdrawToday = value; OnPropertyChanged(); }
        }

        private decimal _betTotal;
        [DisplayName("总下注")]
        public decimal BetTotal
        {
            get => _betTotal;
            set { _betTotal = value; OnPropertyChanged(); }
        }

        private decimal _creditTotal;
        [DisplayName("总上分")]
        public decimal CreditTotal
        {
            get => _creditTotal;
            set { _creditTotal = value; OnPropertyChanged(); }
        }

        private decimal _withdrawTotal;
        [DisplayName("总下分")]
        public decimal WithdrawTotal
        {
            get => _withdrawTotal;
            set { _withdrawTotal = value; OnPropertyChanged(); }
        }

        private decimal _incomeTotal;
        [DisplayName("总盈亏")]
        public decimal IncomeTotal
        {
            get => _incomeTotal;
            set { _incomeTotal = value; OnPropertyChanged(); }
        }

        public V2Member()
        {
            State = MemberState.非会员;
        }
    }
}

