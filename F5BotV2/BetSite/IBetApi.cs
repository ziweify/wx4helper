using CefSharp;
using CefSharp.DevTools.FedCm;
using F5BotV2.CefBrowser;
using F5BotV2.Main;
using F5BotV2.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace F5BotV2.BetSite
{
    /// <summary>
    ///     投注的订单
    /// </summary>

    ///转换给投注的
    public interface IBetOrder
    {
        int IssueId { get; set; }

        CarNumEnum car { get; set; }

        BetPlayEnum play { get; set; }    //玩法

        int moneySum { get; set; }

    }

    public enum BetStatus { 未知 = 0, 成功 = 1, 失败 = 2, 异常 = 3,  赔率获取失败 = 4, 没有数据 = 5}


    /// <summary>
    ///     提供帮助函数的, 计算, 过滤, 等
    /// </summary>
    public class BetStandardOrderList
        : List<BetStandardOrder>
    {
        public int GetAmountTatol()
        {
            int result = 0;
            foreach(var item in this)
            {
                result += item.moneySum;
            }
            return result;
        }
    }

    public class BetStandardOrder
        : IBetOrder
    {
        public int IssueId { get; set; }
        public CarNumEnum car { get; set; }
        public BetPlayEnum play { get; set; }
        public int moneySum { get; set; }
        public float Odds { get; set; } //网站的赔率

        public BetStandardOrder(int IssueId, CarNumEnum car, BetPlayEnum play, int monsySum)
        {
            this.IssueId = IssueId;
            this.car = car;
            this.play = play;
            this.moneySum = monsySum;
        }

        public BetStandardOrder(IBetOrder betorder)
        {
            this.IssueId = betorder.IssueId;
            this.car = betorder.car;
            this.play = betorder.play;
            this.moneySum = betorder.moneySum;
        }
    }


    public interface IBetApi
    {
        /// <summary>
        ///     url,必须以 / 结尾
        /// </summary>
        string urlRoot { get; }

        float amount { get; }

        /// <summary>
        ///     
        /// </summary>
        string cookie { get; }

        BetSiteType betSiteType { get; }

        bool isLoginSuccess { get; set; }       //是否已经登录成功, 外部会监控这个变量做动作, 例如提示登录, 等


        /// <summary>
        ///     重新设置URL..完整的url. 必须以 / 结尾
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        bool SetRootUrl(string url);
        void SetCookie(string cookie);

        void Cancel();  //退出所有的线程等待


        int LoginAsync(string name, string pass, IBetBrowserBase browser);
        bool GetCurrentOpenLotteryData();
        bool GetUserInfoUpdata();

        BetStatus Bet(BetStandardOrderList items);
        OddsBingo GetOdds(CarNumEnum car, BetPlayEnum play); //获取赔率


        /// <summary>
        ///     记录日志
        /// </summary>
        /// <param name="log"></param>
        void loger(Log log);
         

        //网页浏览器版，才有这个方法
        void ChromeBroser_FrameLoadEnd(object sender, FrameLoadEndEventArgs e);
    }


    public static class BetApiBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="api"></param>
        /// <param name="rootUrl">url不带 \ 如果带了, 系统默认抹除</param>
        /// <param name="pathUrl">带 \ 开头</param>
        /// <returns></returns>
        public static string MakeUrl(this IBetApi api, string rootUrl, string pathUrl)
        {
            string result = "";

            if (string.IsNullOrEmpty(rootUrl))
                return "";

            try
            {
                while (rootUrl.LastOrDefault() == '/' || rootUrl.LastOrDefault() == '\\')
                {
                    rootUrl = rootUrl.Remove(rootUrl.Length - 1, 1);
                }

                if (!string.IsNullOrEmpty(pathUrl))
                {
                    pathUrl = pathUrl.Replace('\\', '/').Replace(" ", "");
                    var first = pathUrl.FirstOrDefault();
                    if (first == '/')
                    {

                    }
                    else
                    {
                        pathUrl = $"/{pathUrl}";
                    }
                }

               result = $"{rootUrl}{pathUrl}";
            }
            catch(Exception ex)
            {
                result = "";
            }

            return result;
        }
    }


    public class BetApi
        : IBetApi
    {
        public BetApi(BetSiteType bstype) { 
            this._betSiteType = bstype;
        }

        /// <summary>
        ///     站点根目录, 不带/结尾
        /// </summary>
        private string _urlRoot = "";
        public string urlRoot { get { return _urlRoot; } }

        private string _cookie = "";

        public string cookie { get { return _cookie; } }

        public int LoginAsync(string name, string pass, IBetBrowserBase browser) { return 0; }

        private BetSiteType _betSiteType;
        public BetSiteType betSiteType{ get { return _betSiteType; } }


        private bool _isLoginSuccess = false;
        public bool isLoginSuccess { get { return _isLoginSuccess; } }

        bool IBetApi.isLoginSuccess { get; set; }

        public float amount => throw new NotImplementedException();

        public void SetCookie(string cookie)
        {
            this._cookie = cookie;
        }

        /// <summary>
        ///     设置的url
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public bool SetRootUrl(string url)
        {
            //if(Regex.IsMatch(url, "(http://)|(https://).*/"))
            //不检测 / 
            if (Regex.IsMatch(url, "(http://)|(https://).*"))
            {
                if(url.Last() == '/')
                {
                    url =  url.Remove(url.Length - 1, 1);
                }
                _urlRoot = url;
                return true;
            }
            return false;
        }

        public void ChromeBroser_FrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
            return;
        }

        public BetStatus Bet(BetStandardOrderList items)
        {
            BetStatus status = BetStatus.未知;
            return status;
        }

        public void loger(Log log)
        {
            //记录日志
            MainConfigure.boterServices.loglite.Add(log);
        }

        public bool GetCurrentOpenLotteryData()
        {
            throw new NotImplementedException();
        }

        public bool GetUserInfoUpdata()
        {
            throw new NotImplementedException();
        }

        public void Cancel()
        {
            throw new NotImplementedException();
        }

        public OddsBingo GetOdds(CarNumEnum car, BetPlayEnum play)
        {
            throw new NotImplementedException();
        }
    }
}
