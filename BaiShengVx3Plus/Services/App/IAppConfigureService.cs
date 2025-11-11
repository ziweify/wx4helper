using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BaiShengVx3Plus.Services.App
{
    /// <summary>
    ///     提供整个app的配置数据,配置模式, 界面数据绑定, 初始化, 及保存
    /// </summary>
    public interface IAppConfigureService
        : INotifyPropertyChanged
    {
        string BsUserName { get; set; }   //百盛用户名
        string BsUserPwd { get; set; }    //百盛密码


        /// <summary>
        ///     运行模式
        /// </summary>
        bool IsRunModeDev { get; set; }    //开发模式, 模拟联系人数据,模拟群数据,模拟恢复消息,可以控制界面显示模拟操作相关内容
        bool IsRunModeAdmin { get; set; }  //管理模式(可以手动输入绑定群
        bool IsRunModeBoss { get; set; }   //老板模式

        /// <summary>
        ///     软件模式
        /// </summary>
        bool IsSoftModeVx { get; set; }
        bool IsSoftModeFeitian { get; set; }

        // 属性自动通知
        event PropertyChangedEventHandler? PropertyChanged;

        void OnPropertyChanged([CallerMemberName] string? propertyName = null);
        //{
        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        //}

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null);
        //{
        //    if (Equals(field, value)) return false;

        //    field = value;
        //    OnPropertyChanged(propertyName);
        //    return true;
        //}

        void Load();
        void Save();
    }
}
