using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace F5BotV2.Controls
{
    public partial class InputBox : Form
        , INotifyPropertyChanged
    {

        private string _GroupWxid = "";
        public string GroupWxid
        {
            get { return _GroupWxid; }
            set
            {
                if (_GroupWxid == value)
                    return;
                _GroupWxid = value;
                NotifyPropertyChanged(() => GroupWxid);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged<T>(Expression<Func<T>> property)
        {
            if (PropertyChanged == null)
                return;

            var memberExpression = property.Body as MemberExpression;
            if (memberExpression == null)
                return;
            
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(memberExpression.Member.Name));
        }


        public InputBox()
        {
            InitializeComponent();
            DialogResult = DialogResult.Cancel;
        }

        private void InputBox_Load(object sender, EventArgs e)
        {
            tbx_group_wxid.DataBindings.Add(new Binding("Text", this, "GroupWxid", false, DataSourceUpdateMode.OnPropertyChanged));
        }

        private void btn_ok_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void btn_cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }


    }
}
