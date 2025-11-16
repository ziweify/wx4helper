using System.ComponentModel;
using zhaocaimao.Models;

namespace zhaocaimao.Core
{
    /// <summary>
    /// 资金变动记录 BindingList（线程安全）
    /// </summary>
    public class V2BalanceChangeBindingList : BindingList<V2BalanceChange>
    {
        private readonly SynchronizationContext? _syncContext;

        public V2BalanceChangeBindingList()
        {
            _syncContext = SynchronizationContext.Current;
        }

        protected override void InsertItem(int index, V2BalanceChange item)
        {
            if (_syncContext != null && SynchronizationContext.Current != _syncContext)
            {
                _syncContext.Post(_ => base.InsertItem(index, item), null);
            }
            else
            {
                base.InsertItem(index, item);
            }
        }

        protected override void RemoveItem(int index)
        {
            if (_syncContext != null && SynchronizationContext.Current != _syncContext)
            {
                _syncContext.Post(_ => base.RemoveItem(index), null);
            }
            else
            {
                base.RemoveItem(index);
            }
        }

        protected override void SetItem(int index, V2BalanceChange item)
        {
            if (_syncContext != null && SynchronizationContext.Current != _syncContext)
            {
                _syncContext.Post(_ => base.SetItem(index, item), null);
            }
            else
            {
                base.SetItem(index, item);
            }
        }

        protected override void ClearItems()
        {
            if (_syncContext != null && SynchronizationContext.Current != _syncContext)
            {
                _syncContext.Post(_ => base.ClearItems(), null);
            }
            else
            {
                base.ClearItems();
            }
        }
    }
}

