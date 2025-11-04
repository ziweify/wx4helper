using F5BotV2.BetSite.Boter;
using F5BotV2.Model;
using F5BotV2.Model.BindSqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

namespace F5BotV2.MainOpenLottery
{
    /// <summary>
    ///     小花计划
    /// </summary>
    public class PlanFlower
    {
        private PlanFlowerView _view;
        //private BgLotteryDataBindlite _lotteryDatalite = new BgLotteryDataBindlite();
        //public BgLotteryDataBindlite lotteryDatalite { get { return _lotteryDatalite; } }   //在Boter里面初始化的

        /// <summary>
        ///     表中数据是最新数据插入到最前面
        /// </summary>
        private BgLotteryViewBindlist _lotteryDatalite = new BgLotteryViewBindlist();
        public BgLotteryViewBindlist lotteryDatalite { get { return _lotteryDatalite; } }       //列表中显示的数据

        //暂时不用这个路珠..
        //搞完数据统计再恢复..
        //public LzNumber lzp1 = new LzNumber();   //p1的所有路珠
        //public LzNumber lzp2 = new LzNumber();   //p2的所有路珠
        //public LzNumber lzp3 = new LzNumber();   //p3的所有路珠
        //public LzNumber lzp4 = new LzNumber();   //p4的所有路珠
        //public LzNumber lzp5 = new LzNumber();   //p5的所有路珠

        public PlanFlower(PlanFlowerView view)
        {
            this._view = view;
        }

        /// <summary>
        ///     
        /// </summary>
        /// <param name="date"></param>
        /// <returns>插入了多少条数据</returns>
        public int getdata(DateTime date)
        {
            int response = 0;
            OnClear();
            var dayresult = BoterApi.GetInstance().getbgday(date.ToShortDateString(), 0, false);
            if (dayresult.data != null)
            {
                //顺序插入，最新的插入到最前面
                for (int i = dayresult.data.Count-1; i>=0; i-- )
                {
                    if (OnDataInserting(dayresult.data[i]) == 1)
                    {
                        response++;
                    }
                }
            }
            return response;
        }


        /// <summary>
        ///     清理所有数据
        /// </summary>
        public void OnClear()
        {
            _view.Invoke(new Action(() => {
                _lotteryDatalite.Clear();
            }));
        }


        /// <summary>
        ///     添加数据, 会校验数据, 
        /// </summary>
        /// <param name="data"></param>
        /// <returns>成功返回1, 0表示数据已存在,  >1表示中间差数据, 需要全天数据补充</returns>
        public int OnDataInserting(BgLotteryData data)
        {
            int number = 0;
            try
            {
                var first = _lotteryDatalite.FirstOrDefault();
                if (first == null)
                {
                    _lotteryDatalite.Add(new BgLotteryView(data, null));
                    return 1;
                }

                number = data.IssueId - first.IssueId;
                if (number == 0)
                {
                    return number;
                }
                if (number == 1)
                {
                    _lotteryDatalite.Insert(0, new BgLotteryView(data, first));
                    OnDataInsertSuccess(data);
                    return number;
                }

            }
            catch (Exception ex)
            {
                number = -1;
            }
            return number;
        }


        /// <summary>
        ///     数据插入成功, 并且校验过, 可以在这里进行公式匹配
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public int OnDataInsertSuccess(BgLotteryData data)
        {
            //lzp1.AddNumber(data.IssueId, data.P1);
            //lzp2.AddNumber(data.IssueId, data.P2);
            //lzp3.AddNumber(data.IssueId, data.P3);
            //lzp4.AddNumber(data.IssueId, data.P4);
            //lzp5.AddNumber(data.IssueId, data.P5);
            return 1;
        }
    }
}
