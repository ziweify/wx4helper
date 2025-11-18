using Sunny.UI;
using System.Text.RegularExpressions;
using System.Text.Json;
using zhaocaimao.Contracts;
using zhaocaimao.Models.Games.Binggo;
using zhaocaimao.ViewModels;

namespace zhaocaimao.Views
{
    /// <summary>
    /// 设置窗口
    /// </summary>
    public partial class SettingsForm : UIForm
    {
        private readonly IWeixinSocketClient _socketClient;
        private readonly ILogService _logService;
        private readonly SettingViewModel _settingVmodel;
        private readonly IConfigurationService _configService; // 📝 配置服务
        
        /// <summary>
        /// 🔧 模拟消息处理回调（由 VxMain 提供）
        /// </summary>
        private Func<string, string, Task<(bool success, string? replyMessage, string? errorMessage)>>? _simulateMessageCallback;
        
        public SettingsForm(
            IWeixinSocketClient socketClient, 
            ILogService logService,
            SettingViewModel setting,
            IConfigurationService configService, // 📝 注入配置服务
            Func<string, string, Task<(bool, string?, string?)>>? simulateMessageCallback = null) // 🔧 模拟消息回调
        {
            InitializeComponent();
            _socketClient = socketClient;
            _logService = logService;
            _settingVmodel = setting;
            _configService = configService;
            _simulateMessageCallback = simulateMessageCallback;
            
            // 加载设置
            LoadSettings();
            
            // 🔧 绑定开发模式按钮事件
            btnRunDevSendCommand.Click += BtnRunDevSendCommand_Click;
        }

        private void LoadSettings()
        {
            // Socket 连接设置
            txtHost.Text = "127.0.0.1";
            txtPort.Text = "6328";
            txtReconnectInterval.Text = "5000";
            
            // 更新连接状态
            UpdateConnectionStatus();

            // ✅ 数据绑定（必须在手动设置值之前建立绑定）
            chkRunModeAdminSettings.DataBindings.Add(
                new Binding("Checked", _settingVmodel, "Is管理模式", 
                    false, // formattingEnabled
                    DataSourceUpdateMode.OnPropertyChanged)); // 🔥 关键：属性变化时立即更新
            
            chkRunModelDev.DataBindings.Add(
                new Binding("Checked", _settingVmodel, "Is开发模式", 
                    false, 
                    DataSourceUpdateMode.OnPropertyChanged)); // 🔥 关键：属性变化时立即更新
            
            // 🔧 开发模式：绑定测试会员和测试消息
            // 由于 IConfigurationService 是接口不支持直接绑定，使用手动方式
            tbxRunDevCurrentMember.Text = _configService.GetRunDevCurrentMember();
            tbxRunDevSendMessage.Text = _configService.GetRunDevSendMessage();
            
            // 🔧 手动订阅 TextChanged 事件来同步数据
            tbxRunDevCurrentMember.TextChanged += (s, e) => 
            {
                _configService.SetRunDevCurrentMember(tbxRunDevCurrentMember.Text);
            };
            
            tbxRunDevSendMessage.TextChanged += (s, e) => 
            {
                _configService.SetRunDevSendMessage(tbxRunDevSendMessage.Text);
            };
            
            // 🔥 收单关闭时不发送系统消息 checkbox
            chk收单关闭时不发送系统消息.Checked = _configService.Get收单关闭时不发送系统消息();
            chk收单关闭时不发送系统消息.CheckedChanged += (s, e) =>
            {
                _configService.Set收单关闭时不发送系统消息(chk收单关闭时不发送系统消息.Checked);
                _logService.Info("SettingsForm", $"收单关闭时不发送系统消息: {chk收单关闭时不发送系统消息.Checked}");
            };
            
            // 🔍 测试：验证绑定是否生效（初始值）
            _logService.Info("SettingsForm", $"📋 设置加载: 管理模式={_settingVmodel.Is管理模式}, 开发模式={_settingVmodel.Is开发模式}");
            _logService.Info("SettingsForm", $"📋 UI显示: 管理模式Checked={chkRunModeAdminSettings.Checked}, 开发模式Checked={chkRunModelDev.Checked}");
            _logService.Info("SettingsForm", $"🔧 开发模式配置: 当前会员={tbxRunDevCurrentMember.Text}, 测试消息={tbxRunDevSendMessage.Text}");
            
            // 🔍 测试：验证属性变更通知是否工作
            _settingVmodel.PropertyChanged += (s, e) =>
            {
                _logService.Info("SettingsForm", $"🔔 ViewModel 属性变更: {e.PropertyName}");
            };
            
            // 🔥 加载其他游戏设置（但不覆盖已绑定的控件）
            // LoadGameSettings(); // ❌ 注释掉，避免覆盖数据绑定
        }
        
