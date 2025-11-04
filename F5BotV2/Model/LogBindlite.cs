using F5BotV2.Main;
using F5BotV2.Wx;
using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace F5BotV2.Model
{
    public class LogBindlite
        : BindingList<Log>
    {
        private Form _view;
        public class BaseBindlite
        {
            public BaseBindlite(string dbName)
            {
                this.dbName = dbName;

                string fullPath = $"{Environment.CurrentDirectory}\\{this.dbName}";
                _connectName = new SQLiteConnection(fullPath);
                _connectName.CreateTable<Log>();
            }

            public SQLiteConnection _connectName { get; set; }
            public string dbName { get; set; }

            public SQLiteConnection getConnect()
            {
                return _connectName;
            }

            public TableQuery<Log> getTabble()
            {
                var obj = _connectName.Table<Log>();
                return obj;
            }
        }

        public BaseBindlite sql = new BaseBindlite("v2.bat");

        public LogBindlite(Form view)
        {
            this._view = view;
        }


        /// <summary>
        ///     添加数据
        /// </summary>
        /// <param name="item"></param>
        public new void Add(Log item)
        {
            try
            {
                _view?.BeginInvoke(new Action(() => {
                    try
                    {
                        base.Add(item);
                    }
                    catch(Exception ex)
                    {
                        Debug.WriteLine("Add::Error1::" + ex.Message);
                    }
                    
                }));
            }
            catch(Exception ex)
            {
                Debug.WriteLine("Add::Error2::"+ex.Message);
            }

            try
            {
                sql.getConnect().Insert(item);
            }
            catch(Exception ex)
            {
                Debug.WriteLine("Add::Error3::" + ex.Message);
            }
            
        }


        /// <summary>
        ///     item数据变动, 自动更改到数据库中
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //try
            //{
            //    Log data = sender as Log;
            //    var value = data.GetType().GetProperty(e.PropertyName).GetValue(data);

            //}
            //catch (Exception ex)
            //{
            //    //修改数据失败, 做个记录
            //}
        }
    }
}
