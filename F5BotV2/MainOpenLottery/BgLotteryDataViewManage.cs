//using F5BotV2.Model;
//using F5BotV2.Model.BindSqlite;
//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data;
//using System.Linq;
//using System.Runtime.CompilerServices;
//using System.Text;
//using System.Threading.Tasks;

//namespace F5BotV2.MainOpenLottery
//{
//    public class BgLotteryDataViewManage
//        : BindingList<BgLotteryView>
//    {
//        public BgLotteryDataViewManage()
//        {

//        }

//        public int p1DXL = 0;

//        public new void Add(BgLotteryView item)
//        {
//            throw new Exception("禁止调用该方法!请调用InsertData方法替代!");
//        }
         
//        public new void Insert(int index, BgLotteryView item)
//        {
//           throw new Exception("禁止调用该方法!请调用InsertData方法替代!");
//        }

//        /// <summary>
//        ///     没有错误返回0
//        ///     <0 >0都代表不同的错误
//        /// </summary>
//        /// <param name="index"></param>
//        /// <param name="item"></param>
//        /// <param name="updata">默认统计数据, 插入有结果的数据就统计数据, false则不统计</param>
//        /// <returns></returns>
//        public int InsertLotteryData(int index, BgLotteryView item, bool updata = true)
//        {
//            int result = 0;
//            //校验数据
//            if(this.Count == 0)
//            {
//                //先插入再统计
//                if(result == 0)
//                {
//                    base.Insert(index, item);
//                    result = Updata插入数据后统计辅助数据(index, this.FirstOrDefault(), true);
//                }
                
//                return result;
//            }
//            else
//            {
//                var first = this.FirstOrDefault();
//                if(item.IssueId == (first.IssueId+1))
//                {
//                    base.Insert(index, item);
//                    return Updata插入数据后统计辅助数据(index, item);
//                }
//                else if(item.IssueId == first.IssueId)
//                {
//                //    //说明该期生成了推测数据... 要填充数据进去...
//                //    //这个时候不能用插入数据了
//                //    first.Parse(item);
//                //    UpdataNow(index, first);
//                }
//                else
//                {
//                    //漏期数了, 要补充数据
//                }
//            }
           
//            return -1;
//        }


//        /// <summary>
//        ///     
//        /// </summary>
//        /// <param name="index"></param>
//        /// <param name="item">应该是当前插入的数据</param>
//        /// <param name="item">是否首条数据</param>
//        /// <returns></returns>
//        private int Updata插入数据后统计辅助数据(int index, BgLotteryDataView item, bool firstData = false)
//        {
//            //开始统计数据..
//            //统计推算结果前的数据..
//            //统计当前数据

//            // @本轮大小连续情况, 本轮单双连续情况
//            item.BLDS_Shuang = 0;
//            item.BLDS_Dan = 0;
//            item.BLDS_dsSum = 0;

//            item.BLDX_Da = 0;
//            item.BLDX_Xiao = 0;
//            item.BLDX_dxSum = 0;

//            //累计值
//            item.BLDX_dxAllSum = 0;
//            item.BLDS_dsAllSum = 0;

//            foreach (var number in item.items)
//            {
//                if(number.ds == NumberDS.双)
//                {
//                    item.BLDS_Shuang++;
//                }
//                else if(number.ds == NumberDS.单)
//                {
//                    item.BLDS_Dan++;
//                }

//                if(number.dx == NumberDX.大)
//                {
//                    item.BLDX_Da++;
//                }
//                else if(number.dx == NumberDX.小)
//                {
//                    item.BLDX_Xiao++;
//                }
//            }
//            item.BLDS_dsSum = item.BLDS_Dan - item.BLDS_Shuang;
//            item.BLDX_dxSum = item.BLDX_Da - item.BLDX_Xiao;

            


//            // @联动计算数据, 累计数据
//            item.BLDX_Lian = 0;
//            item.BLDX_Tiao = 0;
//            item.BLDX_ltCount = 0;
//            item.BLDS_Lian = 0;
//            item.BLDS_Tiao = 0;
//            item.BLDS_ltCount = 0;
//            if(!firstData)
//            {
//                var nextData = this[1];
//                for(int i = 0; i < item.items.Count(); i++)
//                {
//                    if (item.items[i].pos == nextData.items[i].pos)
//                    {
//                        //大小
//                        if (item.items[i].dx == nextData.items[i].dx)
//                        {
//                            item.BLDX_Lian++;
//                        }
//                        else
//                        {
//                            item.BLDX_Tiao++;
//                        }

//                        //单双
//                        if (item.items[i].ds == nextData.items[i].ds)
//                        {
//                            item.BLDS_Lian++;
//                        }
//                        else
//                        {
//                            item.BLDS_Tiao++;
//                        }
//                    } 
                    
//                    //P1详细数据
//                    if(item.items[i].pos == CarNumEnum.P1)
//                    {
//                        //大小差
//                        if (item.items[i].dx == NumberDX.大)
//                            item.P1DX_Sum = nextData.P1DX_Sum + 1;
//                        else
//                            item.P1DX_Sum = nextData.P1DX_Sum - 1;

//                        if (item.items[i].dx == nextData.items[i].dx)
//                            item.P1DX_Lian = nextData.P1DX_Lian+1;
//                        else
//                            item.P1DX_Lian = nextData.P1DX_Lian-1;

//                        //单双
//                        if (item.items[i].ds == NumberDS.单)
//                            item.P1DS_Sum = nextData.P1DS_Sum + 1;
//                        else
//                            item.P1DS_Sum = nextData.P1DS_Sum - 1;

//                        if (item.items[i].ds == nextData.items[i].ds)
//                            item.P1DS_Lian = nextData.P1DS_Lian+1;
//                        else
//                            item.P1DS_Lian = nextData.P1DS_Lian-1;
//                    }
//                    //else if(item.items[i].pos == NumberPos.P2)
//                    //{
//                    //    if (item.items[i].dx == NumberDX.大)
//                    //        item.P2DX_Sum = nextData.P2DX_Sum + 1;
//                    //    else
//                    //        item.P2DX_Sum = nextData.P2DX_Sum - 1;
//                    //}
//                }

//                //计算差值
//                //连-跳
//                item.BLDX_ltCount = item.BLDX_Lian - item.BLDX_Tiao;
//                item.BLDS_ltCount = item.BLDS_Lian - item.BLDS_Tiao;

//                //计算累计值
//                item.BLDX_dxAllSum = item.BLDX_dxSum + nextData.BLDX_dxAllSum;
//                item.BLDS_dsAllSum = item.BLDS_dsSum + nextData.BLDS_dsAllSum;
//            }

//            if(firstData)
//            {
//                item.BLDX_dxAllSum = item.BLDX_dxSum;
//                item.BLDS_dsAllSum = item.BLDS_dsSum;

//                for(int i = 0; i < item.items.Count(); i++)
//                {
//                    if(item.items[i].pos == CarNumEnum.P1)
//                    {
//                        if (item.items[i].dx == NumberDX.大)
//                            item.P1DX_Sum++;
//                        else
//                            item.P1DX_Sum--;

//                        //单双
//                        if (item.items[i].ds == NumberDS.单)
//                            item.P1DS_Sum++;
//                        else
//                            item.P1DS_Sum--;
//                    }
//                }
//            }

//            return 0;
//        }

//    }
//}