        /// <summary>
        /// 加载游戏设置到 UI
        /// 🔥 只加载管理模式（其他游戏设置在快速设置面板）
        /// </summary>
        private void LoadGameSettings()
        {
            // 🔥 管理模式（系统设置）- 已通过数据绑定自动加载
            // chkRunModeAdminSettings 已绑定到 _settingVmodel.Is管理模式
            
            _logService.Info("SettingsForm", "✅ 系统设置已加载");
        }

        private void SaveSettings()
        {
            try
            {
                // 保存游戏设置
                SaveGameSettings();
                
                // TODO: 保存到配置文件
                _logService.Info("SettingsForm", $"设置已保存: Host={txtHost.Text}, Port={txtPort.Text}");
                UIMessageBox.ShowSuccess("设置已保存！");
            }
            catch (Exception ex)
            {
                _logService.Error("SettingsForm", "保存设置失败", ex);
                UIMessageBox.ShowError($"保存失败:\n{ex.Message}");
            }
        }
        
        /// <summary>
        /// 保存游戏设置
        /// 🔥 只保存管理模式（其他游戏设置在快速设置面板）
        /// </summary>
        private void SaveGameSettings()
        {
            // 🔥 管理模式（系统设置）- 已通过数据绑定自动保存
            // chkRunModeAdminSettings 已绑定到 _settingVmodel.Is管理模式
            
            _logService.Info("SettingsForm", 
                $"✅ 系统设置已保存: 管理模式={_configService.GetIsRunModeAdmin()}");
        }

        private void UpdateConnectionStatus()
        {
            if (_socketClient.IsConnected)
            {
                lblConnectionStatus.Text = "已连接 ✓";
                lblConnectionStatus.ForeColor = Color.Green;
                btnConnect.Text = "断开连接";
                btnConnect.Enabled = true;
            }
            else
            {
                lblConnectionStatus.Text = "未连接 ✗";
                lblConnectionStatus.ForeColor = Color.Red;
                btnConnect.Text = "连接";
                btnConnect.Enabled = true;
            }
        }

        #region Socket 连接测试

