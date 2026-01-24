using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Unit.La.Models;

namespace Unit.La.Controls
{
    /// <summary>
    /// 自定义 TextBox，用于追踪所有 Text 属性的设置
    /// </summary>
    internal class TraceableTextBox : TextBox
    {
        public string TraceName { get; set; } = "";

        public override string Text
        {
            get => base.Text;
            set
            {
                var oldValue = base.Text;
                var oldSelectionStart = SelectionStart;
                var oldFocused = Focused;
                var stackTrace = new System.Diagnostics.StackTrace(2, true); // 跳过当前方法和调用者
                var caller = stackTrace.GetFrame(0);
                var callerMethod = caller?.GetMethod()?.Name ?? "Unknown";
                var callerFile = caller?.GetFileName() ?? "Unknown";
                var callerLine = caller?.GetFileLineNumber() ?? 0;
                
                var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                var logMsg = $"[TraceableTextBox {timestamp}] {TraceName}.Text 被设置: '{oldValue}' -> '{value}' (SelectionStart={oldSelectionStart}, Focused={oldFocused}, 调用者={callerMethod}@{callerFile}:{callerLine})";
                Debug.WriteLine(logMsg);
                Console.WriteLine(logMsg);
                
                base.Text = value;
                
                var newSelectionStart = SelectionStart;
                var newFocused = Focused;
                if (oldSelectionStart != newSelectionStart || oldFocused != newFocused)
                {
                    var logMsg2 = $"[TraceableTextBox {timestamp}] {TraceName}.Text 设置后: SelectionStart={newSelectionStart} (之前={oldSelectionStart}), Focused={newFocused} (之前={oldFocused})";
                    Debug.WriteLine(logMsg2);
                    Console.WriteLine(logMsg2);
                }
            }
        }
    }

    /// <summary>
    /// 浏览器任务配置面板
    /// 通用的配置界面，可在任何项目中使用
    /// </summary>
    public partial class BrowserConfigPanel : UserControl
    {
        private BrowserTaskConfig? _config;
        private bool _isUpdatingFromConfig = false; // 标记是否正在从配置更新控件

        /// <summary>
        /// 日志输出方法（用于调试）
        /// </summary>
        private void LogDebug(string message)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            Debug.WriteLine($"[BrowserConfigPanel {timestamp}] {message}");
            Console.WriteLine($"[BrowserConfigPanel {timestamp}] {message}");
        }

        /// <summary>
        /// 配置变更事件
        /// </summary>
        public event EventHandler<BrowserTaskConfig>? ConfigChanged;

        public BrowserConfigPanel()
        {
            InitializeComponent();
            InitializeControls();
            
            // 🔥 订阅 Load 事件，确保在控件完全加载后初始化
            Load += BrowserConfigPanel_Load;
        }

        /// <summary>
        /// 控件加载完成事件
        /// 🔥 确保所有控件都正确初始化，避免光标跳转问题
        /// 通过让脚本编辑器获得一次焦点来"激活"全局状态
        /// </summary>
        private void BrowserConfigPanel_Load(object? sender, EventArgs e)
        {
            // 🔥 使用 BeginInvoke 延迟执行，确保窗口完全显示后再触发
            if (IsHandleCreated)
            {
                BeginInvoke(new Action(() =>
                {
                    // 如果脚本编辑器存在，让它获得一次焦点，这样可以修复全局状态
                    var parentForm = FindForm();
                    if (parentForm != null)
                    {
                        var scriptEditor = FindScriptEditorControl(parentForm);
                        if (scriptEditor != null && scriptEditor.CanFocus)
                        {
                            // 临时让脚本编辑器获得焦点，修复全局状态
                            scriptEditor.Focus();
                            Application.DoEvents();
                            // 不切换回来，让用户自然操作
                            // 这样所有 TextBox 控件都会正常工作
                        }
                    }
                }));
            }
        }

        /// <summary>
        /// 查找脚本编辑器控件
        /// </summary>
        private Control? FindScriptEditorControl(Control parent)
        {
            foreach (Control control in parent.Controls)
            {
                // 检查类型名称，避免直接引用（可能不在同一个程序集中）
                if (control.GetType().Name == "ScriptEditorControl")
                {
                    return control;
                }
                
                var found = FindScriptEditorControl(control);
                if (found != null)
                {
                    return found;
                }
            }
            return null;
        }

