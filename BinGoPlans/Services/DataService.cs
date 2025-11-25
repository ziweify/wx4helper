using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BaiShengVx3Plus.Shared.Models.Games.Binggo;
using BaiShengVx3Plus.Shared.Services;
using BinGoPlans.Models;

namespace BinGoPlans.Services
{
    /// <summary>
    /// 数据服务（数据入口）
    /// 实现：默认从 SQLite 加载，如果没有再从网络请求获取，获取后保存到数据库
    /// 直接使用 BinGoDataEntity（继承自 BinGoData），避免不必要的转换
    /// </summary>
    public class DataService
    {
        private readonly BinggoStatisticsService _statisticsService;
        private readonly ApiService _apiService;
        private readonly DatabaseService _databaseService;
        private string _dbPath;

        // 当前加载的数据列表（BinGoDataEntity，可以直接用于显示和计算）
        private List<BinGoDataEntity> _currentDataList = new List<BinGoDataEntity>();

        public DataService(BinggoStatisticsService statisticsService)
        {
            _statisticsService = statisticsService;
            _apiService = new ApiService();
            
            // 默认数据库路径
            var dataDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "BinGoPlans",
                "Data");
            Directory.CreateDirectory(dataDirectory);
            _dbPath = Path.Combine(dataDirectory, "binggo_data.db");
            
            // 初始化数据库服务
            _databaseService = new DatabaseService(_dbPath);
        }

        /// <summary>
        /// 设置数据库路径
        /// </summary>
        public void SetDatabasePath(string dbPath)
        {
            _dbPath = dbPath;
        }

        /// <summary>
        /// 添加单条开奖数据（数据入口，添加即保存）
        /// 使用 BinGoDataEntity，可以直接用于显示和保存
        /// </summary>
        public void AddLotteryData(int issueId, string lotteryData, DateTime openTime)
        {
            var data = new BinGoDataEntity(issueId, lotteryData, openTime);
            if (data.IsOpened)
            {
                // 添加到统计服务
                _statisticsService.AddData(ConvertToBinggoLotteryData(data));
                
                // 保存到数据库（BinGoDataEntity 继承自 BinGoData，可以直接保存）
                _databaseService.SaveData(data);
                
                // 添加到当前数据列表
                _currentDataList.Add(data);
            }
        }

        /// <summary>
        /// 批量添加开奖数据（数据入口，添加即保存）
        /// 使用 BinGoDataEntity，可以直接用于显示和保存
        /// </summary>
        public void AddLotteryDataRange(IEnumerable<(int issueId, string lotteryData, DateTime openTime)> dataList)
        {
            var binGoDataEntityList = new List<BinGoDataEntity>();
            var binggoLotteryDataList = new List<BinggoLotteryData>();

            foreach (var (issueId, lotteryData, openTime) in dataList)
            {
                var binGoDataEntity = new BinGoDataEntity(issueId, lotteryData, openTime);
                if (binGoDataEntity.IsOpened)
                {
                    binGoDataEntityList.Add(binGoDataEntity);
                    binggoLotteryDataList.Add(ConvertToBinggoLotteryData(binGoDataEntity));
                }
            }

            // 添加到统计服务
            _statisticsService.AddDataRange(binggoLotteryDataList);

            // 批量保存到数据库（BinGoDataEntity 继承自 BinGoData，可以直接保存）
            _databaseService.SaveDataRange(binGoDataEntityList);
            
            // 添加到当前数据列表
            _currentDataList.AddRange(binGoDataEntityList);
        }

        /// <summary>
        /// 登录
        /// </summary>
        public async Task<bool> LoginAsync(string username, string password)
        {
            return await _apiService.LoginAsync(username, password);
        }

