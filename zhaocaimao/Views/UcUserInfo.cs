using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using zhaocaimao.Models;

namespace zhaocaimao.Views
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

        public UcUserInfo()
        {
            InitializeComponent();
            
            // 强制设置背景色和样式（确保在 VxMain 中显示正确）
            this.BackColor = Color.White;
            
            // 初始状态
            UpdateDisplay();
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
                tbx_wxnick.Text = "未连接";
                lbl_wxid.Text = "点击连接按钮启动微信";
                pic_headimage.Image = null;
                pic_headimage.BackColor = Color.LightGray;
            }
            else
            {
                // 已登录状态
                tbx_wxnick.Text = _userInfo.Nickname;
                lbl_wxid.Text = $"ID: {_userInfo.Wxid}";
                
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
    }
}