        /// <summary>
        /// 获取或设置配置
        /// 🔥 如果任何输入控件有焦点，不更新控件，避免光标跳转
        /// 🔥 如果窗口还没有完全显示，延迟更新
        /// </summary>
        public BrowserTaskConfig? Config
        {
            get => _config;
            set
            {
                LogDebug($"🔵 Config setter 被调用");
                _config = value;
                
                // 🔥 如果窗口还没有完全显示，延迟更新
                var parentForm = FindForm();
                if (parentForm != null && !parentForm.Visible)
                {
                    LogDebug($"🔵 窗口还没有显示，延迟更新");
                    parentForm.Shown += (s, e) =>
                    {
                        // 窗口显示后，再延迟一点更新，确保脚本编辑器已经获得焦点
                        BeginInvoke(new Action(() =>
                        {
                            System.Threading.Thread.Sleep(100); // 等待脚本编辑器获得焦点
                            UpdateControls();
                        }));
                    };
                    return;
                }
                
                // 🔥 检查焦点状态
                var nameFocused = txtName.Focused;
                var urlFocused = txtUrl.Focused;
                var usernameFocused = txtUsername.Focused;
                var passwordFocused = txtPassword.Focused;
                
                LogDebug($"🔵 焦点检查: Name={nameFocused}, Url={urlFocused}, Username={usernameFocused}, Password={passwordFocused}");
                
                // 🔥 如果任何输入控件有焦点，不更新控件，避免光标跳转
                if (nameFocused || urlFocused || usernameFocused || passwordFocused)
                {
                    LogDebug($"🔵 有控件有焦点，跳过 UpdateControls()");
                    return; // 不更新，让界面自己管理光标
                }
                
                LogDebug($"🔵 没有控件有焦点，调用 UpdateControls()");
                UpdateControls();
            }
        }

        /// <summary>
        /// 初始化控件
        /// </summary>
        private void InitializeControls()
        {
            // 订阅控件变更事件
            txtName.TextChanged += (s, e) => 
            {
                LogDebug($"📝 txtName.TextChanged: Text='{txtName.Text}', SelectionStart={txtName.SelectionStart}, Focused={txtName.Focused}");
                OnConfigPropertyChanged();
            };
            txtUrl.TextChanged += (s, e) => 
            {
                LogDebug($"📝 txtUrl.TextChanged: Text='{txtUrl.Text}', SelectionStart={txtUrl.SelectionStart}, Focused={txtUrl.Focused}");
                OnConfigPropertyChanged();
            };
            txtUsername.TextChanged += (s, e) => 
            {
                LogDebug($"📝 txtUsername.TextChanged: Text='{txtUsername.Text}', SelectionStart={txtUsername.SelectionStart}, Focused={txtUsername.Focused}");
                OnConfigPropertyChanged();
            };
            txtPassword.TextChanged += (s, e) => 
            {
                LogDebug($"📝 txtPassword.TextChanged: Text='{txtPassword.Text}', SelectionStart={txtPassword.SelectionStart}, Focused={txtPassword.Focused}");
                OnConfigPropertyChanged();
            };
            chkAutoLogin.CheckedChanged += (s, e) => OnConfigPropertyChanged();
            
            // 订阅焦点事件
            txtName.GotFocus += (s, e) => LogDebug($"👁️ txtName.GotFocus: SelectionStart={txtName.SelectionStart}");
            txtName.LostFocus += (s, e) => LogDebug($"👁️ txtName.LostFocus");
            txtUrl.GotFocus += (s, e) => LogDebug($"👁️ txtUrl.GotFocus: SelectionStart={txtUrl.SelectionStart}");
            txtUrl.LostFocus += (s, e) => LogDebug($"👁️ txtUrl.LostFocus");
            txtUsername.GotFocus += (s, e) => LogDebug($"👁️ txtUsername.GotFocus: SelectionStart={txtUsername.SelectionStart}");
            txtUsername.LostFocus += (s, e) => LogDebug($"👁️ txtUsername.LostFocus");
            txtPassword.GotFocus += (s, e) => LogDebug($"👁️ txtPassword.GotFocus: SelectionStart={txtPassword.SelectionStart}");
            txtPassword.LostFocus += (s, e) => LogDebug($"👁️ txtPassword.LostFocus");
        }

        /// <summary>
        /// 从控件更新配置
        /// </summary>
        private void UpdateConfigFromControls()
        {
            if (_config == null) return;

            _config.Name = txtName.Text;
            _config.Url = txtUrl.Text;
            _config.Username = txtUsername.Text;
            _config.Password = txtPassword.Text;
            _config.AutoLogin = chkAutoLogin.Checked;
        }

