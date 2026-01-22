#nullable enable

namespace Unit.La.Controls
{
    partial class ScriptEditorControl
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer? components = null;

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.splitContainerMain = new System.Windows.Forms.SplitContainer();
            this.splitContainerLeft = new System.Windows.Forms.SplitContainer();
            this.treeViewFiles = new System.Windows.Forms.TreeView();
            this.labelFiles = new System.Windows.Forms.Label();
            this.listBoxFunctions = new System.Windows.Forms.ListBox();
            this.labelFunctions = new System.Windows.Forms.Label();
            this.splitContainerRight = new System.Windows.Forms.SplitContainer();
            this.scintilla = new ScintillaNET.Scintilla();
            this.tabControlDebug = new System.Windows.Forms.TabControl();
            this.tabPageBreakpoints = new System.Windows.Forms.TabPage();
            this.listBoxBreakpoints = new System.Windows.Forms.ListBox();
            this.tabPageVariables = new System.Windows.Forms.TabPage();
            this.listViewVariables = new System.Windows.Forms.ListView();
            this.columnHeaderName = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderValue = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderType = new System.Windows.Forms.ColumnHeader();
            this.tabPageCallStack = new System.Windows.Forms.TabPage();
            this.listBoxCallStack = new System.Windows.Forms.ListBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).BeginInit();
            this.splitContainerMain.Panel1.SuspendLayout();
            this.splitContainerMain.Panel2.SuspendLayout();
            this.splitContainerMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerLeft)).BeginInit();
            this.splitContainerLeft.Panel1.SuspendLayout();
            this.splitContainerLeft.Panel2.SuspendLayout();
            this.splitContainerLeft.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerRight)).BeginInit();
            this.splitContainerRight.Panel1.SuspendLayout();
            this.splitContainerRight.Panel2.SuspendLayout();
            this.splitContainerRight.SuspendLayout();
            this.tabControlDebug.SuspendLayout();
            this.tabPageBreakpoints.SuspendLayout();
            this.tabPageVariables.SuspendLayout();
            this.tabPageCallStack.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainerMain
            // 
            this.splitContainerMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainerMain.Location = new System.Drawing.Point(0, 0);
            this.splitContainerMain.Name = "splitContainerMain";
            // 
            // splitContainerMain.Panel1
            // 
            this.splitContainerMain.Panel1.Controls.Add(this.splitContainerLeft);
            this.splitContainerMain.Panel1MinSize = 200;
            // 
            // splitContainerMain.Panel2
            // 
            this.splitContainerMain.Panel2.Controls.Add(this.splitContainerRight);
            this.splitContainerMain.Size = new System.Drawing.Size(800, 500);
            this.splitContainerMain.SplitterDistance = 250;
            this.splitContainerMain.TabIndex = 0;
            // 
            // splitContainerLeft
            // 
            this.splitContainerLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerLeft.Location = new System.Drawing.Point(0, 0);
            this.splitContainerLeft.Name = "splitContainerLeft";
            this.splitContainerLeft.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerLeft.Panel1
            // 
            this.splitContainerLeft.Panel1.Controls.Add(this.treeViewFiles);
            this.splitContainerLeft.Panel1.Controls.Add(this.labelFiles);
            this.splitContainerLeft.Panel1MinSize = 150;
            // 
            // splitContainerLeft.Panel2
            // 
            this.splitContainerLeft.Panel2.Controls.Add(this.listBoxFunctions);
            this.splitContainerLeft.Panel2.Controls.Add(this.labelFunctions);
            this.splitContainerLeft.Panel2MinSize = 100;
            this.splitContainerLeft.Size = new System.Drawing.Size(250, 500);
            this.splitContainerLeft.SplitterDistance = 300;
            this.splitContainerLeft.TabIndex = 0;
            // 
            // treeViewFiles
            // 
            this.treeViewFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewFiles.HideSelection = false;
            this.treeViewFiles.Location = new System.Drawing.Point(0, 25);
            this.treeViewFiles.Name = "treeViewFiles";
            this.treeViewFiles.Size = new System.Drawing.Size(250, 275);
            this.treeViewFiles.TabIndex = 1;
            // 
            // labelFiles
            // 
            this.labelFiles.Dock = System.Windows.Forms.DockStyle.Top;
            this.labelFiles.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Bold);
            this.labelFiles.Location = new System.Drawing.Point(0, 0);
            this.labelFiles.Name = "labelFiles";
            this.labelFiles.Padding = new System.Windows.Forms.Padding(5);
            this.labelFiles.Size = new System.Drawing.Size(250, 25);
            this.labelFiles.TabIndex = 0;
            this.labelFiles.Text = "📁 文件资源管理器";
            this.labelFiles.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // listBoxFunctions
            // 
            this.listBoxFunctions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxFunctions.FormattingEnabled = true;
            this.listBoxFunctions.ItemHeight = 17;
            this.listBoxFunctions.Location = new System.Drawing.Point(0, 25);
            this.listBoxFunctions.Name = "listBoxFunctions";
            this.listBoxFunctions.Size = new System.Drawing.Size(250, 175);
            this.listBoxFunctions.TabIndex = 1;
            // 
            // labelFunctions
            // 
            this.labelFunctions.Dock = System.Windows.Forms.DockStyle.Top;
            this.labelFunctions.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Bold);
            this.labelFunctions.Location = new System.Drawing.Point(0, 0);
            this.labelFunctions.Name = "labelFunctions";
            this.labelFunctions.Padding = new System.Windows.Forms.Padding(5);
            this.labelFunctions.Size = new System.Drawing.Size(250, 25);
            this.labelFunctions.TabIndex = 0;
            this.labelFunctions.Text = "🔧 函数列表";
            this.labelFunctions.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // splitContainerRight
            // 
            this.splitContainerRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerRight.Location = new System.Drawing.Point(0, 0);
            this.splitContainerRight.Name = "splitContainerRight";
            this.splitContainerRight.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerRight.Panel1
            // 
            this.splitContainerRight.Panel1.Controls.Add(this.scintilla);
            this.splitContainerRight.Panel1MinSize = 200;
            // 
            // splitContainerRight.Panel2
            // 
            this.splitContainerRight.Panel2.Controls.Add(this.tabControlDebug);
            this.splitContainerRight.Panel2MinSize = 100;
            this.splitContainerRight.Size = new System.Drawing.Size(546, 500);
            this.splitContainerRight.SplitterDistance = 350;
            this.splitContainerRight.TabIndex = 0;
            // 
            // scintilla
            // 
            this.scintilla.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scintilla.Location = new System.Drawing.Point(0, 0);
            this.scintilla.Name = "scintilla";
            this.scintilla.Size = new System.Drawing.Size(546, 350);
            this.scintilla.TabIndex = 0;
            // 
            // tabControlDebug
            // 
            this.tabControlDebug.Controls.Add(this.tabPageBreakpoints);
            this.tabControlDebug.Controls.Add(this.tabPageVariables);
            this.tabControlDebug.Controls.Add(this.tabPageCallStack);
            this.tabControlDebug.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlDebug.Location = new System.Drawing.Point(0, 0);
            this.tabControlDebug.Name = "tabControlDebug";
            this.tabControlDebug.SelectedIndex = 0;
            this.tabControlDebug.Size = new System.Drawing.Size(546, 146);
            this.tabControlDebug.TabIndex = 0;
            // 
            // tabPageBreakpoints
            // 
            this.tabPageBreakpoints.Controls.Add(this.listBoxBreakpoints);
            this.tabPageBreakpoints.Location = new System.Drawing.Point(4, 26);
            this.tabPageBreakpoints.Name = "tabPageBreakpoints";
            this.tabPageBreakpoints.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageBreakpoints.Size = new System.Drawing.Size(538, 116);
            this.tabPageBreakpoints.TabIndex = 0;
            this.tabPageBreakpoints.Text = "断点";
            this.tabPageBreakpoints.UseVisualStyleBackColor = true;
            // 
            // listBoxBreakpoints
            // 
            this.listBoxBreakpoints.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxBreakpoints.FormattingEnabled = true;
            this.listBoxBreakpoints.ItemHeight = 17;
            this.listBoxBreakpoints.Location = new System.Drawing.Point(3, 3);
            this.listBoxBreakpoints.Name = "listBoxBreakpoints";
            this.listBoxBreakpoints.Size = new System.Drawing.Size(532, 110);
            this.listBoxBreakpoints.TabIndex = 0;
            // 
            // tabPageVariables
            // 
            this.tabPageVariables.Controls.Add(this.listViewVariables);
            this.tabPageVariables.Location = new System.Drawing.Point(4, 26);
            this.tabPageVariables.Name = "tabPageVariables";
            this.tabPageVariables.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageVariables.Size = new System.Drawing.Size(538, 116);
            this.tabPageVariables.TabIndex = 1;
            this.tabPageVariables.Text = "变量";
            this.tabPageVariables.UseVisualStyleBackColor = true;
            // 
            // listViewVariables
            // 
            this.listViewVariables.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderName,
            this.columnHeaderValue,
            this.columnHeaderType});
            this.listViewVariables.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewVariables.FullRowSelect = true;
            this.listViewVariables.GridLines = true;
            this.listViewVariables.Location = new System.Drawing.Point(3, 3);
            this.listViewVariables.Name = "listViewVariables";
            this.listViewVariables.Size = new System.Drawing.Size(532, 110);
            this.listViewVariables.TabIndex = 0;
            this.listViewVariables.UseCompatibleStateImageBehavior = false;
            this.listViewVariables.View = System.Windows.Forms.View.Details;
            // 
            // columnHeaderName
            // 
            this.columnHeaderName.Text = "名称";
            this.columnHeaderName.Width = 150;
            // 
            // columnHeaderValue
            // 
            this.columnHeaderValue.Text = "值";
            this.columnHeaderValue.Width = 200;
            // 
            // columnHeaderType
            // 
            this.columnHeaderType.Text = "类型";
            this.columnHeaderType.Width = 100;
            // 
            // tabPageCallStack
            // 
            this.tabPageCallStack.Controls.Add(this.listBoxCallStack);
            this.tabPageCallStack.Location = new System.Drawing.Point(4, 26);
            this.tabPageCallStack.Name = "tabPageCallStack";
            this.tabPageCallStack.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageCallStack.Size = new System.Drawing.Size(538, 116);
            this.tabPageCallStack.TabIndex = 2;
            this.tabPageCallStack.Text = "调用堆栈";
            this.tabPageCallStack.UseVisualStyleBackColor = true;
            // 
            // listBoxCallStack
            // 
            this.listBoxCallStack.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxCallStack.FormattingEnabled = true;
            this.listBoxCallStack.ItemHeight = 17;
            this.listBoxCallStack.Location = new System.Drawing.Point(3, 3);
            this.listBoxCallStack.Name = "listBoxCallStack";
            this.listBoxCallStack.Size = new System.Drawing.Size(532, 110);
            this.listBoxCallStack.TabIndex = 0;
            // 
            // ScriptEditorControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainerMain);
            this.Name = "ScriptEditorControl";
            this.Size = new System.Drawing.Size(800, 500);
            this.splitContainerMain.Panel1.ResumeLayout(false);
            this.splitContainerMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).EndInit();
            this.splitContainerMain.ResumeLayout(false);
            this.splitContainerLeft.Panel1.ResumeLayout(false);
            this.splitContainerLeft.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerLeft)).EndInit();
            this.splitContainerLeft.ResumeLayout(false);
            this.splitContainerRight.Panel1.ResumeLayout(false);
            this.splitContainerRight.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerRight)).EndInit();
            this.splitContainerRight.ResumeLayout(false);
            this.tabControlDebug.ResumeLayout(false);
            this.tabPageBreakpoints.ResumeLayout(false);
            this.tabPageVariables.ResumeLayout(false);
            this.tabPageCallStack.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainerMain;
        private System.Windows.Forms.SplitContainer splitContainerLeft;
        private System.Windows.Forms.TreeView treeViewFiles;
        private System.Windows.Forms.Label labelFiles;
        private System.Windows.Forms.ListBox listBoxFunctions;
        private System.Windows.Forms.Label labelFunctions;
        private System.Windows.Forms.SplitContainer splitContainerRight;
        private ScintillaNET.Scintilla scintilla;
        private System.Windows.Forms.TabControl tabControlDebug;
        private System.Windows.Forms.TabPage tabPageBreakpoints;
        private System.Windows.Forms.ListBox listBoxBreakpoints;
        private System.Windows.Forms.TabPage tabPageVariables;
        private System.Windows.Forms.ListView listViewVariables;
        private System.Windows.Forms.ColumnHeader columnHeaderName;
        private System.Windows.Forms.ColumnHeader columnHeaderValue;
        private System.Windows.Forms.ColumnHeader columnHeaderType;
        private System.Windows.Forms.TabPage tabPageCallStack;
        private System.Windows.Forms.ListBox listBoxCallStack;
    }
}
