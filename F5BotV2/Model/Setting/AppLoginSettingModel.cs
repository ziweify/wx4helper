using LxLib.LxFile;
using LxLib.LxSys;
using F5BotV2.Main;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace F5BotV2.Model.Setting
{
    /// <summary>
    ///     登录设置
    /// </summary>
    public class AppLoginSettingModel
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


        private string _User = "";
        public string User
        {
            get { return _User; }
            set
            {
                if (_User == value)
                    return;
                _User = value;
                NotifyPropertyChanged(() => User);
            }
        }

        private string _Pwd = "";
        public string Pwd
        {
            get { return _Pwd; }
            set
            {
                if (_Pwd == value)
                    return;
                _Pwd = value;
                NotifyPropertyChanged(() => Pwd);
            }
        }

        /// <summary>
        ///     预留信息
        /// </summary>
        private string _PwdRecoverMessage = "";
        public string PwdRecoverMessage
        {
            get { return _PwdRecoverMessage; }
            set
            {
                if (_PwdRecoverMessage == value)
                    return;
                _PwdRecoverMessage = value;
                NotifyPropertyChanged(() => PwdRecoverMessage);
            }
        }

        /// <summary>
        ///     新密码
        /// </summary>
        private string _PwdRecoverNew = "";
        public string PwdRecoverNew
        {
            get { return _PwdRecoverNew; }
            set
            {
                if (_PwdRecoverNew == value)
                    return;
                _PwdRecoverNew = value;
                NotifyPropertyChanged(() => PwdRecoverNew);
            }
        }

        /// <summary>
        ///     确认密码
        /// </summary>
        private string _PwdRecoverConfirm = "";

        public string PwdRecoverConfirm
        {
            get { return _PwdRecoverConfirm; }
            set
            {
                if (_PwdRecoverConfirm == value)
                    return;
                _PwdRecoverConfirm = value;
                NotifyPropertyChanged(() => PwdRecoverConfirm);
            }
        }

        private string _CarNumber = "";
        public string CarNumber
        {
            get { return _CarNumber; }
            set
            {
                if (_CarNumber == value)
                    return;
                _CarNumber = value;
                NotifyPropertyChanged(() => CarNumber);
            }
        }

        private string _FilePath = "";
        public string FilePath
        {
            get { return _FilePath; }
            set
            {
                if (_FilePath == value)
                    return;
                _FilePath = value;
                NotifyPropertyChanged(() => FilePath);
            }
        }

        LxEncrypt encrypt = new LxEncrypt();

        public AppLoginSettingModel()
        {
            FilePath = System.Environment.CurrentDirectory + "\\配置.ini";
        }

        public void load()
        {
            User = LxIniFileHelper.getString("配置", "账号", "", FilePath);
            string encPwd = "";
            try
            {
                encPwd = LxIniFileHelper.getString("配置", "密码", "", FilePath);
                Pwd = encrypt.Decrypto(encPwd);
            }
            catch
            {
                Pwd = encPwd;
            }

        }

        public void loginSave()
        {
            //string encUser = encrypt.Encrypto(user);
            
            LxIniFileHelper.writeString("配置", "账号", User, FilePath);
            try
            {
                string encPwd = encrypt.Encrypto(Pwd);
                LxIniFileHelper.writeString("配置", "密码", encPwd, FilePath);
            }
            catch
            {
                LxIniFileHelper.writeString("配置", "密码", Pwd, FilePath);
            }
            
        }
    }
}
