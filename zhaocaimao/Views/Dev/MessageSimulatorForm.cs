using zhaocaimao.Contracts;
using zhaocaimao.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace zhaocaimao.Views.Dev
{
    /// <summary>
    /// 💬 消息模拟器窗口
    /// 
    /// 功能：
    /// 1. 模拟微信聊天界面
    /// 2. 以会员身份发送测试消息
    /// 3. 显示系统回复
    /// 4. 支持所有消息类型（投注、上下分、管理命令等）
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
        /// 🔥 静态工厂方法：获取或创建窗口（单例）
        /// </summary>
        /// <param name="member">会员信息</param>
        /// <param name="simulateMessageFunc">模拟消息方法</param>
        /// <param name="logService">日志服务</param>
        /// <returns>窗口实例</returns>
        public static MessageSimulatorForm GetOrCreate(
            V2Member member,
            Func<string, string, Task<(bool success, string? replyMessage, string? errorMessage)>> simulateMessageFunc,
            ILogService logService)
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
            var newForm = new MessageSimulatorForm(member, simulateMessageFunc, logService);
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
        private bool _isSending = false;

        #endregion

        #region 构造函数

        /// <summary>
        /// 🔥 私有构造函数（通过静态工厂方法创建）
        /// </summary>
        private MessageSimulatorForm(
            V2Member member,
            Func<string, string, Task<(bool success, string? replyMessage, string? errorMessage)>> simulateMessageFunc,
            ILogService logService)
        {
            InitializeComponent();

            _member = member;
            _simulateMessageFunc = simulateMessageFunc;
            _logService = logService;

            InitializeUI();
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

        #endregion
    }
}

