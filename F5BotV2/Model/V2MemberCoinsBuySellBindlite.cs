using F5BotV2.Main;
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
    public class V2MemberCoinsBuySellBindlite
        : BindingList<V2MemberCoinsBuySell>
    {
        private Form _view;
        private Func<V2MemberCoinsBuySellBindlite, V2MemberCoinsBuySell, bool> funcUpdata;

        public class BaseBindlite
        {
            public BaseBindlite(string dbName)
            {
                this.dbName = dbName;

                string fullPath = $"{Environment.CurrentDirectory}\\{this.dbName}";
                _connectName = new SQLiteConnection(fullPath);
                _connectName.CreateTable<V2MemberCoinsBuySell>();
            }

            public SQLiteConnection _connectName { get; set; }
            public string dbName { get; set; }

            public SQLiteConnection getConnect()
            {
                return _connectName;
            }

            public TableQuery<V2MemberCoinsBuySell> getTabble()
            {
                var obj = _connectName.Table<V2MemberCoinsBuySell>();
                return obj;
            }
        }

        public BaseBindlite sql = new BaseBindlite("v2.bat");

        public V2MemberCoinsBuySellBindlite()
        {
            
        }

        public V2MemberCoinsBuySellBindlite(Form view, Func<V2MemberCoinsBuySellBindlite, V2MemberCoinsBuySell, bool> func)
        {
            this._view = view;
            this.funcUpdata = func;
        }

        /// <summary>
        ///     订单插入
        /// </summary>
        /// <param name="item"></param>
        /// <param name="loading"></param>
        public new void Add(V2MemberCoinsBuySell item,  bool loading = false)
        {
            //不允许添加重复订单
            var fitem = this.FirstOrDefault(p=>p.wxid == item.wxid && p.Timestamp == item.Timestamp && p.Money == item.Money);
            if (fitem != null)
                return;

            _view?.Invoke(new Action(() => {
                base.Add(item);
            }));
            
            item.PropertyChanged += Item_PropertyChanged;
            if (loading)
            {
                funcUpdata?.Invoke(this, item);
                return;
            }
            //先查找数据库
            if (!loading)
            {
                var table = sql.getTabble();
                var t_item = table.FirstOrDefault(p => p.wxid == item.wxid && p.Timestamp == item.Timestamp && p.Money == item.Money);
                if (t_item == null)
                {
                    sql.getConnect().Insert(item);
                    try
                    {
                        funcUpdata?.Invoke(this, item);  //订单成功回调. 通知给view处理，上下分动作
                    }
                    catch { }
                }
                else
                {
                    //错误日志
                }
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
                V2MemberCoinsBuySell contact = sender as V2MemberCoinsBuySell;
                var value = contact.GetType().GetProperty(e.PropertyName).GetValue(contact);
                var con = sql.getTabble().FirstOrDefault(p => p.wxid == contact.wxid && p.id == contact.id);
                Type Ts = con.GetType();
                object v = Convert.ChangeType(value, Ts.GetProperty(e.PropertyName).PropertyType);
                Ts.GetProperty(e.PropertyName).SetValue(con, v, null);
                sql.getConnect().Update(con);

                if(e.PropertyName == "PayStatus")
                {
                    //调用变更函数
                    try
                    {
                        funcUpdata?.Invoke(this, contact);
                    }
                    catch { }

                }
                var connect = sql.getConnect();
                connect.Update(con);
                connect.Commit();
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

        public bool Remove(V2MemberCoinsBuySell item)
        {
            bool bresult = base.Remove(item);

            //在这里下面执行数据库删除操作, 假删除
            sql.getConnect().Delete(item);

            return bresult;
        }
        public new void RemoveAt(int index)
        {
            base.RemoveAt(index);
            sql.getConnect().Delete(index);
            //在这下面执行删除数据库操作
        }

        public bool RemoveAll()
        {
            try
            {
                sql.getConnect().DeleteAll<V2MemberCoinsBuySell>();

            }
            catch { return false; }
            
            return true;
        }

        public new void Clear()
        {
            base.Clear();

            //在这下面执行数据库清理操作

        }
    }
}
