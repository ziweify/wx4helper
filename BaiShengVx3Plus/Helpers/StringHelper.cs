using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BaiShengVx3Plus.Helpers
{
    public static class StringHelper
    {
        public static string UnEscape(this string str)
        {
            if(string.IsNullOrEmpty(str)) return str;

            string response = str;

            // 🔥 1. 移除控制字符（换行、制表符等）
            response = response.Replace("\x001E", "")  // Record Separator
                .Replace("\x07", "")                      // Bell
                .Replace("\n", "")                        // Line Feed
                .Replace("\a", "")                        // Alert
                .Replace("\b", "")                        // Backspace
                .Replace("\r", "")                        // Carriage Return
                .Replace("\f", "")                        // Form Feed
                .Replace("\t", "");                       // Tab

            // 🔥 2. 移除零宽字符和隐藏空格（这些字符会导致排版问题和输入不换行）
            response = response.Replace("\u200B", "")  // 零宽空格
                .Replace("\u200C", "")                  // 零宽非连接符
                .Replace("\u200D", "")                  // 零宽连接符
                .Replace("\uFEFF", "")                  // 零宽不换行空格（BOM）
                .Replace("\u2060", "")                  // 词连接符
                .Replace("\u200E", "")                  // 左到右标记
                .Replace("\u200F", "")                  // 右到左标记
                .Replace("\u202A", "")                  // 左到右嵌入
                .Replace("\u202B", "")                  // 右到左嵌入
                .Replace("\u202C", "")                  // Pop方向格式
                .Replace("\u202D", "")                  // 左到右覆盖
                .Replace("\u202E", "");                 // 右到左覆盖

            // 🔥 3. 移除所有特殊空格（包括非断行空格），替换为普通空格
            response = Regex.Replace(response, @"[\u00A0\u1680\u180E\u2000-\u200A\u202F\u205F\u3000]", " ", RegexOptions.Compiled);

            // 🔥 4. 移除上标字符（可能导致不换行问题）
            // 上标数字：¹²³⁴⁵⁶⁷⁸⁹⁰
            response = response.Replace("¹", "1")      // 上标1 (U+00B9)
                .Replace("²", "2")                      // 上标2 (U+00B2)
                .Replace("³", "3")                      // 上标3 (U+00B3)
                .Replace("⁴", "4")                      // 上标4 (U+2074)
                .Replace("⁵", "5")                      // 上标5 (U+2075)
                .Replace("⁶", "6")                      // 上标6 (U+2076)
                .Replace("⁷", "7")                      // 上标7 (U+2077)
                .Replace("⁸", "8")                      // 上标8 (U+2078)
                .Replace("⁹", "9")                      // 上标9 (U+2079)
                .Replace("⁰", "0");                     // 上标0 (U+2070)

            // 🔥 5. 移除日文平假名 あ（U+3042）- 这个字符会导致微信消息不换行
            response = response.Replace("あ", "?");
            response = response.Replace("@", "?");

            // 🔥 6. 移除藏文符号 ࿐（U+0FD0）- 可能导致不换行问题
            response = response.Replace("\u0FD0", "?");

            // 🔥 6. 移除所有其他控制字符和格式字符（使用正则表达式）
            // 只移除控制字符和格式字符，保留所有可见字符
            response = Regex.Replace(response, @"[\p{Cc}\p{Cf}]", "", RegexOptions.Compiled);
            // \p{Cc} - 控制字符
            // \p{Cf} - 格式字符

            // 🔥 6. 清理多余空格
            response = Regex.Replace(response, @"\s+", " ", RegexOptions.Compiled);  // 多个空格合并为一个
            response = response.Trim();  // 移除首尾空格

            return response;
        }
    }
}
