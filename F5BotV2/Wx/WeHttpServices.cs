using LxLib.LxNet;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HttpHelper = LxLib.LxNet.LxHttpHelper;
using HPSocket.Sdk;
using HPSocket.Tcp;
using HPSocket;
using F5BotV2.Wx.Msg;
using System.Threading;
using System.Net;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;
using System.Windows.Forms.DataVisualization.Charting;

namespace F5BotV2.Wx
{
    public class WeHttpResult<T>
    {
        public int code { get; set; }
        public T data { get; set; }
        public string msg { get; set; }
    }

    

    /// <summary>
    ///     微信http调用
    /// </summary>
    public class WeHttpServices
    {
        public delegate void OnReceiveCallback(string data);
        public OnReceiveCallback onReceiveCallback;

        public string urlRoot = "http://127.0.0.1";
        public int port = 19088;
        public bool isClientConnect = false;
        private TcpClient client;
        private int tcpPort = 5678; //回调端口


        public WeHttpServices()
        {
            client = new TcpClient();
        }

        //开始消息回调
        public bool BeginMessageCallback()
        {
            try
            {
                if (!isClientConnect)
                {
                    client.OnPrepareConnect += OnPrepareConnect;
                    client.OnConnect += OnConnect;
                    client.OnSend += OnSend;
                    client.OnReceive += OnReceive;
                    client.OnClose += OnClose;

                    if (client.Connect("127.0.0.1", (ushort)tcpPort))
                    {
                        Console.WriteLine("连接服务器成功！");
                        isClientConnect = true;
                        // 发送数据
                        //string sendStr = "Hello, HPSocket!";
                        // client.Send(Encoding.UTF8.GetBytes(sendStr), 0, sendStr.Length);
                    }
                    else
                    {
                        isClientConnect = false;
                        Console.WriteLine("连接服务器失败！");
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("连接服务器异常！");
            }


            return isClientConnect;
        }


        // 准备连接事件
        private HandleResult OnPrepareConnect(IClient sender, IntPtr request)
        {
            // 可以在这里进行协议特定的处理
            return HandleResult.Ok;
        }

        // 连接成功事件
        private HandleResult OnConnect(IClient sender)
        {
            Console.WriteLine("连接成功！");
            return HandleResult.Ok;
        }

        // 发送事件
        private HandleResult OnSend(IClient sender, byte[] data)
        {
            Console.WriteLine($"发送数据：{Encoding.UTF8.GetString(data)}");
            return HandleResult.Ok;
        }

        // 接收事件
        public HandleResult OnReceive(IClient sender, byte[] data)
        {
            //更新
            try
            {
                string sdata = Encoding.UTF8.GetString(data);
                //Console.WriteLine($"接收数据：{sdata}");
                if (onReceiveCallback != null)
                    onReceiveCallback(sdata);
            }
            catch (Exception ex)
            {

            }
            return HandleResult.Ok;
            /*
             * 给群发送消息 "3333"
             *     {"content":"3333","createTime":1715562426000,"fromuser":"20909465054@chatroom","index_db":360289069701279549,"local_id":12093,"msg_type":1,"pid":18852,"server_id":7368284513916420098}
             * 给好友发送消息 "3333"
             *     {"content":"3333","createTime":1715562538000,"fromuser":"wxid_oryaa3b0h2fn22","index_db":360289069701279550,"local_id":12094,"msg_type":1,"pid":18852,"server_id":7368284994952757250}
             *     
             * 好友发送给自己的消息 "什么事情"
             *     {"content":"什么事情","createTime":1715562594000,"fromuser":"wxid_oryaa3b0h2fn22","index_db":360289069701279551,"local_id":12095,"msg_type":1,"pid":18852,"server_id":7368285235470925826}
             */
        }

        // 关闭事件
        private HandleResult OnClose(IClient sender, SocketOperation socketOperation, int errorCode)
        {
            Console.WriteLine("连接已关闭！");
            isClientConnect = false;
            return HandleResult.Ok;
        }

        //检查是否登录
        public WeHttpResult<object> checkLogin()
        {
            WeHttpResult<object> result = new WeHttpResult<object>();
            string url = $"{urlRoot}:{port}/api/checkLogin";
            JObject jparam = new JObject();
            string postdata = JsonConvert.SerializeObject(jparam);
            return WePostMessage<object>(url, postdata);
        }

        //获取自身信息
        public WeHttpResult<SelfInfoMsg> GetSelfInfo()
        {
            WeHttpResult<SelfInfoMsg> result = new WeHttpResult<SelfInfoMsg>();
            string url = $"{urlRoot}:{port}/api/GetSelfInfo";
            JObject jparam = new JObject();
            string postdata = JsonConvert.SerializeObject(jparam);
            return WePostMessage<SelfInfoMsg>(url, postdata);
        }

        //获取联系人
        public WeHttpResult<List<ContactMsg>> GetContacts()
        {
            WeHttpResult<List<ContactMsg>> result = new WeHttpResult<List<ContactMsg>>();
            string url = $"{urlRoot}:{port}/api/GetContacts";
            JObject jparam = new JObject();
            string postdata = JsonConvert.SerializeObject(jparam);
            return WePostMessage<List<ContactMsg>>(url, postdata);
        }

        //获取群好友列表 10
        public WeHttpResult<GetMemberListMsg> GetMemberList(string room_id)
        {
            WeHttpResult<GetMemberListMsg> result = new WeHttpResult<GetMemberListMsg>();
            string url = $"{urlRoot}:{port}/api/Get_member_list";
            JObject jparam = new JObject();
            // 添加 JSON 字段
            jparam["room_id"] = room_id;
            string postdata = JsonConvert.SerializeObject(jparam);

            //过滤掉群成员
            return WePostMessage<GetMemberListMsg>(url, postdata);
        }

        //可以得到联系人, 或者群里面的成员昵称
        ////获取昵称 29  GetContactOrChatRoomNickname 这个可以得到联系人和群好友, 但是报错, 所以用这个
        public WeHttpResult<string> GetMemberNickname(string wxid)
        {
            WeHttpResult<string> result = new WeHttpResult<string>();
            //string url = $"{urlRoot}:{port}/api/Get_Meber_DisplayName";
            string url = $"{urlRoot}:{port}/api/GetContactOrChatRoomNickname";//GetContactOrChatRoomNickname
            JObject jparam = new JObject();
            // 添加 JSON 字段
            jparam["wxid"] = wxid;
            //jparam["chatroom_id"] = chatroom_id;
            string postdata = JsonConvert.SerializeObject(jparam);
            Thread.Sleep(10);
            //过滤掉群成员
            return WePostMessage<string>(url, postdata);
        }

        //Get_Meber_DisplayName
        //有些设置了群名片, 群名称的，就用这个获取
        public WeHttpResult<string> GetMemberDisplayName(string chatroom_id, string wxid)
        {
            WeHttpResult<string> result = new WeHttpResult<string>();
            string url = $"{urlRoot}:{port}/api/Get_Meber_DisplayName";
            JObject jparam = new JObject();
            // 添加 JSON 字段
            jparam["wxid"] = wxid;
            jparam["chatroom_id"] = chatroom_id;
            string postdata = JsonConvert.SerializeObject(jparam);

            //过滤掉群成员
            return WePostMessage<string>(url, postdata);
        }

        /// <summary>
        ///     hook进出群
        /// </summary>
        /// <returns></returns>
        public WeHttpResult<string>HookChatroom()
        {
            WeHttpResult<string> result = new WeHttpResult<string>();
            string url = $"{urlRoot}:{port}/api/Hook_chatroom";
            JObject jparam = new JObject();
            // 添加 JSON 字段
            string postdata = JsonConvert.SerializeObject(jparam);

            //过滤掉群成员
            return WePostMessage<string>(url, postdata);
        }

        //发送文本消息
        public WeHttpResult<string> SendTextMsg(string wxid, string msg)
        {
            WeHttpResult<string> result = new WeHttpResult<string>();
            string url = $"{urlRoot}:{port}/api/sendTextMsg";
            JObject jparam = new JObject();
            // 添加 JSON 字段
            jparam["wxid"] = wxid;
            jparam["msg"] = msg;
            string postdata = jparam.ToString().Replace("\r\n", "").Replace(" ", "");
            return WePostMessage<string>(url, postdata);
        }

        //发送图片消息 4
        public WeHttpResult<string> SendImageMsg(string wxid, string path)
        {
            WeHttpResult<string> result = new WeHttpResult<string>();
            string url = $"{urlRoot}:{port}/api/sendImagesMsg";
            JObject jparam = new JObject();
            // 添加 JSON 字段
            jparam["wxid"] = wxid;
            jparam["path"] = path;
            string postdata = JsonConvert.SerializeObject(jparam);
            return WePostMessage<string>(url, postdata);
        }


        public WeHttpResult<T> WePostMessage<T>(string url, string postdata)
        {
            WeHttpResult<T> result = null;
            string error = "";
            string httpret = "";
            try
            {
               
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.Method = "PUT";
                req.ContentType = "application/json";
                byte[] data = Encoding.UTF8.GetBytes(postdata);
                req.ContentLength = data.Length;
                req.GetRequestStream().Write(data, 0, data.Length);
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                Stream stream = resp.GetResponseStream();
                try
                {
                    //获取内容
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        httpret = reader.ReadToEnd();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                result = JsonConvert.DeserializeObject<WeHttpResult<T>>(httpret);
            }
            catch(Exception ex)
            {
                error = ex.Message;
                if (result == null)
                {
                    result = new WeHttpResult<T>();
                }
                result.code = -1; //内部错误 -1
                result.msg = $"html={httpret},";
                result.msg = result.msg + ex.Message;
            }

            return result;
        }

        //邀请进群
        /*
         * {"content":"你邀请\"梅开二度\"加入了群聊  ","createTime":1715558549000,"fromuser":"20909465054@chatroom","index_db":360289069701279516,"local_id":12060,"msg_type":10000,"pid":18852,"server_id":7368267862328213506}
         */
        //移除群聊
        /*
         第二条
        {"ID":1,"chatroom_id":"20909465054@chatroom","mbers":""}{"ID":1,"chatroom_id":"20909465054@chatroom","mbers":"wxid_9d5irldwmckg22^Gwxid_z10c1dpry1w212^Gwxid_oryaa3b0h2fn22^Gwxid_9jqe2vnpas4d12"}
         */

        //接受数据
        //自己给自己发送的
        /*
         * {"content":"22222","createTime":1715554287000,"fromuser":"filehelper","index_db":360289069701279512,"local_id":12056,"msg_type":1,"pid":24468,"server_id":7368249557177597953}
         */
    }

}
