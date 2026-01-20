using System;
using System.Windows.Forms;
using 永利系统.Services;

namespace 永利系统.Views.Pages
{
    /// <summary>
    /// 系统设置页面 - 使用 Form 实现
    /// </summary>
    public partial class SettingsPage : Form
    {
        private readonly LoggingService _loggingService;

        public SettingsPage()
        {
            InitializeComponent();
            
            // 设置为非顶级窗口，可以嵌入到 TabPage 中
            TopLevel = false;
            FormBorderStyle = FormBorderStyle.None;
            Dock = DockStyle.Fill;
            
            _loggingService = LoggingService.Instance;
            InitializeUI();
            
            // 订阅 FormClosing 事件以清理资源
            FormClosing += SettingsPage_FormClosing;
        }

        private void InitializeUI()
        {
            // 初始化系统设置页面的UI
            _loggingService.Info("系统设置", "系统设置页面已初始化");
        }

        private void SettingsPage_FormClosing(object? sender, FormClosingEventArgs e)
        {
            // 保存设置
            SaveSettings();
        }

        /// <summary>
        /// 保存设置
        /// </summary>
        private void SaveSettings()
        {
            // TODO: 实现设置保存逻辑
            _loggingService.Info("系统设置", "保存系统设置");
        }

        private void SettingsPage_Load(object sender, EventArgs e)
        {
            // 页面加载时加载设置
            LoadSettings();
        }

        /// <summary>
        /// 加载设置
        /// </summary>
        private void LoadSettings()
        {
            // TODO: 实现设置加载逻辑
            _loggingService.Info("系统设置", "加载系统设置");
        }
    }
}

