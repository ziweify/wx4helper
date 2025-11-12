using Sunny.UI;
using BaiShengVx3Plus.ViewModels;

namespace BaiShengVx3Plus.Views
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
                DialogResult = DialogResult.OK;
                Close();
            };
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {
            // 设置默认焦点
            txtUsername.Focus();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}

