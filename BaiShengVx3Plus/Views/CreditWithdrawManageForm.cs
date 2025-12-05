using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        private readonly Core.V2CreditWithdrawBindingList _creditWithdrawsBindingList;  // 🔥 使用 BindingList（统一模式）
        private readonly Core.V2MemberBindingList _membersBindingList;  // 🔥 会员列表引用
        private readonly Services.Games.Binggo.CreditWithdrawService _creditWithdrawService;  // 🔥 上下分服务
        private BindingSource _bindingSource;  // 🔥 使用 BindingSource 处理过滤和自动更新
        private int _hoverRowIndex = -1;  // 🔥 跟踪鼠标悬停的行索引

        public CreditWithdrawManageForm(
            SQLiteConnection db, 
            ILogService logService, 
            IWeixinSocketClient socketClient,
            Core.V2CreditWithdrawBindingList creditWithdrawsBindingList,
            Core.V2MemberBindingList membersBindingList,
            Services.Games.Binggo.CreditWithdrawService creditWithdrawService)
        {
            _db = db;
            _logService = logService;
            _socketClient = socketClient;
            _creditWithdrawsBindingList = creditWithdrawsBindingList;  // 🔥 接收 BindingList
            _membersBindingList = membersBindingList;  // 🔥 接收会员列表
            _creditWithdrawService = creditWithdrawService;  // 🔥 接收上下分服务
            
            // 🔥 确保资金变动表存在（修复 "no such table: V2BalanceChange" 错误）
            _db.CreateTable<V2BalanceChange>();
            
            InitializeComponent();
            
            // 🔥 创建 BindingSource 并绑定到 BindingList（标准做法）
            _bindingSource = new BindingSource
            {
                DataSource = _creditWithdrawsBindingList  // 🔥 直接绑定到 BindingList，自动更新
            };
            
            // 初始化下拉框
            InitializeComboBox();
            
            // 配置DataGridView
            ConfigureDataGridView();
            
            // 🔥 直接绑定到 BindingSource（自动更新，无需手动刷新）
            dgvRequests.DataSource = _bindingSource;
            
            // 🔥 添加 DataError 处理，使用非模态通知窗口显示错误
            dgvRequests.DataError += (s, e) => 
            {
                string errorMsg = e.Exception?.Message ?? "未知错误";
                _logService.Warning("上下分管理", $"DataGridView 数据错误: {errorMsg}");
                
                // 🔥 使用非模态通知窗口（单例，不阻塞主程序）
                DataErrorNotifyForm.ShowError("上下分管理", errorMsg);
                
                e.ThrowException = false;
            };
            
            // 🔥 应用默认筛选（等待处理）
            ApplyFilter();
            
            // 🔥 更新统计信息
            UpdateStats();
            
            // 🔥 注册 RowPostPaint、CellPainting 和鼠标事件（美化效果）
            dgvRequests.RowPostPaint += DgvRequests_RowPostPaint;  // 🔥 在单元格绘制后绘制蒙板
            dgvRequests.CellPainting += DgvRequests_CellPainting;
            dgvRequests.CellMouseEnter += DgvRequests_CellMouseEnter;
            dgvRequests.CellMouseLeave += DgvRequests_CellMouseLeave;
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
            cmbStatus.Items.Add("忽略");
            cmbStatus.SelectedIndex = 1;  // 默认显示"等待处理"
        }
        
        /// <summary>
        /// 🔥 格式化枚举列显示（显示中文而不是数字）
        /// </summary>
        private void DgvRequests_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            try
            {
                // 格式化"动作"列（Action 枚举）
                if (dgvRequests.Columns[e.ColumnIndex].DataPropertyName == "Action" && e.Value != null)
                {
                    if (e.Value is CreditWithdrawAction action)
                    {
                        e.Value = action switch
                        {
                            CreditWithdrawAction.上分 => "上分",
                            CreditWithdrawAction.下分 => "下分",
                            _ => "未知"
                        };
                        e.FormattingApplied = true;
                    }
                }
                
                // 格式化"状态"列（Status 枚举）
                if (dgvRequests.Columns[e.ColumnIndex].DataPropertyName == "Status" && e.Value != null)
                {
                    if (e.Value is CreditWithdrawStatus status)
                    {
                        e.Value = status switch
                        {
                            CreditWithdrawStatus.等待处理 => "等待处理",
                            CreditWithdrawStatus.已同意 => "已同意",
                            CreditWithdrawStatus.已拒绝 => "已拒绝",
                            CreditWithdrawStatus.忽略 => "忽略",
                            _ => "未知"
                        };
                        e.FormattingApplied = true;
                    }
                }
            }
            catch (Exception ex)
            {
                _logService?.Error("CreditWithdrawManageForm", "格式化单元格失败", ex);
            }
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
                    DataPropertyName = "Action", 
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
                    DataPropertyName = "Status", 
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
            
            // 🔥 设置枚举列的格式化
            dgvRequests.CellFormatting += DgvRequests_CellFormatting;
            
            // 🔥 添加操作按钮列（同意、忽略、拒绝）- 参考 F5BotV2 Line 82-104
            var btnAgreeColumn = new DataGridViewButtonColumn
            {
                Name = "btnAgree",
                HeaderText = "",
                Text = "同意",
                UseColumnTextForButtonValue = true,  // 🔥 使用按钮文本值
                Width = 50,
                DefaultCellStyle = new DataGridViewCellStyle 
                { 
                    BackColor = Color.Green, 
                    ForeColor = Color.White,
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                }
            };
            dgvRequests.Columns.Insert(0, btnAgreeColumn);  // 🔥 插入到第一列
            
            var btnIgnoreColumn = new DataGridViewButtonColumn
            {
                Name = "btnIgnore",
                HeaderText = "",
                Text = "忽略",
                UseColumnTextForButtonValue = true,  // 🔥 使用按钮文本值
                Width = 50,
                DefaultCellStyle = new DataGridViewCellStyle 
                { 
                    BackColor = Color.LightGray, 
                    ForeColor = Color.Black,
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                }
            };
            dgvRequests.Columns.Insert(1, btnIgnoreColumn);  // 🔥 插入到第二列
            
            var btnRejectColumn = new DataGridViewButtonColumn
            {
                Name = "btnReject",
                HeaderText = "",
                Text = "拒绝",
                UseColumnTextForButtonValue = true,  // 🔥 使用按钮文本值
                Width = 50,
                DefaultCellStyle = new DataGridViewCellStyle 
                { 
                    BackColor = Color.Red, 
                    ForeColor = Color.White,
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                }
            };
            dgvRequests.Columns.Insert(2, btnRejectColumn);  // 🔥 插入到第三列
            
            // 🔥 单元格点击事件（处理按钮点击）
            dgvRequests.CellContentClick += DgvRequests_CellContentClick;
            
            // 🔥 不设置 SelectionBackColor 为透明（会变黑色）
            // 让 CellPainting 控制单元格颜色，RowPostPaint 绘制选中蒙板
            
            // 🔥 设置只读，不允许直接修改数据
            dgvRequests.ReadOnly = false;  // 不能整个设为只读，按钮列需要点击
            dgvRequests.AllowUserToAddRows = false;
            dgvRequests.AllowUserToDeleteRows = false;
            dgvRequests.EditMode = DataGridViewEditMode.EditProgrammatically;  // 🔥 只允许程序编辑，不允许用户双击编辑
            
            // 🔥 将所有非按钮列设置为只读
            foreach (DataGridViewColumn column in dgvRequests.Columns)
            {
                if (!(column is DataGridViewButtonColumn))
                {
                    column.ReadOnly = true;  // 🔥 非按钮列全部只读
                }
            }
        }

        /// <summary>
        /// 🔥 行后绘制：在单元格绘制后绘制选中/Hover 蒙板（覆盖在列颜色之上）
        /// </summary>
        private void DgvRequests_RowPostPaint(object? sender, DataGridViewRowPostPaintEventArgs e)
        {
            if (e.RowIndex < 0 || e.Graphics == null) return;
            
            bool isSelected = dgvRequests.Rows[e.RowIndex].Selected;
            bool isHover = (e.RowIndex == _hoverRowIndex);
            
            // 🔥 只在选中或 Hover 时绘制蒙板（覆盖在原有颜色之上）
            if (isSelected)
            {
                // 绘制半透明蓝色蒙板
                e.Graphics.FillRectangle(
                    new SolidBrush(Color.FromArgb(50, 80, 160, 255)),
                    e.RowBounds);
                
                // 绘制蓝色边框
                using (Pen pen = new Pen(Color.FromArgb(80, 160, 255), 2))
                {
                    e.Graphics.DrawRectangle(pen, 
                        e.RowBounds.X, 
                        e.RowBounds.Y, 
                        e.RowBounds.Width - 1, 
                        e.RowBounds.Height - 1);
                }
            }
            else if (isHover)
            {
                // 绘制半透明淡黄色蒙板
                e.Graphics.FillRectangle(
                    new SolidBrush(Color.FromArgb(30, 255, 235, 150)),
                    e.RowBounds);
            }
        }

        /// <summary>
        /// 🔥 自定义单元格绘制：设置列颜色（保留旧版样式）
        /// </summary>
        private void DgvRequests_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _bindingSource.Count) return;
            
            var request = _bindingSource[e.RowIndex] as V2CreditWithdraw;
            if (request == null) return;
            
            var column = dgvRequests.Columns[e.ColumnIndex];
            var row = dgvRequests.Rows[e.RowIndex];
            
            // 🔥 1. 动作列颜色
            if (column.DataPropertyName == "Action")
            {
                if (request.Action == CreditWithdrawAction.上分)
                {
                    row.Cells[e.ColumnIndex].Style.BackColor = Color.Green;
                    row.Cells[e.ColumnIndex].Style.SelectionBackColor = Color.Green;  // 🔥 选中时也保持绿色
                    row.Cells[e.ColumnIndex].Style.ForeColor = Color.Black;
                    row.Cells[e.ColumnIndex].Style.SelectionForeColor = Color.Black;
                }
                else if (request.Action == CreditWithdrawAction.下分)
                {
                    row.Cells[e.ColumnIndex].Style.BackColor = Color.Red;
                    row.Cells[e.ColumnIndex].Style.SelectionBackColor = Color.Red;  // 🔥 选中时也保持红色
                    row.Cells[e.ColumnIndex].Style.ForeColor = Color.White;
                    row.Cells[e.ColumnIndex].Style.SelectionForeColor = Color.White;
                }
            }
            
            // 🔥 2. 状态列颜色
            if (column.DataPropertyName == "Status")
            {
                if (request.Status == CreditWithdrawStatus.等待处理)
                {
                    row.Cells[e.ColumnIndex].Style.BackColor = Color.Red;
                    row.Cells[e.ColumnIndex].Style.SelectionBackColor = Color.Red;  // 🔥 选中时也保持红色
                    row.Cells[e.ColumnIndex].Style.ForeColor = Color.White;
                    row.Cells[e.ColumnIndex].Style.SelectionForeColor = Color.White;
                }
                else if (request.Status == CreditWithdrawStatus.已同意)
                {
                    row.Cells[e.ColumnIndex].Style.BackColor = Color.Green;
                    row.Cells[e.ColumnIndex].Style.SelectionBackColor = Color.Green;  // 🔥 选中时也保持绿色
                    row.Cells[e.ColumnIndex].Style.ForeColor = Color.White;
                    row.Cells[e.ColumnIndex].Style.SelectionForeColor = Color.White;
                    
                    // 🔥 禁用按钮（已处理）
                    DisableButtonsForProcessedRequest(row);
                }
                else if (request.Status == CreditWithdrawStatus.忽略)
                {
                    row.Cells[e.ColumnIndex].Style.BackColor = Color.LightGray;
                    row.Cells[e.ColumnIndex].Style.SelectionBackColor = Color.LightGray;  // 🔥 选中时也保持浅灰色
                    row.Cells[e.ColumnIndex].Style.ForeColor = Color.Black;
                    row.Cells[e.ColumnIndex].Style.SelectionForeColor = Color.Black;
                    
                    // 🔥 禁用按钮（已处理）
                    DisableButtonsForProcessedRequest(row);
                }
                else if (request.Status == CreditWithdrawStatus.已拒绝)
                {
                    row.Cells[e.ColumnIndex].Style.BackColor = Color.Orange;
                    row.Cells[e.ColumnIndex].Style.SelectionBackColor = Color.Orange;  // 🔥 选中时也保持橙色
                    row.Cells[e.ColumnIndex].Style.ForeColor = Color.White;
                    row.Cells[e.ColumnIndex].Style.SelectionForeColor = Color.White;
                    
                    // 🔥 禁用按钮（已处理）
                    DisableButtonsForProcessedRequest(row);
                }
            }
            
            // 🔥 3. 金额列颜色
            if (column.DataPropertyName == "Amount")
            {
                int amount = (int)request.Amount;
                if (amount >= 10000)
                {
                    row.Cells[e.ColumnIndex].Style.BackColor = Color.Orange;
                    row.Cells[e.ColumnIndex].Style.SelectionBackColor = Color.Orange;  // 🔥 选中时也保持橙色
                    row.Cells[e.ColumnIndex].Style.ForeColor = Color.Black;
                    row.Cells[e.ColumnIndex].Style.SelectionForeColor = Color.Black;
                }
                else if (amount >= 1000)
                {
                    row.Cells[e.ColumnIndex].Style.BackColor = Color.Green;
                    row.Cells[e.ColumnIndex].Style.SelectionBackColor = Color.Green;  // 🔥 选中时也保持绿色
                    row.Cells[e.ColumnIndex].Style.ForeColor = Color.Black;
                    row.Cells[e.ColumnIndex].Style.SelectionForeColor = Color.Black;
                }
                else if (amount >= 100)
                {
                    row.Cells[e.ColumnIndex].Style.BackColor = Color.LightGray;
                    row.Cells[e.ColumnIndex].Style.SelectionBackColor = Color.LightGray;  // 🔥 选中时也保持浅灰色
                    row.Cells[e.ColumnIndex].Style.ForeColor = Color.Black;
                    row.Cells[e.ColumnIndex].Style.SelectionForeColor = Color.Black;
                }
                else
                {
                    // 🔥 小于100的金额，使用白色背景
                    row.Cells[e.ColumnIndex].Style.BackColor = Color.White;
                    row.Cells[e.ColumnIndex].Style.SelectionBackColor = Color.White;
                    row.Cells[e.ColumnIndex].Style.ForeColor = Color.Black;
                    row.Cells[e.ColumnIndex].Style.SelectionForeColor = Color.Black;
                }
            }
            
            // 🔥 4. 其他列（ID、时间、昵称、备注等）默认白色背景
            if (column.DataPropertyName != "Action" && 
                column.DataPropertyName != "Status" && 
                column.DataPropertyName != "Amount" &&
                !(column is DataGridViewButtonColumn))
            {
                row.Cells[e.ColumnIndex].Style.BackColor = Color.White;
                row.Cells[e.ColumnIndex].Style.SelectionBackColor = Color.White;  // 🔥 选中时保持白色
                row.Cells[e.ColumnIndex].Style.ForeColor = Color.Black;
                row.Cells[e.ColumnIndex].Style.SelectionForeColor = Color.Black;
            }
            
            // 🔥 不设置 e.Handled，让 DataGridView 自己绘制单元格（包括按钮列）
        }

        /// <summary>
        /// ❌ 已废弃：旧版自定义文本绘制（现在使用 Style 设置颜色）
        /// </summary>
        [Obsolete("已废弃，现在使用 Style 设置颜色")]
        private void DgvRequests_CellPainting_CustomText(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0 || e.Graphics == null) return;
            
            // 🔥 按钮列使用默认绘制
            if (dgvRequests.Columns[e.ColumnIndex] is DataGridViewButtonColumn)
            {
                return;  // 让按钮列使用默认绘制
            }
            
            // 🔥 绘制背景（使用默认背景，因为 RowPrePaint 已经绘制了整行背景）
            e.PaintBackground(e.CellBounds, true);
            
            // 🔥 绘制文本（强制黑色，确保可见）
            if (e.Value != null && e.CellStyle?.Font != null)
            {
                using (SolidBrush brush = new SolidBrush(Color.Black))  // 🔥 强制黑色文字
                {
                    // 🔥 根据对齐方式计算文本位置
                    StringFormat format = new StringFormat
                    {
                        Alignment = e.CellStyle.Alignment switch
                        {
                            DataGridViewContentAlignment.MiddleCenter or 
                            DataGridViewContentAlignment.TopCenter or 
                            DataGridViewContentAlignment.BottomCenter => StringAlignment.Center,
                            DataGridViewContentAlignment.MiddleRight or 
                            DataGridViewContentAlignment.TopRight or 
                            DataGridViewContentAlignment.BottomRight => StringAlignment.Far,
                            _ => StringAlignment.Near
                        },
                        LineAlignment = StringAlignment.Center
                    };
                    
                    e.Graphics.DrawString(
                        e.Value.ToString() ?? string.Empty,
                        e.CellStyle.Font,
                        brush,
                        e.CellBounds,
                        format);
                }
            }
            
            e.Handled = true;
        }

        /// <summary>
        /// ❌ 已废弃：旧版单元格绘制逻辑（仅设置 Style，不自定义绘制）
        /// </summary>
        [Obsolete("已废弃，使用上面新版 DgvRequests_CellPainting")]
        private void DgvRequests_CellPainting_OldStyleOnly(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _bindingSource.Count)
                return;
            
            var request = _bindingSource[e.RowIndex] as V2CreditWithdraw;
            if (request == null) return;
            
            var column = dgvRequests.Columns[e.ColumnIndex];
            var row = dgvRequests.Rows[e.RowIndex];
            
            // 🔥 1. 动作列颜色（参考 F5BotV2 Line 147-168）
            if (column.DataPropertyName == "Action")
            {
                if (request.Action == CreditWithdrawAction.上分)
                {
                    row.Cells[e.ColumnIndex].Style.BackColor = Color.Green;
                    row.Cells[e.ColumnIndex].Style.ForeColor = Color.Black;
                    // 🔥 同时设置按钮列颜色
                    if (row.Cells["btnAgree"] != null)
                        row.Cells["btnAgree"].Style.BackColor = Color.Green;
                    if (row.Cells["btnIgnore"] != null)
                        row.Cells["btnIgnore"].Style.BackColor = Color.Green;
                }
                else if (request.Action == CreditWithdrawAction.下分)
                {
                    row.Cells[e.ColumnIndex].Style.BackColor = Color.Red;
                    row.Cells[e.ColumnIndex].Style.ForeColor = Color.White;
                    // 🔥 同时设置按钮列颜色
                    if (row.Cells["btnAgree"] != null)
                        row.Cells["btnAgree"].Style.BackColor = Color.Red;
                    if (row.Cells["btnIgnore"] != null)
                        row.Cells["btnIgnore"].Style.BackColor = Color.Red;
                }
            }
            
            // 🔥 2. 状态列颜色（参考 F5BotV2 Line 169-209）
            if (column.DataPropertyName == "Status")
            {
                if (request.Status == CreditWithdrawStatus.等待处理)
                {
                    row.Cells[e.ColumnIndex].Style.BackColor = Color.Red;
                    row.Cells[e.ColumnIndex].Style.ForeColor = Color.White;
                }
                else if (request.Status == CreditWithdrawStatus.已同意)
                {
                    row.Cells[e.ColumnIndex].Style.BackColor = Color.Green;
                    row.Cells[e.ColumnIndex].Style.ForeColor = Color.White;
                    
                    // 🔥 禁用按钮并显示操作过的内容（参考 F5BotV2 Line 179-187）
                    if (row.Cells["btnAgree"] is DataGridViewButtonCell btnAgree)
                    {
                        btnAgree.ReadOnly = true;
                        btnAgree.Value = "已同意";
                        btnAgree.Style.BackColor = Color.Gray;
                        btnAgree.Style.ForeColor = Color.White;
                    }
                    if (row.Cells["btnIgnore"] is DataGridViewButtonCell btnIgnore)
                    {
                        btnIgnore.ReadOnly = true;
                        btnIgnore.Value = "";
                        btnIgnore.Style.BackColor = Color.Gray;
                    }
                    if (row.Cells["btnReject"] is DataGridViewButtonCell btnReject)
                    {
                        btnReject.ReadOnly = true;
                        btnReject.Value = "";
                        btnReject.Style.BackColor = Color.Gray;
                    }
                }
                else if (request.Status == CreditWithdrawStatus.忽略)
                {
                    row.Cells[e.ColumnIndex].Style.BackColor = Color.LightGray;
                    row.Cells[e.ColumnIndex].Style.ForeColor = Color.Black;
                    
                    // 🔥 禁用按钮并显示操作过的内容（参考 F5BotV2 Line 194-204）
                    if (row.Cells["btnAgree"] is DataGridViewButtonCell btnAgree)
                    {
                        btnAgree.ReadOnly = true;
                        btnAgree.Value = "";
                        btnAgree.Style.BackColor = Color.Gray;
                    }
                    if (row.Cells["btnIgnore"] is DataGridViewButtonCell btnIgnore)
                    {
                        btnIgnore.ReadOnly = true;
                        btnIgnore.Value = "已忽略";
                        btnIgnore.Style.BackColor = Color.Gray;
                        btnIgnore.Style.ForeColor = Color.White;
                    }
                    if (row.Cells["btnReject"] is DataGridViewButtonCell btnReject)
                    {
                        btnReject.ReadOnly = true;
                        btnReject.Value = "";
                        btnReject.Style.BackColor = Color.Gray;
                    }
                }
                else if (request.Status == CreditWithdrawStatus.已拒绝)
                {
                    row.Cells[e.ColumnIndex].Style.BackColor = Color.Orange;
                    row.Cells[e.ColumnIndex].Style.ForeColor = Color.White;
                    
                    // 🔥 禁用按钮并显示操作过的内容
                    if (row.Cells["btnAgree"] is DataGridViewButtonCell btnAgree)
                    {
                        btnAgree.ReadOnly = true;
                        btnAgree.Value = "";
                        btnAgree.Style.BackColor = Color.Gray;
                    }
                    if (row.Cells["btnIgnore"] is DataGridViewButtonCell btnIgnore)
                    {
                        btnIgnore.ReadOnly = true;
                        btnIgnore.Value = "";
                        btnIgnore.Style.BackColor = Color.Gray;
                    }
                    if (row.Cells["btnReject"] is DataGridViewButtonCell btnReject)
                    {
                        btnReject.ReadOnly = true;
                        btnReject.Value = "已拒绝";
                        btnReject.Style.BackColor = Color.Gray;
                        btnReject.Style.ForeColor = Color.White;
                    }
                }
            }
            
            // 🔥 3. 金额列颜色（参考 F5BotV2 Line 211-237）
            if (column.DataPropertyName == "Amount")
            {
                int amount = (int)request.Amount;
                if (amount >= 10000)
                {
                    row.Cells[e.ColumnIndex].Style.BackColor = Color.Orange;
                    row.Cells[e.ColumnIndex].Style.ForeColor = Color.Black;
                }
                else if (amount >= 1000)
                {
                    row.Cells[e.ColumnIndex].Style.BackColor = Color.Green;
                    row.Cells[e.ColumnIndex].Style.ForeColor = Color.Black;
                }
                else if (amount >= 100)
                {
                    row.Cells[e.ColumnIndex].Style.BackColor = Color.LightGray;
                    row.Cells[e.ColumnIndex].Style.ForeColor = Color.Black;
                }
            }
        }

        /// <summary>
        /// 单元格点击事件（处理按钮点击）
        /// 🔥 只有点击按钮列时才处理，其他列（备注、申请时间、金额等）直接返回，不弹框
        /// </summary>
        private void DgvRequests_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _bindingSource.Count)
                return;
            
            // 🔥 只有点击按钮列时才处理
            if (dgvRequests.Columns[e.ColumnIndex].Name != "btnAgree" && 
                dgvRequests.Columns[e.ColumnIndex].Name != "btnIgnore" &&
                dgvRequests.Columns[e.ColumnIndex].Name != "btnReject")
            {
                // 点击其他列（备注、申请时间、金额等），直接返回，不弹框
                return;
            }
            
            var request = _bindingSource[e.RowIndex] as V2CreditWithdraw;
            if (request == null) return;
            
            // 🔥 只有"等待处理"状态才能操作（已处理的不弹框，直接返回）
            if (request.Status != CreditWithdrawStatus.等待处理)
            {
                // 不弹框，直接返回（提升用户体验）
                return;
            }
            
            if (dgvRequests.Columns[e.ColumnIndex].Name == "btnAgree")
            {
                // 同意
                ApproveRequest(request);
            }
            else if (dgvRequests.Columns[e.ColumnIndex].Name == "btnIgnore")
            {
                // 忽略
                IgnoreRequest(request);
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
                
                // 🔥 从 BindingList 查找会员（统一模式）
                var member = _membersBindingList.FirstOrDefault(m => m.Wxid == request.Wxid);
                
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
                
                // 🔥 更新申请状态（会自动触发 PropertyChanged，通知 ActionText 和 StatusText 更新）
                request.Status = CreditWithdrawStatus.已同意;
                request.ProcessedBy = Services.Api.BoterApi.GetInstance().User;
                request.ProcessedTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                
                // 🔥 强制刷新 BindingSource 中的该项（确保 UI 立即更新）
                int index = _bindingSource.IndexOf(request);
                if (index >= 0)
                {
                    _bindingSource.ResetItem(index);  // 🔥 强制刷新该行的所有单元格
                }
                
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
                
                // 🔥 保存到数据库（🔥 会员和申请的 PropertyChanged 会自动保存，只需手动插入资金变动）
                _db.Insert(balanceChange);
                
                // 🔥 更新会员的上下分统计（自动触发 PropertyChanged）
                _creditWithdrawsBindingList.UpdateMemberStatistics(_membersBindingList);
                
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
                
                // 🔥 更新统计（BindingList 变化会自动更新 DataGridView，无需手动刷新）
                UpdateStats();
                
                this.ShowSuccessTip($"已同意{actionName}申请");
            }
            catch (Exception ex)
            {
                _logService.Error("上下分管理", "同意申请失败", ex);
                UIMessageBox.ShowError($"处理失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 忽略申请（参考 F5BotV2 Line 1526-1542）
        /// </summary>
        private void IgnoreRequest(V2CreditWithdraw request)
        {
            try
            {
                string actionName = request.Action == CreditWithdrawAction.上分 ? "上分" : "下分";
                
                if (!UIMessageBox.ShowAsk($"确定忽略【{request.Nickname}】的{actionName}申请吗？\n\n金额：{request.Amount:F2}"))
                {
                    return;
                }
                
                // 🔥 调用服务忽略申请
                var (success, errorMessage) = _creditWithdrawService.IgnoreCreditWithdraw(request);
                
                if (!success)
                {
                    UIMessageBox.ShowError($"忽略失败：{errorMessage}");
                    return;
                }
                
                // 🔥 强制刷新 BindingSource 中的该项（确保 UI 立即更新）
                int index = _bindingSource.IndexOf(request);
                if (index >= 0)
                {
                    _bindingSource.ResetItem(index);  // 🔥 强制刷新该行的所有单元格
                }
                
                // 🔥 更新统计（BindingList 变化会自动更新 DataGridView，无需手动刷新）
                UpdateStats();
                
                this.ShowSuccessTip($"已忽略{actionName}申请");
            }
            catch (Exception ex)
            {
                _logService.Error("上下分管理", "忽略申请失败", ex);
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
                
                // 🔥 更新申请状态（PropertyChanged 会自动保存到数据库，并通知 ActionText 和 StatusText 更新）
                request.Status = CreditWithdrawStatus.已拒绝;
                request.ProcessedBy = Services.Api.BoterApi.GetInstance().User;
                request.ProcessedTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                request.Notes = "管理员拒绝";
                
                // 🔥 强制刷新 BindingSource 中的该项（确保 UI 立即更新）
                int index = _bindingSource.IndexOf(request);
                if (index >= 0)
                {
                    _bindingSource.ResetItem(index);  // 🔥 强制刷新该行的所有单元格
                }
                
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
                
                // 🔥 更新统计（BindingList 变化会自动更新 DataGridView，无需手动刷新）
                UpdateStats();
                
                this.ShowSuccessTip($"已拒绝{actionName}申请");
            }
            catch (Exception ex)
            {
                _logService.Error("上下分管理", "拒绝申请失败", ex);
                UIMessageBox.ShowError($"处理失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 应用筛选（使用 BindingSource.Filter，标准做法）
        /// 🔥 当 BindingList 变化时，DataGridView 会自动更新，无需手动刷新
        /// </summary>
        private void ApplyFilter()
        {
            int statusIndex = cmbStatus.SelectedIndex;
            
            if (statusIndex > 0)
            {
                CreditWithdrawStatus targetStatus = statusIndex switch
                {
                    1 => CreditWithdrawStatus.等待处理,
                    2 => CreditWithdrawStatus.已同意,
                    3 => CreditWithdrawStatus.已拒绝,
                    4 => CreditWithdrawStatus.忽略,
                    _ => CreditWithdrawStatus.等待处理
                };
                
                // 🔥 使用 BindingSource.Filter 进行筛选（标准做法）
                // 注意：对于枚举类型，需要转换为整数进行比较
                _bindingSource.Filter = $"Convert(Status, 'System.Int32') = {(int)targetStatus}";
            }
            else
            {
                // 显示全部
                _bindingSource.Filter = null;
            }
            
            UpdateStats();
        }

        /// <summary>
        /// 更新统计信息（从 BindingList 直接统计）
        /// </summary>
        private void UpdateStats()
        {
            // 🔥 直接从 BindingList 统计（线程安全）
            int pendingCount = _creditWithdrawsBindingList.Count(r => r.Status == CreditWithdrawStatus.等待处理);
            
            // 今日上分和下分（已同意的）
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            float todayCredit = _creditWithdrawsBindingList
                .Where(r => r.Status == CreditWithdrawStatus.已同意 && 
                           r.Action == CreditWithdrawAction.上分 &&
                           r.TimeString.StartsWith(today))
                .Sum(r => r.Amount);
            
            float todayWithdraw = _creditWithdrawsBindingList
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
        /// 刷新按钮点击（重新应用筛选和更新统计）
        /// </summary>
        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            ApplyFilter();
            UpdateStats();
        }

        #region 美化效果 - 鼠标事件

        /// <summary>
        /// 禁用已处理请求的按钮（设置为灰色只读状态）
        /// </summary>
        private void DisableButtonsForProcessedRequest(DataGridViewRow row)
        {
            // 🔥 禁用"同意"按钮
            if (row.Cells["btnAgree"] is DataGridViewButtonCell btnAgree)
            {
                btnAgree.ReadOnly = true;
                btnAgree.Style.BackColor = Color.Gray;
                btnAgree.Style.SelectionBackColor = Color.Gray;
                btnAgree.Style.ForeColor = Color.DarkGray;
                btnAgree.Style.SelectionForeColor = Color.DarkGray;
            }
            
            // 🔥 禁用"忽略"按钮
            if (row.Cells["btnIgnore"] is DataGridViewButtonCell btnIgnore)
            {
                btnIgnore.ReadOnly = true;
                btnIgnore.Style.BackColor = Color.Gray;
                btnIgnore.Style.SelectionBackColor = Color.Gray;
                btnIgnore.Style.ForeColor = Color.DarkGray;
                btnIgnore.Style.SelectionForeColor = Color.DarkGray;
            }
            
            // 🔥 禁用"拒绝"按钮
            if (row.Cells["btnReject"] is DataGridViewButtonCell btnReject)
            {
                btnReject.ReadOnly = true;
                btnReject.Style.BackColor = Color.Gray;
                btnReject.Style.SelectionBackColor = Color.Gray;
                btnReject.Style.ForeColor = Color.DarkGray;
                btnReject.Style.SelectionForeColor = Color.DarkGray;
            }
        }

        /// <summary>
        /// 鼠标进入单元格：记录行索引并刷新
        /// </summary>
        private void DgvRequests_CellMouseEnter(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex != _hoverRowIndex)
            {
                _hoverRowIndex = e.RowIndex;
                dgvRequests.InvalidateRow(e.RowIndex);  // 🔥 只刷新当前行
            }
        }

        /// <summary>
        /// 鼠标离开单元格：清除行索引并刷新
        /// </summary>
        private void DgvRequests_CellMouseLeave(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex == _hoverRowIndex)
            {
                int oldIndex = _hoverRowIndex;
                _hoverRowIndex = -1;
                dgvRequests.InvalidateRow(oldIndex);  // 🔥 只刷新之前的行
            }
        }

        #endregion
    }
}

