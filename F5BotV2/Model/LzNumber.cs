using CCWin.Win32.Const;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F5BotV2.Model
{
    /// <summary>
    ///     路珠管理
    /// </summary>
    //public class LzNumber
    //{
    //    public LzNumber()
    //    {

    //    }

    //    //public num数量集合 numDx = new num数量集合();

    //    public Lz大小路集合 lzDx = new Lz大小路集合();  //大小路管理

    //    public Lz单双路集合 lzDs = new Lz单双路集合();

    //    //public Lz尾大小 wdx { get; set; }
    //    public void AddNumber(int issueid, LotteryNumber number)
    //    {
    //        //numDx.AddNumber(issueid, number);
    //        lzDx.AddNumber(issueid, number);
    //        lzDs.AddNumber(issueid, number);
    //    }
    //}

    public class num数量数据
    {
        public num数量数据(int issueid, ILotteryNumber curNumber, num数量数据 last)
        {
            issue_start = issueid;
            if(last == null)
            {
                //大小
                if (curNumber.dx == NumberDX.大)
                    dx_d_total = 1;
                else if (curNumber.dx == NumberDX.小)
                    dx_x_total = 1;

                //尾大小
                if (curNumber.wdx == NumberWDX.尾大)
                    wdx_d_total = 1;
                else if (curNumber.wdx == NumberWDX.尾小)
                    wdx_x_total = 1;

                //单双
                if (curNumber.ds == NumberDS.单)
                    ds_d_total = 1;
                else if (curNumber.ds == NumberDS.双)
                    ds_s_total = 1;
                //和单双
                if(curNumber.hds == NumberHDS.合单)
                    hds_d_total = 1;
                else if(curNumber.hds == NumberHDS.合双)
                    hds_s_total = 1;

                dx_d_overflow = dx_d_total - dx_x_total;
                wdx_d_overflow = wdx_d_total - wdx_x_total;
                ds_d_overflow = ds_d_total - ds_s_total;
                hds_d_overflow = hds_d_total - hds_s_total;
            }
            else
            {
                //大小
                if (curNumber.dx == NumberDX.大)
                {
                    dx_d_total = last.dx_d_total + 1;
                    dx_x_total = last.dx_x_total;
                }
                else if (curNumber.dx == NumberDX.小)
                {
                    dx_d_total = last.dx_d_total;
                    dx_x_total = last.dx_x_total + 1;
                }

                //尾大小
                if(curNumber.wdx == NumberWDX.尾大)
                {
                    wdx_d_total = last.wdx_d_total + 1;
                    wdx_x_total = last.wdx_x_total;
                }
                else if(curNumber.wdx == NumberWDX.尾小)
                {
                    wdx_d_total = last.wdx_d_total;
                    wdx_x_total = last.wdx_x_total+1;
                }

                //单双
                if (curNumber.ds == NumberDS.单)
                {
                    ds_d_total = last.ds_d_total+1;
                    ds_s_total = last.ds_s_total;
                }
                else if (curNumber.ds == NumberDS.双)
                {
                    ds_d_total = last.ds_d_total;
                    ds_s_total = last.ds_s_total+1;
                }

                //和单双
                if(curNumber.hds == NumberHDS.合单)
                {
                    hds_d_total = last.hds_d_total + 1;
                    hds_s_total = last.hds_s_total;
                }
                else if(curNumber.hds == NumberHDS.合双)
                {
                    hds_d_total = last.hds_d_total;
                    hds_s_total = last.hds_s_total+1;
                }

                dx_d_overflow = dx_d_total - dx_x_total;
                wdx_d_overflow = wdx_d_total - wdx_x_total;
                ds_d_overflow = ds_d_total - ds_s_total;
                hds_d_overflow = hds_d_total-hds_s_total;
            }
        }

        public int issue_start { get; set; }
        //大小数据
        public int dx_d_total { get; set; } //大小-大-数量
        public int dx_x_total { get; set; } //大小-小-数量
        public int dx_d_overflow { get; set; }  //大小溢出

        //尾大小数据
        public int wdx_d_total { get; set; }    //尾大小-大数量
        public int wdx_x_total { get; set; }    //尾大小-小数量
        public int wdx_d_overflow { get; set; } //尾大小-大溢出

        //单双数据
        public int ds_d_total { get; set; } //单双-单
        public int ds_s_total { get; set; } //单双-双
        public int ds_d_overflow { get; set; }

        //和单双数据
        public int hds_d_total { get; set; }
        public int hds_s_total { get; set; }
        public int hds_d_overflow { get; set; } //正数-双

    }


    //public class Lz大小路集合
    //    : List<Lz大小>
    //{
    //    public Lz大小路集合()
    //    {

    //    }

    //    /// <summary>
    //    ///     添加数据
    //    /// </summary>
    //    /// <returns></returns>
    //    public void AddNumber(int issueid, LotteryNumber number)
    //    {
    //        var last = this.LastOrDefault();
    //        if (last == null || last.dx != number.dx)
    //            this.Add(new Lz大小(issueid, number, null));
    //        else
    //        {
    //            last.count++;
    //        }
    //    }
    //}


    public class Lz大小
    {
        /// <summary>
        ///     开始期号
        /// </summary>
        public int issue_start { get; set; }    //开始期号
        public int count { get; set; }  //大出现数量, 这个数据显示错了..当前大小路珠的数据,如果当前大, 这里就显示大多少个,  如果当前小, 就显示小多少个
        public NumberDX dx { get; set; }//大或小
        private int[]_lz_d_TotalCount = new int[10];
        public int[] lz_d_TotalCount { get { return _lz_d_TotalCount; } }    //路子1多少个.不包含当前的

        private int[] _lz_x_TotalCount = new int[10];
        public int[] lz_x_TotalCount { get { return _lz_x_TotalCount; } }    //路子开小的, 有多少个

        private int[] _lz_dx_total = new int[10];
        public int[] lz_dx_total { get { return _lz_dx_total; } }           //路子总数

        //跳连数据
        public int lian_count { get; set; }
        public int tiao_count { get; set; }
        public int lian_overflow { get; set; }



        public Lz大小(int issueid, ILotteryNumber number, Lz大小 last)
        {
            if(last == null || last.dx != number.dx)
            {
                this.issue_start = issueid;
                this.dx = number.dx;
                this.count = 1;

                if(last != null)
                {

                    //大小连跳数据
                    lian_count = last.lian_count;
                    tiao_count = last.tiao_count + 1;
                    lian_overflow = lian_count - tiao_count;


                    //先复制数据
                    for (int i = 0; i < _lz_d_TotalCount.Count(); i++)
                    {
                        lz_d_TotalCount[i] = last.lz_d_TotalCount[i];
                        lz_x_TotalCount[i] = last.lz_x_TotalCount[i];
                        lz_dx_total[i] = last.lz_dx_total[i];
                    }

                    int index = last.count - 1;
                    if (index >= 10)
                        index = 9;

                    if (number.dx == NumberDX.大)
                        lz_d_TotalCount[index] = last.lz_d_TotalCount[index]+1;
                    else if(number.dx == NumberDX.小)
                        lz_x_TotalCount[index] = last.lz_x_TotalCount[index]+1;

                    //合计
                    lz_dx_total[index] = last.lz_dx_total[index] + 1;
                    if(lz_dx_total[index] != (lz_d_TotalCount[index] + lz_x_TotalCount[index]))
                    {
                        System.Diagnostics.Debug.WriteLine($"Lz大小::错误::{issueid}::{lz_dx_total[index]} != {lz_d_TotalCount[index]} + {lz_x_TotalCount[index]}");
                    }
                }
            }
            else
            {
                //先复制数据
                for (int i = 0; i < _lz_d_TotalCount.Count(); i++)
                {
                    lz_d_TotalCount[i] = last.lz_d_TotalCount[i];
                    lz_x_TotalCount[i] = last.lz_x_TotalCount[i];
                    lz_dx_total[i] = last.lz_dx_total[i];
                }

                //大小连跳数据
                lian_count = last.lian_count + 1;
                tiao_count = last.tiao_count;
                lian_overflow = lian_count - tiao_count;

                this.issue_start = last.issue_start;
                this.dx = last.dx;
                this.count = last.count+1;
            }
        }
    }


    public class Lz单双
    {
        /// <summary>
        ///     开始期号
        /// </summary>
        public int issue_start { get; set; }
        public int count { get; set; }
        public NumberDS ds { get; set; }

        private int[] _lz_d_TotalCount = new int[10];
        public int[] lz_d_TotalCount { get { return _lz_d_TotalCount; } }    //路子1多少个.不包含当前的

        private int[] _lz_s_TotalCount = new int[10];
        public int[] lz_s_TotalCount { get { return _lz_s_TotalCount; } }    //路子开小的, 有多少个

        private int[] _lz_ds_total = new int[10];
        public int[] lz_ds_total { get { return _lz_ds_total; } }           //有效路子总数, 不包含当前, 前面两个和.  d+s的. 不包含当前的

        //跳连数据
        public int lian_count { get; set; }
        public int tiao_count { get; set; }
        public int lian_overflow { get; set; }

        public Lz单双(int issueid, ILotteryNumber number, Lz单双 last)
        {
            if(last == null || last.ds != number.ds)
            {
                //基本数据
                this.issue_start = issueid;
                this.ds = number.ds;
                this.count = 1;

                if(last != null)
                {
                    //大小连跳数据
                    lian_count = last.lian_count;
                    tiao_count = last.tiao_count + 1;
                    lian_overflow = lian_count - tiao_count;

                    //先复制数据
                    for (int i = 0; i < _lz_d_TotalCount.Count(); i++)
                    {
                        lz_d_TotalCount[i] = last.lz_d_TotalCount[i];
                        lz_s_TotalCount[i] = last.lz_s_TotalCount[i];
                        lz_ds_total[i] = last.lz_ds_total[i];
                    }

                    int index = last.count - 1;
                    if (index >= 10)
                        index = 9;

                    if (number.ds == NumberDS.单)
                        lz_d_TotalCount[index] = last.lz_d_TotalCount[index] + 1;
                    else if (number.ds == NumberDS.双)
                        lz_s_TotalCount[index] = last.lz_s_TotalCount[index] + 1;

                    //合计
                    lz_ds_total[index] = last.lz_ds_total[index] + 1;
                }
            }
            else
            {
                //先复制数据
                for (int i = 0; i < _lz_d_TotalCount.Count(); i++)
                {
                    lz_d_TotalCount[i] = last.lz_d_TotalCount[i];
                    lz_s_TotalCount[i] = last.lz_s_TotalCount[i];
                    lz_ds_total[i] = last.lz_ds_total[i];
                }

                //大小连跳数据
                lian_count = last.lian_count + 1;
                tiao_count = last.tiao_count;
                lian_overflow = lian_count - tiao_count;

                this.issue_start = last.issue_start;
                this.ds = last.ds;
                this.count = last.count+1;
            }
        }
    }


    /// <summary>
    ///     路珠类.大小
    /// </summary>
    public class Lz尾大小
    {
        /// <summary>
        ///     开始期号
        /// </summary>
        public int issue_start { get; set; }
        public int count { get; set; }
        public NumberWDX wdx { get; }

        //辅助增强数据
        private int[] _lz_wd_TotalCount = new int[10];
        public int[] lz_wd_TotalCount { get { return _lz_wd_TotalCount; } }    //路子1多少个.不包含当前的

        private int[] _lz_wx_TotalCount = new int[10];
        public int[] lz_wx_TotalCount { get { return _lz_wx_TotalCount; } }    //路子开小的, 有多少个
        
        private int[] _lz_wdx_total = new int[10];
        public int[] lz_wdx_total { get { return _lz_wdx_total; } }           //有效路子总数, 不包含当前, 前面两个和.  d+s的. 不包含当前的

        //跳连数据
        public int lian_count { get; set; }
        public int tiao_count { get; set; }
        public int lian_overflow { get; set; }


        public Lz尾大小(int issueid, ILotteryNumber number, Lz尾大小 last)
        {
            if(last == null || last.wdx != number.wdx)
            {
                this.issue_start = issueid;
                this.count = 1;
                this.wdx = number.wdx;

                if(last != null)
                {
                    //大小连跳数据
                    lian_count = last.lian_count;
                    tiao_count = last.tiao_count + 1;
                    lian_overflow = lian_count - tiao_count;

                    //先复制数据
                    for (int i = 0; i < _lz_wd_TotalCount.Count(); i++)
                    {
                        lz_wd_TotalCount[i] = last.lz_wd_TotalCount[i];
                        lz_wx_TotalCount[i] = last.lz_wx_TotalCount[i];
                        lz_wdx_total[i] = last.lz_wdx_total[i];
                    }

                    int index = last.count - 1;
                    if (index >= 10)
                        index = 9;

                    if (number.wdx == NumberWDX.尾大)
                        lz_wd_TotalCount[index] = last.lz_wd_TotalCount[index] + 1;   //最后一次的长度
                    else if (number.wdx == NumberWDX.尾小)
                        lz_wx_TotalCount[index] = last.lz_wx_TotalCount[index] + 1;

                    //合计
                    lz_wdx_total[index] = last.lz_wdx_total[index] + 1;
                }
            }
            else
            {
                //先复制数据
                for (int i = 0; i < _lz_wd_TotalCount.Count(); i++)
                {
                    lz_wd_TotalCount[i] = last.lz_wd_TotalCount[i];
                    lz_wx_TotalCount[i] = last.lz_wx_TotalCount[i];
                    lz_wdx_total[i] = last.lz_wdx_total[i];
                }

                //大小连跳数据
                lian_count = last.lian_count + 1;
                tiao_count = last.tiao_count;
                lian_overflow = lian_count - tiao_count;

                //基本数据
                this.issue_start = last.issue_start;
                this.count = last.count+1;
                this.wdx = last.wdx;
            }
        }
    }


    public class Lz和值单双
    {
        /// <summary>
        ///     开始期号
        /// </summary>
        public int issue_start { get; set; }
        public int count { get; set; }
        public NumberHDS hds { get; }


        //辅助增强数据
        private int[] _lz_hd_TotalCount = new int[10];
        public int[] lz_hd_TotalCount { get { return _lz_hd_TotalCount; } }    //路子1多少个.不包含当前的

        private int[] _lz_hs_TotalCount = new int[10];
        public int[] lz_hs_TotalCount { get { return _lz_hs_TotalCount; } }    //路子开小的, 有多少个

        private int[] _lz_hds_total = new int[10];
        public int[] lz_hds_total { get { return _lz_hds_total; } }           //有效路子总数, 不包含当前, 前面两个和.  d+s的. 不包含当前的

        //跳连数据
        public int lian_count { get; set; }
        public int tiao_count { get; set; }
        public int lian_overflow { get; set; }

        public Lz和值单双(int issueid, ILotteryNumber number, Lz和值单双 last)
        {
            if(last == null || last.hds != number.hds)
            {
                //单双数据
                this.issue_start = issueid;
                this.count = 1; 
                this.hds= number.hds;

                if (last != null)
                {
                    //大小连跳数据
                    lian_count = last.lian_count;
                    tiao_count = last.tiao_count + 1;
                    lian_overflow = lian_count - tiao_count;

                    //先复制数据
                    for (int i = 0; i < _lz_hd_TotalCount.Count(); i++)
                    {
                        lz_hd_TotalCount[i] = last.lz_hd_TotalCount[i];
                        lz_hs_TotalCount[i] = last.lz_hs_TotalCount[i];
                        lz_hds_total[i] = last.lz_hds_total[i];
                    }

                    int index = last.count - 1;
                    if (index >= 10)
                        index = 9;

                    if (number.hds == NumberHDS.合单)
                        lz_hd_TotalCount[index] = last.lz_hd_TotalCount[index] + 1;   //最后一次的长度
                    else if (number.wdx == NumberWDX.尾小)
                        lz_hs_TotalCount[index] = last.lz_hs_TotalCount[index] + 1;

                    //合计
                    lz_hds_total[index] = last.lz_hds_total[index] + 1;
                }
            }
            else
            {
                //先复制数据
                for (int i = 0; i < _lz_hd_TotalCount.Count(); i++)
                {
                    lz_hd_TotalCount[i] = last.lz_hd_TotalCount[i];
                    lz_hs_TotalCount[i] = last.lz_hs_TotalCount[i];
                    lz_hds_total[i] = last.lz_hds_total[i];
                }

                //大小连跳数据
                lian_count = last.lian_count + 1;
                tiao_count = last.tiao_count;
                lian_overflow = lian_count - tiao_count;

                //基础数据
                this.issue_start = last.issue_start;
                this.count = last.count+1;
                this.hds = last.hds;
            }
        }
    }
}
