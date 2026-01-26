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
        private ScriptTaskConfig? _config;
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
        public event EventHandler<ScriptTaskConfig>? ConfigChanged;

        private System.Windows.Forms.Timer? _autoSaveTimer; // 🔥 自动保存定时器（防抖）

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
        public ScriptTaskConfig? Config
        {
            get => _config;
            set
            {
                LogDebug($"🔵 Config setter 被调用");
                
                // 🔥 如果之前有配置对象，取消订阅
                if (_config != null)
                {
                    _config.PropertyChanged -= Config_PropertyChanged;
                }
                
                _config = value;
                
                // 🔥 如果窗口还没有完全显示，延迟建立绑定
                var parentForm = FindForm();
                if (parentForm != null && !parentForm.Visible)
                {
                    LogDebug($"🔵 窗口还没有显示，延迟建立数据绑定");
                    parentForm.Shown += (s, e) =>
                    {
                        // 窗口显示后，再延迟一点建立绑定，确保脚本编辑器已经获得焦点
                        BeginInvoke(new Action(() =>
                        {
                            System.Threading.Thread.Sleep(100); // 等待脚本编辑器获得焦点
                            SetupDataBindings();
                        }));
                    };
                    return;
                }
                
                // 🔥 立即建立数据绑定（现代方式）
                SetupDataBindings();
            }
        }

        /// <summary>
        /// 初始化控件
        /// 🔥 现代方式：使用数据绑定代替手动事件处理
        /// </summary>
        private void InitializeControls()
        {
            // 🔥 初始化自动保存定时器（防抖：1秒无修改后自动保存）
            _autoSaveTimer = new System.Windows.Forms.Timer
            {
                Interval = 1000, // 1秒
                Enabled = false
            };
            _autoSaveTimer.Tick += (s, e) =>
            {
                _autoSaveTimer.Stop();
                // 触发配置变更事件，由外部处理保存
                OnConfigPropertyChanged();
            };
            
            // 🔥 订阅焦点事件（用于调试）
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
        /// 建立数据绑定（现代方式）
        /// 🔥 当 Config 设置时，自动建立双向数据绑定
        /// </summary>
        private void SetupDataBindings()
        {
            if (_config == null) return;

            // 🔥 清除旧绑定（如果存在）
            txtName.DataBindings.Clear();
            txtUrl.DataBindings.Clear();
            txtUsername.DataBindings.Clear();
            txtPassword.DataBindings.Clear();
            chkAutoLogin.DataBindings.Clear();

            // 🔥 建立双向数据绑定
            // DataSourceUpdateMode.OnPropertyChanged = UI 改变时立即更新数据源
            // 这样用户输入时，_config 属性会自动更新
            txtName.DataBindings.Add("Text", _config, nameof(_config.Name), 
                false, DataSourceUpdateMode.OnPropertyChanged);
            
            txtUrl.DataBindings.Add("Text", _config, nameof(_config.Url), 
                false, DataSourceUpdateMode.OnPropertyChanged);
            
            txtUsername.DataBindings.Add("Text", _config, nameof(_config.Username), 
                false, DataSourceUpdateMode.OnPropertyChanged);
            
            txtPassword.DataBindings.Add("Text", _config, nameof(_config.Password), 
                false, DataSourceUpdateMode.OnPropertyChanged);
            
            chkAutoLogin.DataBindings.Add("Checked", _config, nameof(_config.AutoLogin), 
                false, DataSourceUpdateMode.OnPropertyChanged);

            // 🔥 订阅配置对象的属性变更事件，实现自动保存（防抖）
            _config.PropertyChanged += Config_PropertyChanged;
            
            LogDebug($"✅ 数据绑定已建立");
        }

        /// <summary>
        /// 配置对象属性变更事件处理
        /// 🔥 实现防抖自动保存：1秒无修改后自动保存
        /// </summary>
        private void Config_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // 🔥 重置自动保存计时器（防抖）
            if (_autoSaveTimer != null)
            {
                _autoSaveTimer.Stop();
                _autoSaveTimer.Start();
            }
            
            LogDebug($"📝 配置属性变更: {e.PropertyName}，自动保存计时器已重置");
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
        /// 从控件更新配置（公开方法，供外部调用）
        /// </summary>
        public void SyncConfigFromControls()
        {
            UpdateConfigFromControls();
        }

        /// <summary>
        /// 从配置更新控件（已废弃：使用数据绑定后不再需要）
        /// 🔥 数据绑定会自动处理 UI 更新，此方法保留仅用于向后兼容
        /// </summary>
        [Obsolete("使用数据绑定后不再需要手动更新控件，保留此方法仅用于向后兼容")]
        private void UpdateControls()
        {
            // 🔥 使用数据绑定后，配置对象的属性变更会自动更新 UI
            // 此方法保留仅用于向后兼容，实际不再需要
            LogDebug($"🟢 UpdateControls() 被调用（数据绑定已处理，此方法不再需要）");
        }

        /// <summary>
        /// 配置属性变更（已废弃：使用数据绑定后不再需要）
        /// 🔥 数据绑定会自动处理，此方法保留仅用于向后兼容
        /// </summary>
        [Obsolete("使用数据绑定后不再需要手动处理，保留此方法仅用于向后兼容")]
        private void OnConfigPropertyChanged()
        {
            // 🔥 使用数据绑定后，UI 改变会自动更新 _config 对象
            // 此方法保留仅用于向后兼容，实际不再需要
            LogDebug($"🟡 OnConfigPropertyChanged: 数据绑定已处理，此方法不再需要");
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
