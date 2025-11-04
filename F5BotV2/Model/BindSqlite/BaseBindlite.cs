using SQLite; 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F5BotV2.Model.BindSqlite
{
    interface IBindSqliteBase
    {
        /// <summary>
        ///     数据连接名，数据实例名
        /// </summary>
         SQLiteConnection _connectName { get; set; }

        /// <summary>
        ///     数据库名
        /// </summary>
         string dbName { get; set; }

        TableQuery<T> getTabble<T>();

        SQLiteConnection getConnect();
    }



    //public class BaseBindlite<T> 
    //    : IBindSqliteBase
    //{
    //    public BaseBindlite(string dbName)
    //    {
    //        this.dbName = dbName;

    //        string fullPath = $"{Environment.CurrentDirectory}\\{this.dbName}";
    //        _connectName = new SQLiteConnection(fullPath);
    //        _connectName.CreateTable<T>();
    //    }

    //    public SQLiteConnection _connectName { get; set; }
    //    public string dbName { get; set; }

    //    public SQLiteConnection getConnect()
    //    {
    //        return _connectName;
    //    }

    //    public TableQuery<T> getTabble() 
    //    {
    //        var obj = _connectName.Table<T>();
    //        return obj;
    //    }
    //}



}
