using CCWin.SkinClass;
using F5BotV2.Model;
using F5BotV2.Model.Attribute;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F5BotV2.MainOpenLottery
{
    public class BgLotteryViewBindlist
        : BindingList<BgLotteryView>
    {
        //public void Insert(BgLotteryView view)
        //{

        //}
    }



    //开建数据
    public class BgLotteryView
    {
        private ILotteryData _bgdata = null; //当前期数据

        [DataGridSetting("", false, 35)]
        public ILotteryData bgdata { get { return _bgdata; } }



        /// <summary>
        ///     
        /// </summary>
        /// <param name="bgdata">当前数据</param>
        /// <param name="lastBgdata">相对于当前对象的上一次数据</param>
        public BgLotteryView(ILotteryData bgdata, BgLotteryView last)
        {
            this._bgdata = bgdata;
            try
            {
                _items = new List<LotteryNumberView>();
                //重构数据
                if (bgdata.items != null)
                {
                    int issueid = bgdata.IssueId;
                    _items.Add(new LotteryNumberView(issueid, _bgdata.P1, last == null ? null : last.P1));
                    _items.Add(new LotteryNumberView(issueid, _bgdata.P2, last == null ? null : last.P2));
                    _items.Add(new LotteryNumberView(issueid, _bgdata.P3, last == null ? null : last.P3));
                    _items.Add(new LotteryNumberView(issueid, _bgdata.P4, last == null ? null : last.P4));
                    _items.Add(new LotteryNumberView(issueid, _bgdata.P5, last == null ? null : last.P5));
                    _items.Add(new LotteryNumberView(issueid, _bgdata.P总, last == null ? null : last.P总));

                    _P1 = items[0];
                    _P2 = items[1];
                    _P3 = items[2];
                    _P4 = items[3];
                    _P5 = items[4];
                    _P总 = items[5];

                    if (_P1.number > _P5.number)
                        P龙虎 = NumberDragonTiger.龙;
                    else
                        P龙虎 = NumberDragonTiger.虎;

                    //计算数据
                    lite = string.Format("{0:f3}", dx_lian_rate_lite()) ;
                    full = string.Format("{0:f3}", dx_lian_rate_full());

                    //计算统计数据，连跳。总
                    lian_total = P1.大小数据.lian_count + P2.大小数据.lian_count + P3.大小数据.lian_count + P4.大小数据.lian_count + P5.大小数据.lian_count + P总.大小数据.lian_count;
                    tiao_total = P1.大小数据.tiao_count + P2.大小数据.tiao_count + P3.大小数据.tiao_count + P4.大小数据.tiao_count + P5.大小数据.tiao_count + P总.大小数据.tiao_count;
                    lian_overflow = P1.大小数据.lian_overflow + P2.大小数据.lian_overflow + P3.大小数据.lian_overflow + P4.大小数据.lian_overflow + P5.大小数据.lian_overflow + P总.大小数据.lian_overflow;
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                if (_P1 == null) { _P1 = new LotteryNumberView(-1, new LotteryNumber(CarNumEnum.未知, 0), null); }
                if (_P2 == null) { _P2 = new LotteryNumberView(-1, new LotteryNumber(CarNumEnum.未知, 0), null); }
                if (_P3 == null) { _P3 = new LotteryNumberView(-1, new LotteryNumber(CarNumEnum.未知, 0), null); }
                if (_P4 == null) { _P4 = new LotteryNumberView(-1, new LotteryNumber(CarNumEnum.未知, 0), null); }
                if (_P5 == null) { _P5 = new LotteryNumberView(-1, new LotteryNumber(CarNumEnum.未知, 0), null); }
                if (_P总 == null) { _P总 = new LotteryNumberView(-1, new LotteryNumber(CarNumEnum.未知, 0), null); }
            }
        }

        [DataGridSetting("期号", true, 70)]
        public int IssueId { get => bgdata.IssueId; set => bgdata.IssueId = value; }
        [DataGridSetting("", false, 35)]
        public string lotteryData { get => bgdata.lotteryData; set => bgdata.lotteryData = value; }

        private List<LotteryNumberView> _items = null;
        [DataGridSetting("items", false, 35)]
        public List<LotteryNumberView> items { get { return _items; } }

        private LotteryNumberView _P1;
        [DataGridSetting("P1", true, 26)]
        public LotteryNumberView P1 { get { return _P1; } }
        [DataGridSetting("大", true, 13)]
        public int p1dx { get { return (int)P1.大小数据.dx; } }
        [DataGridSetting("尾", true, 13)]
        public int p1wdx { get { return (int)P1.尾大小数据.wdx; } }
        [DataGridSetting("单", true, 13)]
        public int p1ds { get { return (int)P1.单双数据.ds; } }
        [DataGridSetting("和", true, 13)]
        public int p1wds { get { return (int)P1.和值单双数据.hds; } }
        [DataGridSetting("路", true, 21)]
        public int p1dxCount { get { return (int)P1.大小数据.count; } }
        [DataGridSetting("路", true, 21)]
        public int p1wdxCount { get { return (int)P1.尾大小数据.count; } }
        [DataGridSetting("路", true, 21)]
        public int p1dsCount { get { return (int)P1.单双数据.count; } }
         [DataGridSetting("路", true, 21)]
        public int p1hdsCount { get { return (int)P1.和值单双数据.count; } }
        [DataGridSetting("数", true, 26)]
        public int p1dx_count { get { return (int)P1.数量数据.dx_d_total; } }  //大小-大的总数
        [DataGridSetting("数", true, 26)]
        public int p1wdx_count { get { return (int)P1.数量数据.wdx_d_total; } }  //大小-大的总数
        [DataGridSetting("数", true, 26)]
        public int p1ds_count { get { return (int)P1.数量数据.ds_d_total; } }  //单双-单的总数
        [DataGridSetting("数", true, 26)]
        public int p1hds_count { get { return (int)P1.数量数据.hds_d_total; } }  //单双-单的总数



        private LotteryNumberView _P2;
        [DataGridSetting("P2", true, 35)]
        public LotteryNumberView P2 { get { return _P2; } }
        [DataGridSetting("大", true, 13)]
        public int p2dx { get { return (int)P2.大小数据.dx; } }
        [DataGridSetting("尾", true, 13)]
        public int p2wdx { get { return (int)P2.尾大小数据.wdx; } }
        [DataGridSetting("单", true, 13)]
        public int p2ds { get { return (int)P2.单双数据.ds; } }
        [DataGridSetting("和", true, 13)]
        public int p2wds { get { return (int)P2.和值单双数据.hds; } }
        [DataGridSetting("路", true, 21)]
        public int p2dxCount { get { return (int)P2.大小数据.count; } }
        [DataGridSetting("路", true, 21)]
        public int p2wdxCount { get { return (int)P2.尾大小数据.count; } }
        [DataGridSetting("路", true, 21)]
        public int p2dsCount { get { return (int)P2.单双数据.count; } }
        [DataGridSetting("路", true, 21)]
        public int p2hdsCount { get { return (int)P2.和值单双数据.count; } }
        [DataGridSetting("数", true, 26)]
        public int p2dx_count { get { return (int)P2.数量数据.dx_d_total; } }  //大小-大的总数
        [DataGridSetting("数", true, 26)]
        public int p2wdx_count { get { return (int)P2.数量数据.wdx_d_total; } }  //大小-大的总数
        [DataGridSetting("数", true, 26)]
        public int p2ds_count { get { return (int)P2.数量数据.ds_d_total; } }  //单双-单的总数
        [DataGridSetting("数", true, 26)]
        public int p2hds_count { get { return (int)P2.数量数据.hds_d_total; } }  //单双-单的总数


        private LotteryNumberView _P3;
        [DataGridSetting("P3", true, 35)]
        public LotteryNumberView P3 { get { return _P3; } }
        [DataGridSetting("大", true, 13)]
        public int p3dx { get { return (int)P3.大小数据.dx; } }
        [DataGridSetting("尾", true, 13)]
        public int p3wdx { get { return (int)P3.尾大小数据.wdx; } }
        [DataGridSetting("单", true, 13)]
        public int p3ds { get { return (int)P3.单双数据.ds; } }
        [DataGridSetting("和", true, 13)]
        public int p3wds { get { return (int)P3.和值单双数据.hds; } }
        [DataGridSetting("路", true, 21)]
        public int p3dxCount { get { return (int)P3.大小数据.count; } }
        [DataGridSetting("路", true, 21)]
        public int p3wdxCount { get { return (int)P3.尾大小数据.count; } }
        [DataGridSetting("路", true, 21)]
        public int p3dsCount { get { return (int)P3.单双数据.count; } }
        [DataGridSetting("路", true, 21)]
        public int p3hdsCount { get { return (int)P3.和值单双数据.count; } }
        [DataGridSetting("数", true, 26)]
        public int p3dx_count { get { return (int)P3.数量数据.dx_d_total; } }  //大小-大的总数
        [DataGridSetting("数", true, 26)]
        public int p3wdx_count { get { return (int)P3.数量数据.wdx_d_total; } }  //大小-大的总数
        [DataGridSetting("数", true, 26)]
        public int p3ds_count { get { return (int)P3.数量数据.ds_d_total; } }  //单双-单的总数
        [DataGridSetting("数", true, 26)]
        public int p3hds_count { get { return (int)P3.数量数据.hds_d_total; } }  //单双-单的总数

        private LotteryNumberView _P4;
        [DataGridSetting("P4", true, 35)]
        public LotteryNumberView P4 { get { return _P4; } }
        [DataGridSetting("大", true, 13)]
        public int p4dx { get { return (int)P4.大小数据.dx; } }
        [DataGridSetting("尾", true, 13)]
        public int p4wdx { get { return (int)P4.尾大小数据.wdx; } }
        [DataGridSetting("单", true, 13)]
        public int p4ds { get { return (int)P4.单双数据.ds; } }
        [DataGridSetting("和", true, 13)]
        public int p4wds { get { return (int)P4.和值单双数据.hds; } }
        [DataGridSetting("路", true, 21)]
        public int p4dxCount { get { return (int)P4.大小数据.count; } }
        [DataGridSetting("路", true, 21)]
        public int p4wdxCount { get { return (int)P4.尾大小数据.count; } }
        [DataGridSetting("路", true, 21)]
        public int p4dsCount { get { return (int)P4.单双数据.count; } }
        [DataGridSetting("路", true, 21)]
        public int p4hdsCount { get { return (int)P4.和值单双数据.count; } }
        [DataGridSetting("数", true, 26)]
        public int p4dx_count { get { return (int)P4.数量数据.dx_d_total; } }  //大小-大的总数
        [DataGridSetting("数", true, 26)]
        public int p4wdx_count { get { return (int)P4.数量数据.wdx_d_total; } }  //大小-大的总数
        [DataGridSetting("数", true, 26)]
        public int p4ds_count { get { return (int)P4.数量数据.ds_d_total; } }  //单双-单的总数
        [DataGridSetting("数", true, 26)]
        public int p4hds_count { get { return (int)P4.数量数据.hds_d_total; } }  //单双-单的总数

        private LotteryNumberView _P5;
        [DataGridSetting("P5", true, 35)]
        public LotteryNumberView P5 { get { return _P5; } }
        [DataGridSetting("大", true, 13)]
        public int p5dx { get { return (int)P5.大小数据.dx; } }
        [DataGridSetting("尾", true, 13)]
        public int p5wdx { get { return (int)P5.尾大小数据.wdx; } }
        [DataGridSetting("单", true, 13)]
        public int p5ds { get { return (int)P5.单双数据.ds; } }
        [DataGridSetting("和", true, 13)]
        public int p5wds { get { return (int)P5.和值单双数据.hds; } }
        [DataGridSetting("路", true, 21)]
        public int p5dxCount { get { return (int)P5.大小数据.count; } }
        [DataGridSetting("路", true, 21)]
        public int p5wdxCount { get { return (int)P5.尾大小数据.count; } }
        [DataGridSetting("路", true, 21)]
        public int p5dsCount { get { return (int)P5.单双数据.count; } }
        [DataGridSetting("路", true, 21)]
        public int p5hdsCount { get { return (int)P5.和值单双数据.count; } }
        [DataGridSetting("数", true, 26)]
        public int p5dx_count { get { return (int)P5.数量数据.dx_d_total; } }  //大小-大的总数
        [DataGridSetting("数", true, 26)]
        public int p5wdx_count { get { return (int)P5.数量数据.wdx_d_total; } }  //大小-大的总数
        [DataGridSetting("数", true, 26)]
        public int p5ds_count { get { return (int)P5.数量数据.ds_d_total; } }  //单双-单的总数
        [DataGridSetting("数", true, 26)]
        public int p5hds_count { get { return (int)P5.数量数据.hds_d_total; } }  //单双-单的总数


        
        private LotteryNumberView _P总 = null;
        [DataGridSetting("总合", true, 39)]
        public LotteryNumberView P总 { get { return _P总; } }

        [DataGridSetting("龙虎", true, 26)]
        public NumberDragonTiger P龙虎 { get => bgdata.P龙虎; set => bgdata.P龙虎 = value; }
        [DataGridSetting("时间", true, 120)]
        public string opentime { get => bgdata.opentime; set => bgdata.opentime = value; }
        [DataGridSetting("", false, 39)]
        public string lastError { get => bgdata.lastError; set => bgdata.lastError = value; }

        [DataGridSetting("L1", true, 26)]
        public int lzdx1 { get { return lzdx1_value(); } }
        [DataGridSetting("L2", true, 26)]
        public int lzdx2 { get { return lzdx2_value(); } }
        [DataGridSetting("L3", true, 26)]
        public int lzdx3 { get { return lzdx3_value();  } }
        [DataGridSetting("L4", true, 26)]
        public int lzdx4 { get { return lzdx4_value(); } }
        [DataGridSetting("L5", true, 26)]
        public int lzdx5 { get { return lzdx5_value(); } }
        [DataGridSetting("L6", true, 26)]
        public int lzdx6 { get { return lzdx6_value(); } }
        [DataGridSetting("L7", true, 26)]
        public int lzdx7 { get { return lzdx7_value(); } }
        [DataGridSetting("L8", true, 26)]
        public int lzdx8 { get { return lzdx8_value(); } }
        [DataGridSetting("L9", true, 26)]
        public int lzdx9 { get { return lzdx9_value();  } }
        [DataGridSetting("L10", true, 26)]
        public int lzdx10 { get { return lzdx10_value(); } }
        [DataGridSetting("lite", true, 56)]
        public string lite { get; set; }
        [DataGridSetting("full", true, 56)]
        public string full { get; set; }
        //连数据 1-6车
        [DataGridSetting("dxl", true, 56)]
        public int lian_total { get; set; }
        [DataGridSetting("dxt", true, 56)]
        public int tiao_total { get; set; }
        [DataGridSetting("dxltOv", true, 56)]
        public int lian_overflow { get; set; }

        private int lzdx1_value()
        {
            int value = P1.大小数据.lz_dx_total[0] + P2.大小数据.lz_dx_total[0] + P3.大小数据.lz_dx_total[0] + P4.大小数据.lz_dx_total[0] + P5.大小数据.lz_dx_total[0] + P总.大小数据.lz_dx_total[0];
            return value;
        }

        private int lzdx2_value()
        {
            int value = P1.大小数据.lz_dx_total[1] + P2.大小数据.lz_dx_total[1] + P3.大小数据.lz_dx_total[1] + P4.大小数据.lz_dx_total[1] + P5.大小数据.lz_dx_total[1] + P总.大小数据.lz_dx_total[1];
            return value;
        }

        private int lzdx3_value()
        {
            int value = P1.大小数据.lz_dx_total[2] + P2.大小数据.lz_dx_total[2] + P3.大小数据.lz_dx_total[2] + P4.大小数据.lz_dx_total[2] + P5.大小数据.lz_dx_total[2] + P总.大小数据.lz_dx_total[2];
            return value;
        }

        private int lzdx4_value()
        {
            int value =  P1.大小数据.lz_dx_total[3] + P2.大小数据.lz_dx_total[3] + P3.大小数据.lz_dx_total[3] + P4.大小数据.lz_dx_total[3] + P5.大小数据.lz_dx_total[3] + P总.大小数据.lz_dx_total[3];
            return value;
        }

        private int lzdx5_value()
        {
            int value = P1.大小数据.lz_dx_total[4] + P2.大小数据.lz_dx_total[4] + P3.大小数据.lz_dx_total[4] + P4.大小数据.lz_dx_total[4] + P5.大小数据.lz_dx_total[4] + P总.大小数据.lz_dx_total[4];
            return value;
        }
        private int lzdx6_value()
        {
            int value = P1.大小数据.lz_dx_total[5] + P2.大小数据.lz_dx_total[5] + P3.大小数据.lz_dx_total[5] + P4.大小数据.lz_dx_total[5] + P5.大小数据.lz_dx_total[5] + P总.大小数据.lz_dx_total[5];
            return value;
        }
        private int lzdx7_value()
        {
            int value = P1.大小数据.lz_dx_total[6] + P2.大小数据.lz_dx_total[6] + P3.大小数据.lz_dx_total[6] + P4.大小数据.lz_dx_total[6] + P5.大小数据.lz_dx_total[6] + P总.大小数据.lz_dx_total[6];
            return value;
        }

        private int lzdx8_value()
        {
            int value = P1.大小数据.lz_dx_total[7] + P2.大小数据.lz_dx_total[7] + P3.大小数据.lz_dx_total[7] + P4.大小数据.lz_dx_total[7] + P5.大小数据.lz_dx_total[7] + P总.大小数据.lz_dx_total[7];
            return value;
        }

        private int lzdx9_value()
        {
            int value = P1.大小数据.lz_dx_total[8] + P2.大小数据.lz_dx_total[8] + P3.大小数据.lz_dx_total[8] + P4.大小数据.lz_dx_total[8] + P5.大小数据.lz_dx_total[8] + P总.大小数据.lz_dx_total[8];
            return value;
        }

        private int lzdx10_value()
        {
            int value = P1.大小数据.lz_dx_total[9] + P2.大小数据.lz_dx_total[9] + P3.大小数据.lz_dx_total[9] + P4.大小数据.lz_dx_total[9] + P5.大小数据.lz_dx_total[9] + P总.大小数据.lz_dx_total[9];
            return value;
        }

        private double dx_lian_rate_lite()
        {
            int lian =  lzdx2_value() + lzdx3_value() + lzdx4_value() + lzdx5_value() + lzdx6_value() + lzdx7_value() + lzdx8_value() + lzdx9_value() + lzdx10_value();
            int tiao = lzdx1_value();
            if (tiao == 0)
                return 0.0f;
            double result = (double)lian / (double)tiao;
            return result;
        }

        private double dx_lian_rate_full()
        {
            int lian = lzdx2_value()*2 + lzdx3_value()*3 + lzdx4_value()*4 + lzdx5_value()*5 + lzdx6_value()*6 + lzdx7_value()*7 + lzdx8_value()*8 + lzdx9_value()*9 + lzdx10_value()*10;
            int tiao = lzdx1_value();
            
            if (tiao == 0)
                return 0.0f;
            double result = (double)lian / (double)tiao;
            return result;
        }
    }
}
