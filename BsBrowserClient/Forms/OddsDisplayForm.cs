using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Unit.Shared.Models;

namespace BsBrowserClient.Forms
{
    /// <summary>
    /// 赔率显示窗口
    /// </summary>
    public partial class OddsDisplayForm : Form
    {
        private DataGridView dgvOdds;
        private Panel pnlTop;
        private Label lblTitle;
        private TextBox txtSearch;
        private Label lblCount;
        
        public OddsDisplayForm()
        {
            InitializeComponent();
        }
        
        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            // 窗口设置
            this.Text = "赔率信息";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimumSize = new Size(700, 400);
            
            // 顶部面板
            pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 40,
                Padding = new Padding(10, 5, 10, 5)
            };
            
            // 标题标签
            lblTitle = new Label
            {
                Text = "📊 平台赔率信息",
                Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(10, 10)
            };
            pnlTop.Controls.Add(lblTitle);
            
            // 搜索框
            txtSearch = new TextBox
            {
                Width = 200,
                Location = new Point(200, 8),
                PlaceholderText = "搜索..."
            };
            txtSearch.TextChanged += TxtSearch_TextChanged;
            pnlTop.Controls.Add(txtSearch);
            
            // 计数标签
            lblCount = new Label
            {
                Text = "共 0 项",
                AutoSize = true,
                Location = new Point(410, 11),
                ForeColor = Color.Gray
            };
            pnlTop.Controls.Add(lblCount);
            
            // DataGridView
            dgvOdds = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = Color.FromArgb(230, 230, 230),
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(248, 248, 248)
                }
            };
            
            // 添加列
            dgvOdds.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Car",
                HeaderText = "车号",
                DataPropertyName = "Car",
                Width = 80,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None
            });
            
            dgvOdds.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Play",
                HeaderText = "玩法",
                DataPropertyName = "Play",
                Width = 80,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None
            });
            
            dgvOdds.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "FullName",
                HeaderText = "完整名称",
                DataPropertyName = "FullName",
                Width = 150,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None
            });
            
            dgvOdds.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "CarName",
                HeaderText = "网站名称",
                DataPropertyName = "CarName",
                Width = 100,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None
            });
            
            dgvOdds.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Odds",
                HeaderText = "赔率",
                DataPropertyName = "Odds",
                Width = 100,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "F2",
                    Alignment = DataGridViewContentAlignment.MiddleRight
                }
            });
            
            dgvOdds.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "OddsId",
                HeaderText = "网站ID",
                DataPropertyName = "OddsId",
                Width = 100,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None
            });
            
            // 添加控件到窗体
            this.Controls.Add(dgvOdds);
            this.Controls.Add(pnlTop);
            
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        
        private List<OddsInfo> _allOdds = new List<OddsInfo>();
        
        /// <summary>
        /// 设置赔率数据
        /// </summary>
        public void SetOddsData(List<OddsInfo> oddsList)
        {
            _allOdds = oddsList ?? new List<OddsInfo>();
            RefreshDisplay();
        }
        
        /// <summary>
        /// 刷新显示
        /// </summary>
        private void RefreshDisplay()
        {
            var filter = txtSearch.Text.Trim();
            var filteredList = string.IsNullOrEmpty(filter)
                ? _allOdds
                : _allOdds.Where(o =>
                    o.FullName.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    o.CarName.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    o.Car.ToString().Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    o.Play.ToString().Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    o.OddsId.Contains(filter, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            
            dgvOdds.DataSource = null;
            dgvOdds.DataSource = filteredList;
            
            lblCount.Text = $"共 {filteredList.Count} 项" + 
                (filteredList.Count != _allOdds.Count ? $" (筛选自 {_allOdds.Count} 项)" : "");
        }
        
        private void TxtSearch_TextChanged(object? sender, EventArgs e)
        {
            RefreshDisplay();
        }
    }
}

