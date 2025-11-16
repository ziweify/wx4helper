using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using zhaocaimao.Contracts;
using zhaocaimao.Models;
using SQLite;
using Sunny.UI;

namespace zhaocaimao.Views
{
    /// <summary>
    /// 资金变动查看窗口
    /// 显示指定会员的所有资金变动记录
    /// </summary>
    public partial class BalanceChangeViewerForm : UIForm
    {
        private readonly string? _wxid;
        private readonly string? _nickname;
        private readonly SQLiteConnection _db;
        private readonly ILogService _logService;
        private List<V2BalanceChange> _allChanges = new List<V2BalanceChange>();
        private List<V2BalanceChange> _filteredChanges = new List<V2BalanceChange>();

        public BalanceChangeViewerForm(string? wxid, string? nickname, SQLiteConnection db, ILogService logService)
        {
            _wxid = wxid;
            _nickname = nickname;
            _db = db;
            _logService = logService;
            
            InitializeComponent();
            
            // 设置窗口标题
            this.Text = $"资金变动 - {_nickname}";
            
            // 初始化下拉框
            InitializeComboBox();
            
            // 配置DataGridView
            ConfigureDataGridView();
            
            // 加载数据
            LoadData();
        }

        /// <summary>
        /// 初始化下拉框
        /// </summary>
        private void InitializeComboBox()
        {
            cmbReason.Items.Clear();
            cmbReason.Items.Add("全部原因");
            cmbReason.Items.Add("下注");
            cmbReason.Items.Add("订单结算");
            cmbReason.Items.Add("订单取消");
            cmbReason.Items.Add("上分");
            cmbReason.Items.Add("下分");
            cmbReason.Items.Add("清空数据");
            cmbReason.Items.Add("手动调整");
            cmbReason.Items.Add("补单");
            cmbReason.SelectedIndex = 0;
        }

        /// <summary>
        /// 配置DataGridView列
        /// </summary>
        private void ConfigureDataGridView()
        {
            dgvBalanceChanges.AutoGenerateColumns = false;
            
            // 清空现有列
            dgvBalanceChanges.Columns.Clear();
            
            // 添加列
            dgvBalanceChanges.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn 
                { 
                    DataPropertyName = "Id", 
                    HeaderText = "ID", 
                    Width = 60 
                },
                new DataGridViewTextBoxColumn 
                { 
                    DataPropertyName = "TimeString", 
                    HeaderText = "变动时间", 
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
                    DataPropertyName = "BalanceBefore", 
                    HeaderText = "变动前", 
                    Width = 90,
                    DefaultCellStyle = new DataGridViewCellStyle 
                    { 
                        Format = "F2", 
                        Alignment = DataGridViewContentAlignment.MiddleRight 
                    } 
                },
                new DataGridViewTextBoxColumn 
                { 
                    DataPropertyName = "BalanceAfter", 
                    HeaderText = "变动后", 
                    Width = 90,
                    DefaultCellStyle = new DataGridViewCellStyle 
                    { 
                        Format = "F2", 
                        Alignment = DataGridViewContentAlignment.MiddleRight 
                    } 
                },
                new DataGridViewTextBoxColumn 
                { 
                    DataPropertyName = "ChangeAmount", 
                    HeaderText = "变动金额", 
                    Width = 100,
                    DefaultCellStyle = new DataGridViewCellStyle 
                    { 
                        Format = "+0.00;-0.00;0.00", 
                        Alignment = DataGridViewContentAlignment.MiddleRight 
                    } 
                },
                new DataGridViewTextBoxColumn 
                { 
                    DataPropertyName = "ReasonText", 
                    HeaderText = "变动原因", 
                    Width = 90 
                },
                new DataGridViewTextBoxColumn 
                { 
                    DataPropertyName = "IssueId", 
                    HeaderText = "期号", 
                    Width = 90 
                },
                new DataGridViewTextBoxColumn 
                { 
                    DataPropertyName = "RelatedOrderId", 
                    HeaderText = "订单ID", 
                    Width = 70 
                },
                new DataGridViewTextBoxColumn 
                { 
                    DataPropertyName = "RelatedOrderInfo", 
                    HeaderText = "订单内容", 
                    Width = 120 
                },
                new DataGridViewTextBoxColumn 
                { 
                    DataPropertyName = "RelatedOrderTime", 
                    HeaderText = "订单时间", 
                    Width = 140 
                },
                new DataGridViewTextBoxColumn 
                { 
                    DataPropertyName = "Notes", 
                    HeaderText = "备注", 
                    Width = 150,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill 
                }
            });
            
