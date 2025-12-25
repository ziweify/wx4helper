using System;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using 永利系统.Services;

namespace 永利系统.Views.Wechat
{
    /// <summary>
    /// 上下分管理窗口 - 现代化设计
    /// </summary>
    public partial class CreditManageForm : XtraForm
    {
        private readonly LoggingService? _loggingService;

        public CreditManageForm()
        {
            InitializeComponent();
            
            _loggingService = LoggingService.Instance;
            
            // 设置窗口属性
            this.Text = "上下分管理";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;  // 无边框
            this.Size = new System.Drawing.Size(1200, 700);
            this.MinimumSize = new System.Drawing.Size(1000, 600);
            
            // 添加阴影效果
            this.BackColor = System.Drawing.Color.FromArgb(250, 250, 250);
            
            InitializeUI();
        }

        /// <summary>
        /// 初始化UI
        /// </summary>
        private void InitializeUI()
        {
            try
            {
                // 初始化状态筛选下拉框
                comboBoxEdit_StatusFilter.Properties.Items.Clear();
                comboBoxEdit_StatusFilter.Properties.Items.AddRange(new object[] 
                { 
                    "全部状态", 
                    "等待处理", 
                    "已同意", 
                    "已拒绝", 
                    "已忽略" 
                });
                comboBoxEdit_StatusFilter.SelectedIndex = 1;  // 默认显示"等待处理"
                
                // 配置 GridControl
                ConfigureGridView();
                
                _loggingService?.Info("上下分管理", "窗口初始化完成");
            }
            catch (Exception ex)
            {
                _loggingService?.Error("上下分管理", "初始化UI失败", ex);
            }
        }

        /// <summary>
        /// 配置 GridView
        /// </summary>
        private void ConfigureGridView()
        {
            // TODO: 配置 GridView 列
            // - 操作按钮列（同意、拒绝、忽略）
            // - ID
            // - 申请时间
            // - 昵称
            // - 动作（上分/下分）
            // - 金额
            // - 状态
            // - 处理人
            // - 处理时间
            // - 备注
        }

        #region 事件处理器

        /// <summary>
        /// 关闭按钮点击
        /// </summary>
        private void SimpleButton_Close_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 最小化按钮点击
        /// </summary>
        private void SimpleButton_Minimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        /// <summary>
        /// 刷新按钮点击
        /// </summary>
        private void SimpleButton_Refresh_Click(object sender, EventArgs e)
        {
            try
            {
                _loggingService?.Info("上下分管理", "刷新数据");
                // TODO: 实现刷新逻辑
            }
            catch (Exception ex)
            {
                _loggingService?.Error("上下分管理", "刷新失败", ex);
            }
        }

        /// <summary>
        /// 状态筛选变化
        /// </summary>
        private void ComboBoxEdit_StatusFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                _loggingService?.Info("上下分管理", $"筛选状态: {comboBoxEdit_StatusFilter.Text}");
                // TODO: 实现筛选逻辑
            }
            catch (Exception ex)
            {
                _loggingService?.Error("上下分管理", "筛选失败", ex);
            }
        }

        /// <summary>
        /// 标题栏鼠标按下（用于拖动窗口）
        /// </summary>
        private void PanelControl_TitleBar_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // 释放鼠标捕获
                NativeMethods.ReleaseCapture();
                // 发送移动窗口消息
                NativeMethods.SendMessage(this.Handle, NativeMethods.WM_NCLBUTTONDOWN, NativeMethods.HT_CAPTION, 0);
            }
        }

        #endregion

        #region Win32 API for Window Dragging

        private static class NativeMethods
        {
            public const int WM_NCLBUTTONDOWN = 0xA1;
            public const int HT_CAPTION = 0x2;

            [System.Runtime.InteropServices.DllImport("user32.dll")]
            public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

            [System.Runtime.InteropServices.DllImport("user32.dll")]
            public static extern bool ReleaseCapture();
        }

        #endregion
    }
}

