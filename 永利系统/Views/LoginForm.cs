using System;
using System.Drawing;
using System.Windows.Forms;
using 永利系统.Services;
using 永利系统.Services.Auth;
using 永利系统.ViewModels;

namespace 永利系统.Views
{
    /// <summary>
    /// 登录窗口
    /// </summary>
    public partial class LoginForm : Form
    {
        private readonly LoginViewModel _viewModel;
        
        // 窗口拖动相关
        private bool _isDragging = false;
        private Point _dragStartPoint;
        
        public LoginViewModel ViewModel => _viewModel;
        
        public LoginForm()
        {
            InitializeComponent();
            
            // 创建 ViewModel
            var loggingService = LoggingService.Instance;
            var authService = new AuthService(loggingService);
            _viewModel = new LoginViewModel(authService, loggingService);
            
            BindViewModel();
        }
        
        private void BindViewModel()
        {
            // 绑定用户名
            txtUsername.DataBindings.Add("Text", _viewModel, nameof(_viewModel.Username), false, DataSourceUpdateMode.OnPropertyChanged);
            
            // 绑定密码
            txtPassword.DataBindings.Add("Text", _viewModel, nameof(_viewModel.Password), false, DataSourceUpdateMode.OnPropertyChanged);
            
            // 绑定记住密码
            chkRememberPassword.DataBindings.Add("Checked", _viewModel, nameof(_viewModel.IsRememberPassword), false, DataSourceUpdateMode.OnPropertyChanged);
            
            // 绑定错误消息
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(_viewModel.ErrorMessage))
                {
                    if (!string.IsNullOrEmpty(_viewModel.ErrorMessage))
                    {
                        // 在 UI 线程中显示消息框
                        if (InvokeRequired)
                        {
                            BeginInvoke(new Action(() =>
                            {
                                MessageBox.Show(_viewModel.ErrorMessage, "登录失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }));
                        }
                        else
                        {
                            MessageBox.Show(_viewModel.ErrorMessage, "登录失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                else if (e.PropertyName == nameof(_viewModel.IsBusy))
                {
                    btnLogin.Enabled = !_viewModel.IsBusy;
                    btnCancel.Enabled = !_viewModel.IsBusy;
                }
                else if (e.PropertyName == nameof(_viewModel.BusyMessage))
                {
                    lblStatus.Text = _viewModel.BusyMessage;
                }
            };
            
            // 登录按钮
            btnLogin.Click += async (s, e) =>
            {
                if (_viewModel.CanLogin)
                {
                    btnLogin.Enabled = false;
                    await _viewModel.ExecuteLoginAsync();
                    btnLogin.Enabled = true;
                }
            };
            
            // 登录成功事件
            _viewModel.LoginSucceeded += (s, e) =>
            {
                // TODO: 如果勾选了记住密码，保存登录信息
                // SaveLoginInfo();
                
                DialogResult = DialogResult.OK;
                Close();
            };
        }
        
        private void LoginForm_Load(object sender, EventArgs e)
        {
            // 设置默认焦点
            if (string.IsNullOrEmpty(txtUsername.Text))
            {
                txtUsername.Focus();
            }
            else
            {
                txtPassword.Focus();
            }
        }
        
        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
        
        private void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            // 按 Enter 键登录
            if (e.KeyCode == Keys.Enter && _viewModel.CanLogin)
            {
                btnLogin.PerformClick();
            }
        }
        
        #region 窗口拖动
        
        private void pnlHeader_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isDragging = true;
                _dragStartPoint = e.Location;
            }
        }
        
        private void pnlHeader_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                Point newLocation = this.Location;
                newLocation.X += e.X - _dragStartPoint.X;
                newLocation.Y += e.Y - _dragStartPoint.Y;
                this.Location = newLocation;
            }
        }
        
        private void pnlHeader_MouseUp(object sender, MouseEventArgs e)
        {
            _isDragging = false;
        }
        
        #endregion
        
        #region 自定义按钮事件
        
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
        
        private void btnMinimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }
        
        private void btnClose_MouseEnter(object sender, EventArgs e)
        {
            btnClose.BackColor = Color.FromArgb(220, 53, 69); // 红色悬停
        }
        
        private void btnClose_MouseLeave(object sender, EventArgs e)
        {
            btnClose.BackColor = Color.Transparent;
        }
        
        private void btnMinimize_MouseEnter(object sender, EventArgs e)
        {
            btnMinimize.BackColor = Color.FromArgb(0, 100, 220); // 深蓝色悬停
        }
        
        private void btnMinimize_MouseLeave(object sender, EventArgs e)
        {
            btnMinimize.BackColor = Color.Transparent;
        }
        
        #endregion
    }
}

