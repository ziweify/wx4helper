using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F5BotV2.CefBrowser
{
    public class CefCookieVisitor : CefSharp.ICookieVisitor
    {
        public event Action<CefSharp.Cookie> SendCookie;
        public bool Visit(CefSharp.Cookie cookie, int count, int total, ref bool deleteCookie)
        {
            deleteCookie = false;
            if (SendCookie != null)
            {
                SendCookie(cookie);
            }
            return true;
        }
        public void Dispose()
        {

        }
    }
}
