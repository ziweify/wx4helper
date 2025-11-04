using F5BotV2.Model;
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
    public partial class XWxGroupView : Form
    {
        private WxGroupBindlite items = null;
        public WxGroup WxGroup = null;
        public XWxGroupView()
        {
            InitializeComponent();
        }

        public XWxGroupView(WxGroupBindlite items)
        {
            InitializeComponent();
            this.items = items;
            if (this.items == null)
            {
                this.items = new WxGroupBindlite();
                this.Text = "没有任何群组数据!请重试!";
            }
            else
            {
                //绑定数据
                this.InitWxGroupsView(dgv_WxContacts);  //联系人,群
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            //开启服务
            //得到列表索引
            try
            {
                int index = dgv_WxContacts.CurrentRow.Index;
                if (index >= 0)
                {
                    WxGroup = dgv_WxContacts.CurrentRow.DataBoundItem as WxGroup;
                    DialogResult = DialogResult.OK;
                }
                else
                {
                    DialogResult = DialogResult.Abort;
                }
                
                //MainConfigure.boterServices.userServicesBegin(wxgroup, callback_RunningStatus);
            }
            catch
            {
                MessageBox.Show("绑定错误! 请重新选择后再次绑定!");
            }
        }

        private void dgv_WxContacts_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0)
                {
                    if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right)
                    {
                        var ss = dgv_WxContacts.CurrentCell.RowIndex;
                        if (ss != e.RowIndex)
                        {
                            dgv_WxContacts.ClearSelection();
                            dgv_WxContacts.CurrentCell = dgv_WxContacts.Rows[e.RowIndex].Cells[e.ColumnIndex];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("选择异常! \r\n请左键点击再次选择后, 再操作!");
            }
        }
    }
}
