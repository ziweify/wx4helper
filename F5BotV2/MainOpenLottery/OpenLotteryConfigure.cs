using CCWin.SkinControl;
using CsQuery.StringScanner.Patterns;
using F5BotV2.BetSite.Qt;
using LxLib.LxSys;
using F5BotV2.Main;
using F5BotV2.Model;
using F5BotV2.Model.BindSqlite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace F5BotV2.MainOpenLottery
{
    public static class OpenLotteryConfigure
    {
        //private static QtBet qt = new QtBet();
        //private static BindlSqliteBgLotteryData dataManage = new BindlSqliteBgLotteryData();
        //public static BgLotteryDataViewManage dataBindList = new BgLotteryDataViewManage();
        //private static OpenLotteryView _openView = new OpenLotteryView();


        //public static OpenLotteryView getView()
        //{
        //    return _openView;
        //}

        public static void getdata(this OpenLotteryView view, DateTime date, string cookie)
        {
            //var ldResult = qt.getLotteryData(date, cookie);
            //view.GetLotteryDataLite(new
            //     Func<SkinDataGridView, bool>((p) => {
            //         foreach (var item in ldResult)
            //         {
            //             view.lotteryDatalite.Add(item);
            //         }
            //         return true;
            //     }));
        }


        public static string InitDataView(this OpenLotteryView view, DataGridView dgv, object datasource, Func<DataGridView, bool> customization)
        {
            dgv.DataSource = datasource;
            dgv.AllowUserToAddRows = false;
            //dgv.ReadOnly = true;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;    //整行选择
            dgv.AllowUserToResizeColumns = false;
            dgv.AllowUserToResizeRows = false;
            dgv.RowHeadersVisible = false;
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            //dgv.RowsDefaultCellStyle.BackColor = Color.FromArgb(100, 52, 152, 219); //修改选择行背景色有问题
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(128, 128, 128);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.DarkBlue;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("宋体", 9, FontStyle.Bold);

            //禁止自动生成列，以下场景会用到：数据源的列超过需要展示的列
            dgv.AutoGenerateColumns = false;

            customization(dgv);
            return "";
        }

        public static void dgvLotteryDataInit(this OpenLotteryView view, SkinDataGridView dgview)
        {
              view.GetDgView(new Func<SkinDataGridView, bool>((p) => {
                  p.DataSource = view.lotteryDatalite;
                //全局属性
                p.AllowUserToResizeColumns = false;    //禁止表格拉动
                p.AllowUserToResizeRows = false;       //禁止表格拉动,防止改变大小
                p.RowHeadersVisible = false; //禁止出现空白首列
                p.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter; //表头居中
                p.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;          //单元格内容居中
                p.EnableHeadersVisualStyles = false;   //允许更改行, 列头的字体颜色
                p.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(128, 128, 128); //表头背景色
                p.ColumnHeadersDefaultCellStyle.ForeColor = Color.DarkBlue;
                p.ColumnHeadersDefaultCellStyle.Font = new Font("宋体", 9, FontStyle.Bold);

                //单个属性
                var cell = dgview.Columns["IssueId"];
                if (cell != null)
                    cell.Width = 60;    // cell.HeaderCell.Value = "表头名";

                cell = dgview.Columns["P1"];
                if (cell != null)
                    cell.Width = 26;

                cell = dgview.Columns["P2"];
                if (cell != null)
                    cell.Width = 26;
                cell = dgview.Columns["P3"];
                if (cell != null)
                    cell.Width = 26;
                cell = dgview.Columns["P4"];
                if (cell != null)
                    cell.Width = 26;
                cell = dgview.Columns["P5"];
                if (cell != null)
                    cell.Width = 26;
                cell = dgview.Columns["P总"];
                if (cell != null)
                    cell.Width = 28;
                cell = dgview.Columns["P龙虎"];
                if (cell != null)
                    cell.Width = 26;
                cell = dgview.Columns["open_timestamp"];
                if (cell != null)
                    cell.Width = 38;
                cell = dgview.Columns["lastError"];
                if (cell != null)
                    cell.Visible = false;
                cell = dgview.Columns["BLDX_Da"];
                if (cell != null)
                    cell.Width = 26;
                cell = dgview.Columns["BLDX_Xiao"];
                if (cell != null)
                    cell.Width = 26;
                cell = dgview.Columns["BLDX_dxSum"];
                if (cell != null)
                    cell.Width = 26;
                cell = dgview.Columns["BLDX_dxAllSum"];
                if (cell != null)
                    cell.Width = 32;
                cell = dgview.Columns["BLDS_Dan"];
                if (cell != null)
                    cell.Width = 26;
                cell = dgview.Columns["BLDS_Shuang"];
                if (cell != null)
                    cell.Width = 26;
                cell = dgview.Columns["BLDS_dsSum"];
                if (cell != null)
                    cell.Width = 26;
                cell = dgview.Columns["BLDS_dsAllSum"];
                if (cell != null)
                    cell.Width = 32;

                cell = dgview.Columns["BLDX_Lian"];
                if (cell != null)
                    cell.Width = 26;
                cell = dgview.Columns["BLDX_Tiao"];
                if (cell != null)
                    cell.Width = 26;
                cell = dgview.Columns["BLDX_ltCount"];
                if (cell != null)
                    cell.Width = 26;
                cell = dgview.Columns["BLDS_Lian"];
                if (cell != null)
                    cell.Width = 26;
                cell = dgview.Columns["BLDS_Tiao"];
                if (cell != null)
                    cell.Width = 26;
                cell = dgview.Columns["BLDS_ltCount"];
                if (cell != null)
                    cell.Width = 26;

                //P1号详细数据
                cell = dgview.Columns["P1DX_Sum"];
                if (cell != null)
                    cell.Width = 26;
                cell = dgview.Columns["P1DX_Lian"];
                if (cell != null)
                    cell.Width = 26;
                cell = dgview.Columns["P1DS_Sum"];
                if (cell != null)
                    cell.Width = 26;
                cell = dgview.Columns["P1DS_Lian"];
                if (cell != null)
                    cell.Width = 26;
                return true;
            }));
        }

        //格式化输出
        public static void dgvLotteryDataFormatting(this OpenLotteryView view, SkinDataGridView dgview, object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex > -1)
            {
                var column = dgview.Columns[e.ColumnIndex];
                string propertyName = "open_timestamp";
                if (column.DataPropertyName == propertyName)
                {
                    var data = (int)dgview.Rows[e.RowIndex].Cells[propertyName].Value;
                    e.Value = LxTimestampHelper.GetDateTime(data).ToString("HH:mm");
                    e.FormattingApplied = true;
                }
            }
        }


        public static void dgvLotteryDataCellPaintting(this OpenLotteryView view, SkinDataGridView dgview, object sender, DataGridViewCellPaintingEventArgs e)
        {
            if(e.RowIndex > -1)
            {
                var column = dgview.Columns[e.ColumnIndex];
                if(column != null)
                {
                    var cellName = column.DataPropertyName;
                   
                    switch (cellName)
                    {
                        case "P1":
                            drawCellStyle(cellDataType.LotteryNumber, dgview, e, cellName);
                            break;
                        case "P2":
                            drawCellStyle(cellDataType.LotteryNumber, dgview, e, cellName);
                            break;
                        case "P3":
                            drawCellStyle(cellDataType.LotteryNumber, dgview, e, cellName);
                            break;
                        case "P4":
                            drawCellStyle(cellDataType.LotteryNumber, dgview, e, cellName);
                            break;
                        case "P5":
                            drawCellStyle(cellDataType.LotteryNumber, dgview, e, cellName);
                            break;
                        case "P总":
                            drawCellStyle(cellDataType.LotteryNumber, dgview, e, cellName);
                            break;
                        case "P龙虎":
                            drawCellStyle(cellDataType.龙虎, dgview, e, cellName);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     LotteryNumber - 开奖号码, 包括p1-p5, p总
        /// </summary>
        private enum cellDataType { 未知 = 0 , LotteryNumber = 1, 龙虎 = 2}
        /// <summary>
        ///     绘制单元格样式
        /// </summary>
        private static void drawCellStyle(cellDataType dtype, SkinDataGridView dgview, DataGridViewCellPaintingEventArgs e, string DataPropertyName)
        {
            switch(dtype)
            {
                case cellDataType.LotteryNumber:
                    {
                        int index = e.RowIndex;
                        var number = (LotteryNumber)dgview.Rows[index].Cells[DataPropertyName].Value;
                        if(number != null)
                        {
                            if (number.dx == NumberDX.大)
                            {
                                //dgview.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Red;       //设置整行颜色
                                var style = dgview.Rows[index].Cells[DataPropertyName].Style;
                                if (style.BackColor != Color.Red)
                                {
                                    style.BackColor = Color.Red;
                                    style.Font = new Font("新宋体", 8, FontStyle.Bold);
                                }
                                if (style.ForeColor != Color.White)
                                    style.ForeColor = Color.White;
                            }
                            else if (number.dx == NumberDX.小)
                            {
                                var style = dgview.Rows[index].Cells[DataPropertyName].Style;
                                if (style.BackColor != Color.Chartreuse)
                                    style.BackColor = Color.Chartreuse;
                                if (style.ForeColor != Color.LightSlateGray)
                                    style.ForeColor = Color.LightSlateGray;
                                //if (style.Font.Name != "新宋体" || style.Font.Bold == false)
                                //{
                                //    style.Font = new Font("新宋体", 8, FontStyle.Regular);
                                //}
                            }

                            if (number.ds == NumberDS.单)
                            {

                            }
                            else if (number.ds == NumberDS.双)
                            {
                                //字体颜色
                                var style = dgview.Rows[index].Cells[DataPropertyName].Style;
                                if (style.ForeColor != Color.Blue)
                                    style.ForeColor = Color.Blue;

                                Rectangle rect = dgview.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false); //得到表格区域
                                                                                                                   //填充背景色
                                SolidBrush b1 = new SolidBrush(e.CellStyle.BackColor);//定义单色画刷　　　　　
                                e.Graphics.FillRectangle(b1, rect);//填充这个矩形
                                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias; //消除锯齿
                                Pen bluePen = new Pen(Color.Blue, 2);   //描边
                                                                        //Rectangle rect1 = new Rectangle(e.CellBounds.Location.X, e.CellBounds.Location.Y, 10, 10);
                                e.Graphics.DrawEllipse(bluePen, rect);
                                //显示
                                var stringFormat = new StringFormat();
                                stringFormat.Alignment = StringAlignment.Center;
                                stringFormat.LineAlignment = StringAlignment.Center;
                                e.Graphics.DrawString(number.ToString(), e.CellStyle.Font, new SolidBrush(e.CellStyle.ForeColor), rect, stringFormat);
                                e.Handled = true;
                            }
                        }
                        break;
                    }
                case cellDataType.龙虎:
                    {
                        int index = e.RowIndex;
                        try
                        {
                            var number = (NumberDragonTiger)dgview.Rows[index].Cells[DataPropertyName].Value;
                            if (number == NumberDragonTiger.龙)
                            {
                                var style = dgview.Rows[index].Cells[DataPropertyName].Style;
                                if (style.BackColor != Color.Red)
                                {
                                    style.BackColor = Color.Red;
                                    style.Font = new Font("新宋体", 8, FontStyle.Bold);
                                }
                                if (style.ForeColor != Color.White)
                                    style.ForeColor = Color.White;
                            }
                            else
                            {
                                var style = dgview.Rows[index].Cells[DataPropertyName].Style;
                                if (style.BackColor != Color.Chartreuse)
                                    style.BackColor = Color.Chartreuse;
                                if (style.ForeColor != Color.LightSlateGray)
                                    style.ForeColor = Color.LightSlateGray;
                            }
                        }
                        catch
                        {
                           
                        }
                        break;
                    }
                default:
                    break;
            }
        }
    }
}
