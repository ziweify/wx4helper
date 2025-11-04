using F5BotV2.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F5BotV2.MainOpenLottery
{
    //public class BgLotteryDataView
    //    : ILotteryDataView
    //{

    //    private ILotteryDataView lotteryData;

    //    public BgLotteryDataView(BgLotteryData lotteryData)
    //    {
    //        this.lotteryData = lotteryData;
    //    }

    //    [DisplayName("期号")]
    //    public int IssueId { get => ((ILotteryDataView)lotteryData).IssueId; set => ((ILotteryDataView)lotteryData).IssueId = value; }
    //    public List<LotteryNumberView> items { get => ((ILotteryDataView)lotteryData).items; set => ((ILotteryDataView)lotteryData).items = value; }
    //    public LotteryNumberView P1 { get => ((ILotteryDataView)lotteryData).P1; set => ((ILotteryDataView)lotteryData).P1 = value; }
    //    public LotteryNumberView P2 { get => ((ILotteryDataView)lotteryData).P2; set => ((ILotteryDataView)lotteryData).P2 = value; }
    //    public LotteryNumberView P3 { get => ((ILotteryDataView)lotteryData).P3; set => ((ILotteryDataView)lotteryData).P3 = value; }
    //    public LotteryNumberView P4 { get => ((ILotteryDataView)lotteryData).P4; set => ((ILotteryDataView)lotteryData).P4 = value; }
    //    public LotteryNumberView P5 { get => ((ILotteryDataView)lotteryData).P5; set => ((ILotteryDataView)lotteryData).P5 = value; }
    //    public LotteryNumberView P总 { get => ((ILotteryDataView)lotteryData).P总; set => ((ILotteryDataView)lotteryData).P总 = value; }

    //    [DisplayName("龙虎")]
    //    public NumberDragonTiger P龙虎 { get => ((ILotteryData)lotteryData).P龙虎; set => ((ILotteryData)lotteryData).P龙虎 = value; }

    //    [DisplayName("时间")]
    //    public string opentime { get => ((ILotteryData)lotteryData).opentime; set => ((ILotteryData)lotteryData).opentime = value; }
        
    //    public string lastError { get => ((ILotteryData)lotteryData).lastError; set => ((ILotteryData)lotteryData).lastError = value; }

    //    //插入前统计数据
    //    //本轮大小连续情况, 本轮单双连续情况
    //    [DisplayName("大数")]
    //    public int BLDX_Da { get; set; }

    //    [DisplayName("小数")]
    //    public int BLDX_Xiao { get; set; }
    //    [DisplayName("差数")]
    //    public int BLDX_dxSum { get; set; }  //大小和值, 大-小的值
    //    [DisplayName("总差数")]
    //    public int BLDX_dxAllSum { get; set; }  //大小和值, 大-小的值

    //    [DisplayName("单数")]
    //    public int BLDS_Dan { get; set; }
    //    [DisplayName("双数")]
    //    public int BLDS_Shuang { get; set; }
    //    [DisplayName("差数")]
    //    public int BLDS_dsSum { get; set; }  //双单的和值, 单-双的值== 正数  单多
    //    [DisplayName("总差数")]
    //    public int BLDS_dsAllSum { get; set; }  //双单的和值, 单-双的值== 正数  单多, 总差数


    //    //联动计算数据, 累计数据
    //    [DisplayName("大小连数")]
    //    public int BLDX_Lian { get; set; }  //大小连几个

    //    [DisplayName("大小跳数")]
    //    public int BLDX_Tiao { get; set; }  //大小连几个
    //    [DisplayName("大小差数")]
    //    public int BLDX_ltCount { get; set; }  //大小连几个

    //    [DisplayName("单双连数")]
    //    public int BLDS_Lian { get; set; }  //大小连几个
    //    [DisplayName("单双跳数")]
    //    public int BLDS_Tiao { get; set; }  //大小连几个
    //    [DisplayName("单双差数")]
    //    public int BLDS_ltCount { get; set; }  //大小连几个


    //    //P1号详细数据
    //    [DisplayName("P1大小差")]
    //    public int P1DX_Sum { get; set; }
    //    [DisplayName("P1大小连")]
    //    public int P1DX_Lian { get; set; }
    //    [DisplayName("P1单双差")]
    //    public int P1DS_Sum { get; set; }
    //    [DisplayName("P1单双连")]
    //    public int P1DS_Lian { get; set; }
    //    string ILotteryData.lotteryData { get => ((ILotteryData)lotteryData).lotteryData; set => ((ILotteryData)lotteryData).lotteryData = value; }

    //    public bool Parse(ILotteryData lottery)
    //    {
    //        this.IssueId = lottery.IssueId;
    //        if (items == null)
    //        {
    //            items = new List<LotteryNumber>();
    //        }
    //        return true;
    //    }

    //}
}
