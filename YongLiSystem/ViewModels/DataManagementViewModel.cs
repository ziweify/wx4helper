using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using DevExpress.Mvvm;
using YongLiSystem.Models;

namespace YongLiSystem.ViewModels
{
    /// <summary>
    /// 数据管理页面 ViewModel
    /// </summary>
    public class DataManagementViewModel : ViewModelBase
    {
        private ObservableCollection<DataItem> _dataItems;
        private DataItem? _selectedItem;
        private string _searchText = string.Empty;

        public DataManagementViewModel()
        {
            _dataItems = new ObservableCollection<DataItem>();
            InitializeCommands();
            LoadData();
        }

        #region 属性

        public ObservableCollection<DataItem> DataItems
        {
            get => _dataItems;
            set
            {
                if (_dataItems != value)
                {
                    _dataItems = value;
                    RaisePropertyChanged();
                }
            }
        }

        public DataItem? SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (_selectedItem != value)
                {
                    _selectedItem = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    RaisePropertyChanged();
                    SearchData();
                }
            }
        }

        #endregion

        #region 命令

        public ICommand? AddCommand { get; private set; }
        public ICommand? EditCommand { get; private set; }
        public ICommand? DeleteCommand { get; private set; }
        public ICommand? SearchCommand { get; private set; }

        private void InitializeCommands()
        {
            AddCommand = new DelegateCommand(() => AddItem());
            EditCommand = new DelegateCommand(() => EditItem(), () => SelectedItem != null);
            DeleteCommand = new DelegateCommand(() => DeleteItem(), () => SelectedItem != null);
            SearchCommand = new DelegateCommand(() => SearchData());
        }

        #endregion

        #region 方法

        private void LoadData()
        {
            // TODO: 从数据库加载实际数据
            DataItems.Clear();
            // 测试数据已删除，等待实际数据源对接
        }

        private void AddItem()
        {
            var newItem = new DataItem
            {
                Id = DataItems.Count + 1,
                Name = "新数据项",
                CreateTime = DateTime.Now,
                IsActive = true
            };
            DataItems.Add(newItem);
            SelectedItem = newItem;
        }

        private void EditItem()
        {
            if (SelectedItem != null)
            {
                // TODO: 实现编辑逻辑
                System.Windows.Forms.MessageBox.Show($"编辑项目: {SelectedItem.Name}");
            }
        }

        private void DeleteItem()
        {
            if (SelectedItem != null)
            {
                var result = System.Windows.Forms.MessageBox.Show(
                    $"确定要删除 '{SelectedItem.Name}' 吗？",
                    "确认删除",
                    System.Windows.Forms.MessageBoxButtons.YesNo,
                    System.Windows.Forms.MessageBoxIcon.Question);

                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    DataItems.Remove(SelectedItem);
                }
            }
        }

        private void SearchData()
        {
            // TODO: 实现搜索逻辑
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                LoadData();
            }
            else
            {
                // 模拟搜索
                var filtered = new ObservableCollection<DataItem>();
                foreach (var item in DataItems)
                {
                    if (item.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                        item.Description?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true)
                    {
                        filtered.Add(item);
                    }
                }
                DataItems = filtered;
            }
        }

        /// <summary>
        /// 刷新数据（用于后台自动刷新）
        /// </summary>
        public void RefreshData()
        {
            LoadData();
        }

        // OnLoaded 方法在 DevExpress.Mvvm.ViewModelBase 中不存在
        // 数据加载已在构造函数中完成

        #endregion
    }
}

