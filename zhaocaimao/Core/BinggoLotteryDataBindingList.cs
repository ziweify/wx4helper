using System;
using System.ComponentModel;
using System.Linq;
using zhaocaimao.Contracts;
using zhaocaimao.Models.Games.Binggo;
using SQLite;

namespace zhaocaimao.Core
{
    /// <summary>
    /// 炳狗开奖数据 BindingList（线程安全、自动保存）
    /// 
    /// 核心功能：
    /// 1. 与 DataGridView 双向绑定
    /// 2. 自动保存到数据库
    /// 3. 支持手动添加/修改开奖数据
    /// 4. 线程安全操作
    /// </summary>
    public class BinggoLotteryDataBindingList : BindingList<BinggoLotteryData>
    {
        private readonly SQLiteConnection _db;
        private readonly ILogService _logService;
        private readonly object _lock = new object();
        
        public BinggoLotteryDataBindingList(SQLiteConnection db, ILogService logService)
        {
            _db = db;
            _logService = logService;
            
            // 🔥 重要：检查并迁移数据库表结构
            try
            {
                // 尝试查询表，如果列不匹配会抛出异常
                var testQuery = _db.Table<BinggoLotteryData>().Take(1).ToList();
                _logService.Info("LotteryData", "数据表验证通过");
            }
            catch (Exception ex)
            {
                _logService.Warning("LotteryData", 
                    $"表结构异常，准备重建: {ex.Message}");
                
                try
                {
                    // 删除旧表
                    _db.Execute("DROP TABLE IF EXISTS BinggoLotteryData");
                    _logService.Info("LotteryData", "旧表已清理");
                }
                catch (Exception dropEx)
                {
                    _logService.Warning("LotteryData", $"清理旧表失败: {dropEx.Message}");
                }
            }
            
            // 创建或重建表
            _db.CreateTable<BinggoLotteryData>();
            _logService.Info("LotteryData", "数据表初始化完成");
            
            // 启用通知
            AllowEdit = true;
            AllowNew = true;
            AllowRemove = false;  // 不允许删除开奖数据
            RaiseListChangedEvents = true;
        }
        
        /// <summary>
        /// 从数据库加载最近的开奖数据
        /// </summary>
        /// <param name="limit">加载数量</param>
        public void LoadFromDatabase(int limit = 100)
        {
            lock (_lock)
            {
                try
                {
                    // 🔥 修复：IsOpened 是计算属性（[Ignore]），SQLite-net 无法转换为 SQL
                    // 先查询有开奖数据的记录（LotteryData 不为空），然后在内存中过滤 IsOpened
                    var dataList = _db.Table<BinggoLotteryData>()
                        .Where(d => !string.IsNullOrEmpty(d.LotteryData))
                        .OrderByDescending(d => d.IssueId)
                        .Take(limit * 2) // 多取一些，因为可能有些记录 LotteryData 不完整
                        .ToList()
                        .Where(d => d.IsOpened) // 在内存中过滤，确保已开奖
                        .Take(limit)
                        .ToList();
                    
                    Clear();
                    
                    foreach (var data in dataList)
                    {
                        base.InsertItem(Count, data);
                        
                        // 订阅属性变化
                        data.PropertyChanged += OnDataPropertyChanged;
                    }
                    
                    _logService.Info("LotteryData", $"已加载 {dataList.Count} 条记录");
                }
                catch (Exception ex)
                {
                    _logService.Error("LotteryData", $"数据加载失败: {ex.Message}", ex);
                }
            }
        }
        
        /// <summary>
        /// 添加或更新开奖数据（线程安全）
        /// </summary>
        /// <param name="data">开奖数据</param>
        public void AddOrUpdate(BinggoLotteryData data)
        {
            lock (_lock)
            {
                try
                {
                    // 查找是否已存在
                    var existing = this.FirstOrDefault(d => d.IssueId == data.IssueId);
                    
                    if (existing != null)
                    {
                        // 更新现有数据
                        existing.LotteryData = data.LotteryData;
                        existing.OpenTime = data.OpenTime;
                        
                        // 保存到数据库
                        _db.Update(existing);
                        
                        _logService.Info("LotteryData", 
                            $"数据已更新: {data.ToLotteryString()}");
                        
                        // 触发列表变更事件
                        ResetBindings();
                    }
                    else
                    {
                        // 新增数据
                        Add(data);
                    }
                }
                catch (Exception ex)
                {
                    _logService.Error("LotteryData", 
                        $"数据操作失败: {ex.Message}", ex);
                }
            }
        }
        
        /// <summary>
        /// 重写 InsertItem：添加时自动保存到数据库
        /// </summary>
        protected override void InsertItem(int index, BinggoLotteryData item)
        {
            lock (_lock)
            {
                try
                {
                    // 检查是否已存在
                    var existing = _db.Table<BinggoLotteryData>()
                        .FirstOrDefault(d => d.IssueId == item.IssueId);
                    
                    if (existing == null)
                    {
                        // 插入新记录
                        _db.Insert(item);
                        item.Id = (int)_db.ExecuteScalar<long>("SELECT last_insert_rowid()");
                        
                        _logService.Info("LotteryData", 
                            $"新增记录: {item.IssueId}");
                    }
                    else
                    {
                        // 更新现有记录
                        item.Id = existing.Id;
                        _db.Update(item);
                        
                        _logService.Info("LotteryData", 
                            $"数据同步: {item.IssueId}");
                    }
                    
                    base.InsertItem(index, item);
                    
                    // 订阅属性变化
                    item.PropertyChanged += OnDataPropertyChanged;
                }
                catch (Exception ex)
                {
                    _logService.Error("LotteryData", 
                        $"数据写入失败: {ex.Message}", ex);
                }
            }
        }
        
        /// <summary>
        /// 重写 SetItem：修改时自动保存到数据库
        /// </summary>
        protected override void SetItem(int index, BinggoLotteryData item)
        {
            lock (_lock)
            {
                try
                {
                    if (item.Id > 0)
                    {
                        _db.Update(item);
                        _logService.Info("LotteryData", 
                            $"记录变更: {item.IssueId}");
                    }
                    
                    base.SetItem(index, item);
                }
                catch (Exception ex)
                {
                    _logService.Error("LotteryData", 
                        $"数据更新失败: {ex.Message}", ex);
                }
            }
        }
        
        /// <summary>
        /// 属性变化时自动保存
        /// </summary>
        private void OnDataPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is BinggoLotteryData data && data.Id > 0)
            {
                lock (_lock)
                {
                    try
                    {
                        _db.Update(data);
                        _logService.Info("LotteryData", 
                            $"属性变更已保存: {data.IssueId} ({e.PropertyName})");
                    }
                    catch (Exception ex)
                    {
                        _logService.Warning("LotteryData", 
                            $"自动保存失败: {ex.Message}");
                    }
                }
            }
        }
        
        /// <summary>
        /// 获取指定期号的数据
        /// </summary>
        public BinggoLotteryData? GetByIssueId(int issueId)
        {
            lock (_lock)
            {
                return this.FirstOrDefault(d => d.IssueId == issueId);
            }
        }
        
        /// <summary>
        /// 刷新绑定（用于手动刷新 UI）
        /// </summary>
        public void RefreshBindings()
        {
            ResetBindings();
        }
    }
}

