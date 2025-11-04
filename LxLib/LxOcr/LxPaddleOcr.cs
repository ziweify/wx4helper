using PaddleOCRSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LxLib.LxOcr
{
    public class LxPaddleOcr
    {
        PaddleOCREngine engine;
        OCRModelConfig config = null;
        OCRParameter oCRParameter = new OCRParameter();    //OCR参数




        public LxPaddleOcr()
        {
            //使用默认配置
            oCRParameter.cpu_math_library_num_threads = 6;      //预测并发线程数
            oCRParameter.enable_mkldnn = true;              //是否使用mkldnn模型
            oCRParameter.cls = false;                       //是否执行文字方向分类
            oCRParameter.use_angle_cls = false;             //是否开启方向检测
            oCRParameter.det_db_score_mode = true;          //是否使用多段线，即文字区域是用多段线还是用矩形，
            oCRParameter.det_db_unclip_ratio = 1.6f;
            oCRParameter.max_side_len = 2000;

            //初始化OCR引擎
            engine = new PaddleOCREngine(config, oCRParameter);

        }

        public OCRResult GetText(string imageFullePath)
        {
            //var imagebyte = File.ReadAllBytes("c:\\1.png");
            var imagebyte = File.ReadAllBytes(imageFullePath);
            OCRResult ocrResult = engine.DetectText(imagebyte);
            return ocrResult;
        }

        public OCRResult GetText(Image image)
        {
            OCRResult ocrResult = engine.DetectText(image);
            return ocrResult;
        }

    }
}
