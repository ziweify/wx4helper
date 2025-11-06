using System.Threading.Tasks;
using BaiShengVx3Plus.Contracts;
using BaiShengVx3Plus.Models.Api;

namespace BaiShengVx3Plus.Services.Api
{
    /// <summary>
    /// 白胜系统 WebAPI 服务实现
    /// </summary>
    public class BsWebApiService : IBsWebApiService
    {
        private readonly IBsWebApiClient _apiClient;
        private readonly Contracts.ILogService _logService;
        private BsApiUser? _currentUser;
        private bool _isAuthenticated;
        private string _lastError = string.Empty;
        
        public BsWebApiService(IBsWebApiClient apiClient, Contracts.ILogService logService)
        {
            _apiClient = apiClient;
            _logService = logService;
        }
        
        public bool IsAuthenticated => _isAuthenticated;
        public BsApiUser? CurrentUser => _currentUser;
        
        public async Task<bool> LoginAsync(string username, string password)
        {
            try
            {
                _logService.Info("BsWebApiService", $"开始登录: {username}");
                
                var response = await _apiClient.PostAsync<BsApiUser>("login", new { username, password });
                
                if (response.IsSuccess && response.Data != null)
                {
                    _currentUser = response.Data;
                    _isAuthenticated = true;
                    
                    // 设置 Sign 供后续请求使用
                    _apiClient.SetSign(_currentUser.Token);
                    
                    _logService.Info("BsWebApiService", 
                        $"登录成功: {username}, 有效期: {_currentUser.ValidUntil:yyyy-MM-dd HH:mm:ss}");
                    
                    return true;
                }
                
                _lastError = response.Msg;
                _logService.Warning("BsWebApiService", $"登录失败: {_lastError}");
                return false;
            }
            catch (System.Exception ex)
            {
                _lastError = $"登录异常: {ex.Message}";
                _logService.Error("BsWebApiService", _lastError, ex);
                return false;
            }
        }
        
        public void Logout()
        {
            _logService.Info("BsWebApiService", $"用户登出: {_currentUser?.Username}");
            _currentUser = null;
            _isAuthenticated = false;
            _apiClient.SetSign(string.Empty);
        }
        
        public string GetLastError() => _lastError;
        
        public async Task<bool> RefreshTokenAsync()
        {
            if (_currentUser == null || string.IsNullOrEmpty(_currentUser.Token))
            {
                _lastError = "当前未登录，无法刷新 Token";
                return false;
            }
            
            var response = await _apiClient.PostAsync<string>("refresh_token", new { token = _currentUser.Token });
            if (response.IsSuccess && !string.IsNullOrEmpty(response.Data))
            {
                _currentUser.Token = response.Data;
                _apiClient.SetSign(_currentUser.Token);
                _logService.Info("BsWebApiService", "Token 刷新成功");
                return true;
            }
            
            _lastError = response.Msg;
            _logService.Warning("BsWebApiService", $"Token 刷新失败: {_lastError}");
            return false;
        }
    }
}
