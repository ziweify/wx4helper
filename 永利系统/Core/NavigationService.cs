using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace 永利系统.Core
{
    /// <summary>
    /// 导航服务，管理页面切换
    /// </summary>
    public class NavigationService
    {
        private readonly Panel _contentPanel;
        private readonly Dictionary<string, UserControl> _pages = new();
        private UserControl? _currentPage;

        public event EventHandler<string>? NavigationChanged;

        public NavigationService(Panel contentPanel)
        {
            _contentPanel = contentPanel ?? throw new ArgumentNullException(nameof(contentPanel));
        }

        /// <summary>
        /// 注册页面
        /// </summary>
        public void RegisterPage(string key, UserControl page)
        {
            if (!_pages.ContainsKey(key))
            {
                _pages[key] = page;
                page.Dock = DockStyle.Fill;
            }
        }

        /// <summary>
        /// 导航到指定页面
        /// </summary>
        public void NavigateTo(string key)
        {
            if (!_pages.TryGetValue(key, out var page))
                return;

            if (_currentPage == page)
                return;

            // 移除当前页面
            if (_currentPage != null)
            {
                _contentPanel.Controls.Remove(_currentPage);
            }

            // 添加新页面
            _contentPanel.Controls.Clear();
            _contentPanel.Controls.Add(page);
            _currentPage = page;

            NavigationChanged?.Invoke(this, key);
        }

        /// <summary>
        /// 获取当前页面键
        /// </summary>
        public string? GetCurrentPageKey()
        {
            if (_currentPage == null)
                return null;

            foreach (var kvp in _pages)
            {
                if (kvp.Value == _currentPage)
                    return kvp.Key;
            }

            return null;
        }
    }
}

