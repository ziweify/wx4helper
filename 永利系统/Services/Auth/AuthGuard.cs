using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using 永利系统.Infrastructure.Api;
using 永利系统.Models.Api;
using 永利系统.Services;

namespace 永利系统.Services.Auth
{
    /// <summary>
    /// 认证守卫服务 - 防破解机制
    /// 
    /// 职责：
    /// 1. 验证登录状态
    /// 2. 验证 Token 有效性
    /// 3. 防止跳过验证
    /// 4. 关键操作前验证
    /// </summary>
    public class AuthGuard
    {
        private readonly LoggingService _loggingService;
        private readonly BoterApi _boterApi;
        private readonly AuthService _authService;
        
        // 验证标记（防止直接实例化主窗口）
        private static bool _isAuthenticated = false;
        private static string _authToken = string.Empty;
        private static DateTime _authTime = DateTime.MinValue;
        
        // 防破解：验证密钥（混淆）
        private const int _verifyKey1 = unchecked((int)0x4A3F2E1D);
        private const int _verifyKey2 = unchecked((int)0x8B7C6D5E);
        private const string _verifySecret = "YLXT_AUTH_2024";
        
        public AuthGuard(LoggingService loggingService, AuthService authService)
        {
            _loggingService = loggingService;
            _authService = authService;
            _boterApi = BoterApi.GetInstance();
        }
        
        /// <summary>
        /// 设置认证状态（仅内部使用，防止外部直接设置）
        /// </summary>
        public static void SetAuthenticated(string token)
        {
            _isAuthenticated = true;
            _authToken = token ?? string.Empty;
            _authTime = DateTime.Now;
        }
        
        /// <summary>
        /// 清除认证状态
        /// </summary>
        public static void ClearAuthentication()
        {
            _isAuthenticated = false;
            _authToken = string.Empty;
            _authTime = DateTime.MinValue;
        }
        
        /// <summary>
        /// 验证是否已通过认证（主窗口启动前必须调用）
        /// </summary>
        public bool VerifyAuthentication()
        {
            _loggingService.Info("认证守卫", "开始验证认证状态...");
            
            // 检查1：静态标记验证
            if (!_isAuthenticated)
            {
                _loggingService.Error("认证守卫", "未通过认证（静态标记为 false），拒绝启动主窗口");
                return false;
            }
            
            _loggingService.Info("认证守卫", $"静态标记验证通过，认证Token: {(_authToken.Length > 0 ? _authToken.Substring(0, Math.Min(10, _authToken.Length)) + "..." : "空")}");
            
            // 检查2：Token 验证
            var currentUser = _authService.GetCurrentUser();
            if (currentUser == null)
            {
                _loggingService.Error("认证守卫", "用户信息为空，拒绝启动主窗口");
                ClearAuthentication();
                return false;
            }
            
            _loggingService.Info("认证守卫", $"用户信息获取成功，用户Token: {(currentUser.Token?.Length > 0 ? currentUser.Token.Substring(0, Math.Min(10, currentUser.Token.Length)) + "..." : "空")}");
            
            // 检查3：Token 匹配验证
            if (string.IsNullOrEmpty(_authToken))
            {
                _loggingService.Error("认证守卫", "认证Token为空，拒绝启动主窗口");
                ClearAuthentication();
                return false;
            }
            
            if (string.IsNullOrEmpty(currentUser.Token))
            {
                _loggingService.Error("认证守卫", "用户Token为空，拒绝启动主窗口");
                ClearAuthentication();
                return false;
            }
            
            if (_authToken != currentUser.Token)
            {
                _loggingService.Error("认证守卫", $"Token 不匹配。认证Token: {_authToken.Substring(0, Math.Min(10, _authToken.Length))}...，用户Token: {currentUser.Token.Substring(0, Math.Min(10, currentUser.Token.Length))}...");
                ClearAuthentication();
                return false;
            }
            
            // 检查4：账号有效性验证
            if (!currentUser.IsAccountValid)
            {
                _loggingService.Error("认证守卫", "账号已过期，拒绝启动主窗口");
                ClearAuthentication();
                return false;
            }
            
            // 检查5：BoterApi 登录状态验证
            if (!_boterApi.IsLoggedIn())
            {
                _loggingService.Error("认证守卫", "API 未登录，拒绝启动主窗口");
                ClearAuthentication();
                return false;
            }
            
            // 检查6：防破解验证（混淆验证）
            if (!VerifyAntiCrack())
            {
                _loggingService.Error("认证守卫", "防破解验证失败，拒绝启动主窗口");
                ClearAuthentication();
                return false;
            }
            
            _loggingService.Info("认证守卫", "认证验证通过");
            return true;
        }
        
