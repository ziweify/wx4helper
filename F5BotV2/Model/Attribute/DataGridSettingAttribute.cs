using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F5BotV2.Model.Attribute
{
    public class DataGridSettingAttribute
        : System.Attribute
    {
        public string DisplayName { get; set; } //显示的名字,一般是列头
        public bool Visable { get; set; }   //是否可见
        public int Width { get; set; }      //宽度

        public DataGridSettingAttribute(string displayName, bool visable, int width)
        {
            DisplayName = displayName;
            Visable = visable;
            Width = width;
        }
    }

    //定义了上面属性后, 可以在DataGrid初始化的时候一句话自动设置界面样式
    /**
     *                
                var t = typeof(BgLotteryView);
                foreach (PropertyInfo pi in t.GetProperties())
                {
                    string Name = pi.Name;
                    string DisplayName = pi.GetCustomAttribute<DataGridSettingAttribute>()?.DisplayName;
                    bool? Visable = pi.GetCustomAttribute<DataGridSettingAttribute>()?.Visable;
                    int? Width = pi.GetCustomAttribute<DataGridSettingAttribute>()?.Width;

                    var cell = p.Columns[Name];
                    if (cell != null)
                    {
                        if (!string.IsNullOrEmpty(DisplayName))
                            cell.HeaderText = DisplayName;
                        if (Visable != null)
                            cell.Visible = Visable.Value;
                        if (Width != null)
                            cell.Width = Width.Value;
                    }
                    //Console.WriteLine("属性名称：" + propertyName + "；显示名称：" + displayName + "；显示宽度：" + displayWidth);
                }
     */
}
