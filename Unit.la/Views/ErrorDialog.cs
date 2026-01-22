using System;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace Unit.La.Views
{
    /// <summary>
    /// 友好的错误对话框
    /// 上半部分：用户友好的错误信息
    /// 下半部分：开发者技术细节
    /// 支持一键复制所有信息
    /// </summary>
    public class ErrorDialog : XtraForm
    {
        private readonly string _userMessage;
        private readonly string _technicalDetails;
        private LabelControl _lblTitle;
        private LabelControl _lblUserMessage;
        private MemoEdit _txtTechnicalDetails;
        private SimpleButton _btnCopy;
        private SimpleButton _btnClose;
        private PictureBox _picIcon;

        public ErrorDialog(string userMessage, string technicalDetails, string title = "发生错误")
        {
            _userMessage = userMessage;
            _technicalDetails = technicalDetails;
            
            InitializeComponents();
            this.Text = title;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(800, 600);
            this.MinimumSize = new Size(600, 400);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowIcon = false;
        }

        private void InitializeComponents()
        {
            // 图标
            _picIcon = new PictureBox
            {
                Location = new Point(20, 20),
                Size = new Size(48, 48),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Image = SystemIcons.Error.ToBitmap()
            };

            // 标题
            _lblTitle = new LabelControl
            {
                Location = new Point(80, 25),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(700, 25),
                Appearance =
                {
                    Font = new Font("Microsoft YaHei", 14, FontStyle.Bold),
                    ForeColor = Color.FromArgb(192, 0, 0)
                },
                Text = "❌ 操作失败"
            };

            // 用户友好的错误信息
            _lblUserMessage = new LabelControl
            {
                Location = new Point(80, 60),
                AutoSizeMode = LabelAutoSizeMode.Vertical,
                Size = new Size(700, 0),
                Appearance =
                {
                    Font = new Font("Microsoft YaHei", 10),
                    ForeColor = Color.Black
                },
                Text = _userMessage,
                AllowHtmlString = true
            };

            // 分隔线
            var separator = new LabelControl
            {
                Location = new Point(20, 140),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(760, 1),
                Appearance = { BackColor = Color.LightGray }
            };

            // 技术详情标签
            var lblTechnical = new LabelControl
            {
                Location = new Point(20, 150),
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(760, 20),
                Appearance =
                {
                    Font = new Font("Microsoft YaHei", 9, FontStyle.Bold),
                    ForeColor = Color.Gray
                },
                Text = "📋 技术详情（供开发人员参考）："
            };

            // 技术详情文本框
            _txtTechnicalDetails = new MemoEdit
            {
                Location = new Point(20, 175),
                Size = new Size(760, 330),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Properties =
                {
                    ReadOnly = true,
                    ScrollBars = ScrollBars.Both,
                    WordWrap = false
                }
            };
            _txtTechnicalDetails.Text = _technicalDetails;
            _txtTechnicalDetails.Properties.Appearance.Font = new Font("Consolas", 9);
            _txtTechnicalDetails.Properties.Appearance.BackColor = Color.FromArgb(245, 245, 245);

            // 复制按钮
            _btnCopy = new SimpleButton
            {
                Location = new Point(600, 520),
                Size = new Size(90, 30),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                Text = "📋 复制全部",
                Appearance = { BackColor = Color.FromArgb(0, 122, 204) }
            };
            _btnCopy.Click += BtnCopy_Click;

            // 关闭按钮
            _btnClose = new SimpleButton
            {
                Location = new Point(700, 520),
                Size = new Size(80, 30),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                Text = "关闭",
                DialogResult = DialogResult.OK
            };

            // 添加所有控件
            Controls.Add(_picIcon);
            Controls.Add(_lblTitle);
            Controls.Add(_lblUserMessage);
            Controls.Add(separator);
            Controls.Add(lblTechnical);
            Controls.Add(_txtTechnicalDetails);
            Controls.Add(_btnCopy);
            Controls.Add(_btnClose);

            AcceptButton = _btnClose;
        }

        private void BtnCopy_Click(object? sender, EventArgs e)
        {
            try
            {
                var fullText = $@"=== 错误报告 ===

【用户信息】
{_userMessage}

【技术详情】
{_technicalDetails}

=== 生成时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss} ===";

                Clipboard.SetText(fullText);
                _btnCopy.Text = "✅ 已复制";
                _btnCopy.Appearance.BackColor = Color.FromArgb(0, 192, 0);

                // 2秒后恢复
                var timer = new System.Windows.Forms.Timer { Interval = 2000 };
                timer.Tick += (s, args) =>
                {
                    _btnCopy.Text = "📋 复制全部";
                    _btnCopy.Appearance.BackColor = Color.FromArgb(0, 122, 204);
                    timer.Stop();
                    timer.Dispose();
                };
                timer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"复制失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 显示脚本错误对话框
        /// </summary>
        public static void ShowScriptError(string scriptError, int lineNumber, string fullStackTrace)
        {
            var userMessage = $@"<b>脚本执行失败</b>

• <color=red>错误位置：</color>第 {lineNumber} 行
• <color=red>错误原因：</color>{scriptError}

<b>可能的解决方案：</b>
1. 检查脚本语法是否正确
2. 确认所有对象（如 config, web）已正确初始化
3. 检查是否有变量为 nil（空值）
4. 查看下方技术详情了解更多信息";

            var technicalDetails = $@"=== 脚本执行错误 ===

错误类型: Runtime Error
错误行号: {lineNumber}
错误信息: {scriptError}

=== 完整堆栈跟踪 ===
{fullStackTrace}

=== 时间戳 ===
{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}";

            var dialog = new ErrorDialog(userMessage, technicalDetails, "脚本执行错误");
            dialog.ShowDialog();
        }

        /// <summary>
        /// 显示通用错误对话框
        /// </summary>
        public static void ShowError(string userMessage, Exception exception)
        {
            var technicalDetails = $@"=== 异常信息 ===

异常类型: {exception.GetType().FullName}
异常消息: {exception.Message}

=== 堆栈跟踪 ===
{exception.StackTrace}

=== 内部异常 ===
{(exception.InnerException != null ? exception.InnerException.ToString() : "无")}

=== 时间戳 ===
{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}";

            var dialog = new ErrorDialog(userMessage, technicalDetails);
            dialog.ShowDialog();
        }
    }
}
