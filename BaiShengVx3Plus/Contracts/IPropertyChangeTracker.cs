using System;
using System.ComponentModel;

namespace BaiShengVx3Plus.Contracts
{
    /// <summary>
    /// 属性变化追踪器接口（监听模型属性变化并自动保存）
    /// </summary>
    public interface IPropertyChangeTracker
    {
        /// <summary>
        /// 开始追踪对象的属性变化
        /// </summary>
        void Track<T>(T obj, string tableName) where T : INotifyPropertyChanged;

        /// <summary>
        /// 停止追踪对象的属性变化
        /// </summary>
        void Untrack<T>(T obj) where T : INotifyPropertyChanged;

        /// <summary>
        /// 清除所有追踪
        /// </summary>
        void ClearAll();
    }
}

