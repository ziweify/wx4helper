using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace YongLiSystem.Helpers
{
    /// <summary>
    /// 工具栏图标生成器 - 将图标保存为PNG文件，供设计器使用
    /// </summary>
    public static class ToolbarIconGenerator
    {
        /// <summary>
        /// 生成所有工具栏图标并保存到指定目录
        /// </summary>
        /// <param name="outputDirectory">输出目录路径</param>
        public static void GenerateAllIcons(string outputDirectory)
        {
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            // 生成所有图标
            SaveIcon(ToolbarIconHelper.CreateConnectIcon(), Path.Combine(outputDirectory, "Connect.png"));
            SaveIcon(ToolbarIconHelper.CreateLogIcon(), Path.Combine(outputDirectory, "Log.png"));
            SaveIcon(ToolbarIconHelper.CreateLotteryResultIcon(), Path.Combine(outputDirectory, "LotteryResult.png"));
            SaveIcon(ToolbarIconHelper.CreateCreditManageIcon(), Path.Combine(outputDirectory, "CreditManage.png"));
            SaveIcon(ToolbarIconHelper.CreateClearDataIcon(), Path.Combine(outputDirectory, "ClearData.png"));
            SaveIcon(ToolbarIconHelper.CreateSettingsIcon(), Path.Combine(outputDirectory, "Settings.png"));

            MessageBox.Show(
                $"图标已成功生成到：\n{outputDirectory}\n\n" +
                "请按以下步骤在设计器中添加：\n" +
                "1. 在设计器中选中 imageList_Toolbar\n" +
                "2. 在属性面板中点击 Images 属性的 ... 按钮\n" +
                "3. 点击 Add 按钮，选择生成的 PNG 文件\n" +
                "4. 为每个按钮设置对应的 ImageKey",
                "图标生成成功",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        /// <summary>
        /// 将 Bitmap 保存为 PNG 文件
        /// </summary>
        private static void SaveIcon(Bitmap icon, string filePath)
        {
            icon.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
            icon.Dispose();
        }
    }
}

