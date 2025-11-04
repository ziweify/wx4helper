using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F5BotV2.CefBrowser
{
    public interface IBetBrowserBase
    {
        bool visual { get; set; }   //窗口当前显示状态
        string Cookies { get; }
        event PropertyChangedEventHandler PropertyChanged;
        ChromiumWebBrowserExtend chromeBroser { get; set; }
        Task<int> LoadUrlAsyn(string url, Func<ChromiumWebBrowserExtend, bool> finshAct);
        void Show();
        void Hide();
        void ReSetBrowser();
    }
}
