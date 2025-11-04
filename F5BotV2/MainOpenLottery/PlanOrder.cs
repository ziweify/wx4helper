using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace F5BotV2.MainOpenLottery
{
    /// <summary>
    ///     推导订单类, 推导出来的结果，就是一个个订单
    /// </summary>
    public class PlanOrder
        : INotifyPropertyChanged
    {
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

        public PlanOrder()
        {
            odds = 1.97f;
        }

        /// <summary>
        ///     订单ID
        /// </summary>
        private int _issueid;
        public int issueid
        {
            get { return _issueid; }
            set
            {
                if (_issueid == value)
                    return;
                _issueid = value;
                NotifyPropertyChanged(() => issueid);
            }
        }

        private int _amount;
        //投注金额
        public int amount
        {
            get { return _amount; }
            set
            {
                if (_amount == value)
                    return;
                _amount = value;
                NotifyPropertyChanged(() => amount);
            }
        } 

        private float _price;
        //盈利, 毛利
        public float profit
        {
            get { return _price; }
            set
            {
                if (_price == value)
                    return;
                _price = value;
                NotifyPropertyChanged(() => profit);
            }
        }

        private float _net_rofit = 0f;
        //纯利
        public float net_rofit
        {
            get { return _net_rofit; }
            set
            {
                if (_net_rofit == value)
                    return;
                _net_rofit = value;
                NotifyPropertyChanged(() => net_rofit);
            }
        }

        private float _odds;
        //赔率, 一般是1.97
        public float odds { get; set; }     //
        public string BetContentStandar { get; set; }   //投注的标注内容, 例 1大100;2大100;3大100


    }
}
