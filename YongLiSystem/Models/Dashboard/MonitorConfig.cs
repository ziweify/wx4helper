using System.ComponentModel;

namespace YongLiSystem.Models.Dashboard
{
    /// <summary>
    /// 监控配置模型
    /// </summary>
    public class MonitorConfig : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private string _url = string.Empty;
        private string _username = string.Empty;
        private string _password = string.Empty;
        private bool _autoLogin = false;
        private string _script = string.Empty;
        private string _latestIssueData = string.Empty;

        /// <summary>
        /// 监控名称
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        /// <summary>
        /// 网址
        /// </summary>
        public string Url
        {
            get => _url;
            set
            {
                if (_url != value)
                {
                    _url = value;
                    OnPropertyChanged(nameof(Url));
                }
            }
        }

        /// <summary>
        /// 用户名
        /// </summary>
        public string Username
        {
            get => _username;
            set
            {
                if (_username != value)
                {
                    _username = value;
                    OnPropertyChanged(nameof(Username));
                }
            }
        }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password
        {
            get => _password;
            set
            {
                if (_password != value)
                {
                    _password = value;
                    OnPropertyChanged(nameof(Password));
                }
            }
        }

        /// <summary>
        /// 是否自动登录
        /// </summary>
        public bool AutoLogin
        {
            get => _autoLogin;
            set
            {
                if (_autoLogin != value)
                {
                    _autoLogin = value;
                    OnPropertyChanged(nameof(AutoLogin));
                }
            }
        }

        /// <summary>
        /// 脚本内容
        /// </summary>
        public string Script
        {
            get => _script;
            set
            {
                if (_script != value)
                {
                    _script = value;
                    OnPropertyChanged(nameof(Script));
                }
            }
        }

        /// <summary>
        /// 最新期号数据
        /// </summary>
        public string LatestIssueData
        {
            get => _latestIssueData;
            set
            {
                if (_latestIssueData != value)
                {
                    _latestIssueData = value;
                    OnPropertyChanged(nameof(LatestIssueData));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

