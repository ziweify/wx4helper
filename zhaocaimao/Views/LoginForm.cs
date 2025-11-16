using Sunny.UI;
using zhaocaimao.ViewModels;
using zhaocaimao.Utils;

namespace zhaocaimao.Views
{
    public partial class LoginForm : UIForm
    {
        private readonly ConfigViewModel _viewModel;

        public LoginForm(ConfigViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindViewModel();
        }

        private void BindViewModel()
        {
            // 绑定用户名
            txtUsername.TextChanged += (s, e) => _viewModel.BsUserName = txtUsername.Text;
            
            // 绑定密码
            txtPassword.TextChanged += (s, e) => _viewModel.BsUserPass = txtPassword.Text;
            
            // 绑定记住密码
            chkRememberPassword.CheckedChanged += (s, e) => _viewModel.IsRememberPassword = chkRememberPassword.Checked;

            // 同步初始值到 ViewModel（设计器中设置的 Text 属性不会触发 TextChanged 事件）
            _viewModel.BsUserName = txtUsername.Text;
            _viewModel.BsUserPass = txtPassword.Text;
            _viewModel.IsRememberPassword = chkRememberPassword.Checked;

            // 登录按钮
            btnLogin.Click += async (s, e) =>
            {
                btnLogin.Enabled = false;
                await _viewModel.LoginCommand.ExecuteAsync(null);
                btnLogin.Enabled = true;
            };

            // 监听错误消息
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(_viewModel.ErrorMessage))
                {
                    if (!string.IsNullOrEmpty(_viewModel.ErrorMessage))
                    {
                        UIMessageBox.ShowError(_viewModel.ErrorMessage);
                    }
                }
                else if (e.PropertyName == nameof(_viewModel.IsBusy))
                {
                    btnLogin.Enabled = !_viewModel.IsBusy;
                }
            };

            // 登录成功事件
            _viewModel.LoginSucceeded += (s, e) =>
            {
                // 🔥 保存登录信息（如果勾选了记住密码）
                SaveLoginInfo();
                
                DialogResult = DialogResult.OK;
                Close();
            };
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {
            // 🔥 显示版本号 - 招财猫主题
            this.Text = $"招财猫 - 系统登录 {VersionInfo.FullVersion}";
            
            // 🔥 加载保存的登录信息
            LoadSavedLoginInfo();
            
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
        
        /// <summary>
        /// 加载保存的登录信息
        /// </summary>
        private void LoadSavedLoginInfo()
        {
            try
            {
                var configService = Program.ServiceProvider?.GetService(typeof(Contracts.IConfigurationService)) 
                    as Services.Configuration.ConfigurationService;
                
                if (configService == null)
                    return;
                
                var isRememberPassword = configService.GetIsRememberPassword();
                
                if (isRememberPassword)
                {
                    var username = configService.GetBsUserName();
                    var password = configService.GetBsUserPassword();
                    
                    // 填充到界面
                    txtUsername.Text = username;
                    txtPassword.Text = password;
                    chkRememberPassword.Checked = true;
                    
                    // 同步到 ViewModel
                    _viewModel.BsUserName = username;
                    _viewModel.BsUserPass = password;
                    _viewModel.IsRememberPassword = true;
                }
            }
            catch (Exception ex)
            {
                // 加载失败不影响登录
                System.Diagnostics.Debug.WriteLine($"加载登录信息失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 保存登录信息（登录成功后调用）
        /// </summary>
        private void SaveLoginInfo()
        {
            try
            {
                var configService = Program.ServiceProvider?.GetService(typeof(Contracts.IConfigurationService)) 
                    as Services.Configuration.ConfigurationService;
                
                if (configService == null)
                    return;
                
                configService.SaveLoginInfo(
                    txtUsername.Text,
                    txtPassword.Text,
                    chkRememberPassword.Checked
                );
            }
            catch (Exception ex)
            {
                // 保存失败不影响登录
                System.Diagnostics.Debug.WriteLine($"保存登录信息失败: {ex.Message}");
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}

