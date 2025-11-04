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
                    // 注册服务
                    services.AddSingleton<IAuthService, AuthService>();
                    services.AddSingleton<IInsUserService, InsUserService>();

                    // 注册ViewModels
                    services.AddTransient<LoginViewModel>();
                    services.AddTransient<VxMainViewModel>();

                    // 注册Views
                    services.AddTransient<LoginForm>();
                    services.AddTransient<VxMain>();
                })
                .Build();

            ServiceProvider = host.Services;

            ApplicationConfiguration.Initialize();

            // 显示登录窗口
            var loginForm = ServiceProvider.GetRequiredService<LoginForm>();
            if (loginForm.ShowDialog() == DialogResult.OK)
            {
                // 登录成功，显示主窗口
                var mainForm = ServiceProvider.GetRequiredService<VxMain>();
                Application.Run(mainForm);
            }
        }
    }
}