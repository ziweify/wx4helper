using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using zhaocaimao.Attributes;

namespace zhaocaimao.Extensions
{
    /// <summary>
    /// DataGridView 扩展方法
    /// 提供基于特性的自动配置功能
    /// </summary>
    public static class DataGridViewExtensions
    {
        /// <summary>
        /// 🔥 从模型特性自动配置 DataGridView
        /// 
        /// 使用方法：
        /// <code>
        /// dgvMembers.ConfigureFromModel&lt;V2Member&gt;();
        /// </code>
        /// 
        /// 特性优先级：
        /// 1. DataGridColumnAttribute（自定义特性）
        /// 2. DisplayNameAttribute（标准特性）
        /// 3. BrowsableAttribute（控制可见性）
        /// 4. DisplayFormatAttribute（标准格式化）
        /// 5. 🔥 自动处理枚举类型的中文显示
        /// </summary>
        public static void ConfigureFromModel<T>(this DataGridView dgv)
        {
            if (dgv.Columns.Count == 0) return;
            
            var properties = typeof(T).GetProperties()
                .Where(p => p.GetCustomAttribute<BrowsableAttribute>()?.Browsable != false)
                .ToList();
            
            // 🔥 第一遍：配置每个列的属性
            foreach (var prop in properties)
            {
                var column = dgv.Columns[prop.Name];
                if (column == null) continue;
                
                // 优先使用自定义 DataGridColumnAttribute
                var dgAttr = prop.GetCustomAttribute<DataGridColumnAttribute>();
                if (dgAttr != null)
                {
                    ApplyDataGridColumnAttribute(column, dgAttr);
                }
                else
                {
                    // 备用：使用标准特性
                    ApplyStandardAttributes(column, prop);
                }
                
                // 🔥 检查是否是枚举类型，如果是则标记需要特殊处理
                if (prop.PropertyType.IsEnum || 
                    (Nullable.GetUnderlyingType(prop.PropertyType)?.IsEnum ?? false))
                {
                    column.Tag = new EnumColumnInfo
                    {
                        PropertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType,
                        IsEnum = true
                    };
                }
            }
            
            // 🔥 注册 CellFormatting 事件处理枚举显示（只注册一次）
            if (!dgv.Tag?.ToString()?.Contains("EnumFormattingRegistered") ?? true)
            {
                dgv.CellFormatting += DataGridView_CellFormatting_EnumHandler;
                dgv.Tag = (dgv.Tag?.ToString() ?? "") + "EnumFormattingRegistered";
            }
            
            // 🔥 第二遍：按 Order 排序列
            var orderedProperties = properties
                .Select(p => new
                {
                    Property = p,
                    Column = dgv.Columns[p.Name],
                    Order = p.GetCustomAttribute<DataGridColumnAttribute>()?.Order ?? int.MaxValue
                })
                .Where(x => x.Column != null)
                .OrderBy(x => x.Order)
                .ThenBy(x => x.Property.Name)
                .ToList();
            
            for (int i = 0; i < orderedProperties.Count; i++)
            {
                if (orderedProperties[i].Column != null)
                {
                    orderedProperties[i].Column.DisplayIndex = i;
                }
            }
        }
        
        /// <summary>
        /// 应用自定义 DataGridColumnAttribute 配置
        /// </summary>
        private static void ApplyDataGridColumnAttribute(DataGridViewColumn column, DataGridColumnAttribute attr)
        {
            // 列标题
            if (!string.IsNullOrEmpty(attr.HeaderText))
            {
                column.HeaderText = attr.HeaderText;
            }
            
            // 列宽
            if (attr.Width > 0)
            {
                column.Width = attr.Width;
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            }
            
            // 最小宽度
            if (attr.MinimumWidth > 0)
            {
                column.MinimumWidth = attr.MinimumWidth;
            }
            
            // 自动调整模式
            if (attr.AutoSizeMode != DataGridViewAutoSizeColumnMode.NotSet)
            {
                column.AutoSizeMode = attr.AutoSizeMode;
            }
            
            // 可见性
            column.Visible = attr.Visible;
            
            // 🔥 格式化字符串
            if (!string.IsNullOrEmpty(attr.Format))
            {
                // 移除 {0: 和 } 包装，只保留格式化部分
                var format = attr.Format;
                if (format.StartsWith("{0:"))
                {
                    format = format.Substring(3);
                }
                if (format.EndsWith("}"))
                {
                    format = format.Substring(0, format.Length - 1);
                }
                column.DefaultCellStyle.Format = format;
            }
            
            // 对齐方式
            if (attr.Alignment != DataGridViewContentAlignment.NotSet)
            {
                column.DefaultCellStyle.Alignment = attr.Alignment;
            }
            
            // 只读
            column.ReadOnly = attr.ReadOnly;
        }
        
