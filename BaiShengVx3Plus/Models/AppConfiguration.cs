using System.ComponentModel;
using System.Text.Json.Serialization;

namespace BaiShengVx3Plus.Models
{
    /// <summary>
    /// 应用程序配置
    /// 存储应用级别的设置（非业务数据）
    /// </summary>
    public class AppConfiguration : INotifyPropertyChanged
    {
        public AppConfiguration() {
            //_currentBoundContact = new WxContact();
        }

        private bool _isOrdersTaskingEnabled = false; // 默认关闭收单
        private bool _isAutoBetEnabled = false; // 默认关闭自动投注
        private int _sealSecondsAhead = 45; // 默认提前45秒封盘


        // 当前绑定的联系人对象
       //private WxContact? _currentBoundContact;
       // public WxContact CurrentBoundContact { get { return _currentBoundContact; } }

        /// <summary>
        /// 收单开关（是否接收微信下注消息）
        /// </summary>
        [JsonPropertyName("isOrdersTaskingEnabled")]
        public bool IsOrdersTaskingEnabled
        {
            get => _isOrdersTaskingEnabled;
            set
            {
                if (_isOrdersTaskingEnabled != value)
                {
                    _isOrdersTaskingEnabled = value;
                    OnPropertyChanged(nameof(IsOrdersTaskingEnabled));
                }
            }
        }
        
        /// <summary>
        /// 自动投注开关（飞单）
        /// </summary>
        [JsonPropertyName("isAutoBetEnabled")]
        public bool IsAutoBetEnabled
        {
            get => _isAutoBetEnabled;
            set
            {
                if (_isAutoBetEnabled != value)
                {
                    _isAutoBetEnabled = value;
                    OnPropertyChanged(nameof(IsAutoBetEnabled));
                }
            }
        }
        
        /// <summary>
        /// 提前封盘秒数
        /// </summary>
        [JsonPropertyName("sealSecondsAhead")]
        public int SealSecondsAhead
        {
            get => _sealSecondsAhead;
            set
            {
                if (_sealSecondsAhead != value)
                {
                    _sealSecondsAhead = value;
                    OnPropertyChanged(nameof(SealSecondsAhead));
                }
            }
        }
        
        public event PropertyChangedEventHandler? PropertyChanged;
        
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

