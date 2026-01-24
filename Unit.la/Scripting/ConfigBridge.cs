using System;
using System.Diagnostics;
using System.Windows.Forms;
using MoonSharp.Interpreter;
using Unit.La.Models;
using Unit.La.Controls;

namespace Unit.La.Scripting
{
    /// <summary>
    /// 配置桥接类 - 供 Lua 脚本读写配置
    /// 支持双向绑定：Lua 可以读取和修改配置，修改后自动更新 UI
    /// 使用方式: 
    ///   local username = config.username  -- 读取
    ///   config.username = "newuser"        -- 写入（自动更新 UI）
    /// 🔥 使用 MoonSharpUserData 标记，让 MoonSharp 能够识别和转换此类型
    /// </summary>
    [MoonSharpUserData]
    public class ConfigBridge
    {
        private readonly BrowserTaskConfig _config;
        private readonly BrowserConfigPanel? _configPanel;
        private readonly Action<string>? _logger;

        /// <summary>
        /// 日志输出方法（用于调试）
        /// </summary>
        private void LogDebug(string message)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            Debug.WriteLine($"[ConfigBridge {timestamp}] {message}");
            Console.WriteLine($"[ConfigBridge {timestamp}] {message}");
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="config">配置对象</param>
        /// <param name="configPanel">配置面板（用于更新 UI）</param>
        /// <param name="logger">日志回调（可选）</param>
        public ConfigBridge(BrowserTaskConfig config, BrowserConfigPanel? configPanel = null, Action<string>? logger = null)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _configPanel = configPanel;
            _logger = logger;
        }

        /// <summary>
        /// URL（可读写）
        /// </summary>
        public string url
        {
            get => _config.Url ?? "";
            set
            {
                if (_config.Url != value)
                {
                    _config.Url = value;
                    UpdateUI(nameof(url), value);
                    _logger?.Invoke($"📝 Lua 修改配置: url = {value}");
                }
            }
        }

        /// <summary>
        /// 用户名（可读写）
        /// </summary>
        public string username
        {
            get => _config.Username ?? "";
            set
            {
                if (_config.Username != value)
                {
                    _config.Username = value;
                    UpdateUI(nameof(username), value);
                    _logger?.Invoke($"📝 Lua 修改配置: username = {value}");
                }
            }
        }

        /// <summary>
        /// 密码（可读写）
        /// </summary>
        public string password
        {
            get => _config.Password ?? "";
            set
            {
                if (_config.Password != value)
                {
                    _config.Password = value;
                    UpdateUI(nameof(password), value);
                    _logger?.Invoke($"📝 Lua 修改配置: password = ***");
                }
            }
        }

        /// <summary>
        /// 是否自动登录（可读写）
        /// </summary>
        public bool autoLogin
        {
            get => _config.AutoLogin;
            set
            {
                if (_config.AutoLogin != value)
                {
                    _config.AutoLogin = value;
                    UpdateUI(nameof(autoLogin), value);
                    _logger?.Invoke($"📝 Lua 修改配置: autoLogin = {value}");
                }
            }
        }

        /// <summary>
        /// 任务名称（可读写）
        /// </summary>
        public string name
        {
            get => _config.Name ?? "";
            set
            {
                if (_config.Name != value)
                {
                    _config.Name = value;
                    UpdateUI(nameof(name), value);
                    _logger?.Invoke($"📝 Lua 修改配置: name = {value}");
                }
            }
        }

        /// <summary>
        /// 更新 UI（在 UI 线程中执行）
        /// </summary>
        private void UpdateUI(string propertyName, object value)
        {
            if (_configPanel == null) return;

            // 🔥 确保在 UI 线程中更新
            if (_configPanel.InvokeRequired)
            {
                _configPanel.Invoke(new Action(() => UpdateUIInternal(propertyName, value)));
            }
            else
            {
                UpdateUIInternal(propertyName, value);
            }
        }

