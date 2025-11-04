using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F5BotV2.Ext
{
    public static class StringExt
    {
        public static string ToUnicodeCodes(this string input)
        {
            //return string.Join(" ", str.Select(c => $"U+{(int)c:X4}"));
            StringBuilder unicodeBuilder = new StringBuilder();

            foreach (char c in input)
            {
                unicodeBuilder.AppendFormat("\\u{0:x4}", (int)c);
            }

            string unicodeString = unicodeBuilder.ToString();
            return unicodeString;
        }

        public static string UnicodeCodesToString(this string input)
        {
            //string unicodeCodes = "\\u4e2d\\u56fd"; // 注意这里的反斜杠需要被转义，否则它会尝试解释为转义字符的开始
            string[] codes = input.Split('\\'); // 拆分字符串以获取单个Unicode代码块
            StringBuilder sb = new StringBuilder();
            foreach (var code in codes)
            {
                if (code.StartsWith("u")) // 检查是否为有效的Unicode代码块
                {
                    char ch = (char)int.Parse(code.Substring(1), NumberStyles.HexNumber); // 转换为char
                    sb.Append(ch); // 添加到StringBuilder中
                }
            }
            string unicodeString = sb.ToString(); // 转换为字符串
            return unicodeString;
        }
    }
}
