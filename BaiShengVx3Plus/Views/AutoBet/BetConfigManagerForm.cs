using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using BaiShengVx3Plus.Contracts;
using BaiShengVx3Plus.Models.AutoBet;
using BaiShengVx3Plus.Services.AutoBet;
using BaiShengVx3Plus.Shared.Platform;
using Sunny.UI;

namespace BaiShengVx3Plus.Views.AutoBet
{
    /// <summary>
    /// 自动投注配置管理器窗口
    /// </summary>
    public partial class BetConfigManagerForm : UIForm
    {
        private readonly AutoBetService _autoBetService;
        private readonly ILogService _logService;
        private BindingList<BetConfig> _configsBindingList;
        private BetConfig? _selectedConfig;

        public BetConfigManagerForm(AutoBetService autoBetService, ILogService logService)
        {
            _autoBetService = autoBetService ?? throw new ArgumentNullException(nameof(autoBetService));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
            
            _configsBindingList = new BindingList<BetConfig>();
            
            InitializeComponent();
        }

        /// <summary>
        /// 窗体加载
        /// </summary>
        private void BetConfigManagerForm_Load(object? sender, EventArgs e)
        {
            try
            {
                // 加载配置列表
                LoadConfigs();
                
                // 初始化平台URL映射
                InitializePlatformUrls();
                
                // 初始化日期范围
                dtpStartDate.Value = DateTime.Today.AddDays(-7);
                dtpEndDate.Value = DateTime.Today;
                
                _logService.Info("ConfigManager", "配置管理器已打开");
            }
            catch (Exception ex)
            {
                _logService.Error("ConfigManager", "加载配置管理器失败", ex);
                UIMessageBox.Show($"加载失败: {ex.Message}", "错误", UIStyle.Red, UIMessageBoxButtons.OK);
            }
        }

        /// <summary>
        /// 加载配置列表
        /// </summary>
        private void LoadConfigs()
        {
            var configs = _autoBetService.GetConfigs();
            
            _configsBindingList.Clear();
            foreach (var config in configs)
            {
                _configsBindingList.Add(config);
            }
            
            dgvConfigs.DataSource = _configsBindingList;
            
            // 配置列显示
            if (dgvConfigs.Columns.Count > 0)
            {
                dgvConfigs.Columns["Id"].HeaderText = "ID";
                dgvConfigs.Columns["Id"].Width = 50;
                dgvConfigs.Columns["ConfigName"].HeaderText = "配置名称";
                dgvConfigs.Columns["ConfigName"].Width = 150;
                dgvConfigs.Columns["Platform"].HeaderText = "平台";
                dgvConfigs.Columns["Platform"].Width = 100;
                dgvConfigs.Columns["Username"].HeaderText = "账号";
                dgvConfigs.Columns["Username"].Width = 120;
                dgvConfigs.Columns["IsDefault"].HeaderText = "默认";
                dgvConfigs.Columns["IsDefault"].Width = 60;
                dgvConfigs.Columns["IsEnabled"].HeaderText = "启用";
                dgvConfigs.Columns["IsEnabled"].Width = 60;
                
                // 隐藏不需要的列
                if (dgvConfigs.Columns["Password"] != null) dgvConfigs.Columns["Password"].Visible = false;
                if (dgvConfigs.Columns["PlatformUrl"] != null) dgvConfigs.Columns["PlatformUrl"].Visible = false;
                if (dgvConfigs.Columns["Cookies"] != null) dgvConfigs.Columns["Cookies"].Visible = false;
                if (dgvConfigs.Columns["Notes"] != null) dgvConfigs.Columns["Notes"].Visible = false;
                if (dgvConfigs.Columns["MinBetAmount"] != null) dgvConfigs.Columns["MinBetAmount"].Visible = false;
                if (dgvConfigs.Columns["MaxBetAmount"] != null) dgvConfigs.Columns["MaxBetAmount"].Visible = false;
                if (dgvConfigs.Columns["ShowBrowser"] != null) dgvConfigs.Columns["ShowBrowser"].Visible = false;
                if (dgvConfigs.Columns["AutoLogin"] != null) dgvConfigs.Columns["AutoLogin"].Visible = false;
                if (dgvConfigs.Columns["Status"] != null) dgvConfigs.Columns["Status"].Visible = false;
                if (dgvConfigs.Columns["Balance"] != null) dgvConfigs.Columns["Balance"].Visible = false;
                if (dgvConfigs.Columns["LastLoginTime"] != null) dgvConfigs.Columns["LastLoginTime"].Visible = false;
                if (dgvConfigs.Columns["LastUpdateTime"] != null) dgvConfigs.Columns["LastUpdateTime"].Visible = false;
            }
            
            _logService.Info("ConfigManager", $"已加载 {configs.Count} 个配置");
        }