        private async void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                if (_socketClient.IsConnected)
                {
                    // 断开连接
                    _socketClient.Disconnect();
                    _logService.Info("SettingsForm", "已断开连接");
                    UpdateConnectionStatus();
                }
                else
                {
                    // 连接
                    string host = txtHost.Text.Trim();
                    int port = int.Parse(txtPort.Text.Trim());

                    lblConnectionStatus.Text = "连接中...";
                    btnConnect.Enabled = false;

                    bool success = await _socketClient.ConnectAsync(host, port, 5000);

                    if (success)
                    {
                        _logService.Info("SettingsForm", $"连接成功: {host}:{port}");
                        UIMessageBox.ShowSuccess($"连接成功！\n{host}:{port}");
                    }
                    else
                    {
                        _logService.Error("SettingsForm", "连接失败");
                        UIMessageBox.ShowError("连接失败！\n请检查服务器是否启动");
                    }

                    UpdateConnectionStatus();
                }
            }
            catch (Exception ex)
            {
                _logService.Error("SettingsForm", "连接操作失败", ex);
                UIMessageBox.ShowError($"操作失败:\n{ex.Message}");
                UpdateConnectionStatus();
            }
        }

        private void btnRefreshStatus_Click(object sender, EventArgs e)
        {
            UpdateConnectionStatus();
            _logService.Info("SettingsForm", $"连接状态: {_socketClient.IsConnected}");
        }

        #endregion

        #region Socket 命令测试

        private async void btnSendCommand_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_socketClient.IsConnected)
                {
                    UIMessageBox.ShowWarning("请先连接到服务器！");
                    return;
                }

                string commandText = txtCommand.Text.Trim();
                if (string.IsNullOrEmpty(commandText))
                {
                    UIMessageBox.ShowWarning("请输入命令！");
                    return;
                }

                // 解析命令
                var (method, parameters) = ParseCommand(commandText);
                if (method == null)
                {
                    UIMessageBox.ShowError("命令格式错误！\n正确格式: MethodName(param1, param2, ...)");
                    return;
                }

                _logService.Info("SettingsForm", $"发送命令: {method}({string.Join(", ", parameters)})");

                // 显示发送信息
                AppendResult($">>> 发送命令: {commandText}", Color.Blue);

                // 发送命令
                btnSendCommand.Enabled = false;
                btnSendCommand.Text = "发送中...";

                // 使用 JsonDocument 替代 dynamic
                var result = await _socketClient.SendAsync<JsonDocument>(method, 10000, parameters);

                // 显示结果
                if (result != null)
                {
                    // JsonDocument 转为格式化的 JSON 字符串
                    string jsonResult = JsonSerializer.Serialize(result.RootElement, new JsonSerializerOptions 
                    { 
                        WriteIndented = true,
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    });
                    AppendResult($"<<< 响应:\n{jsonResult}", Color.Green);
                    _logService.Info("SettingsForm", $"收到响应: {jsonResult}");
                }
                else
                {
                    AppendResult("<<< 响应: (null)", Color.Red);
                    _logService.Warning("SettingsForm", "收到空响应");
                }
            }
            catch (Exception ex)
            {
                AppendResult($"!!! 错误: {ex.Message}", Color.Red);
                _logService.Error("SettingsForm", "发送命令失败", ex);
                UIMessageBox.ShowError($"发送失败:\n{ex.Message}");
            }
            finally
            {
                btnSendCommand.Enabled = true;
                btnSendCommand.Text = "发送";
            }
        }

        private void btnClearResult_Click(object sender, EventArgs e)
        {
            txtResult.Clear();
        }

        private void btnQuickCommand_Click(object sender, EventArgs e)
        {
            if (sender is UIButton btn)
            {
                txtCommand.Text = btn.Tag?.ToString() ?? "";
            }
        }

        /// <summary>
        /// 解析命令格式: MethodName(param1, param2, ...)
        /// </summary>
        private (string? method, object[] parameters) ParseCommand(string commandText)
        {
            try
            {
                // 正则匹配: MethodName(param1, param2, ...)
                var match = Regex.Match(commandText, @"^(\w+)\((.*)\)$");
                if (!match.Success)
                {
                    return (null, Array.Empty<object>());
                }

                string method = match.Groups[1].Value;
                string paramsText = match.Groups[2].Value.Trim();

                // 解析参数
                object[] parameters;
                if (string.IsNullOrEmpty(paramsText))
                {
                    parameters = Array.Empty<object>();
                }
                else
                {
                    // 按逗号分割参数
                    var paramList = new List<object>();
                    var parts = paramsText.Split(',');
                    
                    foreach (var part in parts)
                    {
                        string trimmed = part.Trim();
                        
                        // 尝试解析为不同类型
                        if (trimmed.StartsWith("\"") && trimmed.EndsWith("\""))
                        {
                            // 字符串
                            paramList.Add(trimmed.Trim('"'));
                        }
                        else if (int.TryParse(trimmed, out int intValue))
                        {
                            // 整数
                            paramList.Add(intValue);
                        }
                        else if (double.TryParse(trimmed, out double doubleValue))
                        {
                            // 浮点数
                            paramList.Add(doubleValue);
                        }
                        else if (bool.TryParse(trimmed, out bool boolValue))
                        {
                            // 布尔值
                            paramList.Add(boolValue);
                        }
                        else
                        {
                            // 默认作为字符串
                            paramList.Add(trimmed);
                        }
                    }
                    
                    parameters = paramList.ToArray();
                }

                return (method, parameters);
            }
            catch
            {
                return (null, Array.Empty<object>());
            }
        }

        /// <summary>
        /// 追加结果到文本框
        /// </summary>
        private void AppendResult(string text, Color color)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => AppendResult(text, color)));
                return;
            }

            txtResult.SelectionStart = txtResult.TextLength;
            txtResult.SelectionLength = 0;
            txtResult.SelectionColor = color;
            txtResult.AppendText(text + Environment.NewLine + Environment.NewLine);
            txtResult.SelectionColor = txtResult.ForeColor;
            txtResult.ScrollToCaret();
        }

        #endregion

        #region 自动重连设置

        private void chkAutoReconnect_CheckedChanged(object sender, EventArgs e)
        {
            _socketClient.AutoReconnect = chkAutoReconnect.Checked;
            _logService.Info("SettingsForm", $"自动重连: {chkAutoReconnect.Checked}");
        }

        private void btnApplyReconnect_Click(object sender, EventArgs e)
        {
            try
            {
                int interval = int.Parse(txtReconnectInterval.Text.Trim());
                if (interval < 1000)
                {
                    UIMessageBox.ShowWarning("重连间隔不能小于1000毫秒！");
                    return;
                }

                _socketClient.StopAutoReconnect();
                _socketClient.StartAutoReconnect(interval);
                _logService.Info("SettingsForm", $"重连间隔已设置为: {interval}ms");
                UIMessageBox.ShowSuccess($"重连间隔已设置为: {interval}ms");
            }
            catch (Exception ex)
            {
                _logService.Error("SettingsForm", "设置重连间隔失败", ex);
                UIMessageBox.ShowError($"设置失败:\n{ex.Message}");
            }
        }

        #endregion

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveSettings();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        
        #region 开发模式功能
        
        /// <summary>
        /// 🔧 开发模式：发送测试消息按钮点击事件
        /// 实现：模拟会员发送消息，走真实的订单流程
        /// </summary>
        private async void BtnRunDevSendCommand_Click(object? sender, EventArgs e)
        {
            try
            {
                // 🔥 检查开发模式（防作弊）
                if (!_configService.GetIsRunModeDev())
                {
                    _logService.Warning("SettingsForm", "⚠️ 非开发模式，无法发送测试消息");
                    UIMessageBox.ShowWarning("请先启用开发模式！");
                    return;
                }
                
                // 检查回调是否可用
                if (_simulateMessageCallback == null)
                {
                    _logService.Warning("SettingsForm", "未设置消息处理回调");
                    UIMessageBox.ShowWarning("此功能需要先绑定群组！\n\n请先在主窗口绑定一个群组，然后在会员表右键菜单中使用\"发送测试消息\"功能。");
                    return;
                }
                
                // 获取当前会员信息
                string currentMember = _configService.GetRunDevCurrentMember();
                if (string.IsNullOrWhiteSpace(currentMember))
                {
                    _logService.Warning("SettingsForm", "未设置当前测试会员");
                    UIMessageBox.ShowWarning("请先在会员表中选择一个测试会员！");
                    return;
                }
                
                // 解析会员 wxid（格式：昵称(wxid)）
                string wxid = ExtractWxidFromMemberInfo(currentMember);
                if (string.IsNullOrWhiteSpace(wxid))
                {
                    _logService.Warning("SettingsForm", $"无法解析会员wxid: {currentMember}");
                    UIMessageBox.ShowWarning($"会员信息格式错误：{currentMember}\n\n期望格式：昵称(wxid)");
                    return;
                }
                
                // 获取要发送的消息
                string message = _configService.GetRunDevSendMessage();
                if (string.IsNullOrWhiteSpace(message))
                {
                    _logService.Warning("SettingsForm", "测试消息内容为空");
                    UIMessageBox.ShowWarning("请输入测试消息内容！");
                    return;
                }
                
                _logService.Info("SettingsForm", $"🔧 开发模式-模拟消息: {currentMember} -> {message}");
                
                // 🔥 调用 VxMain 的模拟消息处理方法（走真实的订单流程）
                var (success, replyMessage, errorMessage) = await _simulateMessageCallback(wxid, message);
                
                if (success)
                {
                    string resultMsg = $"✅ 测试消息已成功处理！\n\n会员：{currentMember}\n消息：{message}\n\n";
                    
                    if (!string.IsNullOrEmpty(replyMessage))
                    {
                        resultMsg += $"系统回复：{replyMessage}\n\n";
                    }
                    
                    resultMsg += "订单已创建，请在订单表中查看。\n开奖后会自动结算。";
                    
                    UIMessageBox.ShowSuccess(resultMsg);
                }
                else
                {
                    string errorMsg = $"测试消息处理失败！\n\n会员：{currentMember}\n消息：{message}\n\n";
                    
                    if (!string.IsNullOrEmpty(errorMessage))
                    {
                        errorMsg += $"原因：{errorMessage}";
                    }
                    
                    UIMessageBox.ShowWarning(errorMsg);
                }
            }
            catch (Exception ex)
            {
                _logService.Error("SettingsForm", $"发送测试消息失败: {ex.Message}", ex);
                UIMessageBox.ShowError($"发送测试消息失败！\n\n{ex.Message}");
            }
        }
        
        /// <summary>
        /// 从会员信息字符串中提取 wxid
        /// 格式：昵称(wxid) 或 wxid
        /// </summary>
        private string ExtractWxidFromMemberInfo(string memberInfo)
        {
            if (string.IsNullOrWhiteSpace(memberInfo))
                return string.Empty;
            
            // 匹配格式：昵称(wxid)
            var match = Regex.Match(memberInfo, @"\(([^)]+)\)");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            
            // 如果没有括号，直接返回（可能就是 wxid）
            return memberInfo.Trim();
        }
        
        #endregion
    }
}

