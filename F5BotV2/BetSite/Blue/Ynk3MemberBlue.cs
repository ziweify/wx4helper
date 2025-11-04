using CefSharp;
using F5BotV2.CefBrowser;
using F5BotV2.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F5BotV2.BetSite.Blue
{
    /// <summary>
    ///     蓝YNK3
    /// </summary>
    public class Ynk3MemberBlue
            : IBetApi
            , INotifyPropertyChanged
    {
        public string urlRoot => betApi.urlRoot;

        public float amount => betApi.amount;

        public string cookie => betApi.cookie;

        public BetSiteType betSiteType => betApi.betSiteType;

        public bool isLoginSuccess { get => betApi.isLoginSuccess; set => betApi.isLoginSuccess = value; }
        private IBetApi betApi { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;


        public Ynk3MemberBlue()
        {
           // betApi = BetSiteFactory.Create(BetSiteType.蓝A)
        }

        public BetStatus Bet(BetStandardOrderList items)
        {
            return betApi.Bet(items);
        }

        public void Cancel()
        {
            betApi.Cancel();
        }

        public void ChromeBroser_FrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
            betApi.ChromeBroser_FrameLoadEnd(sender, e);
        }

        public bool GetCurrentOpenLotteryData()
        {
            return betApi.GetCurrentOpenLotteryData();
        }

        public bool GetUserInfoUpdata()
        {
            return betApi.GetUserInfoUpdata();
        }

        public void loger(Log log)
        {
            betApi.loger(log);
        }

        public int LoginAsync(string name, string pass, IBetBrowserBase browser)
        {
            return betApi.LoginAsync(name, pass, browser);
        }

        public void SetCookie(string cookie)
        {
            betApi.SetCookie(cookie);
        }

        public bool SetRootUrl(string url)
        {
            return betApi.SetRootUrl(url);
        }

        public OddsBingo GetOdds(CarNumEnum car, BetPlayEnum play)
        {
            return betApi.GetOdds(car, play);
        }
    }
}
