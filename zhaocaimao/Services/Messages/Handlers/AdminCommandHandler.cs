using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.ComponentModel;
using zhaocaimao.Contracts;
using zhaocaimao.Models;
using zhaocaimao.Core;
using SQLite;

namespace zhaocaimao.Services.Messages.Handlers
{
    /// <summary>
    /// 管理员命令处理器 - 参考 F5BotV2/Boter/BoterServices.cs Line 2613-2830
    /// </summary>
    public class AdminCommandHandler
    {
        private readonly ILogService _logService;
        private readonly IWeixinSocketClient _socketClient;
        private V2MemberBindingList? _membersBindingList;  // 🔥 直接使用 BindingList
        private SQLiteConnection? _db;  // 🔥 直接使用数据库连接

        public AdminCommandHandler(
            ILogService logService,
            IWeixinSocketClient socketClient)
        {
            _logService = logService;
            _socketClient = socketClient;
        }
        
        /// <summary>
        /// 设置会员 BindingList（由外部设置）
        /// </summary>
        public void SetMembersBindingList(V2MemberBindingList? bindingList)
        {
            _membersBindingList = bindingList;
        }
        
        /// <summary>
        /// 设置数据库连接（由外部设置）
        /// </summary>
        public void SetDatabase(SQLiteConnection? db)
        {
            _db = db;
        }

        /// <summary>
        /// 🔥 刷新命令 - 参考 F5BotV2 Line 2613-2708
        /// </summary>
        public async Task<(int code, string? replyMessage, string? errorMessage)> HandleRefreshCommand(
            string groupWxid,
            string message)
        {
            try
            {
                // 去除空格后检查
                if (message.Replace(" ", "") == "刷新")
                {
                    _logService.Info("AdminCommand", $"收到刷新命令: 群={groupWxid}");

                    // 调用刷新逻辑
                    var (success, welcomeMessages) = await RefreshGroupMembers(groupWxid);

                    if (success)
                    {
                        // 🔥 先发送欢迎消息（每个新成员一条）
                        if (welcomeMessages != null && welcomeMessages.Count > 0)
                        {
                            foreach (var welcomeMsg in welcomeMessages)
                            {
                                await _socketClient.SendAsync<object>("SendText", groupWxid, welcomeMsg);
                                await Task.Delay(100); // 避免消息发送过快
                            }
                        }

                        // 🔥 最后发送刷新完成
                        return (0, "^刷新完成", null);
                    }
                    else
                    {
                        return (-1, null, "刷新失败");
                    }
                }

                return (-1, null, null); // 不是刷新命令
            }
            catch (Exception ex)
            {
                _logService.Error("AdminCommand", "处理刷新命令失败", ex);
                return (-1, null, ex.Message);
            }
        }

        /// <summary>
        /// 🔥 管理上下分命令 - 参考 F5BotV2 Line 2711-2830
        /// </summary>
        public async Task<(int code, string? replyMessage, string? errorMessage)> HandleCreditWithdrawCommand(
            string groupWxid,
            string message)
        {
            try
            {
                // 🔥 格式1：@昵称 上/下金额 - F5BotV2 Line 2718
                string regexStr = @"@([^ ]+).(上|下){1}(\d+)(.*)";
                bool brgx = Regex.IsMatch(message, regexStr);

                if (brgx)
                {
                    return await HandleCreditWithdrawByNickname(groupWxid, message, regexStr);
                }

                // 🔥 格式2：ID上/下金额 - F5BotV2 Line 2785
                regexStr = @"(\d+)(上|下){1}(\d+)(.*)";
                brgx = Regex.IsMatch(message, regexStr);

                if (brgx)
                {
                    return await HandleCreditWithdrawById(groupWxid, message, regexStr);
                }

                return (-1, null, null); // 不是管理上下分命令
            }
            catch (Exception ex)
            {
                _logService.Error("AdminCommand", "处理管理上下分命令失败", ex);
                
                // 🔥 如果异常消息以 # 开头，说明是已处理的异常，去除 # 后直接输出
                if (ex.Message.StartsWith("#"))
                {
                    return (0, ex.Message.Substring(1), null);
                }
                
                return (-1, null, ex.Message);
            }
        }

        #region 私有方法

