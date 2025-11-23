using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BaiShengVx3Plus.Shared.Models.Games.Binggo.Statistics;

namespace BinGoPlans.Controls
{
    /// <summary>
    /// 路珠绘制控件（类似百家乐的路珠）
    /// 纵向显示：连续相同结果横向排列，结果改变时换列
    /// </summary>
    public class RoadBeadControl : Control
    {
        private List<RoadBeadItem> _data = new List<RoadBeadItem>();
        private int _cellSize = 25;
        private Font _font;
        private Font _issueFont;

        /// <summary>
        /// 路珠数据项（包含期号信息）
        /// </summary>
        private class RoadBeadItem
        {
            public int IssueId { get; set; }
            public PlayResult Result { get; set; }
        }

        public RoadBeadControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer | ControlStyles.ResizeRedraw, true);
            _font = new Font("Arial", 9, FontStyle.Bold);
            _issueFont = new Font("Arial", 7);
        }

        /// <summary>
        /// 设置数据（需要包含期号信息）
        /// </summary>
        public void SetData(List<PositionPlayResult> data, List<int> issueIds = null)
        {
            _data.Clear();
            if (data != null && data.Count > 0)
            {
                for (int i = 0; i < data.Count; i++)
                {
                    int issueId = 0;
                    if (issueIds != null && i < issueIds.Count)
                    {
                        issueId = issueIds[i];
                    }
                    _data.Add(new RoadBeadItem
                    {
                        IssueId = issueId,
                        Result = data[i].Result
                    });
                }
            }
            
            // 计算并设置控件大小
            UpdateControlSize();
            
            Invalidate();
        }
        
        /// <summary>
        /// 更新控件大小
        /// </summary>
        private void UpdateControlSize()
        {
            if (_data.Count == 0)
            {
                this.Size = new Size(100, 100); // 默认最小大小
                return;
            }
            
            var columns = OrganizeDataIntoColumns();
            int maxRows = columns.Count > 0 ? columns.Max(col => col.Count) : 0;
            if (maxRows == 0)
            {
                this.Size = new Size(100, 100);
                return;
            }
            
            int colCount = columns.Count;
            int rowCount = maxRows + 1; // +1 用于显示期号行
            
            int totalWidth = colCount * _cellSize;
            int totalHeight = rowCount * _cellSize;
            
            this.Size = new Size(Math.Max(100, totalWidth), Math.Max(100, totalHeight));
        }

        /// <summary>
        /// 单元格大小
        /// </summary>
        public int CellSize
        {
            get => _cellSize;
            set
            {
                _cellSize = Math.Max(20, Math.Min(50, value));
                UpdateControlSize();
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.Clear(BackColor);

            if (_data.Count == 0) return;

            // 组织数据为列结构（百家乐路珠方式）
            var columns = OrganizeDataIntoColumns();

            int maxRows = columns.Count > 0 ? columns.Max(col => col.Count) : 0;
            if (maxRows == 0) return;

            // 绘制路珠
            int currentX = 0;
            foreach (var column in columns)
            {
                if (column.Count == 0) continue;

                int startIssueId = column[0].IssueId;
                int issueTail = startIssueId % 100; // 期号尾后2位

                // 绘制第0行：期号
                var issueRect = new Rectangle(currentX, 0, _cellSize - 1, _cellSize - 1);
                g.FillRectangle(Brushes.LightGray, issueRect);
                g.DrawRectangle(Pens.Black, issueRect);
                var issueText = issueTail.ToString("00");
                var issueTextSize = g.MeasureString(issueText, _issueFont);
                var issueTextX = issueRect.X + (issueRect.Width - issueTextSize.Width) / 2;
                var issueTextY = issueRect.Y + (issueRect.Height - issueTextSize.Height) / 2;
                g.DrawString(issueText, _issueFont, Brushes.Black, issueTextX, issueTextY);

                // 绘制数据行（从第1行开始）
                for (int row = 0; row < column.Count; row++)
                {
                    var item = column[row];
                    var rect = new Rectangle(currentX, (row + 1) * _cellSize, _cellSize - 1, _cellSize - 1);

                    // 绘制背景色
                    var colorName = item.Result.GetColorName();
                    var color = Color.FromName(colorName);
                    using (var brush = new SolidBrush(color))
                    {
                        g.FillRectangle(brush, rect);
                    }

                    // 绘制边框
                    g.DrawRectangle(Pens.Black, rect);

                    // 绘制文本
                    var text = item.Result.GetDisplayText();
                    var textSize = g.MeasureString(text, _font);
                    var textX = rect.X + (rect.Width - textSize.Width) / 2;
                    var textY = rect.Y + (rect.Height - textSize.Height) / 2;
                    g.DrawString(text, _font, Brushes.White, textX, textY);
                }

                currentX += _cellSize;
            }
        }

        /// <summary>
        /// 将数据组织为列结构（百家乐路珠方式）
        /// 连续相同的结果在同一列，结果改变时换列
        /// </summary>
        private List<List<RoadBeadItem>> OrganizeDataIntoColumns()
        {
            var columns = new List<List<RoadBeadItem>>();
            if (_data.Count == 0) return columns;

            var currentColumn = new List<RoadBeadItem>();
            PlayResult? lastResult = null;

            foreach (var item in _data)
            {
                // 如果结果改变，开始新的一列
                if (lastResult.HasValue && item.Result != lastResult.Value)
                {
                    if (currentColumn.Count > 0)
                    {
                        columns.Add(currentColumn);
                    }
                    currentColumn = new List<RoadBeadItem>();
                }

                currentColumn.Add(item);
                lastResult = item.Result;
            }

            // 添加最后一列
            if (currentColumn.Count > 0)
            {
                columns.Add(currentColumn);
            }

            return columns;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _font?.Dispose();
                _issueFont?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

