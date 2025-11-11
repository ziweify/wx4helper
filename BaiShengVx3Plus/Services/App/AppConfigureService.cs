//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace BaiShengVx3Plus.Services.App
//{
//    /// <summary>
//    ///     程序配置服务, 
//    /// </summary>
//    public class AppConfigureService
//        : IAppConfigureService
//    {
//        // 实现IAppConfigureService接口，支持数据的自动双向绑定

//        // 假设 IAppConfigureService定义了一些属性, 这里需要实现它们
//        // 为实现数据的自动双向绑定，这里使用事件来通知属性变化

//        public event EventHandler<string> PropertyChanged;

//        private Dictionary<string, object> _configuration = new Dictionary<string, object>();

//        protected virtual void OnPropertyChanged(string propertyName)
//        {
//            PropertyChanged?.Invoke(this, propertyName);
//        }

//        public T GetValue<T>(string key)
//        {
//            if (_configuration.TryGetValue(key, out var value))
//            {
//                if (value is T t)
//                    return t;
//                try
//                {
//                    return (T)Convert.ChangeType(value, typeof(T));
//                }
//                catch
//                {
//                    return default(T);
//                }
//            }
//            return default(T);
//        }

//        public bool SetValue<T>(string key, T value)
//        {
//            if (_configuration.ContainsKey(key))
//            {
//                if (object.Equals(_configuration[key], value))
//                    return false;
//                _configuration[key] = value;
//            }
//            else
//            {
//                _configuration.Add(key, value);
//            }
//            OnPropertyChanged(key);
//            return true;
//        }

//        public IReadOnlyDictionary<string, object> GetAllConfigurations()
//        {
//            return _configuration;
//        }


//        // 索引器：便于属性绑定，自动触发变化事件
//        //public object this[string key]
//        //{
//        //    get => _configuration.ContainsKey(key) ? _configuration[key] : null;
//        //    set
//        //    {
//        //        if (!_configuration.ContainsKey(key) || !object.Equals(_configuration[key], value))
//        //        {
//        //            _configuration[key] = value;
//        //            OnPropertyChanged(key);
//        //        }
//        //    }
//        //}


//    }
//}
