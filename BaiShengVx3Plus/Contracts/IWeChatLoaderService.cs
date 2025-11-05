using System.Threading;
using System.Threading.Tasks;

namespace BaiShengVx3Plus.Contracts
{
    /// <summary>
    /// 微信加载器服务接口（Infrastructure Service）
    /// 职责：进程管理（启动、注入、枚举）
    /// </summary>
    public interface IWeChatLoaderService
    {
        /// <summary>
        /// 智能启动或注入（高层方法）
        /// - 如果微信已运行，尝试注入到现有进程
        /// - 如果微信未运行，启动微信并注入
        /// </summary>
        /// <param name="dllPath">要注入的DLL路径</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>成功返回true，失败返回false</returns>
        Task<(bool Success, string ErrorMessage)> LaunchOrInjectAsync(string dllPath, CancellationToken cancellationToken = default);

        /// <summary>
        /// 启动微信并注入（底层方法）
        /// </summary>
        bool LaunchWeChat(string ip, string port, string dllPath, out string errorMessage);

        /// <summary>
        /// 注入到现有进程（底层方法）
        /// </summary>
        bool InjectToProcess(uint processId, string dllPath, out string errorMessage);

        /// <summary>
        /// 获取所有微信进程ID
        /// </summary>
        List<uint> GetWeChatProcesses();
    }
}

