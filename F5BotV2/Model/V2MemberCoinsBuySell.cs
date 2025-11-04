using LxLib.LxSys;
using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace F5BotV2.Model
{
    public enum V2MemberPayAction { 未知=0, 上分=1,下分=2}
    public enum V2MemberPayStatus { 等待处理 = 0, 同意 = 1, 忽略 = 2 }

    public class V2MemberCoinsBuySell
        : IWxContacts
        , INotifyPropertyChanged
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

        public V2MemberCoinsBuySell()
        {
            this.iWxContact = new WxContacts();
        }

        public V2MemberCoinsBuySell(IWxContacts wxContacts, string GroupWxId, int Timestamp, int Money, V2MemberPayAction payAction, V2MemberPayStatus payStatus)
        {
            this.iWxContact = wxContacts;
            //把time时间按戳抓换成string

            this.Timestamp = Timestamp;
            this.Money = Money;
            this.PayAction = payAction;
            this.PayStatus = payStatus;
            this.GroupWxId = GroupWxId;

            try
            {
                this.Timestring = LxTimestampHelper.GetDateTime(Timestamp).ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch { }
        }

        [PrimaryKey, AutoIncrement]
        public int id { get; set; }

        private IWxContacts iWxContact;

        public string wxid { get => iWxContact.wxid; set => iWxContact.wxid = value; }
        public string account { get => iWxContact.account; set => iWxContact.account = value; }
        [DisplayName("会员名")]
        public string nickname { get => iWxContact.nickname; set => iWxContact.nickname = value; }
        public string avatar { get => iWxContact.avatar; set => iWxContact.avatar = value; }
        public string city { get => iWxContact.city; set => iWxContact.city = value; }
        public string country { get => iWxContact.country; set => iWxContact.country = value; }
        public string province { get => iWxContact.province; set => iWxContact.province = value; }
        public string remark { get => iWxContact.remark; set => iWxContact.remark = value; }
        public int sex { get => iWxContact.sex; set => iWxContact.sex = value; }


        //--------------
        //以下是本类别特定属性
        private int _Timestamp;
        public int Timestamp
        {
            get { return _Timestamp; }
            set
            {
                if (_Timestamp == value)
                    return;
                _Timestamp = value;
                NotifyPropertyChanged(() => Timestamp);
            }
        }


        private string _GroupWxId;
        public string GroupWxId
        {
            get { return _GroupWxId; }
            set
            {
                if (_GroupWxId == value)
                    return;
                _GroupWxId = value;
                NotifyPropertyChanged(() => GroupWxId);
            }
        }

        /// <summary>
        ///     支付动作,上还是下
        /// </summary>
        private V2MemberPayAction _PayAction;
        [DisplayName("动作")]
        public V2MemberPayAction PayAction
        {
            get { return _PayAction; }
            set
            {
                if (_PayAction == value)
                    return;
                _PayAction = value;
                NotifyPropertyChanged(() => PayAction);
            }
        }

        /// <summary>
        ///     金额
        /// </summary>
        private int _Money;
        [DisplayName("金额")]
        public int Money
        {
            get { return _Money; }
            set
            {
                if (_Money == value)
                    return;
                _Money = value;
                NotifyPropertyChanged(() => Money);
            }
        }

        /// <summary>
        ///     时间戳对应的日期时间
        /// </summary>
        private string _Timestring;
        [DisplayName("申请时间")]
        public string Timestring
        {
            get { return _Timestring; }
            set
            {
                if (_Timestring == value)
                    return;
                _Timestring = value;
                NotifyPropertyChanged(() => Timestring);
            }
        }




        /// <summary>
        ///     支付状态
        /// </summary>
        private V2MemberPayStatus _PayStatus;
        [DisplayName("状态")]
        public V2MemberPayStatus PayStatus
        {
            get { return _PayStatus; }
            set
            {
                if (_PayStatus == value)
                    return;
                _PayStatus = value;
                NotifyPropertyChanged(() => PayStatus);
            }
        }

        /// <summary>
        ///     订单备注
        /// </summary>
        private string _Note;
        [DisplayName("备份")]
        public string Note
        {
            get { return _Note; }
            set
            {
                if (_Note == value)
                    return;
                _Note = value;
                NotifyPropertyChanged(() => Note);
            }
        }
    }
}
