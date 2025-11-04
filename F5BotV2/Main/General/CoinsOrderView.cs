using CCWin;
using CCWin.SkinControl;
using CsQuery.HtmlParser;
using F5BotV2.Boter;
using F5BotV2.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Forms;
using System.Xml.Linq;

namespace F5BotV2.Main
{
    public partial class CoinsOrderView : CCSkinMain
    {
        private V2MemberCoinsBuySellBindlite _coinsBindlite;
        private BoterServices boter;
        public CoinsOrderView(BoterServices boter, V2MemberCoinsBuySellBindlite coinsBindlite)
        {
            InitializeComponent();
            this.boter = boter;
            this._coinsBindlite = coinsBindlite;
        }

        private void CoinsOrder_Load(object sender, EventArgs e)
        {
            CoinsDataGridViewInit(this.dgv_coins_order);
        }

        private void CoinsDataGridViewInit(SkinDataGridView dgv)
        {
            dgv.DataSource = _coinsBindlite;
            dgv.AllowUserToAddRows = false;
            dgv.ReadOnly = true;
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
            //右键DataGridView->编辑列->添加列->选择列的DataPropertyName属性，在此属性上指定需要绑定的列名，通过HeaderText设置控件上的标题。
            //比如，我需要绑定"Address"属性，那么在列的DataPropertyName上设置为"Address"就行了，HeaderText设置为"地址"

            var cell = dgv.Columns["id"]; cell.Visible = true; cell.Width = 46;
            cell = dgv.Columns["nickname"]; cell.Visible = true; cell.Width = 80;
            cell = dgv.Columns["PayAction"]; cell.Visible = true; cell.Width = 46;
            cell = dgv.Columns["Money"]; cell.Visible = true; cell.Width = 80;
            cell = dgv.Columns["Timestring"]; cell.Visible = true; cell.Width = 120;
            cell = dgv.Columns["PayStatus"]; cell.Visible = true; cell.Width = 90;



            dgv.Columns["wxid"].Visible = false;
            dgv.Columns["account"].Visible = false;
            dgv.Columns["avatar"].Visible = false;
            dgv.Columns["city"].Visible = false;
            dgv.Columns["country"].Visible = false;
            dgv.Columns["province"].Visible = false;
            dgv.Columns["remark"].Visible = false;
            dgv.Columns["sex"].Visible = false;
            dgv.Columns["Timestamp"].Visible = false;
            dgv.Columns["GroupWxId"].Visible = false;
        }

        private void CoinsOrder_FormClosing(object sender, FormClosingEventArgs e)
        {
            //调试的时候, 禁用这个。
            e.Cancel = true;
            this.Hide();
        }