        /// <summary>
        /// 刷新群成员（添加新成员）
        /// </summary>
        private async Task<(bool success, System.Collections.Generic.List<string>? welcomeMessages)> RefreshGroupMembers(string groupWxid)
        {
            var welcomeMessages = new System.Collections.Generic.List<string>();

            try
            {
                // 1. 获取群成员列表
                var response = await _socketClient.SendAsync<dynamic>("GetChatRoomMembers", groupWxid);
                if (response == null || response.members == null)
                {
                    _logService.Warning("AdminCommand", "获取群成员列表失败");
                    return (false, null);
                }

                string membersStr = response.members.ToString();
                if (string.IsNullOrEmpty(membersStr))
                {
                    _logService.Warning("AdminCommand", "群成员列表为空");
                    return (false, null);
                }

                // 2. 解析成员列表（格式：wxid1^Gwxid2^Gwxid3）
                string[] memberWxids = membersStr.Replace("^G", "|").Split('|');
                _logService.Info("AdminCommand", $"群成员总数: {memberWxids.Length}");

                // 3. 获取当前数据库中的会员列表
                if (_membersBindingList == null || _db == null)
                {
                    _logService.Warning("AdminCommand", "会员列表或数据库未初始化");
                    return (false, null);
                }
                
                var existingMembers = _membersBindingList.ToList();

                // 4. 检查每个成员是否已存在
                foreach (var wxid in memberWxids)
                {
                    if (string.IsNullOrEmpty(wxid)) continue;

                    var existingMember = existingMembers.FirstOrDefault(m => m.Wxid == wxid);
                    if (existingMember == null)
                    {
                        // 🔥 新成员，添加到数据库
                        _logService.Info("AdminCommand", $"发现新成员: {wxid}");

                        // 获取昵称
                        var nickname = await GetMemberNickname(wxid);

                        // 创建新成员
                        var newMember = new V2Member
                        {
                            Wxid = wxid,
                            Nickname = nickname,
                            GroupWxId = groupWxid,
                            State = MemberState.非会员, // 默认非会员
                            Balance = 0,
                            BetToday = 0,
                            BetCur = 0,
                            BetWait = 0,
                            IncomeToday = 0,
                            CreditToday = 0,
                            WithdrawToday = 0,
                            Account = ""
                        };

                        // 添加到 BindingList（会自动同步到数据库）
                        _membersBindingList.Add(newMember);

                        // 重新获取（获得自增ID）
                        var addedMember = _membersBindingList.FirstOrDefault(m => m.Wxid == wxid);

                        // 🔥 生成欢迎消息 - 完全按照 F5BotV2 Line 2696 格式
                        string welcomeMsg = $"^欢迎:[{addedMember?.Id ?? 0}]{nickname}";
                        welcomeMessages.Add(welcomeMsg);

                        _logService.Info("AdminCommand", $"新成员已添加: ID={addedMember?.Id}, 昵称={nickname}");
                    }
                }

                return (true, welcomeMessages);
            }
            catch (Exception ex)
            {
                _logService.Error("AdminCommand", "刷新群成员失败", ex);
                return (false, null);
            }
        }

        /// <summary>
        /// 获取成员昵称
        /// </summary>
        private async Task<string> GetMemberNickname(string wxid)
        {
            try
            {
                var response = await _socketClient.SendAsync<dynamic>("GetContactProfile", wxid);
                if (response != null && response.nickname != null)
                {
                    return response.nickname.ToString();
                }
            }
            catch (Exception ex)
            {
                _logService.Warning("AdminCommand", $"获取昵称失败: {wxid}, {ex.Message}");
            }

            return "未知";
        }

