using F5BotV2.BetSite.Boter;
using F5BotV2.Main;
using F5BotV2.MainOpenLottery;
using F5BotV2.Model.Setting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace F5BotV2
{
    internal static class Program
    {
        public static void SetAssemblyVersion(string filePath, Version version)
        {
            string content = File.ReadAllText(filePath);
            string newContent = content.Replace("1.0.0.0", $"{version}");
            File.WriteAllText(filePath, newContent);
        }

        public static AppMode appMode = AppMode.普通模式;
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [System.Reflection.ObfuscationAttribute(Feature = "Virtualization", Exclude = false)]
        [STAThread]
        static void Main()
        {
            //[assembly: AssemblyVersion("6.3.7")]
            if (appMode == AppMode.普通模式)
            {
                //Version version = new Version("6.3.9.0");
                //Assembly assembly = Assembly.GetExecutingAssembly();
                //assembly.GetName().Version = version;
            }
            else if(appMode == AppMode.X模式)
            {
                //Version version = new Version("9.3.1.0");
                //SetAssemblyVersion("AssemblyInfo.cs", version);
            }
            bool ret;
            System.Threading.Mutex m = new System.Threading.Mutex(true, Application.ProductName, out ret);
            if (ret)
            {

                StartApp();
                m.ReleaseMutex();
            }
            else            {
                MessageBox.Show("F5已在运行! 请不要多次运行!");
                return;
            }

        }

        static void StartApp()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //初始化View
            if (appMode == Model.Setting.AppMode.X模式)
            {
                Application.Run(MainConfigure.getLogin(new XLoginView(BoterApi.GetInstance())));
                if (MainConfigure.getBotApi().loginApiResponse != null)
                {
                    try
                    {
                        if (MainConfigure.getBotApi().loginApiResponse.code == 0)
                        {
                            Application.Run(MainConfigure.getView(new XMainView()));
                        }
                    }
                    catch
                    {

                    }
                }
            }
            else if (appMode == Model.Setting.AppMode.普通模式)
            {
                Application.Run(MainConfigure.getLogin(new LoginView(BoterApi.GetInstance())));
                if (MainConfigure.getBotApi().loginApiResponse != null)
                {
                    try
                    {
                        if (MainConfigure.getBotApi().loginApiResponse.code == 0)
                        {
                            Application.Run(MainConfigure.getView(new MainView()));
                        }
                    }
                    catch
                    {

                    }
                }
            }
        }
    }
}
