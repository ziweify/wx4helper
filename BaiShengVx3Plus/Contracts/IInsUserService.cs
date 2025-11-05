using BaiShengVx3Plus.Models;

namespace BaiShengVx3Plus.Contracts
{
    /// <summary>
    /// InsUser数据服务接口
    /// </summary>
    public interface IInsUserService
    {
        /// <summary>
        /// 获取所有用户
        /// </summary>
        Task<List<InsUser>> GetAllUsersAsync();

        /// <summary>
        /// 添加用户
        /// </summary>
        Task<bool> AddUserAsync(InsUser user);

        /// <summary>
        /// 更新用户
        /// </summary>
        Task<bool> UpdateUserAsync(InsUser user);

        /// <summary>
        /// 删除用户
        /// </summary>
        Task<bool> DeleteUserAsync(string id);

        /// <summary>
        /// 获取用户详情
        /// </summary>
        Task<InsUser?> GetUserByIdAsync(string id);
    }
}