        /// <summary>
        /// 🔥 处理格式1：@昵称 上/下金额 - F5BotV2 Line 2718-2782
        /// </summary>
        private async Task<(int code, string? replyMessage, string? errorMessage)> HandleCreditWithdrawByNickname(
            string groupWxid,
            string message,
            string regexStr)
        {
            var match = Regex.Match(message, regexStr);
            string s1 = match.Groups[1].Value; // 名字
            string s2 = match.Groups[2].Value; // 动作:上|下
            string s3 = match.Groups[3].Value; // 金额
            string s4 = match.Groups[4].Value; // 出错的字符

            _logService.Info("AdminCommand", $"解析管理上下分命令: 昵称={s1}, 动作={s2}, 金额={s3}, 后缀={s4}");

            // 🔥 特殊判断：如果s4包含"留"或"余"，不处理（避免与结算消息冲突）
            // F5BotV2 Line 2729-2732
            if (s4.Contains("留") || s4.Contains("余"))
            {
                _logService.Info("AdminCommand", "后缀包含'留'或'余'，不处理此命令");
                return (0, null, null);
            }

            // 根据昵称查找会员
            if (_membersBindingList == null)
            {
                throw new Exception("#[警告]系统未初始化");
            }
            
            var matchedMembers = _membersBindingList.Where(m => m.Nickname == s1).ToList();

            if (matchedMembers == null || matchedMembers.Count == 0)
            {
                // F5BotV2 Line 2739
                throw new Exception($"#[警告]没找到,{s1}");
            }

            if (matchedMembers.Count > 1)
            {
                // F5BotV2 Line 2744
                throw new Exception($"#[警告]重名,{s1}");
            }

            // 解析金额
            int money = 0;
            try
            {
                money = Convert.ToInt32(s3);
            }
            catch
            {
                // F5BotV2 Line 2753
                throw new Exception("#[警告]金额错误");
            }

            var member = matchedMembers[0];

            // 执行上下分
            bool success = await ExecuteCreditWithdraw(groupWxid, member, s2, money, $"管理直{s2}:{s1}");

            if (success)
            {
                // 🔥 回复格式 - 完全按照 F5BotV2 Line 2780
                string replyMsg = $"@{member.Nickname}\r{member.Id}{s2}{money}|余:{member.Balance}";
                return (0, replyMsg, null);
            }
            else
            {
                return (-1, null, "操作失败");
            }
        }

        /// <summary>
        /// 🔥 处理格式2：ID上/下金额 - F5BotV2 Line 2785-2830
        /// </summary>
        private async Task<(int code, string? replyMessage, string? errorMessage)> HandleCreditWithdrawById(
            string groupWxid,
            string message,
            string regexStr)
        {
            var match = Regex.Match(message, regexStr);
            string s1 = match.Groups[1].Value; // ID
            string s2 = match.Groups[2].Value; // 动作:上|下
            string s3 = match.Groups[3].Value; // 金额
            string s4 = match.Groups[4].Value; // 出错的字符

            _logService.Info("AdminCommand", $"解析管理上下分命令: ID={s1}, 动作={s2}, 金额={s3}, 后缀={s4}");

            // 根据ID查找会员
            if (_membersBindingList == null)
            {
                throw new Exception("#[警告]系统未初始化");
            }
            
            int id = 0;
            try
            {
                id = Convert.ToInt32(s1);
            }
            catch
            {
                throw new Exception("#[警告]ID格式错误");
            }

            var member = _membersBindingList.FirstOrDefault(m => m.Id == id);

            if (member == null)
            {
                // F5BotV2 Line 2800
                throw new Exception("#[警告]ID错误, ID不存在");
            }

            // 解析金额
            int money = 0;
            try
            {
                money = Convert.ToInt32(s3);
            }
            catch
            {
                // F5BotV2 Line 2809
                throw new Exception("#[警告]金额错误");
            }

            // 执行上下分
            bool success = await ExecuteCreditWithdraw(groupWxid, member, s2, money, $"管理直{s2}:{id}");

            if (success)
            {
                // 🔥 回复格式 - 完全按照 F5BotV2 Line 2831
                string replyMsg = $"@{member.Nickname}\r{member.Id}{s2}{money}|余:{member.Balance}";
                return (0, replyMsg, null);
            }
            else
            {
                return (-1, null, "操作失败");
            }
        }

        /// <summary>
        /// 执行上下分操作
        /// </summary>
        private async Task<bool> ExecuteCreditWithdraw(
            string groupWxid,
            V2Member member,
            string action,
            int money,
            string note)
        {
            try
            {
                if (action == "上")
                {
                    // 上分：直接增加余额
                    member.Balance += money;
                    member.CreditToday += money;
                    
                    // BindingList 会自动同步到数据库，无需手动调用

                    _logService.Info("AdminCommand", $"管理上分成功: {member.Nickname} +{money}, 余额={member.Balance}");
                    return true;
                }
                else if (action == "下")
                {
                    // 下分：减少余额（不检查余额，管理员下分可以为负）
                    member.Balance -= money;
                    member.WithdrawToday += money;
                    
                    // BindingList 会自动同步到数据库，无需手动调用

                    _logService.Info("AdminCommand", $"管理下分成功: {member.Nickname} -{money}, 余额={member.Balance}");
                    return true;
                }
                else
                {
                    // F5BotV2 Line 2777
                    throw new Exception("#无效动作!");
                }
            }
            catch (Exception ex)
            {
                _logService.Error("AdminCommand", $"执行上下分失败: {action}{money}", ex);
                throw;
            }
        }

        #endregion
    }
}

