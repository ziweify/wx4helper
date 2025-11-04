using LxLib.LxNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using CCWin.SkinClass;
using CefSharp.DevTools.DOM;
using LxLib.LxNet;
using F5BotV2.Model;
using System.Security.RightsManagement;

namespace F5BotV2.BetSite.Boter
{

    public class BoterApi
    {
        public static int VERIFY_SIGN_OFFTIME = 10000;  //账户过期
        public static int VERIFY_SIGN_INVALID = 10001;  //无效令牌
        public static int VERIFY_SIGN_SUCCESS = 0;      //成功

        private string _urlRoot = "http://8.134.71.102:789";    //http://8.134.59.220:789
        public BoterApiResponse<BoterLoginReponse> loginApiResponse = null;

        private string _user;
        public string user
        {
            get { return _user; }
            set { _user = value; }
        }


        /// <summary>
        ///     服务时间, 过期时间
        /// </summary>
        public DateTime OffTime { get; set; }

        private BoterApi() { 

        }

        private static BoterApi _boterApi;
        private static object _lock = new object();
        public static BoterApi GetInstance()
        {
            if(_boterApi == null)
            {
                lock(_lock)
                {
                    if(_boterApi==null)
                        _boterApi = new BoterApi();
                }
            }

            return _boterApi;
        }


        /// <summary>
        ///     获取日数据
        /// </summary>
        /// <param name="date">日期</param>
        /// <param name="limit">限制多少条</param>
        /// <param name="fill">如果填充是 true, 那么数据不够, 会从上一天拿够limit数据, 即date参数无效, 从当前最新数据开始拿</param>
        /// <returns></returns>
        [System.Reflection.ObfuscationAttribute(Feature = "Virtualization", Exclude = false)]
        public BoterApiResponse<List<BgLotteryData>>getbgday(string date, int limit, bool fill)
        {
            BoterApiResponse<List<BgLotteryData>> response = new BoterApiResponse<List<BgLotteryData>>();
            //http://192.168.110.166/api/boter/getbgday?limit=5
            string param = "";
            if (!string.IsNullOrEmpty(date))
                param += $"date={date}";
            if (!string.IsNullOrEmpty(param))
                param += "&";
            param += $"limit={limit}";
            if (!string.IsNullOrEmpty(param))
                param += "&";
            param += $"sign={loginApiResponse.data.c_sign}";
            if(fill)
                param += $"&fill=1";
            string func_url = $"{_urlRoot}/api/boter/getbgday?{param}";

            LxHttpHelper http = new LxHttpHelper();
            HttpItem item = new HttpItem()
            {
                URL = func_url,
                Method = "GET",
            };
            var hr = http.GetHtml(item);

            //解析数据
            try
            {
                //JObject jResult = JObject.Parse(hr.Html);
                //int code = jResult["code"].ToInt32();
                //string data = jResult["data"].ToString();
                var hret = JsonConvert.DeserializeObject<BoterApiResponse<List<Object>>>(hr.Html);
                if(hret != null)
                {
                    response.code = hret.code;
                    response.msg = hret.msg;
                    if(hret.data != null)
                    {
                        response.data = new List<BgLotteryData>();
                        foreach (var obj in hret.data)
                        {
                            string tmp = obj.ToString();
                            JObject d = JObject.Parse(obj.ToString());
                            string p1 = d["p1"].ToString(); if (string.IsNullOrEmpty(p1)) { p1 = "-1"; }    
                            string p2 = d["p2"].ToString(); if (string.IsNullOrEmpty(p2)) { p2 = "-1"; }
                            string p3 = d["p3"].ToString(); if (string.IsNullOrEmpty(p3)) { p3 = "-1"; }
                            string p4 = d["p4"].ToString(); if (string.IsNullOrEmpty(p4)) { p4 = "-1"; }
                            string p5 = d["p5"].ToString(); if (string.IsNullOrEmpty(p5)) { p5 = "-1"; }
                            string date_tmp = d["date"].ToString();
                            string lottery_time = d["lottery_time"].ToString();
                            int day_index = d["issue_day_index"].ToInt32();
                            int issueid = d["issueid"].ToInt32();
                            response.data.Add(new BgLotteryData().FillLotteryData(issueid, $"{p1},{p2},{p3},{p4},{p5}", lottery_time));
                        }

                        //异步回调
                    }
                }
            }
            catch(Exception ex)
            {

            }

            return response;
        }


