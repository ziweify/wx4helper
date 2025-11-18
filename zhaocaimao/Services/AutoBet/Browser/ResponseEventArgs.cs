using System;

namespace zhaocaimao.Services.AutoBet.Browser
{
    /// <summary>
    /// 响应事件参数 - 与 BsBrowserClient 兼容
    /// </summary>
    public class ResponseEventArgs : EventArgs
    {
        public string SenderName { get; set; } = "";
        public string Url { get; set; } = "";
        public string ReferrerUrl { get; set; } = "";
        public string Context { get; set; } = "";
        public string PostData { get; set; } = "";
        public int StatusCode { get; set; }
        public string ContentType { get; set; } = "";
        public string? ErrorMessage { get; set; }
    }
}

