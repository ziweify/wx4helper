using System;
using System.ComponentModel;

namespace Unit.La.Models
{
    /// <summary>
    /// 浏览器任务信息 - 用于卡片显示的通用模型
    /// 不包含业务逻辑，只包含UI显示所需的数据
    /// </summary>
    public class BrowserTaskInfo : INotifyPropertyChanged
    {
        private int _id;
        private string _name = string.Empty;
        private string _url = string.Empty;
        private string _status = "待启动";
        private bool _isRunning;
        private DateTime _lastRunTime = DateTime.MinValue;
        private object? _tag; // 用于存储项目特定的数据（如 ScriptTask）

        /// <summary>
        /// 任务ID
        /// </summary>
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
        /// 状态描述
        /// </summary>
        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(nameof(Status)); }
        }

        /// <summary>
        /// 是否正在运行
        /// </summary>
        public bool IsRunning
        {
            get => _isRunning;
            set { _isRunning = value; OnPropertyChanged(nameof(IsRunning)); }
        }

        /// <summary>
        /// 最后运行时间
        /// </summary>
        public DateTime LastRunTime
        {
            get => _lastRunTime;
            set { _lastRunTime = value; OnPropertyChanged(nameof(LastRunTime)); }
        }

        /// <summary>
        /// 自定义标签 - 可以存储项目特定的数据对象
        /// 例如：ScriptTask、WorkflowTask 等
        /// </summary>
        public object? Tag
        {
            get => _tag;
            set { _tag = value; OnPropertyChanged(nameof(Tag)); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 创建一个副本
        /// </summary>
        public BrowserTaskInfo Clone()
        {
            return new BrowserTaskInfo
            {
                Id = Id,
                Name = Name,
                Url = Url,
                Status = Status,
                IsRunning = IsRunning,
                LastRunTime = LastRunTime,
                Tag = Tag
            };
        }
    }
}
