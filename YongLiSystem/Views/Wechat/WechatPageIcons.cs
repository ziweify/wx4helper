using System.Drawing;

namespace YongLiSystem.Views.Wechat
{
    /// <summary>
    /// 微信助手工具栏图标资源类
    /// 提供静态方法创建图标，供设计器使用
    /// </summary>
    public static class WechatPageIcons
    {
        /// <summary>
        /// 创建连接图标 (24x24) - 插头图标
        /// </summary>
        public static Image CreateConnectIcon()
        {
            var bitmap = new Bitmap(24, 24);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);
                
                // 绘制插头图标（连接）
                using (var pen = new Pen(Color.FromArgb(0, 120, 215), 2))
                using (var brush = new SolidBrush(Color.FromArgb(0, 120, 215)))
                {
                    // 插头主体（矩形）
                    g.FillRectangle(brush, 6, 8, 12, 10);
                    g.DrawRectangle(pen, 6, 8, 12, 10);
                    
                    // 插头插脚（两个小矩形）
                    g.FillRectangle(brush, 8, 18, 2, 4);
                    g.FillRectangle(brush, 14, 18, 2, 4);
                    
                    // 连接线（从插头延伸）
                    g.DrawLine(pen, 12, 4, 12, 8);
                }
            }
            return bitmap;
        }

        /// <summary>
        /// 创建日志图标 (24x24) - 文档图标
        /// </summary>
        public static Image CreateLogIcon()
        {
            var bitmap = new Bitmap(24, 24);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);
                
                // 绘制文档图标（日志）
                using (var pen = new Pen(Color.FromArgb(0, 120, 215), 2))
                using (var brush = new SolidBrush(Color.FromArgb(0, 120, 215)))
                {
                    // 文档主体
                    var path = new System.Drawing.Drawing2D.GraphicsPath();
                    path.AddLines(new Point[] {
                        new Point(4, 4),
                        new Point(16, 4),
                        new Point(20, 8),
                        new Point(20, 20),
                        new Point(4, 20)
                    });
                    path.CloseAllFigures();
                    g.FillPath(brush, path);
                    g.DrawPath(pen, path);
                    
                    // 文档折角
                    g.DrawLine(new Pen(Color.White, 2), 16, 4, 16, 8);
                    g.DrawLine(new Pen(Color.White, 2), 16, 8, 20, 8);
                    
                    // 绘制文本行（日志）
                    using (var textPen = new Pen(Color.White, 1.5f))
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            g.DrawLine(textPen, 6, 11 + i * 3, 18, 11 + i * 3);
                        }
                    }
                }
            }
            return bitmap;
        }

        /// <summary>
        /// 创建开奖结果图标 (24x24) - 骰子图标
        /// </summary>
        public static Image CreateLotteryIcon()
        {
            var bitmap = new Bitmap(24, 24);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);
                
                // 绘制骰子图标（开奖结果）
                using (var pen = new Pen(Color.FromArgb(255, 193, 7), 2))
                using (var brush = new SolidBrush(Color.FromArgb(255, 193, 7)))
                {
                    // 骰子主体（圆角矩形）
                    var rect = new Rectangle(6, 6, 12, 12);
                    g.FillRectangle(brush, rect);
                    g.DrawRectangle(pen, rect);
                    
                    // 骰子点数（5个点）
                    using (var dotBrush = new SolidBrush(Color.FromArgb(48, 48, 48)))
                    {
                        // 四个角
                        g.FillEllipse(dotBrush, 8, 8, 2, 2);
                        g.FillEllipse(dotBrush, 14, 8, 2, 2);
                        g.FillEllipse(dotBrush, 8, 14, 2, 2);
                        g.FillEllipse(dotBrush, 14, 14, 2, 2);
                        // 中心点
                        g.FillEllipse(dotBrush, 11, 11, 2, 2);
                    }
                }
            }
            return bitmap;
        }

        /// <summary>
        /// 创建上下分管理图标 (24x24) - 货币/转账图标
        /// </summary>
        public static Image CreateCreditIcon()
        {
            var bitmap = new Bitmap(24, 24);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);
                
                // 绘制货币/转账图标（上下分管理）
                using (var pen = new Pen(Color.FromArgb(40, 167, 69), 2))
                using (var brush = new SolidBrush(Color.FromArgb(40, 167, 69)))
                {
                    // 圆形货币符号
                    g.DrawEllipse(pen, 6, 6, 12, 12);
                    
                    // 货币符号 "¥" 或 "$"
                    using (var font = new Font("Arial", 12, FontStyle.Bold))
                    using (var textBrush = new SolidBrush(Color.FromArgb(40, 167, 69)))
                    {
                        g.DrawString("¥", font, textBrush, 7, 4);
                    }
                    
                    // 上下箭头（表示上下分）
                    using (var arrowPen = new Pen(Color.FromArgb(40, 167, 69), 2))
                    {
                        // 上箭头
                        g.DrawLine(arrowPen, 12, 2, 10, 4);
                        g.DrawLine(arrowPen, 12, 2, 14, 4);
                        // 下箭头
                        g.DrawLine(arrowPen, 12, 20, 10, 18);
                        g.DrawLine(arrowPen, 12, 20, 14, 18);
                    }
                }
            }
            return bitmap;
        }

        /// <summary>
        /// 创建清空数据图标 (24x24) - 垃圾桶图标
        /// </summary>
        public static Image CreateClearIcon()
        {
            var bitmap = new Bitmap(24, 24);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);
                
                // 绘制垃圾桶图标（清空数据）
                using (var pen = new Pen(Color.FromArgb(220, 53, 69), 2))
                using (var brush = new SolidBrush(Color.FromArgb(220, 53, 69)))
                {
                    // 垃圾桶主体（梯形）
                    var path = new System.Drawing.Drawing2D.GraphicsPath();
                    path.AddLines(new Point[] {
                        new Point(8, 6),
                        new Point(16, 6),
                        new Point(18, 18),
                        new Point(6, 18)
                    });
                    path.CloseAllFigures();
                    g.FillPath(brush, path);
                    g.DrawPath(pen, path);
                    
                    // 垃圾桶盖子
                    g.DrawRectangle(pen, 7, 4, 10, 2);
                    g.FillRectangle(brush, 7, 4, 10, 2);
                    
                    // 盖子把手
                    g.DrawEllipse(pen, 11, 2, 2, 2);
                    g.FillEllipse(brush, 11, 2, 2, 2);
                    
                    // 垃圾桶条纹（表示已清空）
                    using (var linePen = new Pen(Color.White, 1))
                    {
                        g.DrawLine(linePen, 8, 10, 18, 10);
                        g.DrawLine(linePen, 8, 14, 18, 14);
                    }
                }
            }
            return bitmap;
        }

        /// <summary>
        /// 创建设置图标 (24x24) - 齿轮图标
        /// </summary>
        public static Image CreateSettingsIcon()
        {
            var bitmap = new Bitmap(24, 24);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);
                
                // 绘制齿轮图标（设置）
                using (var pen = new Pen(Color.FromArgb(108, 117, 125), 2))
                using (var brush = new SolidBrush(Color.FromArgb(108, 117, 125)))
                {
                    // 齿轮外圈（8个齿）
                    var centerX = 12f;
                    var centerY = 12f;
                    var outerRadius = 8f;
                    var innerRadius = 5f;
                    
                    var points = new PointF[16];
                    for (int i = 0; i < 8; i++)
                    {
                        var angle1 = i * System.Math.PI / 4 - System.Math.PI / 2;
                        var angle2 = (i + 0.5) * System.Math.PI / 4 - System.Math.PI / 2;
                        
                        points[i * 2] = new PointF(
                            centerX + (float)(outerRadius * System.Math.Cos(angle1)),
                            centerY + (float)(outerRadius * System.Math.Sin(angle1))
                        );
                        points[i * 2 + 1] = new PointF(
                            centerX + (float)(innerRadius * System.Math.Cos(angle2)),
                            centerY + (float)(innerRadius * System.Math.Sin(angle2))
                        );
                    }
                    
                    var path = new System.Drawing.Drawing2D.GraphicsPath();
                    path.AddPolygon(points);
                    g.FillPath(brush, path);
                    g.DrawPath(pen, path);
                    
                    // 中心圆
                    g.FillEllipse(Brushes.White, 9, 9, 6, 6);
                    g.DrawEllipse(pen, 9, 9, 6, 6);
                }
            }
            return bitmap;
        }
    }
}

