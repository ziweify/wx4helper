using Sunny.UI;
using zhaocaimao.Contracts;
using zhaocaimao.Contracts.Games;
using zhaocaimao.Models.Games.Binggo;
using zhaocaimao.Core;
using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace zhaocaimao.Views
{
    /// <summary>
    /// 记录查询系统
    /// 
    /// 功能：
    /// - 记录浏览与检索
    /// - 历史记录查询
    /// - 手动录入功能
    /// - 实时数据同步
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
            // 窗体设置 - 使用深蓝/紫色主题，现代化渐变设计
            this.Text = "记录查询系统";
            this.Size = new Size(1180, 780);  // 不超过主窗口大小 (1200x808)
            this.Padding = new Padding(0, 35, 0, 0);
            this.ShowTitle = true;
            this.ShowRadius = true;
            this.Style = UIStyle.Blue;  // 使用蓝色主题，与原项目完全不同
            this.BackColor = Color.FromArgb(245, 248, 255);  // 淡蓝色背景
            
            // ====================================
            // 顶部工具栏区域 - 现代化三栏布局（紫色/蓝色主题）
            // ====================================
            
            // 左侧查询卡片 - 深蓝色主题
            var pnlQueryCard = new UIPanel
            {
                Location = new Point(15, 50),
                Size = new Size(280, 160),  // 紧凑的卡片
                FillColor = Color.FromArgb(255, 255, 255),  // 纯白背景
                RectColor = Color.FromArgb(33, 150, 243),  // 深蓝色边框
                Radius = 15,  // 适中的圆角
                RadiusSides = UICornerRadiusSides.All,
                BackColor = Color.White
            };
            this.Controls.Add(pnlQueryCard);
            
            // 中间操作卡片 - 紫色主题
            var pnlActionCard = new UIPanel
            {
                Location = new Point(305, 50),
                Size = new Size(280, 160),
                FillColor = Color.FromArgb(255, 255, 255),
                RectColor = Color.FromArgb(156, 39, 176),  // 紫色边框
                Radius = 15,
                RadiusSides = UICornerRadiusSides.All,
                BackColor = Color.White
            };
            this.Controls.Add(pnlActionCard);
            
            // 右侧状态卡片 - 靛蓝色主题
            var pnlStatusCard = new UIPanel
            {
                Location = new Point(595, 50),
                Size = new Size(280, 160),
                FillColor = Color.FromArgb(255, 255, 255),
                RectColor = Color.FromArgb(63, 81, 181),  // 靛蓝色边框
                Radius = 15,
                RadiusSides = UICornerRadiusSides.All,
                BackColor = Color.White
            };
            this.Controls.Add(pnlStatusCard);
            
            // 最右侧统计卡片 - 橙色主题
            var pnlStatsCard = new UIPanel
            {
                Location = new Point(885, 50),
                Size = new Size(280, 160),
                FillColor = Color.FromArgb(255, 255, 255),
                RectColor = Color.FromArgb(255, 152, 0),  // 橙色边框
                Radius = 15,
                RadiusSides = UICornerRadiusSides.All,
                BackColor = Color.White
            };
            this.Controls.Add(pnlStatsCard);
            
            // 日期选择器 - 左侧查询卡片
            var lblDate = new UILabel
            {
                Text = "📅 日期筛选",
                Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Bold),
                Location = new Point(15, 10),
                Size = new Size(150, 30),
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.FromArgb(33, 150, 243)  // 深蓝色
            };
            pnlQueryCard.Controls.Add(lblDate);
            
            dtpQueryDate = new UIDatePicker
            {
                Location = new Point(15, 50),
                Size = new Size(160, 38),
                Value = DateTime.Today,
                Font = new Font("Microsoft YaHei UI", 10F)
            };
            pnlQueryCard.Controls.Add(dtpQueryDate);
            
            btnQueryByDate = new UIButton
            {
                Text = "查询",
                Location = new Point(185, 50),
                Size = new Size(80, 38),
                Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Regular),
                TipsFont = new Font("Microsoft YaHei UI", 9F),
                Cursor = Cursors.Hand,
                FillColor = Color.FromArgb(33, 150, 243),  // 深蓝色按钮
                FillHoverColor = Color.FromArgb(25, 118, 210),
                FillPressColor = Color.FromArgb(21, 101, 192),
                RectColor = Color.FromArgb(33, 150, 243),
                RectHoverColor = Color.FromArgb(25, 118, 210),
                RectPressColor = Color.FromArgb(21, 101, 192)
            };
            btnQueryByDate.Click += BtnQueryByDate_Click;
            pnlQueryCard.Controls.Add(btnQueryByDate);
            
            btnRefreshToday = new UIButton
            {
                Text = "刷新今日",
                Location = new Point(15, 100),
                Size = new Size(250, 38),
                Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Regular),
                TipsFont = new Font("Microsoft YaHei UI", 9F),
                Cursor = Cursors.Hand,
                FillColor = Color.FromArgb(100, 181, 246),  // 浅蓝色按钮
                FillHoverColor = Color.FromArgb(66, 165, 245),
                FillPressColor = Color.FromArgb(33, 150, 243),
                RectColor = Color.FromArgb(100, 181, 246),
                RectHoverColor = Color.FromArgb(66, 165, 245),
                RectPressColor = Color.FromArgb(33, 150, 243)
            };
            btnRefreshToday.Click += BtnRefreshToday_Click;
            pnlQueryCard.Controls.Add(btnRefreshToday);
            
            // 数据补录区域 - 中间操作卡片
            var lblManual = new UILabel
            {
                Text = "✏️ 数据补录",
                Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Bold),
                Location = new Point(15, 10),
                Size = new Size(150, 30),
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.FromArgb(156, 39, 176)  // 紫色
            };
            pnlActionCard.Controls.Add(lblManual);
            
            txtManualIssue = new UITextBox
            {
                Location = new Point(15, 50),
                Size = new Size(140, 38),
                Watermark = "记录编号",
                Font = new Font("Microsoft YaHei UI", 10F)
            };
            pnlActionCard.Controls.Add(txtManualIssue);
            
            btnManualInput = new UIButton
            {
                Text = "提交",
                Location = new Point(165, 50),
                Size = new Size(100, 38),
                Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Regular),
                TipsFont = new Font("Microsoft YaHei UI", 9F),
                Cursor = Cursors.Hand,
                FillColor = Color.FromArgb(156, 39, 176),  // 紫色按钮
                FillHoverColor = Color.FromArgb(142, 36, 170),
                FillPressColor = Color.FromArgb(123, 31, 162),
                RectColor = Color.FromArgb(156, 39, 176),
                RectHoverColor = Color.FromArgb(142, 36, 170),
                RectPressColor = Color.FromArgb(123, 31, 162)
            };
            btnManualInput.Click += BtnManualInput_Click;
            pnlActionCard.Controls.Add(btnManualInput);
            
            // 添加说明文字
            var lblTip = new UILabel
            {
                Text = "数据异常时补充录入",
                Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Italic),
                Location = new Point(15, 100),
                Size = new Size(250, 50),
                ForeColor = Color.FromArgb(128, 128, 128),
                TextAlign = ContentAlignment.MiddleLeft
            };
            pnlActionCard.Controls.Add(lblTip);
            
            // 状态标签 - 右侧状态卡片
            var lblStatusTitle = new UILabel
            {
                Text = "⚡ 运行状态",
                Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Bold),
                Location = new Point(15, 10),
                Size = new Size(150, 30),
                ForeColor = Color.FromArgb(63, 81, 181),
                TextAlign = ContentAlignment.MiddleLeft
            };
            pnlStatusCard.Controls.Add(lblStatusTitle);
            
            lblStatus = new UILabel
            {
                Text = "系统运行正常",
                Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Regular),
                Location = new Point(15, 50),
                Size = new Size(250, 100),
                ForeColor = Color.FromArgb(63, 81, 181),  // 靛蓝色状态文字
                TextAlign = ContentAlignment.TopLeft
            };
            pnlStatusCard.Controls.Add(lblStatus);
            
            // 统计信息 - 最右侧统计卡片
            var lblStatsTitle = new UILabel
            {
                Text = "📊 统计信息",
                Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Bold),
                Location = new Point(15, 10),
                Size = new Size(150, 30),
                ForeColor = Color.FromArgb(255, 152, 0),
                TextAlign = ContentAlignment.MiddleLeft
            };
            pnlStatsCard.Controls.Add(lblStatsTitle);
            
            var lblStatsInfo = new UILabel
            {
                Text = "记录数: 0\n更新时间: --:--:--",
                Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Regular),
                Location = new Point(15, 50),
                Size = new Size(250, 100),
                ForeColor = Color.FromArgb(255, 152, 0),
                TextAlign = ContentAlignment.TopLeft
            };
            pnlStatsCard.Controls.Add(lblStatsInfo);
            
            // ====================================
            // 数据表格 - 现代化卡片式设计（深蓝/紫色主题）
            // ====================================
            
            // 添加表格容器面板，使用深蓝色主题边框
            var pnlTableContainer = new UIPanel
            {
                Location = new Point(15, 220),
                Size = new Size(1150, 540),
                FillColor = Color.White,
                RectColor = Color.FromArgb(33, 150, 243),  // 深蓝色边框
                Radius = 15,  // 适中的圆角
                RadiusSides = UICornerRadiusSides.All,
                BackColor = Color.White
            };
            this.Controls.Add(pnlTableContainer);
            
            // 添加表格标题栏 - 深蓝色渐变
            var pnlTitleBar = new UIPanel
            {
                Location = new Point(0, 0),
                Size = new Size(1150, 45),
                FillColor = Color.FromArgb(33, 150, 243),  // 深蓝色标题栏
                RectColor = Color.FromArgb(33, 150, 243),
                Radius = 0,  // 标题栏不需要圆角
                RadiusSides = UICornerRadiusSides.None,
                BackColor = Color.FromArgb(33, 150, 243)
            };
            pnlTableContainer.Controls.Add(pnlTitleBar);
            
            var lblTableTitle = new UILabel
            {
                Text = "📋 记录列表",
                Font = new Font("Microsoft YaHei UI", 13F, FontStyle.Bold),
                Location = new Point(20, 12),
                Size = new Size(150, 25),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleLeft
            };
            pnlTitleBar.Controls.Add(lblTableTitle);
            
            // 添加记录数量标签
            var lblRecordCount = new UILabel
            {
                Text = "总计: 0 条",
                Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Regular),
                Location = new Point(180, 12),
                Size = new Size(150, 25),
                ForeColor = Color.FromArgb(187, 222, 251),
                TextAlign = ContentAlignment.MiddleLeft
            };
            pnlTitleBar.Controls.Add(lblRecordCount);
            
            dgvLotteryData = new UIDataGridView
            {
                Location = new Point(15, 50),
                Size = new Size(1120, 480),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,  // 无边框，更现代
                Font = new Font("Microsoft YaHei UI", 9.5F),  // 减小字体
                RowTemplate = { Height = 40 },  // 减小行高，确保数据能完整显示
                GridColor = Color.FromArgb(227, 242, 253),  // 淡蓝色网格线
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.White,
                    ForeColor = Color.FromArgb(64, 64, 64),
                    SelectionBackColor = Color.FromArgb(100, 181, 246),  // 浅蓝色选中背景
                    SelectionForeColor = Color.White,  // 白色选中文字
                    Padding = new Padding(8, 4, 8, 4)  // 减小内边距
                },
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(33, 150, 243),  // 深蓝色表头
                    ForeColor = Color.White,  // 白色表头文字
                    Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold),  // 减小字体
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                    Padding = new Padding(8, 8, 8, 8)  // 减小内边距
                },
                EnableHeadersVisualStyles = false,
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(250, 251, 255)  // 淡蓝色交替行
                }
            };
            pnlTableContainer.Controls.Add(dgvLotteryData);
        }
        
        private void InitializeDataGridView()
        {
            // 清空现有列
            dgvLotteryData.Columns.Clear();
            
            // 添加列 - 使用更通用的标题和样式，调整列宽确保数据完整显示
            dgvLotteryData.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "IssueId",
                HeaderText = "编号",
                DataPropertyName = "IssueId",
                Width = 100,
                DefaultCellStyle = new DataGridViewCellStyle 
                { 
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                    Font = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(33, 150, 243)  // 深蓝色编号
                }
            });
            
            dgvLotteryData.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "OpenTime",
                HeaderText = "时间",
                DataPropertyName = "OpenTime",
                Width = 160,
                DefaultCellStyle = new DataGridViewCellStyle 
                { 
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                    Font = new Font("Microsoft YaHei UI", 9F)
                }
            });
            
            // P1-P5 列（调整列宽，确保数字能完整显示）
            for (int i = 1; i <= 5; i++)
            {
                dgvLotteryData.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = $"P{i}",
                    HeaderText = $"P{i}",
                    DataPropertyName = $"P{i}",
                    Width = 60,  // 增加列宽，确保两位数能完整显示
                    DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Alignment = DataGridViewContentAlignment.MiddleCenter,
                        Font = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Bold)
                    }
                });
            }
            
            dgvLotteryData.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "PSum",
                HeaderText = "总和",
                DataPropertyName = "PSum",
                Width = 70,  // 增加列宽，确保三位数能完整显示
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                    Font = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Bold)
                }
            });
            
            dgvLotteryData.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "DragonTiger",
                HeaderText = "龙虎",
                DataPropertyName = "DragonTiger",
                Width = 60,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                    Font = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Bold)
                }
            });
            
            dgvLotteryData.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Size",
                HeaderText = "大小",
                DataPropertyName = "Size",
                Width = 60,
                DefaultCellStyle = new DataGridViewCellStyle 
                { 
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                    Font = new Font("Microsoft YaHei UI", 9F)
                }
            });
            
            dgvLotteryData.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "OddEven",
                HeaderText = "单双",
                DataPropertyName = "OddEven",
                Width = 60,
                DefaultCellStyle = new DataGridViewCellStyle 
                { 
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                    Font = new Font("Microsoft YaHei UI", 9F)
                }
            });
            
            dgvLotteryData.Columns.Add(new DataGridViewCheckBoxColumn
            {
                Name = "IsOpened",
                HeaderText = "状态",
                DataPropertyName = "IsOpened",
                Width = 70,
                DefaultCellStyle = new DataGridViewCellStyle 
                { 
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                    Font = new Font("Microsoft YaHei UI", 9F)
                }
            });
            
            // 🔥 设置行样式（只用颜色区分大小）
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
                
                    lblStatus.Text = "系统运行正常\n等待数据加载...";
                    lblStatus.ForeColor = Color.FromArgb(63, 81, 181);
                    _logService.Info("DataViewer", "记录查询窗口已加载");
            }
            catch (Exception ex)
            {
                _logService.Error("DataViewer", "窗口加载失败", ex);
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
                    
                    lblStatus.Text = $"查询完成\n共 {data.Count} 条记录\n日期: {queryDate:yyyy-MM-dd}";
                    lblStatus.ForeColor = Color.FromArgb(33, 150, 243);
                    _logService.Info("DataViewer", $"日期查询: {queryDate:yyyy-MM-dd}，共 {data.Count} 条");
                }
                else
                {
                    lblStatus.Text = $"未查询到数据\n日期: {queryDate:yyyy-MM-dd}";
                    lblStatus.ForeColor = Color.FromArgb(255, 152, 0);
                    UIMessageBox.ShowWarning($"未查询到 {queryDate:yyyy-MM-dd} 的记录");
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"查询失败\n{ex.Message}";
                lblStatus.ForeColor = Color.FromArgb(244, 67, 54);
                _logService.Error("DataViewer", "数据查询失败", ex);
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
                    
                    lblStatus.Text = $"已加载今日数据\n共 {data.Count} 条记录\n时间: {DateTime.Now:HH:mm:ss}";
                    lblStatus.ForeColor = Color.FromArgb(33, 150, 243);
                    _logService.Info("DataViewer", $"今日数据已加载，共 {data.Count} 条");
                }
                else
                {
                    lblStatus.Text = "暂无今日数据\n请稍后刷新";
                    lblStatus.ForeColor = Color.FromArgb(128, 128, 128);
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"加载失败\n{ex.Message}";
                lblStatus.ForeColor = Color.FromArgb(244, 67, 54);
                _logService.Error("DataViewer", "今日数据加载失败", ex);
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
                    UIMessageBox.ShowWarning("请输入编号");
                    return;
                }
                
                if (!int.TryParse(issueText, out int issueId))
                {
                    UIMessageBox.ShowWarning("编号格式错误");
                    return;
                }
                
                // 弹出手动输入对话框（使用简单的 InputBox）
                string numbersInput = Microsoft.VisualBasic.Interaction.InputBox(
                    "请输入数值（用逗号分隔，例如：1,5,12,20,28）", 
                    "手动录入", 
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
                
                // 手动录入数据
                string lotteryData = string.Join(",", numbers);
                string openTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                
                var newData = new BinggoLotteryData().FillLotteryData(issueId, lotteryData, openTime);
                
                // 保存到数据库
                await _lotteryService.SaveLotteryDataAsync(newData);
                
                // 刷新列表
                await LoadTodayDataAsync();
                
                lblStatus.Text = $"补录成功\n编号: {issueId}\n时间: {DateTime.Now:HH:mm:ss}";
                lblStatus.ForeColor = Color.FromArgb(156, 39, 176);
                UIMessageBox.ShowSuccess($"记录编号 {issueId} 补录成功！");
                
                _logService.Info("DataViewer", $"手动录入: {issueId} -> {lotteryData}");
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"录入失败\n{ex.Message}";
                lblStatus.ForeColor = Color.FromArgb(244, 67, 54);
                _logService.Error("DataViewer", "手动录入失败", ex);
                UIMessageBox.ShowError($"手动录入失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 单元格格式化：数值大小颜色区分，并正确显示 LotteryNumber 对象
        /// </summary>
        private void DgvLotteryData_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            try
            {
                if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
                
                var columnName = dgvLotteryData.Columns[e.ColumnIndex].Name;
                var row = dgvLotteryData.Rows[e.RowIndex];
                var dataItem = row.DataBoundItem as BinggoLotteryData;
                
                // P1-P5 和 PSum 列：显示 LotteryNumber 的 Number 属性
                if ((columnName.StartsWith("P") && columnName.Length == 2 && char.IsDigit(columnName[1])) || 
                    columnName == "PSum")
                {
                    int number = 0;
                    LotteryNumber? lotteryNumber = null;
                    
                    // 根据列名获取对应的 LotteryNumber
                    if (dataItem != null)
                    {
                        lotteryNumber = columnName switch
                        {
                            "P1" => dataItem.P1,
                            "P2" => dataItem.P2,
                            "P3" => dataItem.P3,
                            "P4" => dataItem.P4,
                            "P5" => dataItem.P5,
                            "PSum" => dataItem.PSum,
                            _ => null
                        };
                        
                        if (lotteryNumber != null)
                        {
                            number = lotteryNumber.Number;
                            e.Value = number.ToString();  // 设置显示值为数字
                            e.FormattingApplied = true;
                        }
                        else
                        {
                            e.Value = "--";
                            e.FormattingApplied = true;
                            return;
                        }
                    }
                    else
                    {
                        // 如果无法获取数据项，尝试从现有值解析
                        if (e.Value != null && int.TryParse(e.Value.ToString(), out number))
                        {
                            // 值已经是数字，继续处理
                        }
                        else
                        {
                            return;
                        }
                    }
                    
                    // 判断大小：总和 >= 85 为大，< 85 为小
                    // P1-P5: >= 15 为大；PSum: >= 85 为大
                    bool isBig = (columnName == "PSum") ? (number >= 85) : (number >= 15);
                    
                    if (isBig)
                    {
                        e.CellStyle.BackColor = Color.FromArgb(255, 152, 0);  // 橙色（大）
                        e.CellStyle.ForeColor = Color.White;
                        e.CellStyle.Font = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Bold);
                    }
                    else
                    {
                        e.CellStyle.BackColor = Color.FromArgb(33, 150, 243);  // 深蓝色（小）
                        e.CellStyle.ForeColor = Color.White;
                        e.CellStyle.Font = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Bold);
                    }
                }
                // DragonTiger 列：显示龙虎文本
                else if (columnName == "DragonTiger")
                {
                    if (dataItem != null)
                    {
                        e.Value = dataItem.GetDragonTigerText();
                        e.FormattingApplied = true;
                    }
                }
                // Size 列：显示大小文本
                else if (columnName == "Size")
                {
                    if (dataItem != null && dataItem.PSum != null)
                    {
                        e.Value = dataItem.PSum.GetSizeText();
                        e.FormattingApplied = true;
                    }
                }
                // OddEven 列：显示单双文本
                else if (columnName == "OddEven")
                {
                    if (dataItem != null && dataItem.PSum != null)
                    {
                        e.Value = dataItem.PSum.GetOddEvenText();
                        e.FormattingApplied = true;
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
            // 隐藏而不是关闭，保持数据状态
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
            
            base.OnFormClosing(e);
        }
    }
}

