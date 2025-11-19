namespace BaiShengVx3Plus.Views.Controls
{
    partial class SoundSettingsPanel
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            grpSoundSettings = new Sunny.UI.UIGroupBox();
            chkEnableSound = new Sunny.UI.UICheckBox();
            
            // 封盘声音
            lblSealing = new Sunny.UI.UILabel();
            txtSealingSound = new Sunny.UI.UITextBox();
            btnBrowseSealing = new Sunny.UI.UIButton();
            btnTestSealing = new Sunny.UI.UIButton();
            trbSealingVolume = new Sunny.UI.UITrackBar();
            lblSealingVolume = new Sunny.UI.UILabel();
            
            // 开奖声音
            lblLottery = new Sunny.UI.UILabel();
            txtLotterySound = new Sunny.UI.UITextBox();
            btnBrowseLottery = new Sunny.UI.UIButton();
            btnTestLottery = new Sunny.UI.UIButton();
            trbLotteryVolume = new Sunny.UI.UITrackBar();
            lblLotteryVolume = new Sunny.UI.UILabel();
            
            // 上分声音
            lblCreditUp = new Sunny.UI.UILabel();
            txtCreditUpSound = new Sunny.UI.UITextBox();
            btnBrowseCreditUp = new Sunny.UI.UIButton();
            btnTestCreditUp = new Sunny.UI.UIButton();
            trbCreditUpVolume = new Sunny.UI.UITrackBar();
            lblCreditUpVolume = new Sunny.UI.UILabel();
            
            // 下分声音
            lblCreditDown = new Sunny.UI.UILabel();
            txtCreditDownSound = new Sunny.UI.UITextBox();
            btnBrowseCreditDown = new Sunny.UI.UIButton();
            btnTestCreditDown = new Sunny.UI.UIButton();
            trbCreditDownVolume = new Sunny.UI.UITrackBar();
            lblCreditDownVolume = new Sunny.UI.UILabel();
            
            grpSoundSettings.SuspendLayout();
            SuspendLayout();
            
            // 
            // grpSoundSettings
            // 
            grpSoundSettings.Controls.Add(chkEnableSound);
            grpSoundSettings.Controls.Add(lblSealing);
            grpSoundSettings.Controls.Add(txtSealingSound);
            grpSoundSettings.Controls.Add(btnBrowseSealing);
            grpSoundSettings.Controls.Add(btnTestSealing);
            grpSoundSettings.Controls.Add(trbSealingVolume);
            grpSoundSettings.Controls.Add(lblSealingVolume);
            grpSoundSettings.Controls.Add(lblLottery);
            grpSoundSettings.Controls.Add(txtLotterySound);
            grpSoundSettings.Controls.Add(btnBrowseLottery);
            grpSoundSettings.Controls.Add(btnTestLottery);
            grpSoundSettings.Controls.Add(trbLotteryVolume);
            grpSoundSettings.Controls.Add(lblLotteryVolume);
            grpSoundSettings.Controls.Add(lblCreditUp);
            grpSoundSettings.Controls.Add(txtCreditUpSound);
            grpSoundSettings.Controls.Add(btnBrowseCreditUp);
            grpSoundSettings.Controls.Add(btnTestCreditUp);
            grpSoundSettings.Controls.Add(trbCreditUpVolume);
            grpSoundSettings.Controls.Add(lblCreditUpVolume);
            grpSoundSettings.Controls.Add(lblCreditDown);
            grpSoundSettings.Controls.Add(txtCreditDownSound);
            grpSoundSettings.Controls.Add(btnBrowseCreditDown);
            grpSoundSettings.Controls.Add(btnTestCreditDown);
            grpSoundSettings.Controls.Add(trbCreditDownVolume);
            grpSoundSettings.Controls.Add(lblCreditDownVolume);
            grpSoundSettings.Dock = System.Windows.Forms.DockStyle.Fill;
            grpSoundSettings.Font = new System.Drawing.Font("微软雅黑", 12F);
            grpSoundSettings.Location = new System.Drawing.Point(0, 0);
            grpSoundSettings.Name = "grpSoundSettings";
            grpSoundSettings.Size = new System.Drawing.Size(750, 450);
            grpSoundSettings.TabIndex = 0;
            grpSoundSettings.Text = "🔊 声音设置";
            grpSoundSettings.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            
            // 
            // chkEnableSound
            // 
            chkEnableSound.Checked = true;
            chkEnableSound.Font = new System.Drawing.Font("微软雅黑", 12F);
            chkEnableSound.Location = new System.Drawing.Point(20, 40);
            chkEnableSound.Name = "chkEnableSound";
            chkEnableSound.Size = new System.Drawing.Size(150, 30);
            chkEnableSound.TabIndex = 0;
            chkEnableSound.Text = "启用声音";
            
            int yPos = 80;
            int rowHeight = 70;
            
            // === 封盘声音 ===
            SetupSoundRow(lblSealing, txtSealingSound, btnBrowseSealing, btnTestSealing, trbSealingVolume, lblSealingVolume,
                          "封盘声音:", "mp3_fp.mp3", yPos, 0);
            
            yPos += rowHeight;
            
            // === 开奖声音 ===
            SetupSoundRow(lblLottery, txtLotterySound, btnBrowseLottery, btnTestLottery, trbLotteryVolume, lblLotteryVolume,
                          "开奖声音:", "mp3_kj.mp3", yPos, 1);
            
            yPos += rowHeight;
            
            // === 上分声音 ===
            SetupSoundRow(lblCreditUp, txtCreditUpSound, btnBrowseCreditUp, btnTestCreditUp, trbCreditUpVolume, lblCreditUpVolume,
                          "上分声音:", "mp3_shang.mp3", yPos, 2);
            
            yPos += rowHeight;
            
            // === 下分声音 ===
            SetupSoundRow(lblCreditDown, txtCreditDownSound, btnBrowseCreditDown, btnTestCreditDown, trbCreditDownVolume, lblCreditDownVolume,
                          "下分声音:", "mp3_xia.mp3", yPos, 3);
            
            // 
            // SoundSettingsPanel
            // 
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(grpSoundSettings);
            Name = "SoundSettingsPanel";
            Size = new System.Drawing.Size(750, 450);
            grpSoundSettings.ResumeLayout(false);
            ResumeLayout(false);
        }
        
        private void SetupSoundRow(Sunny.UI.UILabel label, Sunny.UI.UITextBox textBox, 
                                   Sunny.UI.UIButton btnBrowse, Sunny.UI.UIButton btnTest,
                                   Sunny.UI.UITrackBar trackBar, Sunny.UI.UILabel lblVolume,
                                   string labelText, string defaultFile, int yPos, int index)
        {
            // Label
            label.Font = new System.Drawing.Font("微软雅黑", 10F);
            label.Location = new System.Drawing.Point(20, yPos);
            label.Name = $"lbl{index}";
            label.Size = new System.Drawing.Size(100, 25);
            label.TabIndex = index * 10 + 1;
            label.Text = labelText;
            label.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            
            // TextBox
            textBox.Font = new System.Drawing.Font("微软雅黑", 10F);
            textBox.Location = new System.Drawing.Point(130, yPos);
            textBox.Name = $"txt{index}";
            textBox.Size = new System.Drawing.Size(250, 25);
            textBox.TabIndex = index * 10 + 2;
            textBox.Text = defaultFile;
            textBox.Watermark = "声音文件路径（相对于 sound 文件夹）";
            
            // Browse Button
            btnBrowse.Font = new System.Drawing.Font("微软雅黑", 9F);
            btnBrowse.Location = new System.Drawing.Point(390, yPos);
            btnBrowse.Name = $"btnBrowse{index}";
            btnBrowse.Size = new System.Drawing.Size(60, 25);
            btnBrowse.TabIndex = index * 10 + 3;
            btnBrowse.Text = "浏览";
            btnBrowse.Click += (s, e) => BrowseSoundFile(textBox);
            
            // Test Button
            btnTest.Font = new System.Drawing.Font("微软雅黑", 9F);
            btnTest.Location = new System.Drawing.Point(460, yPos);
            btnTest.Name = $"btnTest{index}";
            btnTest.Size = new System.Drawing.Size(60, 25);
            btnTest.TabIndex = index * 10 + 4;
            btnTest.Text = "测试";
            btnTest.Tag = textBox; // 保存关联的文本框
            btnTest.Click += BtnTest_Click;
            
            // TrackBar
            trackBar.Font = new System.Drawing.Font("微软雅黑", 9F);
            trackBar.Location = new System.Drawing.Point(530, yPos);
            trackBar.Maximum = 100;
            trackBar.Minimum = 0;
            trackBar.Name = $"trb{index}";
            trackBar.Size = new System.Drawing.Size(140, 25);
            trackBar.TabIndex = index * 10 + 5;
            trackBar.Value = 100;
            trackBar.ValueChanged += (s, e) => UpdateVolumeLabel(trackBar, lblVolume);
            
            // Volume Label
            lblVolume.Font = new System.Drawing.Font("微软雅黑", 9F);
            lblVolume.Location = new System.Drawing.Point(680, yPos);
            lblVolume.Name = $"lblVol{index}";
            lblVolume.Size = new System.Drawing.Size(50, 25);
            lblVolume.TabIndex = index * 10 + 6;
            lblVolume.Text = "100%";
            lblVolume.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        }

        private Sunny.UI.UIGroupBox grpSoundSettings;
        private Sunny.UI.UICheckBox chkEnableSound;
        
        // 封盘
        private Sunny.UI.UILabel lblSealing;
        private Sunny.UI.UITextBox txtSealingSound;
        private Sunny.UI.UIButton btnBrowseSealing;
        private Sunny.UI.UIButton btnTestSealing;
        private Sunny.UI.UITrackBar trbSealingVolume;
        private Sunny.UI.UILabel lblSealingVolume;
        
        // 开奖
        private Sunny.UI.UILabel lblLottery;
        private Sunny.UI.UITextBox txtLotterySound;
        private Sunny.UI.UIButton btnBrowseLottery;
        private Sunny.UI.UIButton btnTestLottery;
        private Sunny.UI.UITrackBar trbLotteryVolume;
        private Sunny.UI.UILabel lblLotteryVolume;
        
        // 上分
        private Sunny.UI.UILabel lblCreditUp;
        private Sunny.UI.UITextBox txtCreditUpSound;
        private Sunny.UI.UIButton btnBrowseCreditUp;
        private Sunny.UI.UIButton btnTestCreditUp;
        private Sunny.UI.UITrackBar trbCreditUpVolume;
        private Sunny.UI.UILabel lblCreditUpVolume;
        
        // 下分
        private Sunny.UI.UILabel lblCreditDown;
        private Sunny.UI.UITextBox txtCreditDownSound;
        private Sunny.UI.UIButton btnBrowseCreditDown;
        private Sunny.UI.UIButton btnTestCreditDown;
        private Sunny.UI.UITrackBar trbCreditDownVolume;
        private Sunny.UI.UILabel lblCreditDownVolume;
    }
}

