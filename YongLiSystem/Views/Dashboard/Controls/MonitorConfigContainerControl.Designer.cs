using DevExpress.XtraTab;

namespace 永利系统.Views.Dashboard.Controls
{
    partial class MonitorConfigContainerControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            xtraTabControl = new XtraTabControl();
            xtraTabPageA = new XtraTabPage();
            xtraTabPageB = new XtraTabPage();
            xtraTabPageC = new XtraTabPage();
            ((System.ComponentModel.ISupportInitialize)xtraTabControl).BeginInit();
            xtraTabControl.SuspendLayout();
            SuspendLayout();
            // 
            // xtraTabControl
            // 
            xtraTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            xtraTabControl.Location = new System.Drawing.Point(0, 0);
            xtraTabControl.Name = "xtraTabControl";
            xtraTabControl.SelectedTabPage = xtraTabPageA;
            xtraTabControl.Size = new System.Drawing.Size(320, 680);
            xtraTabControl.TabIndex = 0;
            xtraTabControl.TabPages.AddRange(new XtraTabPage[] { xtraTabPageA, xtraTabPageB, xtraTabPageC });
            // 
            // xtraTabPageA
            // 
            xtraTabPageA.Name = "xtraTabPageA";
            xtraTabPageA.Size = new System.Drawing.Size(318, 654);
            xtraTabPageA.Text = "监控A";
            // 
            // xtraTabPageB
            // 
            xtraTabPageB.Name = "xtraTabPageB";
            xtraTabPageB.Size = new System.Drawing.Size(318, 654);
            xtraTabPageB.Text = "监控B";
            // 
            // xtraTabPageC
            // 
            xtraTabPageC.Name = "xtraTabPageC";
            xtraTabPageC.Size = new System.Drawing.Size(318, 654);
            xtraTabPageC.Text = "监控C";
            // 
            // MonitorConfigContainerControl
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(xtraTabControl);
            Name = "MonitorConfigContainerControl";
            Size = new System.Drawing.Size(320, 680);
            ((System.ComponentModel.ISupportInitialize)xtraTabControl).EndInit();
            xtraTabControl.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private XtraTabControl xtraTabControl;
        private XtraTabPage xtraTabPageA;
        private XtraTabPage xtraTabPageB;
        private XtraTabPage xtraTabPageC;
    }
}

