using Sunny.UI;
using System.Text.RegularExpressions;
using System.Text.Json;
using BaiShengVx3Plus.Contracts;

namespace BaiShengVx3Plus.Views
{
    /// <summary>
    /// 设置窗口
    /// </summary>
    public partial class SettingsForm : UIForm
    {
        private readonly IWeixinSocketClient _socketClient;
        private readonly ILogService _logService;

        public SettingsForm(IWeixinSocketClient socketClient, ILogService logService)
        {
            InitializeComponent();
            _socketClient = socketClient;
            _logService = logService;
            
            // 加载设置
            LoadSettings();
        }

        private void LoadSettings()
        {
            // 从配置文件或默认值加载设置
            txtHost.Text = "127.0.0.1";
            txtPort.Text = "6328";
            txtReconnectInterval.Text = "5000";
            
            // 更新连接状态
            UpdateConnectionStatus();
        }

        private void SaveSettings()
        {
            try
            {
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
    }
}

