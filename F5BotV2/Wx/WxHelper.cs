using CCWin.SkinClass;
using F5BotV2.Main;
using F5BotV2.Model;
using F5BotV2.Model.BindSqlite;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Forms;
using System.Windows.Threading;


namespace F5BotV2.Wx
{

    public class WxMemberLite
    {
        public string nickname { get; set; }
        public string wxid { get; set; }
    }

    /// <summary>
    ///     群成员加入消息
    /// </summary>
    public class GroupMembersWxPacket
    {
        public string avatar { get; set; }
        public int is_mamager { get; set; } //是否群管理
        public string nickname { get; set; } //群名称
        public string room_wxid { get; set; }   //群id
        public int total_member { get; set; }

        public List<WxMemberLite> member_list;

        public GroupMembersWxPacket()
        {
            member_list = new List<WxMemberLite>();
        }

    }

    public class GroupMembersExitWxPacket
    {
        public string avatar { get; set; }
        public int is_mamager { get; set; } //是否群管理
        public string nickname { get; set; } //群名称
        public string room_wxid { get; set; }   //群id
        public int total_member { get; set; }

        public List<WxMemberLite> member_list;

        public GroupMembersExitWxPacket()
        {
            member_list = new List<WxMemberLite>();
        }
    }


    public enum WxHelperStatus { 微信登入成功 = 1, 微信自身头像下载完成 = 2}
    public delegate int WxHelperStatusHandle(WxHelperStatus status, object value);

    //更换微信接口
    public class WxHelper
        : INotifyPropertyChanged
        , IWxAccount
    {
        public WeHttpServices wxServices = new WeHttpServices();
        public WxHelperStatusHandle WxHelperStatusChange;
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged<T>(Expression<Func<T>> property)
        {
            if (PropertyChanged == null)
                return;

            var memberExpression = property.Body as MemberExpression;
            if (memberExpression == null)
                return;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(memberExpression.Member.Name));
        }

        /// <summary>
        ///     WxHelper,帮助类
        ///     群id group_wxid : 
        ///     消息,
        ///     返回值
        /// </summary>
        //public Func<WxHelper, string, string, bool> func_11032;   //获取群成员信息
        //public Func<WxHelper, string, bool> func_11046;     //群消息获取处理,回调(this, 完整消息)
        public Func<WxHelper, GroupMembersWxPacket, bool> func_11098;   //群成员新增通知
        public Func<WxHelper, GroupMembersWxPacket, bool> func_11099;   //群成员新增通知

        //微信联系人列表.自动保存
        private WxContactsBindlite _wxContacts = new WxContactsBindlite();
        public WxContactsBindlite wxContacts { get { return _wxContacts; } }
        private WxGroupBindlite _wxGroups = new WxGroupBindlite();
        public WxGroupBindlite wxGroups { get{ return _wxGroups; } }

        //设置微信版本配置
        private static string viflag = ConfigurationManager.AppSettings["viflag"];
        private static string wxversion = ConfigurationManager.AppSettings["wxversion"];



        //private System.Timers.Timer mTimer;
        private static System.Timers.Timer m_Timer;
        private Form _view;


        public WxHelper(Form view)
        {
            //初始化微信信息接口, 回调接口
            this._view = view;

        }

        private int _clientid;
        public int clientid
        {
            get { return _clientid; }
            set
            {
                if (_clientid == value)
                    return;
                _clientid = value;
                NotifyPropertyChanged(() => clientid);
            }
        }


        //IWxAccount接口
        private string _account;
        public string account
        {
            get { return _account; }
            set
            {
                if (_account == value)
                    return;
                _account = value;
                NotifyPropertyChanged(() => account);
            }
        }

        private string _wxid = "";
        public string wxid
        {
            get { return _wxid; }
            set
            {
                if (_wxid == value)
                    return;
                _wxid = value;
                NotifyPropertyChanged(() => wxid);
            }
        }
        private string _nickname;
        public string nickname
        {
            get { return _nickname; }
            set
            {
                if (_nickname == value)
                    return;
                _nickname = value;
                NotifyPropertyChanged(() => nickname);
            }
        }