        /// <summary>
        /// 从配置更新控件
        /// 🔥 最简单的方法：如果任何输入控件有焦点，完全不更新，不设置 Text 属性
        /// 🔥 不管理光标，不设置光标，让界面自己管理
        /// </summary>
        private void UpdateControls()
        {
            LogDebug($"🟢 UpdateControls() 被调用");
            
            if (_config == null)
            {
                LogDebug($"🟢 _config 为 null，返回");
                return;
            }

            // 🔥 检查焦点状态
            var nameFocused = txtName.Focused;
            var urlFocused = txtUrl.Focused;
            var usernameFocused = txtUsername.Focused;
            var passwordFocused = txtPassword.Focused;
            
            LogDebug($"🟢 焦点检查: Name={nameFocused}, Url={urlFocused}, Username={usernameFocused}, Password={passwordFocused}");

            // 🔥 如果任何输入控件有焦点，完全不更新，不设置 Text 属性
            // 这是最严格的检查，确保用户操作时不会更新控件
            if (nameFocused || urlFocused || usernameFocused || passwordFocused)
            {
                LogDebug($"🟢 有控件有焦点，跳过更新");
                return; // 不更新，不设置 Text，让界面自己管理光标
            }

            _isUpdatingFromConfig = true; // 防止触发 ConfigChanged 事件
            try
            {
                // 🔥 只有在没有任何控件有焦点时，才更新控件
                // 🔥 不管理光标，不设置光标，只设置 Text 属性
                var configName = _config.Name ?? "";
                var configUrl = _config.Url ?? "";
                var configUsername = _config.Username ?? "";
                var configPassword = _config.Password ?? "";

                LogDebug($"🟢 准备更新控件:");
                LogDebug($"  - Name: '{txtName.Text}' -> '{configName}'");
                LogDebug($"  - Url: '{txtUrl.Text}' -> '{configUrl}'");
                LogDebug($"  - Username: '{txtUsername.Text}' -> '{configUsername}' (SelectionStart={txtUsername.SelectionStart})");
                LogDebug($"  - Password: '{txtPassword.Text}' -> '{configPassword}' (SelectionStart={txtPassword.SelectionStart})");

                // 🔥 只有在文本不同时才设置，避免不必要的更新
                if (txtName.Text != configName)
                {
                    LogDebug($"🟢 更新 txtName.Text");
                    txtName.Text = configName;
                }
                        
                if (txtUrl.Text != configUrl)
                {
                    LogDebug($"🟢 更新 txtUrl.Text");
                    txtUrl.Text = configUrl;
                }
                        
                if (txtUsername.Text != configUsername)
                {
                    LogDebug($"🟢 更新 txtUsername.Text (更新前 SelectionStart={txtUsername.SelectionStart})");
                    txtUsername.Text = configUsername;
                    LogDebug($"🟢 更新 txtUsername.Text (更新后 SelectionStart={txtUsername.SelectionStart})");
                }
                    
                if (txtPassword.Text != configPassword)
                {
                    LogDebug($"🟢 更新 txtPassword.Text (更新前 SelectionStart={txtPassword.SelectionStart})");
                    txtPassword.Text = configPassword;
                    LogDebug($"🟢 更新 txtPassword.Text (更新后 SelectionStart={txtPassword.SelectionStart})");
                }
                    
                chkAutoLogin.Checked = _config.AutoLogin;
                
                LogDebug($"🟢 UpdateControls() 完成");
            }
            finally
            {
                _isUpdatingFromConfig = false;
            }
        }

        /// <summary>
        /// 配置属性变更
        /// </summary>
        private void OnConfigPropertyChanged()
        {
            // 如果正在从配置更新控件，不触发事件（避免循环）
            if (_isUpdatingFromConfig)
            {
                LogDebug($"🟡 OnConfigPropertyChanged: _isUpdatingFromConfig=true，跳过");
                return;
            }
            
            LogDebug($"🟡 OnConfigPropertyChanged: 调用 UpdateConfigFromControls()");
            UpdateConfigFromControls();
            // 注释掉自动触发事件，改为只在用户点击"保存"时触发
            // ConfigChanged?.Invoke(this, _config!);
        }

        /// <summary>
        /// 验证配置
        /// </summary>
        public bool ValidateConfig(out string errorMessage)
        {
            errorMessage = "";

            if (_config == null)
            {
                errorMessage = "配置对象为空";
                return false;
            }

            // 🔧 重要：在验证之前，确保从控件更新到配置对象
            UpdateConfigFromControls();

            if (string.IsNullOrWhiteSpace(_config.Url))
            {
                errorMessage = "URL 不能为空";
                return false;
            }

            if (!Uri.IsWellFormedUriString(_config.Url, UriKind.Absolute))
            {
                errorMessage = "URL 格式不正确";
                return false;
            }

            if (_config.AutoLogin)
            {
                if (string.IsNullOrWhiteSpace(_config.Username))
                {
                    errorMessage = "启用自动登录时，用户名不能为空";
                    return false;
                }

                if (string.IsNullOrWhiteSpace(_config.Password))
                {
                    errorMessage = "启用自动登录时，密码不能为空";
                    return false;
                }
            }

            return true;
        }
    }
}
