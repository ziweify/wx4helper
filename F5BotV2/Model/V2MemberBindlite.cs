using F5BotV2.Wx;
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
    public class V2MemberBindlite
        : BindingList<V2Member>
    {
        public class BaseBindlite
        {
            public BaseBindlite(string dbName)
            {
                this.dbName = dbName;

                string fullPath = $"{Environment.CurrentDirectory}\\{this.dbName}";
                _connectName = new SQLiteConnection(fullPath);
                _connectName.CreateTable<V2Member>();
            }

            public SQLiteConnection _connectName { get; set; }
            public string dbName { get; set; }

            public SQLiteConnection getConnect()
            {
                return _connectName;
            }

            public TableQuery<V2Member> getTabble()
            {
                var obj = _connectName.Table<V2Member>();
                return obj;
            }
        }

        private BaseBindlite _sql = new BaseBindlite("v2.bat");
         public BaseBindlite sql { get { return _sql; } }

        public V2MemberBindlite()
        {

        }

        public new void Add(V2Member item)
        {
            throw new Exception("禁止直接调用! 没有群ID!");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="group_wxid"></param>
        /// <param name="item"></param>
        /// <param name="isContacts">isContacts 是否属于联系人,是否属于好友</param>
        public new void Add(string group_wxid, V2Member item, Func<V2Member, MemBerState> getMemberState)
        {
            //写入组iD
            item.GroupWxId = group_wxid;

            //查询列表.排除重复添加
            var fitem = this.FirstOrDefault(p => p.wxid == item.wxid);
            if (fitem == null)
            {
                fitem = item;
                base.Add(fitem);
                fitem.PropertyChanged += Item_PropertyChanged;
            }

            //插入数据库.检查是否有这个数据
            var con = _sql.getTabble().FirstOrDefault(p => p.GroupWxId == fitem.GroupWxId &&
            p.wxid == fitem.wxid);
            if (con == null)
            {
                _sql.getConnect().Insert(fitem);
            }
            else
            {
                if (fitem.id == 0)
                    fitem.id = con.id;

                if (fitem.State != con.State)
                    fitem.State = con.State;

                if (fitem.nickname != con.nickname)
                {
                    //更新, 并且要做日志
                    fitem.NotifyPropertyChanged(() => "nickname");
                }

                if(fitem.display_name != con.display_name)
                {
                    fitem.NotifyPropertyChanged(() => fitem.display_name);
                }

                if (fitem.account != con.account)
                {
                    //更新.并且要做日志
                }

                //账单数据
                if (fitem.Balance != con.Balance)
                    fitem.Balance = con.Balance;

                if (fitem.BetToday != con.BetToday)
                    fitem.BetToday = con.BetToday;

                if (fitem.BetTotal != con.BetTotal)
                    fitem.BetTotal = con.BetTotal;

                if (fitem.CreditToday != con.CreditToday)
                    fitem.CreditToday = con.CreditToday;

                if (fitem.CreditTotal != con.CreditTotal)
                    fitem.CreditTotal = con.CreditTotal;

                if (fitem.IncomeTotal != con.IncomeTotal)
                    fitem.IncomeTotal = con.IncomeTotal;

                if (fitem.IncomeToday != con.IncomeToday)
                    fitem.IncomeToday = con.IncomeToday;

                if (fitem.WithdrawToday != con.WithdrawToday)
                    fitem.WithdrawToday = con.WithdrawToday;

                if (fitem.WithdrawTotal != con.WithdrawTotal)
                    fitem.WithdrawTotal = con.WithdrawTotal;
            }

            ////处理会员类型数据
            if (getMemberState != null)
            {
                var memberState = getMemberState.Invoke(fitem);
                if(memberState != fitem.State)
                {
                    if(fitem.State != MemBerState.管理
                        && fitem.State != MemBerState.托)
                    {
                        fitem.State = memberState;
                    }
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
                V2Member contact = sender as V2Member;
                var value = contact.GetType().GetProperty(e.PropertyName).GetValue(contact);
                var con = _sql.getTabble().FirstOrDefault(p => p.wxid == contact.wxid && p.id == contact.id);
                Type Ts = con.GetType();
                object v = Convert.ChangeType(value, Ts.GetProperty(e.PropertyName).PropertyType);
                Ts.GetProperty(e.PropertyName).SetValue(con, v, null);
                //_sql.getConnect().Update(con);

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
            var con = _sql.getTabble().ToList();


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

        public bool Remove(V2Member item)
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
