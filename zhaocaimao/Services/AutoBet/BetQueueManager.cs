using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using zhaocaimao.Contracts;
using zhaocaimao.Models.AutoBet;

namespace zhaocaimao.Services.AutoBet
{
    /// <summary>
    /// 投注队列管理器
    /// 管理投注任务的异步执行，避免阻塞
    /// </summary>
    public class BetQueueManager
    {
        private readonly ILogService _log;
        private readonly BetRecordService _betRecordService;
        private readonly ConcurrentDictionary<int, Task> _pendingBets = new();  // recordId -> Task
        
        public BetQueueManager(ILogService log, BetRecordService betRecordService)
        {
            _log = log;
            _betRecordService = betRecordService;
        }
        
        /// <summary>
        /// 添加投注任务到队列（异步执行）
        /// </summary>
        public void EnqueueBet(int recordId, Func<Task<BetResult>> betAction)
        {
            var task = Task.Run(async () =>
            {
                try
                {
                    _log.Info("BetQueueManager", $"开始执行投注:RecordId={recordId}");
                    
                    var result = await betAction();
                    
                    // 更新投注记录
                    _betRecordService.UpdateResult(
                        recordId,
                        result.Success,
                        result.Result,
                        result.ErrorMessage,
                        result.PostStartTime,
                        result.PostEndTime,
                        result.OrderNo
                    );
                    
                    _log.Info("BetQueueManager", 
                        $"投注完成:RecordId={recordId} 成功={result.Success} 耗时={result.DurationMs}ms");
                }
                catch (Exception ex)
                {
                    _log.Error("BetQueueManager", $"投注执行异常:RecordId={recordId}", ex);
                    
                    // 记录异常
                    _betRecordService.UpdateResult(
                        recordId,
                        false,
                        null,
                        ex.Message,
                        null,
                        null,
                        null
                    );
                }
                finally
                {
                    // 移除完成的任务
                    _pendingBets.TryRemove(recordId, out _);
                }
            });
            
            _pendingBets.TryAdd(recordId, task);
            
            _log.Info("BetQueueManager", $"✅ 投注任务已加入队列:RecordId={recordId} 当前队列数:{_pendingBets.Count}");
        }
        
        /// <summary>
        /// 获取待执行的投注数量
        /// </summary>
        public int GetPendingCount()
        {
            return _pendingBets.Count;
        }
        
        /// <summary>
        /// 等待所有投注完成（用于测试）
        /// </summary>
        public async Task WaitAllAsync(int timeoutMs = 30000)
        {
            var cts = new CancellationTokenSource(timeoutMs);
            
            try
            {
                await Task.WhenAll(_pendingBets.Values).WaitAsync(cts.Token);
            }
            catch (OperationCanceledException)
            {
                _log.Warning("BetQueueManager", $"等待投注超时:{timeoutMs}ms 还有{_pendingBets.Count}个未完成");
            }
        }
    }
    
}

