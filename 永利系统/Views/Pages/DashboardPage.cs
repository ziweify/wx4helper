using System;
using System.Windows.Forms;
using 永利系统.ViewModels;

namespace 永利系统.Views.Pages
{
    public partial class DashboardPage : UserControl
    {
        private readonly DashboardViewModel _viewModel;

        public DashboardPage()
        {
            InitializeComponent();
            _viewModel = new DashboardViewModel();
            BindViewModel();
        }

        private void BindViewModel()
        {
            // 绑定数据到控件
            lblTotalRecords.DataBindings.Add("Text", _viewModel, nameof(_viewModel.TotalRecords));
            lblTodayRecords.DataBindings.Add("Text", _viewModel, nameof(_viewModel.TodayRecords));
            lblTotalAmount.DataBindings.Add("Text", _viewModel, nameof(_viewModel.TotalAmount));
        }

        private void DashboardPage_Load(object sender, EventArgs e)
        {
            _viewModel.OnLoaded();
        }
    }
}

