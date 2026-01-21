using System;
using System.Windows.Forms;
using Unit.La.Models;

namespace Unit.La.Controls
{
    /// <summary>
    /// 浏览器任务配置面板
    /// 通用的配置界面，可在任何项目中使用
    /// </summary>
    public partial class BrowserConfigPanel : UserControl
    {
        private BrowserTaskConfig? _config;

        /// <summary>
        /// 配置变更事件
        /// </summary>
        public event EventHandler<BrowserTaskConfig>? ConfigChanged;

        public BrowserConfigPanel()
        {
            InitializeComponent();
            InitializeControls();
        }

        /// <summary>
        /// 获取或设置配置
        /// </summary>
        public BrowserTaskConfig? Config
        {
            get => _config;
            set
            {
                _config = value;
                UpdateControls();
            }
        }

        /// <summary>
        /// 初始化控件
        /// </summary>
        private void InitializeControls()
        {
            // 订阅控件变更事件
            txtName.TextChanged += (s, e) => OnConfigPropertyChanged();
            txtUrl.TextChanged += (s, e) => OnConfigPropertyChanged();
            txtUsername.TextChanged += (s, e) => OnConfigPropertyChanged();
            txtPassword.TextChanged += (s, e) => OnConfigPropertyChanged();
            chkAutoLogin.CheckedChanged += (s, e) => OnConfigPropertyChanged();
        }

        /// <summary>
        /// 从控件更新配置
        /// </summary>
        private void UpdateConfigFromControls()
        {
            if (_config == null) return;

            _config.Name = txtName.Text;
            _config.Url = txtUrl.Text;
            _config.Username = txtUsername.Text;
            _config.Password = txtPassword.Text;
            _config.AutoLogin = chkAutoLogin.Checked;
        }

        /// <summary>
        /// 从配置更新控件
        /// </summary>
        private void UpdateControls()
        {
            if (_config == null) return;

            txtName.Text = _config.Name;
            txtUrl.Text = _config.Url;
            txtUsername.Text = _config.Username;
            txtPassword.Text = _config.Password;
            chkAutoLogin.Checked = _config.AutoLogin;
        }

        /// <summary>
        /// 配置属性变更
        /// </summary>
        private void OnConfigPropertyChanged()
        {
            UpdateConfigFromControls();
            ConfigChanged?.Invoke(this, _config!);
        }

        /// <summary>
        /// 验证配置
        /// </summary>
        public bool ValidateConfig(out string errorMessage)
        {
            errorMessage = "";

            if (_config == null)
            {
                errorMessage = "配置对象为空";
                return false;
            }

            if (string.IsNullOrWhiteSpace(_config.Url))
            {
                errorMessage = "URL 不能为空";
                return false;
            }

            if (!Uri.IsWellFormedUriString(_config.Url, UriKind.Absolute))
            {
                errorMessage = "URL 格式不正确";
                return false;
            }

            if (_config.AutoLogin)
            {
                if (string.IsNullOrWhiteSpace(_config.Username))
                {
                    errorMessage = "启用自动登录时，用户名不能为空";
                    return false;
                }

                if (string.IsNullOrWhiteSpace(_config.Password))
                {
                    errorMessage = "启用自动登录时，密码不能为空";
                    return false;
                }
            }

            return true;
        }
    }
}
