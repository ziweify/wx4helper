using F5BotV2.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F5BotV2.BetSite
{
    public abstract class OddsBingGouBase
        : List<OddsBingo>
    {
        public OddsBingGouBase() 
        {

            this.Add(new OddsBingo(Model.CarNumEnum.P1, Model.BetPlayEnum.单, "", "B1DS_D", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P1, Model.BetPlayEnum.双, "", "B1DS_S", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P1, Model.BetPlayEnum.大, "", "B1DX_D", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P1, Model.BetPlayEnum.小, "", "B1DX_X", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P1, Model.BetPlayEnum.尾大, "", "B1WDX_D", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P1, Model.BetPlayEnum.尾小, "", "B1WDX_X", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P1, Model.BetPlayEnum.合单, "", "B1HDS_D", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P1, Model.BetPlayEnum.合双, "", "B1HDS_S", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P1, Model.BetPlayEnum.福, "", "FLSX1_F", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P1, Model.BetPlayEnum.禄, "", "FLSX1_L", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P1, Model.BetPlayEnum.寿, "", "FLSX1_S", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P1, Model.BetPlayEnum.喜, "", "FLSX1_X", 0f));

            this.Add(new OddsBingo(Model.CarNumEnum.P2, Model.BetPlayEnum.单, "", "B2DS_D", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P2, Model.BetPlayEnum.双, "", "B2DS_S", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P2, Model.BetPlayEnum.大, "", "B2DX_D", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P2, Model.BetPlayEnum.小, "", "B2DX_X", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P2, Model.BetPlayEnum.尾大, "", "B2WDX_D", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P2, Model.BetPlayEnum.尾小, "", "B2WDX_X", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P2, Model.BetPlayEnum.合单, "", "B2HDS_D", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P2, Model.BetPlayEnum.合双, "", "B2HDS_S", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P2, Model.BetPlayEnum.福, "", "FLSX2_F", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P2, Model.BetPlayEnum.禄, "", "FLSX2_L", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P2, Model.BetPlayEnum.寿, "", "FLSX2_S", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P2, Model.BetPlayEnum.喜, "", "FLSX2_X", 0f));

            this.Add(new OddsBingo(Model.CarNumEnum.P3, Model.BetPlayEnum.单, "", "B3DS_D", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P3, Model.BetPlayEnum.双, "", "B3DS_S", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P3, Model.BetPlayEnum.大, "", "B3DX_D", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P3, Model.BetPlayEnum.小, "", "B3DX_X", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P3, Model.BetPlayEnum.尾大, "", "B3WDX_D", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P3, Model.BetPlayEnum.尾小, "", "B3WDX_X", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P3, Model.BetPlayEnum.合单, "", "B3HDS_D", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P3, Model.BetPlayEnum.合双, "", "B3HDS_S", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P3, Model.BetPlayEnum.福, "", "FLSX3_F", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P3, Model.BetPlayEnum.禄, "", "FLSX3_L", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P3, Model.BetPlayEnum.寿, "", "FLSX3_S", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P3, Model.BetPlayEnum.喜, "", "FLSX3_X", 0f));

            this.Add(new OddsBingo(Model.CarNumEnum.P4, Model.BetPlayEnum.单, "", "B4DS_D", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P4, Model.BetPlayEnum.双, "", "B4DS_S", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P4, Model.BetPlayEnum.大, "", "B4DX_D", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P4, Model.BetPlayEnum.小, "", "B4DX_X", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P4, Model.BetPlayEnum.尾大, "", "B4WDX_D", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P4, Model.BetPlayEnum.尾小, "", "B4WDX_X", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P4, Model.BetPlayEnum.合单, "", "B4HDS_D", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P4, Model.BetPlayEnum.合双, "", "B4HDS_S", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P4, Model.BetPlayEnum.福, "", "FLSX4_F", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P4, Model.BetPlayEnum.禄, "", "FLSX4_L", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P4, Model.BetPlayEnum.寿, "", "FLSX4_S", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P4, Model.BetPlayEnum.喜, "", "FLSX4_X", 0f));

            this.Add(new OddsBingo(Model.CarNumEnum.P5, Model.BetPlayEnum.单, "", "B5DS_D", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P5, Model.BetPlayEnum.双, "", "B5DS_S", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P5, Model.BetPlayEnum.大, "", "B5DX_D", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P5, Model.BetPlayEnum.小, "", "B5DX_X", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P5, Model.BetPlayEnum.尾大, "", "B5WDX_D", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P5, Model.BetPlayEnum.尾小, "", "B5WDX_X", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P5, Model.BetPlayEnum.合单, "", "B5HDS_D", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P5, Model.BetPlayEnum.合双, "", "B5HDS_S", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P5, Model.BetPlayEnum.福, "", "FLSX5_F", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P5, Model.BetPlayEnum.禄, "", "FLSX5_L", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P5, Model.BetPlayEnum.寿, "", "FLSX5_S", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P5, Model.BetPlayEnum.喜, "", "FLSX5_X", 0f));

            //总和:ZHDS_D
            this.Add(new OddsBingo(Model.CarNumEnum.P总, Model.BetPlayEnum.单, "", "ZHDS_D", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P总, Model.BetPlayEnum.双, "", "ZHDS_S", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P总, Model.BetPlayEnum.大, "", "ZHDX_D", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P总, Model.BetPlayEnum.小, "", "ZHDX_X", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P总, Model.BetPlayEnum.尾大, "", "HWDX_D", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P总, Model.BetPlayEnum.尾小, "", "HWDX_X", 0f));
            //LH_L
            this.Add(new OddsBingo(Model.CarNumEnum.P总, Model.BetPlayEnum.龙, "", "LH_L", 0f));
            this.Add(new OddsBingo(Model.CarNumEnum.P总, Model.BetPlayEnum.虎, "", "LH_H", 0f));
        }


        private bool _isUpdata = false;  //是否已经更新
        public bool isUpdata
        {
            get { return _isUpdata; }
            set
            {
                _isUpdata = value;
            }
        }


        /// <summary>
        ///     
        /// </summary>
        /// <param name="car"></param>
        /// <param name="play"></param>
        /// <param name="safemode">安全模式, 如果没有找到元素 true表示不会抛出异常 </param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public OddsBingo GetOdds(CarNumEnum car, BetPlayEnum play, bool safemode = false)
        {
            var odds = this.FirstOrDefault(p => p.car == car && p.play == play);
            if (odds.odds == 0)
            {
                if (!safemode)
                {
                    throw new Exception();
                }
            }

            return odds;
        }


        public OddsBingo GetOdds(string odsName, bool safemode = false)
        {
            var odds = this.FirstOrDefault(p => p.oddsName == odsName);
            if (!safemode)
            {
                if (odds.odds == 0)
                    throw new Exception();
            }
            return odds;
        }

        public abstract bool GetUpdata(string url, string cookie, string p_type);
    }
}
