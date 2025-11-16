using System;
using System.Drawing;
using System.Windows.Forms;
using zhaocaimao.Models;
using zhaocaimao.Services.Messages.Handlers;
using Sunny.UI;

namespace zhaocaimao
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
            // 🔥 1. 添加常规功能：手动调整余额（原有菜单基础上增加）
            // ========================================
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
            
            var setCurrentMemberItem = new ToolStripMenuItem
            {
                Text = "设为当前测试会员",
                Name = "menuSetCurrentMember"
            };
            setCurrentMemberItem.Click += MenuSetCurrentMember_Click;
            
            _devOptionsMenuItem.DropDownItems.Add(sendTestMessageItem);
            _devOptionsMenuItem.DropDownItems.Add(setCurrentMemberItem);
            
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
        
        #endregion
    }
}

