using System;
using System.Windows.Forms;
using 永利系统.Views;

namespace 永利系统
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            // 启用应用程序的可视样式
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 设置默认字体
            DevExpress.LookAndFeel.UserLookAndFeel.Default.SetSkinStyle("Office 2019 Colorful");

            // 运行主窗体
            Application.Run(new Main());
        }
    }
}

