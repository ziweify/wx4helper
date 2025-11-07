using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using BaiShengVx3Plus.Contracts;
using BaiShengVx3Plus.Models;
using SQLite;
using Sunny.UI;

namespace BaiShengVx3Plus.Views
{
    /// <summary>
    /// 上下分管理窗口
    /// 管理员处理会员的上下分申请
    /// </summary>
    public partial class CreditWithdrawManageForm : UIForm
    {
        private readonly SQLiteConnection _db;
        private readonly ILogService _logService;
        private readonly IWeixinSocketClient _socketClient;
        private List<V2CreditWithdraw> _allRequests = new List<V2CreditWithdraw>();
        private List<V2CreditWithdraw> _filteredRequests = new List<V2CreditWithdraw>();

        public CreditWithdrawManageForm(SQLiteConnection db, ILogService logService, IWeixinSocketClient socketClient)
        {
            _db = db;
            _logService = logService;
            _socketClient = socketClient;
            
            InitializeComponent();
            
            // 初始化下拉框
            InitializeComboBox();
            
            // 配置DataGridView
            ConfigureDataGridView();
            
            // 加载数据
            LoadData();
        }

        /// <summary>
        /// 初始化状态下拉框
        /// </summary>
        private void InitializeComboBox()
        {
            cmbStatus.Items.Clear();
            cmbStatus.Items.Add("全部状态");
            cmbStatus.Items.Add("等待处理");
            cmbStatus.Items.Add("已同意");
            cmbStatus.Items.Add("已拒绝");
            cmbStatus.SelectedIndex = 1;  // 默认显示"等待处理"
        }

        /// <summary>
        /// 配置DataGridView列
        /// </summary>
        private void ConfigureDataGridView()
        {
            dgvRequests.AutoGenerateColumns = false;
            dgvRequests.Columns.Clear();
            
            // 基础信息列
            dgvRequests.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn 
                { 
                    DataPropertyName = "Id", 
                    HeaderText = "ID", 
                    Width = 50 
                },
                new DataGridViewTextBoxColumn 
                { 
                    DataPropertyName = "TimeString", 
                    HeaderText = "申请时间", 
                    Width = 140 
                },
                new DataGridViewTextBoxColumn 
                { 
                    DataPropertyName = "Nickname", 
                    HeaderText = "昵称", 
                    Width = 100 
                },
                new DataGridViewTextBoxColumn 
                { 
                    DataPropertyName = "ActionText", 
                    HeaderText = "动作", 
                    Width = 70 
                },
                new DataGridViewTextBoxColumn 
                { 
                    DataPropertyName = "Amount", 
                    HeaderText = "金额", 
                    Width = 90,
                    DefaultCellStyle = new DataGridViewCellStyle 
                    { 
                        Format = "F2", 
                        Alignment = DataGridViewContentAlignment.MiddleRight 
                    } 
                },
                new DataGridViewTextBoxColumn 
                { 
                    DataPropertyName = "StatusText", 
                    HeaderText = "状态", 
                    Width = 80 
                },
                new DataGridViewTextBoxColumn 
                { 
                    DataPropertyName = "ProcessedBy", 
                    HeaderText = "处理人", 
                    Width = 90 
                },
                new DataGridViewTextBoxColumn 
                { 
                    DataPropertyName = "ProcessedTime", 
                    HeaderText = "处理时间", 
                    Width = 140 
                },
                new DataGridViewTextBoxColumn 
                { 
                    DataPropertyName = "Notes", 
                    HeaderText = "备注", 
                    Width = 120,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill 
                }
            });
            
            // 🔥 添加操作按钮列（同意、拒绝）
            var btnAgreeColumn = new DataGridViewButtonColumn
            {
                Name = "btnAgree",
                HeaderText = "操作",
                Text = "同意",
                UseColumnTextForButtonValue = false,
                Width = 60
            };
            dgvRequests.Columns.Add(btnAgreeColumn);
            
            var btnRejectColumn = new DataGridViewButtonColumn
            {
                Name = "btnReject",
                HeaderText = "",
                Text = "拒绝",
                UseColumnTextForButtonValue = false,
                Width = 60
            };
            dgvRequests.Columns.Add(btnRejectColumn);
            
            // 🔥 单元格点击事件（处理按钮点击）
            dgvRequests.CellContentClick += DgvRequests_CellContentClick;
            