        /// <summary>
        /// 内部更新 UI 方法（假定已在 UI 线程）
        /// 🔥 最简单的方法：如果任何输入控件有焦点，完全不更新，不设置 Text 属性
        /// 🔥 不管理光标，不设置光标，让界面自己管理
        /// </summary>
        private void UpdateUIInternal(string propertyName, object value)
        {
            LogDebug($"🔴 UpdateUIInternal() 被调用: propertyName={propertyName}, value={value}");
            
            if (_configPanel == null)
            {
                LogDebug($"🔴 _configPanel 为 null，返回");
                return;
            }

            // 🔥 检查焦点状态
            var nameFocused = _configPanel.txtName.Focused;
            var urlFocused = _configPanel.txtUrl.Focused;
            var usernameFocused = _configPanel.txtUsername.Focused;
            var passwordFocused = _configPanel.txtPassword.Focused;
            
            LogDebug($"🔴 焦点检查: Name={nameFocused}, Url={urlFocused}, Username={usernameFocused}, Password={passwordFocused}");

            // 🔥 如果任何输入控件有焦点，完全不更新，不设置 Text 属性
            // 这是最严格的检查，确保用户操作时不会更新控件
            if (nameFocused || urlFocused || usernameFocused || passwordFocused)
            {
                LogDebug($"🔴 有控件有焦点，跳过更新");
                return; // 不更新，不设置 Text，让界面自己管理光标
            }

            try
            {
                // 🔥 只有在没有任何控件有焦点时，才更新控件
                // 🔥 不管理光标，不设置光标，只设置 Text 属性
                switch (propertyName)
                {
                    case nameof(url):
                        if (_configPanel.txtUrl.Text != (string)value)
                        {
                            LogDebug($"🔴 更新 txtUrl.Text: '{_configPanel.txtUrl.Text}' -> '{(string)value}'");
                            _configPanel.txtUrl.Text = (string)value;
                        }
                        break;

                    case nameof(username):
                        if (_configPanel.txtUsername.Text != (string)value)
                        {
                            LogDebug($"🔴 更新 txtUsername.Text: '{_configPanel.txtUsername.Text}' -> '{(string)value}' (更新前 SelectionStart={_configPanel.txtUsername.SelectionStart})");
                            _configPanel.txtUsername.Text = (string)value;
                            LogDebug($"🔴 更新 txtUsername.Text (更新后 SelectionStart={_configPanel.txtUsername.SelectionStart})");
                        }
                        break;

                    case nameof(password):
                        if (_configPanel.txtPassword.Text != (string)value)
                        {
                            LogDebug($"🔴 更新 txtPassword.Text: '{_configPanel.txtPassword.Text}' -> '***' (更新前 SelectionStart={_configPanel.txtPassword.SelectionStart})");
                            _configPanel.txtPassword.Text = (string)value;
                            LogDebug($"🔴 更新 txtPassword.Text (更新后 SelectionStart={_configPanel.txtPassword.SelectionStart})");
                        }
                        break;

                    case nameof(autoLogin):
                        LogDebug($"🔴 更新 chkAutoLogin.Checked: {_configPanel.chkAutoLogin.Checked} -> {(bool)value}");
                        _configPanel.chkAutoLogin.Checked = (bool)value;
                        break;

                    case nameof(name):
                        if (_configPanel.txtName.Text != (string)value)
                        {
                            LogDebug($"🔴 更新 txtName.Text: '{_configPanel.txtName.Text}' -> '{(string)value}'");
                            _configPanel.txtName.Text = (string)value;
                        }
                        break;
                }
                
                LogDebug($"🔴 UpdateUIInternal() 完成");
            }
            catch (Exception ex)
            {
                LogDebug($"🔴 UpdateUIInternal() 异常: {ex.Message}");
                _logger?.Invoke($"❌ 更新 UI 失败: {ex.Message}");
            }
        }
    }
}
