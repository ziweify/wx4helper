using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace BaiShengVx3Plus.Controls
{
    /// <summary>
    /// 自定义DataGridView按钮单元格 - 支持禁用状态
    /// 参考 F5BotV2 CustomDGVButtonCell
    /// </summary>
    public class CustomDGVButtonCell : DataGridViewButtonCell
    {
        public bool Enabled { get; set; }
        
        // 默认启用按钮
        public CustomDGVButtonCell()
        {
            Enabled = true;
        }
        
        // 重写 Clone 方法，复制 Enabled 属性
        public override object Clone()
        {
            CustomDGVButtonCell cell = (CustomDGVButtonCell)base.Clone();
            cell.Enabled = Enabled;
            return cell;
        }
        
        public override void InitializeEditingControl(int rowIndex, object initialFormattedValue,
           DataGridViewCellStyle dataGridViewCellStyle)
        {
            base.InitializeEditingControl(rowIndex, initialFormattedValue, dataGridViewCellStyle);
        }
        
        protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex,
            DataGridViewElementStates elementState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle,
            DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
        {
            // 按钮禁用时，绘制禁用样式
            if (!this.Enabled)
            {
                // 绘制单元格背景
                if ((paintParts & DataGridViewPaintParts.Background) == DataGridViewPaintParts.Background)
                {
                    SolidBrush cellBackground = new SolidBrush(cellStyle.BackColor);
                    graphics.FillRectangle(cellBackground, cellBounds);
                    cellBackground.Dispose();
                }

                // 绘制单元格边框
                if ((paintParts & DataGridViewPaintParts.Border) == DataGridViewPaintParts.Border)
                {
                    PaintBorder(graphics, clipBounds, cellBounds, cellStyle, advancedBorderStyle);
                }

                // 计算按钮区域
                Rectangle buttonArea = cellBounds;
                Rectangle buttonAdjustment = BorderWidths(advancedBorderStyle);
                buttonArea.X += buttonAdjustment.X;
                buttonArea.Y += buttonAdjustment.Y;
                buttonArea.Height -= buttonAdjustment.Height;
                buttonArea.Width -= buttonAdjustment.Width;

                // 绘制禁用按钮
                ButtonRenderer.DrawButton(graphics, buttonArea, PushButtonState.Disabled);

                // 绘制禁用按钮文本
                if (FormattedValue is string)
                {
                    TextRenderer.DrawText(graphics, (string)FormattedValue, DataGridView.Font, buttonArea,
                        SystemColors.GrayText);
                }
            }
            else
            {
                // 按钮启用时，使用基类的绘制方法
                base.Paint(graphics, clipBounds, cellBounds, rowIndex, elementState, value, formattedValue, errorText,
                    cellStyle, advancedBorderStyle, paintParts);
            }
        }
    }

    /// <summary>
    /// 自定义DataGridView按钮列 - 支持禁用状态
    /// 参考 F5BotV2 CustomDgvButtonColumn
    /// </summary>
    public class CustomDgvButtonColumn : DataGridViewButtonColumn
    {
        public CustomDgvButtonColumn()
        {
            CellTemplate = new CustomDGVButtonCell();
        }
    }
}

