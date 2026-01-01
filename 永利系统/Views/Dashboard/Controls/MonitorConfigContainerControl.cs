using System;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraTab;
using 永利系统.Models.Dashboard;

namespace 永利系统.Views.Dashboard.Controls
{
    /// <summary>
    /// 监控配置容器控件 - 包含ABC三个监控的配置
    /// </summary>
    public partial class MonitorConfigContainerControl : XtraUserControl
    {
        private MonitorConfigControl? _monitorAConfigControl;
        private MonitorConfigControl? _monitorBConfigControl;
        private MonitorConfigControl? _monitorCConfigControl;

        // 配置模型
        public MonitorConfig MonitorAConfig { get; private set; }
        public MonitorConfig MonitorBConfig { get; private set; }
        public MonitorConfig MonitorCConfig { get; private set; }

        public MonitorConfigContainerControl()
        {
            InitializeComponent();

            // 创建配置模型
            MonitorAConfig = new MonitorConfig { Name = "MonitorA" };
            MonitorBConfig = new MonitorConfig { Name = "MonitorB" };
            MonitorCConfig = new MonitorConfig { Name = "MonitorC" };

            InitializeMonitorConfigs();
        }

        /// <summary>
        /// 初始化三个监控配置控件
        /// </summary>
        private void InitializeMonitorConfigs()
        {
            // 创建监控A配置控件
            _monitorAConfigControl = new MonitorConfigControl
            {
                Dock = DockStyle.Fill
            };
            xtraTabPageA.Controls.Add(_monitorAConfigControl);
            BindConfig(_monitorAConfigControl, MonitorAConfig);

            // 创建监控B配置控件
            _monitorBConfigControl = new MonitorConfigControl
            {
                Dock = DockStyle.Fill
            };
            xtraTabPageB.Controls.Add(_monitorBConfigControl);
            BindConfig(_monitorBConfigControl, MonitorBConfig);

            // 创建监控C配置控件
            _monitorCConfigControl = new MonitorConfigControl
            {
                Dock = DockStyle.Fill
            };
            xtraTabPageC.Controls.Add(_monitorCConfigControl);
            BindConfig(_monitorCConfigControl, MonitorCConfig);

            // 订阅事件
            SubscribeEvents();
        }

        /// <summary>
        /// 绑定配置到控件
        /// </summary>
        private void BindConfig(MonitorConfigControl control, MonitorConfig config)
        {
            // 双向绑定
            control.UrlChanged += (s, e) => config.Url = control.Url;
            control.UsernameChanged += (s, e) => config.Username = control.Username;
            control.PasswordChanged += (s, e) => config.Password = control.Password;
            control.AutoLoginChanged += (s, e) => config.AutoLogin = control.AutoLogin;
            control.ScriptChanged += (s, e) => config.Script = control.Script;

            // 从配置更新到控件
            config.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(MonitorConfig.Url):
                        control.Url = config.Url;
                        break;
                    case nameof(MonitorConfig.Username):
                        control.Username = config.Username;
                        break;
                    case nameof(MonitorConfig.Password):
                        control.Password = config.Password;
                        break;
                    case nameof(MonitorConfig.AutoLogin):
                        control.AutoLogin = config.AutoLogin;
                        break;
                    case nameof(MonitorConfig.Script):
                        control.Script = config.Script;
                        break;
                    case nameof(MonitorConfig.LatestIssueData):
                        control.LatestIssueData = config.LatestIssueData;
                        break;
                }
            };
        }

        /// <summary>
        /// 订阅监控配置事件
        /// </summary>
        private void SubscribeEvents()
        {
            if (_monitorAConfigControl != null)
            {
                _monitorAConfigControl.LoginClicked += (s, e) => OnMonitorCommand("MonitorA", "Login");
                _monitorAConfigControl.CollectClicked += (s, e) => OnMonitorCommand("MonitorA", "Collect");
                _monitorAConfigControl.GetCookieClicked += (s, e) => OnMonitorCommand("MonitorA", "GetCookie");
            }

            if (_monitorBConfigControl != null)
            {
                _monitorBConfigControl.LoginClicked += (s, e) => OnMonitorCommand("MonitorB", "Login");
                _monitorBConfigControl.CollectClicked += (s, e) => OnMonitorCommand("MonitorB", "Collect");
                _monitorBConfigControl.GetCookieClicked += (s, e) => OnMonitorCommand("MonitorB", "GetCookie");
            }

            if (_monitorCConfigControl != null)
            {
                _monitorCConfigControl.LoginClicked += (s, e) => OnMonitorCommand("MonitorC", "Login");
                _monitorCConfigControl.CollectClicked += (s, e) => OnMonitorCommand("MonitorC", "Collect");
                _monitorCConfigControl.GetCookieClicked += (s, e) => OnMonitorCommand("MonitorC", "GetCookie");
            }
        }

        /// <summary>
        /// 监控命令事件
        /// </summary>
        public event EventHandler<MonitorCommandEventArgs>? MonitorCommand;

        private void OnMonitorCommand(string monitorName, string commandName)
        {
            MonitorCommand?.Invoke(this, new MonitorCommandEventArgs(monitorName, commandName));
        }
    }

    /// <summary>
    /// 监控命令事件参数
    /// </summary>
    public class MonitorCommandEventArgs : EventArgs
    {
        public string MonitorName { get; }
        public string CommandName { get; }

        public MonitorCommandEventArgs(string monitorName, string commandName)
        {
            MonitorName = monitorName;
            CommandName = commandName;
        }
    }
}
