namespace zhaocaimao.Models
{
    /// <summary>
    /// InsUser 模型 - 对应图片中的用户信息
    /// </summary>
    public class InsUser
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// 账号
        /// </summary>
        public string Account { get; set; } = string.Empty;

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// 地址
        /// </summary>
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// 余额
        /// </summary>
        public decimal Balance { get; set; }

        /// <summary>
        /// 是否VIP
        /// </summary>
        public bool IsVip { get; set; }

        /// <summary>
        /// 上次登录时间
        /// </summary>
        public DateTime? LastLoginTime { get; set; }

        /// <summary>
        /// 当前时间
        /// </summary>
        public DateTime CurrentTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 秒数
        /// </summary>
        public int Seconds { get; set; }

        /// <summary>
        /// 在线状态
        /// </summary>
        public bool IsOnline { get; set; }
    }
}

