using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace 永利系统.Helpers
{
    /// <summary>
    /// 工具栏图标辅助类 - 动态生成工具栏图标
    /// </summary>
    public static class ToolbarIconHelper
    {
        /// <summary>
        /// 初始化 ImageList，添加所有工具栏图标
        /// </summary>
        public static void InitializeToolbarIcons(ImageList imageList)
        {
            if (imageList == null)
                throw new ArgumentNullException(nameof(imageList));

            imageList.Images.Clear();
            imageList.ImageSize = new Size(24, 24);
            imageList.ColorDepth = ColorDepth.Depth32Bit;

            // 添加各种图标
            imageList.Images.Add("Connect", CreateConnectIcon());
            imageList.Images.Add("Log", CreateLogIcon());
            imageList.Images.Add("LotteryResult", CreateLotteryResultIcon());
            imageList.Images.Add("CreditManage", CreateCreditManageIcon());
            imageList.Images.Add("ClearData", CreateClearDataIcon());
            imageList.Images.Add("Settings", CreateSettingsIcon());
        }

        /// <summary>
        /// 创建"连接"图标 - 链条图标
        /// </summary>
        public static Bitmap CreateConnectIcon()
        {
            Bitmap bmp = new Bitmap(24, 24);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                using (Pen pen = new Pen(Color.FromArgb(0, 122, 204), 2.5f))
                {
                    // 左边的圆环
                    g.DrawEllipse(pen, 2, 7, 8, 8);
                    // 右边的圆环
                    g.DrawEllipse(pen, 14, 7, 8, 8);
                    // 中间的连接线
                    g.DrawLine(pen, 10, 11, 14, 11);
                }
            }
            return bmp;
        }

        /// <summary>
        /// 创建"日志"图标 - 文档图标
        /// </summary>
        public static Bitmap CreateLogIcon()
        {
            Bitmap bmp = new Bitmap(24, 24);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                using (Brush brush = new SolidBrush(Color.FromArgb(0, 122, 204)))
                using (Pen pen = new Pen(Color.FromArgb(0, 122, 204), 1.5f))
                {
                    // 文档外框
                    g.DrawRectangle(pen, 5, 3, 14, 18);
                    // 顶部折角
                    g.DrawLine(pen, 14, 3, 14, 8);
                    g.DrawLine(pen, 14, 8, 19, 8);
                    // 文本行
                    g.DrawLine(pen, 8, 9, 16, 9);
                    g.DrawLine(pen, 8, 12, 16, 12);
                    g.DrawLine(pen, 8, 15, 13, 15);
                }
            }
            return bmp;
        }

        /// <summary>
        /// 创建"开奖结果"图标 - 奖杯图标
        /// </summary>
        public static Bitmap CreateLotteryResultIcon()
        {
            Bitmap bmp = new Bitmap(24, 24);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                using (Brush brush = new SolidBrush(Color.FromArgb(255, 193, 7)))
                using (Pen pen = new Pen(Color.FromArgb(255, 193, 7), 2f))
                {
                    // 奖杯杯身
                    Point[] cupBody = new Point[]
                    {
                        new Point(9, 6),
                        new Point(15, 6),
                        new Point(14, 12),
                        new Point(10, 12)
                    };
                    g.FillPolygon(brush, cupBody);
                    
                    // 奖杯底座
                    g.FillRectangle(brush, 8, 12, 8, 2);
                    g.FillRectangle(brush, 7, 14, 10, 3);
                    
                    // 把手
                    g.DrawArc(pen, 3, 6, 6, 6, 270, 180);
                    g.DrawArc(pen, 15, 6, 6, 6, 90, 180);
                }
            }
            return bmp;
        }

        /// <summary>
        /// 创建"上下分管理"图标 - 上下箭头图标
        /// </summary>
        public static Bitmap CreateCreditManageIcon()
        {
            Bitmap bmp = new Bitmap(24, 24);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                using (Brush greenBrush = new SolidBrush(Color.FromArgb(76, 175, 80)))
                using (Brush redBrush = new SolidBrush(Color.FromArgb(244, 67, 54)))
                {
                    // 向上的箭头（绿色，代表加分）
                    Point[] upArrow = new Point[]
                    {
                        new Point(7, 8),
                        new Point(12, 3),
                        new Point(17, 8),
                        new Point(14, 8),
                        new Point(14, 11),
                        new Point(10, 11),
                        new Point(10, 8)
                    };
                    g.FillPolygon(greenBrush, upArrow);
                    
                    // 向下的箭头（红色，代表扣分）
                    Point[] downArrow = new Point[]
                    {
                        new Point(7, 16),
                        new Point(12, 21),
                        new Point(17, 16),
                        new Point(14, 16),
                        new Point(14, 13),
                        new Point(10, 13),
                        new Point(10, 16)
                    };
                    g.FillPolygon(redBrush, downArrow);
                }
            }
            return bmp;
        }

        /// <summary>
        /// 创建"清空数据"图标 - 垃圾桶图标
        /// </summary>
        public static Bitmap CreateClearDataIcon()
        {
            Bitmap bmp = new Bitmap(24, 24);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                using (Brush brush = new SolidBrush(Color.FromArgb(244, 67, 54)))
                using (Pen pen = new Pen(Color.FromArgb(244, 67, 54), 1.5f))
                {
                    // 垃圾桶盖子
                    g.FillRectangle(brush, 6, 5, 12, 2);
                    // 垃圾桶把手
                    g.DrawArc(pen, 9, 2, 6, 4, 0, 180);
                    // 垃圾桶身体
                    Point[] body = new Point[]
                    {
                        new Point(7, 7),
                        new Point(17, 7),
                        new Point(16, 20),
                        new Point(8, 20)
                    };
                    g.FillPolygon(brush, body);
                    // 垃圾桶内部线条
                    using (Pen whitePen = new Pen(Color.White, 1.5f))
                    {
                        g.DrawLine(whitePen, 12, 9, 12, 18);
                        g.DrawLine(whitePen, 9, 9, 9, 18);
                        g.DrawLine(whitePen, 15, 9, 15, 18);
                    }
                }
            }
            return bmp;
        }

        /// <summary>
        /// 创建"设置"图标 - 齿轮图标
        /// </summary>
        public static Bitmap CreateSettingsIcon()
        {
            Bitmap bmp = new Bitmap(24, 24);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                using (Brush brush = new SolidBrush(Color.FromArgb(96, 125, 139)))
                using (Pen pen = new Pen(Color.FromArgb(96, 125, 139), 2f))
                {
                    // 简化的齿轮 - 外圈
                    int centerX = 12, centerY = 12;
                    int outerRadius = 9;
                    int innerRadius = 6;
                    int teethCount = 8;
                    
                    // 绘制齿轮齿
                    for (int i = 0; i < teethCount; i++)
                    {
                        double angle1 = (Math.PI * 2 / teethCount) * i - 0.15;
                        double angle2 = (Math.PI * 2 / teethCount) * i + 0.15;
                        
                        int x1 = centerX + (int)(outerRadius * Math.Cos(angle1));
                        int y1 = centerY + (int)(outerRadius * Math.Sin(angle1));
                        int x2 = centerX + (int)(outerRadius * Math.Cos(angle2));
                        int y2 = centerY + (int)(outerRadius * Math.Sin(angle2));
                        int x3 = centerX + (int)(innerRadius * Math.Cos(angle2));
                        int y3 = centerY + (int)(innerRadius * Math.Sin(angle2));
                        int x4 = centerX + (int)(innerRadius * Math.Cos(angle1));
                        int y4 = centerY + (int)(innerRadius * Math.Sin(angle1));
                        
                        Point[] tooth = new Point[] { 
                            new Point(x1, y1), 
                            new Point(x2, y2), 
                            new Point(x3, y3), 
                            new Point(x4, y4) 
                        };
                        g.FillPolygon(brush, tooth);
                    }
                    
                    // 中心圆孔
                    using (Brush holeBrush = new SolidBrush(Color.White))
                    {
                        g.FillEllipse(holeBrush, centerX - 3, centerY - 3, 6, 6);
                    }
                }
            }
            return bmp;
        }
    }
}