        /// <summary>
        /// 加载指定日期的数据
        /// 1. 先从 SQLite 加载
        /// 2. 如果没有，再从网络请求获取
        /// 3. 获取后保存到数据库
        /// </summary>
        public async Task LoadDataByDateAsync(DateTime date)
        {
            try
            {
                // 🔥 步骤1: 先从 SQLite 加载
                var dbDataList = _databaseService.LoadDataByDate(date);
                
                if (dbDataList != null && dbDataList.Count > 0)
                {
                    // 数据库中有数据，直接使用（BinGoDataEntity 继承自 BinGoData）
                    _currentDataList = dbDataList;
                    
                    // 添加到统计服务（转换为 BinggoLotteryData）
                    var binggoLotteryDataList = dbDataList.Select(ConvertToBinggoLotteryData).ToList();
                    _statisticsService.AddDataRange(binggoLotteryDataList);
                    return;
                }

                // 🔥 步骤2: 数据库中没有数据，从网络请求获取
                var apiDataList = await _apiService.GetLotteryDataByDateAsync(date);
                
                if (apiDataList != null && apiDataList.Count > 0)
                {
                    // 🔥 步骤3: 转换为 BinGoDataEntity 并保存到数据库
                    var binGoDataEntityList = new List<BinGoDataEntity>();
                    var binggoLotteryDataList = new List<BinggoLotteryData>();

                    foreach (var apiData in apiDataList)
                    {
                        var binGoDataEntity = new BinGoDataEntity(
                            apiData.IssueId,
                            apiData.LotteryData,
                            apiData.OpenTime);
                        
                        if (binGoDataEntity.IsOpened)
                        {
                            binGoDataEntityList.Add(binGoDataEntity);
                            binggoLotteryDataList.Add(apiData);
                        }
                    }

                    // 保存到当前数据列表（BinGoDataEntity，可以直接用于显示）
                    _currentDataList = binGoDataEntityList;

                    // 添加到统计服务
                    _statisticsService.AddDataRange(binggoLotteryDataList);

                    // 保存到数据库（BinGoDataEntity 继承自 BinGoData，可以直接保存）
                    _databaseService.SaveDataRange(binGoDataEntityList);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"加载数据失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 从SQLite数据库加载所有数据
        /// </summary>
        public void LoadAllFromDatabase()
        {
            try
            {
                var dbDataList = _databaseService.LoadAllData();
                
                // 保存到当前数据列表（BinGoDataEntity，可以直接用于显示）
                _currentDataList = dbDataList;
                
                // 添加到统计服务（转换为 BinggoLotteryData）
                var binggoLotteryDataList = dbDataList.Select(ConvertToBinggoLotteryData).ToList();
                _statisticsService.AddDataRange(binggoLotteryDataList);
            }
            catch (Exception ex)
            {
                throw new Exception($"加载数据库失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 获取当前加载的数据列表（BinGoDataEntity，可以直接用于显示和计算）
        /// </summary>
        public List<BinGoDataEntity> GetCurrentDataList()
        {
            return _currentDataList;
        }

        /// <summary>
        /// 检查指定日期是否有数据
        /// </summary>
        public bool HasDataForDate(DateTime date)
        {
            return _databaseService.HasDataForDate(date);
        }

        /// <summary>
        /// 将 BinGoData 转换为 BinggoLotteryData（用于统计服务）
        /// </summary>
        private BinggoLotteryData ConvertToBinggoLotteryData(BinGoData binGoData)
        {
            var data = new BinggoLotteryData();
            data.FillLotteryData(binGoData.IssueId, binGoData.LotteryData, binGoData.OpenTime);
            return data;
        }

        /// <summary>
        /// 将 BinggoLotteryData 转换为 BinGoData
        /// </summary>
        private BinGoData ConvertToBinGoData(BinggoLotteryData binggoLotteryData)
        {
            return new BinGoData(
                binggoLotteryData.IssueId,
                binggoLotteryData.LotteryData,
                binggoLotteryData.OpenTime);
        }

        /// <summary>
        /// 获取统计服务实例
        /// </summary>
        public BinggoStatisticsService GetStatisticsService()
        {
            return _statisticsService;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            _databaseService?.Dispose();
        }
    }
}
