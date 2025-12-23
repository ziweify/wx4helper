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

            // 🔥 DevExpress 许可证初始化（必须在 Application.EnableVisualStyles() 之前）
            // 尝试使用不同的 API 注册许可证（DevExpress 23.2）
            try
            {
                // 方法1: 尝试使用 XtraEditors 命名空间
                var licenseType = Type.GetType("DevExpress.XtraEditors.LicenseManager, DevExpress.XtraEditors.v23.2");
                if (licenseType != null)
                {
                    var registerMethod = licenseType.GetMethod("RegisterLicense", new[] { typeof(string) });
                    if (registerMethod != null)
                    {
                        registerMethod.Invoke(null, new object[] { "DeltaFoX, 697903559/6 (#9223372036854775807)" });
                    }
                }
            }
            catch (Exception ex)
            {
                // 许可证注册失败，但不阻止程序运行（可能会显示注册对话框）
                System.Diagnostics.Debug.WriteLine($"DevExpress 许可证注册失败: {ex.Message}");
            }

            // 启用应用程序的可视样式
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 设置默认字体
            DevExpress.LookAndFeel.UserLookAndFeel.Default.SetSkinStyle("Office 2019 Colorful");

            // 🔥 加载配置
            var configManager = Services.Config.ConfigManager.Instance;
            configManager.Load();

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

