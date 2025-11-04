using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace F5BotV2.Model
{
    public class LotteryNumberView
        : INotifyPropertyChanged
        , ILotteryNumber
    {
        private ILotteryNumber _lotteryNumber;

        /// <summary>
        ///     
        /// </summary>
        /// <param name="data">当前数据</param>
        /// <param name="lastData">上一条数据</param>
        public LotteryNumberView(int issueid, ILotteryNumber number, LotteryNumberView last)
        {
            this._lotteryNumber = number;
        
                _数量数据 = new num数量数据(issueid, number, last==null?null:last.数量数据);
                _大小数据 = new Lz大小(issueid, number, last==null?null:last.大小数据);
                _尾大小数据 = new Lz尾大小(issueid, number, last == null ? null : last.尾大小数据);
                _单双数据 = new Lz单双(issueid, number, last == null ? null : last.单双数据);
                _和值单双数据 = new Lz和值单双(issueid, number, last == null ? null : last.和值单双数据);
            //统计数据

            
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged<T>(Expression<Func<T>> property)
        {
            if (PropertyChanged == null)
                return;

            var memberExpression = property.Body as MemberExpression;
            if (memberExpression == null)
                return;

            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(memberExpression.Member.Name));
        }

        private num数量数据 _数量数据;  public num数量数据 数量数据 { get { return _数量数据; } }
        private Lz大小 _大小数据;   public Lz大小 大小数据 { get { return _大小数据; }  }
        private Lz尾大小 _尾大小数据; public Lz尾大小 尾大小数据 { get { return _尾大小数据; }  }
        private Lz单双 _单双数据;  public Lz单双 单双数据 { get { return _单双数据; } }
        private Lz和值单双 _和值单双数据;    public Lz和值单双 和值单双数据 { get { return _和值单双数据; }  }


        public int number { get => _lotteryNumber.number; set => _lotteryNumber.number = value; }
        public NumberDX dx { get => _lotteryNumber.dx; set => _lotteryNumber.dx = value; }
        public NumberDS ds { get => _lotteryNumber.ds; set => _lotteryNumber.ds = value; }
        public CarNumEnum pos { get => _lotteryNumber.pos; set => _lotteryNumber.pos = value; }
        public NumberWDX wdx { get => _lotteryNumber.wdx; set => _lotteryNumber.wdx = value; }
        public NumberHDS hds { get => _lotteryNumber.hds; set => _lotteryNumber.hds = value; }


        /// <summary>
        ///     显示在列表的数据
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.number.ToString();
        }
    }
}