        private string _avatar;
        public string avatar {
            get { return _avatar; }
            set
            {
                if (_avatar == value)
                    return;
                _avatar = value;
                NotifyPropertyChanged(() => avatar);
            }
        }

        private string _phone;
        public string phone {
            get { return _phone; }
            set
            {
                if (_phone == value)
                    return;
                _phone = value;
                NotifyPropertyChanged(() => phone);
            }
        }
        private int _pid = 0;
        public int pid {
            get { return _pid; }
            set
            {
                if (_pid == value)
                    return;
                _pid = value;
                NotifyPropertyChanged(() => pid);
            }
        }
        private string _wx_user_dir = "";
        public string wx_user_dir {
            get { return _wx_user_dir; }
            set
            {
                if (_wx_user_dir == value)
                    return;
                _wx_user_dir = value;
                NotifyPropertyChanged(() => wx_user_dir);
            }
        }


        /// <summary>
        ///     打开微信
        /// </summary>
        public void OpenWx(Action callback)
        {
            //读取安装的注册表
            try
            {
                //检查是否已经开启
                string wepath = "";
                if (pid == 0)
                {
                    //Process prc = Process.GetProcessById(pid);
                    //if(prc != null)
                    //{
                    //    MessageBox.Show("微信正在运行中! ");
                    //    return;
                    //}

                    var regWe = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Tencent\\WeChat", true);
                    if (regWe == null)
                    {
                        MessageBox.Show("请重新安装微信! code = 100");
                        return;
                    }

                    wepath = regWe.GetValue("InstallPath").ToString();
                    if (!Directory.Exists(wepath))
                    {
                        MessageBox.Show("请重新安装微信! code = 101");
                        return;
                    }


                    //检查进程是否存在, 存在
                    Process[] processes = Process.GetProcessesByName("WeChat");
                    foreach (Process process in processes)
                    {
                        var mainmodoule = process.MainModule;
                        if (mainmodoule.FileName.IndexOf(wepath) != -1)
                        {
                            pid = process.Id;
                            break;
                        }
                    }
                }

                //启动进程
                if(pid == 0)
                {
                    Debug.WriteLine("启动wechat");
                    Process proc = System.Diagnostics.Process.Start($"{wepath}\\WeChat.exe");
                    pid = proc.Id;
                }

                Task.Factory.StartNew(() => {
                    while(true)
                    {
                        if (!wxServices.isClientConnect)
                        {
                            Thread.Sleep(3000);
                            Debug.WriteLine("启动wechat::beginMessageCallback");
                            wxServices.BeginMessageCallback();
                        }

                        var login = wxServices.checkLogin();
                        if (login.code == 1)
                        {
                            if (UiUpdataContact() >= 1)
                            {
                                Debug.WriteLine("启动wechat::hookroom");
                                wxServices.HookChatroom();
                                break;
                            }
                        }

                        //如果进程不存在就退出
                        try
                        {
                            Process pwe = Process.GetProcessById(pid);
                        }
                        catch
                        {
                            Debug.WriteLine("we进程已退出");
                            break;
                        }

                        Thread.Sleep(1000);
                    }

                    callback();
                });

              
             
            }
            catch(Exception ex)
            {
                _view.GetLogBindLite().Add(Log.Create($"OpenWx::异常",  ex.Message));
            }
        }


        //刷新联系人
        public int UiUpdataContact()
        {
            int count = 0;
            try
            {
                var selfinfo = wxServices.GetSelfInfo();
                Debug.WriteLine($"UpdataContact::{JsonConvert.SerializeObject(selfinfo)}");
                _view.Invoke(new Action(() => {
                    this.account = selfinfo.data.name;
                    this.wxid = selfinfo.data.wxid;
                    this.nickname = selfinfo.data.name;
                }));

                var contacts = wxServices.GetContacts();
                count = contacts.data.Count;
                if(count > 0)
                {
                    foreach (var contact in contacts.data)
                    {
                        if (contact.wxid.IndexOf("@chatroom") >= 0)
                        {
                            _view.Invoke(new Action(() => {
                                wxGroups.Add(new WxGroup()
                                {
                                    nickname = contact.nick_name,
                                    WeMainId = selfinfo.data.wxid,
                                    wxid = contact.wxid });
                            }));
                        }
                        else
                        {
                            _view.Invoke(new Action(() => {
                                WxContacts v = new WxContacts();
                                v.WeMainId = this.wxid;
                                v.WeMainNikeName = selfinfo.data.name;
                                v.wxid = contact.wxid;
                                v.nickname = contact.nick_name;
                                v.ctype = Model.ContactType.contact;
                                wxContacts.Add(v);
                            }));

                        }
                    }
                }
            }
            catch(Exception ex)
            {

            }
            return count;
        }


