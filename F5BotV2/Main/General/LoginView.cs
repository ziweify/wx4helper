using CCWin;
using F5BotV2.BetSite.Boter;
using F5BotV2.Ext;
using LxLib.LxFile;
using LxLib.LxNet;
using LxLib.LxSys;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace F5BotV2.Main
{
    public partial class LoginView : CCSkinMain
    {
        BoterApi boterApi;
        public LoginView(BoterApi boterApi)
        {
            InitializeComponent();
            this.boterApi = boterApi;
        }

        /// <summary>
        ///     登录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoginView_Load(object sender, EventArgs e)
        {
            //var titme_name = Assembly.GetExecutingAssembly().GetName();
            //var title_var =  Assembly.GetExecutingAssembly().GetName().Version.ToString();
            tbx_LoginUser.SkinTxt.DataBindings.Add(new Binding("Text", MainConfigure.appLoginSetting, "User", true, DataSourceUpdateMode.OnPropertyChanged));
            tbx_PwdRecoverUser.SkinTxt.DataBindings.Add(new Binding("Text", MainConfigure.appLoginSetting, "User", true, DataSourceUpdateMode.OnPropertyChanged));
            tbx_TbUser.SkinTxt.DataBindings.Add(new Binding("Text", MainConfigure.appLoginSetting, "User", true, DataSourceUpdateMode.OnPropertyChanged));

            //密码
            tbx_LoginPwd.SkinTxt.DataBindings.Add(new Binding("Text", MainConfigure.appLoginSetting, "Pwd", true, DataSourceUpdateMode.OnPropertyChanged));
            tbx_PwdRecoverPass.SkinTxt.DataBindings.Add(new Binding("Text", MainConfigure.appLoginSetting, "Pwd", true, DataSourceUpdateMode.OnPropertyChanged));
            //预留信息
            tbx_PwdRecoverMessage.SkinTxt.DataBindings.Add(new Binding("Text", MainConfigure.appLoginSetting, "PwdRecoverMessage", true, DataSourceUpdateMode.OnPropertyChanged));
            //新密码 PwdRecoverNew
            tbx_PwdRecoverNew.SkinTxt.DataBindings.Add(new Binding("Text", MainConfigure.appLoginSetting, "PwdRecoverNew", true, DataSourceUpdateMode.OnPropertyChanged));
            //确认密码 
            tbx_PwdRecoverConfirm.SkinTxt.DataBindings.Add(new Binding("Text", MainConfigure.appLoginSetting, "PwdRecoverConfirm", true, DataSourceUpdateMode.OnPropertyChanged));
            //礼品卡号  CarNumber
            tbx_CarNumber.SkinTxt.DataBindings.Add(new Binding("Text", MainConfigure.appLoginSetting, "CarNumber", true, DataSourceUpdateMode.OnPropertyChanged));

            MainConfigure.appLoginSetting.load();

            tab_LoginMain.SelectedIndex = 0;

            this.Text = $"小花{Assembly.GetExecutingAssembly().GetName().Version.ToString()} - 仅供测试娱乐!";
        }


        [System.Reflection.ObfuscationAttribute(Feature = "Virtualization", Exclude = false)]
        private void btnLogin_Click(object sender, EventArgs e)
        {
            var response = boterApi.login(MainConfigure.appLoginSetting.User, MainConfigure.appLoginSetting.Pwd);
            if (response.code == 0)
            {
                //登录成功记录数据
                string user = MainConfigure.appLoginSetting.User;
                //记录
                MainConfigure.appLoginSetting.loginSave();
                DialogResult = DialogResult.OK;

                //登录成功，时间>0才关闭
                
                this.Close();
            }
            else
            {
                MessageBox.Show(string.Format("{0}::{1}", response.code, response.msg));
            }
        }

        private void btn_PwRecover_Click(object sender, EventArgs e)
        {
            //重置密码
            try
            {
                //确认新密码::PwdRecoverConfirm
                if(string.IsNullOrEmpty(MainConfigure.appLoginSetting.PwdRecoverConfirm) 
                    || string.IsNullOrEmpty(MainConfigure.appLoginSetting.PwdRecoverNew))
                {
                    MessageBox.Show("密码不能为空!");
                    return;
                }

                if (MainConfigure.appLoginSetting.PwdRecoverConfirm != MainConfigure.appLoginSetting.PwdRecoverNew)
                {
                    MessageBox.Show("两次密码输入不正确! \r\n请重新输入!");
                    return;
                }
                    

                var response = boterApi.PassRecover(MainConfigure.appLoginSetting.User
                , MainConfigure.appLoginSetting.Pwd
                , MainConfigure.appLoginSetting.PwdRecoverNew
                , MainConfigure.appLoginSetting.PwdRecoverMessage);

                //测试
                if (response != null)
                {
                    MessageBox.Show($"{response.msg}", $"{response.code}");
                }
            }
            catch(Exception ex)
            {

            }
        }

        private void btnAddCredit_Click(object sender, EventArgs e)
        {
            string data = "平码一";
            MessageBox.Show(data.ToUnicodeCodes());
        }
    }
}
