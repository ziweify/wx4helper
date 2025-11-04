using CCWin.SkinClass;
using F5BotV2.Model.Setting;
using LxLib.LxFile;
using LxLib.LxSys;
using Microsoft.SqlServer.Server;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F5BotV2.Main
{
    public class AppMainSettingDecorator
        : IAppMainSetting
    {
        AppMainSettingModel model;

        public AppMainSettingDecorator(AppMainSettingModel model)
        {
            this.model = model;
        }

        public int wxMinBet { get => ((IAppMainSetting)model).wxMinBet; set => ((IAppMainSetting)model).wxMinBet = value; }
        public int wxMaxBet { get => ((IAppMainSetting)model).wxMaxBet; set => ((IAppMainSetting)model).wxMaxBet = value; }
        public float wxOdds { get => ((IAppMainSetting)model).wxOdds; set => ((IAppMainSetting)model).wxOdds = value; }

        /// <summary>
        ///     提前多少秒关闭
        /// </summary>
        public int reduceCloseSeconds { get => ((IAppMainSetting)model).reduceCloseSeconds; set => ((IAppMainSetting)model).reduceCloseSeconds = value; }
        public string showBrowserText { get => ((IAppMainSetting)model).showBrowserText; set => ((IAppMainSetting)model).showBrowserText = value; }
        public string panUserName { get => ((IAppMainSetting)model).panUserName; set => ((IAppMainSetting)model).panUserName = value; }
        public string panUserPwd { get => ((IAppMainSetting)model).panUserPwd; set => ((IAppMainSetting)model).panUserPwd = value; }
        public string dbName { get => ((IAppMainSetting)model).dbName; set => ((IAppMainSetting)model).dbName = value; }
        public string panAddress { get => ((IAppMainSetting)model).panAddress; set => ((IAppMainSetting)model).panAddress = value; }
        public string platform { get => ((IAppMainSetting)model).platform; set => ((IAppMainSetting)model).platform = value; }
        public string wxClearPath { get => ((IAppMainSetting)model).wxClearPath; set => ((IAppMainSetting)model).wxClearPath = value; }
        public bool Zsjs { get => ((IAppMainSetting)model).Zsjs; set => ((IAppMainSetting)model).Zsjs = value; }
        public bool Zsxs { get => ((IAppMainSetting)model).Zsxs; set => ((IAppMainSetting)model).Zsxs = value; }


        //这里面对设置数据, 保存, 加载
        //没有错误, 返回0, 其他返回错误代码
        [System.Reflection.ObfuscationAttribute(Feature = "Virtualization", Exclude = false)]
        public int Load()
        {
            LxEncrypt encrypt = new LxEncrypt();
            string FilePath = System.Environment.CurrentDirectory + "\\配置.ini";
            model.panUserName = LxIniFileHelper.getString("配置", "PanUserName", "", FilePath);           
            string encPwd = "";
            try{ encPwd = LxIniFileHelper.getString("配置", "panUserPwd", "", FilePath);model.panUserPwd = encrypt.Decrypto(encPwd);}catch{model.panUserPwd = "";}
            try{ model.wxOdds = Convert.ToSingle(LxIniFileHelper.getString("配置", "wxOdds", "1.97", FilePath)); }catch { model.wxOdds = 1.97f; }
            try { model.wxMinBet = Convert.ToInt32(LxIniFileHelper.getString("配置", "wxMinBet", "10", FilePath)); } catch { model.wxMinBet = 10; }
            try { model.wxMaxBet = Convert.ToInt32(LxIniFileHelper.getString("配置", "wxMaxBet", "20000", FilePath)); } catch { model.wxMaxBet = 20000; }
            try { model.reduceCloseSeconds = Convert.ToInt32(LxIniFileHelper.getString("配置", "reduceCloseSeconds", "49", FilePath)); } catch { model.reduceCloseSeconds = 49; }
            try { model.panAddress = LxIniFileHelper.getString("配置", "panAddress", "https://8912794526-tky.c4ux0uslgd.com/", FilePath); } catch { model.panAddress = "https://8912794526-tky.c4ux0uslgd.com/"; }
            try { model.platform = LxIniFileHelper.getString("配置", "platform", "海峡", FilePath); } catch { model.platform = "海峡"; }
            try { model.wxClearPath = LxIniFileHelper.getString("配置", "清理", "", FilePath); } catch { model.wxClearPath = ""; }

            //结算方式
            try { model.Zsjs = Convert.ToBoolean(LxIniFileHelper.getString("配置", "整数结算", "False", FilePath)); } catch { model.Zsjs = false; }
            try { model.Zsxs = Convert.ToBoolean(LxIniFileHelper.getString("配置", "整数显示", "False", FilePath)); } catch { model.Zsxs = false; }

            model.showBrowserText = "显示";
            
            //model.panUserName = "";    //cs050
            //model.panUserPwd = "";    //Aaa051
            //model.panAddress = "https://8912794526-tky.c4ux0uslgd.com/";
           // model.panAddress = "https://5058961397-cj.mm666.co/";
            //网络请求, 加载代码...
            dbName = "v2.bat";
            //model.AppMode = AppMode.X模式;
            
            //wxOdds = 1.97f;
            //wxMinBet = 10;
            //wxMaxBet = 20000;
            //reduceCloseSeconds = 49;
            return 0;
        }

        //保存
        //成功返回0, 失败返回错误代码
        [System.Reflection.ObfuscationAttribute(Feature = "Virtualization", Exclude = false)]
        public int Save()
        {
            //网络保存
            LxEncrypt encrypt = new LxEncrypt();
            string FilePath = System.Environment.CurrentDirectory + "\\配置.ini";
            LxIniFileHelper.writeString("配置", "panUserName", panUserName, FilePath);
            try{
                string encPwd = encrypt.Encrypto(panUserPwd);
                LxIniFileHelper.writeString("配置", "panUserPwd", encPwd, FilePath);
            }
            catch{LxIniFileHelper.writeString("配置", "panUserPwd", "", FilePath);}
            LxIniFileHelper.writeString("配置", "wxOdds", string.Format("%f:2", wxOdds) , FilePath);
            LxIniFileHelper.writeString("配置", "wxMinBet", Convert.ToString(wxMinBet), FilePath);
            LxIniFileHelper.writeString("配置", "wxMaxBet", Convert.ToString(wxMaxBet), FilePath);
            LxIniFileHelper.writeString("配置", "reduceCloseSeconds", Convert.ToString(reduceCloseSeconds), FilePath);
            LxIniFileHelper.writeString("配置", "panAddress", Convert.ToString(panAddress), FilePath);
            LxIniFileHelper.writeString("配置", "platform", Convert.ToString(platform), FilePath);
            LxIniFileHelper.writeString("配置", "清理", wxClearPath, FilePath);

            LxIniFileHelper.writeString("配置", "整数结算", Zsjs.ToString(), FilePath);
            LxIniFileHelper.writeString("配置", "整数显示", Zsxs.ToString(), FilePath);

            //
            return 0;
        }
    }
}
