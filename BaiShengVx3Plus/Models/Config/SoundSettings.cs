using System.ComponentModel;

namespace BaiShengVx3Plus.Models.Config
{
    /// <summary>
    /// 声音设置（保存相对路径）
    /// </summary>
    public class SoundSettings : INotifyPropertyChanged
    {
        private string _sealingSound = "mp3_fp.mp3";
        private string _lotterySound = "mp3_kj.mp3";
        private string _creditUpSound = "mp3_shang.mp3";
        private string _creditDownSound = "mp3_xia.mp3";
        private int _sealingVolume = 100;
        private int _lotteryVolume = 100;
        private int _creditUpVolume = 100;
        private int _creditDownVolume = 100;
        private bool _enableSound = true;

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// 封盘声音文件（相对路径）
        /// </summary>
        public string SealingSound
        {
            get => _sealingSound;
            set
            {
                if (_sealingSound != value)
                {
                    _sealingSound = value;
                    OnPropertyChanged(nameof(SealingSound));
                }
            }
        }

        /// <summary>
        /// 开奖声音文件（相对路径）
        /// </summary>
        public string LotterySound
        {
            get => _lotterySound;
            set
            {
                if (_lotterySound != value)
                {
                    _lotterySound = value;
                    OnPropertyChanged(nameof(LotterySound));
                }
            }
        }

        /// <summary>
        /// 上分声音文件（相对路径）
        /// </summary>
        public string CreditUpSound
        {
            get => _creditUpSound;
            set
            {
                if (_creditUpSound != value)
                {
                    _creditUpSound = value;
                    OnPropertyChanged(nameof(CreditUpSound));
                }
            }
        }

        /// <summary>
        /// 下分声音文件（相对路径）
        /// </summary>
        public string CreditDownSound
        {
            get => _creditDownSound;
            set
            {
                if (_creditDownSound != value)
                {
                    _creditDownSound = value;
                    OnPropertyChanged(nameof(CreditDownSound));
                }
            }
        }

        /// <summary>
        /// 封盘音量 (0-100)
        /// </summary>
        public int SealingVolume
        {
            get => _sealingVolume;
            set
            {
                if (_sealingVolume != value)
                {
                    _sealingVolume = Math.Clamp(value, 0, 100);
                    OnPropertyChanged(nameof(SealingVolume));
                }
            }
        }

        /// <summary>
        /// 开奖音量 (0-100)
        /// </summary>
        public int LotteryVolume
        {
            get => _lotteryVolume;
            set
            {
                if (_lotteryVolume != value)
                {
                    _lotteryVolume = Math.Clamp(value, 0, 100);
                    OnPropertyChanged(nameof(LotteryVolume));
                }
            }
        }

        /// <summary>
        /// 上分音量 (0-100)
        /// </summary>
        public int CreditUpVolume
        {
            get => _creditUpVolume;
            set
            {
                if (_creditUpVolume != value)
                {
                    _creditUpVolume = Math.Clamp(value, 0, 100);
                    OnPropertyChanged(nameof(CreditUpVolume));
                }
            }
        }

        /// <summary>
        /// 下分音量 (0-100)
        /// </summary>
        public int CreditDownVolume
        {
            get => _creditDownVolume;
            set
            {
                if (_creditDownVolume != value)
                {
                    _creditDownVolume = Math.Clamp(value, 0, 100);
                    OnPropertyChanged(nameof(CreditDownVolume));
                }
            }
        }

        /// <summary>
        /// 是否启用声音
        /// </summary>
        public bool EnableSound
        {
            get => _enableSound;
            set
            {
                if (_enableSound != value)
                {
                    _enableSound = value;
                    OnPropertyChanged(nameof(EnableSound));
                }
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

