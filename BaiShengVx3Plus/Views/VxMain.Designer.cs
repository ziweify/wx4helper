using BaiShengVx3Plus.Models;

namespace BaiShengVx3Plus
{
    partial class VxMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            DataGridViewCellStyle dataGridViewCellStyle25 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle26 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle27 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle28 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle29 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle30 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle31 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle32 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle33 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle34 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle35 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle36 = new DataGridViewCellStyle();
            cmsMembers = new Sunny.UI.UIContextMenuStrip();
            splitContainerMain = new Sunny.UI.UISplitContainer();
            pnlLeft = new Sunny.UI.UIPanel();
            dgvContacts = new Sunny.UI.UIDataGridView();
            pnlLeftTop = new Sunny.UI.UIPanel();
            btnRefreshContacts = new Sunny.UI.UIButton();
            btnBindingContacts = new Sunny.UI.UIButton();
            txtCurrentContact = new Sunny.UI.UITextBox();
            pnlRight = new Sunny.UI.UIPanel();
            splitContainerRight = new Sunny.UI.UISplitContainer();
            pnlMembers = new Sunny.UI.UIPanel();
            dgvMembers = new Sunny.UI.UIDataGridView();
            pnlMembersTop = new Sunny.UI.UIPanel();
            lblMemberInfo = new Sunny.UI.UILabel();
            pnlOrders = new Sunny.UI.UIPanel();
            dgvOrders = new Sunny.UI.UIDataGridView();
            pnlOrdersTop = new Sunny.UI.UIPanel();
            lblOrderInfo = new Sunny.UI.UILabel();
            pnlTopButtons = new Sunny.UI.UIPanel();
            btnClearData = new Sunny.UI.UIButton();
            btnOpenLotteryResult = new Sunny.UI.UIButton();
            btnConnect = new Sunny.UI.UIButton();
            btnLog = new Sunny.UI.UIButton();
            btnSettings = new Sunny.UI.UIButton();
            statusStrip = new StatusStrip();
            lblStatus = new ToolStripStatusLabel();
            ucUserInfo1 = new BaiShengVx3Plus.Views.UcUserInfo();
            (splitContainerMain).BeginInit();
            splitContainerMain.Panel1.SuspendLayout();
            splitContainerMain.Panel2.SuspendLayout();
            splitContainerMain.SuspendLayout();
            pnlLeft.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvContacts).BeginInit();
            pnlLeftTop.SuspendLayout();
            pnlRight.SuspendLayout();
            (splitContainerRight).BeginInit();
            splitContainerRight.Panel1.SuspendLayout();
            splitContainerRight.Panel2.SuspendLayout();
            splitContainerRight.SuspendLayout();
            pnlMembers.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvMembers).BeginInit();
            pnlMembersTop.SuspendLayout();
            pnlOrders.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvOrders).BeginInit();
            pnlOrdersTop.SuspendLayout();
            pnlTopButtons.SuspendLayout();
            statusStrip.SuspendLayout();
            SuspendLayout();
            // 
            // cmsMembers
            // 
            cmsMembers.BackColor = Color.FromArgb(243, 249, 255);
            cmsMembers.Font = new Font("微软雅黑", 10F);
            cmsMembers.ImageScalingSize = new Size(20, 20);
            cmsMembers.Items.AddRange(new ToolStripItem[] {
                new ToolStripMenuItem("🔄 清零", null, OnMenuClearBalance_Click) { Name = "menuClearBalance" },
                new ToolStripMenuItem("🗑️ 删除", null, OnMenuDeleteMember_Click) { Name = "menuDeleteMember" },
                new ToolStripSeparator(),
                new ToolStripMenuItem("👔 设置角色", null, new ToolStripItem[] {
                    new ToolStripMenuItem("👑 管理", null, OnMenuSetRole_Click) { Name = "menuRoleAdmin", Tag = MemberState.管理 },
                    new ToolStripMenuItem("🤖 托", null, OnMenuSetRole_Click) { Name = "menuRoleTrust", Tag = MemberState.托 },
                    new ToolStripMenuItem("👤 普会", null, OnMenuSetRole_Click) { Name = "menuRoleNormal", Tag = MemberState.普会 },
                    new ToolStripMenuItem("💎 蓝会", null, OnMenuSetRole_Click) { Name = "menuRoleBlue", Tag = MemberState.蓝会 },
                    new ToolStripMenuItem("💜 紫会", null, OnMenuSetRole_Click) { Name = "menuRolePurple", Tag = MemberState.紫会 }
                }) { Name = "menuSetRole" }
            });
            cmsMembers.Name = "cmsMembers";
            cmsMembers.Size = new Size(181, 148);
            cmsMembers.ZoomScaleRect = new Rectangle(0, 0, 0, 0);
            // 
            // splitContainerMain
            // 
            splitContainerMain.Cursor = Cursors.VSplit;
            splitContainerMain.Dock = DockStyle.Fill;
            splitContainerMain.Location = new Point(0, 95);
            splitContainerMain.MinimumSize = new Size(20, 20);
            splitContainerMain.Name = "splitContainerMain";
            // 
            // splitContainerMain.Panel1
            // 
            splitContainerMain.Panel1.Controls.Add(pnlLeft);
            // 
            // splitContainerMain.Panel2
            // 
            splitContainerMain.Panel2.Controls.Add(pnlRight);
            splitContainerMain.Size = new Size(1200, 668);
            splitContainerMain.SplitterDistance = 244;
            splitContainerMain.SplitterWidth = 5;
            splitContainerMain.TabIndex = 0;
            // 
            // pnlLeft
            // 
            pnlLeft.Controls.Add(dgvContacts);
            pnlLeft.Controls.Add(pnlLeftTop);
            pnlLeft.Dock = DockStyle.Fill;
            pnlLeft.Font = new Font("微软雅黑", 12F);
            pnlLeft.Location = new Point(0, 0);
            pnlLeft.Margin = new Padding(4, 5, 4, 5);
            pnlLeft.MinimumSize = new Size(1, 1);
            pnlLeft.Name = "pnlLeft";
            pnlLeft.Size = new Size(244, 668);
            pnlLeft.TabIndex = 0;
            pnlLeft.Text = null;
            pnlLeft.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // dgvContacts
            // 
            dgvContacts.AllowUserToAddRows = false;
            dgvContacts.AllowUserToDeleteRows = false;
            dgvContacts.AllowUserToResizeRows = false;
            dataGridViewCellStyle25.BackColor = Color.FromArgb(235, 243, 255);
            dgvContacts.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle25;
            dgvContacts.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvContacts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvContacts.BackgroundColor = Color.White;
            dgvContacts.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle26.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle26.BackColor = Color.FromArgb(80, 160, 255);
            dataGridViewCellStyle26.Font = new Font("微软雅黑", 12F);
            dataGridViewCellStyle26.ForeColor = Color.White;
            dataGridViewCellStyle26.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle26.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle26.WrapMode = DataGridViewTriState.True;
            dgvContacts.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle26;
            dgvContacts.ColumnHeadersHeight = 32;
            dgvContacts.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvContacts.EnableHeadersVisualStyles = false;
            dgvContacts.Font = new Font("微软雅黑", 12F);
            dgvContacts.GridColor = Color.FromArgb(80, 160, 255);
            dgvContacts.Location = new Point(3, 43);
            dgvContacts.MultiSelect = false;
            dgvContacts.Name = "dgvContacts";
            dgvContacts.ReadOnly = true;
            dataGridViewCellStyle27.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle27.BackColor = Color.FromArgb(235, 243, 255);
            dataGridViewCellStyle27.Font = new Font("微软雅黑", 12F);
            dataGridViewCellStyle27.ForeColor = Color.FromArgb(48, 48, 48);
            dataGridViewCellStyle27.SelectionBackColor = Color.FromArgb(80, 160, 255);
            dataGridViewCellStyle27.SelectionForeColor = Color.White;
            dataGridViewCellStyle27.WrapMode = DataGridViewTriState.True;
            dgvContacts.RowHeadersDefaultCellStyle = dataGridViewCellStyle27;
            dgvContacts.RowHeadersVisible = false;
            dgvContacts.RowHeadersWidth = 51;
            dataGridViewCellStyle28.BackColor = Color.White;
            dataGridViewCellStyle28.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            dgvContacts.RowsDefaultCellStyle = dataGridViewCellStyle28;
            dgvContacts.RowTemplate.Height = 29;
            dgvContacts.SelectedIndex = -1;
            dgvContacts.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvContacts.Size = new Size(244, 200);
            dgvContacts.StripeOddColor = Color.FromArgb(235, 243, 255);
            dgvContacts.TabIndex = 1;
            dgvContacts.SelectionChanged += dgvContacts_SelectionChanged;
            // 
            // pnlLeftTop
            // 
            pnlLeftTop.Controls.Add(btnRefreshContacts);
            pnlLeftTop.Controls.Add(btnBindingContacts);
            pnlLeftTop.Controls.Add(txtCurrentContact);
            pnlLeftTop.Dock = DockStyle.Top;
            pnlLeftTop.Font = new Font("微软雅黑", 12F);
            pnlLeftTop.Location = new Point(0, 0);
            pnlLeftTop.Margin = new Padding(4, 5, 4, 5);
            pnlLeftTop.MinimumSize = new Size(1, 1);
            pnlLeftTop.Name = "pnlLeftTop";
            pnlLeftTop.Size = new Size(244, 40);
            pnlLeftTop.TabIndex = 0;
            pnlLeftTop.Text = null;
            pnlLeftTop.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // btnRefreshContacts
            // 
            btnRefreshContacts.Cursor = Cursors.Hand;
            btnRefreshContacts.Dock = DockStyle.Right;
            btnRefreshContacts.Font = new Font("微软雅黑", 9F);
            btnRefreshContacts.Location = new Point(169, 0);
            btnRefreshContacts.MinimumSize = new Size(1, 1);
            btnRefreshContacts.Name = "btnRefreshContacts";
            btnRefreshContacts.Size = new Size(39, 40);
            btnRefreshContacts.TabIndex = 2;
            btnRefreshContacts.Text = "刷新";
            btnRefreshContacts.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnRefreshContacts.Click += btnRefreshContacts_Click;
            // 
            // btnBindingContacts
            // 
            btnBindingContacts.Cursor = Cursors.Hand;
            btnBindingContacts.Dock = DockStyle.Right;
            btnBindingContacts.Font = new Font("微软雅黑", 9F);
            btnBindingContacts.Location = new Point(208, 0);
            btnBindingContacts.MinimumSize = new Size(1, 1);
            btnBindingContacts.Name = "btnBindingContacts";
            btnBindingContacts.Size = new Size(36, 40);
            btnBindingContacts.TabIndex = 1;
            btnBindingContacts.Text = "绑定";
            btnBindingContacts.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnBindingContacts.Click += btnBindingContacts_Click;
            // 
            // txtCurrentContact
            // 
            txtCurrentContact.Cursor = Cursors.IBeam;
            txtCurrentContact.Dock = DockStyle.Fill;
            txtCurrentContact.FillColor = Color.FromArgb(243, 249, 255);
            txtCurrentContact.Font = new Font("微软雅黑", 10F);
            txtCurrentContact.Location = new Point(0, 0);
            txtCurrentContact.Margin = new Padding(4, 5, 4, 5);
            txtCurrentContact.MinimumSize = new Size(1, 16);
            txtCurrentContact.Name = "txtCurrentContact";
            txtCurrentContact.Padding = new Padding(5);
            txtCurrentContact.ReadOnly = true;
            txtCurrentContact.ShowText = false;
            txtCurrentContact.Size = new Size(244, 40);
            txtCurrentContact.TabIndex = 0;
            txtCurrentContact.Text = "未绑定联系人";
            txtCurrentContact.TextAlignment = ContentAlignment.MiddleLeft;
            txtCurrentContact.Watermark = "点击绑定按钮选择联系人";
            // 
            // pnlRight
            // 
            pnlRight.Controls.Add(splitContainerRight);
            pnlRight.Dock = DockStyle.Fill;
            pnlRight.Font = new Font("微软雅黑", 12F);
            pnlRight.Location = new Point(0, 0);
            pnlRight.Margin = new Padding(4, 5, 4, 5);
            pnlRight.MinimumSize = new Size(1, 1);
            pnlRight.Name = "pnlRight";
            pnlRight.Size = new Size(951, 668);
            pnlRight.TabIndex = 0;
            pnlRight.Text = null;
            pnlRight.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // splitContainerRight
            // 
            splitContainerRight.Cursor = Cursors.HSplit;
            splitContainerRight.Dock = DockStyle.Fill;
            splitContainerRight.Location = new Point(0, 0);
            splitContainerRight.MinimumSize = new Size(20, 20);
            splitContainerRight.Name = "splitContainerRight";
            splitContainerRight.Orientation = Orientation.Horizontal;
            // 
            // splitContainerRight.Panel1
            // 
            splitContainerRight.Panel1.Controls.Add(pnlMembers);
            // 
            // splitContainerRight.Panel2
            // 
            splitContainerRight.Panel2.Controls.Add(pnlOrders);
            splitContainerRight.Size = new Size(951, 668);
            splitContainerRight.SplitterDistance = 312;
            splitContainerRight.SplitterWidth = 5;
            splitContainerRight.TabIndex = 0;
            // 
            // pnlMembers
            // 
            pnlMembers.Controls.Add(dgvMembers);
            pnlMembers.Controls.Add(pnlMembersTop);
            pnlMembers.Dock = DockStyle.Fill;
            pnlMembers.Font = new Font("微软雅黑", 12F);
            pnlMembers.Location = new Point(0, 0);
            pnlMembers.Margin = new Padding(4, 5, 4, 5);
            pnlMembers.MinimumSize = new Size(1, 1);
            pnlMembers.Name = "pnlMembers";
            pnlMembers.Size = new Size(951, 312);
            pnlMembers.TabIndex = 0;
            pnlMembers.Text = null;
            pnlMembers.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // dgvMembers
            // 
            dgvMembers.AllowUserToAddRows = false;
            dgvMembers.AllowUserToDeleteRows = false;
            dgvMembers.AllowUserToResizeRows = false;
            dataGridViewCellStyle29.BackColor = Color.FromArgb(235, 243, 255);
            dgvMembers.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle29;
            dgvMembers.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvMembers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dgvMembers.BackgroundColor = Color.White;
            dgvMembers.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dgvMembers.ContextMenuStrip = cmsMembers;
            dataGridViewCellStyle30.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle30.BackColor = Color.FromArgb(80, 160, 255);
            dataGridViewCellStyle30.Font = new Font("微软雅黑", 12F);
            dataGridViewCellStyle30.ForeColor = Color.White;
            dataGridViewCellStyle30.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle30.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle30.WrapMode = DataGridViewTriState.True;
            dgvMembers.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle30;
            dgvMembers.ColumnHeadersHeight = 32;
            dgvMembers.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvMembers.EnableHeadersVisualStyles = false;
            dgvMembers.Font = new Font("微软雅黑", 10F);
            dgvMembers.GridColor = Color.FromArgb(80, 160, 255);
            dgvMembers.Location = new Point(0, 30);
            dgvMembers.MultiSelect = false;
            dgvMembers.Name = "dgvMembers";
            dataGridViewCellStyle31.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle31.BackColor = Color.FromArgb(235, 243, 255);
            dataGridViewCellStyle31.Font = new Font("微软雅黑", 10F);
            dataGridViewCellStyle31.ForeColor = Color.FromArgb(48, 48, 48);
            dataGridViewCellStyle31.SelectionBackColor = Color.FromArgb(80, 160, 255);
            dataGridViewCellStyle31.SelectionForeColor = Color.White;
            dataGridViewCellStyle31.WrapMode = DataGridViewTriState.True;
            dgvMembers.RowHeadersDefaultCellStyle = dataGridViewCellStyle31;
            dgvMembers.RowHeadersVisible = false;
            dgvMembers.RowHeadersWidth = 51;
            dataGridViewCellStyle32.BackColor = Color.White;
            dataGridViewCellStyle32.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            dgvMembers.RowsDefaultCellStyle = dataGridViewCellStyle32;
            dgvMembers.RowTemplate.Height = 29;
            dgvMembers.SelectedIndex = -1;
            dgvMembers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvMembers.Size = new Size(951, 282);
            dgvMembers.StripeOddColor = Color.FromArgb(235, 243, 255);
            dgvMembers.TabIndex = 1;
            dgvMembers.SelectionChanged += dgvMembers_SelectionChanged;
            // 
            // pnlMembersTop
            // 
            pnlMembersTop.Controls.Add(lblMemberInfo);
            pnlMembersTop.Dock = DockStyle.Top;
            pnlMembersTop.Font = new Font("微软雅黑", 12F);
            pnlMembersTop.Location = new Point(0, 0);
            pnlMembersTop.Margin = new Padding(4, 5, 4, 5);
            pnlMembersTop.MinimumSize = new Size(1, 1);
            pnlMembersTop.Name = "pnlMembersTop";
            pnlMembersTop.Size = new Size(951, 30);
            pnlMembersTop.TabIndex = 0;
            pnlMembersTop.Text = null;
            pnlMembersTop.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // lblMemberInfo
            // 
            lblMemberInfo.Dock = DockStyle.Fill;
            lblMemberInfo.Font = new Font("微软雅黑", 10F, FontStyle.Bold);
            lblMemberInfo.ForeColor = Color.FromArgb(48, 48, 48);
            lblMemberInfo.Location = new Point(0, 0);
            lblMemberInfo.Name = "lblMemberInfo";
            lblMemberInfo.Size = new Size(951, 30);
            lblMemberInfo.TabIndex = 0;
            lblMemberInfo.Text = "会员列表 (共0人)";
            lblMemberInfo.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // pnlOrders
            // 
            pnlOrders.Controls.Add(dgvOrders);
            pnlOrders.Controls.Add(pnlOrdersTop);
            pnlOrders.Dock = DockStyle.Fill;
            pnlOrders.Font = new Font("微软雅黑", 12F);
            pnlOrders.Location = new Point(0, 0);
            pnlOrders.Margin = new Padding(4, 5, 4, 5);
            pnlOrders.MinimumSize = new Size(1, 1);
            pnlOrders.Name = "pnlOrders";
            pnlOrders.Size = new Size(951, 351);
            pnlOrders.TabIndex = 0;
            pnlOrders.Text = null;
            pnlOrders.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // dgvOrders
            // 
            dgvOrders.AllowUserToAddRows = false;
            dgvOrders.AllowUserToDeleteRows = false;
            dgvOrders.AllowUserToResizeRows = false;
            dataGridViewCellStyle33.BackColor = Color.FromArgb(235, 243, 255);
            dgvOrders.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle33;
            dgvOrders.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvOrders.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dgvOrders.BackgroundColor = Color.White;
            dgvOrders.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle34.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle34.BackColor = Color.FromArgb(80, 160, 255);
            dataGridViewCellStyle34.Font = new Font("微软雅黑", 12F);
            dataGridViewCellStyle34.ForeColor = Color.White;
            dataGridViewCellStyle34.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle34.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle34.WrapMode = DataGridViewTriState.True;
            dgvOrders.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle34;
            dgvOrders.ColumnHeadersHeight = 32;
            dgvOrders.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvOrders.EnableHeadersVisualStyles = false;
            dgvOrders.Font = new Font("微软雅黑", 10F);
            dgvOrders.GridColor = Color.FromArgb(80, 160, 255);
            dgvOrders.Location = new Point(0, 30);
            dgvOrders.MultiSelect = false;
            dgvOrders.Name = "dgvOrders";
            dataGridViewCellStyle35.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle35.BackColor = Color.FromArgb(235, 243, 255);
            dataGridViewCellStyle35.Font = new Font("微软雅黑", 10F);
            dataGridViewCellStyle35.ForeColor = Color.FromArgb(48, 48, 48);
            dataGridViewCellStyle35.SelectionBackColor = Color.FromArgb(80, 160, 255);
            dataGridViewCellStyle35.SelectionForeColor = Color.White;
            dataGridViewCellStyle35.WrapMode = DataGridViewTriState.True;
            dgvOrders.RowHeadersDefaultCellStyle = dataGridViewCellStyle35;
            dgvOrders.RowHeadersVisible = false;
            dgvOrders.RowHeadersWidth = 51;
            dataGridViewCellStyle36.BackColor = Color.White;
            dataGridViewCellStyle36.Font = new Font("宋体", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            dgvOrders.RowsDefaultCellStyle = dataGridViewCellStyle36;
            dgvOrders.RowTemplate.Height = 29;
            dgvOrders.SelectedIndex = -1;
            dgvOrders.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvOrders.Size = new Size(951, 321);
            dgvOrders.StripeOddColor = Color.FromArgb(235, 243, 255);
            dgvOrders.TabIndex = 1;
            // 
            // pnlOrdersTop
            // 
            pnlOrdersTop.Controls.Add(lblOrderInfo);
            pnlOrdersTop.Dock = DockStyle.Top;
            pnlOrdersTop.Font = new Font("微软雅黑", 12F);
            pnlOrdersTop.Location = new Point(0, 0);
            pnlOrdersTop.Margin = new Padding(4, 5, 4, 5);
            pnlOrdersTop.MinimumSize = new Size(1, 1);
            pnlOrdersTop.Name = "pnlOrdersTop";
            pnlOrdersTop.Size = new Size(951, 30);
            pnlOrdersTop.TabIndex = 0;
            pnlOrdersTop.Text = null;
            pnlOrdersTop.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // lblOrderInfo
            // 
            lblOrderInfo.Dock = DockStyle.Fill;
            lblOrderInfo.Font = new Font("微软雅黑", 10F, FontStyle.Bold);
            lblOrderInfo.ForeColor = Color.FromArgb(48, 48, 48);
            lblOrderInfo.Location = new Point(0, 0);
            lblOrderInfo.Name = "lblOrderInfo";
            lblOrderInfo.Size = new Size(951, 30);
            lblOrderInfo.TabIndex = 0;
            lblOrderInfo.Text = "订单列表 (共0单)";
            lblOrderInfo.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // pnlTopButtons
            // 
            pnlTopButtons.Controls.Add(ucUserInfo1);
            pnlTopButtons.Controls.Add(btnClearData);
            pnlTopButtons.Controls.Add(btnOpenLotteryResult);
            pnlTopButtons.Controls.Add(btnConnect);
            pnlTopButtons.Controls.Add(btnLog);
            pnlTopButtons.Controls.Add(btnSettings);
            pnlTopButtons.Dock = DockStyle.Top;
            pnlTopButtons.Font = new Font("微软雅黑", 12F);
            pnlTopButtons.Location = new Point(0, 35);
            pnlTopButtons.Margin = new Padding(4, 5, 4, 5);
            pnlTopButtons.MinimumSize = new Size(1, 1);
            pnlTopButtons.Name = "pnlTopButtons";
            pnlTopButtons.Padding = new Padding(5);
            pnlTopButtons.Size = new Size(1200, 60);
            pnlTopButtons.TabIndex = 1;
            pnlTopButtons.Text = null;
            pnlTopButtons.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // btnClearData
            // 
            btnClearData.Cursor = Cursors.Hand;
            btnClearData.Font = new Font("微软雅黑", 10F);
            btnClearData.Location = new Point(989, 12);
            btnClearData.MinimumSize = new Size(1, 1);
            btnClearData.Name = "btnClearData";
            btnClearData.Size = new Size(100, 40);
            btnClearData.TabIndex = 4;
            btnClearData.Text = "清空数据";
            btnClearData.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnClearData.Click += btnClearData_Click;
            // 
            // btnOpenLotteryResult
            // 
            btnOpenLotteryResult.Cursor = Cursors.Hand;
            btnOpenLotteryResult.Font = new Font("微软雅黑", 10F);
            btnOpenLotteryResult.Location = new Point(460, 14);
            btnOpenLotteryResult.MinimumSize = new Size(1, 1);
            btnOpenLotteryResult.Name = "btnOpenLotteryResult";
            btnOpenLotteryResult.Size = new Size(100, 40);
            btnOpenLotteryResult.TabIndex = 3;
            btnOpenLotteryResult.Text = "开奖结果";
            btnOpenLotteryResult.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnOpenLotteryResult.Click += btnOpenLotteryResult_Click;
            // 
            // btnConnect
            // 
            btnConnect.Cursor = Cursors.Hand;
            btnConnect.FillHoverColor = Color.FromArgb(100, 180, 255);
            btnConnect.FillPressColor = Color.FromArgb(60, 140, 235);
            btnConnect.FillSelectedColor = Color.FromArgb(60, 140, 235);
            btnConnect.Font = new Font("微软雅黑", 10F, FontStyle.Bold);
            btnConnect.Location = new Point(253, 12);
            btnConnect.MinimumSize = new Size(1, 1);
            btnConnect.Name = "btnConnect";
            btnConnect.Radius = 6;
            btnConnect.RectHoverColor = Color.FromArgb(100, 180, 255);
            btnConnect.RectPressColor = Color.FromArgb(60, 140, 235);
            btnConnect.RectSelectedColor = Color.FromArgb(60, 140, 235);
            btnConnect.Size = new Size(100, 40);
            btnConnect.TabIndex = 2;
            btnConnect.Text = "连接";
            btnConnect.TipsFont = new Font("微软雅黑", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnConnect.Click += btnConnect_Click;
            // 
            // btnLog
            // 
            btnLog.Cursor = Cursors.Hand;
            btnLog.Font = new Font("微软雅黑", 10F);
            btnLog.Location = new Point(359, 14);
            btnLog.MinimumSize = new Size(1, 1);
            btnLog.Name = "btnLog";
            btnLog.Size = new Size(95, 40);
            btnLog.TabIndex = 1;
            btnLog.Text = "日志";
            btnLog.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnLog.Click += btnLog_Click;
            // 
            // btnSettings
            // 
            btnSettings.Cursor = Cursors.Hand;
            btnSettings.Dock = DockStyle.Right;
            btnSettings.Font = new Font("微软雅黑", 10F);
            btnSettings.Location = new Point(1095, 5);
            btnSettings.MinimumSize = new Size(1, 1);
            btnSettings.Name = "btnSettings";
            btnSettings.Size = new Size(100, 50);
            btnSettings.TabIndex = 0;
            btnSettings.Text = "设置";
            btnSettings.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnSettings.Click += btnSettings_Click;
            // 
            // statusStrip
            // 
            statusStrip.BackColor = Color.FromArgb(243, 249, 255);
            statusStrip.Font = new Font("微软雅黑", 10F);
            statusStrip.ImageScalingSize = new Size(20, 20);
            statusStrip.Items.AddRange(new ToolStripItem[] { lblStatus });
            statusStrip.Location = new Point(0, 763);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new Size(1200, 25);
            statusStrip.TabIndex = 2;
            // 
            // lblStatus
            // 
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(37, 20);
            lblStatus.Text = "就绪";
            // 
            // ucUserInfo1
            // 
            ucUserInfo1.BackColor = Color.White;
            ucUserInfo1.Location = new Point(1, 0);
            ucUserInfo1.Name = "ucUserInfo1";
            ucUserInfo1.Size = new Size(246, 60);
            ucUserInfo1.TabIndex = 5;
            // 
            // VxMain
            // 
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(1200, 788);
            Controls.Add(splitContainerMain);
            Controls.Add(pnlTopButtons);
            Controls.Add(statusStrip);
            Name = "VxMain";
            Text = "百胜VX3Plus - 管理系统";
            ZoomScaleRect = new Rectangle(15, 15, 980, 762);
            Load += VxMain_Load;
            splitContainerMain.Panel1.ResumeLayout(false);
            splitContainerMain.Panel2.ResumeLayout(false);
            (splitContainerMain).EndInit();
            splitContainerMain.ResumeLayout(false);
            pnlLeft.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvContacts).EndInit();
            pnlLeftTop.ResumeLayout(false);
            pnlRight.ResumeLayout(false);
            splitContainerRight.Panel1.ResumeLayout(false);
            splitContainerRight.Panel2.ResumeLayout(false);
            (splitContainerRight).EndInit();
            splitContainerRight.ResumeLayout(false);
            pnlMembers.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvMembers).EndInit();
            pnlMembersTop.ResumeLayout(false);
            pnlOrders.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvOrders).EndInit();
            pnlOrdersTop.ResumeLayout(false);
            pnlTopButtons.ResumeLayout(false);
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Sunny.UI.UISplitContainer splitContainerMain;
        private Sunny.UI.UIPanel pnlLeft;
        private Sunny.UI.UIDataGridView dgvContacts;
        private Sunny.UI.UIPanel pnlLeftTop;
        private Sunny.UI.UIButton btnRefreshContacts;
        private Sunny.UI.UIButton btnBindingContacts;
        private Sunny.UI.UITextBox txtCurrentContact;
        private Sunny.UI.UIPanel pnlRight;
        private Sunny.UI.UISplitContainer splitContainerRight;
        private Sunny.UI.UIPanel pnlMembers;
        private Sunny.UI.UIDataGridView dgvMembers;
        private Sunny.UI.UIPanel pnlMembersTop;
        private Sunny.UI.UILabel lblMemberInfo;
        private Sunny.UI.UIPanel pnlOrders;
        private Sunny.UI.UIDataGridView dgvOrders;
        private Sunny.UI.UIPanel pnlOrdersTop;
        private Sunny.UI.UILabel lblOrderInfo;
        private Sunny.UI.UIPanel pnlTopButtons;
        private Sunny.UI.UIButton btnSettings;
        private Sunny.UI.UIButton btnConnect;
        private Sunny.UI.UIButton btnLog;
        private Sunny.UI.UIButton btnOpenLotteryResult;
        private Sunny.UI.UIButton btnClearData;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel lblStatus;
        private Views.UcUserInfo ucUserInfo1;
        private Sunny.UI.UIContextMenuStrip cmsMembers;
    }
}
