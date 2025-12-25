using DevExpress.XtraEditors;

namespace 永利系统.Views.Wechat
{
    /// <summary>
    /// 微信助手设置窗口
    /// 非模态窗口，支持最前端显示
    /// </summary>
    public partial class WechatSettingsForm : XtraForm
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public WechatSettingsForm()
        {
            InitializeComponent();

            // 设置窗口属性
            this.Text = "微信助手设置";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            this.TopMost = true; // 🔥 一直在最顶端显示

            // TODO: 加载设置
            // LoadSettings();
        }

        /// <summary>
        /// 加载设置到 UI
        /// </summary>
        private void LoadSettings()
        {
            // TODO: 从 ConfigManager 加载配置并绑定到控件
        }

        /// <summary>
        /// 保存设置
        /// </summary>
        private void SaveSettings()
        {
            // TODO: 从控件读取值并保存到 ConfigManager
        }

        /// <summary>
        /// 保存按钮点击事件
        /// </summary>
        private void SimpleButton_Save_Click(object sender, System.EventArgs e)
        {
            SaveSettings();
            this.Close();
        }

        /// <summary>
        /// 取消按钮点击事件
        /// </summary>
        private void SimpleButton_Cancel_Click(object sender, System.EventArgs e)
        {
            this.Close();
        }

        private void labelControl_TestMessage_Click(object sender, System.EventArgs e)
        {

        }
    }
}