        /// <summary>
        /// 应用标准特性配置（备用方案）
        /// </summary>
        private static void ApplyStandardAttributes(DataGridViewColumn column, PropertyInfo prop)
        {
            // DisplayName 特性
            var displayNameAttr = prop.GetCustomAttribute<DisplayNameAttribute>();
            if (displayNameAttr != null)
            {
                column.HeaderText = displayNameAttr.DisplayName;
            }
            
            // DisplayFormat 特性
            var formatAttr = prop.GetCustomAttribute<DisplayFormatAttribute>();
            if (formatAttr != null && !string.IsNullOrEmpty(formatAttr.DataFormatString))
            {
                var format = formatAttr.DataFormatString;
                if (format.StartsWith("{0:"))
                {
                    format = format.Substring(3);
                }
                if (format.EndsWith("}"))
                {
                    format = format.Substring(0, format.Length - 1);
                }
                column.DefaultCellStyle.Format = format;
            }
            
            // Browsable 特性（控制可见性）
            var browsableAttr = prop.GetCustomAttribute<BrowsableAttribute>();
            if (browsableAttr != null && !browsableAttr.Browsable)
            {
                column.Visible = false;
            }
        }
        
        /// <summary>
        /// 🔥 隐藏指定列（辅助方法）
        /// </summary>
        public static void HideColumn(this DataGridView dgv, string columnName)
        {
            if (dgv.Columns[columnName] is DataGridViewColumn column)
            {
                column.Visible = false;
            }
        }
        
        /// <summary>
        /// 🔥 批量隐藏列（辅助方法）
        /// </summary>
        public static void HideColumns(this DataGridView dgv, params string[] columnNames)
        {
            foreach (var columnName in columnNames)
            {
                dgv.HideColumn(columnName);
            }
        }
        
        /// <summary>
        /// 🔥 显示指定列（辅助方法）
        /// </summary>
        public static void ShowColumn(this DataGridView dgv, string columnName)
        {
            if (dgv.Columns[columnName] is DataGridViewColumn column)
            {
                column.Visible = true;
            }
        }
        
        /// <summary>
        /// 🔥 枚举列信息（用于 Column.Tag）
        /// </summary>
        private class EnumColumnInfo
        {
            public Type? PropertyType { get; set; }
            public bool IsEnum { get; set; }
        }
        
        /// <summary>
        /// 🔥 CellFormatting 事件处理器：自动显示枚举的中文名称
        /// 
        /// 原理：
        /// 1. DataGridView 默认显示枚举的 ToString()（如："未知"、"上分"、"下分"）
        /// 2. 但某些情况下可能显示数值（0、1、2）
        /// 3. 通过 CellFormatting 事件统一转换为中文名称
        /// 
        /// 示例：
        /// - CreditWithdrawAction.上分 → "上分"
        /// - CreditWithdrawStatus.已同意 → "已同意"
        /// - MemberState.管理 → "管理"
        /// </summary>
        private static void DataGridView_CellFormatting_EnumHandler(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (sender is not DataGridView dgv) return;
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            
            try
            {
                // 检查列是否标记为枚举列
                var column = dgv.Columns[e.ColumnIndex];
                if (column.Tag is EnumColumnInfo enumInfo && enumInfo.IsEnum && enumInfo.PropertyType != null)
                {
                    // 获取单元格的原始值
                    var cellValue = e.Value;
                    
                    if (cellValue != null)
                    {
                        // 🔥 关键：将枚举值转换为中文名称
                        if (cellValue.GetType() == enumInfo.PropertyType || 
                            cellValue.GetType().IsEnum)
                        {
                            // 直接使用枚举的 ToString()（C# 会自动返回枚举的名称，如"上分"）
                            e.Value = cellValue.ToString();
                            e.FormattingApplied = true;
                        }
                        else if (cellValue is int or long)
                        {
                            // 如果是数值，转换为枚举再转换为字符串
                            var numericValue = Convert.ToInt32(cellValue);
                            var enumValue = Enum.ToObject(enumInfo.PropertyType, numericValue);
                            e.Value = enumValue.ToString();
                            e.FormattingApplied = true;
                        }
                    }
                    else
                    {
                        // 如果值为 null，显示空字符串
                        e.Value = "";
                        e.FormattingApplied = true;
                    }
                }
            }
            catch
            {
                // 如果转换失败，保持原值不变
                // 不抛出异常，避免影响 DataGridView 的正常显示
            }
        }
    }
}

