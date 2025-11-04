using BaiShengVx3Plus.Native;
using System.Text;

namespace BaiShengVx3Plus.Services
{
    /// <summary>
    /// 微信加载器服务实现
    /// </summary>
    public class WeChatLoaderService : IWeChatLoaderService
    {
        public bool LaunchWeChat(string ip, string port, string dllPath, out string errorMessage)
        {
            var error = new StringBuilder(512);
            bool result = LoaderNative.LaunchWeChatWithInjection(ip, port, dllPath, error, 512);
            errorMessage = error.ToString();
            return result;
        }

        public bool InjectToProcess(uint processId, string dllPath, out string errorMessage)
        {
            var error = new StringBuilder(512);
            bool result = LoaderNative.InjectDllToProcess(processId, dllPath, error, 512);
            errorMessage = error.ToString();
            return result;
        }

        public List<uint> GetWeChatProcesses()
        {
            uint[] pids = new uint[10];
            int count = LoaderNative.GetWeChatProcesses(pids, 10);
            return pids.Take(count).ToList();
        }
    }
}

