using System;
using System.ComponentModel;

namespace BaiShengVx3Plus.Core
{
    /// <summary>
    /// 扩展 BindingList，在移除项之前提供事件通知。
    /// 这允许在项从列表中移除之前执行数据库删除操作。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TrackableBindingList<T> : BindingList<T>
    {
        public event EventHandler<ItemRemovingEventArgs<T>>? ItemRemoving;

        public TrackableBindingList() : base() { }
        public TrackableBindingList(IList<T> list) : base(list) { }

        protected override void RemoveItem(int index)
        {
            if (index >= 0 && index < Count)
            {
                T itemToRemove = this[index];
                var args = new ItemRemovingEventArgs<T>(itemToRemove, index);
                ItemRemoving?.Invoke(this, args);

                // 如果事件处理器没有取消移除，则执行基类的移除操作
                if (!args.Cancel)
                {
                    base.RemoveItem(index);
                }
            }
        }
    }

    /// <summary>
    /// ItemRemoving 事件参数
    /// </summary>
    public class ItemRemovingEventArgs<T> : CancelEventArgs
    {
        public T Item { get; }
        public int Index { get; }

        public ItemRemovingEventArgs(T item, int index)
        {
            Item = item;
            Index = index;
        }
    }
}