            // 🔥 单元格格式化（按钮可见性控制）
            dgvRequests.CellFormatting += DgvRequests_CellFormatting;
        }

        /// <summary>
        /// 单元格格式化（控制按钮显示）
        /// </summary>
        private void DgvRequests_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvRequests.Columns[e.ColumnIndex].Name == "btnAgree" || 
                dgvRequests.Columns[e.ColumnIndex].Name == "btnReject")
            {
                if (e.RowIndex >= 0 && e.RowIndex < _filteredRequests.Count)
                {
                    var request = _filteredRequests[e.RowIndex];
                    
                    // 只有"等待处理"状态才显示按钮
                    if (request.Status != CreditWithdrawStatus.等待处理)
                    {
                        e.Value = "";  // 隐藏按钮文本
                    }
                    else
                    {
                        e.Value = dgvRequests.Columns[e.ColumnIndex].Name == "btnAgree" ? "同意" : "拒绝";
                    }
                }
            }
        }

        /// <summary>
        /// 单元格点击事件（处理按钮点击）
        /// </summary>
        private void DgvRequests_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _filteredRequests.Count)
                return;
            
            var request = _filteredRequests[e.RowIndex];
            
            // 只有"等待处理"状态才能操作
            if (request.Status != CreditWithdrawStatus.等待处理)
            {
                UIMessageBox.ShowWarning("该申请已处理，无法再次操作");
                return;
            }
            
            if (dgvRequests.Columns[e.ColumnIndex].Name == "btnAgree")
            {
                // 同意
                ApproveRequest(request);
            }
            else if (dgvRequests.Columns[e.ColumnIndex].Name == "btnReject")
            {
                // 拒绝
                RejectRequest(request);
            }
        }

        /// <summary>
        /// 同意申请
        /// </summary>
        private void ApproveRequest(V2CreditWithdraw request)
        {
            try
            {
                string actionName = request.Action == CreditWithdrawAction.上分 ? "上分" : "下分";
                
                if (!UIMessageBox.ShowAsk($"确定同意【{request.Nickname}】的{actionName}申请吗？\n\n金额：{request.Amount:F2}"))
                {
                    return;
                }
                
                // 🔥 查找会员
                var member = _db.Table<V2Member>()
                    .FirstOrDefault(m => m.Wxid == request.Wxid && m.GroupWxId == request.GroupWxId);
                
                if (member == null)
                {
                    UIMessageBox.ShowError("未找到该会员");
                    return;
                }
                
                float balanceBefore = member.Balance;
                float balanceAfter;
                
                if (request.Action == CreditWithdrawAction.上分)
                {
                    // 🔥 上分处理
                    balanceAfter = balanceBefore + request.Amount;
                    member.Balance = balanceAfter;
                    member.CreditToday += request.Amount;
                    member.CreditTotal += request.Amount;
                }
                else
                {
                    // 🔥 下分处理（再次检查余额）
                    if (member.Balance < request.Amount)
                    {
                        // 🔥 参考 F5BotV2 第467行：存储不足的回复
                        string errorMsg = $"@{member.Nickname} 存储不足!";
                        _ = _socketClient.SendAsync<object>("SendMessage", member.GroupWxId, errorMsg);
                        
                        UIMessageBox.ShowError($"会员余额不足！\n当前余额：{member.Balance:F2}\n申请金额：{request.Amount:F2}");
                        return;
                    }
                    
                    balanceAfter = balanceBefore - request.Amount;
                    member.Balance = balanceAfter;
                    member.WithdrawToday += request.Amount;
                    member.WithdrawTotal += request.Amount;
                }
                
                // 🔥 更新申请状态
                request.Status = CreditWithdrawStatus.已同意;
                request.ProcessedBy = Services.Api.BoterApi.GetInstance().User;
                request.ProcessedTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                
                // 🔥 记录到资金变动表
                var balanceChange = new V2BalanceChange
                {
                    GroupWxId = member.GroupWxId,
                    Wxid = member.Wxid,
                    Nickname = member.Nickname,
                    BalanceBefore = balanceBefore,
                    BalanceAfter = balanceAfter,
                    ChangeAmount = request.Action == CreditWithdrawAction.上分 ? request.Amount : -request.Amount,
                    Reason = request.Action == CreditWithdrawAction.上分 ? ChangeReason.上分 : ChangeReason.下分,
                    IssueId = 0,
                    TimeString = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Timestamp = DateTimeOffset.Now.ToUnixTimeSeconds(),
                    Notes = $"管理员同意{actionName}申请"
                };
                
                // 🔥 保存到数据库
                _db.Update(member);
                _db.Update(request);
                _db.Insert(balanceChange);
                
                // 🔥 发送微信通知（参考 F5BotV2 第433行和第478行）
                string notifyMessage = $"@{member.Nickname}\r[{member.Id}]{actionName}{(int)request.Amount}完成|余:{(int)member.Balance}";
                
                _ = _socketClient.SendAsync<object>("SendMessage", member.GroupWxId, notifyMessage);
                
                // 🔥 日志记录
                _logService.Info("上下分管理", 
                    $"同意{actionName}申请\n" +
                    $"会员：{member.Nickname}\n" +
                    $"金额：{request.Amount:F2}\n" +
                    $"变动前：{balanceBefore:F2}\n" +
                    $"变动后：{balanceAfter:F2}\n" +
                    $"处理人：{request.ProcessedBy}");
                
                // 刷新列表
                LoadData();
                
                this.ShowSuccessTip($"已同意{actionName}申请");
            }
            catch (Exception ex)
            {
                _logService.Error("上下分管理", "同意申请失败", ex);
                UIMessageBox.ShowError($"处理失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 拒绝申请
        /// </summary>
        private void RejectRequest(V2CreditWithdraw request)
        {
            try
            {
                string actionName = request.Action == CreditWithdrawAction.上分 ? "上分" : "下分";
                
                if (!UIMessageBox.ShowAsk($"确定拒绝【{request.Nickname}】的{actionName}申请吗？\n\n金额：{request.Amount:F2}"))
                {
                    return;
                }
                
                // 🔥 更新申请状态
                request.Status = CreditWithdrawStatus.已拒绝;
                request.ProcessedBy = Services.Api.BoterApi.GetInstance().User;
                request.ProcessedTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                request.Notes = "管理员拒绝";
                
                // 🔥 保存到数据库
                _db.Update(request);
                
                // 🔥 发送微信通知
                // 注意：F5BotV2没有拒绝功能的专门消息，这里保持简单提示
                string notifyMessage = $"@{request.Nickname} {actionName}申请已被管理员拒绝";
                
                _ = _socketClient.SendAsync<object>("SendMessage", request.GroupWxId, notifyMessage);
                
                // 🔥 日志记录
                _logService.Info("上下分管理", 
                    $"拒绝{actionName}申请\n" +
                    $"会员：{request.Nickname}\n" +
                    $"金额：{request.Amount:F2}\n" +
                    $"处理人：{request.ProcessedBy}");
                
                // 刷新列表
                LoadData();
                
                this.ShowSuccessTip($"已拒绝{actionName}申请");
            }
            catch (Exception ex)
            {
                _logService.Error("上下分管理", "拒绝申请失败", ex);
                UIMessageBox.ShowError($"处理失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 加载数据
        /// </summary>
        private void LoadData()
        {
            try
            {
                // 🔥 确保表存在
                _db.CreateTable<V2CreditWithdraw>();
                
                // 加载所有申请
                _allRequests = _db.Table<V2CreditWithdraw>()
                    .OrderByDescending(r => r.Timestamp)
                    .ToList();
                
                _logService.Info("上下分管理", $"加载了 {_allRequests.Count} 条申请记录");
                
                ApplyFilter();
            }
            catch (Exception ex)
            {
                _logService.Error("上下分管理", "加载数据失败", ex);
                UIMessageBox.ShowError($"加载数据失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 应用筛选
        /// </summary>
        private void ApplyFilter()
        {
            int statusIndex = cmbStatus.SelectedIndex;
            
            _filteredRequests = _allRequests.Where(r =>
            {
                // 状态筛选
                if (statusIndex > 0)
                {
                    CreditWithdrawStatus targetStatus = statusIndex switch
                    {
                        1 => CreditWithdrawStatus.等待处理,
                        2 => CreditWithdrawStatus.已同意,
                        3 => CreditWithdrawStatus.已拒绝,
                        _ => CreditWithdrawStatus.等待处理
                    };
                    
                    if (r.Status != targetStatus)
                    {
                        return false;
                    }
                }
                
                return true;
            }).ToList();
            
            RefreshGrid();
        }

        /// <summary>
        /// 刷新DataGridView
        /// </summary>
        private void RefreshGrid()
        {
            dgvRequests.DataSource = null;
            dgvRequests.DataSource = _filteredRequests;
            
            UpdateStats();
        }

        /// <summary>
        /// 更新统计信息
        /// </summary>
        private void UpdateStats()
        {
            int pendingCount = _allRequests.Count(r => r.Status == CreditWithdrawStatus.等待处理);
            
            // 今日上分和下分（已同意的）
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            float todayCredit = _allRequests
                .Where(r => r.Status == CreditWithdrawStatus.已同意 && 
                           r.Action == CreditWithdrawAction.上分 &&
                           r.TimeString.StartsWith(today))
                .Sum(r => r.Amount);
            
            float todayWithdraw = _allRequests
                .Where(r => r.Status == CreditWithdrawStatus.已同意 && 
                           r.Action == CreditWithdrawAction.下分 &&
                           r.TimeString.StartsWith(today))
                .Sum(r => r.Amount);
            
            lblStats.Text = $"待处理: {pendingCount} 笔 | 今日上分: {todayCredit:F2} | 今日下分: {todayWithdraw:F2}";
        }

        /// <summary>
        /// 状态筛选变化
        /// </summary>
        private void CmbStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyFilter();
        }

        /// <summary>
        /// 刷新按钮点击
        /// </summary>
        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadData();
        }
    }
}

