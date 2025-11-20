using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using zhaocaimao.Native;
using zhaocaimao.Models;
using zhaocaimao.Contracts;

namespace zhaocaimao.Services.WeChat
{
    /// <summary>
    /// 微信加载器服务实现
    /// 这是一个有状态的服务对象，管理所有微信进程
    /// 
    /// 面向对象特性：
    /// 1. 封装：隐藏了Native调用细节，提供简洁的接口
    /// 2. 抽象：通过接口定义行为契约
    /// 3. 单一职责：只负责微信的启动、注入和进程管理
    /// 4. 依赖倒置：依赖抽象（接口）而不是具体实现
    /// </summary>
    public class WeChatLoaderService : IWeChatLoaderService
    {
        // ========================================
        // 私有字段（对象的状态）
        // ========================================
        
        /// <summary>
        /// 管理的所有微信进程（线程安全的字典）
        /// 这就是面向对象中的"对象状态"
        /// </summary>
        private readonly ConcurrentDictionary<uint, WeChatProcess> _managedProcesses;

        /// <summary>
        /// 配置信息
        /// </summary>
        private readonly WeChatLoaderConfig _config;

        /// <summary>
        /// 事件：进程启动时触发
        /// </summary>
        public event EventHandler<WeChatProcess>? ProcessLaunched;

        /// <summary>
        /// 事件：注入成功时触发
        /// </summary>
        public event EventHandler<WeChatProcess>? ProcessInjected;

        // ========================================
        // 构造函数
        // ========================================

        public WeChatLoaderService()
        {
            _managedProcesses = new ConcurrentDictionary<uint, WeChatProcess>();
            _config = new WeChatLoaderConfig
            {
                RabbitMqIp = "127.0.0.1",
                RabbitMqPort = "5672",
                DefaultDllPath = "WeixinX.dll"
            };
        }

        // ========================================
        // 公共方法（对象的行为）
        // ========================================

        /// <summary>
        /// 启动微信并注入（带状态管理）
        /// </summary>
        public bool LaunchWeChat(string ip, string port, string dllPath, out string errorMessage)
        {
            var error = new StringBuilder(512);
            bool result = LoaderNative.LaunchWeChatWithInjection(ip, port, dllPath, error, 512);
            errorMessage = error.ToString();

            if (result)
            {
                // 启动成功，获取新进程并管理
                var processes = GetWeChatProcesses();
                if (processes.Count > 0)
                {
                    var newPid = processes.Last();
                    var process = new WeChatProcess(newPid);
                    process.MarkAsInjected(dllPath);
                    
                    // 添加到管理列表
                    _managedProcesses.TryAdd(newPid, process);
                    
                    // 触发事件
                    ProcessLaunched?.Invoke(this, process);
                    ProcessInjected?.Invoke(this, process);
                }
            }

            return result;
        }

        /// <summary>
        /// 注入到现有进程（带状态管理）
        /// </summary>
        public bool InjectToProcess(uint processId, string dllPath, out string errorMessage)
        {
            var error = new StringBuilder(512);
            bool result = LoaderNative.InjectDllToProcess(processId, dllPath, error, 512);
            errorMessage = error.ToString();

            if (result)
            {
                // 注入成功，更新或创建进程对象
                var process = _managedProcesses.GetOrAdd(processId, pid => new WeChatProcess(pid));
                process.MarkAsInjected(dllPath);
                
                // 触发事件
                ProcessInjected?.Invoke(this, process);
            }

            return result;
        }

        /// <summary>
        /// 获取所有微信进程ID
        /// </summary>
        public List<uint> GetWeChatProcesses()
        {
            uint[] pids = new uint[10];
            int count = LoaderNative.GetWeChatProcesses(pids, 10);
            return pids.Take(count).ToList();
        }

