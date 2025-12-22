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
            
            // 从配置加载登录信息
            LoadLoginInfo();
            
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
                // 保存登录信息
                SaveLoginInfo();
                
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
        
        #region 登录信息保存/加载
        
        /// <summary>
        /// 从配置加载登录信息
        /// </summary>
        private void LoadLoginInfo()
        {
            try
            {
                var config = Services.Config.ConfigManager.Instance.Config.Login;
                
                // 始终加载用户名
                if (!string.IsNullOrEmpty(config.Username))
                {
                    _viewModel.Username = config.Username;
                    LoggingService.Instance.Debug("登录窗口", $"已加载用户名: {config.Username}");
                }
                
                // 如果记住密码，则加载密码
                if (config.RememberPassword && !string.IsNullOrEmpty(config.EncryptedPassword))
                {
                    var decryptedPassword = Services.Config.ConfigManager.DecryptPassword(config.EncryptedPassword);
                    _viewModel.Password = decryptedPassword;
                    _viewModel.IsRememberPassword = true;
                    LoggingService.Instance.Debug("登录窗口", "已加载密码（已加密）");
                }
            }
            catch (Exception ex)
            {
                LoggingService.Instance.Error("登录窗口", $"加载登录信息失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 保存登录信息到配置
        /// </summary>
        private void SaveLoginInfo()
        {
            try
            {
                LoggingService.Instance.Debug("登录窗口", "开始保存登录信息...");
                
                var configManager = Services.Config.ConfigManager.Instance;
                var config = configManager.Config.Login;
                
                LoggingService.Instance.Debug("登录窗口", $"ViewModel - Username: {_viewModel.Username}, Password长度: {_viewModel.Password?.Length ?? 0}, RememberPassword: {_viewModel.IsRememberPassword}");
                
                // 始终保存用户名
                config.Username = _viewModel.Username;
                LoggingService.Instance.Debug("登录窗口", $"已设置用户名到配置: {config.Username}");
                
                // 根据"记住密码"选项决定是否保存密码
                if (_viewModel.IsRememberPassword)
                {
                    // 加密并保存密码
                    config.EncryptedPassword = Services.Config.ConfigManager.EncryptPassword(_viewModel.Password);
                    config.RememberPassword = true;
                    LoggingService.Instance.Debug("登录窗口", $"已加密密码，长度: {config.EncryptedPassword?.Length ?? 0}");
                }
                else
                {
                    // 清除密码
                    config.EncryptedPassword = string.Empty;
                    config.RememberPassword = false;
                    LoggingService.Instance.Debug("登录窗口", "已清除密码");
                }
                
                LoggingService.Instance.Debug("登录窗口", "准备调用 SaveNow()...");
                
                // 立即保存配置
                configManager.SaveNow();
                
                LoggingService.Instance.Info("登录窗口", $"登录信息已保存到: {Infrastructure.Paths.AppPaths.ConfigFile}");
                LoggingService.Instance.Debug("登录窗口", "SaveLoginInfo 方法执行完成");
            }
            catch (Exception ex)
            {
                LoggingService.Instance.Error("登录窗口", $"保存登录信息失败: {ex.Message}\n堆栈: {ex.StackTrace}");
            }
        }
        
        #endregion
    }
}

