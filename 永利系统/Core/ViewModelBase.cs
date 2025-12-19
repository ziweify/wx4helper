using System;

namespace 永利系统.Core
{
    /// <summary>
    /// ViewModel 基类
    /// </summary>
    public abstract class ViewModelBase : ObservableObject
    {
        private bool _isBusy;
        private string _title = string.Empty;

        /// <summary>
        /// 是否正在执行操作
        /// </summary>
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        /// <summary>
        /// 视图标题
        /// </summary>
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        /// <summary>
        /// 初始化方法，在视图加载时调用
        /// </summary>
        public virtual void OnLoaded()
        {
        }

        /// <summary>
        /// 清理方法，在视图卸载时调用
        /// </summary>
        public virtual void OnUnloaded()
        {
        }
    }
}

