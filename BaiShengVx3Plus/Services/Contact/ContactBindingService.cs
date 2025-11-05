using BaiShengVx3Plus.Models;
using BaiShengVx3Plus.Contracts;

namespace BaiShengVx3Plus.Services.Contact
{
    /// <summary>
    /// 联系人绑定服务实现
    /// </summary>
    public class ContactBindingService : IContactBindingService
    {
        private WxContact? _currentContact;

        public event EventHandler<WxContact?>? BindingChanged;

        public void BindContact(WxContact contact)
        {
            _currentContact = contact;
            BindingChanged?.Invoke(this, contact);
        }

        public WxContact? GetCurrentContact()
        {
            return _currentContact;
        }

        public void ClearBinding()
        {
            _currentContact = null;
            BindingChanged?.Invoke(this, null);
        }
    }
}

