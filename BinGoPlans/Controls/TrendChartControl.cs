using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BaiShengVx3Plus.Shared.Models.Games.Binggo.Statistics;

namespace BinGoPlans.Controls
{
        /// <summary>
        /// 走势图控件（显示每期的累计统计）
        /// </summary>
        public class TrendChartControl : Control
        {
            private List<TrendDataPoint> _data = new List<TrendDataPoint>();
            private GamePlayType _playType = GamePlayType.Size;
            private Font _font;
            private Pen _linePen;
            private Pen _bigLinePen;
            private Pen _smallLinePen;
            private int _pointRadius = 3;
            private int _padding = 40;

        public TrendChartControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer | ControlStyles.ResizeRedraw, true);
            _font = new Font("Arial", 8);
            _linePen = new Pen(Color.Blue, 2);
            _bigLinePen = new Pen(Color.Red, 2);
            _smallLinePen = new Pen(Color.Blue, 2);
        }

        /// <summary>
        /// 设置数据
        /// </summary>
        public void SetData(List<TrendDataPoint> data)
        {
            _data = data ?? new List<TrendDataPoint>();
            Invalidate();
        }

        /// <summary>
        /// 玩法类型
        /// </summary>
        public GamePlayType PlayType
        {
            get => _playType;
            set
            {
                _playType = value;
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

            var drawArea = new Rectangle(_padding, _padding, Width - _padding * 2, Height - _padding * 2);
            if (drawArea.Width <= 0 || drawArea.Height <= 0) return;

            // 获取数据范围
            int maxValue = GetMaxValue();
            int minValue = GetMinValue();
            int range = Math.Max(1, maxValue - minValue);

            // 绘制坐标轴
            DrawAxes(g, drawArea, minValue, maxValue);

            // 绘制数据点（显示累计值，而不是每期的统计值）
            var bigPoints = new List<PointF>();
            var smallPoints = new List<PointF>();
            float stepX = _data.Count > 1 ? (float)drawArea.Width / (_data.Count - 1) : drawArea.Width;

            int cumulativeBig = 0;
            int cumulativeSmall = 0;

            for (int i = 0; i < _data.Count; i++)
            {
                var point = _data[i];
                
                // 累计统计
                if (_playType == GamePlayType.Size)
                {
                    cumulativeBig += point.BigCount;
                    cumulativeSmall += point.SmallCount;
                }
                else if (_playType == GamePlayType.OddEven)
                {
                    cumulativeBig += point.OddCount;
                    cumulativeSmall += point.EvenCount;
                }
                else if (_playType == GamePlayType.TailSize)
                {
                    cumulativeBig += point.TailBigCount;
                    cumulativeSmall += point.TailSmallCount;
                }
                else if (_playType == GamePlayType.SumOddEven)
                {
                    cumulativeBig += point.SumOddCount;
                    cumulativeSmall += point.SumEvenCount;
                }
                else if (_playType == GamePlayType.DragonTiger)
                {
                    cumulativeBig += point.DragonCount;
                    cumulativeSmall += point.TigerCount;
                }

                float x = drawArea.Left + i * stepX;
                
                // 计算Y坐标（使用累计值）
                int maxCumulative = Math.Max(cumulativeBig, cumulativeSmall);
                int minCumulative = 0;
                int cumulativeRange = Math.Max(1, maxCumulative - minCumulative);
                
                if (cumulativeBig > 0)
                {
                    float yBig = drawArea.Bottom - (cumulativeBig - minCumulative) * drawArea.Height / cumulativeRange;
                    bigPoints.Add(new PointF(x, yBig));
                }
                
                if (cumulativeSmall > 0)
                {
                    float ySmall = drawArea.Bottom - (cumulativeSmall - minCumulative) * drawArea.Height / cumulativeRange;
                    smallPoints.Add(new PointF(x, ySmall));
                }
            }

            // 绘制连线（大/单/尾大/合单/龙 用红色，小/双/尾小/合双/虎 用蓝色）
            if (bigPoints.Count > 1)
            {
                for (int i = 0; i < bigPoints.Count - 1; i++)
                {
                    g.DrawLine(_bigLinePen, bigPoints[i], bigPoints[i + 1]);
                }
            }

            if (smallPoints.Count > 1)
            {
                for (int i = 0; i < smallPoints.Count - 1; i++)
                {
                    g.DrawLine(_smallLinePen, smallPoints[i], smallPoints[i + 1]);
                }
            }

            // 绘制数据点
            foreach (var point in bigPoints)
            {
                g.FillEllipse(Brushes.Red, point.X - _pointRadius, point.Y - _pointRadius, _pointRadius * 2, _pointRadius * 2);
                g.DrawEllipse(Pens.DarkRed, point.X - _pointRadius, point.Y - _pointRadius, _pointRadius * 2, _pointRadius * 2);
            }

            foreach (var point in smallPoints)
            {
                g.FillEllipse(Brushes.Blue, point.X - _pointRadius, point.Y - _pointRadius, _pointRadius * 2, _pointRadius * 2);
                g.DrawEllipse(Pens.DarkBlue, point.X - _pointRadius, point.Y - _pointRadius, _pointRadius * 2, _pointRadius * 2);
            }
        }


        private void DrawAxes(Graphics g, Rectangle drawArea, int minValue, int maxValue)
        {
            // Y轴
            g.DrawLine(Pens.Black, drawArea.Left, drawArea.Top, drawArea.Left, drawArea.Bottom);
            // X轴
            g.DrawLine(Pens.Black, drawArea.Left, drawArea.Bottom, drawArea.Right, drawArea.Bottom);

            // Y轴标签（显示累计值范围）
            int ySteps = 5;
            for (int i = 0; i <= ySteps; i++)
            {
                int value = minValue + (maxValue - minValue) * i / ySteps;
                float y = drawArea.Bottom - (i * drawArea.Height / ySteps);
                g.DrawString(value.ToString(), _font, Brushes.Black, 5, y - _font.Height / 2);
                g.DrawLine(Pens.LightGray, drawArea.Left, y, drawArea.Right, y);
            }

            // X轴标签（显示期号）
            int xSteps = Math.Min(10, _data.Count);
            if (xSteps > 1)
            {
                for (int i = 0; i < xSteps; i++)
                {
                    int index = i * (_data.Count - 1) / (xSteps - 1);
                    if (index >= _data.Count) break;
                    float x = drawArea.Left + index * (float)drawArea.Width / Math.Max(1, _data.Count - 1);
                    string label = _data[index].IssueId.ToString();
                    g.DrawString(label, _font, Brushes.Black, x - 20, drawArea.Bottom + 5);
                }
            }
        }

        private int GetMaxValue()
        {
            if (_data.Count == 0) return 10;
            
            // 计算累计最大值
            int cumulativeBig = 0;
            int cumulativeSmall = 0;
            int maxCumulative = 0;

            foreach (var point in _data)
            {
                if (_playType == GamePlayType.Size)
                {
                    cumulativeBig += point.BigCount;
                    cumulativeSmall += point.SmallCount;
                }
                else if (_playType == GamePlayType.OddEven)
                {
                    cumulativeBig += point.OddCount;
                    cumulativeSmall += point.EvenCount;
                }
                else if (_playType == GamePlayType.TailSize)
                {
                    cumulativeBig += point.TailBigCount;
                    cumulativeSmall += point.TailSmallCount;
                }
                else if (_playType == GamePlayType.SumOddEven)
                {
                    cumulativeBig += point.SumOddCount;
                    cumulativeSmall += point.SumEvenCount;
                }
                else if (_playType == GamePlayType.DragonTiger)
                {
                    cumulativeBig += point.DragonCount;
                    cumulativeSmall += point.TigerCount;
                }
                
                maxCumulative = Math.Max(maxCumulative, Math.Max(cumulativeBig, cumulativeSmall));
            }
            
            return maxCumulative > 0 ? maxCumulative : 10;
        }

        private int GetMinValue()
        {
            return 0;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _font?.Dispose();
                _linePen?.Dispose();
                _bigLinePen?.Dispose();
                _smallLinePen?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

