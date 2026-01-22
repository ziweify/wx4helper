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
            splitContainerMain = new SplitContainer();
            splitContainerLeft = new SplitContainer();
            treeViewFiles = new TreeView();
            labelFiles = new Label();
            listBoxFunctions = new ListBox();
            labelFunctions = new Label();
            splitContainerRight = new SplitContainer();
            scintilla = new ScintillaNET.Scintilla();
            tabControlDebug = new TabControl();
            tabPageBreakpoints = new TabPage();
            listBoxBreakpoints = new ListBox();
            tabPageVariables = new TabPage();
            listViewVariables = new ListView();
            columnHeaderName = new ColumnHeader();
            columnHeaderValue = new ColumnHeader();
            columnHeaderType = new ColumnHeader();
            tabPageCallStack = new TabPage();
            listBoxCallStack = new ListBox();
            ((System.ComponentModel.ISupportInitialize)splitContainerMain).BeginInit();
            splitContainerMain.Panel1.SuspendLayout();
            splitContainerMain.Panel2.SuspendLayout();
            splitContainerMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainerLeft).BeginInit();
            splitContainerLeft.Panel1.SuspendLayout();
            splitContainerLeft.Panel2.SuspendLayout();
            splitContainerLeft.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainerRight).BeginInit();
            splitContainerRight.Panel1.SuspendLayout();
            splitContainerRight.Panel2.SuspendLayout();
            splitContainerRight.SuspendLayout();
            tabControlDebug.SuspendLayout();
            tabPageBreakpoints.SuspendLayout();
            tabPageVariables.SuspendLayout();
            tabPageCallStack.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainerMain
            // 
            splitContainerMain.Dock = DockStyle.Fill;
            splitContainerMain.FixedPanel = FixedPanel.Panel1;
            splitContainerMain.Location = new Point(0, 0);
            splitContainerMain.Margin = new Padding(3, 4, 3, 4);
            splitContainerMain.Name = "splitContainerMain";
            // 
            // splitContainerMain.Panel1
            // 
            splitContainerMain.Panel1.Controls.Add(splitContainerLeft);
            splitContainerMain.Panel1MinSize = 200;
            // 
            // splitContainerMain.Panel2
            // 
            splitContainerMain.Panel2.Controls.Add(splitContainerRight);
            splitContainerMain.Size = new Size(800, 607);
            splitContainerMain.SplitterDistance = 200;
            splitContainerMain.TabIndex = 0;
            // 
            // splitContainerLeft
            // 
            splitContainerLeft.Dock = DockStyle.Fill;
            splitContainerLeft.Location = new Point(0, 0);
            splitContainerLeft.Margin = new Padding(3, 4, 3, 4);
            splitContainerLeft.Name = "splitContainerLeft";
            splitContainerLeft.Orientation = Orientation.Horizontal;
            // 
            // splitContainerLeft.Panel1
            // 
            splitContainerLeft.Panel1.Controls.Add(treeViewFiles);
            splitContainerLeft.Panel1.Controls.Add(labelFiles);
            splitContainerLeft.Panel1MinSize = 150;
            // 
            // splitContainerLeft.Panel2
            // 
            splitContainerLeft.Panel2.Controls.Add(listBoxFunctions);
            splitContainerLeft.Panel2.Controls.Add(labelFunctions);
            splitContainerLeft.Panel2MinSize = 100;
            splitContainerLeft.Size = new Size(200, 607);
            splitContainerLeft.SplitterDistance = 364;
            splitContainerLeft.SplitterWidth = 5;
            splitContainerLeft.TabIndex = 0;
            // 
            // treeViewFiles
            // 
            treeViewFiles.Dock = DockStyle.Fill;
            treeViewFiles.HideSelection = false;
            treeViewFiles.Location = new Point(0, 30);
            treeViewFiles.Margin = new Padding(3, 4, 3, 4);
            treeViewFiles.Name = "treeViewFiles";
            treeViewFiles.Size = new Size(200, 334);
            treeViewFiles.TabIndex = 1;
            // 
            // labelFiles
            // 
            labelFiles.Dock = DockStyle.Top;
            labelFiles.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
            labelFiles.Location = new Point(0, 0);
            labelFiles.Name = "labelFiles";
            labelFiles.Padding = new Padding(5, 6, 5, 6);
            labelFiles.Size = new Size(200, 30);
            labelFiles.TabIndex = 0;
            labelFiles.Text = "📁 文件资源管理器";
            labelFiles.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // listBoxFunctions
            // 
            listBoxFunctions.Dock = DockStyle.Fill;
            listBoxFunctions.FormattingEnabled = true;
            listBoxFunctions.ItemHeight = 17;
            listBoxFunctions.Location = new Point(0, 30);
            listBoxFunctions.Margin = new Padding(3, 4, 3, 4);
            listBoxFunctions.Name = "listBoxFunctions";
            listBoxFunctions.Size = new Size(200, 208);
            listBoxFunctions.TabIndex = 1;
            // 
            // labelFunctions
            // 
            labelFunctions.Dock = DockStyle.Top;
            labelFunctions.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
            labelFunctions.Location = new Point(0, 0);
            labelFunctions.Name = "labelFunctions";
            labelFunctions.Padding = new Padding(5, 6, 5, 6);
            labelFunctions.Size = new Size(200, 30);
            labelFunctions.TabIndex = 0;
            labelFunctions.Text = "🔧 函数列表";
            labelFunctions.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // splitContainerRight
            // 
            splitContainerRight.Dock = DockStyle.Fill;
            splitContainerRight.Location = new Point(0, 0);
            splitContainerRight.Margin = new Padding(3, 4, 3, 4);
            splitContainerRight.Name = "splitContainerRight";
            splitContainerRight.Orientation = Orientation.Horizontal;
            // 
            // splitContainerRight.Panel1
            // 
            splitContainerRight.Panel1.Controls.Add(scintilla);
            splitContainerRight.Panel1MinSize = 200;
            // 
            // splitContainerRight.Panel2
            // 
            splitContainerRight.Panel2.Controls.Add(tabControlDebug);
            splitContainerRight.Panel2MinSize = 100;
            splitContainerRight.Size = new Size(596, 607);
            splitContainerRight.SplitterDistance = 424;
            splitContainerRight.SplitterWidth = 5;
            splitContainerRight.TabIndex = 0;
            // 
            // scintilla
            // 
            scintilla.AutoCMaxHeight = 9;
            scintilla.BiDirectionality = ScintillaNET.BiDirectionalDisplayType.Disabled;
            scintilla.CaretLineBackColor = Color.Black;
            scintilla.CaretLineVisible = true;
            scintilla.Dock = DockStyle.Fill;
            scintilla.LexerName = null;
            scintilla.Location = new Point(0, 0);
            scintilla.Margin = new Padding(3, 4, 3, 4);
            scintilla.Name = "scintilla";
            scintilla.ScrollWidth = 1;
            scintilla.Size = new Size(596, 424);
            scintilla.TabIndents = true;
            scintilla.TabIndex = 0;
            scintilla.UseRightToLeftReadingLayout = false;
            scintilla.WrapMode = ScintillaNET.WrapMode.None;
            // 
            // tabControlDebug
            // 
            tabControlDebug.Controls.Add(tabPageBreakpoints);
            tabControlDebug.Controls.Add(tabPageVariables);
            tabControlDebug.Controls.Add(tabPageCallStack);
            tabControlDebug.Dock = DockStyle.Fill;
            tabControlDebug.Location = new Point(0, 0);
            tabControlDebug.Margin = new Padding(3, 4, 3, 4);
            tabControlDebug.Name = "tabControlDebug";
            tabControlDebug.SelectedIndex = 0;
            tabControlDebug.Size = new Size(596, 178);
            tabControlDebug.TabIndex = 0;
            // 
            // tabPageBreakpoints
            // 
            tabPageBreakpoints.Controls.Add(listBoxBreakpoints);
            tabPageBreakpoints.Location = new Point(4, 26);
            tabPageBreakpoints.Margin = new Padding(3, 4, 3, 4);
            tabPageBreakpoints.Name = "tabPageBreakpoints";
            tabPageBreakpoints.Padding = new Padding(3, 4, 3, 4);
            tabPageBreakpoints.Size = new Size(588, 148);
            tabPageBreakpoints.TabIndex = 0;
            tabPageBreakpoints.Text = "断点";
            tabPageBreakpoints.UseVisualStyleBackColor = true;
            // 
            // listBoxBreakpoints
            // 
            listBoxBreakpoints.Dock = DockStyle.Fill;
            listBoxBreakpoints.FormattingEnabled = true;
            listBoxBreakpoints.ItemHeight = 17;
            listBoxBreakpoints.Location = new Point(3, 4);
            listBoxBreakpoints.Margin = new Padding(3, 4, 3, 4);
            listBoxBreakpoints.Name = "listBoxBreakpoints";
            listBoxBreakpoints.Size = new Size(582, 140);
            listBoxBreakpoints.TabIndex = 0;
            // 
            // tabPageVariables
            // 
            tabPageVariables.Controls.Add(listViewVariables);
            tabPageVariables.Location = new Point(4, 26);
            tabPageVariables.Margin = new Padding(3, 4, 3, 4);
            tabPageVariables.Name = "tabPageVariables";
            tabPageVariables.Padding = new Padding(3, 4, 3, 4);
            tabPageVariables.Size = new Size(538, 148);
            tabPageVariables.TabIndex = 1;
            tabPageVariables.Text = "变量";
            tabPageVariables.UseVisualStyleBackColor = true;
            // 
            // listViewVariables
            // 
            listViewVariables.Columns.AddRange(new ColumnHeader[] { columnHeaderName, columnHeaderValue, columnHeaderType });
            listViewVariables.Dock = DockStyle.Fill;
            listViewVariables.FullRowSelect = true;
            listViewVariables.GridLines = true;
            listViewVariables.Location = new Point(3, 4);
            listViewVariables.Margin = new Padding(3, 4, 3, 4);
            listViewVariables.Name = "listViewVariables";
            listViewVariables.Size = new Size(532, 140);
            listViewVariables.TabIndex = 0;
            listViewVariables.UseCompatibleStateImageBehavior = false;
            listViewVariables.View = View.Details;
            // 
            // columnHeaderName
            // 
            columnHeaderName.Text = "名称";
            columnHeaderName.Width = 150;
            // 
            // columnHeaderValue
            // 
            columnHeaderValue.Text = "值";
            columnHeaderValue.Width = 200;
            // 
            // columnHeaderType
            // 
            columnHeaderType.Text = "类型";
            columnHeaderType.Width = 100;
            // 
            // tabPageCallStack
            // 
            tabPageCallStack.Controls.Add(listBoxCallStack);
            tabPageCallStack.Location = new Point(4, 26);
            tabPageCallStack.Margin = new Padding(3, 4, 3, 4);
            tabPageCallStack.Name = "tabPageCallStack";
            tabPageCallStack.Padding = new Padding(3, 4, 3, 4);
            tabPageCallStack.Size = new Size(538, 148);
            tabPageCallStack.TabIndex = 2;
            tabPageCallStack.Text = "调用堆栈";
            tabPageCallStack.UseVisualStyleBackColor = true;
            // 
            // listBoxCallStack
            // 
            listBoxCallStack.Dock = DockStyle.Fill;
            listBoxCallStack.FormattingEnabled = true;
            listBoxCallStack.ItemHeight = 17;
            listBoxCallStack.Location = new Point(3, 4);
            listBoxCallStack.Margin = new Padding(3, 4, 3, 4);
            listBoxCallStack.Name = "listBoxCallStack";
            listBoxCallStack.Size = new Size(532, 140);
            listBoxCallStack.TabIndex = 0;
            // 
            // ScriptEditorControl
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(splitContainerMain);
            Margin = new Padding(3, 4, 3, 4);
            Name = "ScriptEditorControl";
            Size = new Size(800, 607);
            splitContainerMain.Panel1.ResumeLayout(false);
            splitContainerMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerMain).EndInit();
            splitContainerMain.ResumeLayout(false);
            splitContainerLeft.Panel1.ResumeLayout(false);
            splitContainerLeft.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerLeft).EndInit();
            splitContainerLeft.ResumeLayout(false);
            splitContainerRight.Panel1.ResumeLayout(false);
            splitContainerRight.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerRight).EndInit();
            splitContainerRight.ResumeLayout(false);
            tabControlDebug.ResumeLayout(false);
            tabPageBreakpoints.ResumeLayout(false);
            tabPageVariables.ResumeLayout(false);
            tabPageCallStack.ResumeLayout(false);
            ResumeLayout(false);
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
