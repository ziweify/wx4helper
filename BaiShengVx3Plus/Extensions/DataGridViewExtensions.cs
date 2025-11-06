using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using BaiShengVx3Plus.Attributes;

namespace BaiShengVx3Plus.Extensions
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
    }
}

