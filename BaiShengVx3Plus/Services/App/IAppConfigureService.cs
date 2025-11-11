using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaiShengVx3Plus.Services.App
{
    /// <summary>
    ///     提供整个app的配置数据,配置模式, 界面数据绑定, 初始化, 及保存
    /// </summary>
    public interface IAppConfigureService
    {
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
        bool IsSoftModeFeidan { get; set; }
    }
}
