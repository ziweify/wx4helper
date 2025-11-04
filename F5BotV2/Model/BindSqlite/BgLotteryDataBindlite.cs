using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F5BotV2.Model.BindSqlite
{
    /// <summary>
    ///     绑定并自动修改的
    /// </summary>
    public class BgLotteryDataBindlite
        : BindingList<BgLotteryData>
    {
        class BaseBindlite1
        {
            public BaseBindlite1(string dbName)
            {
                this.dbName = dbName;

                string fullPath = $"{Environment.CurrentDirectory}\\{this.dbName}";
                _connectName = new SQLiteConnection(fullPath);
                _connectName.CreateTable<BgLotteryData>();
            }

            public SQLiteConnection _connectName { get; set; }
            public string dbName { get; set; }

            public SQLiteConnection getConnect()
            {
                return _connectName;
            }

            public TableQuery<BgLotteryData> getTabble()
            {
                var obj = _connectName.Table<BgLotteryData>();
                return obj;
            }
        }

        //异常了
        private BaseBindlite1 sql = new BaseBindlite1("v2.bat");

        public BgLotteryDataBindlite()
        {

        }


        /// <summary>
        ///     添加数据
        /// </summary>
        /// <param name="item"></param>
        public new void Add(BgLotteryData item)
        {
            //查询列表, 查询主键, 排除重复添加
            var fitem = this.FirstOrDefault(p=>p.IssueId == item.IssueId);
            if(fitem == null)
            {
                base.Add(item);
                //item.PropertyChanged += Item_PropertyChanged;
            }

            //插入数据库, 检查是否有这个数据
            //var con = sqlite.getTabble().FirstOrDefault(p=>p.IssueId == item.IssueId);

            var con = sql.getTabble().FirstOrDefault(p => p.IssueId == item.IssueId);
            if(con == null)
            {
                sql.getConnect().Insert(item);
            }
        }


        /// <summary>
        ///     item数据变动, 自动更改到数据库中
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        //private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        //{
        //    try
        //    {
        //        BgLotteryData data = sender as BgLotteryData;
        //        var value = data.GetType().GetProperty(e.PropertyName).GetValue(data);

        //    }
        //    catch(Exception ex)
        //    {
        //        //修改数据失败, 做个记录
        //    }
        //}
    }
}
