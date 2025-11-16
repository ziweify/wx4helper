using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace zhaocaimao.Models
{
    /// <summary>
    /// 微信联系人模型
    /// </summary>
    public class WxContact : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string _wxid = string.Empty;
        [DisplayName("微信ID")]
        public string Wxid
        {
            get => _wxid;
            set { _wxid = value; OnPropertyChanged(); }
        }

        private string _account = string.Empty;
        [DisplayName("微信号")]
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

        private string _remark = string.Empty;
        [DisplayName("备注")]
        public string Remark
        {
            get => _remark;
            set { _remark = value; OnPropertyChanged(); }
        }

        private string _avatar = string.Empty;
        [DisplayName("头像")]
        public string Avatar
        {
            get => _avatar;
            set { _avatar = value; OnPropertyChanged(); }
        }

        private int _sex;
        [DisplayName("性别")]
        public int Sex
        {
            get => _sex;
            set { _sex = value; OnPropertyChanged(); }
        }

        private string _province = string.Empty;
        [DisplayName("省份")]
        public string Province
        {
            get => _province;
            set { _province = value; OnPropertyChanged(); }
        }

        private string _city = string.Empty;
        [DisplayName("城市")]
        public string City
        {
            get => _city;
            set { _city = value; OnPropertyChanged(); }
        }

        private string _country = string.Empty;
        [DisplayName("国家")]
        public string Country
        {
            get => _country;
            set { _country = value; OnPropertyChanged(); }
        }

        private bool _isGroup;
        [DisplayName("是否群组")]
        public bool IsGroup
        {
            get => _isGroup;
            set { _isGroup = value; OnPropertyChanged(); }
        }

        public WxContact()
        {
        }
    }
}

