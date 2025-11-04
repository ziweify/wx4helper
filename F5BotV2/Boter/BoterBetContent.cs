using F5BotV2.BetSite;
using F5BotV2.Model;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace F5BotV2.Boter
{
   // public enum BetCarEnum { P1 = 1, P2 = 2, P3 =3, P4 = 4, P5 = 5, P总 = 6 };

    /// <summary>
    ///     投注上下文.这个是要被显示在bet列表里面的部分内容。
    /// </summary>
    public class BoterBetContents
    {
        private int _issueid;
        public int issueid { get { return _issueid; }  }

        private string _msg_origin;
        /// <summary>
        ///     消息内容
        /// </summary>
        public string msg_origin { get { return _msg_origin; } }

        private int _code = 0;  //没有错误就是0
        public int code { get { return _code; } }

        private string _lasterror;
        public string lasterror { get { return _lasterror; } }


        public List<BoterBetItem> boterItems = new List<BoterBetItem>();

        public int GetCount()
        {
            if (boterItems == null)
                return 0;
            return boterItems.Count();
        }

        public int GetAmountTatol()
        {
            if (boterItems == null)
                return 0;
            int sum = 0;
            foreach(var item in boterItems)
            {
                sum = sum + item.moneySum;
            }
            return sum;
        }


        /// <summary>
        ///     返回值其实恒定 = -1,主要是没匹配, 就恒定返回-1
        /// </summary>
        /// <param name="msg">原始消息</param>
        public BoterBetContents(int issueid, string msg_origin)
        {
            try
            {
                _code = -1;
                _issueid = issueid;
                this._msg_origin = msg_origin;
                _msg_origin = this._msg_origin.Replace('，', ',');     //把半角,转换
                _msg_origin = this._msg_origin.Replace('\r', ',');    //把换行
                //测试i
                //this._msg = "12366总和总和总大单尾小100,123大50";
                //123大100
                //一二三大100
                string[] betstring = _msg_origin.Split(',');
                foreach (string s in betstring)
                {
                    //字符串类型1: 特殊格式 123尾小50尾大100单30双60, 第一步拆分成  123尾小60,123尾大100,123单100
                    //([123456一二三四五六总和]*){1}
                    //var tmp1 = "1尾小45尾大66";
                    //var tmp2 = "1234大单100";
                    //成功匹配, 但是弃用该规则, 市场弃用
                    //string regexHead = "([123456一二三四五六总和]+){1}([大小单双尾大尾小合单合双龙虎]+)(\\d+)";  //匹配车头
                    //var rgxHead = Regex.Match(s, regexHead);
                    //var h1 = rgxHead.Groups[1].Value;
                    //var h2 = rgxHead.Groups[2].Value;
                    //var h3 = rgxHead.Groups[3].Value;

                    //string regex = "([大小单双尾大尾小合单合双龙虎]+)(\\d+)";
                    //var items = Regex.Matches(s, regex);
                    //var len = items.Count;
                    //if (len == 0)
                    //    throw new Exception("无效货单!");

                    //foreach (var item in items)
                    //{
                    //    string message = h1 + item;
                    //    //字符串类型2: 通用格式 12345大单双100  这种单一金额格式的
                    //    regex = "([123456一二三四五六总和]*){1}([大小单双尾大尾小合单合双龙虎]*)(\\d*$)";
                    //    if (Regex.IsMatch(message, regex))
                    //    {
                    //        if (ParseBetStandardString(message) == 0)   //解析单个字符串 123456大单双100 只解析这类型的字符串, 解析单个类型的
                    //        {
                    //            //要记录下，为什么没有生成订单
                    //        }

                    //        if (boterItems.Count == 0)
                    //            throw new Exception("无效货单!");
                    //        _code = 0;
                    //    }
                    //}

                    //字符串类型2: 特殊格式 123大4小2双龙虎100 => 1大100,2大100,3大100,4小100,2双100,龙100,虎100  这两种格式只能选择一种
                    //string regexHead = "([123456一二三四五六总和]+){1}([大小单双尾大尾小合单合双龙虎]+)(\\d+)";  //匹配车头
                   //var  ss = "111123大4小5单龙100";
                    string regexHead = "(([123456一二三四五六总和]+)?([大小单双尾大尾小合单合双龙虎]+))+(\\d+)";
                    var rgxHead = Regex.Match(s, regexHead);
                    var h1 = rgxHead.Groups[1].Value;
                    var h2 = rgxHead.Groups[2].Value;
                    var h3 = rgxHead.Groups[3].Value;
                    var h4 = rgxHead.Groups[4].Value;
                    string regex = "([123456一二三四五六总和]+)?([大小单双尾大尾小合单合双龙虎]+)";
                    var items = Regex.Matches(s, regex);
                    var len = items.Count;
                    if (len == 0)
                        throw new Exception("无效货单!");

                    foreach (var item in items)
                    {
                        string message = item+h4;
                        //字符串类型2: 通用格式 12345大单双100  这种单一金额格式的
                        regex = "([123456一二三四五六总和]*){1}([大小单双尾大尾小合单合双龙虎]*)(\\d*$)";
                        if (Regex.IsMatch(message, regex))
                        {
                            if (ParseBetStandardString(message) == 0)   //解析单个字符串 123456大单双100 只解析这类型的字符串, 解析单个类型的
                            {
                                //要记录下，为什么没有生成订单
                            }

                            if (boterItems.Count == 0)
                                throw new Exception("无效货单!");
                            _code = 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _lasterror = ex.Message;
                _code = -1;
            }
        }


        /// <summary>
        ///     string格式: 123456大单双100 只解析这类型的字符串, 解析单个类型的
        /// </summary>
        /// <param name="betString"></param>
        /// <returns>返回注单数</returns>
        public int ParseBetStandardString(string betString)
        {
            int reponse = 0;
            string text = betString.Replace(" ", "");   //排除空格
            string regex = "([123456一二三四五六总和]*){1}([大小单双尾大尾小合单合双龙虎]*)(\\d*)([^#]*)";
            var items = Regex.Match(text, regex);
            var s0 = items.Groups[0].Value;
            var s1 = items.Groups[1].Value;
            var s2 = items.Groups[2].Value;
            var s3 = items.Groups[3].Value;
            var s4 = items.Groups[4].Value;

            //分解成每个单独的字符串,语句解析
            //解析成标准的下单字符串，| 分隔
            StringBuilder sbBets = new StringBuilder(128);
            List<CarNumEnum> cars = new List<CarNumEnum>();
            string strCars = s1.Replace("总和", "6").Replace("总", "6")
                .Replace("一", "1")
                .Replace("二", "2")
                .Replace("三", "3")
                .Replace("四", "4")
                .Replace("五", "5")
                .Replace("六", "6");
            if (string.IsNullOrEmpty(strCars))
            {
                strCars = "6";
            }

            try
            {
                foreach (char c in strCars)
                {
                    string s_tmp = c.ToString();
                    //BetCarEnum betcar = (BetCarEnum)Enum.Parse(typeof(BetCarEnum), s);
                    CarNumEnum betcar = (CarNumEnum)Convert.ToInt32(s_tmp);
                    cars.Add(betcar);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"校验失败,{s1},请检查后重新开始!");
            }

            List<BetPlayEnum> plays = new List<BetPlayEnum>();
            string strPlays = s2.Replace("尾大", "5")
                .Replace("尾小", "6")
                .Replace("合单", "7")
                .Replace("合双", "8");
            //.Replace("龙", "9")
            //.Replace("虎", "A")      //特殊处理..用两种方法处理
            //.Replace("大", "1")
            //.Replace("小", "2")
            //.Replace("单", "3")
            //.Replace("双", "4");
            try
            {
                foreach (var strPlay in strPlays)
                {
                    BetPlayEnum play = BetPlayEnum.未知;
                    string tmp = strPlay.ToString();
                    if (Regex.IsMatch(tmp, @"^\d+$"))
                    {
                        play = (BetPlayEnum)Convert.ToInt32(tmp);
                    }
                    else
                    {
                        play = (BetPlayEnum)Enum.Parse(typeof(BetPlayEnum), tmp); //(BetPlayEnum)Convert.ToInt32(tmp);
                    }
                    if (play == BetPlayEnum.未知)
                        throw new Exception($"校验失败,{s2},请检查后重新开始!");
                    plays.Add(play);
                }

            }
            catch (Exception ex)
            {
                throw new Exception($"校验失败,{s2},请检查后重新开始!");
            }
            int money = 0;
            try
            {
                money = Convert.ToInt32(s3);
            }
            catch { throw new Exception($"货单校验失败,{s3}"); }

            //开始组装订单
            foreach (var play in plays)
            {
                if (play == BetPlayEnum.龙 || play == BetPlayEnum.虎)
                {
                    var item = boterItems.FirstOrDefault(p => p.car == CarNumEnum.P总 && p.play == play);
                    if (item != null)
                    {
                        item.numberAdd();
                        reponse++;
                    }
                    else
                    {
                        boterItems.Add(new BoterBetItem(issueid, CarNumEnum.P总, play, money));
                        reponse++;
                    }
                    continue;
                }



                foreach (var c in cars)
                {
                    var item = boterItems.FirstOrDefault(p => p.car == c && p.play == play);
                    if (item != null)
                    {
                        item.numberAdd();
                        reponse++;
                    }
                    else
                    {
                        boterItems.Add(new BoterBetItem(issueid, c, play, money));
                        reponse++;
                    } 
                }
            }

            return reponse;
        }

        public string ToStandarString()
        {
            StringBuilder sb = new StringBuilder();
            foreach(var item in boterItems)
            {
                if (sb.Length > 0)
                    sb.Append(",");
                sb.Append($"{(int)item.car}{item.play.ToString()}{item.moneySum}");
            }
            return sb.ToString();
        }

        /// <summary>
        ///     回复客户消息的数据
        /// </summary>
        /// <returns></returns>
        public string ToReplyString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in boterItems)
            {
                if (sb.Length > 0)
                    sb.Append("/");
                sb.Append($"{(int)item.car}{item.play.ToString()}");
                if (item.numbers > 1)
                    sb.Append($"*{item.numbers}");
            }
            return sb.ToString();
        }

    }

    public class BoterBetItem
        : IBetOrder
    {
        public BoterBetItem(int issueid, CarNumEnum car, BetPlayEnum play, int money)
        {
            this._car = car;
            this._play = play;
            this._money = money;
            this._numbers = 1;
            this._moneySum = money * this.numbers;
            this.IssueId = issueid;
        }

        private CarNumEnum _car;
        public CarNumEnum car
        {
            get { return _car; }
            set
            {
                if (_car == value)
                    return;
                _car = value;
            }
        } //车号

        private BetPlayEnum _play;
        public BetPlayEnum play { get { return _play; }
            set
            {
                if (_play == value)
                    return;
                _play = value;
            }
        }    //玩法

        private int _numbers = 0;
        public int numbers
        {
            get { return _numbers; }
            set
            {
                if (_numbers == value)
                    return;
                _numbers = value;
            }
        }    //数量,一般是1, 数量*金额 = 实际金额

        private int _money = 0;
        public int money { get { return _money; } }          //金额

        private int _moneySum = 0;
        public int moneySum
        {
            get { return _moneySum; }
            set
            {
                if (_moneySum == value)
                    return;
                _moneySum = value;
            }
        }   //总金额

        public int IssueId { get; set; }

        public void numberAdd()
        {
            _numbers++;
            _moneySum = _money * _numbers;
        }

        public string ToString()
        {
            return $"{car}|{play}|{money}";
        }

        //开奖
        public float OpenLottery(BgLotteryData data,  float odds = 1.97f, bool isZsjs = false)
        {
            float win = 0;

            if(data.IssueId != this.IssueId)
            {
                throw new Exception("开奖数据期号与订单不符!");
            }

                var number = data.GetCarNumber(car);
            if (play == BetPlayEnum.大 || play == BetPlayEnum.小)
            {
                if (number.dx.ToString() == play.ToString())
                {
                    win = numbers * money * odds;
                }
            }
            else if (play == BetPlayEnum.单 || play == BetPlayEnum.双)
            {
                if (number.ds.ToString() == play.ToString())
                {
                    win = numbers * money * odds;
                }
            }
            else if (play == BetPlayEnum.尾大 || play == BetPlayEnum.尾小)
            {
                if (number.wdx.ToString() == play.ToString())
                {
                    win = numbers * money * odds;
                }
            }
            else if (play == BetPlayEnum.合单 || play == BetPlayEnum.合双)
            {
                if (number.hds.ToString() == play.ToString())
                {
                    win = numbers * money * odds;
                }
            }
            else if (play == BetPlayEnum.龙 || play == BetPlayEnum.虎)
            {
                if (data.P龙虎.ToString() == play.ToString())
                {
                    win = numbers * money * odds;
                }
            }
            else
            {
                throw new Exception("结算错误!");
            }

            //判断是否需要进行整数结算
            if(isZsjs)
            {
                win = (int)win;
            }
            return win;
        }
    }
}
