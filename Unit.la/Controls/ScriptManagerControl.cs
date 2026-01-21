using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Unit.La.Models;
using Unit.La.Scripting;

namespace Unit.La.Controls
{
    /// <summary>
    /// 脚本管理器控件 - 支持远程/本地模式切换和脚本列表管理
    /// </summary>
    public partial class ScriptManagerControl : UserControl
    {
        private ScriptSourceConfig _sourceConfig = new();
        private List<ScriptInfo> _scripts = new();
        private ScriptInfo? _currentScript;

        // UI 组件
        private Panel panelTop;
        private RadioButton radioLocal;
        private RadioButton radioRemote;
        private Panel panelLocalConfig;
        private Panel panelRemoteConfig;
        private ListBox listScripts;
        private Panel panelButtons;

        // 本地模式组件
        private TextBox txtLocalDirectory;
        private Button btnBrowseLocal;
        private Button btnCreateTemplate;
        private Button btnRefreshLocal;

        // 远程模式组件
        private TextBox txtRemoteUrl;
        private TextBox txtAuthToken;
        private Button btnTestConnection;
        private Button btnLoadRemote;
        private Label lblConnectionStatus;

        // 操作按钮
        private Button btnNewScript;
        private Button btnDeleteScript;
        private Button btnOpenFolder;

        /// <summary>
        /// 当前脚本源配置
        /// </summary>
        [Browsable(false)]
        public ScriptSourceConfig SourceConfig
        {
            get => _sourceConfig;
            set
            {
                _sourceConfig = value ?? new ScriptSourceConfig();
                UpdateUIFromConfig();
            }
        }

        /// <summary>
        /// 当前选中的脚本
        /// </summary>
        [Browsable(false)]
        public ScriptInfo? CurrentScript
        {
            get => _currentScript;
            private set
            {
                _currentScript = value;
                ScriptSelected?.Invoke(this, _currentScript);
            }
        }

        /// <summary>
        /// 所有已加载的脚本
        /// </summary>
        [Browsable(false)]
        public List<ScriptInfo> Scripts => _scripts;

        /// <summary>
        /// 脚本选中事件
        /// </summary>
        public event EventHandler<ScriptInfo?>? ScriptSelected;

        /// <summary>
        /// 脚本列表更新事件
        /// </summary>
        public event EventHandler? ScriptsUpdated;

        /// <summary>
        /// 配置变更事件
        /// </summary>
        public event EventHandler<ScriptSourceConfig>? ConfigChanged;

        public ScriptManagerControl()
        {
            InitializeComponent();
            InitializeCustomUI();
        }

        private void InitializeCustomUI()
        {
            SuspendLayout();

            // 顶部模式选择面板
            panelTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = Color.FromArgb(240, 240, 240)
            };

            radioLocal = new RadioButton
            {
                Text = "📁 本地文件",
                Location = new Point(20, 10),
                AutoSize = true,
                Checked = true,
                Font = new Font("Microsoft YaHei", 10F, FontStyle.Regular)
            };
            radioLocal.CheckedChanged += RadioMode_CheckedChanged;

            radioRemote = new RadioButton
            {
                Text = "🌐 远程URL",
                Location = new Point(150, 10),
                AutoSize = true,
                Font = new Font("Microsoft YaHei", 10F, FontStyle.Regular)
            };
            radioRemote.CheckedChanged += RadioMode_CheckedChanged;

            panelTop.Controls.AddRange(new Control[] { radioLocal, radioRemote });

            // 本地配置面板
            panelLocalConfig = CreateLocalConfigPanel();

            // 远程配置面板
            panelRemoteConfig = CreateRemoteConfigPanel();
            panelRemoteConfig.Visible = false;

            // 脚本列表
            var lblScripts = new Label
            {
                Text = "脚本列表:",
                Dock = DockStyle.Top,
                Height = 25,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(5, 0, 0, 0),
                Font = new Font("Microsoft YaHei", 9F, FontStyle.Bold)
            };

            listScripts = new ListBox
            {
                Dock = DockStyle.Fill,
                DisplayMember = "DisplayName",
                Font = new Font("Consolas", 9F)
            };
            listScripts.SelectedIndexChanged += ListScripts_SelectedIndexChanged;
            listScripts.DoubleClick += ListScripts_DoubleClick;

            // 底部按钮面板
            panelButtons = CreateButtonPanel();

