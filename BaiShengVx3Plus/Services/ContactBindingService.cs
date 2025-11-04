using BaiShengVx3Plus.Models;

namespace BaiShengVx3Plus.Services
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
            // TODO: 保存到数据库或配置文件
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

