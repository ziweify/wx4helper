using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F5BotV2.BetSite.Boter
{
    //boterapi调用返回的标准格式
    public class BoterApiResponse<T>
    {
        public int code { get; set; }
        public string msg { get; set; }
        public T data { get; set; }
    }
}
