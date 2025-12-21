using System;
using System.Windows.Forms;
using 永利系统.Services;

namespace 永利系统.Views.Pages
{
    /// <summary>
    /// 微信助手页面
    /// </summary>
    public partial class WechatPage : UserControl
    {
        private readonly LoggingService _loggingService;

        public WechatPage()
        {
            InitializeComponent();
            _loggingService = LoggingService.Instance;
            InitializeUI();
        }

        private void InitializeUI()
        {
            // 这里可以添加微信助手的具体UI控件
            // 例如：联系人列表、消息发送区域、自动回复设置等
            
            _loggingService.Info("微信助手", "微信助手页面已初始化");
        }
    }
}

