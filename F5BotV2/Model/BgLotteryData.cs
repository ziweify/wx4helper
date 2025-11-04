using F5BotV2.BetSite;
using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace F5BotV2.Model
{
    public interface ILotteryData
    {
        /// <summary>
        ///     期号ID,用做主键
        /// </summary>
        int IssueId { get; set; }

        string lotteryData { get; set; }

        [Ignore]
        List<LotteryNumber> items { get; set; }

         LotteryNumber P1 { get; set; }
         LotteryNumber P2 { get; set; }
         LotteryNumber P3 { get; set; }
         LotteryNumber P4 { get; set; }
         LotteryNumber P5 { get; set; }
         LotteryNumber P总 { get; set; }
         NumberDragonTiger P龙虎 { get; set; }

        /// <summary>
        ///     开奖时间戳
        /// </summary>
         string opentime { get; set; }

        /// <summary>
        ///     最后一次错误信息
        /// </summary>
         string lastError { get; set; }
    }



    public enum BetPlayEnum
    {
        未知 = 0
        , 单 = 1
        , 双 = 2
        , 大 = 3
        , 小 = 4
        , 尾大 = 5    //尾大尾小这几个不能用10位数
        , 尾小 = 6
        , 合单 = 7
        , 合双 = 8  //这个代表 1-5车的两个数之和, 并不是总和
        , 龙 = 9
        , 虎 = 10
        , 福 = 11
        , 禄 = 12 
        , 寿 = 13
        , 喜 = 14
    }


    public enum NumberDX { 未知 = 3, 小 = 0, 大 = 1}
    public enum NumberDS { 未知 = 3, 双 = 0, 单 = 1}
    public enum NumberWDX { 未知 = 3, 尾小 = 0, 尾大 = 1}     //尾大小
    public enum NumberHDS { 未知 = 3, 合双 = 0, 合单 = 1 }     //合单双
    //public enum BetCarEnum { P1 = 1, P2 = 2, P3 =3, P4 = 4, P5 = 5, P总 = 6 };
    public enum CarNumEnum { 未知 = 0, P1 = 1, P2 = 2, P3 = 3, P4 = 4, P5 = 5, P总 = 6 }
    public enum NumberDragonTiger { 未知 = -1, 虎 = 0, 龙 = 1}


    public class BgLotteryDataComparer : IComparer<BgLotteryData>
    {
        public int Compare(BgLotteryData x, BgLotteryData y)
        {
            if (object.ReferenceEquals(x, y))
            {
                return 0;
            }
            //从1号车在前面, 大的在后面
            if (x.IssueId > y.IssueId)
                return 1;
            if (x.IssueId < y.IssueId)
                return -1;

            ////如果等于
            //if (x.play > y.play)
            //    return 1;
            //if (x.play < y.play)
            //    return -1;


            return 0;
        }
    }

    public class BgLotteryData
        : ILotteryData
        //, INotifyPropertyChanged
    {

        public BgLotteryData()
        {
            //items = new List<LotteryNumber>();
        }

        public LotteryNumber GetCarNumber(CarNumEnum car)
        {
            switch(car)
            {
                case CarNumEnum.P1:
                    return P1;
                case CarNumEnum.P2:
                    return P2;
                case CarNumEnum.P3:
                    return P3;
                case CarNumEnum.P4:
                    return P4;
                case CarNumEnum.P5:
                    return P5;
                case CarNumEnum.P总:
                    return P总;
                default:
                    return null;
            }
        }

        public BgLotteryData FillLotteryData(int issueId, string lotteryData, string openTime)
        {
            try
            {
                this.IssueId = issueId;
                this.lotteryData = lotteryData;

                items = new List<LotteryNumber>();
                this.opentime = openTime;
                string[] data = lotteryData.Split(',');
                if (data.Count() >= 5)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        int number = Convert.ToInt32(data[i]);
                        items.Add(new LotteryNumber((CarNumEnum)i, number));
                    }

                    P1 = items[0];
                    P2 = items[1];
                    P3 = items[2];
                    P4 = items[3];
                    P5 = items[4];

                    //处理总和数据
                    items.Add(new LotteryNumber(CarNumEnum.P总, P1.number + P2.number + P3.number + P4.number + P5.number));
                    P总 = items[5];
                    if (P1.number > P5.number)
                    {
                        P龙虎 = NumberDragonTiger.龙;
                    }
                    else
                    {
                        P龙虎 = NumberDragonTiger.虎;
                    }
                }

                return this;
            }
            catch (Exception ex)
            {
                lastError = string.Format("issueId={0},lotteryData={1},openTimestamp={2},msg={3}"
                    , IssueId
                    , lotteryData
                    , openTime
                    , ex.Message);
                
            }
            return this;
        }

        /// <summary>
        ///     开奖期号, ID
        /// </summary>
        [PrimaryKey]
        public int IssueId { get; set; }

        public string lotteryData { get; set; }

        /// <summary>
        ///     做成一个list是为了后续的数据检测, 公示搜索, 匹配, 等操作
        /// </summary>
        [Ignore]
        public List<LotteryNumber> items { get; set; }
        [Ignore]
        public LotteryNumber P1 { get; set; }
        [Ignore]
        public LotteryNumber P2 { get; set; }
        [Ignore]
        public LotteryNumber P3 { get; set; }
        [Ignore]
        public LotteryNumber P4 { get; set; }
        [Ignore]
        public LotteryNumber P5 { get; set; }
        [Ignore]
        public LotteryNumber P总 { get; set; }
        [Ignore]
        public NumberDragonTiger P龙虎 { get; set; }

        /// <summary>
        ///     开奖时间戳
        /// </summary>
        [DisplayName("时间")]
        public string opentime { get; set; }

        /// <summary>
        ///     最后一次错误信息
        /// </summary>
        public string lastError { get; set; }


        public BgLotteryData Updata(BgLotteryData data)
        {
            PropertyInfo[] propertys = data.GetType().GetProperties();
            PropertyInfo[] piThis = this.GetType().GetProperties();
            foreach (PropertyInfo pi in propertys)
            {
                object value1 = pi.GetValue(data, null); //用pi.GetValue获得值
                string name = pi.Name;                  //获得属性的名字,后面就可以根据名字判断来进行些自己想要的操作
                this.GetType().GetProperty(name).SetValue(this, value1);
            }
            return this;
        }


        public string ToLotteryString()
        {
            string response = "";
            try
            {
                response = $"{P1.number},{P2.number},{P3.number},{P4.number},{P5.number} {P总.dx} {P总.ds} {P龙虎}";
            }
            catch
            {
                response = $"0,0,0,0,0 * * *";

            }
            return response;
        }
    }


}
