using System;
using System.Windows.Forms;
using 永利系统.ViewModels;

namespace 永利系统.Views.Pages
{
    public partial class DataManagementPage : UserControl
    {
        private readonly DataManagementViewModel _viewModel;

        public DataManagementPage()
        {
            InitializeComponent();
            _viewModel = new DataManagementViewModel();
            BindViewModel();
        }

        private void BindViewModel()
        {
            // 绑定数据
            gridControl1.DataSource = _viewModel.DataItems;
            txtSearch.DataBindings.Add("Text", _viewModel, nameof(_viewModel.SearchText));
            
            // 绑定命令
            btnAdd.Click += (s, e) => _viewModel.AddCommand?.Execute(null);
            btnEdit.Click += (s, e) => _viewModel.EditCommand?.Execute(null);
            btnDelete.Click += (s, e) => _viewModel.DeleteCommand?.Execute(null);
            btnSearch.Click += (s, e) => _viewModel.SearchCommand?.Execute(null);
        }

        private void DataManagementPage_Load(object sender, EventArgs e)
        {
            _viewModel.OnLoaded();
        }

        private void gridView1_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            var view = sender as DevExpress.XtraGrid.Views.Grid.GridView;
            if (view != null && view.FocusedRowHandle >= 0)
            {
                _viewModel.SelectedItem = view.GetRow(view.FocusedRowHandle) as 永利系统.Models.DataItem;
            }
        }
    }
}

