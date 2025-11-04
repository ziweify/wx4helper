using CCWin;
using CCWin.SkinControl;
using F5BotV2.Model;
using F5BotV2.Model.BindSqlite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace F5BotV2.Main
{

    
    public partial class LogView : CCSkinMain
    {
        private LogBindlite loglite;
        public LogView()
        {
            InitializeComponent();
        }

        /// <summary>
        ///     传递绑定的数据库参数进来
        /// </summary>
        public LogView(LogBindlite loglite)
        {
            InitializeComponent();
            this.loglite = loglite;

            //绑定datagrid
           // dgvLog.DataSource = loglite;
        }

        private void LogView_Load(object sender, EventArgs e)
        {
            CoinsDataGridViewInit(dgvLog);
        }

        private void CoinsDataGridViewInit(SkinDataGridView dgv)
        {
            dgv.DataSource = loglite;
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
            cell = dgv.Columns["User"]; cell.Visible = true; cell.Width = 80;
            cell = dgv.Columns["Action"]; cell.Visible = true; cell.Width = 200;
            cell = dgv.Columns["Context"]; cell.Visible = true; cell.Width = 650;
            cell = dgv.Columns["AtTimestamp"]; cell.Visible = false; 


            cell = dgv.Columns["ustate"]; cell.Visible = false;
            cell = dgv.Columns["uTime"]; cell.Visible = false;
            cell = dgv.Columns["AtTimeString"]; cell.Visible = true; cell.Width = 120;

        }

        private void LogView_FormClosing(object sender, FormClosingEventArgs e)
        {
            //取消关闭操作
            e.Cancel = true;
            this.Hide();
        }
    }
}
