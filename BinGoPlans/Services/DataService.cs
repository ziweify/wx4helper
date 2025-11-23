using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;
using BaiShengVx3Plus.Shared.Models.Games.Binggo;
using BaiShengVx3Plus.Shared.Services;

namespace BinGoPlans.Services
{
    /// <summary>
    /// 数据服务（数据入口）
    /// </summary>
    public class DataService
    {
        private readonly BinggoStatisticsService _statisticsService;
        private readonly ApiService _apiService;
        private string _dbPath;

        public DataService(BinggoStatisticsService statisticsService)
        {
            _statisticsService = statisticsService;
            _apiService = new ApiService();
        }

        /// <summary>
        /// 设置数据库路径
        /// </summary>
        public void SetDatabasePath(string dbPath)
        {
            _dbPath = dbPath;
        }

        /// <summary>
        /// 添加单条开奖数据（数据入口）
        /// </summary>
        public void AddLotteryData(int issueId, string lotteryData, DateTime openTime)
        {
            var data = new BinggoLotteryData();
            data.FillLotteryData(issueId, lotteryData, openTime);
            _statisticsService.AddData(data);
        }

        /// <summary>
        /// 批量添加开奖数据（数据入口）
        /// </summary>
        public void AddLotteryDataRange(IEnumerable<(int issueId, string lotteryData, DateTime openTime)> dataList)
        {
            foreach (var (issueId, lotteryData, openTime) in dataList)
            {
                AddLotteryData(issueId, lotteryData, openTime);
            }
        }

        /// <summary>
        /// 登录
        /// </summary>
        public async Task<bool> LoginAsync(string username, string password)
        {
            return await _apiService.LoginAsync(username, password);
        }

        /// <summary>
        /// 从API加载指定日期的数据
        /// </summary>
        public async Task LoadFromApiAsync(DateTime date)
        {
            try
            {
                var dataList = await _apiService.GetLotteryDataByDateAsync(date);
                _statisticsService.AddDataRange(dataList);
            }
            catch (Exception ex)
            {
                throw new Exception($"加载数据失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 从SQLite数据库加载数据
        /// </summary>
        public void LoadFromDatabase(string dbPath = null)
        {
            var path = dbPath ?? _dbPath;
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                return;

            try
            {
                using var connection = new SQLiteConnection($"Data Source={path};Version=3;");
                connection.Open();

                var command = new SQLiteCommand("SELECT IssueId, LotteryData, OpenTime FROM BinggoLotteryData ORDER BY OpenTime", connection);
                using var reader = command.ExecuteReader();

                var dataList = new List<BinggoLotteryData>();
                while (reader.Read())
                {
                    var issueId = reader.GetInt32(0);
                    var lotteryData = reader.GetString(1);
                    var openTimeStr = reader.GetString(2);

                    if (DateTime.TryParse(openTimeStr, out var openTime))
                    {
                        var data = new BinggoLotteryData();
                        data.FillLotteryData(issueId, lotteryData, openTime);
                        dataList.Add(data);
                    }
                }

                _statisticsService.AddDataRange(dataList);
            }
            catch (Exception ex)
            {
                throw new Exception($"加载数据库失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 获取统计服务实例
        /// </summary>
        public BinggoStatisticsService GetStatisticsService()
        {
            return _statisticsService;
        }
    }
}

