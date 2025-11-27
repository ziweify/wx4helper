using System;
using System.Drawing;
using System.Windows.Forms;
using BaiShengVx3Plus.Models;
using BaiShengVx3Plus.Services.Messages.Handlers;
using Sunny.UI;

namespace BaiShengVx3Plus
{
    /// <summary>
    /// VxMain 的部分类 - 开发模式菜单
    /// </summary>
    public partial class VxMain
    {
        private ToolStripMenuItem? _devOptionsMenuItem;
        private ToolStripSeparator? _separatorBeforeDevOptions;
        
        /// <summary>
        /// 初始化会员表右键菜单的开发选项
        /// 🔥 在现有菜单 (cmsMembers) 基础上追加开发选项
        /// </summary>
        private void InitializeMemberContextMenu()
        {
            // ========================================
            // 🔥 1. 添加常规功能（原有菜单基础上增加）
            // ========================================
            
            // 🔄 刷新会员（从服务器重新获取群成员列表，更新昵称）
            var refreshMembersItem = new ToolStripMenuItem
            {
                Text = "🔄 刷新会员",
                Name = "menuRefreshMembers"
            };
            refreshMembersItem.Click += MenuRefreshMembers_Click;
            cmsMembers.Items.Add(refreshMembersItem);
            
            // 💰 手动调整余额
            var adjustBalanceItem = new ToolStripMenuItem
            {
                Text = "💰 手动调整余额",
                Name = "menuAdjustBalance"
            };
            adjustBalanceItem.Click += MenuAdjustBalance_Click;
            cmsMembers.Items.Add(adjustBalanceItem);
            
            // ========================================
            // 🔥 2. 添加开发模式专属功能（动态显示）
            // ========================================
            
            // 添加分隔线（开发模式下显示）
            _separatorBeforeDevOptions = new ToolStripSeparator
            {
                Visible = false
            };
            cmsMembers.Items.Add(_separatorBeforeDevOptions);
            
            // 创建"开发选项"菜单项（开发模式下显示）
            _devOptionsMenuItem = new ToolStripMenuItem
            {
                Text = "🔧 开发选项",
                Name = "menuDevOptions",
                Visible = false,
                Enabled = false
            };
            
            // 添加子菜单项
            var sendTestMessageItem = new ToolStripMenuItem
            {
                Text = "发送测试消息",
                Name = "menuSendTestMessage"
            };
            sendTestMessageItem.Click += MenuSendTestMessage_Click;
            
            var sendMessageSimulatorItem = new ToolStripMenuItem
            {
                Text = "📱 发送消息（模拟窗口）",
                Name = "menuSendMessageSimulator",
                ShortcutKeys = Keys.Control | Keys.M
            };
            sendMessageSimulatorItem.Click += MenuSendMessageSimulator_Click;
            
            var setCurrentMemberItem = new ToolStripMenuItem
            {
                Text = "设为当前测试会员",
                Name = "menuSetCurrentMember"
            };
            setCurrentMemberItem.Click += MenuSetCurrentMember_Click;
            
            // 🔊 测试声音播放
            var testSoundItem = new ToolStripMenuItem
            {
                Text = "🔊 测试声音播放",
                Name = "menuTestSound"
            };
            testSoundItem.Click += MenuTestSound_Click;
            
            // 📤 发送结算消息到微信群
            var resendSettlementItem = new ToolStripMenuItem
            {
                Text = "📤 发送结算消息到微信群",
                Name = "menuResendSettlement"
            };
            resendSettlementItem.Click += MenuResendSettlement_Click;
            
            _devOptionsMenuItem.DropDownItems.Add(sendTestMessageItem);
            _devOptionsMenuItem.DropDownItems.Add(sendMessageSimulatorItem);
            _devOptionsMenuItem.DropDownItems.Add(setCurrentMemberItem);
            _devOptionsMenuItem.DropDownItems.Add(new ToolStripSeparator());
            _devOptionsMenuItem.DropDownItems.Add(testSoundItem);
            _devOptionsMenuItem.DropDownItems.Add(new ToolStripSeparator());
            _devOptionsMenuItem.DropDownItems.Add(resendSettlementItem);
            
            cmsMembers.Items.Add(_devOptionsMenuItem);
            
            // 监听右键菜单打开事件，根据开发模式动态设置可见性
            cmsMembers.Opening += MemberContextMenu_Opening;
            
            _logService.Info("VxMain", "✅ 会员表右键菜单已扩展（原有功能 + 手动调整余额 + 开发选项）");
        }
        
