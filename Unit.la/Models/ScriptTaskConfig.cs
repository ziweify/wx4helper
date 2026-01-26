using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Unit.La.Models
{
    /// <summary>
    /// 脚本任务配置模型
    /// 通用的配置类，可在任何项目中使用
    /// </summary>
    public class ScriptTaskConfig : INotifyPropertyChanged
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
        /// Lua 脚本内容（运行时使用，不保存到 JSON）
        /// 🔥 注意：脚本内容不保存到 JSON，只保存 ScriptDirectory 路径
        /// </summary>
        [JsonIgnore]
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
        public ScriptTaskConfig Clone()
        {
            return new ScriptTaskConfig
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

        #region JSON 序列化/反序列化

        /// <summary>
        /// 序列化为 JSON 字符串
        /// </summary>
        public string ToJson(bool indented = true)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = indented,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            return JsonSerializer.Serialize(this, options);
        }

        /// <summary>
        /// 从 JSON 字符串反序列化
        /// </summary>
        public static ScriptTaskConfig? FromJson(string json)
        {
            try
            {
                return JsonSerializer.Deserialize<ScriptTaskConfig>(json);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 保存到 JSON 文件
        /// </summary>
        public void SaveToFile(string filePath)
        {
            try
            {
                var directory = System.IO.Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !System.IO.Directory.Exists(directory))
                {
                    System.IO.Directory.CreateDirectory(directory);
                }

                var json = ToJson();
                System.IO.File.WriteAllText(filePath, json, System.Text.Encoding.UTF8);
            }
            catch (Exception ex)
            {
                throw new Exception($"保存配置到文件失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 从 JSON 文件加载
        /// </summary>
        public static ScriptTaskConfig? LoadFromFile(string filePath)
        {
            try
            {
                if (!System.IO.File.Exists(filePath))
                    return null;

                var json = System.IO.File.ReadAllText(filePath, System.Text.Encoding.UTF8);
                return FromJson(json);
            }
            catch (Exception ex)
            {
                throw new Exception($"从文件加载配置失败: {ex.Message}", ex);
            }
        }

        #endregion
    }
}
