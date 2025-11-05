using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BaiShengVx3Plus.Models;

namespace BaiShengVx3Plus.Views
{
    /// <summary>
    /// 用户信息显示控件（现代化设计）
    /// </summary>
    public partial class UcUserInfo : UserControl
    {
        private WxUserInfo? _userInfo;

        /// <summary>
        /// 用户信息数据源（支持数据绑定）
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public WxUserInfo? UserInfo
        {
            get => _userInfo;
            set
            {
                // 取消旧的数据绑定
                if (_userInfo != null)
                {
                    _userInfo.PropertyChanged -= UserInfo_PropertyChanged;
                }

                _userInfo = value;

                // 订阅新的数据绑定
                if (_userInfo != null)
                {
                    _userInfo.PropertyChanged += UserInfo_PropertyChanged;
                }

                // 更新显示
                UpdateDisplay();
            }
        }

        /// <summary>
        /// 采集按钮点击事件（由外部处理）
        /// </summary>
        public event EventHandler? CollectButtonClick;

        public UcUserInfo()
        {
            InitializeComponent();
            
            // 强制设置背景色和样式（确保在 VxMain 中显示正确）
            this.BackColor = Color.White;
            
            InitializeComponent_Custom();
        }

        /// <summary>
        /// 自定义初始化（美化UI）
        /// </summary>
        private void InitializeComponent_Custom()
        {
            // 设置采集按钮点击事件
            btnGetContactList.Click += BtnGetContactList_Click;
            
            // 初始状态
            UpdateDisplay();
        }

        /// <summary>
        /// 采集按钮点击处理
        /// </summary>
        private void BtnGetContactList_Click(object? sender, EventArgs e)
        {
            // 触发外部事件
            CollectButtonClick?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 数据变化时更新显示（线程安全）
        /// </summary>
        private void UserInfo_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // 线程安全的更新UI
            if (InvokeRequired)
            {
                Invoke(new Action(UpdateDisplay));
            }
            else
            {
                UpdateDisplay();
            }
        }

        /// <summary>
        /// 更新显示内容
        /// </summary>
        private void UpdateDisplay()
        {
            if (_userInfo == null || !_userInfo.IsLoggedIn)
            {
                // 未登录状态
                tbx_wxnick.Text = "未登录";
                lbl_wxid.Text = "请先登录微信";
                pic_headimage.Image = null;
                pic_headimage.BackColor = Color.LightGray;
                btnGetContactList.Enabled = false;
            }
            else
            {
                // 已登录状态
                tbx_wxnick.Text = _userInfo.Nickname;
                lbl_wxid.Text = $"ID: {_userInfo.Wxid}";
                btnGetContactList.Enabled = true;
                
                // 加载头像（如果有）
                LoadAvatar(_userInfo.Avatar);
            }
        }

        /// <summary>
        /// 加载用户头像
        /// </summary>
        private void LoadAvatar(string avatarPath)
        {
            try
            {
                if (!string.IsNullOrEmpty(avatarPath) && System.IO.File.Exists(avatarPath))
                {
                    pic_headimage.Image = Image.FromFile(avatarPath);
                    pic_headimage.SizeMode = PictureBoxSizeMode.Zoom;
                }
                else
                {
                    // 默认头像
                    pic_headimage.Image = null;
                    pic_headimage.BackColor = Color.FromArgb(80, 160, 255);
                }
            }
            catch
            {
                pic_headimage.Image = null;
                pic_headimage.BackColor = Color.FromArgb(80, 160, 255);
            }
        }

        /// <summary>
        /// 设置采集按钮的启用状态
        /// </summary>
        public void SetCollectButtonEnabled(bool enabled)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => btnGetContactList.Enabled = enabled));
            }
            else
            {
                btnGetContactList.Enabled = enabled;
            }
        }
    }
}