        /// <summary>
        /// 初始化平台URL映射
        /// </summary>
        private void InitializePlatformUrls()
        {
            // 平台URL映射表（可以后续移到配置文件）
            var platformUrls = new Dictionary<string, string>
            {
                { "YunDing28", "https://www.yunding28.com" },
                { "HaiXia28", "https://www.haixia28.com" },
                { "HongHai28", "https://www.honghai28.com" }
            };
        }

        /// <summary>
        /// 配置列表选择变更
        /// </summary>
        private void dgvConfigs_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvConfigs.SelectedRows.Count == 0)
            {
                _selectedConfig = null;
                ClearConfigDetails();
                return;
            }
            
            _selectedConfig = dgvConfigs.SelectedRows[0].DataBoundItem as BetConfig;
            if (_selectedConfig != null)
            {
                LoadConfigDetails(_selectedConfig);
                LoadConfigRecords(_selectedConfig.Id);
                UpdateStatusLabel(_selectedConfig);
            }
        }

        /// <summary>
        /// 加载配置详情
        /// </summary>
        private void LoadConfigDetails(BetConfig config)
        {
            txtConfigName.Text = config.ConfigName;
            cbxPlatform.Text = config.Platform;
            txtPlatformUrl.Text = config.PlatformUrl;
            txtUsername.Text = config.Username;
            txtPassword.Text = config.Password;
            txtMinBetAmount.Text = config.MinBetAmount.ToString();
            txtMaxBetAmount.Text = config.MaxBetAmount.ToString();
            chkEnabled.Checked = config.IsEnabled;
            chkAutoLogin.Checked = config.AutoLogin;
            chkShowBrowser.Checked = config.ShowBrowser;
            txtNotes.Text = config.Notes;
            txtCookies.Text = config.Cookies;
            
            // 默认配置不允许删除
            btnDelete.Enabled = !config.IsDefault;
        }

        /// <summary>
        /// 清空配置详情
        /// </summary>
        private void ClearConfigDetails()
        {
            txtConfigName.Text = "";
            cbxPlatform.SelectedIndex = 0;
            txtPlatformUrl.Text = "";
            txtUsername.Text = "";
            txtPassword.Text = "";
            txtMinBetAmount.Text = "1";
            txtMaxBetAmount.Text = "10000";
            chkEnabled.Checked = true;
            chkAutoLogin.Checked = true;
            chkShowBrowser.Checked = false;
            txtNotes.Text = "";
            txtCookies.Text = "";
            
            btnDelete.Enabled = false;
        }

        /// <summary>
        /// 加载配置的投注记录
        /// </summary>
        private void LoadConfigRecords(int configId)
        {
            try
            {
                var startDate = dtpStartDate.Value.Date;
                var endDate = dtpEndDate.Value.Date.AddDays(1).AddSeconds(-1);
                
                // TODO: 从数据库加载投注记录
                // var records = _autoBetService.GetBetRecords(configId, startDate, endDate);
                // dgvRecords.DataSource = records;
                
                dgvRecords.DataSource = null;
            }
            catch (Exception ex)
            {
                _logService.Error("ConfigManager", "加载投注记录失败", ex);
            }
        }

        /// <summary>
        /// 更新状态标签
        /// </summary>
        private void UpdateStatusLabel(BetConfig config)
        {
            lblStatus.Text = $"状态: {config.Status}";
        }

        #region 按钮事件

        /// <summary>
        /// 新增配置
        /// </summary>
        private void btnAdd_Click(object? sender, EventArgs e)
        {
            try
            {
                var newConfig = new BetConfig
                {
                    ConfigName = "新配置",
                    Platform = "YunDing28",
                    PlatformUrl = "https://www.yunding28.com",
                    IsEnabled = true,
                    AutoLogin = true,
                    MinBetAmount = 1,
                    MaxBetAmount = 10000
                };
                
                _autoBetService.SaveConfig(newConfig);
                _configsBindingList.Add(newConfig);
                
                // 选中新配置
                dgvConfigs.ClearSelection();
                foreach (DataGridViewRow row in dgvConfigs.Rows)
                {
                    if (row.DataBoundItem == newConfig)
                    {
                        row.Selected = true;
                        break;
                    }
                }
                
                _logService.Info("ConfigManager", $"已新增配置: {newConfig.ConfigName}");
                UIMessageBox.Show("配置已新增！", "成功", UIStyle.Green, UIMessageBoxButtons.OK);
            }
            catch (Exception ex)
            {
                _logService.Error("ConfigManager", "新增配置失败", ex);
                UIMessageBox.Show($"新增失败: {ex.Message}", "错误", UIStyle.Red, UIMessageBoxButtons.OK);
            }
        }

        /// <summary>
        /// 编辑配置（当前是直接在右侧编辑）
        /// </summary>
        private void btnEdit_Click(object? sender, EventArgs e)
        {
            if (_selectedConfig == null)
            {
                UIMessageBox.Show("请先选择一个配置！", "提示", UIStyle.Blue, UIMessageBoxButtons.OK);
                return;
            }
            
            UIMessageBox.Show("请在右侧编辑配置，然后点击【保存配置】按钮。", "提示", UIStyle.Blue, UIMessageBoxButtons.OK);
        }

        /// <summary>
        /// 删除配置
        /// </summary>
        private void btnDelete_Click(object? sender, EventArgs e)
        {
            if (_selectedConfig == null)
            {
                UIMessageBox.Show("请先选择一个配置！", "提示", UIStyle.Blue, UIMessageBoxButtons.OK);
                return;
            }
            
            if (_selectedConfig.IsDefault)
            {
                UIMessageBox.Show("默认配置不能删除！", "提示", UIStyle.Orange, UIMessageBoxButtons.OK);
                return;
            }
            
            if (MessageBox.Show($"确定要删除配置【{_selectedConfig.ConfigName}】吗？", "确认", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                try
                {
                    _autoBetService.DeleteConfig(_selectedConfig.Id);
                    _configsBindingList.Remove(_selectedConfig);
                    
                    _logService.Info("ConfigManager", $"已删除配置: {_selectedConfig.ConfigName}");
                    UIMessageBox.Show("配置已删除！", "成功", UIStyle.Green, UIMessageBoxButtons.OK);
                }
                catch (Exception ex)
                {
                    _logService.Error("ConfigManager", "删除配置失败", ex);
                    UIMessageBox.Show($"删除失败: {ex.Message}", "错误", UIStyle.Red, UIMessageBoxButtons.OK);
                }
            }
        }

        /// <summary>
        /// 保存配置
        /// </summary>
        private void btnSave_Click(object? sender, EventArgs e)
        {
            if (_selectedConfig == null)
            {
                UIMessageBox.Show("请先选择一个配置！", "提示", UIStyle.Blue, UIMessageBoxButtons.OK);
                return;
            }
            
            try
            {
                // 验证输入
                if (string.IsNullOrWhiteSpace(txtConfigName.Text))
                {
                    UIMessageBox.Show("请输入配置名称！", "提示", UIStyle.Orange, UIMessageBoxButtons.OK);
                    return;
                }
                
                // 更新配置
                _selectedConfig.ConfigName = txtConfigName.Text.Trim();
                _selectedConfig.Platform = cbxPlatform.Text;
                _selectedConfig.PlatformUrl = txtPlatformUrl.Text.Trim();
                _selectedConfig.Username = txtUsername.Text.Trim();
                _selectedConfig.Password = txtPassword.Text;
                _selectedConfig.MinBetAmount = decimal.TryParse(txtMinBetAmount.Text, out var minAmount) ? minAmount : 1;
                _selectedConfig.MaxBetAmount = decimal.TryParse(txtMaxBetAmount.Text, out var maxAmount) ? maxAmount : 10000;
                _selectedConfig.IsEnabled = chkEnabled.Checked;
                _selectedConfig.AutoLogin = chkAutoLogin.Checked;
                _selectedConfig.ShowBrowser = chkShowBrowser.Checked;
                _selectedConfig.Notes = txtNotes.Text;
                _selectedConfig.Cookies = txtCookies.Text;
                
                // 保存到数据库
                _autoBetService.SaveConfig(_selectedConfig);
                
                // 刷新列表显示
                dgvConfigs.Refresh();
                
                _logService.Info("ConfigManager", $"已保存配置: {_selectedConfig.ConfigName}");
                UIMessageBox.Show("配置已保存！", "成功", UIStyle.Green, UIMessageBoxButtons.OK);
            }
            catch (Exception ex)
            {
                _logService.Error("ConfigManager", "保存配置失败", ex);
                UIMessageBox.Show($"保存失败: {ex.Message}", "错误", UIStyle.Red, UIMessageBoxButtons.OK);
            }
        }

        /// <summary>
        /// 启动浏览器
        /// </summary>
        private async void btnStartBrowser_Click(object? sender, EventArgs e)
        {
            if (_selectedConfig == null)
            {
                UIMessageBox.Show("请先选择一个配置！", "提示", UIStyle.Blue, UIMessageBoxButtons.OK);
                return;
            }
            
            try
            {
                var success = await _autoBetService.StartBrowser(_selectedConfig.Id);
                
                if (success)
                {
                    _logService.Info("ConfigManager", $"✅ 浏览器已启动: {_selectedConfig.ConfigName}");
                    UIMessageBox.Show("浏览器已启动！", "成功", UIStyle.Green, UIMessageBoxButtons.OK);
                    UpdateStatusLabel(_selectedConfig);
                }
                else
                {
                    _logService.Error("ConfigManager", $"❌ 启动浏览器失败: {_selectedConfig.ConfigName}");
                    UIMessageBox.Show("启动浏览器失败！", "错误", UIStyle.Red, UIMessageBoxButtons.OK);
                }
            }
            catch (Exception ex)
            {
                _logService.Error("ConfigManager", "启动浏览器失败", ex);
                UIMessageBox.Show($"启动失败: {ex.Message}", "错误", UIStyle.Red, UIMessageBoxButtons.OK);
            }
        }

        /// <summary>
        /// 停止浏览器
        /// </summary>
        private void btnStopBrowser_Click(object? sender, EventArgs e)
        {
            if (_selectedConfig == null)
            {
                UIMessageBox.Show("请先选择一个配置！", "提示", UIStyle.Blue, UIMessageBoxButtons.OK);
                return;
            }
            
            try
            {
                _autoBetService.StopBrowser(_selectedConfig.Id);
                
                _logService.Info("ConfigManager", $"⏹️ 浏览器已停止: {_selectedConfig.ConfigName}");
                UIMessageBox.Show("浏览器已停止！", "成功", UIStyle.Green, UIMessageBoxButtons.OK);
                UpdateStatusLabel(_selectedConfig);
            }
            catch (Exception ex)
            {
                _logService.Error("ConfigManager", "停止浏览器失败", ex);
                UIMessageBox.Show($"停止失败: {ex.Message}", "错误", UIStyle.Red, UIMessageBoxButtons.OK);
            }
        }

        /// <summary>
        /// 测试连接
        /// </summary>
        private async void btnTestConnection_Click(object? sender, EventArgs e)
        {
            if (_selectedConfig == null)
            {
                UIMessageBox.Show("请先选择一个配置！", "提示", UIStyle.Blue, UIMessageBoxButtons.OK);
                return;
            }
            
            UIMessageBox.Show("测试连接功能待实现！", "提示", UIStyle.Blue, UIMessageBoxButtons.OK);
        }

        /// <summary>
        /// 刷新投注记录
        /// </summary>
        private void btnRefreshRecords_Click(object? sender, EventArgs e)
        {
            if (_selectedConfig != null)
            {
                LoadConfigRecords(_selectedConfig.Id);
            }
        }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        private void btnClose_Click(object? sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// 平台下拉框变更
        /// </summary>
        private void cbxPlatform_SelectedIndexChanged(object? sender, EventArgs e)
        {
            // 根据平台自动填充URL
            var platformUrls = new Dictionary<string, string>
            {
                { "YunDing28", "https://www.yunding28.com" },
                { "HaiXia28", "https://www.haixia28.com" },
                { "HongHai28", "https://www.honghai28.com" }
            };
            
            if (platformUrls.TryGetValue(cbxPlatform.Text, out var url))
            {
                txtPlatformUrl.Text = url;
            }
        }

        #endregion

        #region 命令面板事件

        /// <summary>
        /// 快捷按钮：投注
        /// </summary>
        private void BtnBetCommand_Click(object? sender, EventArgs e)
        {
            txtCommand.Text = "投注(1234大10)";
        }

        /// <summary>
        /// 快捷按钮：获取盘口额度
        /// </summary>
        private void BtnGetQuotaCommand_Click(object? sender, EventArgs e)
        {
            txtCommand.Text = "获取盘口额度";
        }

        /// <summary>
        /// 快捷按钮：获取Cookie
        /// </summary>
        private void BtnGetCookieCommand_Click(object? sender, EventArgs e)
        {
            txtCommand.Text = "获取Cookie";
        }

        /// <summary>
        /// 发送命令按钮
        /// </summary>
        private async void BtnSendCommand_Click(object? sender, EventArgs e)
        {
            if (_selectedConfig == null)
            {
                AppendCommandResult("❌ 错误:未选择配置");
                return;
            }

            var command = txtCommand.Text.Trim();
            if (string.IsNullOrEmpty(command))
            {
                AppendCommandResult("❌ 错误:命令不能为空");
                return;
            }

            try
            {
                btnSendCommand.Enabled = false;
                AppendCommandResult($"📤 发送命令:{command}");
                AppendCommandResult($"   时间:{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");

                // TODO: 这里需要实现命令发送逻辑
                // 1. 解析命令（投注、获取额度、获取Cookie）
                // 2. 如果是投注命令，需要解析投注内容并生成BetRecord
                // 3. 通过Socket发送到BrowserClient
                // 4. 等待返回结果

                AppendCommandResult("⚠️ 命令发送功能正在开发中...");
                AppendCommandResult("");

                _logService.Info("CommandPanel", $"发送命令:配置[{_selectedConfig.ConfigName}] 命令[{command}]");
            }
            catch (Exception ex)
            {
                AppendCommandResult($"❌ 异常:{ex.Message}");
                _logService.Error("CommandPanel", "发送命令失败", ex);
            }
            finally
            {
                btnSendCommand.Enabled = true;
            }
        }

        /// <summary>
        /// 追加命令结果
        /// </summary>
        private void AppendCommandResult(string text)
        {
            if (InvokeRequired)
            {
                Invoke(() => AppendCommandResult(text));
                return;
            }

            txtCommandResult.Text += text + Environment.NewLine;
            
            // 自动滚动到底部
            txtCommandResult.SelectionStart = txtCommandResult.Text.Length;
            txtCommandResult.ScrollToCaret();
        }

        #endregion
    }
}

