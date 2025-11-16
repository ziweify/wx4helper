using System.ComponentModel;

namespace zhaocaimao.Models.Games.Binggo
{
    /// <summary>
    /// 炳狗下注单项
    /// </summary>
    public class BinggoBetItem : INotifyPropertyChanged
    {
        private int _quantity = 1;
        
        /// <summary>
        /// 车号 (1-6, 6=总和)
        /// </summary>
        public int CarNumber { get; set; }
        
        /// <summary>
        /// 玩法类型
        /// </summary>
        public BinggoPlayType PlayType { get; set; }
        
        /// <summary>
        /// 单注金额
        /// </summary>
        public decimal Amount { get; set; }
        
        /// <summary>
        /// 数量（多次下注同一项时累加）
        /// </summary>
        public int Quantity
        {
            get => _quantity;
            set
            {
                if (_quantity != value)
                {
                    _quantity = value;
                    OnPropertyChanged(nameof(Quantity));
                    OnPropertyChanged(nameof(TotalAmount));
                }
            }
        }
        
        /// <summary>
        /// 总金额 = 单注金额 × 数量
        /// </summary>
        public decimal TotalAmount => Amount * Quantity;
        
        public BinggoBetItem(int carNumber, BinggoPlayType playType, decimal amount)
        {
            CarNumber = carNumber;
            PlayType = playType;
            Amount = amount;
            Quantity = 1;
        }
        
        /// <summary>
        /// 增加数量
        /// </summary>
        public void AddQuantity()
        {
            Quantity++;
        }
        
        /// <summary>
        /// 转换为标准字符串 (例如: "1大100")
        /// 🔥 参考 F5BotV2/Boter/BoterBetContent.cs 第296行：使用 moneySum（总金额）
        /// </summary>
        public override string ToString()
        {
            return $"{CarNumber}{PlayType}{TotalAmount}";  // 🔥 使用 TotalAmount（总金额）= Amount * Quantity
        }
        
        /// <summary>
        /// 转换为回复字符串 (例如: "1大*2" 如果数量>1)
        /// </summary>
        public string ToReplyString()
        {
            var result = $"{CarNumber}{PlayType}";
            if (Quantity > 1)
            {
                result += $"*{Quantity}";
            }
            return result;
        }
        
        public event PropertyChangedEventHandler? PropertyChanged;
        
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    
    /// <summary>
    /// 炳狗玩法类型
    /// </summary>
    public enum BinggoPlayType
    {
        未知 = 0,
        大 = 1,
        小 = 2,
        单 = 3,
        双 = 4,
        尾大 = 5,
        尾小 = 6,
        合单 = 7,
        合双 = 8,
        龙 = 9,
        虎 = 10
    }
}

