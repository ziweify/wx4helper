using System;
using System.ComponentModel;
using System.Linq;
using BaiShengVx3Plus.Contracts;
using BaiShengVx3Plus.Models.Games.Binggo;
using SQLite;

namespace BaiShengVx3Plus.Core
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
            
            // 创建表
            _db.CreateTable<BinggoLotteryData>();
            
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
                    var dataList = _db.Table<BinggoLotteryData>()
                        .Where(d => d.IsOpened)
                        .OrderByDescending(d => d.IssueId)
                        .Take(limit)
                        .ToList();
                    
                    Clear();
                    
                    foreach (var data in dataList)
                    {
                        base.InsertItem(Count, data);
                        
                        // 订阅属性变化
                        data.PropertyChanged += OnDataPropertyChanged;
                    }
                    
                    _logService.Info("BinggoLotteryDataBindingList", $"从数据库加载 {dataList.Count} 期数据");
                }
                catch (Exception ex)
                {
                    _logService.Error("BinggoLotteryDataBindingList", $"加载数据失败: {ex.Message}", ex);
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
                        existing.NumbersString = data.NumbersString;
                        existing.IssueStartTime = data.IssueStartTime;
                        existing.OpenTime = data.OpenTime;
                        
                        // 保存到数据库
                        _db.Update(existing);
                        
                        _logService.Info("BinggoLotteryDataBindingList", 
                            $"更新开奖数据: {data.IssueId} - {data.NumbersString}");
                        
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
                    _logService.Error("BinggoLotteryDataBindingList", 
                        $"添加或更新数据失败: {ex.Message}", ex);
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
                        
                        _logService.Info("BinggoLotteryDataBindingList", 
                            $"💾 新增开奖数据: {item.IssueId}");
                    }
                    else
                    {
                        // 更新现有记录
                        item.Id = existing.Id;
                        _db.Update(item);
                        
                        _logService.Info("BinggoLotteryDataBindingList", 
                            $"🔄 更新开奖数据: {item.IssueId}");
                    }
                    
                    base.InsertItem(index, item);
                    
                    // 订阅属性变化
                    item.PropertyChanged += OnDataPropertyChanged;
                }
                catch (Exception ex)
                {
                    _logService.Error("BinggoLotteryDataBindingList", 
                        $"插入数据失败: {ex.Message}", ex);
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
                        _logService.Info("BinggoLotteryDataBindingList", 
                            $"📝 修改开奖数据: {item.IssueId}");
                    }
                    
                    base.SetItem(index, item);
                }
                catch (Exception ex)
                {
                    _logService.Error("BinggoLotteryDataBindingList", 
                        $"修改数据失败: {ex.Message}", ex);
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
                        _logService.Info("BinggoLotteryDataBindingList", 
                            $"🔄 自动保存: {data.IssueId} (修改字段: {e.PropertyName})");
                    }
                    catch (Exception ex)
                    {
                        _logService.Warning("BinggoLotteryDataBindingList", 
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

