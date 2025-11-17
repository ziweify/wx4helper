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
            
            // 🔥 订阅 DataBindingComplete 事件，在数据绑定完成后设置列属性
            dgvRecords.DataBindingComplete += DgvRecords_DataBindingComplete;
            dgvRecords.SelectionChanged += DgvRecords_SelectionChanged;
        }
        
        /// <summary>
        /// 数据绑定完成后配置列显示（避免 NullReferenceException）
        /// </summary>
        private void DgvRecords_DataBindingComplete(object? sender, DataGridViewBindingCompleteEventArgs e)
        {
            try
            {
                if (dgvRecords.Columns.Count == 0) return;
                
                // 配置列显示
                if (dgvRecords.Columns["Id"] != null)
                {
                    dgvRecords.Columns["Id"].HeaderText = "ID";
                    dgvRecords.Columns["Id"].Width = 50;
                }
                if (dgvRecords.Columns["IssueId"] != null)
                {
                    dgvRecords.Columns["IssueId"].HeaderText = "期号";
                    dgvRecords.Columns["IssueId"].Width = 100;
                }
                if (dgvRecords.Columns["Source"] != null)
                {
                    dgvRecords.Columns["Source"].HeaderText = "来源";
                    dgvRecords.Columns["Source"].Width = 60;
                }
                if (dgvRecords.Columns["BetContentStandard"] != null)
                {
                    dgvRecords.Columns["BetContentStandard"].HeaderText = "投注内容";
                    dgvRecords.Columns["BetContentStandard"].Width = 200;
                }
                if (dgvRecords.Columns["TotalAmount"] != null)
                {
                    dgvRecords.Columns["TotalAmount"].HeaderText = "金额";
                    dgvRecords.Columns["TotalAmount"].Width = 80;
                }
                if (dgvRecords.Columns["Success"] != null)
                {
                    dgvRecords.Columns["Success"].HeaderText = "成功";
                    dgvRecords.Columns["Success"].Width = 60;
                }
                if (dgvRecords.Columns["DurationMs"] != null)
                {
                    dgvRecords.Columns["DurationMs"].HeaderText = "耗时(ms)";
                    dgvRecords.Columns["DurationMs"].Width = 80;
                }
                if (dgvRecords.Columns["SendTime"] != null)
                {
                    dgvRecords.Columns["SendTime"].HeaderText = "发送时间";
                    dgvRecords.Columns["SendTime"].Width = 150;
                    dgvRecords.Columns["SendTime"].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm:ss";
                }
                
                // 隐藏不需要的列
                if (dgvRecords.Columns["ConfigId"] != null) dgvRecords.Columns["ConfigId"].Visible = false;
                if (dgvRecords.Columns["OrderIds"] != null) dgvRecords.Columns["OrderIds"].Visible = false;
                if (dgvRecords.Columns["PostStartTime"] != null) dgvRecords.Columns["PostStartTime"].Visible = false;
                if (dgvRecords.Columns["PostEndTime"] != null) dgvRecords.Columns["PostEndTime"].Visible = false;
                if (dgvRecords.Columns["Result"] != null) dgvRecords.Columns["Result"].Visible = false;
                if (dgvRecords.Columns["ErrorMessage"] != null) dgvRecords.Columns["ErrorMessage"].Visible = false;
                if (dgvRecords.Columns["OrderNo"] != null) dgvRecords.Columns["OrderNo"].Visible = false;
                if (dgvRecords.Columns["CreateTime"] != null) dgvRecords.Columns["CreateTime"].Visible = false;
                if (dgvRecords.Columns["UpdateTime"] != null) dgvRecords.Columns["UpdateTime"].Visible = false;
            }
            catch (Exception ex)
            {
                _logService.Error("BetConfigManagerForm", $"配置列显示失败: {ex.Message}", ex);
            }
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
                // 🔥 配置名称：4个中文宽度（使用 TextRenderer 精确测量）
                using (var g = dgvConfigs.CreateGraphics())
                {
                    var font = dgvConfigs.Font;
                    int width = System.Windows.Forms.TextRenderer.MeasureText("配置名称", font).Width; // 4个中文字符
                    dgvConfigs.Columns["ConfigName"].Width = width + 20; // 加上边距
                }
                dgvConfigs.Columns["Platform"].HeaderText = "平台";
                // 🔥 平台：3个中文宽度
                using (var g = dgvConfigs.CreateGraphics())
                {
                    var font = dgvConfigs.Font;
                    int width = System.Windows.Forms.TextRenderer.MeasureText("平台台", font).Width; // 3个中文字符
                    dgvConfigs.Columns["Platform"].Width = width + 20; // 加上边距
                }
                dgvConfigs.Columns["Username"].HeaderText = "账号";
                // 🔥 账号：7个字母宽度
                using (var g = dgvConfigs.CreateGraphics())
                {
                    var font = dgvConfigs.Font;
                    int width = System.Windows.Forms.TextRenderer.MeasureText("ABCDEFG", font).Width; // 7个字母
                    dgvConfigs.Columns["Username"].Width = width + 20; // 加上边距
                }
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
        /// 投注记录选择变更事件 - 显示详细信息
        /// </summary>
        private void DgvRecords_SelectionChanged(object? sender, EventArgs e)
        {
            try
            {
                if (dgvRecords.SelectedRows.Count == 0)
                {
                    tbxRecordsDetailed.Text = "";
                    return;
                }
                
                var selectedRow = dgvRecords.SelectedRows[0];
                var record = selectedRow.DataBoundItem as BetRecord;
                
                if (record == null)
                {
                    tbxRecordsDetailed.Text = "无法获取记录数据";
                    return;
                }
                
                // 🔥 格式化输出所有字段信息
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("═══════════════════════════════════════");
                sb.AppendLine("📋 投注记录详细信息");
                sb.AppendLine("═══════════════════════════════════════");
                sb.AppendLine();
                
                // 基本信息
                sb.AppendLine("【基本信息】");
                sb.AppendLine($"ID: {record.Id}");
                sb.AppendLine($"配置ID: {record.ConfigId}");
                sb.AppendLine($"期号: {record.IssueId}");
                sb.AppendLine($"来源: {record.Source}");
                sb.AppendLine();
                
                // 投注内容
                sb.AppendLine("【投注内容】");
                sb.AppendLine($"投注内容: {record.BetContentStandard ?? "(空)"}");
                sb.AppendLine($"总金额: {record.TotalAmount:F2} 元");
                sb.AppendLine($"关联订单ID: {record.OrderIds ?? "(无)"}");
                sb.AppendLine();
                
                // 时间信息
                sb.AppendLine("【时间信息】");
                sb.AppendLine($"发送时间: {record.SendTime:yyyy-MM-dd HH:mm:ss.fff}");
                sb.AppendLine($"POST开始: {record.PostStartTime?.ToString("yyyy-MM-dd HH:mm:ss.fff") ?? "(未记录)"}");
                sb.AppendLine($"POST结束: {record.PostEndTime?.ToString("yyyy-MM-dd HH:mm:ss.fff") ?? "(未记录)"}");
                if (record.PostStartTime.HasValue && record.PostEndTime.HasValue)
                {
                    var actualDuration = (record.PostEndTime.Value - record.PostStartTime.Value).TotalMilliseconds;
                    sb.AppendLine($"实际耗时: {actualDuration:F2} ms");
                }
                sb.AppendLine($"记录耗时: {record.DurationMs?.ToString() ?? "(未记录)"} ms");
                sb.AppendLine($"创建时间: {record.CreateTime:yyyy-MM-dd HH:mm:ss.fff}");
                sb.AppendLine($"更新时间: {record.UpdateTime?.ToString("yyyy-MM-dd HH:mm:ss.fff") ?? "(未更新)"}");
                sb.AppendLine();
                
                // 结果信息
                sb.AppendLine("【结果信息】");
                sb.AppendLine($"成功状态: {GetSuccessStatus(record.Success)}");
                sb.AppendLine($"平台订单号: {record.OrderNo ?? "(无)"}");
                sb.AppendLine();
                
                // 错误信息（如果有）
                if (!string.IsNullOrEmpty(record.ErrorMessage))
                {
                    sb.AppendLine("【❌ 错误信息】");
                    sb.AppendLine(record.ErrorMessage);
                    sb.AppendLine();
                }
                
                // 返回结果（如果有）
                if (!string.IsNullOrEmpty(record.Result))
                {
                    sb.AppendLine("【📥 平台返回结果】");
                    // 🔥 智能格式化JSON显示
                    sb.AppendLine(FormatJsonForDisplay(record.Result));
                    sb.AppendLine();
                }
                
                sb.AppendLine("═══════════════════════════════════════");
                
                tbxRecordsDetailed.Text = sb.ToString();
            }
            catch (Exception ex)
            {
                _logService.Error("BetConfigManagerForm", $"显示记录详情失败: {ex.Message}", ex);
                tbxRecordsDetailed.Text = $"显示记录详情失败: {ex.Message}";
            }
        }
        
        /// <summary>
        /// 获取成功状态的文本描述
        /// </summary>
        private string GetSuccessStatus(bool? success)
        {
            if (success == null) return "⏳ 等待中";
            if (success == true) return "✅ 成功";
            return "❌ 失败";
        }
        
        /// <summary>
        /// 格式化JSON用于显示（自动解析转义的JSON字符串）
        /// </summary>
        private string FormatJsonForDisplay(string jsonString)
        {
            try
            {
                // 🔥 尝试解析为JSON对象
                var jsonObj = Newtonsoft.Json.Linq.JToken.Parse(jsonString);
                
                // 🔥 如果是对象，格式化显示（缩进2个空格）
                return jsonObj.ToString(Newtonsoft.Json.Formatting.Indented);
            }
            catch
            {
                // 🔥 如果解析失败，可能是转义过的JSON字符串
                try
                {
                    // 尝试反序列化为字符串（去掉外层转义）
                    var unescaped = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(jsonString);
                    if (!string.IsNullOrEmpty(unescaped) && unescaped != jsonString)
                    {
                        // 递归尝试格式化反转义后的字符串
                        return FormatJsonForDisplay(unescaped);
                    }
                }
                catch
                {
                    // 忽略错误
                }
                
                // 🔥 如果都失败了，返回原始字符串
                return jsonString;
            }
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
                
                // 从数据库加载投注记录
                var betRecordService = Program.ServiceProvider.GetService(typeof(Services.AutoBet.BetRecordService)) 
                    as Services.AutoBet.BetRecordService;
                
                if (betRecordService != null)
                {
                    var records = betRecordService.GetByConfigAndDateRange(configId, startDate, endDate);
                    dgvRecords.DataSource = records;
                    
                    // 🔥 列配置已移至 DataBindingComplete 事件中处理，避免 NullReferenceException
                    
                    _logService.Info("ConfigManager", $"已加载{records.Length}条投注记录");
                }
                else
                {
                    dgvRecords.DataSource = null;
                    _logService.Warning("ConfigManager", "BetRecordService未初始化");
                }
            }
            catch (Exception ex)
            {
                _logService.Error("ConfigManager", "加载投注记录失败", ex);
                dgvRecords.DataSource = null;
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

                // 1. 解析命令
                var (cmdName, cmdParam) = ParseCommand(command);
                
                if (string.IsNullOrEmpty(cmdName))
                {
                    AppendCommandResult("❌ 错误:无法解析命令");
                    AppendCommandResult("💡 命令格式:");
                    AppendCommandResult("   • 投注: 投注(1234大10)");
                    AppendCommandResult("   • 获取额度: 获取盘口额度");
                    AppendCommandResult("   • 获取Cookie: 获取Cookie");
                    return;
                }
                
                AppendCommandResult($"📝 命令:{cmdName}");
                if (!string.IsNullOrEmpty(cmdParam))
                {
                    AppendCommandResult($"   参数:{cmdParam}");
                }

                // 2. 通过AutoBetService发送Socket命令
                var result = await SendCommandToBrowserAsync(cmdName, cmdParam);
                
                // 3. 显示结果（格式优化）
                AppendCommandResult("");
                AppendCommandResult("==================================================");
                AppendCommandResult($"✅ 执行结果:成功={result.Success}");
                AppendCommandResult($"   消息:{result.Message ?? "(无)"}");
                
                if (result.Data != null)
                {
                    var dataJson = Newtonsoft.Json.JsonConvert.SerializeObject(result.Data, Newtonsoft.Json.Formatting.Indented);
                    AppendCommandResult($"   返回数据:");
                    AppendCommandResult(dataJson);
                }
                else
                {
                    AppendCommandResult($"   返回数据:(无)");
                }
                
                if (!string.IsNullOrEmpty(result.ErrorMessage))
                {
                    AppendCommandResult($"   错误信息:{result.ErrorMessage}");
                }
                
                AppendCommandResult("==================================================");
                AppendCommandResult("");

                _logService.Info("CommandPanel", $"发送命令:配置[{_selectedConfig.ConfigName}] 命令[{command}] 结果={result.Success}");
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
        
        /// <summary>
        /// 解析命令：支持 "投注(1234大10)" 或 "获取Cookie"
        /// </summary>
        private (string cmdName, string cmdParam) ParseCommand(string command)
        {
            try
            {
                var trimmed = command.Trim();
                
                // 检查是否包含括号
                var openParen = trimmed.IndexOf('(');
                var closeParen = trimmed.LastIndexOf(')');
                
                if (openParen > 0 && closeParen > openParen)
                {
                    // 带参数：投注(1234大10)
                    var cmdName = trimmed.Substring(0, openParen).Trim();
                    var cmdParam = trimmed.Substring(openParen + 1, closeParen - openParen - 1).Trim();
                    return (cmdName, cmdParam);
                }
                else
                {
                    // 无参数命令：获取Cookie, 获取盘口额度
                    return (trimmed, "");
                }
            }
            catch
            {
                return ("", "");
            }
        }
        
        /// <summary>
        /// 发送命令到浏览器客户端
        /// </summary>
        private async Task<CommandResponse> SendCommandToBrowserAsync(string cmdName, string cmdParam)
        {
            var defaultResponse = new CommandResponse
            {
                Success = false,
                Message = "未实现"
            };
            
            try
            {
                if (_selectedConfig == null)
                {
                    return new CommandResponse { Success = false, Message = "未选择配置" };
                }
                
                // 通过AutoBetService获取BrowserClient连接
                var autoBetService = Program.ServiceProvider.GetService(typeof(Services.AutoBet.AutoBetService)) as Services.AutoBet.AutoBetService;
                if (autoBetService == null)
                {
                    return new CommandResponse { Success = false, Message = "AutoBetService未初始化" };
                }
                
                // 根据命令类型调用不同的方法
                switch (cmdName)
                {
                    case "投注":
                        // 1. 获取当前期号
                        var lotteryService = Program.ServiceProvider.GetService(typeof(Contracts.Games.IBinggoLotteryService)) 
                            as Contracts.Games.IBinggoLotteryService;
                        var currentIssueId = lotteryService?.CurrentIssueId ?? 0;
                        
                        if (currentIssueId == 0)
                        {
                            _logService.Warning("CommandPanel", "无法获取当前期号，将使用期号0");
                        }
                        
                        // 2. 解析投注内容
                        var originalContent = cmdParam; // "1234大10"
                        var standardContent = BaiShengVx3Plus.Shared.Parsers.BetContentParser.ParseBetContentToString(originalContent); // "1大10,2大10,3大10,4大10"
                        var totalAmount = CalculateTotalAmount(standardContent);
                        
                        _logService.Info("CommandPanel", $"投注解析:原始={originalContent} 标准={standardContent} 金额={totalAmount}");
                        
                        // 3. 生成BetRecord
                        var betRecordService = Program.ServiceProvider.GetService(typeof(Services.AutoBet.BetRecordService)) 
                            as Services.AutoBet.BetRecordService;
                        
                        if (betRecordService == null)
                        {
                            return new CommandResponse 
                            { 
                                Success = false, 
                                Message = "BetRecordService未初始化" 
                            };
                        }
                        
                        var betRecord = new Models.AutoBet.BetRecord
                        {
                            ConfigId = _selectedConfig.Id,
                            IssueId = currentIssueId,
                            Source = Models.AutoBet.BetRecordSource.命令, // 手动命令
                            OrderIds = "", // 手动投注无关联订单
                            BetContentStandard = standardContent,
                            TotalAmount = totalAmount,
                            SendTime = DateTime.Now
                        };
                        
                        betRecord = betRecordService.Create(betRecord);
                        
                        if (betRecord == null)
                        {
                            return new CommandResponse
                            {
                                Success = false,
                                Message = "创建投注记录失败",
                                ErrorMessage = "数据库未初始化"
                            };
                        }
                        
                        _logService.Info("CommandPanel", $"BetRecord已创建:ID={betRecord.Id}");
                        
                        // 4. 发送投注命令
                        _logService.Info("CommandPanel", $"准备发送投注命令:ConfigId={_selectedConfig.Id}, IssueId={currentIssueId}, Content={standardContent}");
                        AppendCommandResult($"⏳ 正在发送投注命令到浏览器...");
                        
                        var betResult = await autoBetService.SendBetCommandAsync(
                            _selectedConfig.Id, 
                            currentIssueId.ToString(), 
                            standardContent
                        );
                        
                        _logService.Info("CommandPanel", $"投注命令返回:Success={betResult.Success}, Error={betResult.ErrorMessage}");
                        AppendCommandResult($"✅ 浏览器已返回结果");
                        
                        // 5. 更新BetRecord
                        betRecord.Success = betResult.Success;
                        betRecord.PostStartTime = betResult.PostStartTime;
                        betRecord.PostEndTime = betResult.PostEndTime;
                        betRecord.DurationMs = betResult.DurationMs;
                        betRecord.Result = betResult.Result;
                        betRecord.ErrorMessage = betResult.ErrorMessage;
                        betRecord.OrderNo = betResult.OrderNo;
                        betRecordService.Update(betRecord);
                        
                        _logService.Info("CommandPanel", $"BetRecord已更新:成功={betRecord.Success}");
                        
                        // 🔥 刷新投注记录列表
                        if (InvokeRequired)
                        {
                            Invoke(() => LoadConfigRecords(_selectedConfig.Id));
                        }
                        else
                        {
                            LoadConfigRecords(_selectedConfig.Id);
                        }
                        
                        // 🔥 安全地构建返回数据（避免 JToken 序列化错误）
                        var responseData = new Dictionary<string, object?>
                        {
                            ["betRecordId"] = betRecord.Id,
                            ["issueId"] = currentIssueId,
                            ["originalContent"] = originalContent,
                            ["standardContent"] = standardContent,
                            ["totalAmount"] = totalAmount,
                            ["betResult"] = new Dictionary<string, object?>
                            {
                                ["Success"] = betResult.Success,
                                ["OrderId"] = betResult.OrderId,
                                ["Result"] = betResult.Result,
                                ["ErrorMessage"] = betResult.ErrorMessage,
                                ["PostStartTime"] = betResult.PostStartTime?.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                                ["PostEndTime"] = betResult.PostEndTime?.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                                ["DurationMs"] = betResult.DurationMs,
                                ["OrderNo"] = betResult.OrderNo,
                                ["Data"] = betResult.Data is Newtonsoft.Json.Linq.JToken jToken 
                                    ? jToken.ToString()  // 将 JToken 转换为字符串，避免序列化错误
                                    : betResult.Data
                            }
                        };
                        
                        return new CommandResponse
                        {
                            Success = betResult.Success,
                            Message = betResult.ErrorMessage ?? (betResult.Success ? "投注成功" : "投注失败"),
                            Data = responseData,
                            ErrorMessage = betResult.ErrorMessage
                        };
                        
                    case "获取Cookie":
                        var cookieResult = await SendSocketCommandAsync(_selectedConfig.Id, "获取Cookie", null);
                        return cookieResult;
                        
                    case "获取盘口额度":
                        var quotaResult = await SendSocketCommandAsync(_selectedConfig.Id, "获取盘口额度", null);
                        return quotaResult;
                        
                    default:
                        return new CommandResponse { Success = false, Message = $"未知命令:{cmdName}" };
                }
            }
            catch (Exception ex)
            {
                _logService.Error("CommandPanel", $"发送命令失败:{cmdName}", ex);
                return new CommandResponse
                {
                    Success = false,
                    Message = "发送失败",
                    ErrorMessage = ex.Message
                };
            }
        }
        
        /// <summary>
        /// 计算总金额："1大10,2大20" → 30
        /// </summary>
        private decimal CalculateTotalAmount(string standardContent)
        {
            try
            {
                decimal total = 0;
                var items = standardContent.Split(',');
                
                foreach (var item in items)
                {
                    var match = System.Text.RegularExpressions.Regex.Match(item, @"(\d+)$");
                    if (match.Success && decimal.TryParse(match.Groups[1].Value, out var amount))
                    {
                        total += amount;
                    }
                }
                
                return total;
            }
            catch
            {
                return 0;
            }
        }
        
        /// <summary>
        /// 发送Socket命令（通用方法）
        /// </summary>
        private async Task<CommandResponse> SendSocketCommandAsync(int configId, string command, object? data)
        {
            try
            {
                var autoBetService = Program.ServiceProvider.GetService(typeof(Services.AutoBet.AutoBetService)) as Services.AutoBet.AutoBetService;
                if (autoBetService == null)
                {
                    return new CommandResponse { Success = false, Message = "AutoBetService未初始化" };
                }
                
                // 通过AutoBetService的BrowserClient发送命令
                var browserClient = autoBetService.GetBrowserClient(configId);
                if (browserClient == null)
                {
                    return new CommandResponse { Success = false, Message = "浏览器客户端未连接" };
                }
                
                var result = await browserClient.SendCommandAsync(command, data);
                
                return new CommandResponse
                {
                    Success = result.Success,
                    Message = result.ErrorMessage ?? (result.Success ? "成功" : "失败"),
                    Data = result.Data,
                    ErrorMessage = result.ErrorMessage
                };
            }
            catch (Exception ex)
            {
                return new CommandResponse
                {
                    Success = false,
                    Message = "发送命令异常",
                    ErrorMessage = ex.Message
                };
            }
        }

        #endregion
    }
    
    /// <summary>
    /// 命令响应（临时数据结构）
    /// </summary>
    public class CommandResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public object? Data { get; set; }
        public string? ErrorMessage { get; set; }
    }
}