            // 组装布局
            var panelList = new Panel { Dock = DockStyle.Fill };
            panelList.Controls.Add(listScripts);
            panelList.Controls.Add(lblScripts);

            Controls.Add(panelList);
            Controls.Add(panelButtons);
            Controls.Add(panelLocalConfig);
            Controls.Add(panelRemoteConfig);
            Controls.Add(panelTop);

            ResumeLayout(false);
        }

        private Panel CreateLocalConfigPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                Padding = new Padding(10)
            };

            var lblDirectory = new Label
            {
                Text = "本地目录:",
                Location = new Point(10, 15),
                AutoSize = true
            };

            txtLocalDirectory = new TextBox
            {
                Location = new Point(80, 12),
                Width = 350,
                ReadOnly = true
            };

            btnBrowseLocal = new Button
            {
                Text = "浏览...",
                Location = new Point(440, 11),
                Width = 70,
                Height = 25
            };
            btnBrowseLocal.Click += BtnBrowseLocal_Click;

            btnCreateTemplate = new Button
            {
                Text = "创建模板",
                Location = new Point(520, 11),
                Width = 90,
                Height = 25
            };
            btnCreateTemplate.Click += BtnCreateTemplate_Click;

            btnRefreshLocal = new Button
            {
                Text = "🔄 刷新",
                Location = new Point(620, 11),
                Width = 80,
                Height = 25
            };
            btnRefreshLocal.Click += BtnRefreshLocal_Click;

            var lblHint = new Label
            {
                Text = "💡 提示: 选择包含 .lua 脚本文件的文件夹",
                Location = new Point(80, 45),
                AutoSize = true,
                ForeColor = Color.Gray
            };

            panel.Controls.AddRange(new Control[]
            {
                lblDirectory, txtLocalDirectory,
                btnBrowseLocal, btnCreateTemplate, btnRefreshLocal,
                lblHint
            });

            return panel;
        }

        private Panel CreateRemoteConfigPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 110,
                Padding = new Padding(10)
            };

            var lblUrl = new Label
            {
                Text = "远程URL:",
                Location = new Point(10, 15),
                AutoSize = true
            };

            txtRemoteUrl = new TextBox
            {
                Location = new Point(80, 12),
                Width = 430,
                PlaceholderText = "https://api.example.com/scripts"
            };

            var lblToken = new Label
            {
                Text = "认证Token:",
                Location = new Point(10, 45),
                AutoSize = true
            };

            txtAuthToken = new TextBox
            {
                Location = new Point(80, 42),
                Width = 430,
                PlaceholderText = "可选，如需认证请填写",
                PasswordChar = '*'
            };

            btnTestConnection = new Button
            {
                Text = "测试连接",
                Location = new Point(520, 11),
                Width = 90,
                Height = 25
            };
            btnTestConnection.Click += BtnTestConnection_Click;

            btnLoadRemote = new Button
            {
                Text = "🔄 加载脚本",
                Location = new Point(520, 41),
                Width = 90,
                Height = 25
            };
            btnLoadRemote.Click += BtnLoadRemote_Click;

            lblConnectionStatus = new Label
            {
                Location = new Point(80, 75),
                AutoSize = true,
                ForeColor = Color.Gray,
                Text = "📋 JSON格式: {\"脚本a\": \"内容\", \"脚本b\": \"内容\"}"
            };

            panel.Controls.AddRange(new Control[]
            {
                lblUrl, txtRemoteUrl,
                lblToken, txtAuthToken,
                btnTestConnection, btnLoadRemote,
                lblConnectionStatus
            });

            return panel;
        }

        private Panel CreateButtonPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 45,
                Padding = new Padding(10, 5, 10, 5)
            };

            btnNewScript = new Button
            {
                Text = "➕ 新建脚本",
                Location = new Point(10, 8),
                Width = 100,
                Height = 30
            };
            btnNewScript.Click += BtnNewScript_Click;

            btnDeleteScript = new Button
            {
                Text = "🗑 删除",
                Location = new Point(120, 8),
                Width = 80,
                Height = 30,
                Enabled = false
            };
            btnDeleteScript.Click += BtnDeleteScript_Click;

            btnOpenFolder = new Button
            {
                Text = "📂 打开文件夹",
                Location = new Point(210, 8),
                Width = 120,
                Height = 30,
                Enabled = false
            };
            btnOpenFolder.Click += BtnOpenFolder_Click;

            panel.Controls.AddRange(new Control[]
            {
                btnNewScript, btnDeleteScript, btnOpenFolder
            });

            return panel;
        }

        #region 事件处理

        private void RadioMode_CheckedChanged(object? sender, EventArgs e)
        {
            if (radioLocal.Checked)
            {
                _sourceConfig.Mode = ScriptSourceMode.Local;
                panelLocalConfig.Visible = true;
                panelRemoteConfig.Visible = false;
                btnOpenFolder.Enabled = !string.IsNullOrEmpty(_sourceConfig.LocalDirectory);
            }
            else
            {
                _sourceConfig.Mode = ScriptSourceMode.Remote;
                panelLocalConfig.Visible = false;
                panelRemoteConfig.Visible = true;
                btnOpenFolder.Enabled = false;
            }

            ConfigChanged?.Invoke(this, _sourceConfig);
        }

        private void BtnBrowseLocal_Click(object? sender, EventArgs e)
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = "选择包含Lua脚本的文件夹",
                ShowNewFolderButton = true
            };

            if (!string.IsNullOrEmpty(txtLocalDirectory.Text))
            {
                dialog.SelectedPath = txtLocalDirectory.Text;
            }

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                txtLocalDirectory.Text = dialog.SelectedPath;
                _sourceConfig.LocalDirectory = dialog.SelectedPath;
                btnOpenFolder.Enabled = true;

                // 自动加载脚本
                LoadLocalScripts();
                
                ConfigChanged?.Invoke(this, _sourceConfig);
            }
        }

        private void BtnCreateTemplate_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_sourceConfig.LocalDirectory))
            {
                MessageBox.Show("请先选择本地目录", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                LocalScriptLoader.CreateDefaultScripts(_sourceConfig.LocalDirectory);
                MessageBox.Show("模板脚本创建成功！\n\n已创建:\n- main.lua\n- functions.lua\n- README.md",
                    "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);

                LoadLocalScripts();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"创建模板失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnRefreshLocal_Click(object? sender, EventArgs e)
        {
            LoadLocalScripts();
        }

        private async void BtnTestConnection_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtRemoteUrl.Text))
            {
                MessageBox.Show("请输入远程URL", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            _sourceConfig.RemoteUrl = txtRemoteUrl.Text;
            _sourceConfig.RemoteAuthToken = txtAuthToken.Text;

            lblConnectionStatus.Text = "⏳ 连接中...";
            lblConnectionStatus.ForeColor = Color.Blue;
            btnTestConnection.Enabled = false;

            try
            {
                var loader = new RemoteScriptLoader(_sourceConfig);
                var (success, message) = await loader.TestConnectionAsync();

                if (success)
                {
                    lblConnectionStatus.Text = $"✅ {message}";
                    lblConnectionStatus.ForeColor = Color.Green;
                }
                else
                {
                    lblConnectionStatus.Text = $"❌ {message}";
                    lblConnectionStatus.ForeColor = Color.Red;
                }
            }
            catch (Exception ex)
            {
                lblConnectionStatus.Text = $"❌ {ex.Message}";
                lblConnectionStatus.ForeColor = Color.Red;
            }
            finally
            {
                btnTestConnection.Enabled = true;
            }
        }

        private async void BtnLoadRemote_Click(object? sender, EventArgs e)
        {
            await LoadRemoteScripts();
        }

        private void BtnNewScript_Click(object? sender, EventArgs e)
        {
            if (_sourceConfig.Mode == ScriptSourceMode.Remote)
            {
                MessageBox.Show("远程模式不支持新建脚本，请在本地模式下操作", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (string.IsNullOrEmpty(_sourceConfig.LocalDirectory))
            {
                MessageBox.Show("请先选择本地目录", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var dialog = new ScriptNameDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var newScript = new ScriptInfo
                {
                    Name = dialog.ScriptName,
                    DisplayName = dialog.ScriptDisplayName,
                    Type = dialog.ScriptType,
                    Content = GetTemplateContent(dialog.ScriptType),
                    FilePath = System.IO.Path.Combine(_sourceConfig.LocalDirectory, dialog.ScriptName)
                };

                try
                {
                    var loader = new LocalScriptLoader(_sourceConfig);
                    loader.SaveScript(newScript);

                    _scripts.Add(newScript);
                    UpdateScriptList();

                    MessageBox.Show("脚本创建成功", "成功",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"创建脚本失败: {ex.Message}", "错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnDeleteScript_Click(object? sender, EventArgs e)
        {
            if (CurrentScript == null) return;

            if (_sourceConfig.Mode == ScriptSourceMode.Remote)
            {
                MessageBox.Show("远程模式不支持删除脚本", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var result = MessageBox.Show(
                $"确定要删除脚本 \"{CurrentScript.DisplayName}\" 吗？\n\n文件将被永久删除！",
                "确认删除",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    var loader = new LocalScriptLoader(_sourceConfig);
                    loader.DeleteScript(CurrentScript);

                    _scripts.Remove(CurrentScript);
                    UpdateScriptList();

                    MessageBox.Show("脚本删除成功", "成功",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"删除脚本失败: {ex.Message}", "错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnOpenFolder_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_sourceConfig.LocalDirectory))
                return;

            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = _sourceConfig.LocalDirectory,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开文件夹失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ListScripts_SelectedIndexChanged(object? sender, EventArgs e)
        {
            CurrentScript = listScripts.SelectedItem as ScriptInfo;
            btnDeleteScript.Enabled = CurrentScript != null && _sourceConfig.Mode == ScriptSourceMode.Local;
        }

        private void ListScripts_DoubleClick(object? sender, EventArgs e)
        {
            // 双击可以触发编辑或其他操作
            if (CurrentScript != null)
            {
                ScriptSelected?.Invoke(this, CurrentScript);
            }
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 加载本地脚本
        /// </summary>
        public void LoadLocalScripts()
        {
            if (string.IsNullOrEmpty(_sourceConfig.LocalDirectory))
            {
                MessageBox.Show("请先选择本地目录", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                var loader = new LocalScriptLoader(_sourceConfig);
                _scripts = loader.LoadScripts();

                UpdateScriptList();

                MessageBox.Show($"加载成功！共 {_scripts.Count} 个脚本", "成功",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载本地脚本失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 加载远程脚本
        /// </summary>
        public async Task LoadRemoteScripts()
        {
            if (string.IsNullOrEmpty(txtRemoteUrl.Text))
            {
                MessageBox.Show("请输入远程URL", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            _sourceConfig.RemoteUrl = txtRemoteUrl.Text;
            _sourceConfig.RemoteAuthToken = txtAuthToken.Text;

            lblConnectionStatus.Text = "⏳ 加载中...";
            lblConnectionStatus.ForeColor = Color.Blue;
            btnLoadRemote.Enabled = false;

            try
            {
                var loader = new RemoteScriptLoader(_sourceConfig);
                var scriptsDict = await loader.LoadScriptsAsync();
                _scripts = loader.ConvertToScriptInfos(scriptsDict);

                UpdateScriptList();

                lblConnectionStatus.Text = $"✅ 加载成功！共 {_scripts.Count} 个脚本";
                lblConnectionStatus.ForeColor = Color.Green;

                ConfigChanged?.Invoke(this, _sourceConfig);
            }
            catch (Exception ex)
            {
                lblConnectionStatus.Text = $"❌ {ex.Message}";
                lblConnectionStatus.ForeColor = Color.Red;
                MessageBox.Show($"加载远程脚本失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnLoadRemote.Enabled = true;
            }
        }

        /// <summary>
        /// 保存脚本（仅本地模式）
        /// </summary>
        public void SaveScript(ScriptInfo script)
        {
            if (_sourceConfig.Mode != ScriptSourceMode.Local)
            {
                throw new InvalidOperationException("仅本地模式支持保存脚本");
            }

            var loader = new LocalScriptLoader(_sourceConfig);
            loader.SaveScript(script);
        }

        /// <summary>
        /// 获取主脚本
        /// </summary>
        public ScriptInfo? GetMainScript()
        {
            return _scripts.FirstOrDefault(s => s.Type == ScriptType.Main);
        }

        /// <summary>
        /// 获取功能库脚本
        /// </summary>
        public ScriptInfo? GetFunctionsScript()
        {
            return _scripts.FirstOrDefault(s => s.Type == ScriptType.Functions);
        }

        #endregion

        #region 私有方法

        private void UpdateUIFromConfig()
        {
            radioLocal.Checked = _sourceConfig.Mode == ScriptSourceMode.Local;
            radioRemote.Checked = _sourceConfig.Mode == ScriptSourceMode.Remote;

            txtLocalDirectory.Text = _sourceConfig.LocalDirectory;
            txtRemoteUrl.Text = _sourceConfig.RemoteUrl;
            txtAuthToken.Text = _sourceConfig.RemoteAuthToken ?? string.Empty;

            btnOpenFolder.Enabled = !string.IsNullOrEmpty(_sourceConfig.LocalDirectory);
        }

        private void UpdateScriptList()
        {
            listScripts.Items.Clear();

            // 按类型排序：Main > Functions > Test > Custom
            var sortedScripts = _scripts.OrderBy(s => s.Type).ThenBy(s => s.DisplayName);

            foreach (var script in sortedScripts)
            {
                var icon = script.Type switch
                {
                    ScriptType.Main => "🚀",
                    ScriptType.Functions => "📚",
                    ScriptType.Test => "🧪",
                    _ => "📄"
                };

                listScripts.Items.Add(script);
            }

            ScriptsUpdated?.Invoke(this, EventArgs.Empty);
        }

        private string GetTemplateContent(ScriptType type)
        {
            return type switch
            {
                ScriptType.Main => @"-- 主脚本
log('主脚本开始执行')

function main()
    -- 在这里编写主要业务逻辑
    return true
end

main()
",
                ScriptType.Functions => @"-- 功能库
log('功能库加载中...')

function login(username, password)
    log('登录: ' .. username)
    return true
end

log('功能库加载完成')
",
                ScriptType.Test => @"-- 测试脚本
log('测试脚本开始')

-- 在这里编写测试代码

log('测试完成')
",
                _ => @"-- 自定义脚本
log('脚本开始')

-- 在这里编写代码

log('脚本结束')
"
            };
        }

        #endregion
    }

    #region 辅助类

    /// <summary>
    /// 脚本名称输入对话框
    /// </summary>
    public class ScriptNameDialog : Form
    {
        private TextBox txtName;
        private TextBox txtDisplayName;
        private ComboBox comboType;

        public string ScriptName => txtName.Text;
        public string ScriptDisplayName => txtDisplayName.Text;
        public ScriptType ScriptType => (ScriptType)comboType.SelectedIndex;

        public ScriptNameDialog()
        {
            Text = "新建脚本";
            Size = new Size(400, 250);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            // 脚本文件名
            var lblName = new Label
            {
                Text = "脚本文件名:",
                Location = new Point(20, 20),
                AutoSize = true
            };

            txtName = new TextBox
            {
                Location = new Point(120, 18),
                Size = new Size(240, 25),
                PlaceholderText = "例如: my_script.lua"
            };

            // 显示名称
            var lblDisplayName = new Label
            {
                Text = "显示名称:",
                Location = new Point(20, 60),
                AutoSize = true
            };

            txtDisplayName = new TextBox
            {
                Location = new Point(120, 58),
                Size = new Size(240, 25),
                PlaceholderText = "例如: 我的脚本"
            };

            // 脚本类型
            var lblType = new Label
            {
                Text = "脚本类型:",
                Location = new Point(20, 100),
                AutoSize = true
            };

            comboType = new ComboBox
            {
                Location = new Point(120, 98),
                Size = new Size(240, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            comboType.Items.AddRange(new object[] { "主脚本", "功能库", "测试脚本", "自定义" });
            comboType.SelectedIndex = 3; // 默认自定义

            // 按钮
            var btnOK = new Button
            {
                Text = "确定",
                Location = new Point(180, 150),
                Size = new Size(80, 30),
                DialogResult = DialogResult.OK
            };
            btnOK.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    MessageBox.Show("请输入脚本文件名", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    DialogResult = DialogResult.None;
                    return;
                }

                if (!txtName.Text.EndsWith(".lua", StringComparison.OrdinalIgnoreCase))
                {
                    txtName.Text += ".lua";
                }

                if (string.IsNullOrWhiteSpace(txtDisplayName.Text))
                {
                    txtDisplayName.Text = System.IO.Path.GetFileNameWithoutExtension(txtName.Text);
                }
            };

            var btnCancel = new Button
            {
                Text = "取消",
                Location = new Point(280, 150),
                Size = new Size(80, 30),
                DialogResult = DialogResult.Cancel
            };

            Controls.AddRange(new Control[]
            {
                lblName, txtName,
                lblDisplayName, txtDisplayName,
                lblType, comboType,
                btnOK, btnCancel
            });

            AcceptButton = btnOK;
            CancelButton = btnCancel;
        }
    }

    #endregion
}
