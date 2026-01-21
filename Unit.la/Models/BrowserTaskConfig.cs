using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Unit.La.Models
{
    /// <summary>
    /// 浏览器任务配置模型
    /// 通用的配置类，可在任何项目中使用
    /// </summary>
    public class BrowserTaskConfig : INotifyPropertyChanged
    {
        private string _name = "";
        private string _url = "";
        private string _username = "";
        private string _password = "";
        private string _script = "";
        private bool _autoLogin;
        private string? _scriptDirectory;
        private ScriptSourceMode _scriptSourceMode = ScriptSourceMode.Local;

        /// <summary>
        /// 任务名称
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        /// <summary>
        /// 目标 URL
        /// </summary>
        public string Url
        {
            get => _url;
            set
            {
                if (_url != value)
                {
                    _url = value;
                    OnPropertyChanged(nameof(Url));
                }
            }
        }

        /// <summary>
        /// 用户名（用于自动登录）
        /// </summary>
        public string Username
        {
            get => _username;
            set
            {
                if (_username != value)
                {
                    _username = value;
                    OnPropertyChanged(nameof(Username));
                }
            }
        }

        /// <summary>
        /// 密码（用于自动登录）
        /// </summary>
        public string Password
        {
            get => _password;
            set
            {
                if (_password != value)
                {
                    _password = value;
                    OnPropertyChanged(nameof(Password));
                }
            }
        }

        /// <summary>
        /// Lua 脚本内容
        /// </summary>
        public string Script
        {
            get => _script;
            set
            {
                if (_script != value)
                {
                    _script = value;
                    OnPropertyChanged(nameof(Script));
                }
            }
        }

        /// <summary>
        /// 是否自动登录
        /// </summary>
        public bool AutoLogin
        {
            get => _autoLogin;
            set
            {
                if (_autoLogin != value)
                {
                    _autoLogin = value;
                    OnPropertyChanged(nameof(AutoLogin));
                }
            }
        }

        /// <summary>
        /// 脚本目录（本地模式）
        /// </summary>
        public string? ScriptDirectory
        {
            get => _scriptDirectory;
            set
            {
                if (_scriptDirectory != value)
                {
                    _scriptDirectory = value;
                    OnPropertyChanged(nameof(ScriptDirectory));
                }
            }
        }

        /// <summary>
        /// 脚本源模式（本地/远程）
        /// </summary>
        public ScriptSourceMode ScriptSourceMode
        {
            get => _scriptSourceMode;
            set
            {
                if (_scriptSourceMode != value)
                {
                    _scriptSourceMode = value;
                    OnPropertyChanged(nameof(ScriptSourceMode));
                }
            }
        }

        /// <summary>
        /// 自定义数据（扩展字段）
        /// 允许项目添加额外的配置项
        /// </summary>
        public Dictionary<string, string> CustomData { get; set; } = new();

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 最后修改时间
        /// </summary>
        public DateTime LastModifiedTime { get; set; } = DateTime.Now;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            LastModifiedTime = DateTime.Now;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 克隆配置
        /// </summary>
        public BrowserTaskConfig Clone()
        {
            return new BrowserTaskConfig
            {
                Name = Name,
                Url = Url,
                Username = Username,
                Password = Password,
                Script = Script,
                AutoLogin = AutoLogin,
                ScriptDirectory = ScriptDirectory,
                ScriptSourceMode = ScriptSourceMode,
                CustomData = new Dictionary<string, string>(CustomData),
                CreatedTime = CreatedTime,
                LastModifiedTime = LastModifiedTime
            };
        }
    }
}