        /// <summary>
        /// 智能启动或注入（高层方法）
        /// - 如果微信已运行，尝试注入到现有进程
        /// - 如果微信未运行，启动微信并注入
        /// </summary>
        public async Task<(bool Success, string ErrorMessage)> LaunchOrInjectAsync(string dllPath, CancellationToken cancellationToken = default)
        {
            // 检查 DLL 文件是否存在
            if (!File.Exists(dllPath))
            {
                return (false, $"找不到 DLL 文件: {dllPath}");
            }

            // 获取运行中的微信进程
            var processes = GetWeChatProcesses();
            Console.WriteLine($"[WeChatLoaderService] 检测到 {processes.Count} 个微信进程");

            if (processes.Count > 0)
            {
                Console.WriteLine($"[WeChatLoaderService] 尝试注入到现有进程...");
                
                // 微信已运行，尝试注入到现有进程
                foreach (var processId in processes)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    
                    Console.WriteLine($"[WeChatLoaderService] 正在注入进程 {processId}...");
                    if (InjectToProcess(processId, dllPath, out string error))
                    {
                        // 注入成功，等待生效
                        Console.WriteLine($"[WeChatLoaderService] ✓ 成功注入到进程 {processId}");
                        await Task.Delay(500, cancellationToken);
                        return (true, $"成功注入到进程 {processId}");
                    }
                    else
                    {
                        Console.WriteLine($"[WeChatLoaderService] ✗ 注入进程 {processId} 失败: {error}");
                    }
                }

                // 🔥 所有进程注入失败，强制结束所有微信进程
                Console.WriteLine($"[WeChatLoaderService] 所有进程注入失败，强制结束 {processes.Count} 个进程...");
                foreach (var processId in processes)
                {
                    try
                    {
                        var process = System.Diagnostics.Process.GetProcessById((int)processId);
                        Console.WriteLine($"[WeChatLoaderService] 正在结束进程 {processId}...");
                        process.Kill();
                        process.WaitForExit(3000); // 等待最多3秒
                        Console.WriteLine($"[WeChatLoaderService] ✓ 进程 {processId} 已结束");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[WeChatLoaderService] 结束进程 {processId} 失败: {ex.Message}");
                    }
                }

                // 等待进程完全退出
                Console.WriteLine($"[WeChatLoaderService] 等待进程完全退出...");
                await Task.Delay(1000, cancellationToken);
                Console.WriteLine($"[WeChatLoaderService] 准备重新启动微信...");
            }

            // 微信未运行（或已强制结束），启动微信并注入
            Console.WriteLine($"[WeChatLoaderService] 正在启动微信并注入 DLL...");
            if (LaunchWeChat(_config.RabbitMqIp, _config.RabbitMqPort, dllPath, out string launchError))
            {
                // 启动并注入成功，等待生效
                Console.WriteLine($"[WeChatLoaderService] ✓ 微信启动并注入成功");
                await Task.Delay(500, cancellationToken);
                return (true, "成功启动微信并注入");
            }
            else
            {
                Console.WriteLine($"[WeChatLoaderService] ✗ 启动微信失败: {launchError}");
                return (false, $"启动微信失败: {launchError}");
            }
        }

        // ========================================
        // 状态查询方法（对象的查询行为）
        // ========================================

        /// <summary>
        /// 获取所有管理的进程
        /// </summary>
        public IReadOnlyCollection<WeChatProcess> GetManagedProcesses()
        {
            return _managedProcesses.Values.ToList().AsReadOnly();
        }

        /// <summary>
        /// 获取指定进程信息
        /// </summary>
        public WeChatProcess? GetProcessInfo(uint processId)
        {
            _managedProcesses.TryGetValue(processId, out var process);
            return process;
        }

        /// <summary>
        /// 检查进程是否已注入
        /// </summary>
        public bool IsProcessInjected(uint processId)
        {
            return _managedProcesses.TryGetValue(processId, out var process) && process.IsInjected;
        }

        /// <summary>
        /// 获取已注入的进程数量
        /// </summary>
        public int GetInjectedProcessCount()
        {
            return _managedProcesses.Values.Count(p => p.IsInjected);
        }

        /// <summary>
        /// 更新进程心跳
        /// </summary>
        public void UpdateProcessHeartbeat(uint processId)
        {
            if (_managedProcesses.TryGetValue(processId, out var process))
            {
                process.UpdateHeartbeat();
            }
        }

        /// <summary>
        /// 移除已停止的进程
        /// </summary>
        public void RemoveStoppedProcess(uint processId)
        {
            if (_managedProcesses.TryRemove(processId, out var process))
            {
                process.Status = WeChatProcessStatus.Stopped;
            }
        }

        /// <summary>
        /// 清理所有进程
        /// </summary>
        public void ClearAllProcesses()
        {
            _managedProcesses.Clear();
        }

        /// <summary>
        /// 获取进程统计信息
        /// </summary>
        public ProcessStatistics GetStatistics()
        {
            return new ProcessStatistics
            {
                TotalProcesses = _managedProcesses.Count,
                InjectedProcesses = _managedProcesses.Values.Count(p => p.IsInjected),
                RunningProcesses = _managedProcesses.Values.Count(p => p.IsAlive),
                LastUpdateTime = DateTime.Now
            };
        }
    }

    // ========================================
    // 配置类（值对象）
    // ========================================

    /// <summary>
    /// 微信加载器配置
    /// </summary>
    public class WeChatLoaderConfig
    {
        public string RabbitMqIp { get; set; } = "127.0.0.1";
        public string RabbitMqPort { get; set; } = "5672";
        public string DefaultDllPath { get; set; } = "WeixinX.dll";
    }

    /// <summary>
    /// 进程统计信息（值对象）
    /// </summary>
    public class ProcessStatistics
    {
        public int TotalProcesses { get; set; }
        public int InjectedProcesses { get; set; }
        public int RunningProcesses { get; set; }
        public DateTime LastUpdateTime { get; set; }

        public override string ToString()
        {
            return $"总进程: {TotalProcesses}, 已注入: {InjectedProcesses}, 运行中: {RunningProcesses}";
        }
    }
}

