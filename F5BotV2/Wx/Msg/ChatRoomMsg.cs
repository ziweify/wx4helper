using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F5BotV2.Wx.Msg
{
    /// <summary>
    ///    
    /// </summary>
    public class GetMemberListMsg
    {
        public string chatRoomId { get; set; }
        /// <summary>
        ///     群管理员
        /// </summary>
        public string admin { get; set; }
        public string adminNickname { get; set; }
       
        public string members { get; set; }
        public string memberNickname { get; set; }
    }


    public class ChatRoomMsg
    {
       public string content { get; set; }
        public long createTime { get; set; }

        public string main_wxid { get; set; }   //群ID

        public string from_wxid { get; set; }
        public int pid { get; set; }
        public int msg_type { get; set; }
        public int status { get; set; }
    }

}
