using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace 永利系统.Models.Config
{
    /// <summary>
    /// 应用程序配置类（根配置）
    /// </summary>
    public class AppConfig : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// 登录设置
        /// </summary>
        public LoginSettings Login { get; set; } = new LoginSettings();

        /// <summary>
        /// 窗口设置
        /// </summary>
        public WindowSettings Window { get; set; } = new WindowSettings();

        /// <summary>
        /// 微信模块设置
        /// </summary>
        public WechatSettings Wechat { get; set; } = new WechatSettings();

        /// <summary>
        /// 应用设置
        /// </summary>
        public AppSettings App { get; set; } = new AppSettings();

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// 登录设置
    /// </summary>
    public class LoginSettings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private string _username = string.Empty;
        private string _encryptedPassword = string.Empty;
        private bool _rememberPassword = false;

        /// <summary>
        /// 用户名（明文保存）
        /// </summary>
        public string Username
        {
            get => _username;
            set
            {
                if (_username != value)
                {
                    _username = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 加密后的密码（DPAPI加密）
        /// </summary>
        public string EncryptedPassword
        {
            get => _encryptedPassword;
            set
            {
                if (_encryptedPassword != value)
                {
                    _encryptedPassword = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 是否记住密码
        /// </summary>
        public bool RememberPassword
        {
            get => _rememberPassword;
            set
            {
                if (_rememberPassword != value)
                {
                    _rememberPassword = value;
                    OnPropertyChanged();
                }
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// 窗口设置
    /// </summary>
    public class WindowSettings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private int _width = 1440;
        private int _height = 900;
        private int _x = -1;  // -1 表示居中
        private int _y = -1;
        private bool _maximized = false;

        /// <summary>
        /// 窗口宽度
        /// </summary>
        public int Width
        {
            get => _width;
            set
            {
                if (_width != value)
                {
                    _width = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 窗口高度
        /// </summary>
        public int Height
        {
            get => _height;
            set
            {
                if (_height != value)
                {
                    _height = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 窗口 X 坐标
        /// </summary>
        public int X
        {
            get => _x;
            set
            {
                if (_x != value)
                {
                    _x = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 窗口 Y 坐标
        /// </summary>
        public int Y
        {
            get => _y;
            set
            {
                if (_y != value)
                {
                    _y = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 是否最大化
        /// </summary>
        public bool Maximized
        {
            get => _maximized;
            set
            {
                if (_maximized != value)
                {
                    _maximized = value;
                    OnPropertyChanged();
                }
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// 微信模块设置
    /// </summary>
    public class WechatSettings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private decimal _odds = 1.95m;
        private decimal _defaultAmount = 100m;

        /// <summary>
        /// 赔率
        /// </summary>
        public decimal Odds
        {
            get => _odds;
            set
            {
                if (_odds != value)
                {
                    _odds = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 默认金额
        /// </summary>
        public decimal DefaultAmount
        {
            get => _defaultAmount;
            set
            {
                if (_defaultAmount != value)
                {
                    _defaultAmount = value;
                    OnPropertyChanged();
                }
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// 应用设置
    /// </summary>
    public class AppSettings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private string _theme = "Default";
        private bool _autoCheckUpdate = true;
        private int _logRetentionDays = 7;

        /// <summary>
        /// 主题
        /// </summary>
        public string Theme
        {
            get => _theme;
            set
            {
                if (_theme != value)
                {
                    _theme = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 自动检查更新
        /// </summary>
        public bool AutoCheckUpdate
        {
            get => _autoCheckUpdate;
            set
            {
                if (_autoCheckUpdate != value)
                {
                    _autoCheckUpdate = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 日志保留天数
        /// </summary>
        public int LogRetentionDays
        {
            get => _logRetentionDays;
            set
            {
                if (_logRetentionDays != value)
                {
                    _logRetentionDays = value;
                    OnPropertyChanged();
                }
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

