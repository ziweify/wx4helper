using BaiShengVx3Plus.Native;
using BaiShengVx3Plus.Models;
using System.Text;
using System.Collections.Concurrent;

namespace BaiShengVx3Plus.Services
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

