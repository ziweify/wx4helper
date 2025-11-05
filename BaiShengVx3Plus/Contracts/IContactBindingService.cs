using BaiShengVx3Plus.Models;

namespace BaiShengVx3Plus.Contracts
{
    /// <summary>
    /// 联系人绑定服务接口
    /// </summary>
    public interface IContactBindingService
    {
        /// <summary>
        /// 绑定联系人
        /// </summary>
        void BindContact(WxContact contact);

        /// <summary>
        /// 获取当前绑定的联系人
        /// </summary>
        WxContact? GetCurrentContact();

        /// <summary>
        /// 清除绑定
        /// </summary>
        void ClearBinding();

        /// <summary>
        /// 绑定变更事件
        /// </summary>
        event EventHandler<WxContact?>? BindingChanged;
    }
}

