using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace YongLiSystem.Models.Wechat
{
    /// <summary>
    /// 微信联系人数据模型（用于数据绑定）
    /// </summary>
    public class Contact : INotifyPropertyChanged
    {
        private string _wxid = "";
        private string? _account;
        private string? _nickname;
        private string? _displayName;
        private string? _avatar;
        private bool _isGroup;
        private string? _groupWxId;
        private DateTime _lastUpdateTime;

        /// <summary>
        /// 微信ID（唯一标识）
        /// </summary>
        public string Wxid
        {
            get => _wxid;
            set => SetField(ref _wxid, value);
        }

        /// <summary>
        /// 微信号
        /// </summary>
        public string? Account
        {
            get => _account;
            set => SetField(ref _account, value);
        }

        /// <summary>
        /// 昵称
        /// </summary>
        public string? Nickname
        {
            get => _nickname;
            set => SetField(ref _nickname, value);
        }

        /// <summary>
        /// 群昵称（在群中的显示名称）
        /// </summary>
        public string? DisplayName
        {
            get => _displayName;
            set => SetField(ref _displayName, value);
        }

        /// <summary>
        /// 头像URL
        /// </summary>
        public string? Avatar
        {
            get => _avatar;
            set => SetField(ref _avatar, value);
        }

        /// <summary>
        /// 是否为群组
        /// </summary>
        public bool IsGroup
        {
            get => _isGroup;
            set => SetField(ref _isGroup, value);
        }

        /// <summary>
        /// 所属群组ID（如果是群成员）
        /// </summary>
        public string? GroupWxId
        {
            get => _groupWxId;
            set => SetField(ref _groupWxId, value);
        }

        /// <summary>
        /// 最后更新时间
        /// </summary>
        public DateTime LastUpdateTime
        {
            get => _lastUpdateTime;
            set => SetField(ref _lastUpdateTime, value);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (System.Collections.Generic.EqualityComparer<T>.Default.Equals(field, value))
                return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}

