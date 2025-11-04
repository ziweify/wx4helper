using F5BotV2.BetSite.Qt;
using F5BotV2.Model;
using Google.Protobuf.WellKnownTypes;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace F5BotV2.Boter
{

    /// <summary>
    ///     此法解析会员结果
    /// </summary>
    public class LexicalMemberResult
    {
        public LexicalMemberResult()
        {

        }
    }

    //新老版本关系转换
    /*
     *          WxBuffer = json["data"]["msg"].ToString();
                WxIdMain = json["data"]["room_wxid"].ToString();
                WxIdSub = json["data"]["from_wxid"].ToString();
                TimeStamp = json["data"]["timestamp"].ToString();
     */

    /// <summary>
    ///     消息处理类
    /// </summary>
    public class BoterWxGroupMessage
    {
        private string _orginMsg;
        public string orginMsg { get { return _orginMsg; }  }

        private string _msg;
        /// <summary>
        ///     处理过前后空格的消息的内容
        /// </summary>
        public string msg { get { return _msg; } }

        private string _room_wxid;
        public string room_wxid { get { return _room_wxid; } }


        private string _from_wxid;
        public string from_wxid { get { return _from_wxid; } }


        private string _to_wxid;
        public string to_wxid { get { return _to_wxid; } }


        private int _wx_type;
        public int wx_type { get { return _wx_type; } }


        private int _timestamp;
        public int timestamp { get { return _timestamp; } }


        /// <summary>
        ///     构造函数异常了。这里就返回true;
        /// </summary>
        private bool _error = false;
        public bool error { get { return _error; } }

        public BoterWxGroupMessage(string orginMsg)
        {
            this._orginMsg = orginMsg;
            try
            {
                JObject json = JObject.Parse(orginMsg);
                _msg = json["data"]["msg"].ToString().Trim(' ');
                _room_wxid = json["data"]["room_wxid"].ToString();
                _from_wxid = json["data"]["from_wxid"].ToString();
                _timestamp = Convert.ToInt32(json["data"]["timestamp"].ToString());
            }
            catch(Exception ex)
            {
                this._error = true;
            }
        }

        public BoterWxGroupMessage(string room_wxid, string from_wxid, string msg, int timestamp)
        {
            this._room_wxid = room_wxid;
            this._from_wxid = from_wxid;
            this._msg = msg;
            this._timestamp = timestamp;
        }

        public bool IsGroupMessage()
        {
            //@chatroom
            bool result = false;
            int index = room_wxid.IndexOf("@chatroom");
            if (index >= 0)
            {
                result = true;
            }
            return result;
        }

    }
}
