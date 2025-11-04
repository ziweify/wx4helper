using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace F5BotV2.Controls
{
    class ComboBoxEx : ComboBox
    {
        struct RECT
        {
            public int left, top, right, bottom;
        }
        struct COMBOBOXINFO
        {
            public int cbSize;
            public RECT rcItem;
            public RECT rcButton;
            public int stateButton;
            public IntPtr hwndCombo;
            public IntPtr hwndItem;
            public IntPtr hwndList;
        }
        [DllImport("user32")]
        private static extern int GetComboBoxInfo(IntPtr hwnd, out COMBOBOXINFO comboInfo);
        COMBOBOXINFO combo = new COMBOBOXINFO();
        private class NativeCombo : NativeWindow
        {
            public delegate void CallBack(int index);
            CallBack CallBackFun;
            public NativeCombo(CallBack callBack)
                : base()
            {
                CallBackFun = callBack;
            }
            public Dictionary<int, Rectangle> dict = new Dictionary<int, Rectangle>();
            //this is custom MouseDown event to hook into later
            protected override void WndProc(ref Message m)
            {
                if (m.Msg == 0x201)//WM_LBUTTONDOWN = 0x201
                {
                    int x = m.LParam.ToInt32() & 0x00ff;
                    int y = m.LParam.ToInt32() >> 16;
                    Point Location = new Point(x, y);
                    foreach (var kv in dict)
                    {
                        if (kv.Value.Contains(Location))
                        {
                            CallBackFun(kv.Key);
                            return;
                        }
                    }
                }
                base.WndProc(ref m);
            }
        }
        NativeCombo nativeCombo;
        //This is the MouseDown event handler to handle the clicked icon
        private void nativeComboCallBack(int index)
        {
            if (ListImgClick != null) ListImgClick(this, index);
            this.Focus();
        }
        public ComboBoxEx() : base()
        {
            this.HandleCreated += (s, e) =>
            {
                combo.cbSize = Marshal.SizeOf(combo);
                GetComboBoxInfo(this.Handle, out combo);
            };
            this.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
        }
        protected override void OnDropDown(EventArgs e)
        {
            base.OnDropDown(e);
            if (combo.hwndList != IntPtr.Zero && nativeCombo == null)
            {
                nativeCombo = new NativeCombo(nativeComboCallBack);
                nativeCombo.AssignHandle(combo.hwndList);
            }
        }
        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            base.OnDrawItem(e);
            if ((e.State & DrawItemState.Selected) != 0)//鼠标选中在这个项上
            {
                //渐变画刷
                LinearGradientBrush brush = new LinearGradientBrush(e.Bounds, Color.FromArgb(0, 120, 215),
                                                 Color.FromArgb(0, 120, 215), LinearGradientMode.Vertical);
                //填充区域
                Rectangle borderRect = new Rectangle(0, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height - 2);
                e.Graphics.FillRectangle(brush, borderRect);
                //画边框
                Pen pen = new Pen(Color.FromArgb(0, 120, 215));
                e.Graphics.DrawRectangle(pen, borderRect);
            }
            else
            {
                SolidBrush brush = new SolidBrush(Color.FromArgb(255, 255, 255));
                e.Graphics.FillRectangle(brush, e.Bounds);
            }
            //图片绘制的区域
            Rectangle imgRect = new Rectangle(e.Bounds.Width - 20, e.Bounds.Y, 20, e.Bounds.Height);
            nativeCombo.dict[e.Index] = imgRect;
            if (listImage != null && (e.State & DrawItemState.Selected) != 0)
            {
                e.Graphics.DrawImage(listImage, imgRect);
            }
            Rectangle textRect = new Rectangle(0, imgRect.Y, e.Bounds.Width - imgRect.Width, e.Bounds.Height + 2);
            string itemText = this.Items[e.Index].ToString();
            StringFormat strFormat = new StringFormat();
            strFormat.LineAlignment = StringAlignment.Center;
            e.Graphics.DrawString(itemText, e.Font, new SolidBrush(e.ForeColor), textRect, strFormat);
        }
        #region 自定义属性和事件
        private Image listImage;
        [Category("数据"), Description("子项图片"), Browsable(true)]
        public Image ListImage
        {
            get
            {
                return listImage;
            }
            set
            {
                listImage = value;
            }
        }
        //定义委托
        public delegate void ListImgClickHandle(object sender, int index);
        //定义事件
        public event ListImgClickHandle ListImgClick;


        //删除子项例子
        //string text = comboBoxEx1.Text;
        //if (comboBoxEx1.Items[index].ToString() == text)
        //{
        //    text = "";
        //}
        //comboBoxEx1.Items.RemoveAt(index);
        //comboBoxEx1.Text = text;
        #endregion  
    }

}
