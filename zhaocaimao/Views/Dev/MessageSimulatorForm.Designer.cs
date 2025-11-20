namespace zhaocaimao.Views.Dev
{
    partial class MessageSimulatorForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.rtbMessages = new System.Windows.Forms.RichTextBox();
            this.txtInput = new System.Windows.Forms.TextBox();
            this.btnSend = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.cbxQuickCommands = new System.Windows.Forms.ComboBox();
            this.lblQuickCommands = new System.Windows.Forms.Label();
            this.pnlBottom = new System.Windows.Forms.Panel();
            this.pnlMessages = new System.Windows.Forms.Panel();
            this.pnlBottom.SuspendLayout();
            this.pnlMessages.SuspendLayout();
            this.SuspendLayout();
            // 
            // rtbMessages
            // 
            this.rtbMessages.BackColor = System.Drawing.Color.White;
            this.rtbMessages.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtbMessages.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbMessages.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F);
            this.rtbMessages.Location = new System.Drawing.Point(10, 10);
            this.rtbMessages.Name = "rtbMessages";
            this.rtbMessages.ReadOnly = true;
            this.rtbMessages.Size = new System.Drawing.Size(564, 380);
            this.rtbMessages.TabIndex = 0;
            this.rtbMessages.Text = "";
            // 
            // txtInput
            // 
            this.txtInput.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F);
            this.txtInput.Location = new System.Drawing.Point(12, 12);
            this.txtInput.Multiline = true;
            this.txtInput.Name = "txtInput";
            this.txtInput.PlaceholderText = "请输入消息... (按 Enter 发送，Shift+Enter 换行)";
            this.txtInput.Size = new System.Drawing.Size(564, 60);
            this.txtInput.TabIndex = 1;
            this.txtInput.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtInput_KeyDown);
            // 
            // btnSend
            // 
            this.btnSend.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(174)))), ((int)(((byte)(96)))));
            this.btnSend.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSend.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnSend.ForeColor = System.Drawing.Color.White;
            this.btnSend.Location = new System.Drawing.Point(476, 78);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(100, 32);
            this.btnSend.TabIndex = 2;
            this.btnSend.Text = "发送(Enter)";
            this.btnSend.UseVisualStyleBackColor = false;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // btnClear
            // 
            this.btnClear.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(195)))), ((int)(((byte)(199)))));
            this.btnClear.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClear.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            this.btnClear.ForeColor = System.Drawing.Color.White;
            this.btnClear.Location = new System.Drawing.Point(370, 78);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(100, 32);
            this.btnClear.TabIndex = 3;
            this.btnClear.Text = "清空历史";
            this.btnClear.UseVisualStyleBackColor = false;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // cbxQuickCommands
            // 
            this.cbxQuickCommands.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxQuickCommands.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            this.cbxQuickCommands.FormattingEnabled = true;
            this.cbxQuickCommands.Location = new System.Drawing.Point(82, 81);
            this.cbxQuickCommands.Name = "cbxQuickCommands";
            this.cbxQuickCommands.Size = new System.Drawing.Size(282, 28);
            this.cbxQuickCommands.TabIndex = 4;
            this.cbxQuickCommands.SelectedIndexChanged += new System.EventHandler(this.cbxQuickCommands_SelectedIndexChanged);
            // 
            // lblQuickCommands
            // 
            this.lblQuickCommands.AutoSize = true;
            this.lblQuickCommands.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            this.lblQuickCommands.Location = new System.Drawing.Point(12, 85);
            this.lblQuickCommands.Name = "lblQuickCommands";
            this.lblQuickCommands.Size = new System.Drawing.Size(69, 20);
            this.lblQuickCommands.TabIndex = 5;
            this.lblQuickCommands.Text = "快捷命令";
            // 
            // pnlBottom
            // 
            this.pnlBottom.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(242)))), ((int)(((byte)(245)))));
            this.pnlBottom.Controls.Add(this.lblQuickCommands);
            this.pnlBottom.Controls.Add(this.cbxQuickCommands);
            this.pnlBottom.Controls.Add(this.btnClear);
            this.pnlBottom.Controls.Add(this.btnSend);
            this.pnlBottom.Controls.Add(this.txtInput);
            this.pnlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlBottom.Location = new System.Drawing.Point(0, 400);
            this.pnlBottom.Name = "pnlBottom";
            this.pnlBottom.Size = new System.Drawing.Size(584, 121);
            this.pnlBottom.TabIndex = 6;
            // 
            // pnlMessages
            // 
            this.pnlMessages.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
            this.pnlMessages.Controls.Add(this.rtbMessages);
            this.pnlMessages.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMessages.Location = new System.Drawing.Point(0, 0);
            this.pnlMessages.Name = "pnlMessages";
            this.pnlMessages.Padding = new System.Windows.Forms.Padding(10);
            this.pnlMessages.Size = new System.Drawing.Size(584, 400);
            this.pnlMessages.TabIndex = 7;
            // 
            // MessageSimulatorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 521);
            this.Controls.Add(this.pnlMessages);
            this.Controls.Add(this.pnlBottom);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.MinimumSize = new System.Drawing.Size(600, 560);
            this.Name = "MessageSimulatorForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "💬 消息模拟器";
            this.pnlBottom.ResumeLayout(false);
            this.pnlBottom.PerformLayout();
            this.pnlMessages.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox rtbMessages;
        private System.Windows.Forms.TextBox txtInput;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.ComboBox cbxQuickCommands;
        private System.Windows.Forms.Label lblQuickCommands;
        private System.Windows.Forms.Panel pnlBottom;
        private System.Windows.Forms.Panel pnlMessages;
    }
}