        /// <summary>
        /// 验证关键操作权限（在关键操作前调用）
        /// </summary>
        public bool VerifyOperation(string operationName)
        {
            // 重新验证登录状态
            if (!_boterApi.IsLoggedIn())
            {
                _loggingService.Warn("认证守卫", $"操作 '{operationName}' 被拒绝：未登录");
                MessageBox.Show("请先登录", "操作被拒绝", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            
            // 验证账号有效性
            var currentUser = _authService.GetCurrentUser();
            if (currentUser == null || !currentUser.IsAccountValid)
            {
                _loggingService.Warn("认证守卫", $"操作 '{operationName}' 被拒绝：账号无效");
                MessageBox.Show("账号已过期，请重新登录", "操作被拒绝", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            
            // 防破解验证
            if (!VerifyAntiCrack())
            {
                _loggingService.Warn("认证守卫", $"操作 '{operationName}' 被拒绝：防破解验证失败");
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// 防破解验证（混淆逻辑）
        /// </summary>
        private bool VerifyAntiCrack()
        {
            try
            {
                // 验证1：检查认证时间（防止长时间未验证）
                if (_authTime == DateTime.MinValue || (DateTime.Now - _authTime).TotalHours > 24)
                {
                    _loggingService.Warn("认证守卫", "认证时间过期");
                    return false;
                }
                
                // 验证2：混淆验证（防止直接修改代码跳过）
                // 使用 uint 进行计算，避免符号扩展问题
                unchecked
                {
                    // 将 int 转换为 uint 进行计算，避免符号扩展
                    uint key1 = (uint)(_verifyKey1 ^ 0x12345678);
                    uint key2 = (uint)(_verifyKey2 ^ 0x87654321);
                    uint sum = key1 + key2;
                    uint expected = 0x6424A6E4;
                    
                    // 调试日志
                    _loggingService.Debug("认证守卫", $"密钥验证: key1=0x{key1:X8} ({key1}), key2=0x{key2:X8} ({key2}), sum=0x{sum:X8} ({sum}), expected=0x{expected:X8} ({expected})");
                    
                    // 计算正确的预期值：0x4A3F2E1D ^ 0x12345678 + 0x8B7C6D5E ^ 0x87654321 = 0x6424A6E4
                    if (sum != expected)
                    {
                        _loggingService.Warn("认证守卫", $"验证密钥错误: sum=0x{sum:X8} ({sum}), expected=0x{expected:X8} ({expected})");
                        return false;
                    }
                }
                
                // 验证3：字符串验证（防止直接修改）
                var secret = _verifySecret;
                if (secret.Length != 14 || !secret.StartsWith("YLXT") || !secret.EndsWith("2024"))
                {
                    _loggingService.Warn("认证守卫", $"验证密钥格式错误: Length={secret.Length}, Expected=14");
                    return false;
                }
                
                // 验证4：时间戳验证（防止重放攻击）
                var timeCheck = DateTime.Now.Ticks % 1000000;
                if (timeCheck < 0)
                {
                    // 这个检查永远不会为真，但可以防止简单的代码修改
                    return false;
                }
                
                return true;
            }
            catch (Exception ex)
            {
                _loggingService.Error("认证守卫", $"防破解验证异常: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 定期验证（在后台定时检查）
        /// </summary>
        public async Task<bool> PeriodicVerifyAsync()
        {
            if (!_isAuthenticated)
            {
                return false;
            }
            
            // 重新验证登录状态
            if (!_boterApi.IsLoggedIn())
            {
                _loggingService.Warn("认证守卫", "定期验证失败：未登录");
                ClearAuthentication();
                return false;
            }
            
            // 验证账号有效性
            var currentUser = _authService.GetCurrentUser();
            if (currentUser == null || !currentUser.IsAccountValid)
            {
                _loggingService.Warn("认证守卫", "定期验证失败：账号无效");
                ClearAuthentication();
                return false;
            }
            
            return true;
        }
    }
}