        private void dgv_coins_order_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if(e.RowIndex > -1)
            {
                var column = dgv_coins_order.Columns[e.ColumnIndex];
                var rowIndex = e.RowIndex;
                var cellName = column.DataPropertyName;
                if (column != null)
                {
                    switch(column.DataPropertyName)
                    {
                        case "PayAction":
                            {
                                var data = (V2MemberPayAction)this.dgv_coins_order.Rows[rowIndex].Cells[cellName].Value;
                                if(data == V2MemberPayAction.上分)
                                {
                                    dgv_coins_order.Rows[rowIndex].Cells[cellName].Style.BackColor = Color.Green;   //设置单元格颜色
                                    dgv_coins_order.Rows[rowIndex].Cells[cellName].Style.ForeColor = Color.Black;
                                }
                                else if(data == V2MemberPayAction.下分)
                                {
                                    dgv_coins_order.Rows[rowIndex].Cells[cellName].Style.BackColor = Color.Red;   //设置单元格颜色
                                    dgv_coins_order.Rows[rowIndex].Cells[cellName].Style.ForeColor = Color.White;
                                }
                                break;
                            }
                        case "PayStatus":
                            {
                                var data = (V2MemberPayStatus)this.dgv_coins_order.Rows[rowIndex].Cells[cellName].Value;
                                if (data == V2MemberPayStatus.等待处理)
                                {
                                    dgv_coins_order.Rows[rowIndex].Cells[cellName].Style.BackColor = Color.Red;   //设置单元格颜色
                                    dgv_coins_order.Rows[rowIndex].Cells[cellName].Style.ForeColor = Color.White;
                                }
                                else if (data == V2MemberPayStatus.同意)
                                {
                                    dgv_coins_order.Rows[rowIndex].Cells[cellName].Style.BackColor = Color.Green;   //设置单元格颜色
                                    dgv_coins_order.Rows[rowIndex].Cells[cellName].Style.ForeColor = Color.White;
                                }
                                else if(data == V2MemberPayStatus.忽略)
                                {
                                    dgv_coins_order.Rows[rowIndex].Cells[cellName].Style.BackColor = Color.LightGray;   //设置单元格颜色
                                    dgv_coins_order.Rows[rowIndex].Cells[cellName].Style.ForeColor = Color.Black;
                                }
                                break;
                            }
                        case "Money":
                            {
                                //百元:白色
                                //千元:绿色
                                //万元:橙色
                                int data = (int)this.dgv_coins_order.Rows[rowIndex].Cells[cellName].Value;
                                if(data > 0)
                                {
                                    if (data >= 10000)
                                    {
                                        dgv_coins_order.Rows[rowIndex].Cells[cellName].Style.BackColor = Color.Orange;   //设置单元格颜色
                                        dgv_coins_order.Rows[rowIndex].Cells[cellName].Style.ForeColor = Color.Black;
                                    }
                                    else if (data >= 1000)
                                    { //说明是千
                                        dgv_coins_order.Rows[rowIndex].Cells[cellName].Style.BackColor = Color.Green;   //设置单元格颜色
                                        dgv_coins_order.Rows[rowIndex].Cells[cellName].Style.ForeColor = Color.Black;
                                    }
                                    else if (data >= 100)
                                    {
                                        dgv_coins_order.Rows[rowIndex].Cells[cellName].Style.BackColor = Color.LightGray;   //设置单元格颜色
                                        dgv_coins_order.Rows[rowIndex].Cells[cellName].Style.ForeColor = Color.Black;
                                    }
                                }

                                break;
                            }
                        default:
                            break;
                    }
                }
            }
        }

        private void dgviewSetCellStyle(DataGridViewCellPaintingEventArgs e, int rowIndex, string cellName)
        {

        }

        private void 同意ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(dgv_coins_order.CurrentRow != null)
            {
                int index = dgv_coins_order.CurrentRow.Index;
                if (index >= 0)
                {
                    var order = dgv_coins_order.CurrentRow.DataBoundItem as V2MemberCoinsBuySell;

                    try
                    {
                        if (order.PayAction == V2MemberPayAction.上分)
                            boter.OnActionMemberCredit(order);
                        else if (order.PayAction == V2MemberPayAction.下分)
                            boter.OnActionMemberWithdraw(order);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
        }

        private void 忽略ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dgv_coins_order.CurrentRow != null)
            {
                int index = dgv_coins_order.CurrentRow.Index;
                if (index >= 0)
                {
                    var order = dgv_coins_order.CurrentRow.DataBoundItem as V2MemberCoinsBuySell;
                    try
                    {
                        if (order.PayStatus != V2MemberPayStatus.等待处理)
                            throw new Exception("不允许重复处理!");
                        order.PayStatus = V2MemberPayStatus.忽略;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
        }

        private void 刷新ToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void dgv_coins_order_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0)
                {
                    if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right)
                    {
                        var ss = dgv_coins_order.CurrentCell.RowIndex;
                        if (ss != e.RowIndex)
                        {
                            dgv_coins_order.ClearSelection();
                            dgv_coins_order.CurrentCell = dgv_coins_order.Rows[e.RowIndex].Cells[e.ColumnIndex];
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("选择异常! \r\n请左键点击再次选择后, 再操作!");
            }
        }
    }
}
