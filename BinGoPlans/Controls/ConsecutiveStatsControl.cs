using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BaiShengVx3Plus.Shared.Models.Games.Binggo.Statistics;

namespace BinGoPlans.Controls
{
    /// <summary>
    /// 连续统计控件
    /// </summary>
    public class ConsecutiveStatsControl : UserControl
    {
        private List<ConsecutiveStats> _data = new List<ConsecutiveStats>();
        private DataGridView _gridView;

        public ConsecutiveStatsControl()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            _gridView = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None
            };

            _gridView.Columns.Add("ConsecutiveCount", "连续次数");
            _gridView.Columns.Add("OccurrenceCount", "出现次数");
            _gridView.Columns[0].Width = 150;
            _gridView.Columns[1].Width = 150;

            Controls.Add(_gridView);
        }

        /// <summary>
        /// 设置数据
        /// </summary>
        public void SetData(List<ConsecutiveStats> data)
        {
            _data = data ?? new List<ConsecutiveStats>();
            RefreshGrid();
        }

        private void RefreshGrid()
        {
            _gridView.Rows.Clear();

            foreach (var stat in _data.OrderBy(s => s.ConsecutiveCount))
            {
                string countText = stat.ConsecutiveCount == 12 ? "11+" : stat.ConsecutiveCount.ToString();
                _gridView.Rows.Add(countText, stat.OccurrenceCount);
            }
        }
    }
}

