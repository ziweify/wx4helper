using DevExpress.LookAndFeel;
using DevExpress.Skins;
using DevExpress.UserSkins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using SQLitePCL;

namespace BinGoPlans
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // 🔥 初始化 SQLite 原生库（必须在最前面）
            try
            {
                Batteries.Init();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ SQLite 初始化失败:\n{ex.Message}\n\n{ex.StackTrace}", 
                    "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