        /// <summary>
        /// 右键菜单打开时检查开发模式
        /// 🔥 防止作弊：每次打开时都检查，不能被灰色按钮专家破解
        /// </summary>
        private void MemberContextMenu_Opening(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_devOptionsMenuItem == null || _separatorBeforeDevOptions == null) return;
            
            // 🔥 每次打开菜单都重新检查开发模式状态（防作弊）
            bool isDevMode = _configService.GetIsRunModeDev();
            
            // 动态显示/隐藏开发选项和分隔线
            _separatorBeforeDevOptions.Visible = isDevMode;
            _devOptionsMenuItem.Visible = isDevMode;
            _devOptionsMenuItem.Enabled = isDevMode;
            
            _logService.Debug("VxMain", $"右键菜单打开检查: 开发模式={isDevMode}");
        }
        
        /// <summary>
        /// 菜单项：发送测试消息（模拟会员发送消息，走真实订单流程）
        /// 🔥 从第③步开始：MessageDispatcher → ChatMessageHandler → BinggoMessageHandler
        /// </summary>
        private async void MenuSendTestMessage_Click(object? sender, EventArgs e)
        {
            try
            {
                // 🔥 再次检查开发模式（防作弊）
                if (!_configService.GetIsRunModeDev())
                {
                    _logService.Warning("VxMain", "⚠️ 非开发模式，无法发送测试消息");
                    UIMessageBox.ShowWarning("请先在设置中启用开发模式！");
                    return;
                }
                
                // 获取选中的会员
                if (dgvMembers.CurrentRow?.DataBoundItem is not V2Member member)
                {
                    _logService.Warning("VxMain", "未选中会员");
                    UIMessageBox.ShowWarning("请先选择一个会员！");
                    return;
                }
                
                // 获取要发送的消息
                string message = _configService.GetRunDevSendMessage();
                if (string.IsNullOrWhiteSpace(message))
                {
                    _logService.Warning("VxMain", "测试消息内容为空");
                    UIMessageBox.ShowWarning("请在设置中配置测试消息内容！");
                    return;
                }
                
                _logService.Info("VxMain", $"🔧 开发模式-模拟会员发送消息: {member.Nickname}({member.Wxid}) -> {message}");
                
                // 🔥 调用统一的模拟消息方法（从第③步 MessageDispatcher 开始）
                var (success, replyMessage, errorMessage) = await SimulateMemberMessageAsync(member.Wxid, message);
                
                if (success)
                {
                    _logService.Info("VxMain", $"✅ 测试消息已处理完成，回复: {replyMessage ?? "无回复"}");
                    
                    string resultMsg = $"✅ 测试消息已成功处理！\n\n会员：{member.Nickname}\n消息：{message}\n\n";
                    
                    if (!string.IsNullOrEmpty(replyMessage))
                    {
                        resultMsg += $"系统回复：{replyMessage}\n\n";
                    }
                    
                    resultMsg += "订单已创建，请在订单表中查看。\n开奖后会自动结算。\n\n🔥 流程：MessageDispatcher → ChatMessageHandler → BinggoMessageHandler";
                    
                    UIMessageBox.ShowSuccess(resultMsg);
                }
                else
                {
                    _logService.Warning("VxMain", $"⚠️ 测试消息未被处理");
                    UIMessageBox.ShowWarning($"测试消息未被处理！\n\n会员：{member.Nickname}\n消息：{message}\n\n原因：{errorMessage ?? "未知错误"}");
                }
            }
            catch (Exception ex)
            {
                _logService.Error("VxMain", $"处理测试消息失败: {ex.Message}", ex);
                UIMessageBox.ShowError($"处理测试消息失败！\n\n{ex.Message}");
            }
        }
        
