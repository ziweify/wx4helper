using System.ComponentModel;
using BaiShengVx3Plus.Models;

namespace BaiShengVx3Plus.Core
{
    /// <summary>
    /// 上下分申请 BindingList（线程安全）
    /// </summary>
    public class V2CreditWithdrawBindingList : BindingList<V2CreditWithdraw>
    {
        private readonly SynchronizationContext? _syncContext;

        public V2CreditWithdrawBindingList()
        {
            _syncContext = SynchronizationContext.Current;
        }

        protected override void InsertItem(int index, V2CreditWithdraw item)
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

        protected override void SetItem(int index, V2CreditWithdraw item)
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

