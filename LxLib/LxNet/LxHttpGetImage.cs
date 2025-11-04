using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LxLib.LxNet
{
    public class LxHttpGetImage
    {
        private int S_Height;
        private int S_Width;
        private int F_Height;
        private int F_Width;
        private string MyURL;

        public int ScreenHeight
        {
            get { return S_Height; }
            set { S_Height = value; }
        }

        public int ScreenWidth
        {
            get { return S_Width; }
            set { S_Width = value; }
        }

        public int ImageHeight
        {
            get { return F_Height; }
            set { F_Height = value; }
        }

        public int ImageWidth
        {
            get { return F_Width; }
            set { F_Width = value; }
        }

        public string WebSite
        {
            get { return MyURL; }
            set { MyURL = value; }
        }

        public LxHttpGetImage(string WebSite, int ScreenWidth, int ScreenHeight, int ImageWidth, int ImageHeight)
        {
            this.WebSite = WebSite;
            this.ScreenWidth = ScreenWidth;
            this.ScreenHeight = ScreenHeight;
            this.ImageHeight = ImageHeight;
            this.ImageWidth = ImageWidth;
        }

        public Bitmap GetBitmap()
        {
            WebPageBitmap Shot = new WebPageBitmap(this.WebSite, this.ScreenWidth, this.ScreenHeight);
            Shot.GetIt();
            Bitmap Pic = Shot.DrawBitmap(this.ImageHeight, this.ImageWidth);
            return Pic;
        }
    }

    class WebPageBitmap
    {
        WebBrowser MyBrowser;
        string URL;
        int Height;
        int Width;

        public WebPageBitmap(string url, int width, int height)
        {
            this.Height = height;
            this.Width = width;
            this.URL = url;
            MyBrowser = new WebBrowser();
            MyBrowser.ScrollBarsEnabled = false;
            MyBrowser.ScriptErrorsSuppressed = true;
            MyBrowser.Size = new Size(this.Width, this.Height);
        }

        public void GetIt()
        {
           MyBrowser.Navigate(this.URL);
           while (MyBrowser.ReadyState != WebBrowserReadyState.Complete)
           {
                Application.DoEvents();
            }
        }

        public Bitmap DrawBitmap(int theight, int twidth)
        {
            Bitmap myBitmap = new Bitmap(Width, Height);
            Rectangle DrawRect = new Rectangle(0, 0, Width, Height);
            MyBrowser.DrawToBitmap(myBitmap, DrawRect);
            System.Drawing.Image imgOutput = myBitmap;
            System.Drawing.Image oThumbNail = new Bitmap(twidth, theight, imgOutput.PixelFormat);
            Graphics g = Graphics.FromImage(oThumbNail);
            g.CompositingQuality = CompositingQuality.HighSpeed;
            g.SmoothingMode = SmoothingMode.HighSpeed;
            g.InterpolationMode = InterpolationMode.HighQualityBilinear;
            Rectangle oRectangle = new Rectangle(0, 0, twidth, theight);
            g.DrawImage(imgOutput, oRectangle);
            try
            {
                return (Bitmap)oThumbNail;
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                imgOutput.Dispose();
                imgOutput = null;
                MyBrowser.Dispose();
                MyBrowser = null;
            }
        }
    }
}