        /// <summary>
        ///     消息处理
        /// </summary>
        private void MsgProcessing()
        {

        }

        /// <summary>
        ///     消息处理
        /// </summary>
        /// <param name="msg"></param>
        private void ShowMsg(string msg)
        {
            _view.BeginInvoke(new Action(() =>
            {
                try
                {
                    var js = new System.Web.Script.Serialization.JavaScriptSerializer();
                    var jarr = js.Deserialize<Dictionary<string, object>>(msg);
                    int n = jarr.Count();
                    if (n > 0)
                    {
                        _clientid = Convert.ToInt32(jarr["clientid"]);
                        string type = jarr["type"].ToString();

                        JObject jObj = JObject.Parse(msg);
                        switch(type)//type != "11046" && type != "11089" && type != "11047"
                        {
                            case "90001":
                                Debug.WriteLine("客户端进入::" + msg);
                                break;
                            case "11024":
                                Debug.WriteLine("客户端进入::" + msg);
                                break;
                            case "11088":
                                Debug.WriteLine("客户端进入::" + msg);
                                break;
                            //case "11025"://登陆成功
                            //    bindloginusermes(msg);   //绑定一些信息
                            //    CallFriendList_11030();         // 获得好友列表（发送指令）
                            //    CallGroup_11031();              //获得群列表（发送指令）
                            //    //ShowLog(_view, "Wx登录成功");
                            //    //_view.btnOpenWx.Enabled = true;
                            //    _view.GetLogBindLite().Add(Log.Create($"消息::微信登录::{wxid}::{nickname}", "成功"));
                            //    //_view.(_view, "微信登录", $"{_mutualData.wxId}");
                            //    break;
                            case "11030":   //获得所有好友
                                if (_clientid == 0)
                                {
                                    break;
                                }
                                var wxContact = JsonConvert.DeserializeObject<WxContactsBindlite>(jObj["data"].ToString());
                                if (wxContact != null && wxContact.Count > 0)
                                {
                                    foreach (var v in wxContact)
                                    {
                                        v.WeMainId = this.wxid;
                                        v.WeMainNikeName = this.nickname;
                                        v.ctype = Model.ContactType.contact;
                                        wxContacts.Add(v);
                                    }
                                }
                                //ShowLog(_view, "获得好友");
                                _view.GetLogBindLite().Add(Log.Create($"消息::{nickname}-{wxid}::获取所有好友", "成功"));
                                break;
                            case "11031":       //获取群列表
                                if (_clientid == 0)
                                {
                                    break;
                                }
                                var groups = JsonConvert.DeserializeObject<BindingList<WxGroup>>(jObj["data"].ToString());
                                if (groups != null && groups.Count > 0)
                                {
                                    foreach (var v in groups)
                                    {
                                        v.WeMainId = this.wxid;
                                        wxGroups.Add(v);
                                    }
                                }
                                _view.GetLogBindLite().Add(Log.Create($"消息::{nickname}-{wxid}::获取群列表",$"{jObj["data"].ToString()}"));
                                break;
                            //case "11032": //获取群组成员信息
                            //    {
                            //        if (_clientid == 0)
                            //            break;
                                    
                            //        func_11032(this
                            //            , jObj["data"]["group_wxid"].ToString()
                            //            , jObj["data"]["member_list"].ToString());
                            //        break;
                            //    }
                            case "11046":
                                {//群消息
                                    //if (_clientid == 0)
                                    //    break;
                                    //func_11046(this, msg);
                                    break;
                                }
                            case "11098":
                                {
                                    //群成员新增通知
                                    if (_clientid == 0)
                                        break;
                                    try
                                    {
                                        //解析
                                        GroupMembersWxPacket packaget = new GroupMembersWxPacket();
                                        packaget.avatar = jObj["data"]["avatar"].ToString();
                                        packaget.is_mamager = jObj["data"]["is_manager"].ToInt32();
                                        packaget.nickname = jObj["data"]["nickname"].ToString();
                                        packaget.room_wxid = jObj["data"]["room_wxid"].ToString();
                                        packaget.total_member = jObj["data"]["total_member"].ToInt32();
                                        try
                                        {
                                            JArray jr = JArray.Parse(jObj["data"]["member_list"].ToString());
                                            foreach(var j in jr)
                                            {
                                                WxMemberLite mlite = new WxMemberLite() { 
                                                     nickname = j["nickname"].ToString(),
                                                      wxid = j["wxid"].ToString()
                                                };
                                                packaget.member_list.Add(mlite);
                                            }
                                        }
                                        catch { }
                                        func_11098(this, packaget);
                                    }
                                    catch (Exception ex)
                                    {
                                    }
                                    break;
                                }
                            case "11099":
                                {
                                    //群成员退出通知
                                    if (_clientid == 0)
                                        break;
                                    try
                                    {
                                        //解析
                                        GroupMembersWxPacket packaget = new GroupMembersWxPacket();
                                        packaget.avatar = jObj["data"]["avatar"].ToString();
                                        packaget.is_mamager = jObj["data"]["is_manager"].ToInt32();
                                        packaget.nickname = jObj["data"]["nickname"].ToString();
                                        packaget.room_wxid = jObj["data"]["room_wxid"].ToString();
                                        packaget.total_member = jObj["data"]["total_member"].ToInt32();
                                        try
                                        {
                                            JArray jr = JArray.Parse(jObj["data"]["member_list"].ToString());
                                            foreach (var j in jr)
                                            {
                                                WxMemberLite mlite = new WxMemberLite()
                                                {
                                                    nickname = j["nickname"].ToString(),
                                                    wxid = j["wxid"].ToString()
                                                };
                                                packaget.member_list.Add(mlite);
                                            }
                                        }
                                        catch { }
                                        if(string.IsNullOrEmpty(packaget.avatar))
                                            func_11099(this, packaget);
                                        else
                                            func_11098(this, packaget);
                                    }
                                    catch (Exception ex)
                                    {
                                    }
                                    break;
                                }
                            default:
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _view.GetLogBindLite().Add(Log.Create("消息::异常", $"{ex.Message}\r\n{ex.StackTrace}"));
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }

            }));
        }



        /// <summary>
        ///     获取微信登录信息
        ///     昵称
        ///     头像
        ///     等....
        /// </summary>
        public void bindloginusermes(string msg)
        {
            JObject jObj = JObject.Parse(msg);
            try
            {
                this.clientid = Convert.ToInt32(jObj["clientid"].ToString());
            }
            catch { this._clientid = -1; }

            this.account = jObj["data"]["account"].ToString();
            this.wxid = jObj["data"]["wxid"].ToString(); ;
            this.nickname = jObj["data"]["nickname"].ToString();
            this.avatar = jObj["data"]["avatar"].ToString();
            this.phone = jObj["data"]["phone"].ToString();

            this.pid = jObj["data"]["pid"].ToInt32();
            this.wx_user_dir = jObj["data"]["wx_user_dir"].ToString();

            FixWxVersion();
        }


        public void FixWxVersion()
        {

        }



        public void CallSendText_11036(string wxid, string text) //发送文本消息
        {
            if (_clientid > 0)
            {
              
            }
            if(MainConfigure.boterServices.RunningStatus)
            {
                this.wxServices.SendTextMsg(wxid, text);
            }
           
        }

        public void CallSendImage(string wxid, string filePath) //发送图片消息
        {
            if (_clientid > 0)
            {
                
            }
            if (MainConfigure.boterServices.RunningStatus)
            {
                this.wxServices.SendImageMsg(wxid, filePath);
            }
               
        }
    }
}
