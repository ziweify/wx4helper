namespace BaiShengVx3Plus.Services
{
    /// <summary>
    /// 微信加载器服务接口
    /// </summary>
    public interface IWeChatLoaderService
    {
        /// <summary>
        /// 启动微信并注入
        /// </summary>
        bool LaunchWeChat(string ip, string port, string dllPath, out string errorMessage);

        /// <summary>
        /// 注入到现有进程
        /// </summary>
        bool InjectToProcess(uint processId, string dllPath, out string errorMessage);

        /// <summary>
        /// 获取所有微信进程ID
        /// </summary>
        List<uint> GetWeChatProcesses();
    }
}

