using System;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace 永利系统.Views.Dashboard.Controls
{
    /// <summary>
    /// 监控配置用户控件
    /// </summary>
    public partial class MonitorConfigControl : XtraUserControl
    {
        public MonitorConfigControl()
        {
            InitializeComponent();
            SubscribeToControlEvents();
        }

        /// <summary>
        /// 订阅控件事件以触发属性变更
        /// </summary>
        private void SubscribeToControlEvents()
        {
            if (txtUrl != null)
                txtUrl.EditValueChanged += (s, e) => UrlChanged?.Invoke(this, EventArgs.Empty);
            
            if (txtUsername != null)
                txtUsername.EditValueChanged += (s, e) => UsernameChanged?.Invoke(this, EventArgs.Empty);
            
            if (txtPassword != null)
                txtPassword.EditValueChanged += (s, e) => PasswordChanged?.Invoke(this, EventArgs.Empty);
            
            if (chkAutoLogin != null)
                chkAutoLogin.CheckedChanged += (s, e) => AutoLoginChanged?.Invoke(this, EventArgs.Empty);
            
            if (memoScript != null)
                memoScript.EditValueChanged += (s, e) => ScriptChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 获取或设置网址
        /// </summary>
        public string Url
        {
            get => txtUrl?.EditValue?.ToString() ?? string.Empty;
            set { if (txtUrl != null) txtUrl.EditValue = value; }
        }

        /// <summary>
        /// 获取或设置用户名
        /// </summary>
        public string Username
        {
            get => txtUsername?.EditValue?.ToString() ?? string.Empty;
            set { if (txtUsername != null) txtUsername.EditValue = value; }
        }

        /// <summary>
        /// 获取或设置密码
        /// </summary>
        public string Password
        {
            get => txtPassword?.EditValue?.ToString() ?? string.Empty;
            set { if (txtPassword != null) txtPassword.EditValue = value; }
        }

        /// <summary>
        /// 获取或设置是否自动登录
        /// </summary>
        public bool AutoLogin
        {
            get => chkAutoLogin?.Checked ?? false;
            set { if (chkAutoLogin != null) chkAutoLogin.Checked = value; }
        }

        /// <summary>
        /// 获取或设置脚本内容
        /// </summary>
        public string Script
        {
            get => memoScript?.EditValue?.ToString() ?? string.Empty;
            set { if (memoScript != null) memoScript.EditValue = value; }
        }

        /// <summary>
        /// 获取或设置最新期号数据
        /// </summary>
        public string LatestIssueData
        {
            get => txtLatestIssueData?.EditValue?.ToString() ?? string.Empty;
            set { if (txtLatestIssueData != null) txtLatestIssueData.EditValue = value; }
        }

        // 属性变更事件
        public event EventHandler? UrlChanged;
        public event EventHandler? UsernameChanged;
        public event EventHandler? PasswordChanged;
        public event EventHandler? AutoLoginChanged;
        public event EventHandler? ScriptChanged;

        /// <summary>
        /// 登录按钮点击事件
        /// </summary>
        public event EventHandler? LoginClicked;

        /// <summary>
        /// 采集按钮点击事件
        /// </summary>
        public event EventHandler? CollectClicked;

        /// <summary>
        /// 获取Cookie按钮点击事件
        /// </summary>
        public event EventHandler? GetCookieClicked;

        private void BtnLogin_Click(object? sender, EventArgs e)
        {
            LoginClicked?.Invoke(this, EventArgs.Empty);
        }

        private void BtnCollect_Click(object? sender, EventArgs e)
        {
            CollectClicked?.Invoke(this, EventArgs.Empty);
        }

        private void BtnGetCookie_Click(object? sender, EventArgs e)
        {
            GetCookieClicked?.Invoke(this, EventArgs.Empty);
        }
    }
}

