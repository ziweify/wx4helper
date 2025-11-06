using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using BaiShengVx3Plus.Contracts;
using BaiShengVx3Plus.Contracts.Messages;
using BaiShengVx3Plus.Contracts.Games;
using BaiShengVx3Plus.Services.Auth;
using BaiShengVx3Plus.Services.Logging;
using BaiShengVx3Plus.Services.WeChat;
using BaiShengVx3Plus.Services.Contact;
using BaiShengVx3Plus.Services.UserInfo;
using BaiShengVx3Plus.Services.GroupBinding;
using BaiShengVx3Plus.Services.Messages;
using BaiShengVx3Plus.Services.Messages.Handlers;
using BaiShengVx3Plus.Services.Games.Binggo;
using BaiShengVx3Plus.Services;
using BaiShengVx3Plus.Services.Api;
using BaiShengVx3Plus.Models.Games.Binggo;
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
            try
            {
                // 🔥 初始化 SQLite 原生库（必须在最前面）
                try
                {
                    SQLitePCL.Batteries.Init();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"❌ SQLite 初始化失败:\n{ex.Message}\n\n{ex.StackTrace}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                Application.SetHighDpiMode(HighDpiMode.SystemAware);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                // 配置依赖注入
                var host = Host.CreateDefaultBuilder()
                    .ConfigureServices((context, services) =>
                    {
                        // 核心服务
                        services.AddSingleton<ILogService, LogService>();           // 日志服务（logs.db）
                        
                        // 业务服务
                        services.AddSingleton<IAuthService, AuthService>();
                        services.AddSingleton<IInsUserService, InsUserService>();
                        services.AddSingleton<IWeChatLoaderService, WeChatLoaderService>();
                        services.AddSingleton<IWeixinSocketClient, WeixinSocketClient>(); // Socket 通信客户端
                        services.AddSingleton<IContactDataService, ContactDataService>(); // 联系人数据服务
                        services.AddSingleton<IUserInfoService, UserInfoService>();       // 用户信息服务
                        services.AddSingleton<IWeChatService, WeChatService>();           // 微信应用服务（编排层）
                        services.AddSingleton<IGroupBindingService, GroupBindingService>(); // 群组绑定服务
                        services.AddSingleton<IMemberDataService, MemberDataService>();    // 会员数据访问服务
                            
                            // 🎮 游戏配置和服务
                            services.AddSingleton(new BinggoGameSettings());            // 炳狗游戏配置
                            services.AddSingleton<BinggoOrderValidator>();              // 炳狗订单验证器
                            services.AddSingleton<BinggoMessageHandler>();              // 炳狗消息处理器
                            
                            // 🌐 WebAPI 服务
                            services.AddHttpClient<IBsWebApiClient, BsWebApiClient>(); // HTTP 客户端
                            services.AddSingleton<IBsWebApiService, BsWebApiService>(); // WebAPI 服务
                            
                            // 🎲 炳狗开奖和订单服务
                            services.AddSingleton<IBinggoLotteryService, BinggoLotteryService>(); // 开奖服务
                            services.AddSingleton<IBinggoOrderService, BinggoOrderService>();     // 订单服务

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
                ILogService? logService = null;
                try
                {
                    logService = ServiceProvider.GetRequiredService<ILogService>();
                    logService.Info("Program", "应用程序启动");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"❌ 日志服务初始化失败:\n{ex.Message}\n\n{ex.StackTrace}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 初始化消息分发器并注册处理器
                try
                {
                    var dispatcher = ServiceProvider.GetRequiredService<MessageDispatcher>();
                    var handlers = ServiceProvider.GetServices<IMessageHandler>();
                    foreach (var handler in handlers)
                    {
                        dispatcher.RegisterHandler(handler);
                    }
                    logService.Info("Program", "消息处理器注册完成");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"❌ 消息处理器注册失败:\n{ex.Message}\n\n{ex.StackTrace}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 显示登录窗口
                LoginForm? loginForm = null;
                try
                {
                    loginForm = ServiceProvider.GetRequiredService<LoginForm>();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"❌ 创建登录窗口失败:\n{ex.Message}\n\n{ex.StackTrace}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (loginForm.ShowDialog() == DialogResult.OK)
                {
                    logService.Info("Program", "用户登录成功");
                    
                    // 登录成功，显示主窗口
                    try
                    {
                        var mainForm = ServiceProvider.GetRequiredService<VxMain>();
                        Application.Run(mainForm);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"❌ 创建或显示主窗口失败:\n{ex.Message}\n\n{ex.StackTrace}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        logService.Error("Program", "主窗口创建失败", ex);
                    }
                }
                else
                {
                    logService.Info("Program", "用户取消登录");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ 程序启动失败:\n{ex.Message}\n\n{ex.StackTrace}", "严重错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}