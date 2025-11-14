using System;
using System.Text;

namespace BaiShengVx3Plus.Utils
{
    /// <summary>
    /// 密码加密/解密工具类（简单的Base64编码）
    /// </summary>
    public static class PasswordHelper
    {
        // 简单的混淆密钥（防止明文存储）
        private const string Salt = "BaiSheng_Vx3Plus_2024";
        
        /// <summary>
        /// 加密密码（Base64 + 简单XOR）
        /// </summary>
        public static string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;
            
            try
            {
                // 1. 转为字节
                var plainBytes = Encoding.UTF8.GetBytes(plainText);
                var saltBytes = Encoding.UTF8.GetBytes(Salt);
                
                // 2. 简单XOR加密
                var encryptedBytes = new byte[plainBytes.Length];
                for (int i = 0; i < plainBytes.Length; i++)
                {
                    encryptedBytes[i] = (byte)(plainBytes[i] ^ saltBytes[i % saltBytes.Length]);
                }
                
                // 3. Base64编码
                return Convert.ToBase64String(encryptedBytes);
            }
            catch
            {
                return string.Empty;
            }
        }
        
        /// <summary>
        /// 解密密码
        /// </summary>
        public static string Decrypt(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText))
                return string.Empty;
            
            try
            {
                // 1. Base64解码
                var encryptedBytes = Convert.FromBase64String(encryptedText);
                var saltBytes = Encoding.UTF8.GetBytes(Salt);
                
                // 2. XOR解密
                var plainBytes = new byte[encryptedBytes.Length];
                for (int i = 0; i < encryptedBytes.Length; i++)
                {
                    plainBytes[i] = (byte)(encryptedBytes[i] ^ saltBytes[i % saltBytes.Length]);
                }
                
                // 3. 转为字符串
                return Encoding.UTF8.GetString(plainBytes);
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}

