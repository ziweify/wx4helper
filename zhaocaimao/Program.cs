using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using zhaocaimao.Contracts;
using zhaocaimao.Contracts.Messages;
using zhaocaimao.Contracts.Games;
using zhaocaimao.Services.Auth;
using zhaocaimao.Services.Logging;
using zhaocaimao.Services.WeChat;
using zhaocaimao.Services.Contact;
using zhaocaimao.Services.UserInfo;
using zhaocaimao.Services.GroupBinding;
using zhaocaimao.Services.Messages;
using zhaocaimao.Services.Messages.Handlers;
using zhaocaimao.Services.Games.Binggo;
using zhaocaimao.Services;
using zhaocaimao.Services.Api;
using zhaocaimao.Models.Games.Binggo;
using zhaocaimao.ViewModels;
using zhaocaimao.Views;

namespace zhaocaimao
{
    internal static class Program
    {
        public static IServiceProvider? ServiceProvider { get; private set; }

        [STAThread]
        static void Main()
        {
            try
            {
                // 初始化 SQLite
                try
                {
                    SQLitePCL.Batteries.Init();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"SQLite初始化失败:\n{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                        services.AddSingleton<Services.Database.DatabaseInitializer>();
                        services.AddSingleton<ILogService, LogService>();
                        services.AddSingleton<IConfigurationService, Services.Configuration.ConfigurationService>();
                        
                        // 业务服务
                        services.AddSingleton<IInsUserService, InsUserService>();
                        services.AddSingleton<IWeChatLoaderService, WeChatLoaderService>();
                        services.AddSingleton<IWeixinSocketClient, WeixinSocketClient>();
                        services.AddSingleton<IContactDataService, ContactDataService>();
                        services.AddSingleton<IUserInfoService, UserInfoService>();
                        services.AddSingleton<IWeChatService, WeChatService>();
                        services.AddSingleton<IGroupBindingService, GroupBindingService>();
                        services.AddSingleton<IMemberDataService, MemberDataService>();
                            
                        // 游戏服务
                        services.AddSingleton(new BinggoGameSettings());
                        services.AddSingleton<BinggoOrderValidator>();
                        services.AddSingleton<AdminCommandHandler>();
                        services.AddSingleton<BinggoMessageHandler>();
                        
                        // 炳狗服务
                        services.AddSingleton<IBinggoLotteryService, BinggoLotteryService>();
                        services.AddSingleton<IBinggoOrderService, BinggoOrderService>();
                        services.AddSingleton<BinggoStatisticsService>();

                        // 自动投注服务
                        services.AddSingleton<Services.AutoBet.BetRecordService>();
                        services.AddSingleton<Services.AutoBet.OrderMerger>();
                        services.AddSingleton<Services.AutoBet.BetQueueManager>();
                        services.AddSingleton<Services.AutoBet.AutoBetService>();
                        services.AddSingleton<Services.AutoBet.AutoBetCoordinator>();

                        // 消息处理
                        services.AddSingleton<MessageDispatcher>();
                        services.AddTransient<IMessageHandler, ChatMessageHandler>();
                        services.AddTransient<IMessageHandler, LoginEventHandler>();
                        services.AddTransient<IMessageHandler, LogoutEventHandler>();
                        services.AddTransient<IMessageHandler, MemberJoinHandler>();
                        services.AddTransient<IMessageHandler, MemberLeaveHandler>();

                        // ViewModels
                        services.AddTransient<ConfigViewModel>();
                        services.AddTransient<VxMainViewModel>();
                        services.AddSingleton<ViewModels.SettingViewModel>();

                        // Views
                        services.AddTransient<LoginForm>();
                        services.AddTransient<VxMain>();
                    })
                    .Build();

                ServiceProvider = host.Services;

                ApplicationConfiguration.Initialize();

                // 初始化日志
                ILogService? logService = null;
                try
                {
                    logService = ServiceProvider.GetRequiredService<ILogService>();
                    logService.Info("Program", "招财猫应用程序启动");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"日志服务初始化失败:\n{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 初始化消息分发器
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
                    MessageBox.Show($"消息处理器注册失败:\n{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    MessageBox.Show($"创建登录窗口失败:\n{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (loginForm.ShowDialog() == DialogResult.OK)
                {
                    logService.Info("Program", "用户登录成功");
                    
                    try
                    {
                        var mainForm = ServiceProvider.GetRequiredService<VxMain>();
                        Application.Run(mainForm);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"创建主窗口失败:\n{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                MessageBox.Show($"程序启动失败:\n{ex.Message}", "严重错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
