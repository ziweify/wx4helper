using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SQLite;
using System.Linq;
using BaiShengVx3Plus.Contracts;

namespace BaiShengVx3Plus.Services.Database
{
    /// <summary>
    /// 属性变化追踪器（监听模型属性变化并自动保存单个字段）
    /// 核心优势：只更新改变的字段，性能最优
    /// </summary>
    public class PropertyChangeTracker : IPropertyChangeTracker
    {
        private readonly IDatabaseService _dbService;
        private readonly ILogService _logService;

        // 追踪的对象和表名映射
        private readonly Dictionary<INotifyPropertyChanged, string> _trackedObjects = new();
        
        // 用于同步访问追踪字典
        private readonly object _lock = new object();

        public PropertyChangeTracker(IDatabaseService dbService, ILogService logService)
        {
            _dbService = dbService;
            _logService = logService;
        }

        /// <summary>
        /// 开始追踪对象的属性变化
        /// </summary>
        public void Track<T>(T obj, string tableName) where T : INotifyPropertyChanged
        {
            if (obj == null) return;

            lock (_lock)
            {
                if (_trackedObjects.ContainsKey(obj))
                {
                    _logService.Debug("PropertyChangeTracker", $"对象已在追踪中: {tableName}");
                    return;
                }

                // 订阅属性变化事件
                obj.PropertyChanged += OnPropertyChanged;
                _trackedObjects[obj] = tableName;

                _logService.Debug("PropertyChangeTracker", $"✓ 开始追踪: {tableName}");
            }
        }

        /// <summary>
        /// 停止追踪对象的属性变化
        /// </summary>
        public void Untrack<T>(T obj) where T : INotifyPropertyChanged
        {
            if (obj == null) return;

            lock (_lock)
            {
                if (_trackedObjects.ContainsKey(obj))
                {
                    obj.PropertyChanged -= OnPropertyChanged;
                    _trackedObjects.Remove(obj);
                    
                    _logService.Debug("PropertyChangeTracker", "✓ 停止追踪");
                }
            }
        }

        /// <summary>
        /// 清除所有追踪
        /// </summary>
        public void ClearAll()
        {
            lock (_lock)
            {
                foreach (var obj in _trackedObjects.Keys.ToList())
                {
                    obj.PropertyChanged -= OnPropertyChanged;
                }
                _trackedObjects.Clear();
                
                _logService.Info("PropertyChangeTracker", "✓ 已清除所有追踪");
            }
        }

        /// <summary>
        /// 属性变化事件处理器（核心逻辑）
        /// </summary>
        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender == null || e.PropertyName == null) return;

            string tableName;
            lock (_lock)
            {
                if (!_trackedObjects.TryGetValue((INotifyPropertyChanged)sender, out tableName!))
                {
                    return; // 对象已不在追踪中
                }
            }

            try
            {
                // 🔥 核心逻辑：只更新改变的那个字段
                SaveSingleProperty(sender, tableName, e.PropertyName);
            }
            catch (Exception ex)
            {
                _logService.Error("PropertyChangeTracker", 
                    $"保存属性失败: {tableName}.{e.PropertyName}", ex);
            }
        }

        /// <summary>
        /// 保存单个属性到数据库（只更新一个字段，性能最优）
        /// </summary>
        private void SaveSingleProperty(object obj, string tableName, string propertyName)
        {
            // 跳过不需要保存的属性
            if (propertyName == "Id" || propertyName == "TimeStampCreate")
            {
                return; // Id 是主键，TimeStampCreate 不应修改
            }

            // 获取对象类型和属性值
            var type = obj.GetType();
            var property = type.GetProperty(propertyName);
            
            if (property == null)
            {
                _logService.Warning("PropertyChangeTracker", $"属性不存在: {propertyName}");
                return;
            }

            var value = property.GetValue(obj);
            var idProperty = type.GetProperty("Id");
            
            if (idProperty == null)
            {
                _logService.Warning("PropertyChangeTracker", "对象没有 Id 属性");
                return;
            }

            var id = idProperty.GetValue(obj);

            // 🔥 立即同步写入数据库（只更新一个字段）
            using var conn = _dbService.GetConnection();
            using var transaction = conn.BeginTransaction();

            try
            {
                // 动态构造 SQL（只更新改变的字段）
                var sql = $"UPDATE {tableName} SET {propertyName} = @Value WHERE Id = @Id";

                using var cmd = new SQLiteCommand(sql, conn, transaction);
                cmd.Parameters.AddWithValue("@Value", value ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Id", id);

                var affected = cmd.ExecuteNonQuery();

                transaction.Commit();

                if (affected > 0)
                {
                    _logService.Debug("PropertyChangeTracker", 
                        $"✓ 字段已保存: {tableName}.{propertyName} = {value} (Id: {id})");
                }
                else
                {
                    _logService.Warning("PropertyChangeTracker", 
                        $"未找到记录: {tableName} (Id: {id})");
                }
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}

