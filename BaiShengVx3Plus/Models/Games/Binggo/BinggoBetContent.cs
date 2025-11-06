using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BaiShengVx3Plus.Models.Games.Binggo
{
    /// <summary>
    /// 炳狗下注内容（包含多个下注项）
    /// </summary>
    public class BinggoBetContent
    {
        /// <summary>
        /// 期号
        /// </summary>
        public int IssueId { get; set; }
        
        /// <summary>
        /// 原始消息内容
        /// </summary>
        public string OriginalMessage { get; set; } = string.Empty;
        
        /// <summary>
        /// 解析状态码 (0=成功, -1=失败)
        /// </summary>
        public int Code { get; set; } = -1;
        
        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;
        
        /// <summary>
        /// 下注项列表
        /// </summary>
        public List<BinggoBetItem> Items { get; set; } = new List<BinggoBetItem>();
        
        /// <summary>
        /// 下注项数量
        /// </summary>
        public int Count => Items?.Count ?? 0;
        
        /// <summary>
        /// 总金额
        /// </summary>
        public decimal TotalAmount => Items?.Sum(item => item.TotalAmount) ?? 0;
        
        public BinggoBetContent(int issueId, string originalMessage)
        {
            IssueId = issueId;
            OriginalMessage = originalMessage;
        }
        
        /// <summary>
        /// 转换为标准字符串 (例如: "1大100,2小50,龙100")
        /// </summary>
        public string ToStandardString()
        {
            if (Items == null || Items.Count == 0)
                return string.Empty;
            
            var sb = new StringBuilder();
            foreach (var item in Items)
            {
                if (sb.Length > 0)
                    sb.Append(",");
                sb.Append(item.ToString());
            }
            return sb.ToString();
        }
        
        /// <summary>
        /// 转换为回复字符串 (例如: "1大*2/2小/龙")
        /// </summary>
        public string ToReplyString()
        {
            if (Items == null || Items.Count == 0)
                return string.Empty;
            
            var sb = new StringBuilder();
            foreach (var item in Items)
            {
                if (sb.Length > 0)
                    sb.Append("/");
                sb.Append(item.ToReplyString());
            }
            return sb.ToString();
        }
    }
}
