using BaiShengVx3Plus.Contracts;
using BaiShengVx3Plus.Contracts.Games;
using BaiShengVx3Plus.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BaiShengVx3Plus.Views.Dev
{
    /// <summary>
    /// 💬 消息模拟器窗口
    /// 
    /// 功能：
    /// 1. 模拟微信聊天界面
    /// 2. 以会员身份发送测试消息
    /// 3. 显示系统回复
    /// 4. 支持所有消息类型（投注、上下分、管理命令等）
    /// 5. 🔥 显示系统消息（开盘、封盘、开奖、结算等）
    /// 6. 🔥 显示图片消息（历史记录图片等）
    /// 
    /// 设计原则：
    /// - 非模态窗口
    /// - 同一会员只能开一个窗口
    /// - 复用 VxMain.SimulateMemberMessageAsync 方法
    /// - 不修改任何业务逻辑
    /// </summary>
    public partial class MessageSimulatorForm : Form
    {
        #region 静态窗口管理（单例模式）

        /// <summary>
        /// 🔥 静态字典：管理所有打开的窗口（Wxid → Form）
        /// </summary>
        private static readonly Dictionary<string, MessageSimulatorForm> _openWindows = new();

        /// <summary>
        /// 🔥 静态事件：开发模式下发送到群的消息通知（用于消息模拟器显示）
        /// </summary>
        public static event EventHandler<(string messageType, string message, string? imagePath)>? SystemMessageSent;

        /// <summary>
        /// 🔥 静态方法：通知所有消息模拟器窗口显示系统消息（开发模式专用）
        /// </summary>
        public static void NotifySystemMessage(string messageType, string message, string? imagePath = null)
        {
            // 🔥 调试日志：记录通知调用
            var subscriberCount = SystemMessageSent?.GetInvocationList()?.Length ?? 0;
            System.Diagnostics.Debug.WriteLine($"[NotifySystemMessage] messageType={messageType}, message长度={message?.Length ?? 0}, imagePath={imagePath ?? "null"}, 订阅者数量={subscriberCount}");
            
            try
            {
                SystemMessageSent?.Invoke(null, (messageType, message, imagePath));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[NotifySystemMessage] 异常: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 🔥 静态工厂方法：获取或创建窗口（单例）
        /// </summary>
        /// <param name="member">会员信息</param>
        /// <param name="simulateMessageFunc">模拟消息方法</param>
        /// <param name="logService">日志服务</param>
        /// <param name="lotteryService">彩票服务（可选，用于订阅系统消息）</param>
        /// <returns>窗口实例</returns>
        public static MessageSimulatorForm GetOrCreate(
            V2Member member,
            Func<string, string, Task<(bool success, string? replyMessage, string? errorMessage)>> simulateMessageFunc,
            ILogService logService,
            IBinggoLotteryService? lotteryService = null)
        {
            if (_openWindows.TryGetValue(member.Wxid, out var existingForm))
            {
                // ✅ 已有窗口，激活并返回
                existingForm.Activate();
                existingForm.BringToFront();
                existingForm.Focus();
                
                logService.Info("MessageSimulator", $"激活已有窗口: {member.Nickname} ({member.Wxid})");
                return existingForm;
            }

            // ✅ 创建新窗口
            var newForm = new MessageSimulatorForm(member, simulateMessageFunc, logService, lotteryService);
            _openWindows[member.Wxid] = newForm;

            // ✅ 窗口关闭时移除
            newForm.FormClosed += (s, e) =>
            {
                _openWindows.Remove(member.Wxid);
                logService.Info("MessageSimulator", $"窗口已关闭: {member.Nickname} ({member.Wxid})");
            };

            logService.Info("MessageSimulator", $"创建新窗口: {member.Nickname} ({member.Wxid})");
            return newForm;
        }

        #endregion

        #region 字段

        private readonly V2Member _member;
        private readonly Func<string, string, Task<(bool success, string? replyMessage, string? errorMessage)>> _simulateMessageFunc;
        private readonly ILogService _logService;
        private readonly IBinggoLotteryService? _lotteryService;
        private bool _isSending = false;

        #endregion

        #region 构造函数

        /// <summary>
        /// 🔥 私有构造函数（通过静态工厂方法创建）
        /// </summary>
        private MessageSimulatorForm(
            V2Member member,
            Func<string, string, Task<(bool success, string? replyMessage, string? errorMessage)>> simulateMessageFunc,
            ILogService logService,
            IBinggoLotteryService? lotteryService = null)
        {
            InitializeComponent();

            _member = member;
            _simulateMessageFunc = simulateMessageFunc;
            _logService = logService;
            _lotteryService = lotteryService;

            InitializeUI();
            SubscribeToSystemMessages();
            SubscribeToStaticNotifications();
        }

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化 UI
        /// </summary>
        private void InitializeUI()
        {
            // 1. 设置窗口标题
            string roleText = _member.State == MemberState.管理 ? "👑 管理员" : "👤 会员";
            this.Text = $"💬 消息模拟器 - {roleText} {_member.Nickname} ({_member.Wxid})";

            // 2. 初始化快捷命令
            InitializeQuickCommands();

            // 3. 显示欢迎消息
            AppendSystemMessage($"欢迎使用消息模拟器！\n\n" +
                $"当前身份: {roleText}\n" +
                $"会员昵称: {_member.Nickname}\n" +
                $"微信ID: {_member.Wxid}\n" +
                $"当前余额: {_member.Balance:F2}\n\n" +
                $"💡 提示:\n" +
                $"- 按 Enter 发送消息\n" +
                $"- 按 Shift+Enter 换行\n" +
                $"- 可使用快捷命令快速输入");

            // 4. 聚焦到输入框
            txtInput.Focus();
        }

        /// <summary>
        /// 初始化快捷命令
        /// </summary>
        private void InitializeQuickCommands()
        {
            var commands = new List<string>
            {
                "-- 请选择 --"
            };

            // 根据会员身份添加命令
            if (_member.State == MemberState.管理)
            {
                commands.AddRange(new[]
                {
                    "-- 管理员命令 --",
                    "刷新",
                    "清零",
                    "封盘",
                    "开盘",
                    "7上100",
                    "7下100",
                    "8查",
                    "-- 会员命令（仅供测试）--",
                    "123大10",
                    "123小10",
                    "查",
                    "取消"
                });
            }
            else
            {
                commands.AddRange(new[]
                {
                    "-- 会员命令 --",
                    "123大10",
                    "123小10",
                    "124单5",
                    "125双5",
                    "上100",
                    "下100",
                    "查",
                    "取消"
                });
            }

            cbxQuickCommands.Items.AddRange(commands.ToArray());
            cbxQuickCommands.SelectedIndex = 0;
        }

        #endregion

        #region 事件处理

        /// <summary>
        /// 发送按钮点击事件
        /// </summary>
        private async void btnSend_Click(object sender, EventArgs e)
        {
            await SendMessageAsync();
        }

        /// <summary>
        /// 清空历史按钮点击事件
        /// </summary>
        private void btnClear_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(
                "确定要清空消息历史吗？",
                "确认",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes)
            {
                rtbMessages.Clear();
                _logService.Info("MessageSimulator", $"清空消息历史: {_member.Nickname}");
            }
        }

        /// <summary>
        /// 快捷命令选择事件
        /// </summary>
        private void cbxQuickCommands_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbxQuickCommands.SelectedIndex > 0 &&
                !cbxQuickCommands.SelectedItem?.ToString()?.StartsWith("--") == true)
            {
                txtInput.Text = cbxQuickCommands.SelectedItem?.ToString() ?? "";
                txtInput.Focus();
                txtInput.SelectAll();
                
                // 重置下拉框
                cbxQuickCommands.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// 输入框键盘事件（支持 Enter 发送）
        /// </summary>
        private async void txtInput_KeyDown(object sender, KeyEventArgs e)
        {
            // Enter 发送，Shift+Enter 换行
            if (e.KeyCode == Keys.Enter && !e.Shift)
            {
                e.SuppressKeyPress = true;  // 阻止默认的换行行为
                await SendMessageAsync();
            }
        }

        #endregion

        #region 核心功能

        /// <summary>
        /// 🔥 发送消息（核心方法）
        /// </summary>
        private async Task SendMessageAsync()
        {
            // 1. 检查是否正在发送
            if (_isSending)
            {
                _logService.Warning("MessageSimulator", "上一条消息正在处理中，请稍候...");
                return;
            }

            // 2. 获取消息内容
            string message = txtInput.Text.Trim();
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            try
            {
                _isSending = true;
                btnSend.Enabled = false;
                btnSend.Text = "发送中...";

                // 3. 显示发送的消息（右对齐，绿色）
                AppendMemberMessage(_member.Nickname, message);

                // 4. 清空输入框
                txtInput.Clear();

                _logService.Info("MessageSimulator", 
                    $"发送消息: {_member.Nickname} ({_member.Wxid}) -> {message}");

                // 5. 🔥 调用模拟方法（复用 VxMain.SimulateMemberMessageAsync）
                var (success, replyMessage, errorMessage) = await _simulateMessageFunc(_member.Wxid, message);

                // 6. 显示回复消息
                if (success)
                {
                    string displayMessage = replyMessage ?? "✅ 消息已处理（无回复）";
                    AppendSystemMessage(displayMessage);
                    
                    _logService.Info("MessageSimulator", 
                        $"收到回复: {displayMessage.Substring(0, Math.Min(50, displayMessage.Length))}...");
                }
                else
                {
                    string displayMessage = errorMessage ?? "未知错误";
                    AppendErrorMessage(displayMessage);
                    
                    _logService.Warning("MessageSimulator", $"消息处理失败: {displayMessage}");
                }
            }
            catch (Exception ex)
            {
                _logService.Error("MessageSimulator", "发送消息时发生异常", ex);
                AppendErrorMessage($"⚠️ 发送失败:\n{ex.Message}");
            }
            finally
            {
                _isSending = false;
                btnSend.Enabled = true;
                btnSend.Text = "发送(Enter)";
                txtInput.Focus();
            }
        }

        #endregion

        #region 系统消息订阅

        /// <summary>
        /// 🔥 订阅系统消息事件（开盘、封盘、开奖等）
        /// </summary>
        private void SubscribeToSystemMessages()
        {
            if (_lotteryService == null)
            {
                _logService.Debug("MessageSimulator", "彩票服务未提供，跳过订阅系统消息");
                return;
            }

            try
            {
                // 订阅状态变更事件（开盘、封盘等）
                _lotteryService.StatusChanged += LotteryService_StatusChanged;
                
                // 订阅开奖事件
                _lotteryService.LotteryOpened += LotteryService_LotteryOpened;
                
                // 订阅期号变更事件
                _lotteryService.IssueChanged += LotteryService_IssueChanged;
                
                _logService.Info("MessageSimulator", $"已订阅系统消息事件: {_member.Nickname}");
            }
            catch (Exception ex)
            {
                _logService.Error("MessageSimulator", "订阅系统消息失败", ex);
            }
        }

        /// <summary>
        /// 🔥 订阅静态消息通知（开发模式下发送到群的消息）
        /// </summary>
        private void SubscribeToStaticNotifications()
        {
            try
            {
                SystemMessageSent += MessageSimulatorForm_SystemMessageSent;
                _logService.Info("MessageSimulator", $"已订阅静态消息通知: {_member.Nickname}");
            }
            catch (Exception ex)
            {
                _logService.Error("MessageSimulator", "订阅静态消息通知失败", ex);
            }
        }

        /// <summary>
        /// 🔥 静态消息通知事件处理
        /// </summary>
        private void MessageSimulatorForm_SystemMessageSent(object? sender, (string messageType, string message, string? imagePath) e)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => MessageSimulatorForm_SystemMessageSent(sender, e)));
                return;
            }

            try
            {
                _logService.Info("MessageSimulator", $"📨 收到静态消息通知 - messageType: {e.messageType}, message长度: {e.message?.Length ?? 0}, imagePath: {e.imagePath ?? "null"}");
                
                if (!string.IsNullOrEmpty(e.imagePath))
                {
                    // 图片消息
                    _logService.Info("MessageSimulator", $"🖼️ 调用 ShowSystemImage: {e.imagePath}");
                    ShowSystemImage(e.imagePath, e.message);
                }
                else
                {
                    // 文本消息
                    string preview = e.message?.Length > 100 ? e.message.Substring(0, 100) + "..." : e.message ?? "";
                    _logService.Info("MessageSimulator", $"📝 调用 ShowSystemMessage: messageType={e.messageType}, message预览: {preview}");
                    ShowSystemMessage(e.message, e.messageType);
                }
            }
            catch (Exception ex)
            {
                _logService.Error("MessageSimulator", $"处理静态消息通知失败: {ex.Message}", ex);
                _logService.Error("MessageSimulator", $"异常堆栈: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// 🔥 取消订阅系统消息事件
        /// </summary>
        private void UnsubscribeFromSystemMessages()
        {
            if (_lotteryService == null) return;

            try
            {
                _lotteryService.StatusChanged -= LotteryService_StatusChanged;
                _lotteryService.LotteryOpened -= LotteryService_LotteryOpened;
                _lotteryService.IssueChanged -= LotteryService_IssueChanged;
                
                _logService.Info("MessageSimulator", $"已取消订阅系统消息事件: {_member.Nickname}");
            }
            catch (Exception ex)
            {
                _logService.Error("MessageSimulator", "取消订阅系统消息失败", ex);
            }
        }

        /// <summary>
        /// 🔥 取消订阅静态消息通知
        /// </summary>
        private void UnsubscribeFromStaticNotifications()
        {
            try
            {
                SystemMessageSent -= MessageSimulatorForm_SystemMessageSent;
                _logService.Info("MessageSimulator", $"已取消订阅静态消息通知: {_member.Nickname}");
            }
            catch (Exception ex)
            {
                _logService.Error("MessageSimulator", "取消订阅静态消息通知失败", ex);
            }
        }

        /// <summary>
        /// 🔥 状态变更事件处理（开盘、封盘等）
        /// 🔥 注意：这里不显示消息，因为状态变更只是内部事件
        /// 🔥 实际发送到微信的消息会通过 NotifySystemMessage 通知，这里只记录日志
        /// </summary>
        private void LotteryService_StatusChanged(object? sender, Models.Games.Binggo.Events.BinggoStatusChangedEventArgs e)
        {
            // 🔥 不显示状态变更事件，因为这不是发送到微信的消息
            // 🔥 实际发送到微信的消息会通过 NotifySystemMessage 通知
            // 这里只记录日志用于调试
            _logService.Debug("MessageSimulator", $"状态变更: {e.OldStatus} → {e.NewStatus}, 期号: {e.IssueId}");
        }

        /// <summary>
        /// 🔥 开奖事件处理
        /// 🔥 注意：这里不显示消息，因为开奖事件只是内部事件
        /// 🔥 实际发送到微信的结算消息（中~名单、留~名单）会通过 NotifySystemMessage 通知
        /// </summary>
        private void LotteryService_LotteryOpened(object? sender, Models.Games.Binggo.Events.BinggoLotteryOpenedEventArgs e)
        {
            // 🔥 不显示开奖事件，因为这不是发送到微信的消息
            // 🔥 实际发送到微信的结算消息会通过 NotifySystemMessage 通知
            // 这里只记录日志用于调试
            var data = e.LotteryData;
            if (data != null)
            {
                _logService.Debug("MessageSimulator", $"开奖事件: 期号 {data.IssueId} - {data.ToLotteryString()}");
            }
        }

        /// <summary>
        /// 🔥 期号变更事件处理
        /// 🔥 注意：这里不显示消息，因为期号变更只是内部事件
        /// 🔥 实际发送到微信的消息会通过 NotifySystemMessage 通知
        /// </summary>
        private void LotteryService_IssueChanged(object? sender, Models.Games.Binggo.Events.BinggoIssueChangedEventArgs e)
        {
            // 🔥 不显示期号变更事件，因为这不是发送到微信的消息
            // 🔥 实际发送到微信的消息会通过 NotifySystemMessage 通知
            // 这里只记录日志用于调试
            _logService.Debug("MessageSimulator", $"期号变更: {e.OldIssueId} → {e.NewIssueId}");
        }

        /// <summary>
        /// 🔥 显示系统发送的文本消息（开盘、封盘、结算等）
        /// 🔥 格式：实际消息内容（与微信完全一致）+ ((额外信息))
        /// </summary>
        public void ShowSystemMessage(string message, string messageType = "系统消息")
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => ShowSystemMessage(message, messageType)));
                return;
            }

            try
            {
                _logService.Debug("MessageSimulator", $"ShowSystemMessage 被调用 - messageType: {messageType}, message长度: {message?.Length ?? 0}");
                
                Color color = messageType switch
                {
                    "开盘" => Color.FromArgb(39, 174, 96),      // 绿色开盘
                    "封盘" => Color.FromArgb(230, 126, 34),     // 橙色封盘
                    "封盘提醒" => Color.FromArgb(230, 126, 34), // 橙色封盘提醒
                    "开奖" => Color.FromArgb(231, 76, 60),      // 红色开奖
                    "结算" => Color.FromArgb(52, 152, 219),     // 蓝色结算
                    "图片" => Color.FromArgb(155, 89, 182),     // 紫色图片
                    _ => Color.FromArgb(127, 140, 141)          // 灰色默认
                };

                // 🔥 格式：实际消息内容（与微信完全一致）+ ((消息类型))
                // 这样用户可以看到实际发送的内容，同时知道消息类型
                // ⚠️ 注意：如果消息本身以 \r 结尾，需要先处理换行符
                string normalizedMessage = message?.Replace("\r\n", "\n").Replace("\r", "\n") ?? "";
                string displayMessage = $"{normalizedMessage}\n((消息类型: {messageType}))";
                
                _logService.Debug("MessageSimulator", $"准备显示消息，displayMessage长度: {displayMessage.Length}");
                AppendSystemNotification(displayMessage, color);
            }
            catch (Exception ex)
            {
                _logService.Error("MessageSimulator", $"ShowSystemMessage 异常: {ex.Message}", ex);
                // 🔥 如果出错，至少尝试显示原始消息
                try
                {
                    rtbMessages.SelectionAlignment = HorizontalAlignment.Left;
                    rtbMessages.SelectionColor = Color.Black;
                    rtbMessages.SelectionFont = new Font(rtbMessages.Font.FontFamily, 10);
                    rtbMessages.AppendText($"[{DateTime.Now:HH:mm:ss}] 系统消息\n  {message}\n\n");
                    rtbMessages.ScrollToCaret();
                }
                catch
                {
                    // 忽略错误
                }
            }
        }

        /// <summary>
        /// 🔥 显示系统发送的图片消息
        /// 🔥 在 RichTextBox 中显示图片缩略图 + 路径信息
        /// </summary>
        public void ShowSystemImage(string imagePath, string? description = null)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => ShowSystemImage(imagePath, description)));
                return;
            }

            try
            {
                if (!File.Exists(imagePath))
                {
                    AppendSystemNotification($"((图片文件不存在: {imagePath}))", Color.Orange);
                    return;
                }

                // 🔥 时间戳（左对齐，灰色小字）
                rtbMessages.SelectionAlignment = HorizontalAlignment.Left;
                rtbMessages.SelectionColor = Color.Gray;
                rtbMessages.SelectionFont = new Font(rtbMessages.Font.FontFamily, 8);
                rtbMessages.AppendText($"[{DateTime.Now:HH:mm:ss}] 系统消息\n");

                // 🔥 加载图片并创建缩略图
                using (var originalImage = Image.FromFile(imagePath))
                {
                    // 🔥 创建缩略图（最大宽度300px，保持比例）
                    int maxWidth = 300;
                    int maxHeight = 300;
                    int thumbWidth = originalImage.Width;
                    int thumbHeight = originalImage.Height;

                    if (thumbWidth > maxWidth || thumbHeight > maxHeight)
                    {
                        double ratio = Math.Min((double)maxWidth / thumbWidth, (double)maxHeight / thumbHeight);
                        thumbWidth = (int)(thumbWidth * ratio);
                        thumbHeight = (int)(thumbHeight * ratio);
                    }

                    using (var thumbnail = new Bitmap(thumbWidth, thumbHeight))
                    {
                        using (var graphics = Graphics.FromImage(thumbnail))
                        {
                            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                            graphics.DrawImage(originalImage, 0, 0, thumbWidth, thumbHeight);
                        }

                        // 🔥 使用 Clipboard 将图片插入到 RichTextBox
                        // 保存当前 Clipboard 内容，插入后恢复
                        IDataObject? clipboardBackup = null;
                        try
                        {
                            if (Clipboard.ContainsData(DataFormats.Bitmap))
                            {
                                clipboardBackup = Clipboard.GetDataObject();
                            }

                            Clipboard.SetImage(thumbnail);
                            rtbMessages.Paste();

                            // 🔥 恢复 Clipboard（如果之前有内容）
                            if (clipboardBackup != null)
                            {
                                Clipboard.SetDataObject(clipboardBackup);
                            }
                            else
                            {
                                Clipboard.Clear();
                            }
                        }
                        catch (Exception clipEx)
                        {
                            _logService.Warning("MessageSimulator", $"Clipboard 操作失败，尝试直接显示路径: {clipEx.Message}");
                            // 🔥 如果 Clipboard 操作失败，回退到显示路径
                            rtbMessages.SelectionColor = Color.FromArgb(155, 89, 182);
                            rtbMessages.SelectionFont = new Font(rtbMessages.Font.FontFamily, 10);
                            rtbMessages.AppendText($"  {imagePath}\n");
                        }
                    }
                }

                // 🔥 添加换行和图片信息
                rtbMessages.AppendText("\n");

                // 🔥 图片路径和描述信息（深绿色，更醒目）
                rtbMessages.SelectionAlignment = HorizontalAlignment.Left;
                rtbMessages.SelectionColor = Color.FromArgb(46, 125, 50); // 深绿色
                rtbMessages.SelectionFont = new Font(rtbMessages.Font.FontFamily, 8, FontStyle.Bold | FontStyle.Italic);
                
                string infoText = $"((消息类型: 图片";
                if (!string.IsNullOrEmpty(description))
                {
                    infoText += $", 描述: {description}";
                }
                infoText += $", 文件名: {Path.GetFileName(imagePath)}))";
                rtbMessages.AppendText($"  {infoText}\n\n");

                rtbMessages.ScrollToCaret();
            }
            catch (Exception ex)
            {
                _logService.Error("MessageSimulator", "显示图片消息失败", ex);
                AppendErrorMessage($"((显示图片失败: {ex.Message}))");
            }
        }

        #endregion

        #region 消息显示

        /// <summary>
        /// 🔥 追加会员消息（右对齐，绿色）
        /// </summary>
        private void AppendMemberMessage(string nickname, string message)
        {
            try
            {
                // 时间戳（右对齐，灰色小字）
                rtbMessages.SelectionAlignment = HorizontalAlignment.Right;
                rtbMessages.SelectionColor = Color.Gray;
                rtbMessages.SelectionFont = new Font(rtbMessages.Font.FontFamily, 8);
                rtbMessages.AppendText($"[{DateTime.Now:HH:mm:ss}] {nickname}\n");

                // 消息内容（右对齐，深绿色）
                rtbMessages.SelectionAlignment = HorizontalAlignment.Right;
                rtbMessages.SelectionColor = Color.FromArgb(39, 174, 96);  // 绿色
                rtbMessages.SelectionFont = new Font(rtbMessages.Font.FontFamily, 10, FontStyle.Bold);
                rtbMessages.AppendText($"  {message}\n\n");

                // 滚动到底部
                rtbMessages.ScrollToCaret();
            }
            catch (Exception ex)
            {
                _logService.Error("MessageSimulator", "追加会员消息失败", ex);
            }
        }

        /// <summary>
        /// 🔥 追加系统回复（左对齐，灰色）
        /// </summary>
        private void AppendSystemMessage(string message)
        {
            try
            {
                // 时间戳（左对齐，灰色小字）
                rtbMessages.SelectionAlignment = HorizontalAlignment.Left;
                rtbMessages.SelectionColor = Color.Gray;
                rtbMessages.SelectionFont = new Font(rtbMessages.Font.FontFamily, 8);
                rtbMessages.AppendText($"[{DateTime.Now:HH:mm:ss}] 系统回复\n");

                // 消息内容（左对齐，黑色）
                rtbMessages.SelectionAlignment = HorizontalAlignment.Left;
                rtbMessages.SelectionColor = Color.Black;
                rtbMessages.SelectionFont = new Font(rtbMessages.Font.FontFamily, 10);
                rtbMessages.AppendText($"  {message}\n\n");

                // 滚动到底部
                rtbMessages.ScrollToCaret();
            }
            catch (Exception ex)
            {
                _logService.Error("MessageSimulator", "追加系统消息失败", ex);
            }
        }

        /// <summary>
        /// 🔥 追加错误消息（左对齐，红色）
        /// </summary>
        private void AppendErrorMessage(string message)
        {
            try
            {
                // 时间戳（左对齐，红色小字）
                rtbMessages.SelectionAlignment = HorizontalAlignment.Left;
                rtbMessages.SelectionColor = Color.Red;
                rtbMessages.SelectionFont = new Font(rtbMessages.Font.FontFamily, 8, FontStyle.Bold);
                rtbMessages.AppendText($"[{DateTime.Now:HH:mm:ss}] ⚠️ 错误\n");

                // 消息内容（左对齐，深红色）
                rtbMessages.SelectionAlignment = HorizontalAlignment.Left;
                rtbMessages.SelectionColor = Color.DarkRed;
                rtbMessages.SelectionFont = new Font(rtbMessages.Font.FontFamily, 10);
                rtbMessages.AppendText($"  {message}\n\n");

                // 滚动到底部
                rtbMessages.ScrollToCaret();
            }
            catch (Exception ex)
            {
                _logService.Error("MessageSimulator", "追加错误消息失败", ex);
            }
        }

        /// <summary>
        /// 🔥 追加系统通知消息（左对齐，自定义颜色）
        /// 🔥 格式：实际消息内容（与微信完全一致）+ ((额外信息))
        /// 
        /// ⚠️ 注意：此方法仅用于在消息模拟器中显示消息，不会影响实际发送到微信的消息内容
        /// ⚠️ 微信消息使用 \r 换行符，但 RichTextBox 控件需要 \n 才能正确显示换行
        /// ⚠️ 因此这里只在显示时转换换行符，原始消息内容保持不变
        /// </summary>
        private void AppendSystemNotification(string message, Color color)
        {
            try
            {
                // 🔥 调试日志：记录接收到的消息
                _logService.Debug("MessageSimulator", $"AppendSystemNotification 收到消息，长度: {message?.Length ?? 0}");
                
                // 时间戳（左对齐，灰色小字）
                rtbMessages.SelectionAlignment = HorizontalAlignment.Left;
                rtbMessages.SelectionColor = Color.Gray;
                rtbMessages.SelectionFont = new Font(rtbMessages.Font.FontFamily, 8);
                rtbMessages.AppendText($"[{DateTime.Now:HH:mm:ss}] 系统消息\n");

                // 🔥 统一处理换行符：将 \r\n 和 \r 都转换为 \n，然后分割
                // ⚠️ 注意：这只是为了在 RichTextBox 中正确显示，不会影响原始消息内容
                // ⚠️ 实际发送到微信的消息仍然使用原始的 \r 格式
                if (string.IsNullOrEmpty(message))
                {
                    _logService.Warning("MessageSimulator", "⚠️ 消息内容为空！");
                    rtbMessages.AppendText("  (消息内容为空)\n\n");
                    rtbMessages.ScrollToCaret();
                    return;
                }
                
                string normalizedMessage = message.Replace("\r\n", "\n").Replace("\r", "\n");
                
                // 🔥 调试日志：记录转换后的消息
                _logService.Debug("MessageSimulator", $"转换后消息，长度: {normalizedMessage.Length}, 行数: {normalizedMessage.Split('\n').Length}");
                _logService.Debug("MessageSimulator", $"消息前100字符: {normalizedMessage.Substring(0, Math.Min(100, normalizedMessage.Length))}");
                
                // 分离实际消息和额外信息（用 ((...)) 包裹的部分）
                // 🔥 使用 StringSplitOptions.None 保留空行，以便正确显示格式
                string[] lines = normalizedMessage.Split(new[] { '\n' }, StringSplitOptions.None);
                bool hasActualMessage = false;
                int actualMessageCount = 0;
                int extraInfoCount = 0;
                
                foreach (var line in lines)
                {
                    // 🔥 去除尾部空白，但保留前导空白（用于缩进）
                    string trimmedLine = line.TrimEnd();
                    
                    // 🔥 空行也显示，保持消息格式
                    if (string.IsNullOrWhiteSpace(trimmedLine))
                    {
                        rtbMessages.AppendText("\n");
                        continue;
                    }
                        
                    // 🔥 检查是否是额外信息（用 ((...)) 包裹）
                    // 🔥 注意：需要去除首尾空白后再检查，因为可能有前导空格
                    string checkLine = trimmedLine.Trim();
                    bool isExtraInfo = checkLine.StartsWith("((") && checkLine.EndsWith("))");
                    
                    if (isExtraInfo)
                    {
                        // 🔥 额外信息用绿色显示，更醒目
                        rtbMessages.SelectionAlignment = HorizontalAlignment.Left;
                        rtbMessages.SelectionColor = Color.FromArgb(46, 125, 50); // 深绿色，更醒目
                        rtbMessages.SelectionFont = new Font(rtbMessages.Font.FontFamily, 8, FontStyle.Bold | FontStyle.Italic);
                        rtbMessages.AppendText($"  {trimmedLine}\n");
                        extraInfoCount++;
                        _logService.Debug("MessageSimulator", $"识别为额外信息: {checkLine}");
                    }
                    else
                    {
                        // 实际消息内容用正常颜色显示（与微信一致）
                        rtbMessages.SelectionAlignment = HorizontalAlignment.Left;
                        rtbMessages.SelectionColor = Color.Black;  // 实际消息用黑色，与微信一致
                        rtbMessages.SelectionFont = new Font(rtbMessages.Font.FontFamily, 10);
                        rtbMessages.AppendText($"  {trimmedLine}\n");
                        hasActualMessage = true;
                        actualMessageCount++;
                        string preview = trimmedLine.Length > 50 ? trimmedLine.Substring(0, 50) + "..." : trimmedLine;
                        _logService.Debug("MessageSimulator", $"识别为实际消息: {preview}");
                    }
                }
                
                // 🔥 调试日志：记录解析结果
                _logService.Debug("MessageSimulator", $"解析完成 - 实际消息行数: {actualMessageCount}, 额外信息行数: {extraInfoCount}, hasActualMessage: {hasActualMessage}");
                
                // 🔥 如果没有实际消息（只有额外信息），说明可能解析有问题，直接显示原始消息
                if (!hasActualMessage)
                {
                    _logService.Warning("MessageSimulator", $"⚠️ 没有识别到实际消息内容，只有额外信息！消息内容: {normalizedMessage.Substring(0, Math.Min(200, normalizedMessage.Length))}");
                    // 🔥 如果只有额外信息，说明解析有问题，直接显示原始消息（去除额外信息）
                    // 这种情况不应该发生，但为了保险起见，还是显示原始消息
                    rtbMessages.SelectionAlignment = HorizontalAlignment.Left;
                    rtbMessages.SelectionColor = Color.Black;
                    rtbMessages.SelectionFont = new Font(rtbMessages.Font.FontFamily, 10);
                    // 🔥 移除所有 ((...)) 行，只显示实际消息
                    // 注意：逻辑是：如果一行同时以 (( 开头且以 )) 结尾，则不显示（这是额外信息）
                    // 否则显示（这是实际消息）
                    var actualLines = lines.Where(l => {
                        string check = l.Trim();
                        // 如果同时以 (( 开头且以 )) 结尾，则是额外信息，返回 false（不显示）
                        // 否则是实际消息，返回 true（显示）
                        return !(check.StartsWith("((") && check.EndsWith("))"));
                    });
                    foreach (var actualLine in actualLines)
                    {
                        string trimmed = actualLine.TrimEnd();
                        if (!string.IsNullOrWhiteSpace(trimmed))
                        {
                            rtbMessages.AppendText($"  {trimmed}\n");
                        }
                    }
                }
                
                rtbMessages.AppendText("\n");

                // 滚动到底部
                rtbMessages.ScrollToCaret();
            }
            catch (Exception ex)
            {
                _logService.Error("MessageSimulator", $"追加系统通知失败: {ex.Message}", ex);
                _logService.Error("MessageSimulator", $"异常堆栈: {ex.StackTrace}");
                // 🔥 如果解析失败，直接显示原始消息
                try
                {
                    rtbMessages.SelectionAlignment = HorizontalAlignment.Left;
                    rtbMessages.SelectionColor = Color.Black;
                    rtbMessages.SelectionFont = new Font(rtbMessages.Font.FontFamily, 10);
                    rtbMessages.AppendText($"  {message}\n\n");
                    rtbMessages.ScrollToCaret();
                }
                catch (Exception ex2)
                {
                    _logService.Error("MessageSimulator", $"显示原始消息也失败: {ex2.Message}");
                }
            }
        }

        #endregion

        #region 窗口生命周期

        /// <summary>
        /// 🔥 窗口关闭时清理资源
        /// </summary>
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            UnsubscribeFromSystemMessages();
            UnsubscribeFromStaticNotifications();
            base.OnFormClosed(e);
        }

        #endregion
    }
}

