namespace zhaocaimao.Models
{
    /// <summary>
    /// 用户模型
    /// </summary>
    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string RealName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsVip { get; set; }
        public DateTime? VipExpireTime { get; set; }
        public decimal Balance { get; set; }
        public bool IsOnline { get; set; }
    }
}