            // 单元格格式化（变动金额着色）
            dgvBalanceChanges.CellFormatting += (s, e) =>
            {
                if (e.ColumnIndex == 5 && e.Value != null) // ChangeAmount 列
                {
                    if (float.TryParse(e.Value.ToString(), out float amount))
                    {
                        if (amount > 0)
                        {
                            e.CellStyle.ForeColor = System.Drawing.Color.Green;
                            e.CellStyle.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Bold);
                        }
                        else if (amount < 0)
                        {
                            e.CellStyle.ForeColor = System.Drawing.Color.Red;
                            e.CellStyle.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Bold);
                        }
                    }
                }
            };
        }

        /// <summary>
        /// 加载数据
        /// </summary>
        private void LoadData()
        {
            try
            {
                // 🔥 确保表存在
                _db.CreateTable<V2BalanceChange>();
                
                // 加载指定会员的资金变动记录
                if (!string.IsNullOrEmpty(_wxid))
                {
                    _allChanges = _db.Table<V2BalanceChange>()
                        .Where(c => c.Wxid == _wxid)
                        .OrderByDescending(c => c.Timestamp)
                        .ToList();
                }
                else
                {
                    // 如果没有指定 Wxid，加载所有记录
                    _allChanges = _db.Table<V2BalanceChange>()
                        .OrderByDescending(c => c.Timestamp)
                        .ToList();
                }
                
                _filteredChanges = new List<V2BalanceChange>(_allChanges);
                
                _logService.Info("资金变动查看", $"加载了 {_allChanges.Count} 条资金变动记录");
                
                RefreshGrid();
            }
            catch (Exception ex)
            {
                _logService.Error("资金变动查看", "加载数据失败", ex);
                UIMessageBox.ShowError($"加载数据失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 刷新 DataGridView
        /// </summary>
        private void RefreshGrid()
        {
            dgvBalanceChanges.DataSource = null;
            dgvBalanceChanges.DataSource = _filteredChanges;
            
            UpdateStats();
        }

        /// <summary>
        /// 更新统计信息
        /// </summary>
        private void UpdateStats()
        {
            float totalIncrease = _filteredChanges.Where(c => c.ChangeAmount > 0).Sum(c => c.ChangeAmount);
            float totalDecrease = _filteredChanges.Where(c => c.ChangeAmount < 0).Sum(c => c.ChangeAmount);
            float netChange = _filteredChanges.Sum(c => c.ChangeAmount);
            
            lblStats.Text = $"共 {_filteredChanges.Count} 条记录 | 增加: {totalIncrease:F2} | 减少: {totalDecrease:F2} | 净变化: {netChange:+0.00;-0.00;0.00}";
        }

        /// <summary>
        /// 应用筛选
        /// </summary>
        private void ApplyFilter()
        {
            string searchText = txtSearch.Text?.Trim() ?? "";
            int reasonIndex = cmbReason.SelectedIndex;
            
            _filteredChanges = _allChanges.Where(c =>
            {
                // 搜索过滤
                if (!string.IsNullOrEmpty(searchText))
                {
                    if (!c.Nickname?.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true &&
                        !c.Wxid?.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true)
                    {
                        return false;
                    }
                }
                
                // 原因过滤
                if (reasonIndex > 0)
                {
                    ChangeReason reason = reasonIndex switch
                    {
                        1 => ChangeReason.下注,
                        2 => ChangeReason.订单结算,
                        3 => ChangeReason.订单取消,
                        4 => ChangeReason.上分,
                        5 => ChangeReason.下分,
                        6 => ChangeReason.清空数据,
                        7 => ChangeReason.手动调整,
                        8 => ChangeReason.补单,
                        _ => ChangeReason.未知
                    };
                    
                    if (c.Reason != reason)
                    {
                        return false;
                    }
                }
                
                return true;
            }).ToList();
            
            RefreshGrid();
        }

        /// <summary>
        /// 搜索按钮点击
        /// </summary>
        private void BtnSearch_Click(object sender, EventArgs e)
        {
            ApplyFilter();
        }

        /// <summary>
        /// 重置按钮点击
        /// </summary>
        private void BtnReset_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";
            cmbReason.SelectedIndex = 0;
            ApplyFilter();
        }

        /// <summary>
        /// 搜索文本框回车
        /// </summary>
        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ApplyFilter();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        /// <summary>
        /// 原因下拉框变化
        /// </summary>
        private void CmbReason_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyFilter();
        }
    }
}

