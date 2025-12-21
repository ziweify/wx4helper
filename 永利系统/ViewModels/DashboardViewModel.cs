using System;
using System.Collections.ObjectModel;
using DevExpress.Mvvm;

namespace 永利系统.ViewModels
{
    /// <summary>
    /// Dashboard 页面 ViewModel
    /// </summary>
    public class DashboardViewModel : ViewModelBase
    {
        private int _totalRecords;
        private int _todayRecords;
        private decimal _totalAmount;

        public DashboardViewModel()
        {
            LoadDashboardData();
        }

        #region 属性

        public int TotalRecords
        {
            //get => _totalRecords;
            //set => SetProperty(ref _totalRecords, value, nameof(TotalRecords));

            get => _totalRecords;
            set
            {
                if (_totalRecords != value)
                {
                    _totalRecords = value;
                    RaisePropertyChanged();
                }
            }
        }

        public int TodayRecords
        {
            get => _todayRecords;
            set
            {
                if (_todayRecords != value)
                {
                    _todayRecords = value;
                    RaisePropertyChanged();
                }
            }
        }

        public decimal TotalAmount
        {
            get => _totalAmount;
            set
            {
                if (_totalAmount != value)
                {
                    _totalAmount = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region 方法

        private void LoadDashboardData()
        {
            // TODO: 从数据库或服务加载实际数据
            TotalRecords = 1250;
            TodayRecords = 45;
            TotalAmount = 356789.50m;
        }

        /// <summary>
        /// 刷新数据（用于后台自动刷新）
        /// </summary>
        public void RefreshData()
        {
            LoadDashboardData();
        }

        // OnLoaded 方法在 DevExpress.Mvvm.ViewModelBase 中不存在
        // 数据加载已在构造函数中完成

        #endregion
    }
}

