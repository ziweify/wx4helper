using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace F5BotV2.Model
{
    public class WxGroupBindlite
        : BindingList<WxGroup>
    {
        class BaseBindlite
        {
            public BaseBindlite(string dbName)
            {
                this.dbName = dbName;

                string fullPath = $"{Environment.CurrentDirectory}\\{this.dbName}";
                _connectName = new SQLiteConnection(fullPath);
                _connectName.CreateTable<WxGroup>();
            }

            public SQLiteConnection _connectName { get; set; }
            public string dbName { get; set; }

            public SQLiteConnection getConnect()
            {
                return _connectName;
            }

            public TableQuery<WxGroup> getTabble()
            {
                var obj = _connectName.Table<WxGroup>();
                return obj;
            }
        }

        private BaseBindlite sql = new BaseBindlite("v2.bat");

        public WxGroupBindlite()
        {

        }

        public new void Add(WxGroup item)
        {
            var fitem = this.FirstOrDefault(p => p.wxid == item.wxid);
            if (fitem != null)
                return;

            base.Add(item);
            item.PropertyChanged += Item_PropertyChanged;

            //先查找数据库
            var table = sql.getTabble();
            var t_item = table.FirstOrDefault(p => p.wxid == item.wxid
            && p.WeMainId == item.WeMainId
            );

            if (t_item == null)
            {
                sql.getConnect().Insert(item);
            }
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //属性修改
            //得到属性值, 写入到数据库中
            //string dbpath = $"{Environment.CurrentDirectory}\\MyData.db";
            //WxMessageContact contact = sender as WxMessageContact;
            //var value = contact.GetType().GetProperty(e.PropertyName).GetValue(contact);
            //using (var db = new MyMessageContact(dbpath))
            //{
            //    var con = db.contact.FirstOrDefault(p => p.GetType().GetProperty(e.PropertyName).GetValue(p)  == value);
            //    if (con != null)
            //    {
            //        int count = db.Update(con);
            //        string log = $"{DateTime.Now}, 修改{count}条记录";
            //    }
            //}
            try
            {
                WxGroup contact = sender as WxGroup;
                var value = contact.GetType().GetProperty(e.PropertyName).GetValue(contact);
                var con = sql.getTabble().FirstOrDefault(p => p.wxid == contact.wxid);
                Type Ts = con.GetType();
                object v = Convert.ChangeType(value, Ts.GetProperty(e.PropertyName).PropertyType);
                Ts.GetProperty(e.PropertyName).SetValue(con, v, null);
                sql.getConnect().Update(con);
            }
            catch (Exception ex)
            {
                //修改数据失败, 做个记录
                MessageBox.Show($"缓存写入失败::请将信息发送给管理员={ex.Message}");
            }

            //属性变更, 更新数据库
            //e.PropertyName 属性名
            //value 属性名对应的值
        }

        public void Select()
        {
            var con = sql.getTabble().ToList();


            //string dbpath = $"{Environment.CurrentDirectory}\\MyData.db";
            //WxMessageContact contact = sender as WxMessageContact;
            //var value = contact.GetType().GetProperty(e.PropertyName).GetValue(contact);
            //using (var db = new MyMessageContact(dbpath))
            //{
            //    //下面就是查询语句
            //    var con = db.contact.FirstOrDefault(p => p.GetType().GetProperty(e.PropertyName).GetValue(p) == value);
            //    if (con != null)
            //    {
            //        //return con; //返回查询结果
            //    }
            //}
        }

        public bool Remove(WxGroup item)
        {
            bool bresult = base.Remove(item);

            //在这里下面执行数据库删除操作, 假删除


            return bresult;
        }
        public new void RemoveAt(int index)
        {
            base.RemoveAt(index);

            //在这下面执行删除数据库操作
        }

        public new void Clear()
        {
            base.Clear();

            //在这下面执行数据库清理操作

        }
    }
}