        /// <summary>
        /// 🔥 菜单项：发送消息（模拟窗口）
        /// 打开微信风格的消息模拟窗口，以会员身份发送测试消息
        /// </summary>
        private void MenuSendMessageSimulator_Click(object? sender, EventArgs e)
        {
            try
            {
                // 🔥 再次检查开发模式（防作弊）
                if (!_configService.GetIsRunModeDev())
                {
                    _logService.Warning("VxMain", "⚠️ 非开发模式，无法打开消息模拟器");
                    UIMessageBox.ShowWarning("请先在设置中启用开发模式！");
                    return;
                }
                
                // 🔥 获取选中的会员
                if (dgvMembers.CurrentRow?.DataBoundItem is not V2Member member)
                {
                    _logService.Warning("VxMain", "未选中会员");
                    UIMessageBox.ShowWarning("请先选择一个会员！");
                    return;
                }
                
                // 🔥 检查是否已绑定群
                if (_groupBindingService.CurrentBoundGroup == null)
                {
                    _logService.Warning("VxMain", "未绑定群组");
                    UIMessageBox.ShowWarning("请先绑定一个群组！");
                    return;
                }
                
                _logService.Info("VxMain", $"📱 打开消息模拟窗口: {member.Nickname} ({member.Wxid})");
                
                // 🔥 获取或创建消息模拟窗口（单例模式，同一会员只能开一个窗口）
                var simulatorForm = BaiShengVx3Plus.Views.Dev.MessageSimulatorForm.GetOrCreate(
                    member,
                    SimulateMemberMessageAsync,  // ← 复用已有方法！
                    _logService);
                
                // 🔥 显示为非模态窗口
                simulatorForm.Show(this);
            }
            catch (Exception ex)
            {
                _logService.Error("VxMain", $"打开消息模拟窗口失败: {ex.Message}", ex);
                UIMessageBox.ShowError($"打开消息模拟窗口失败！\n\n{ex.Message}");
            }
        }
        
        /// <summary>
        /// 菜单项：设为当前测试会员
        /// </summary>
        private void MenuSetCurrentMember_Click(object? sender, EventArgs e)
        {
            try
            {
                // 🔥 再次检查开发模式（防作弊）
                if (!_configService.GetIsRunModeDev())
                {
                    _logService.Warning("VxMain", "⚠️ 非开发模式，无法设置测试会员");
                    UIMessageBox.ShowWarning("请先在设置中启用开发模式！");
                    return;
                }
                
                // 获取选中的会员
                if (dgvMembers.CurrentRow?.DataBoundItem is not V2Member member)
                {
                    _logService.Warning("VxMain", "未选中会员");
                    UIMessageBox.ShowWarning("请先选择一个会员！");
                    return;
                }
                
                // 更新配置
                string memberInfo = $"{member.Nickname}({member.Wxid})";
                _configService.SetRunDevCurrentMember(memberInfo);
                
                _logService.Info("VxMain", $"✅ 已设置当前测试会员: {memberInfo}");
                UIMessageBox.ShowSuccess($"已设置当前测试会员：\n\n{memberInfo}");
            }
            catch (Exception ex)
            {
                _logService.Error("VxMain", $"设置测试会员失败: {ex.Message}", ex);
                UIMessageBox.ShowError($"设置测试会员失败！\n\n{ex.Message}");
            }
        }
        
