using LxLib.LxSys;
using Newtonsoft.Json;
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
    /// <summary>
    ///     联系人类型
    /// </summary>
    public enum ContactType { unkow = 0, contact = 1, group = 2, public_number = 3 }


    /// <summary>
    ///     某信联系人数据
    /// </summary>
    public class WxContacts
        : INotifyPropertyChanged
        , IWxContacts
    {
        public WxContacts()
        {
            AtTimestamp = LxTimestampHelper.GetTimeStampToInt32();
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

        [PrimaryKey, AutoIncrement]
        public int id { get; set; }


        private string _WeHeadUrl;
        /// <summary>
        ///     头像链接地址
        /// </summary>
        public string WeHeadUrl
        {
            get { return _WeHeadUrl; }
            set
            {
                if (_WeHeadUrl == value)
                    return;
                _WeHeadUrl = value;
                NotifyPropertyChanged(() => WeHeadUrl);
            }
        }

        private string _WeHeadUrlBig;
        /// <summary>
        ///     大头像链接地址
        /// </summary>
        public string WeHeadUrlBig
        {
            get { return _WeHeadUrlBig; }
            set
            {
                if (_WeHeadUrlBig == value)
                    return;
                _WeHeadUrlBig = value;
                NotifyPropertyChanged(() => WeHeadUrlBig);
            }
        }


        /// <summary>
        ///     所属的微信ID
        /// </summary>
        private string _WeMainId;
        public string WeMainId
        {
            get { return _WeMainId; }
            set
            {
                if (_WeMainId == value)
                    return;
                _WeMainId = value;
                NotifyPropertyChanged(() => WeMainId);
            }
        }

        /// <summary>
        ///     所属的微信昵称
        /// </summary>
        private string _WeMainNikeName;
        public string WeMainNikeName
        {
            get { return _WeMainNikeName; }
            set
            {
                if (_WeMainNikeName == value)
                    return;
                _WeMainNikeName = value;
                NotifyPropertyChanged(() => WeMainNikeName);
            }
        }

        private string _nickname;
        public string nickname
        {
            get { return _nickname; }
            set
            {
                if (_nickname == value)
                    return;
                _nickname = value;
                NotifyPropertyChanged(() => nickname);
            }
        }

        private string _avatar;
        public string avatar
        {
            get { return _avatar; }
            set
            {
                if (_avatar == value)
                    return;
                _avatar = value;
                NotifyPropertyChanged(() => avatar);
            }
        }

        private string _wxid;
        public string wxid
        {
            get { return _wxid; }
            set
            {
                if (_wxid == value)
                    return;
                _wxid = value;
                NotifyPropertyChanged(() => wxid);
            }
        }

        private string _account;
        public string account
        {
            get { return _account; }
            set
            {
                if (_account == value)
                    return;
                _account = value;
                NotifyPropertyChanged(() => account);
            }
        }



        private string _city;
        public string city
        {
            get { return _city; }
            set
            {
                if (_city == value)
                    return;
                _city = value;
                NotifyPropertyChanged(() => city);
            }
        }

        private string _country;
        public string country
        {
            get { return _country; }
            set
            {
                if (_country == value)
                    return;
                _country = value;
                NotifyPropertyChanged(() => country);
            }
        }

        private string _province;
        public string province
        {
            get { return _province; }
            set
            {
                if (_province == value)
                    return;
                _province = value;
                NotifyPropertyChanged(() => province);
            }
        }

        private string _remark;
        public string remark
        {
            get { return _remark; }
            set
            {
                if (_remark == value)
                    return;
                _remark = value;
                NotifyPropertyChanged(() => remark);
            }
        }

        private int _sex;
        public int sex
        {
            get { return _sex; }
            set
            {
                if (_sex == value)
                    return;
                _sex = value;
                NotifyPropertyChanged(() => sex);
            }
        }

        private ContactType _ctype;
        //新增加.群标志
        public ContactType ctype
        {
            get { return _ctype; }
            set
            {
                if (_ctype == value)
                    return;
                _ctype = value;
                NotifyPropertyChanged(() => ctype);
            }
        }

        /// <summary>
        ///     消息接受时间戳, 或者入库时间
        /// </summary>
        private int _AtTimestamp;
        public int AtTimestamp
        {
            get { return _AtTimestamp; }
            set
            {
                if (_AtTimestamp == value)
                    return;
                _AtTimestamp = value;
                NotifyPropertyChanged(() => AtTimestamp);
            }
        }
    }
}
