using System.Runtime.InteropServices;
using System.Text;
using System.IO;

namespace zhaocaimao.Native
{
    /// <summary>
    /// Loader.dll P/Invoke 声明
    /// </summary>
    public static class LoaderNative
    {
        private const string DLL_NAME = "Loader.dll";

        // 🔥 静态构造函数：在使用前加载 DLL
        static LoaderNative()
        {
            // 获取固定路径：bin\release\net8.0-windows\Loader.dll
            //var basePath = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar));
            // 🔥 使用固定路径：bin\release\net8.0-windows\WeixinX.dll
            var basePath = AppDomain.CurrentDomain.BaseDirectory;

            basePath = Path.GetDirectoryName(basePath); // 回到 bin 目录
            
            if (string.IsNullOrEmpty(basePath))
            {
                throw new InvalidOperationException("无法获取应用程序基础路径");
            }
            
            var dllPath = Path.Combine(basePath,  "Loader.dll");
            
            Console.WriteLine($"[LoaderNative] 加载 Loader.dll: {dllPath}");
            
            if (!File.Exists(dllPath))
            {
                throw new FileNotFoundException($"找不到 Loader.dll: {dllPath}");
            }

            // 使用 LoadLibrary 预加载 DLL
            var handle = LoadLibrary(dllPath);
            if (handle == IntPtr.Zero)
            {
                var error = Marshal.GetLastWin32Error();
                throw new DllNotFoundException($"无法加载 Loader.dll: {dllPath}, Error: {error}");
            }
            
            Console.WriteLine($"[LoaderNative] ✓ Loader.dll 加载成功");
        }

        // Windows API: LoadLibrary
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr LoadLibrary(string lpFileName);

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

