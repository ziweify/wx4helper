using System;
using System.ComponentModel;
using SQLite;

namespace YongLiSystem.Models.Dashboard
{
    /// <summary>
    /// 脚本任务模型 - 用于定制采集任务
    /// </summary>
    [Table("script_tasks")]
    public class ScriptTask : INotifyPropertyChanged
    {
        private int _id;
        private string _name = string.Empty;
        private string _url = string.Empty;
        private string _username = string.Empty;
        private string _password = string.Empty;
        private bool _autoLogin;
        private string _script = string.Empty;
        private bool _isRunning;
        private string _status = "待启动";
        private DateTime _createdTime;
        private DateTime _lastRunTime;

        /// <summary>
        /// 数据库ID
        /// </summary>
        [PrimaryKey, AutoIncrement]
        public int Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(nameof(Id)); }
        }

        /// <summary>
        /// 任务名称
        /// </summary>
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        /// <summary>
        /// 网址
        /// </summary>
        public string Url
        {
            get => _url;
            set { _url = value; OnPropertyChanged(nameof(Url)); }
        }

        /// <summary>
        /// 用户名
        /// </summary>
        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(nameof(Username)); }
        }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(nameof(Password)); }
        }

        /// <summary>
        /// 是否自动登录
        /// </summary>
        public bool AutoLogin
        {
            get => _autoLogin;
            set { _autoLogin = value; OnPropertyChanged(nameof(AutoLogin)); }
        }

        /// <summary>
        /// 脚本内容
        /// </summary>
        public string Script
        {
            get => _script;
            set { _script = value; OnPropertyChanged(nameof(Script)); }
        }

        /// <summary>
        /// 是否正在运行
        /// </summary>
        [Ignore] // 不保存到数据库，运行时状态
        public bool IsRunning
        {
            get => _isRunning;
            set { _isRunning = value; OnPropertyChanged(nameof(IsRunning)); }
        }

        /// <summary>
        /// 状态描述
        /// </summary>
        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(nameof(Status)); }
        }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedTime
        {
            get => _createdTime;
            set { _createdTime = value; OnPropertyChanged(nameof(CreatedTime)); }
        }

        /// <summary>
        /// 最后运行时间
        /// </summary>
        public DateTime LastRunTime
        {
            get => _lastRunTime;
            set { _lastRunTime = value; OnPropertyChanged(nameof(LastRunTime)); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
