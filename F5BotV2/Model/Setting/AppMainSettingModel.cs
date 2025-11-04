using Org.BouncyCastle.Asn1.Sec;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace F5BotV2.Model.Setting
{
    //internal class IAppSettingWx
    //{
    //}

    public interface IAppMainSetting
    {
        //全局
        string dbName { get; set; }
        string panAddress { get; set; }

        string wxClearPath { get; set; }


         int wxMinBet { get; set; }
        int wxMaxBet { get; set; }
        float wxOdds { get; set; }

        int reduceCloseSeconds { get; set; }
        string showBrowserText { get; set; }

        //盘相关
        string panUserName { get; set; }
        string panUserPwd { get; set; }
        string platform { get; set; }

        bool Zsjs { get; set; }
        bool Zsxs { get; set; }
    }

    /// <summary>
    ///     X模式对应L的需求
    /// </summary>
    public enum AppMode { 普通模式 = 0, X模式 = 1 }

    /// <summary>
    ///     微信模块设置
    /// </summary>
    public class AppMainSettingModel
        : INotifyPropertyChanged
        , IAppMainSetting
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

        public AppMainSettingModel() {
        }


        //private AppMode _appMode;
        //public AppMode AppMode
        //{
        //    get { return _appMode; }
        //    set
        //    {
        //        if (_appMode != value)
        //            _appMode = value;
        //        NotifyPropertyChanged(() => AppMode);
        //    }
        //}

        /// <summary>
        ///     减少开奖关闭秒数
        ///     即提前多少时间封盘
        /// </summary>
        private int _reduceCloseSeconds;
        public int reduceCloseSeconds
        {
            get { return _reduceCloseSeconds; }
            set
            {
                if(_reduceCloseSeconds != value)
                    _reduceCloseSeconds = value;
                NotifyPropertyChanged(() => reduceCloseSeconds);
            }
        }

        /// <summary>
        ///     按钮文字,(显示浏览器按钮
        /// </summary>

        private string _showBrowserText = "显示";
        public string showBrowserText
        {
            get { return _showBrowserText; }
            set
            {
                if (_showBrowserText == value)
                    return;
                _showBrowserText = value;
                NotifyPropertyChanged(() => showBrowserText);
            }
        }

        //这数据
        private string _dbName;
        public string dbName {
            get { return _dbName; }
            set {
                if (_dbName == value)
                    return;
                _dbName = value;
                NotifyPropertyChanged(() => dbName);
            }
        }


        private bool _Zsjs;
        public bool Zsjs
        {
            get { return _Zsjs; }
            set
            {
                if (_Zsjs == value)
                    return;
                _Zsjs = value;
                NotifyPropertyChanged(() => Zsjs);
            }
        }

        private bool _Zsxs;
        public bool Zsxs
        {
            get { return _Zsxs; }
            set
            {
                if (_Zsxs == value)
                    return;
                _Zsxs = value;
                NotifyPropertyChanged(() => Zsxs);
            }
        }


        //最小投注
        private int _wxMinBet;
        public int wxMinBet {
            get { return _wxMinBet; }
            set
            {
                if (_wxMinBet == value)
                    return;
                _wxMinBet = value;
                NotifyPropertyChanged(() => wxMinBet);
            }
        }

        private int _wxMaxBet;
        public int wxMaxBet { get { return _wxMaxBet; } set {
                if (_wxMaxBet == value)
                    return;
                _wxMaxBet = value;
                NotifyPropertyChanged(() => wxMaxBet);
            } }

        private float _wxOdds;
        public float wxOdds { get { return _wxOdds; } set {
                if (_wxOdds == value)
                    return;
                _wxOdds = value;
                NotifyPropertyChanged(() => wxOdds);
            } }


        //这数据
        private string _panUserName;
        public string panUserName { get { return _panUserName; } set {
                if (_panUserName == value)
                    return;
                _panUserName = value;
                NotifyPropertyChanged(() => panUserName);
            } }


        private string _panUserPwd;
        public string panUserPwd { get { return _panUserPwd; } set {
                if (_panUserPwd == value)
                    return;
                _panUserPwd = value;
                NotifyPropertyChanged(() => panUserPwd);
            } }

        private string _panAddress;
        public string panAddress {
            get { return _panAddress; }
            set
            {
                if (_panAddress == value)
                    return;
                _panAddress = value;
                NotifyPropertyChanged(() => panAddress);
            }
        }

        //platform
        //盘口地址
        private string _platform;
        public string platform
        {
            get { return _platform; }
            set
            {
                if (_platform == value)
                    return;
                _platform = value;
                NotifyPropertyChanged(() => platform);
            }
        }

        private string _wxClearPath;
        public string wxClearPath {
            get { return _wxClearPath; }
            set
            {
                if (_wxClearPath == value)
                    return;
                _wxClearPath = value;
                NotifyPropertyChanged(() => wxClearPath);
            }
        }
    }
}
