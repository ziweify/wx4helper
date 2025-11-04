using LxLib.LxSys;
using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace F5BotV2.Model
{
    public class WxGroup
        : INotifyPropertyChanged
    {
        public WxGroup()
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


        private string _wxid;
        /// <summary>
        ///     群ID
        /// </summary>
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

        /// <summary>
        ///     所属的微信ID.. 即是哪个微信,微信ID账号的微信ID
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

        private string _nickname = "";
        [DisplayName("群名称")]
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
        /// <summary>
        ///     群头像图片地址
        /// </summary>
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

        private string _manager_wxid;
        /// <summary>
        ///     群管理ID
        /// </summary>
        public string manager_wxid
        {
            get { return _manager_wxid; }
            set
            {
                if (_manager_wxid == value)
                    return;
                _manager_wxid = value;
                NotifyPropertyChanged(() => manager_wxid);
            }
        }


        private int _is_manager;
        /// <summary>
        ///     自己是否为群主
        /// </summary>
        [DisplayName("群主")]
        public int is_manager
        {
            get { return _is_manager; }
            set
            {
                if (_is_manager == value)
                    return;
                _is_manager = value;
                NotifyPropertyChanged(() => is_manager);
            }
        }

        private List<string> _member_list;
        /// <summary>
        ///     群成员列表
        /// </summary>
        [Ignore]
        public List<string> member_list
        {
            get { return _member_list; }
            set
            {
                if (_member_list == value)
                    return;
                _member_list = value;
                NotifyPropertyChanged(() => member_list);
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

        public WxGroup Updata(WxGroup data)
        {
            PropertyInfo[] propertys = data.GetType().GetProperties();
            PropertyInfo[] piThis = this.GetType().GetProperties();
            foreach (PropertyInfo pi in propertys)
            {
                object value1 = pi.GetValue(data, null); //用pi.GetValue获得值
                string name = pi.Name;                  //获得属性的名字,后面就可以根据名字判断来进行些自己想要的操作
                this.GetType().GetProperty(name).SetValue(this, value1);
            }
            //NotifyPropertyChanged(() => nickname);
            return this;
        }
    }
}
