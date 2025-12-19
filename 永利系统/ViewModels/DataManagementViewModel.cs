using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using 永利系统.Core;
using 永利系统.Models;

namespace 永利系统.ViewModels
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
            Title = "数据管理";
            _dataItems = new ObservableCollection<DataItem>();
            InitializeCommands();
            LoadData();
        }

        #region 属性

        public ObservableCollection<DataItem> DataItems
        {
            get => _dataItems;
            set => SetProperty(ref _dataItems, value);
        }

        public DataItem? SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
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
            AddCommand = new RelayCommand(_ => AddItem());
            EditCommand = new RelayCommand(_ => EditItem(), _ => SelectedItem != null);
            DeleteCommand = new RelayCommand(_ => DeleteItem(), _ => SelectedItem != null);
            SearchCommand = new RelayCommand(_ => SearchData());
        }

        #endregion

        #region 方法

        private void LoadData()
        {
            // TODO: 从数据库加载实际数据
            DataItems.Clear();
            for (int i = 1; i <= 50; i++)
            {
                DataItems.Add(new DataItem
                {
                    Id = i,
                    Name = $"数据项 {i}",
                    Description = $"这是第 {i} 条数据的描述",
                    Amount = (decimal)(i * 100.5),
                    CreateTime = DateTime.Now.AddDays(-i),
                    IsActive = i % 3 != 0
                });
            }
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

        public override void OnLoaded()
        {
            base.OnLoaded();
            LoadData();
        }

        #endregion
    }
}

