using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using BaiShengVx3Plus.Services;
using BaiShengVx3Plus.ViewModels;
using BaiShengVx3Plus.Views;

namespace BaiShengVx3Plus
{
    internal static class Program
    {
        public static IServiceProvider? ServiceProvider { get; private set; }

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 配置依赖注入
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // 核心服务
                    services.AddSingleton<ILogService, LogService>();           // 日志服务（logs.db）
                    services.AddSingleton<IDatabaseService, DatabaseService>(); // 数据库服务（business.db）
                    
                    // 业务服务
                    services.AddSingleton<IAuthService, AuthService>();
                    services.AddSingleton<IInsUserService, InsUserService>();
                    services.AddSingleton<IContactBindingService, ContactBindingService>();
                    services.AddSingleton<IWeChatLoaderService, WeChatLoaderService>();

                    // 注册ViewModels
                    services.AddTransient<LoginViewModel>();
                    services.AddTransient<VxMainViewModel>();

                    // 注册Views
                    services.AddTransient<LoginForm>();
                    services.AddTransient<LogViewerForm>();  // 日志查看器
                    services.AddTransient<VxMain>();         // 主窗口
                })
                .Build();

            ServiceProvider = host.Services;

            ApplicationConfiguration.Initialize();

            // 初始化日志服务
            var logService = ServiceProvider.GetRequiredService<ILogService>();
            logService.Info("Program", "应用程序启动");

            // 显示登录窗口
            var loginForm = ServiceProvider.GetRequiredService<LoginForm>();
            if (loginForm.ShowDialog() == DialogResult.OK)
            {
                logService.Info("Program", "用户登录成功");
                
                // 登录成功，显示主窗口
                var mainForm = ServiceProvider.GetRequiredService<VxMain>();
                Application.Run(mainForm);
            }
            else
            {
                logService.Info("Program", "用户取消登录");
            }
        }
    }
}