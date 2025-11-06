using System.Collections.Generic;
using System.Threading.Tasks;
using BaiShengVx3Plus.Models.Api;

namespace BaiShengVx3Plus.Contracts
{
    /// <summary>
    /// 白胜系统 WebAPI HTTP 客户端接口
    /// 
    /// 职责：封装 HTTP 请求（GET/POST）
    /// 
    /// 设计理念：
    /// - 这个接口只提供基础的 HTTP 通信能力
    /// - 具体的业务逻辑（登录、获取开奖数据等）由 IBsWebApiService 实现
    /// - 这样设计更灵活，易于扩展和维护
    /// </summary>
    public interface IBsWebApiClient
    {
        /// <summary>
        /// 设置 API 根地址
        /// </summary>
        /// <param name="baseUrl">API 基础 URL，例如: http://8.134.71.102:789</param>
        void SetBaseUrl(string baseUrl);
        
        /// <summary>
        /// 设置认证签名
        /// </summary>
        /// <param name="sign">登录后获取的签名</param>
        void SetSign(string sign);
        
        /// <summary>
        /// 发送 GET 请求
        /// </summary>
        /// <typeparam name="T">响应数据类型</typeparam>
        /// <param name="endpoint">API 端点，例如: "login"</param>
        /// <param name="parameters">查询参数</param>
        /// <returns>API 响应</returns>
        Task<BsApiResponse<T>> GetAsync<T>(string endpoint, Dictionary<string, string>? parameters = null);
        
        /// <summary>
        /// 发送 POST 请求
        /// </summary>
        /// <typeparam name="T">响应数据类型</typeparam>
        /// <param name="endpoint">API 端点</param>
        /// <param name="data">请求体数据</param>
        /// <returns>API 响应</returns>
        Task<BsApiResponse<T>> PostAsync<T>(string endpoint, object? data = null);
        
        // ========================================
        // 🎲 炳狗游戏专用 API（基于 GetAsync/PostAsync 封装）
        // ========================================
        
        /// <summary>
        /// 获取当前期炳狗数据
        /// </summary>
        Task<BsApiResponse<T>> GetCurrentBinggoDataAsync<T>();
        
        /// <summary>
        /// 获取指定期号的炳狗数据
        /// </summary>
        Task<BsApiResponse<T>> GetBinggoDataAsync<T>(int issueId);
        
        /// <summary>
        /// 获取最近 N 期炳狗数据
        /// </summary>
        Task<BsApiResponse<T>> GetRecentBinggoDataAsync<T>(int count = 10);
        
        /// <summary>
        /// 获取指定日期的所有炳狗数据
        /// </summary>
        Task<BsApiResponse<T>> GetBinggoDataListAsync<T>(System.DateTime date);
    }
}
