using F5BotV2.Main;
using Newtonsoft.Json;
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
    /// <summary>
    ///     确认订单表
    /// </summary>
    public class V2MemberOrderBindlite
        : BindingList<V2MemberOrder>
    {
        private Form _view;
        public class BaseBindlite
        {
            public BaseBindlite(string dbName)
            {
                this.dbName = dbName;

                string fullPath = $"{Environment.CurrentDirectory}\\{this.dbName}";
                _connectName = new SQLiteConnection(fullPath);
                _connectName.CreateTable<V2MemberOrder>();
            }

            public SQLiteConnection _connectName { get; set; }
            public string dbName { get; set; }

            public SQLiteConnection getConnect()
            {
                return _connectName;
            }

            public TableQuery<V2MemberOrder> getTabble()
            {
                var obj = _connectName.Table<V2MemberOrder>();
                return obj;
            }
        }

        public BaseBindlite sql = new BaseBindlite("v2.bat");

        public V2MemberOrderBindlite(Form view)
        {
            this._view = view;
        }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="item"></param>
      /// <param name="FuncFinish"></param>
      /// <param name="loading">true表示加载哦没事, 不记录重复日志</param>
        public new bool Add(V2MemberOrder item, Func<V2MemberOrder, bool>FuncFinish, bool loading = false)
        {
            var fitem = this.FirstOrDefault(p=>p.TimeStampBet == item.TimeStampBet 
            && p.wxid == item.wxid 
            && (int)p.AmountTotal == (int)item.AmountTotal
            && p.BetContentOriginal == item.BetContentOriginal
            );
            if (fitem != null)
            {
                MainConfigure.boterServices.loglite.Add(Log.Create("错误::Order重复::内存表中的订单重复", JsonConvert.SerializeObject(item)));
                return false;
            }

            _view?.Invoke(new Action(() => {
                base.Add(item);
            }));
            
            item.PropertyChanged += Item_PropertyChanged;

            if(loading)
            {
                FuncFinish?.Invoke(item);
                return true;
            }
            //先查找数据库
            if(!loading)
            {
                var table = sql.getTabble();
                var t_item = table.FirstOrDefault(p => p.BetContentOriginal == item.BetContentOriginal 
                && p.TimeStampBet == item.TimeStampBet
                && p.wxid == item.wxid
                && p.BetContentOriginal == item.BetContentOriginal
                );
                if (t_item == null)
                {
                    sql.getConnect().Insert(item);
                    FuncFinish?.Invoke(item);
                    return true;
                }
                else
                {
                    MainConfigure.boterServices.loglite.Add(Log.Create("错误::Order重复::当前插入数据", JsonConvert.SerializeObject(item)));
                    MainConfigure.boterServices.loglite.Add(Log.Create("错误::Order重复::重复的数据", JsonConvert.SerializeObject(t_item)));
                    //MessageBox.Show($"存储失败:已有相同数据,{JsonConvert.SerializeObject(item)}");
                    return false;
                }
            }

            return false;
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
                V2MemberOrder contact = sender as V2MemberOrder;
                var value = contact.GetType().GetProperty(e.PropertyName).GetValue(contact);
                var con = sql.getTabble().FirstOrDefault(p => p.wxid == contact.wxid && p.id == contact.id);
                Type Ts = con.GetType();
                object v = Convert.ChangeType(value, Ts.GetProperty(e.PropertyName).PropertyType);
                Ts.GetProperty(e.PropertyName).SetValue(con, v, null);
                var connect = sql.getConnect();
                connect.Update(con);
                connect.Commit();
            }
            catch (Exception ex)
            {
                //修改数据失败, 做个记录
                MainConfigure.boterServices.loglite.Add(Log.Create("错误::Order属性修改错误", ex.Message));
                //MessageBox.Show($"缓存写入失败::请将信息发送给管理员={ex.Message}");
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

        public bool Remove(V2MemberOrder item)
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
                sql.getConnect().DeleteAll<V2MemberOrder>();
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
