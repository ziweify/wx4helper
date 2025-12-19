using System;
using System.Collections.ObjectModel;
using 永利系统.Core;

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
            Title = "数据概览";
            LoadDashboardData();
        }

        #region 属性

        public int TotalRecords
        {
            get => _totalRecords;
            set => SetProperty(ref _totalRecords, value);
        }

        public int TodayRecords
        {
            get => _todayRecords;
            set => SetProperty(ref _todayRecords, value);
        }

        public decimal TotalAmount
        {
            get => _totalAmount;
            set => SetProperty(ref _totalAmount, value);
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

        public override void OnLoaded()
        {
            base.OnLoaded();
            LoadDashboardData();
        }

        #endregion
    }
}

