using System.Runtime.InteropServices;
using System.Text;

namespace BaiShengVx3Plus.Native
{
    /// <summary>
    /// Loader.dll P/Invoke 声明
    /// </summary>
    public static class LoaderNative
    {
        private const string DLL_NAME = "Loader.dll";

        /// <summary>
        /// 启动微信并注入 WeixinX.dll
        /// </summary>
        [DllImport(DLL_NAME, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LaunchWeChatWithInjection(
            [MarshalAs(UnmanagedType.LPWStr)] string ip,
            [MarshalAs(UnmanagedType.LPWStr)] string port,
            [MarshalAs(UnmanagedType.LPWStr)] string dllPath,
            [MarshalAs(UnmanagedType.LPWStr)] StringBuilder errorMessage,
            int errorMessageSize
        );

        /// <summary>
        /// 注入 DLL 到指定进程
        /// </summary>
        [DllImport(DLL_NAME, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool InjectDllToProcess(
            uint processId,
            [MarshalAs(UnmanagedType.LPWStr)] string dllPath,
            [MarshalAs(UnmanagedType.LPWStr)] StringBuilder errorMessage,
            int errorMessageSize
        );

        /// <summary>
        /// 获取所有微信进程
        /// </summary>
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetWeChatProcesses(
            [Out] uint[] processIds,
            int maxCount
        );
    }
}

