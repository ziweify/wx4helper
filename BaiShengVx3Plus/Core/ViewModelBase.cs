using CommunityToolkit.Mvvm.ComponentModel;

namespace BaiShengVx3Plus.Core
{
    /// <summary>
    /// ViewModel基类
    /// </summary>
    public class ViewModelBase : ObservableObject
    {
        private bool _isBusy;
        private string _busyMessage = string.Empty;

        /// <summary>
        /// 是否繁忙
        /// </summary>
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        /// <summary>
        /// 繁忙消息
        /// </summary>
        public string BusyMessage
        {
            get => _busyMessage;
            set => SetProperty(ref _busyMessage, value);
        }
    }
}

