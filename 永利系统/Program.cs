using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using 永利系统.Services;
using 永利系统.Services.Auth;
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
            // 初始化 SQLite 原生库（必须在最前面）
            try
            {
                SQLitePCL.Batteries.Init();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ SQLite 初始化失败:\n{ex.Message}\n\n{ex.StackTrace}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 启用应用程序的可视样式
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 设置默认字体
            DevExpress.LookAndFeel.UserLookAndFeel.Default.SetSkinStyle("Office 2019 Colorful");

            // 🔥 防破解：必须先登录才能启动主窗口
            var loggingService = LoggingService.Instance;
            var authService = new AuthService(loggingService);
            var authGuard = new AuthGuard(loggingService, authService);
            
            // 显示登录窗口
            using (var loginForm = new LoginForm())
            {
                if (loginForm.ShowDialog() == DialogResult.OK)
                {
                    // 验证认证状态（防破解）
                    if (!authGuard.VerifyAuthentication())
                    {
                        MessageBox.Show("认证验证失败，程序将退出", "安全验证", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    
                    loggingService.Info("程序启动", "登录验证通过，启动主窗口");
                    
                    // 登录成功，显示主窗口
                    try
                    {
                        Application.Run(new MainTabs(authGuard));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"❌ 创建或显示主窗口失败:\n{ex.Message}\n\n{ex.StackTrace}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        loggingService.Error("程序启动", $"主窗口创建失败: {ex.Message}");
                    }
                }
                else
                {
                    loggingService.Info("程序启动", "用户取消登录，程序退出");
                }
            }
        }
    }
}

