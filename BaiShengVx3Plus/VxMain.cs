using Sunny.UI;
using BaiShengVx3Plus.ViewModels;
using System.ComponentModel;

namespace BaiShengVx3Plus
{
    public partial class VxMain : UIForm
    {
        private readonly VxMainViewModel _viewModel;

        public VxMain(VxMainViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindViewModel();
        }

        private void BindViewModel()
        {
            // 绑定当前用户信息
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;

            // 绑定InsUser列表
            dgvInsUsers.DataSource = new BindingList<Models.InsUser>(_viewModel.InsUsers.ToList());

            // 登出事件
            _viewModel.LogoutRequested += (s, e) =>
            {
                Application.Restart();
                Environment.Exit(0);
            };
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(() => ViewModel_PropertyChanged(sender, e));
                return;
            }

            switch (e.PropertyName)
            {
                case nameof(_viewModel.CurrentUser):
                    UpdateCurrentUserDisplay();
                    break;
                case nameof(_viewModel.SelectedInsUser):
                    UpdateSelectedUserDisplay();
                    break;
                case nameof(_viewModel.StatusMessage):
                    lblStatus.Text = _viewModel.StatusMessage;
                    break;
                case nameof(_viewModel.InsUsers):
                    dgvInsUsers.DataSource = new BindingList<Models.InsUser>(_viewModel.InsUsers.ToList());
                    break;
            }
        }

        private void UpdateCurrentUserDisplay()
        {
            if (_viewModel.CurrentUser != null)
            {
                // 更新当前用户显示（如果需要）
            }
        }

        private void UpdateSelectedUserDisplay()
        {
            if (_viewModel.SelectedInsUser != null)
            {
                var user = _viewModel.SelectedInsUser;
                lblUserId.Text = user.Id;
                lblUserName.Text = user.UserName;
                txtAccount.Text = user.Account;
                txtPassword.Text = user.Password;
                txtAddress.Text = user.Address;
                lblBalance.Text = user.Balance.ToString("F2");
                lblLastLoginTime.Text = user.LastLoginTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "";
                lblCurrentTime.Text = user.CurrentTime.ToString("yyyy-MM-dd HH:mm:ss");
                numSeconds.Value = user.Seconds;
            }
        }

        private void VxMain_Load(object sender, EventArgs e)
        {
            // 初始化界面
            lblStatus.Text = _viewModel.StatusMessage;
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            _viewModel.RefreshCommand.Execute(null);
        }

        private void btnAddUser_Click(object sender, EventArgs e)
        {
            _viewModel.AddInsUserCommand.Execute(null);
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            _viewModel.OpenSettingsCommand.Execute(null);
        }

        private void btnSubscription_Click(object sender, EventArgs e)
        {
            _viewModel.OpenSubscriptionManagementCommand.Execute(null);
        }

        private void btnWeChatCard_Click(object sender, EventArgs e)
        {
            _viewModel.OpenWeChatDataCardCommand.Execute(null);
        }

        private void btnModifyPassword_Click(object sender, EventArgs e)
        {
            _viewModel.ModifyPasswordCommand.Execute(null);
        }

        private void btnRecharge_Click(object sender, EventArgs e)
        {
            _viewModel.RechargeCommand.Execute(null);
        }

        private void btnTransfer_Click(object sender, EventArgs e)
        {
            _viewModel.TransferPointsCommand.Execute(null);
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            if (UIMessageBox.ShowAsk("确定要退出系统吗？"))
            {
                _viewModel.LogoutCommand.Execute(null);
            }
        }

        private void dgvInsUsers_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvInsUsers.CurrentRow != null && dgvInsUsers.CurrentRow.DataBoundItem is Models.InsUser user)
            {
                _viewModel.SelectedInsUser = user;
            }
        }
    }
}
