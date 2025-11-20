using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace zhaocaimao.Models
{
    /// <summary>
    /// 微信用户信息模型
    /// </summary>
    public class WxUserInfo : INotifyPropertyChanged
    {
        private string _wxid = string.Empty;
        private string _nickname = string.Empty;
        private string _account = string.Empty;
        private string _mobile = string.Empty;
        private string _avatar = string.Empty;
        private string _dataPath = string.Empty;
        private string _currentDataPath = string.Empty;
        private string _dbKey = string.Empty;

        /// <summary>
        /// 微信ID
        /// </summary>
        public string Wxid
        {
            get => _wxid;
            set => SetProperty(ref _wxid, value);
        }

        /// <summary>
        /// 微信昵称
        /// </summary>
        public string Nickname
        {
            get => _nickname;
            set => SetProperty(ref _nickname, value);
        }

        /// <summary>
        /// 微信号
        /// </summary>
        public string Account
        {
            get => _account;
            set => SetProperty(ref _account, value);
        }

        /// <summary>
        /// 手机号
        /// </summary>
        public string Mobile
        {
            get => _mobile;
            set => SetProperty(ref _mobile, value);
        }

        /// <summary>
        /// 头像地址
        /// </summary>
        public string Avatar
        {
            get => _avatar;
            set => SetProperty(ref _avatar, value);
        }

        /// <summary>
        /// 数据路径
        /// </summary>
        public string DataPath
        {
            get => _dataPath;
            set => SetProperty(ref _dataPath, value);
        }

        /// <summary>
        /// 当前数据路径
        /// </summary>
        public string CurrentDataPath
        {
            get => _currentDataPath;
            set => SetProperty(ref _currentDataPath, value);
        }

        /// <summary>
        /// 数据库密钥
        /// </summary>
        public string DbKey
        {
            get => _dbKey;
            set => SetProperty(ref _dbKey, value);
        }

        /// <summary>
        /// 是否已登录
        /// </summary>
        public bool IsLoggedIn => !string.IsNullOrEmpty(Wxid);

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
                return false;

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        #endregion
    }
}

