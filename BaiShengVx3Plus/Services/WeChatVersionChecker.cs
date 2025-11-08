using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace BaiShengVx3Plus.Services
{
    /// <summary>
    /// 微信版本检测服务
    /// </summary>
    public class WeChatVersionChecker
    {
        private const string REQUIRED_VERSION = "4.1.0.21";
        private const string WECHAT_REGISTRY_KEY = @"SOFTWARE\Tencent\WeChat";
        private const string WECHAT_INSTALLER_PATH = @"tools\WeChatWin4.1.0.21.exe";
        
        /// <summary>
        /// 检查微信版本是否符合要求
        /// </summary>
        public static (bool isValid, string currentVersion) CheckVersion()
        {
            try
            {
                // 1. 从注册表读取微信版本
                using var key = Registry.CurrentUser.OpenSubKey(WECHAT_REGISTRY_KEY);
                if (key == null)
                {
                    // 尝试 LocalMachine
                    using var keyLM = Registry.LocalMachine.OpenSubKey(WECHAT_REGISTRY_KEY);
                    if (keyLM == null)
                    {
                        return (false, "未安装");
                    }
                    
                    var versionLM = keyLM.GetValue("Version")?.ToString() ?? "";
                    return (versionLM == REQUIRED_VERSION, versionLM);
                }
                
                var version = key.GetValue("Version")?.ToString() ?? "";
                if (string.IsNullOrEmpty(version))
                {
                    // 尝试从微信安装路径读取版本信息
                    var installPath = key.GetValue("InstallPath")?.ToString() ?? "";
                    if (!string.IsNullOrEmpty(installPath))
                    {
                        var wechatExe = Path.Combine(installPath, "WeChat.exe");
                        if (File.Exists(wechatExe))
                        {
                            var versionInfo = FileVersionInfo.GetVersionInfo(wechatExe);
                            version = versionInfo.FileVersion ?? "";
                        }
                    }
                }
                
                return (version == REQUIRED_VERSION, version);
            }
            catch (Exception ex)
            {
                return (false, $"检测失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 获取微信安装程序的完整路径
        /// </summary>
        public static string GetInstallerPath()
        {
            var appDir = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(appDir, WECHAT_INSTALLER_PATH);
        }
        
        /// <summary>
        /// 检查安装程序是否存在
        /// </summary>
        public static bool InstallerExists()
        {
            var installerPath = GetInstallerPath();
            return File.Exists(installerPath);
        }
        
        /// <summary>
        /// 启动微信安装程序
        /// </summary>
        public static async Task<bool> InstallWeChatAsync(IProgress<string>? progress = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var installerPath = GetInstallerPath();
                
                if (!File.Exists(installerPath))
                {
                    progress?.Report($"❌ 安装程序不存在: {installerPath}");
                    return false;
                }
                
                progress?.Report($"🚀 正在启动安装程序...");
                progress?.Report($"📁 路径: {installerPath}");
                
                // 启动安装程序（需要管理员权限）
                var startInfo = new ProcessStartInfo
                {
                    FileName = installerPath,
                    UseShellExecute = true,  // 使用 Shell 执行，可以触发 UAC
                    Verb = "runas"  // 请求管理员权限
                };
                
                var process = Process.Start(startInfo);
                if (process == null)
                {
                    progress?.Report("❌ 启动安装程序失败");
                    return false;
                }
                
                progress?.Report($"⏳ 等待安装完成...");
                progress?.Report($"💡 请在安装程序中完成安装步骤");
                
                // 等待安装程序退出
                await Task.Run(() =>
                {
                    while (!process.HasExited && !cancellationToken.IsCancellationRequested)
                    {
                        process.WaitForExit(1000);
                    }
                }, cancellationToken);
                
                if (cancellationToken.IsCancellationRequested)
                {
                    progress?.Report("⚠️ 安装已取消");
                    return false;
                }
                
                progress?.Report($"✅ 安装程序已退出，退出码: {process.ExitCode}");
                
                // 等待一下，让注册表更新
                await Task.Delay(2000, cancellationToken);
                
                // 验证安装结果
                var (isValid, currentVersion) = CheckVersion();
                if (isValid)
                {
                    progress?.Report($"✅ 微信 {REQUIRED_VERSION} 安装成功！");
                    return true;
                }
                else
                {
                    progress?.Report($"⚠️ 检测到版本: {currentVersion}");
                    progress?.Report($"💡 如果您已完成安装，请重启本程序");
                    return false;
                }
            }
            catch (System.ComponentModel.Win32Exception ex) when (ex.NativeErrorCode == 1223)
            {
                // 用户取消了 UAC 提示
                progress?.Report("⚠️ 用户取消了管理员权限请求");
                return false;
            }
            catch (Exception ex)
            {
                progress?.Report($"❌ 安装失败: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 启动微信
        /// </summary>
        public static async Task<bool> LaunchWeChatAsync(IProgress<string>? progress = null)
        {
            try
            {
                progress?.Report("🚀 正在启动微信...");
                
                // 从注册表获取微信安装路径
                using var key = Registry.CurrentUser.OpenSubKey(WECHAT_REGISTRY_KEY) 
                    ?? Registry.LocalMachine.OpenSubKey(WECHAT_REGISTRY_KEY);
                
                if (key == null)
                {
                    progress?.Report("❌ 无法找到微信安装信息");
                    return false;
                }
                
                var installPath = key.GetValue("InstallPath")?.ToString() ?? "";
                if (string.IsNullOrEmpty(installPath))
                {
                    progress?.Report("❌ 无法获取微信安装路径");
                    return false;
                }
                
                var wechatExe = Path.Combine(installPath, "WeChat.exe");
                if (!File.Exists(wechatExe))
                {
                    progress?.Report($"❌ 微信程序不存在: {wechatExe}");
                    return false;
                }
                
                // 启动微信
                Process.Start(new ProcessStartInfo
                {
                    FileName = wechatExe,
                    UseShellExecute = true
                });
                
                progress?.Report("✅ 微信已启动");
                
                // 等待微信启动
                await Task.Delay(3000);
                
                return true;
            }
            catch (Exception ex)
            {
                progress?.Report($"❌ 启动微信失败: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 获取需要的版本号
        /// </summary>
        public static string GetRequiredVersion()
        {
            return REQUIRED_VERSION;
        }
    }
}

