using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zhaocaimao.Helpers
{
    public static class StringHelper
    {
        public static string UnEscape(this string str)
        {
            if(string.IsNullOrEmpty(str)) return str;

            string response = str.Replace("\x001E", "")
                .Replace("\x07", "")
                .Replace("\n", "")
                .Replace("\a", "")
                .Replace("\b", "")
                .Replace("\n", "")
                .Replace("\r", "")
                .Replace("\f", "")
                .Replace("\t", "");
            return response;
        }
    }
}
