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
        private Services.Games.Binggo.CreditWithdrawService? _creditWithdrawService;  // 🔥 上下分服务
        private V2CreditWithdrawBindingList? _creditWithdrawsBindingList;  // 🔥 上下分 BindingList

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
        /// 设置上下分服务（由外部设置）
        /// </summary>
        public void SetCreditWithdrawService(Services.Games.Binggo.CreditWithdrawService? service)
        {
            _creditWithdrawService = service;
        }
        
        /// <summary>
        /// 设置上下分 BindingList（由外部设置）
        /// </summary>
        public void SetCreditWithdrawsBindingList(V2CreditWithdrawBindingList? bindingList)
        {
            _creditWithdrawsBindingList = bindingList;
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
                // 🔥 1. 获取群成员列表（参考 F5BotV2 Line 2638：GetMemberList）
                // 使用 GetGroupContacts 命令，传入群ID作为参数
                var response = await _socketClient.SendAsync<dynamic>("GetGroupContacts", groupWxid);
                if (response == null)
                {
                    _logService.Warning("AdminCommand", "获取群成员列表失败：响应为空");
                    return (false, null);
                }

                // 🔥 GetGroupContacts 返回的是 JSON 数组，每个元素包含 member_wxid 字段
                // 参考 WeixinX/WeixinX/Features.cpp Line 737-915
                System.Collections.Generic.List<string> memberWxids = new System.Collections.Generic.List<string>();
                
                if (response is Newtonsoft.Json.Linq.JArray jArray)
                {
                    foreach (var item in jArray)
                    {
                        var memberWxid = item["member_wxid"]?.ToString();
                        if (!string.IsNullOrEmpty(memberWxid))
                        {
                            memberWxids.Add(memberWxid);
                        }
                    }
                }
                else if (response is System.Collections.IEnumerable enumerable)
                {
                    foreach (var item in enumerable)
                    {
                        var memberWxid = item?.GetType().GetProperty("member_wxid")?.GetValue(item)?.ToString();
                        if (!string.IsNullOrEmpty(memberWxid))
                        {
                            memberWxids.Add(memberWxid);
                        }
                    }
                }
                else
                {
                    _logService.Warning("AdminCommand", $"群成员列表格式不正确: {response.GetType().Name}");
                    return (false, null);
                }

                if (memberWxids.Count == 0)
                {
                    _logService.Warning("AdminCommand", "群成员列表为空");
                    return (false, null);
                }

                _logService.Info("AdminCommand", $"群成员总数: {memberWxids.Count}");

                // 3. 获取当前数据库中的会员列表
                if (_membersBindingList == null || _db == null)
                {
                    _logService.Warning("AdminCommand", "会员列表或数据库未初始化");
                    return (false, null);
                }
                
                var existingMembers = _membersBindingList.ToList();

                // 🔥 4. 检查每个成员是否已存在（参考 F5BotV2 Line 2645-2697）
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
                _logService.Error("AdminCommand", "❌ 会员列表未初始化！");
                _logService.Error("AdminCommand", $"   数据库状态: {(_db != null ? "已初始化" : "未初始化")}");
                _logService.Error("AdminCommand", $"   会员列表: null");
                _logService.Error("AdminCommand", "   请检查：1. 是否已绑定群 2. BindGroupAsync 是否成功执行");
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
                _logService.Error("AdminCommand", "❌ 会员列表未初始化！");
                _logService.Error("AdminCommand", $"   数据库状态: {(_db != null ? "已初始化" : "未初始化")}");
                _logService.Error("AdminCommand", $"   会员列表: null");
                _logService.Error("AdminCommand", "   请检查：1. 是否已绑定群 2. BindGroupAsync 是否成功执行");
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
        /// 执行上下分操作（参考 F5BotV2 Line 2759-2762, 2814-2817）
        /// 🔥 创建上下分记录并调用服务处理
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
                // 🔥 1. 创建上下分记录（参考 F5BotV2 Line 2759, 2814）
                CreditWithdrawAction payAction = action == "上" ? CreditWithdrawAction.上分 : CreditWithdrawAction.下分;
                long timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
                
                var creditWithdraw = new V2CreditWithdraw
                {
                    GroupWxId = groupWxid,
                    Wxid = member.Wxid,
                    Nickname = member.Nickname,
                    Account = member.Account,
                    Action = payAction,
                    Amount = money,
                    Status = CreditWithdrawStatus.等待处理,  // 🔥 初始状态为等待处理
                    TimeString = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Timestamp = timestamp,
                    Notes = note
                };
                
                // 🔥 2. 添加到 BindingList（会自动保存到数据库）
                if (_creditWithdrawsBindingList != null)
                {
                    _creditWithdrawsBindingList.Add(creditWithdraw);
                }
                else if (_db != null)
                {
                    // 如果没有 BindingList，直接插入数据库
                    _db.Insert(creditWithdraw);
                }
                
                // 🔥 3. 调用 CreditWithdrawService 处理（参考 F5BotV2 Line 2762, 2817）
                // 这会自动处理余额、更新状态、发送通知等
                if (_creditWithdrawService != null)
                {
                    var (success, errorMessage) = _creditWithdrawService.ProcessCreditWithdraw(
                        creditWithdraw,
                        member,
                        isLoading: false);
                    
                    if (!success)
                    {
                        _logService.Error("AdminCommand", $"处理上下分失败: {errorMessage}");
                        return false;
                    }
                    
                    _logService.Info("AdminCommand", $"管理{action}分成功: {member.Nickname} {action}{money}, 余额={member.Balance}");
                    return true;
                }
                else
                {
                    // 🔥 如果没有服务，直接处理（兼容旧逻辑）
                    if (action == "上")
                    {
                        member.Balance += money;
                        member.CreditToday += money;
                        creditWithdraw.Status = CreditWithdrawStatus.已同意;
                        creditWithdraw.ProcessedBy = Services.Api.BoterApi.GetInstance().User;
                        creditWithdraw.ProcessedTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        
                        if (_creditWithdrawsBindingList != null)
                        {
                            // BindingList 会自动保存
                        }
                        else if (_db != null)
                        {
                            _db.Update(creditWithdraw);
                        }
                        
                        _logService.Info("AdminCommand", $"管理上分成功: {member.Nickname} +{money}, 余额={member.Balance}");
                        return true;
                    }
                    else if (action == "下")
                    {
                        member.Balance -= money;
                        member.WithdrawToday += money;
                        creditWithdraw.Status = CreditWithdrawStatus.已同意;
                        creditWithdraw.ProcessedBy = Services.Api.BoterApi.GetInstance().User;
                        creditWithdraw.ProcessedTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        
                        if (_creditWithdrawsBindingList != null)
                        {
                            // BindingList 会自动保存
                        }
                        else if (_db != null)
                        {
                            _db.Update(creditWithdraw);
                        }
                        
                        _logService.Info("AdminCommand", $"管理下分成功: {member.Nickname} -{money}, 余额={member.Balance}");
                        return true;
                    }
                    else
                    {
                        throw new Exception("#无效动作!");
                    }
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

