using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LxLib.LxNet
{
    /// <summary>
    ///     httphelper的扩展
    /// </summary>
    public static class LxHttpHelperEx
    {
        public static bool  Base64ToImage(this LxHttpHelper http, string dataURL, string fileFullPath, ImageFormat imgformat)
        {
            try
            {
                //dataURL = dataURL.Replace("data:image/png;base64,", "").Replace("data:image/jgp;base64,", "").Replace("data:image/jpg;base64,", "").Replace("data:image/jpeg;base64,", "");//将base64头部信息替换
                //byte[] bytes = Convert.FromBase64String(dataURL);
                //MemoryStream memStream = new MemoryStream(bytes);
                //System.Drawing.Image mImage = System.Drawing.Image.FromStream(memStream);
                //mImage.Save(fileFullPath, ImageFormat.Png);

                //Bitmap bp = new Bitmap(mImage);

                //if (!Directory.Exists(fileFullPath))
                //{
                //    Directory.CreateDirectory(fileFullPath);
                //}

                //bp.Save(fileFullPath,  ImageFormat.Png);//保存到服务器路径
                //bp.Dispose();
                //return true;//返回相对路径 
                var image = LxHttpHelperEx.Base64ToImage(dataURL, imgformat);

                if (!Directory.Exists(fileFullPath))
                {
                    Directory.CreateDirectory(fileFullPath);
                }
                image.Save(fileFullPath, ImageFormat.Png);
                
                
                return true;

            }
            catch (Exception e)
            {
                
            }
            return false;
        }

        //静态函数.运行非HttpHelper的类也调用
        //dataurl = data:image/png;base64 开头的base64图片编码
        /// <summary>
        ///     
        /// </summary>
        /// <param name="dataURL"></param>
        /// <param name="fileFullPath"></param>
        /// <param name="imgformat"></param>
        /// <returns>成功返回image, 失败返回null</returns>
        public static System.Drawing.Image Base64ToImage(string dataURL, ImageFormat imgformat)
        {
            try
            {
                dataURL = dataURL.Replace("data:image/png;base64,", "").Replace("data:image/jgp;base64,", "").Replace("data:image/jpg;base64,", "").Replace("data:image/jpeg;base64,", "");//将base64头部信息替换
                byte[] bytes = Convert.FromBase64String(dataURL);
                MemoryStream memStream = new MemoryStream(bytes);
                System.Drawing.Image mImage = System.Drawing.Image.FromStream(memStream);
               
                //if (!Directory.Exists(fileFullPath))
                //{
                //    Directory.CreateDirectory(fileFullPath);
                //}
                //保存图片
                //mImage.Save(fileFullPath, ImageFormat.Png);

                //Bitmap bp = new Bitmap(mImage);
                //bp.Save(fileFullPath, ImageFormat.Png);//保存到服务器路径
                //bp.Dispose();
                return mImage;//返回相对路径 

            }
            catch (Exception e)
            {

            }
            return null;
        }


        public static Bitmap GetImage(this LxHttpHelper http, string imageUrl, int timeoutMM = 10000)
        {
            Bitmap bitmap = null;
            try
            {
                HttpWebRequest req;
                HttpWebResponse res = null;
                System.Uri httpUrl = new System.Uri(imageUrl);
                req = (HttpWebRequest)(WebRequest.Create(httpUrl));
                req.Timeout = timeoutMM; //设置超时值10秒
                req.Method = "GET";
                res = (HttpWebResponse)(req.GetResponse());
                Image image = Image.FromStream(res.GetResponseStream());
                bitmap = new Bitmap(image);
            }
            catch (System.Exception ex)
            {
                bitmap = null;
            }
            return bitmap;
        }
    }

}
