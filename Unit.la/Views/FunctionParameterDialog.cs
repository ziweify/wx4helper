using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Unit.La.Scripting;

namespace Unit.La.Views
{
    /// <summary>
    /// 函数参数输入对话框
    /// 根据函数参数动态生成输入框
    /// </summary>
    public partial class FunctionParameterDialog : Form
    {
        private readonly LuaFunctionParser.FunctionInfo _functionInfo;
        private readonly Dictionary<string, TextBox> _parameterControls = new();
        private readonly Dictionary<string, Label> _parameterLabels = new();

        /// <summary>
        /// 获取用户输入的参数值
        /// </summary>
        public Dictionary<string, string> ParameterValues { get; private set; } = new();

        public FunctionParameterDialog(LuaFunctionParser.FunctionInfo functionInfo)
        {
            _functionInfo = functionInfo ?? throw new ArgumentNullException(nameof(functionInfo));
            InitializeComponent();
            InitializeParameterControls();
        }

        private void InitializeComponent()
        {
            this.Text = $"运行函数: {_functionInfo.Name}";
            this.Size = new Size(500, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;

            // 主面板
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                ColumnCount = 2,
                RowCount = 1
            };

            // 设置列宽
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            // 参数面板（滚动）
            var scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true
            };

            var paramPanel = new TableLayoutPanel
            {
                AutoSize = true,
                ColumnCount = 2,
                Padding = new Padding(5)
            };

            paramPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            paramPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            scrollPanel.Controls.Add(paramPanel);

            // 添加参数控件
            int rowIndex = 0;
            foreach (var param in _functionInfo.Parameters)
            {
                // 参数标签
                var label = new Label
                {
                    Text = $"{param.Name}:",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleRight,
                    Padding = new Padding(0, 5, 10, 5)
                };
                _parameterLabels[param.Name] = label;

                // 参数输入框
                var textBox = new TextBox
                {
                    Dock = DockStyle.Fill,
                    Margin = new Padding(0, 5, 0, 5)
                };

                // 如果有默认值，显示在输入框中
                if (param.HasDefaultValue)
                {
                    textBox.Text = param.DefaultValue;
                    textBox.ForeColor = Color.Gray;
                    textBox.Enter += (s, e) =>
                    {
                        if (textBox.ForeColor == Color.Gray)
                        {
                            textBox.Text = "";
                            textBox.ForeColor = Color.Black;
                        }
                    };
                    textBox.Leave += (s, e) =>
                    {
                        if (string.IsNullOrEmpty(textBox.Text))
                        {
                            textBox.Text = param.DefaultValue;
                            textBox.ForeColor = Color.Gray;
                        }
                    };
                }

                _parameterControls[param.Name] = textBox;

                // 添加到面板
                paramPanel.RowCount = rowIndex + 1;
                paramPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
                paramPanel.Controls.Add(label, 0, rowIndex);
                paramPanel.Controls.Add(textBox, 1, rowIndex);

                rowIndex++;
            }

            // 如果没有参数，显示提示
            if (_functionInfo.Parameters.Count == 0)
            {
                var noParamLabel = new Label
                {
                    Text = "此函数无需参数",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = Color.Gray
                };
                paramPanel.RowCount = 1;
                paramPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                paramPanel.Controls.Add(noParamLabel, 0, 0);
                paramPanel.SetColumnSpan(noParamLabel, 2);
            }

            // 按钮面板
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                Padding = new Padding(10)
            };

            var btnRun = new Button
            {
                Text = "运行",
                DialogResult = DialogResult.OK,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                Size = new Size(80, 30),
                Location = new Point(buttonPanel.Width - 180, 10)
            };

            var btnCancel = new Button
            {
                Text = "取消",
                DialogResult = DialogResult.Cancel,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                Size = new Size(80, 30),
                Location = new Point(buttonPanel.Width - 90, 10)
            };

            buttonPanel.Controls.Add(btnRun);
            buttonPanel.Controls.Add(btnCancel);

            // 函数签名显示
            var signatureLabel = new Label
            {
                Text = $"函数签名: {_functionInfo.FullSignature}",
                Dock = DockStyle.Top,
                Height = 30,
                Padding = new Padding(10, 5, 10, 5),
                BackColor = Color.LightGray,
                Font = new Font("Consolas", 9)
            };

            // 组装
            mainPanel.Controls.Add(scrollPanel, 0, 0);
            mainPanel.SetColumnSpan(scrollPanel, 2);

            this.Controls.Add(signatureLabel);
            this.Controls.Add(mainPanel);
            this.Controls.Add(buttonPanel);

            // 设置 AcceptButton 和 CancelButton
            this.AcceptButton = btnRun;
            this.CancelButton = btnCancel;

            // 处理确定按钮
            btnRun.Click += BtnRun_Click;
        }

        private void InitializeParameterControls()
        {
            // 已经在 InitializeComponent 中完成
        }

        private void BtnRun_Click(object sender, EventArgs e)
        {
            ParameterValues = new Dictionary<string, string>();

            foreach (var kvp in _parameterControls)
            {
                var paramName = kvp.Key;
                var textBox = kvp.Value;
                var value = textBox.Text.Trim();

                // 如果输入框是灰色（使用默认值），使用默认值
                if (textBox.ForeColor == Color.Gray && string.IsNullOrEmpty(value))
                {
                    var param = _functionInfo.Parameters.FirstOrDefault(p => p.Name == paramName);
                    if (param != null && param.HasDefaultValue)
                    {
                        value = param.DefaultValue;
                    }
                }

                ParameterValues[paramName] = value;
            }
        }
    }
}
