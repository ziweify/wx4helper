using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace F5BotV2.Wx
{
    /// <summary>
    ///     微信账号
    /// </summary>
    public interface IWxAccount
    {
        /// <summary>
        ///     账号
        /// </summary>
        string account { get; set; }

        /// <summary>
        ///     头像网络下载地址
        /// </summary>
        string avatar { get; set; }

        /// <summary>
        ///     用户名
        /// </summary>

        string nickname { get; set; }

        /// <summary>
        ///     实名验证电话
        /// </summary>
        string phone { get; set; }

        int pid { get; set; }

        /// <summary>
        ///     用户数据目录
        /// </summary>
        string wx_user_dir { get; set; }

        /// <summary>
        ///     微信ID
        /// </summary>
        string wxid { get; set; }
    }
}
