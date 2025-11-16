using zhaocaimao.Models;
using zhaocaimao.Contracts;

namespace zhaocaimao.Services.Auth
{
    /// <summary>
    /// InsUser数据服务实现
    /// </summary>
    public class InsUserService : IInsUserService
    {
        private readonly List<InsUser> _users = new();

        public InsUserService()
        {
            // 初始化示例数据
            InitializeSampleData();
        }

        private void InitializeSampleData()
        {
            _users.Add(new InsUser
            {
                Id = "111065741",
                UserName = "开发测试中",
                Account = "wwwww11",
                Password = "******",
                Balance = 2354.00m,
                IsVip = true,
                LastLoginTime = DateTime.Parse("2023-12-19 13:19:23"),
                CurrentTime = DateTime.Now,
                Seconds = 3000,
                IsOnline = true
            });
        }

        public Task<List<InsUser>> GetAllUsersAsync()
        {
            return Task.FromResult(_users.ToList());
        }

        public Task<bool> AddUserAsync(InsUser user)
        {
            user.Id = DateTime.Now.Ticks.ToString();
            _users.Add(user);
            return Task.FromResult(true);
        }

        public Task<bool> UpdateUserAsync(InsUser user)
        {
            var existingUser = _users.FirstOrDefault(u => u.Id == user.Id);
            if (existingUser != null)
            {
                var index = _users.IndexOf(existingUser);
                _users[index] = user;
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public Task<bool> DeleteUserAsync(string id)
        {
            var user = _users.FirstOrDefault(u => u.Id == id);
            if (user != null)
            {
                _users.Remove(user);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public Task<InsUser?> GetUserByIdAsync(string id)
        {
            var user = _users.FirstOrDefault(u => u.Id == id);
            return Task.FromResult(user);
        }
    }
}

