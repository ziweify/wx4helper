using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaiShengVx3Plus.Models.Games.Binggo;
using BaiShengVx3Plus.Services.Api;
using SharedBinggoLotteryData = BaiShengVx3Plus.Shared.Models.Games.Binggo.BinggoLotteryData;

namespace BinGoPlans.Services
{
    /// <summary>
    /// API服务（封装BaiShengVx3Plus的API调用）
    /// </summary>
    public class ApiService
    {
        private BoterApi _boterApi;
        private bool _isLoggedIn = false;

        public ApiService()
        {
            _boterApi = BoterApi.GetInstance();
        }

        /// <summary>
        /// 登录
        /// </summary>
        public async Task<bool> LoginAsync(string username, string password)
        {
            try
            {
                var response = await _boterApi.LoginAsync(username, password);
                _isLoggedIn = response != null && response.Code == 0;
                return _isLoggedIn;
            }
            catch (Exception ex)
            {
                throw new Exception($"登录失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 检查是否已登录
        /// </summary>
        public bool IsLoggedIn()
        {
            return _isLoggedIn && _boterApi.LoginApiResponse != null && _boterApi.LoginApiResponse.Code == 0;
        }

        /// <summary>
        /// 获取指定日期的开奖数据
        /// </summary>
        public async Task<List<SharedBinggoLotteryData>> GetLotteryDataByDateAsync(DateTime date)
        {
            if (!IsLoggedIn())
            {
                throw new Exception("请先登录");
            }

            try
            {
                string dateStr = date.ToString("yyyy-MM-dd");
                var response = await _boterApi.GetBgDayAsync(dateStr, 203, false);

                if (response.Code == 0 && response.Data != null)
                {
                    // 转换为Shared项目的数据模型
                    var result = new List<SharedBinggoLotteryData>();
                    foreach (var item in response.Data)
                    {
                        var data = new SharedBinggoLotteryData();
                        // BaiShengVx3Plus的BinggoLotteryData使用string类型的OpenTime
                        DateTime openTime;
                        string openTimeStr = item.OpenTime ?? "";
                        if (DateTime.TryParse(openTimeStr, out openTime))
                        {
                            data.FillLotteryData(item.IssueId, item.LotteryData, openTime);
                        }
                        else
                        {
                            // 如果解析失败，使用当前时间
                            data.FillLotteryData(item.IssueId, item.LotteryData, DateTime.Now);
                        }
                        result.Add(data);
                    }
                    return result;
                }
                else
                {
                    throw new Exception($"获取数据失败: {response.Msg}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"获取 {date:yyyy-MM-dd} 数据失败: {ex.Message}", ex);
            }
        }
    }
}