        /// <summary>
        /// 会员选择变化事件 - 自动更新当前测试会员
        /// </summary>
        private void DgvMembers_SelectionChanged(object? sender, EventArgs e)
        {
            try
            {
                // 只在开发模式下自动更新
                if (!_configService.GetIsRunModeDev())
                    return;
                
                // 获取选中的会员
                if (dgvMembers.CurrentRow?.DataBoundItem is V2Member member)
                {
                    string memberInfo = $"{member.Nickname}({member.Wxid})";
                    _configService.SetRunDevCurrentMember(memberInfo);
                    
                    _logService.Debug("VxMain", $"🔧 自动更新当前测试会员: {memberInfo}");
                }
            }
            catch (Exception ex)
            {
                _logService.Error("VxMain", $"更新当前测试会员失败: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// 🔧 公共方法：模拟会员发送消息（供 SettingsForm 调用）
        /// 🔥 使用和 ChatMessageHandler 完全相同的代码逻辑
        /// </summary>
        /// <param name="memberWxid">会员微信ID</param>
        /// <param name="message">消息内容</param>
        /// <returns>(是否成功, 回复消息, 错误信息)</returns>
        public async Task<(bool success, string? replyMessage, string? errorMessage)> SimulateMemberMessageAsync(
            string memberWxid, 
            string message)
        {
            try
            {
                // 🔥 检查开发模式（防作弊）
                if (!_configService.GetIsRunModeDev())
                {
                    return (false, null, "非开发模式，无法模拟消息");
                }
                
                // 检查是否已绑定群
                var currentGroup = _groupBindingService.CurrentBoundGroup;
                if (currentGroup == null)
                {
                    return (false, null, "未绑定群组，请先绑定一个群组");
                }
                
                _logService.Info("VxMain", $"🔧 开发模式-模拟会员发送消息到群: {currentGroup.Nickname} | 会员: {memberWxid} | 消息: {message}");
                
                // ========================================
                // 🎮 使用和 ChatMessageHandler 完全相同的处理逻辑
                // ========================================
                
                // 1. 🔥 检查收单开关（与 ChatMessageHandler 第68行一致）
                _logService.Debug("VxMain", $"🔍 检查收单开关: IsOrdersTaskingEnabled = {BinggoMessageHandler.IsOrdersTaskingEnabled}");
                if (!BinggoMessageHandler.IsOrdersTaskingEnabled)
                {
                    _logService.Info("VxMain", "⏸️ 收单已关闭，忽略群消息");
                    return (false, null, "❌ 收单已关闭\n\n当前系统收单开关处于关闭状态，无法接受下注。");
                }
                
                // 2. 🔥 获取发送者会员信息（与 ChatMessageHandler 第76行一致）
                V2Member? member = null;
                if (_membersBindingList != null)
                {
                    foreach (var m in _membersBindingList)
                    {
                        if (m.Wxid == memberWxid)
                        {
                            member = m;
                            break;
                        }
                    }
                }
                
                if (member == null)
                {
                    _logService.Debug("VxMain", $"未找到会员: {memberWxid}，跳过炳狗处理");
                    return (false, null, $"❌ 未找到会员\n\n微信ID: {memberWxid}\n\n该会员不在当前绑定群的会员列表中。");
                }
                
                // 3. 🔥 调用炳狗消息处理器（与 ChatMessageHandler 第90行完全一致）
                _logService.Info("VxMain", $"📨 调用 BinggoMessageHandler.HandleMessageAsync");
                
                // 🔥 获取当前用户 wxid 和群 wxid
                string currentUserWxid = _userInfoService.GetCurrentWxid();
                string groupWxid = _groupBindingService.CurrentBoundGroup?.Wxid ?? "";
                
                var (handled, replyMessage) = await _binggoMessageHandler.HandleMessageAsync(
                    member, 
                    message,
                    groupWxid,          // 🔥 群ID
                    currentUserWxid);   // 🔥 当前用户ID
                
                // 4. 🔥 处理返回结果（与 ChatMessageHandler 第89行逻辑一致）
                if (handled && !string.IsNullOrEmpty(replyMessage))
                {
                    // ✅ 消息已处理，有回复（成功或失败都会有回复消息）
                    _logService.Info("VxMain", 
                        $"✅ 消息已处理，回复: {replyMessage.Substring(0, Math.Min(50, replyMessage.Length))}...");
                    
                    return (true, replyMessage, null);
                }
                else if (handled && string.IsNullOrEmpty(replyMessage))
                {
                    // ✅ 消息已处理，但没有回复
                    _logService.Info("VxMain", "✅ 消息已处理（无回复）");
                    return (true, "✅ 消息已处理（无回复）", null);
                }
                else
                {
                    // ⚠️ 消息未处理（被过滤器忽略）
                    _logService.Warning("VxMain", $"⚠️ 消息未处理（可能不符合下注格式或被过滤）");
                    
                    // 🔥 构造详细的诊断信息
                    string diagnosticInfo = "⚠️ 消息被系统忽略\n\n";
                    diagnosticInfo += "可能原因：\n";
                    diagnosticInfo += "1. 消息格式不符合下注规则\n";
                    diagnosticInfo += "   （需包含：大/小/单/双/对子等关键字）\n";
                    diagnosticInfo += "2. 消息被过滤器拦截\n";
                    diagnosticInfo += "   - 以 @ 或 [ 开头的消息\n";
                    diagnosticInfo += "   - 包含 <msg> 标签的消息\n";
                    diagnosticInfo += "   - 长度小于 2 个字符的消息\n";
                    diagnosticInfo += $"\n💬 消息内容: {message}\n";
                    diagnosticInfo += $"👤 会员: {member.Nickname}({member.Wxid})\n";
                    diagnosticInfo += $"🔄 收单状态: {(BinggoMessageHandler.IsOrdersTaskingEnabled ? "✅ 已开启" : "❌ 已关闭")}";
                    
                    return (false, null, diagnosticInfo);
                }
            }
            catch (Exception ex)
            {
                _logService.Error("VxMain", $"模拟消息处理失败: {ex.Message}", ex);
                return (false, null, $"❌ 系统异常\n\n{ex.Message}");
            }
        }
        
        #region 常用功能菜单事件
        
        /// <summary>
        /// 🔄 刷新会员（从服务器重新获取群成员列表，自动更新昵称）
        /// </summary>
        private async void MenuRefreshMembers_Click(object? sender, EventArgs e)
        {
            try
            {
                // 检查是否已绑定群
                if (_groupBindingService.CurrentBoundGroup == null)
                {
                    UIMessageBox.ShowWarning("请先绑定一个群组！");
                    return;
                }
                
                // 检查会员列表是否已初始化
                if (_membersBindingList == null)
                {
                    UIMessageBox.ShowWarning("会员列表未初始化！");
                    return;
                }
                
                _logService.Info("VxMain", $"🔄 开始刷新群成员: {_groupBindingService.CurrentBoundGroup.Nickname}");
                
                // 🔥 调用 GroupBindingService 的刷新方法
                var (success, memberCount) = await _groupBindingService.RefreshCurrentGroupMembersAsync(
                    _socketClient,
                    _membersBindingList);
                
                if (success)
                {
                    _logService.Info("VxMain", $"✅ 刷新完成: {memberCount} 个会员");
                    UIMessageBox.ShowSuccess($"刷新成功！\n\n共 {memberCount} 个会员\n\n昵称变化已自动更新并记录到日志。");
                    
                    // 🔥 刷新统计数据
                    _statisticsService.UpdateStatistics();
                }
                else
                {
                    _logService.Warning("VxMain", "刷新失败，请检查网络连接");
                    UIMessageBox.ShowWarning("刷新失败！\n\n无法从服务器获取群成员列表，\n请检查网络连接或微信登录状态。");
                }
            }
            catch (Exception ex)
            {
                _logService.Error("VxMain", $"刷新会员失败: {ex.Message}", ex);
                UIMessageBox.ShowError($"刷新会员失败！\n\n{ex.Message}");
            }
        }
        
        /// <summary>
        /// 💰 手动调整余额
        /// </summary>
        private void MenuAdjustBalance_Click(object? sender, EventArgs e)
        {
            try
            {
                if (dgvMembers.CurrentRow?.DataBoundItem is not V2Member member)
                {
                    UIMessageBox.ShowWarning("请先选择一个会员！");
                    return;
                }
                
                // 使用输入框获取调整金额
                string input = Microsoft.VisualBasic.Interaction.InputBox(
                    $"请输入调整金额（正数=增加，负数=减少）\n\n会员：{member.Nickname}\n当前余额：{member.Balance:F2}",
                    "调整会员余额",
                    "0");
                
                if (string.IsNullOrWhiteSpace(input))
                    return;
                
                if (!float.TryParse(input, out float amount) || amount == 0)
                {
                    UIMessageBox.ShowWarning("请输入有效的调整金额！");
                    return;
                }
                
                float oldBalance = member.Balance;
                float newBalance = oldBalance + amount;
                
                if (newBalance < 0)
                {
                    UIMessageBox.ShowWarning("调整后余额不能为负数！");
                    return;
                }
                
                // 确认调整
                string actionText = amount > 0 ? "增加" : "减少";
                if (!UIMessageBox.ShowAsk($"确定要{actionText}【{member.Nickname}】的余额吗？\n\n" +
                    $"调整金额：{amount:F2}\n" +
                    $"调整前余额：{oldBalance:F2}\n" +
                    $"调整后余额：{newBalance:F2}"))
                {
                    return;
                }
                
                // 调整余额
                member.Balance = newBalance;
                
                // 记录到资金变动表
                if (_db != null)
                {
                    var balanceChange = new V2BalanceChange
                    {
                        GroupWxId = member.GroupWxId,
                        Wxid = member.Wxid,
                        Nickname = member.Nickname,
                        BalanceBefore = oldBalance,
                        BalanceAfter = newBalance,
                        ChangeAmount = amount,
                        Reason = ChangeReason.手动调整,
                        IssueId = 0,
                        TimeString = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        Timestamp = DateTimeOffset.Now.ToUnixTimeSeconds(),
                        Notes = $"管理员手动调整余额：{amount:F2}"
                    };
                    
                    _db.Insert(balanceChange);
                }
                
                _logService.Info("VxMain", $"手动调整余额: {member.Nickname} {oldBalance:F2} → {newBalance:F2}");
                UIMessageBox.ShowSuccess($"余额调整成功！\n\n" +
                    $"会员：{member.Nickname}\n" +
                    $"新余额：{newBalance:F2}");
            }
            catch (Exception ex)
            {
                _logService.Error("VxMain", $"调整余额失败: {ex.Message}", ex);
                UIMessageBox.ShowError($"调整余额失败：{ex.Message}");
            }
        }
        
        /// <summary>
        /// 📤 发送结算消息到微信群
        /// </summary>
        private async void MenuResendSettlement_Click(object? sender, EventArgs e)
        {
            try
            {
                // 🔥 再次检查开发模式（防作弊）
                if (!_configService.GetIsRunModeDev())
                {
                    _logService.Warning("VxMain", "⚠️ 非开发模式，无法发送结算消息");
                    UIMessageBox.ShowWarning("请先在设置中启用开发模式！");
                    return;
                }

                // 🔥 检查是否已绑定群
                if (_groupBindingService.CurrentBoundGroup == null)
                {
                    _logService.Warning("VxMain", "未绑定群组");
                    UIMessageBox.ShowWarning("请先绑定一个群组！");
                    return;
                }

                // 🔥 检查微信连接
                if (_socketClient == null || !_socketClient.IsConnected)
                {
                    _logService.Warning("VxMain", "微信未连接");
                    UIMessageBox.ShowWarning("微信未连接，请先登录微信！");
                    return;
                }

                // 确认操作
                if (!UIMessageBox.ShowAsk("确定要重新发送结算消息到微信群吗？\n\n" +
                    "系统将查找最新已开奖的期号，\n" +
                    "并重新发送中~名单和留~名单。"))
                {
                    return;
                }

                _logService.Info("VxMain", "📤 开始重新发送结算消息...");

                // 🔥 调用开奖服务的重新发送方法
                // 注意：需要将 IBinggoLotteryService 转换为 BinggoLotteryService 才能调用 ResendSettlementMessagesAsync
                // 或者通过接口添加这个方法
                if (_lotteryService is Services.Games.Binggo.BinggoLotteryService lotteryService)
                {
                    var (success, message) = await lotteryService.ResendSettlementMessagesAsync();
                    
                    if (success)
                    {
                        _logService.Info("VxMain", $"✅ {message}");
                        UIMessageBox.ShowSuccess($"结算消息已重新发送！\n\n{message}");
                    }
                    else
                    {
                        _logService.Warning("VxMain", $"⚠️ {message}");
                        UIMessageBox.ShowWarning($"重新发送结算消息失败！\n\n{message}");
                    }
                }
                else
                {
                    _logService.Error("VxMain", "无法获取 BinggoLotteryService 实例");
                    UIMessageBox.ShowError("系统错误：无法获取开奖服务实例！");
                }
            }
            catch (Exception ex)
            {
                _logService.Error("VxMain", $"重新发送结算消息失败: {ex.Message}", ex);
                UIMessageBox.ShowError($"重新发送结算消息失败！\n\n{ex.Message}");
            }
        }
        
        /// <summary>
        /// 🔊 测试声音播放
        /// </summary>
        private void MenuTestSound_Click(object? sender, EventArgs e)
        {
            try
            {
                // 🔥 使用正确的 GetService 方式（非泛型）
                var soundService = Program.ServiceProvider.GetService(typeof(Services.Sound.SoundService)) as Services.Sound.SoundService;
                if (soundService == null)
                {
                    UIMessageBox.ShowError("声音服务未初始化！");
                    _logService.Error("VxMain", "SoundService 未找到");
                    return;
                }

                // 创建一个简单的测试菜单
                var testForm = new Form
                {
                    Text = "🔊 测试声音播放",
                    Size = new Size(400, 300),
                    StartPosition = FormStartPosition.CenterParent,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    MaximizeBox = false,
                    MinimizeBox = false
                };

                var flowPanel = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.TopDown,
                    Padding = new Padding(20),
                    AutoScroll = true
                };

                // 测试按钮
                var btnSealing = new Button { Text = "🔔 测试封盘声音", Width = 300, Height = 40 };
                btnSealing.Click += (s, ev) =>
                {
                    _logService.Info("VxMain", "🔊 手动测试封盘声音");
                    soundService.PlaySealingSound();
                    UIMessageTip.ShowOk("封盘声音已播放");
                };

                var btnLottery = new Button { Text = "🎲 测试开奖声音", Width = 300, Height = 40 };
                btnLottery.Click += (s, ev) =>
                {
                    _logService.Info("VxMain", "🔊 手动测试开奖声音");
                    soundService.PlayLotterySound();
                    UIMessageTip.ShowOk("开奖声音已播放");
                };

                var btnCreditUp = new Button { Text = "💰 测试上分声音", Width = 300, Height = 40 };
                btnCreditUp.Click += (s, ev) =>
                {
                    _logService.Info("VxMain", "🔊 手动测试上分声音");
                    soundService.PlayCreditUpSound();
                    UIMessageTip.ShowOk("上分声音已播放");
                };

                var btnCreditDown = new Button { Text = "💸 测试下分声音", Width = 300, Height = 40 };
                btnCreditDown.Click += (s, ev) =>
                {
                    _logService.Info("VxMain", "🔊 手动测试下分声音");
                    soundService.PlayCreditDownSound();
                    UIMessageTip.ShowOk("下分声音已播放");
                };

                flowPanel.Controls.Add(btnSealing);
                flowPanel.Controls.Add(btnLottery);
                flowPanel.Controls.Add(btnCreditUp);
                flowPanel.Controls.Add(btnCreditDown);

                testForm.Controls.Add(flowPanel);
                testForm.ShowDialog(this);
            }
            catch (Exception ex)
            {
                _logService.Error("VxMain", $"测试声音失败: {ex.Message}", ex);
                UIMessageBox.ShowError($"测试声音失败！\n\n{ex.Message}");
            }
        }
        
        #endregion
    }
}

