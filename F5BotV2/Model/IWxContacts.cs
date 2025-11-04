using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F5BotV2.Model
{
    public interface IWxContacts
    {
        /// <summary>
        /// wxid
        /// </summary>
        string wxid { get; set; }
        /// <summary>
        /// 微信号(有可能为空)
        /// </summary>
        string account { get; set; }

        /// <summary>
        /// 微信昵称
        /// </summary>
        string nickname { get; set; }

        /// <summary>
        /// 头像的url地址
        /// </summary>
        string avatar { get; set; }

        /// <summary>
        /// 城市(可能为空)
        /// </summary>
        string city { get; set; }

        /// <summary>
        /// 祖国(可能为空)
        /// </summary>
        string country { get; set; }

        /// <summary>
        /// 省份(可能为空)
        /// </summary>
        string province { get; set; }

        /// <summary>
        /// 好友备注
        /// </summary>
        string remark { get; set; }

        /// <summary>
        /// 性别:0未知，1男,2女
        /// </summary>
        int sex { get; set; }
    }
}
