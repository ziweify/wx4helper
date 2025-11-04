using CCWin.SkinClass;
using F5BotV2.Main;
using LxLib.LxSys;
using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace F5BotV2.Model
{

    public interface ILog
    {
        //Log Create(string User, string Action, string Context);
        int id { get; set; }
        string User { get; set; }
        string Action { get; set; }
        string Context { get; set; }
        int AtTimestamp { get; set; }
        string AtTimeString { get; set; }
        int ustate { get; set; }
        int uTime { get; set; }
    }

    public class Log
        : INotifyPropertyChanged
        , ILog
    {
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

        public static Log Create(string Action, string Context)
        {
            Log log = new Log();
            log.User = MainConfigure.appLoginSetting.User;
            log.Action = Action;
            log.Context = Context;
            log.ustate = 0;
            try
            {
                log.AtTimestamp = Conversion.ToInt32(LxTimestampHelper.GetTimeStamp());
                log.AtTimeString = DateTime.Now.ToString();
            }
            catch
            {
                log.AtTimestamp = -1;
            }

            return log;
        }

        //public static Log Create(string User, string Action, string Context)
        //{
        //    Log log = new Log();
        //    log.User = User;
        //    log.Action = Action;
        //    log.Context = Context;
        //    log.ustate = 0;
        //    try
        //    {
        //        log.AtTimestamp = Conversion.ToInt32(LxTimestampHelper.GetTimeStamp());
        //        log.AtTimeString = DateTime.Now.ToString();
        //    }
        //    catch
        //    {
        //        log.AtTimestamp = -1;
        //    }
            
        //    return log;
        //}

        private int _id;
        [PrimaryKey, AutoIncrement]
        public int id
        {
            get { return _id; }
            set
            {
                if (_id == value)
                    return;
                _id = value;
                NotifyPropertyChanged(() => id);
            }
        }

        private string _User;
        public string User
        {
            get { return _User; }
            set
            {
                if (_User == value)
                    return;
                _User = value;
                NotifyPropertyChanged(() => User);
            }
        }


        /// <summary>
        ///     消息动作，日志类型
        /// </summary>
        private string _Action;
        public string Action
        {
            get { return _Action; }
            set
            {
                if (_Action == value)
                    return;
                _Action = value;
                NotifyPropertyChanged(() => Action);
            }
        }


        /// <summary>
        ///     消息内容,消息全文
        /// </summary>
        private string _Context;
        public string Context
        {
            get { return _Context; }
            set
            {
                if (_Context == value)
                    return;
                _Context = value;
                NotifyPropertyChanged(() => Context);
            }
        }


        /// <summary>
        ///     消息接受时间戳, 或者入库时间
        /// </summary>
        private int _AtTimestamp;
        public int AtTimestamp
        {
            get { return _AtTimestamp; }
            set
            {
                if (_AtTimestamp == value)
                    return;
                _AtTimestamp = value;
                NotifyPropertyChanged(() => AtTimestamp);
            }
        }

        /// <summary>
        ///     上传,是否上传, 未上传是0
        /// </summary>
        private int _ustate;
        public int ustate
        {
            get { return _ustate; }
            set
            {
                if (_ustate == value)
                    return;
                _ustate = value;
                NotifyPropertyChanged(() => ustate);
            }
        }


        /// <summary>
        ///     上传时间
        /// </summary>
        private int _uTime;
        public int uTime
        {
            get { return _uTime; }
            set
            {
                if (_uTime == value)
                    return;
                _uTime = value;
                NotifyPropertyChanged(() => uTime);
            }
        }

        private string _AtTimeString;
        public string AtTimeString {
            get { return _AtTimeString; }
            set
            {
                if (_AtTimeString == value)
                    return;
                _AtTimeString = value;
                NotifyPropertyChanged(() => AtTimeString);
            }
        }
    }

    //public class LogV2
    //    : ILog
    //{
    //    private Log log;
    //    public LogV2(Log log)
    //    {
    //        this.log = log;
    //    }

    //    public int id { get => log.id; set => log.id = value; }
    //    public string User { get => log.User; set => log.User = value; }
    //    public string Action { get => log.Action; set => log.Action = value; }
    //    public string Context { get => log.Context; set => log.Context = value; }
    //    public int AtTimestamp { get => log.AtTimestamp; set => log.AtTimestamp = value; }
    //    public int ustate { get => log.ustate; set => log.ustate = value; }
    //    public int uTime { get => log.uTime; set => log.uTime = value; }

    //    //public Log Create(string User, string Action, string Context)
    //    //{
    //    //    return Log.Create(User, Action, Context);
    //    //}

    //    public Log Create(string Action, string Context)
    //    {
    //        return Log.Create(User, Action, Context);
    //    }
    //}
}
