using CefSharp;
using F5BotV2.CefBrowser;
using F5BotV2.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F5BotV2.BetSite.yyz168
{
    public class NoneSite
        : IBetApi
    {
        IBetApi BetApi { get; set; }

        public string urlRoot => BetApi.urlRoot;

        public string cookie => BetApi.cookie;

        public BetSiteType betSiteType => BetApi.betSiteType;

        public bool isLoginSuccess { get => BetApi.isLoginSuccess; set => BetApi.isLoginSuccess = value; }

        public float amount => throw new NotImplementedException();

        public NoneSite()
        {
            BetApi = new BetApi( BetSiteType.不使用盘口);
        }

        public int LoginAsync(string name, string pass, IBetBrowserBase browser)
        {
            return 0;
        }

        public bool SetRootUrl(string url)
        {
            return BetApi.SetRootUrl(url);
        }

        public void SetCookie(string cookie)
        {
            BetApi.SetCookie(cookie);
        }

        public void ChromeBroser_FrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
            return;
        }

        public BetStatus Bet(BetStandardOrderList items)
        {
            BetStatus status = BetStatus.未知;
            BetApi.Bet(items);
            return status;
        }

        public void loger(Log log)
        {
            BetApi.loger(log);
        }

        public bool GetCurrentOpenLotteryData()
        {
            return true;
        }

        public bool GetUserInfoUpdata()
        {
            return true;
        }

        public void Cancel()
        {
            return;
        }

        public OddsBingo GetOdds(CarNumEnum car, BetPlayEnum play)
        {
            return BetApi.GetOdds(car, play);
        }
    }
}
