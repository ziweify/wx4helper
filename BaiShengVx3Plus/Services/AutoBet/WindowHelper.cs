using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace BaiShengVx3Plus.Services.AutoBet
{
    /// <summary>
    /// Windows 窗口辅助工具 - 用于查找已存在的浏览器窗口
    /// </summary>
    public static class WindowHelper
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr FindWindow(string? lpClassName, string? lpWindowName);
        
        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);
        
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
        
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
        
        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);
        
        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
        
        /// <summary>
        /// 通过窗口标题查找进程
        /// </summary>
        /// <param name="windowTitle">窗口标题（精确匹配或开头匹配）</param>
        /// <param name="exactMatch">是否精确匹配，false 则只匹配开头</param>
        /// <returns>找到的进程，如果没找到返回 null</returns>
        public static Process? FindProcessByWindowTitle(string windowTitle, bool exactMatch = true)
        {
            Process? foundProcess = null;
            
            EnumWindows((hWnd, lParam) =>
            {
                if (!IsWindowVisible(hWnd))
                {
                    return true; // 跳过不可见窗口
                }
                
                var sb = new StringBuilder(256);
                GetWindowText(hWnd, sb, sb.Capacity);
                var title = sb.ToString();
                
                bool isMatch = exactMatch 
                    ? title == windowTitle 
                    : title.StartsWith(windowTitle, StringComparison.OrdinalIgnoreCase);
                
                if (isMatch)
                {
                    // 获取进程 ID
                    GetWindowThreadProcessId(hWnd, out int processId);
                    
                    try
                    {
                        foundProcess = Process.GetProcessById(processId);
                        return false; // 找到了，停止枚举
                    }
                    catch
                    {
                        // 进程可能已退出
                    }
                }
                
                return true; // 继续枚举
            }, IntPtr.Zero);
            
            return foundProcess;
        }
        
        /// <summary>
        /// 查找所有匹配标题的窗口进程
        /// </summary>
        public static List<Process> FindAllProcessesByWindowTitle(string windowTitle, bool exactMatch = true)
        {
            var processes = new List<Process>();
            
            EnumWindows((hWnd, lParam) =>
            {
                if (!IsWindowVisible(hWnd))
                {
                    return true;
                }
                
                var sb = new StringBuilder(256);
                GetWindowText(hWnd, sb, sb.Capacity);
                var title = sb.ToString();
                
                bool isMatch = exactMatch 
                    ? title == windowTitle 
                    : title.StartsWith(windowTitle, StringComparison.OrdinalIgnoreCase);
                
                if (isMatch)
                {
                    GetWindowThreadProcessId(hWnd, out int processId);
                    
                    try
                    {
                        var process = Process.GetProcessById(processId);
                        if (!processes.Exists(p => p.Id == process.Id))
                        {
                            processes.Add(process);
                        }
                    }
                    catch
                    {
                        // 进程可能已退出
                    }
                }
                
                return true;
            }, IntPtr.Zero);
            
            return processes;
        }
        
        /// <summary>
        /// 检查指定标题的窗口是否存在
        /// </summary>
        public static bool WindowExists(string windowTitle)
        {
            return FindProcessByWindowTitle(windowTitle) != null;
        }
    }
}

