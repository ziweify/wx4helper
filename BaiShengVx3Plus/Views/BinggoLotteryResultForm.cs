using Sunny.UI;
using BaiShengVx3Plus.Contracts;
using BaiShengVx3Plus.Contracts.Games;
using BaiShengVx3Plus.Models.Games.Binggo;
using BaiShengVx3Plus.Core;
using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace BaiShengVx3Plus.Views
{
    /// <summary>
    /// 炳狗开奖结果窗口
    /// 🔥 参考 F5BotV2 的 OpenLotteryView
    /// 
    /// 功能：
    /// - 显示开奖数据列表（DataGridView）
    /// - 查询指定日期的开奖数据
    /// - 手动输入开奖数据（卡奖时使用）
    /// - 实时刷新最新数据
    /// </summary>
    public partial class BinggoLotteryResultForm : UIForm
    {
        private readonly IBinggoLotteryService _lotteryService;
        private readonly ILogService _logService;
        private BinggoLotteryDataBindingList? _bindingList;
        
        // UI 控件
        private UIDataGridView dgvLotteryData = null!;
        private UIDatePicker dtpQueryDate = null!;
        private UIButton btnQueryByDate = null!;
        private UIButton btnRefreshToday = null!;
        private UITextBox txtManualIssue = null!;
        private UIButton btnManualInput = null!;
        private UILabel lblStatus = null!;
        
        public BinggoLotteryResultForm(
            IBinggoLotteryService lotteryService,
            ILogService logService)
        {
            _lotteryService = lotteryService ?? throw new ArgumentNullException(nameof(lotteryService));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
            
            InitializeComponent();
            InitializeDataGridView();
        }
        
        private void InitializeComponent()
        {
            // 窗体设置
            this.Text = "开奖结果";
            this.Size = new Size(1000, 700);
            this.Padding = new Padding(0, 35, 0, 0);
            this.ShowTitle = true;
            this.ShowRadius = false;
            this.Style = UIStyle.Blue;
            
            // ====================================
            // 🔥 顶部工具栏区域
            // ====================================
            
            var pnlToolbar = new UIPanel
            {
                Location = new Point(10, 40),
                Size = new Size(970, 50),
                FillColor = Color.FromArgb(243, 249, 255),
                RectColor = Color.FromArgb(220, 220, 220),
                Radius = 5,
                RadiusSides = UICornerRadiusSides.All
            };
            this.Controls.Add(pnlToolbar);
            
            // 日期选择器
            var lblDate = new UILabel
            {
                Text = "查询日期:",
                Font = new Font("微软雅黑", 10F),
                Location = new Point(10, 13),
                Size = new Size(70, 25),
                TextAlign = ContentAlignment.MiddleRight
            };
            pnlToolbar.Controls.Add(lblDate);
            
            dtpQueryDate = new UIDatePicker
            {
                Location = new Point(85, 11),
                Size = new Size(150, 29),
                Value = DateTime.Today
            };
            pnlToolbar.Controls.Add(dtpQueryDate);
            
            btnQueryByDate = new UIButton
            {
                Text = "查询",
                Location = new Point(240, 11),
                Size = new Size(80, 29),
                Font = new Font("微软雅黑", 9F),
                TipsFont = new Font("宋体", 9F),
                Cursor = Cursors.Hand
            };
            btnQueryByDate.Click += BtnQueryByDate_Click;
            pnlToolbar.Controls.Add(btnQueryByDate);
            
            btnRefreshToday = new UIButton
            {
                Text = "刷新今日",
                Location = new Point(330, 11),
                Size = new Size(80, 29),
                Font = new Font("微软雅黑", 9F),
                TipsFont = new Font("宋体", 9F),
                Cursor = Cursors.Hand
            };
            btnRefreshToday.Click += BtnRefreshToday_Click;
            pnlToolbar.Controls.Add(btnRefreshToday);
            
            // 🔥 手动开奖区域（卡奖时使用）
            var lblManual = new UILabel
            {
                Text = "手动开奖:",
                Font = new Font("微软雅黑", 10F),
                Location = new Point(440, 13),
                Size = new Size(75, 25),
                TextAlign = ContentAlignment.MiddleRight
            };
            pnlToolbar.Controls.Add(lblManual);
            
            txtManualIssue = new UITextBox
            {
                Location = new Point(520, 11),
                Size = new Size(120, 29),
                Watermark = "输入期号"
            };
            pnlToolbar.Controls.Add(txtManualIssue);
            
            btnManualInput = new UIButton
            {
                Text = "手动输入",
                Location = new Point(650, 11),
                Size = new Size(90, 29),
                Font = new Font("微软雅黑", 9F),
                TipsFont = new Font("宋体", 9F),
                Cursor = Cursors.Hand,
                FillColor = Color.FromArgb(255, 152, 0)
            };
            btnManualInput.Click += BtnManualInput_Click;
            pnlToolbar.Controls.Add(btnManualInput);
            
            // 状态标签
            lblStatus = new UILabel
            {
                Text = "就绪",
                Font = new Font("微软雅黑", 9F),
                Location = new Point(760, 13),
                Size = new Size(200, 25),
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleLeft
            };
            pnlToolbar.Controls.Add(lblStatus);
            
            // ====================================
            // 🔥 DataGridView 数据表格
            // ====================================
            
            dgvLotteryData = new UIDataGridView
            {
                Location = new Point(10, 100),
                Size = new Size(970, 550),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D,
                Font = new Font("微软雅黑", 9F)
            };
            this.Controls.Add(dgvLotteryData);
        }
        
        private void InitializeDataGridView()
        {
            // 清空现有列
            dgvLotteryData.Columns.Clear();
            
            // 添加列
            dgvLotteryData.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "IssueId",
                HeaderText = "期号",
                DataPropertyName = "IssueId",
                Width = 100,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });
            
            dgvLotteryData.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "OpenTime",
                HeaderText = "开奖时间",
                DataPropertyName = "OpenTime",
                Width = 150,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });
            
            // P1-P5 列
            for (int i = 1; i <= 5; i++)
            {
                dgvLotteryData.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = $"P{i}",
                    HeaderText = $"P{i}",
                    DataPropertyName = $"P{i}",
                    Width = 40,
                    DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Alignment = DataGridViewContentAlignment.MiddleCenter,
                        Font = new Font("微软雅黑", 9F, FontStyle.Bold)
                    }
                });
            }
            
            dgvLotteryData.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "PSum",
                HeaderText = "总和",
                DataPropertyName = "PSum",
                Width = 50,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                    Font = new Font("微软雅黑", 9F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(255, 87, 34)
                }
            });
            
            dgvLotteryData.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "DragonTiger",
                HeaderText = "龙虎",
                DataPropertyName = "DragonTiger",
                Width = 50,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                    Font = new Font("微软雅黑", 9F, FontStyle.Bold)
                }
            });
            
            dgvLotteryData.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Size",
                HeaderText = "大小",
                DataPropertyName = "Size",
                Width = 50,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });
            
            dgvLotteryData.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "OddEven",
                HeaderText = "单双",
                DataPropertyName = "OddEven",
                Width = 50,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });
            
            dgvLotteryData.Columns.Add(new DataGridViewCheckBoxColumn
            {
                Name = "IsOpened",
                HeaderText = "已开奖",
                DataPropertyName = "IsOpened",
                Width = 60,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });
            
            // 🔥 设置行样式
            dgvLotteryData.CellFormatting += DgvLotteryData_CellFormatting;
        }
        
        /// <summary>
        /// 设置 BindingList（由外部传入）
        /// </summary>
        public void SetBindingList(BinggoLotteryDataBindingList? bindingList)
        {
            _bindingList = bindingList;
            if (_bindingList != null)
            {
                dgvLotteryData.DataSource = _bindingList;
            }
        }
        
        private void BinggoLotteryResultForm_Load(object sender, EventArgs e)
        {
            try
            {
                // 加载今日数据
                _ = LoadTodayDataAsync();
                
                lblStatus.Text = "就绪";
                _logService.Info("BinggoLotteryResultForm", "开奖结果窗口已加载");
            }
            catch (Exception ex)
            {
                _logService.Error("BinggoLotteryResultForm", "加载失败", ex);
                UIMessageBox.ShowError($"加载失败: {ex.Message}");
            }
        }
        
        private async void BtnQueryByDate_Click(object? sender, EventArgs e)
        {
            try
            {
                lblStatus.Text = "正在查询...";
                DateTime queryDate = dtpQueryDate.Value.Date;
                
                var data = await _lotteryService.GetLotteryDataByDateAsync(queryDate);
                
                if (data != null && data.Count > 0)
                {
                    // 清空现有数据
                    _bindingList?.Clear();
                    
                    // 添加查询结果
                    foreach (var item in data.OrderByDescending(x => x.IssueId))
                    {
                        _bindingList?.Add(item);
                    }
                    
                    lblStatus.Text = $"查询完成，共 {data.Count} 条数据";
                    _logService.Info("BinggoLotteryResultForm", $"查询日期 {queryDate:yyyy-MM-dd}，获取 {data.Count} 条数据");
                }
                else
                {
                    lblStatus.Text = "未查询到数据";
                    UIMessageBox.ShowWarning($"未查询到 {queryDate:yyyy-MM-dd} 的开奖数据");
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "查询失败";
                _logService.Error("BinggoLotteryResultForm", "查询数据失败", ex);
                UIMessageBox.ShowError($"查询失败: {ex.Message}");
            }
        }
        
        private async void BtnRefreshToday_Click(object? sender, EventArgs e)
        {
            await LoadTodayDataAsync();
        }
        
        private async Task LoadTodayDataAsync()
        {
            try
            {
                lblStatus.Text = "正在加载今日数据...";
                
                var data = await _lotteryService.GetLotteryDataByDateAsync(DateTime.Today);
                
                if (data != null && data.Count > 0)
                {
                    // 清空现有数据
                    _bindingList?.Clear();
                    
                    // 添加今日数据
                    foreach (var item in data.OrderByDescending(x => x.IssueId))
                    {
                        _bindingList?.Add(item);
                    }
                    
                    lblStatus.Text = $"已加载今日数据，共 {data.Count} 条";
                    _logService.Info("BinggoLotteryResultForm", $"加载今日数据，共 {data.Count} 条");
                }
                else
                {
                    lblStatus.Text = "暂无今日数据";
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "加载失败";
                _logService.Error("BinggoLotteryResultForm", "加载今日数据失败", ex);
                UIMessageBox.ShowError($"加载今日数据失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 🔥 手动输入开奖数据（卡奖时使用）
        /// </summary>
        private async void BtnManualInput_Click(object? sender, EventArgs e)
        {
            try
            {
                string issueText = txtManualIssue.Text?.Trim() ?? "";
                
                if (string.IsNullOrEmpty(issueText))
                {
                    UIMessageBox.ShowWarning("请输入期号");
                    return;
                }
                
                if (!int.TryParse(issueText, out int issueId))
                {
                    UIMessageBox.ShowWarning("期号格式错误");
                    return;
                }
                
                // 弹出手动输入对话框（使用简单的 InputBox）
                string numbersInput = Microsoft.VisualBasic.Interaction.InputBox(
                    "请输入开奖号码（用逗号分隔，例如：1,5,12,20,28）", 
                    "手动开奖", 
                    "", 
                    -1, -1);
                
                if (string.IsNullOrEmpty(numbersInput))
                {
                    return; // 用户取消
                }
                
                // 验证格式
                var parts = numbersInput.Split(',');
                if (parts.Length != 5)
                {
                    UIMessageBox.ShowWarning("必须输入5个号码，用逗号分隔");
                    return;
                }
                
                var numbers = new int[5];
                for (int i = 0; i < 5; i++)
                {
                    if (!int.TryParse(parts[i].Trim(), out numbers[i]) || numbers[i] < 1 || numbers[i] > 28)
                    {
                        UIMessageBox.ShowWarning($"号码 {parts[i]} 无效，必须是1-28之间的整数");
                        return;
                    }
                }
                
                // 🔥 手动触发开奖
                string lotteryData = string.Join(",", numbers);
                string openTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                
                var newData = new BinggoLotteryData().FillLotteryData(issueId, lotteryData, openTime);
                
                // 保存到数据库
                await _lotteryService.SaveLotteryDataAsync(newData);
                
                // 刷新列表
                await LoadTodayDataAsync();
                
                lblStatus.Text = $"手动开奖成功: {issueId}";
                UIMessageBox.ShowSuccess($"期号 {issueId} 手动开奖成功！");
                
                _logService.Info("BinggoLotteryResultForm", $"手动开奖: {issueId} -> {lotteryData}");
            }
            catch (Exception ex)
            {
                lblStatus.Text = "手动开奖失败";
                _logService.Error("BinggoLotteryResultForm", "手动开奖失败", ex);
                UIMessageBox.ShowError($"手动开奖失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 🔥 单元格格式化（设置号码颜色）
        /// </summary>
        private void DgvLotteryData_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            try
            {
                if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
                
                var columnName = dgvLotteryData.Columns[e.ColumnIndex].Name;
                
                // P1-P5 列设置颜色
                if (columnName.StartsWith("P") && columnName.Length == 2 && char.IsDigit(columnName[1]))
                {
                    var row = dgvLotteryData.Rows[e.RowIndex];
                    var data = row.DataBoundItem as BinggoLotteryData;
                    
                    if (data != null && e.Value != null)
                    {
                        int number = 0;
                        if (columnName == "P1" && data.P1 != null) number = data.P1.Number;
                        else if (columnName == "P2" && data.P2 != null) number = data.P2.Number;
                        else if (columnName == "P3" && data.P3 != null) number = data.P3.Number;
                        else if (columnName == "P4" && data.P4 != null) number = data.P4.Number;
                        else if (columnName == "P5" && data.P5 != null) number = data.P5.Number;
                        
                        if (number >= 1 && number <= 10)
                        {
                            e.CellStyle.BackColor = Color.FromArgb(33, 150, 243); // 蓝色
                            e.CellStyle.ForeColor = Color.White;
                        }
                        else if (number >= 11 && number <= 20)
                        {
                            e.CellStyle.BackColor = Color.FromArgb(76, 175, 80); // 绿色
                            e.CellStyle.ForeColor = Color.White;
                        }
                        else if (number >= 21 && number <= 28)
                        {
                            e.CellStyle.BackColor = Color.FromArgb(244, 67, 54); // 红色
                            e.CellStyle.ForeColor = Color.White;
                        }
                    }
                }
            }
            catch
            {
                // 忽略格式化错误
            }
        }
        
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // 🔥 隐藏而不是关闭（参考 F5BotV2）
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
            
            base.OnFormClosing(e);
        }
    }
}