        /// <summary>
        ///     登录, 拿到密钥
        /// </summary>
        /// <param name="user"></param>
        /// <param name="pwd"></param>
        /// <returns></returns>
        [System.Reflection.ObfuscationAttribute(Feature = "Virtualization", Exclude = false)]
        public BoterApiResponse<BoterLoginReponse> login(string user, string pwd)
        {
            this.user = user;

            LxHttpHelper http = new LxHttpHelper();
            string func_url = $"{_urlRoot}/api/boter/login?user={user}&pwd={pwd}";

            HttpItem item = new HttpItem()
            {
                URL = func_url,
                Method = "GET",
            };

            HttpResult hr = http.GetHtml(item);
            try
            {
                if (hr.StatusCode == 0)
                    throw new Exception("连接异常!");

                //创建对象解析数据
                loginApiResponse = JsonConvert.DeserializeObject<BoterApiResponse<BoterLoginReponse>>(hr.Html);

                //不创建对象解析数据
                //JObject jlogin = JObject.Parse(hr.Html);
                //_code = jlogin["code"].ToInt32();
                //_msg = jlogin["msg"].ToString();
                //if(code == 0)
                //{
                //    var data = jlogin["data"];
                //    if (data != null)
                //    {
                //        var c_off_time = jlogin["c_off_time"];
                //        var c_token_public = jlogin["c_token_public"];
                //        var c_sign = jlogin["c_sign"];
                //        return true;
                //    }
                //}

                //return false;
            }
            catch (Exception ex)
            {
                loginApiResponse = new BoterApiResponse<BoterLoginReponse>()
                {
                    code = -1,
                    data = null,
                    msg = $"login error::{ex.Message}",
                };
            }
            return loginApiResponse;
        }


        /// <summary>
        ///  密码找回
        /// </summary>
        /// <param name="user"></param>
        /// <param name="pass"></param>
        /// <param name="reserveMessage">预留信息, 预留信息正确才能够修改密码! 或者第一次预留信息为空的时候, 可以修改</param>
        [System.Reflection.ObfuscationAttribute(Feature = "Virtualization", Exclude = false)]
        public BoterApiResponse<Object> PassRecover(string name, string PassOld, string PassNew, string ReserveMessage)
        {
            string url = "";
            url = $"{_urlRoot}/api/boter/bgpass?name={name}&pwd_old={PassOld}&pwd_new={PassNew}&reserve_message={ReserveMessage}";
    
            LxHttpHelper http = new LxHttpHelper();
            HttpItem item = new HttpItem()
            {
                Method = "GET",
                URL = url,
            };
            var hr = http.GetHtml(item);
            BoterApiResponse<Object> apiret = JsonConvert.DeserializeObject<BoterApiResponse<Object>>(hr.Html);
            return apiret;
        }


        /// <summary>
        ///  获取某个期号数据
        /// </summary>
        /// <param name="issueid"></param>
        /// <returns></returns>
        [System.Reflection.ObfuscationAttribute(Feature = "Virtualization", Exclude = false)]
        public BoterApiResponse<BoterBgDataResponse> getBgdata(int issueid = 0)
        {
            string func_url = $"{_urlRoot}/api/boter/getbgData";
            string param = "";
            if(issueid > 0)
                param = $"?issueid={issueid}";
            if (string.IsNullOrEmpty(param))
                param += $"?";
            else
                param += $"&";
            param += $"sign={loginApiResponse.data.c_sign}";

            func_url = func_url + param;

            LxHttpHelper http = new LxHttpHelper();
            HttpItem item = new HttpItem()
            {
                Method = "GET",
                URL = func_url,
            };
            var hr = http.GetHtml(item);
            BoterApiResponse<BoterBgDataResponse> apiret = JsonConvert.DeserializeObject<BoterApiResponse<BoterBgDataResponse>>(hr.Html);
            return apiret;
        }
    }
}
