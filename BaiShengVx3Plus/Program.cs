using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using BaiShengVx3Plus.Contracts;
using BaiShengVx3Plus.Contracts.Messages;
using BaiShengVx3Plus.Services.Auth;
using BaiShengVx3Plus.Services.Database;
using BaiShengVx3Plus.Services.Logging;
using BaiShengVx3Plus.Services.WeChat;
using BaiShengVx3Plus.Services.Contact;
using BaiShengVx3Plus.Services.UserInfo;
using BaiShengVx3Plus.Services.Member;
using BaiShengVx3Plus.Services.Order;
using BaiShengVx3Plus.Services.Messages;
using BaiShengVx3Plus.Services.Messages.Handlers;
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
                        services.AddSingleton<IWeixinSocketClient, WeixinSocketClient>(); // Socket 通信客户端
                        services.AddSingleton<IContactDataService, ContactDataService>(); // 联系人数据服务
                        services.AddSingleton<IUserInfoService, UserInfoService>();       // 用户信息服务
                        services.AddSingleton<IWeChatService, WeChatService>();           // 微信应用服务（编排层）

                        // 🔥 现代化属性追踪服务（自动保存单个字段）
                        services.AddSingleton<IPropertyChangeTracker, PropertyChangeTracker>(); // 属性变化追踪器
                        services.AddSingleton<IMemberService, MemberService>();                 // 会员服务
                        services.AddSingleton<IOrderService, OrderService>();                   // 订单服务

                        // 消息处理
                        services.AddSingleton<MessageDispatcher>();  // 消息分发器（单例）
                        services.AddTransient<IMessageHandler, ChatMessageHandler>();
                        services.AddTransient<IMessageHandler, LoginEventHandler>();
                        services.AddTransient<IMessageHandler, LogoutEventHandler>();
                        services.AddTransient<IMessageHandler, MemberJoinHandler>();
                        services.AddTransient<IMessageHandler, MemberLeaveHandler>();

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

            // 初始化消息分发器并注册处理器
            var dispatcher = ServiceProvider.GetRequiredService<MessageDispatcher>();
            var handlers = ServiceProvider.GetServices<IMessageHandler>();
            foreach (var handler in handlers)
            {
                dispatcher.RegisterHandler(handler);
            }
            logService.Info("Program", "消息处理器注册完成");

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