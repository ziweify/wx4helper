using System;
using System.Windows.Forms;

namespace zhaocaimao.Attributes
{
    /// <summary>
    /// DataGridView 列配置特性
    /// 用于在模型属性上声明式配置列的显示方式
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class DataGridColumnAttribute : Attribute
    {
        /// <summary>
        /// 列标题（中文显示名称）
        /// </summary>
        public string? HeaderText { get; set; }
        
        /// <summary>
        /// 列宽度（像素），-1 表示自动
        /// </summary>
        public int Width { get; set; } = -1;
        
        /// <summary>
        /// 是否可见
        /// </summary>
        public bool Visible { get; set; } = true;
        
        /// <summary>
        /// 格式化字符串
        /// 例如：
        /// - "{0:F2}" - 固定2位小数：1234.56
        /// - "{0:N2}" - 千分位+2位小数：1,234.56
        /// - "{0:+0.00;-0.00;0.00}" - 显示正负号：+123.45
        /// - "{0:P1}" - 百分比：12.3%
        /// </summary>
        public string? Format { get; set; }
        
        /// <summary>
        /// 显示顺序（数字越小越靠前）
        /// </summary>
        public int Order { get; set; } = int.MaxValue;
        
        /// <summary>
        /// 对齐方式
        /// </summary>
        public DataGridViewContentAlignment Alignment { get; set; } = DataGridViewContentAlignment.NotSet;
        
        /// <summary>
        /// 是否只读
        /// </summary>
        public bool ReadOnly { get; set; } = false;
        
        /// <summary>
        /// 最小宽度
        /// </summary>
        public int MinimumWidth { get; set; } = 5;
        
        /// <summary>
        /// 自动调整列宽模式
        /// </summary>
        public DataGridViewAutoSizeColumnMode AutoSizeMode { get; set; } = DataGridViewAutoSizeColumnMode.NotSet;
    }
}

