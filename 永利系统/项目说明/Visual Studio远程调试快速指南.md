# Visual Studio 远程调试配置指南（Windows 11）

## 📋 适用场景
- **本地开发**: Visual Studio 2022 + Windows 11
- **远程调试**: Windows 11 目标机器
- **调试项目**: 永利系统、BaiShengVx3Plus (.NET 8 WinForms)

---

## 🎯 工作原理

```
┌────────────────────────────────────┐
│  本地开发机 (Visual Studio 2022)   │
│  ├─ 编写代码                       │
│  ├─ 编译项目                       │
│  └─ 设置断点、单步调试             │
└────────────────────────────────────┘
            ↓ 网络连接 (TCP 4024)
┌────────────────────────────────────┐
│  远程 Win11 (Remote Debugger)      │
│  ├─ 运行程序                       │
│  └─ 接受调试指令                   │
└────────────────────────────────────┘
```

**优势：**
- ✅ 代码在本地（快速编辑）
- ✅ 程序在远程运行（真实环境）
- ✅ 完整的 VS 调试功能（断点、变量查看、调用堆栈等）

---

## 📦 第一步：远程机器配置（一次性，5分钟）

### 1.1 下载 Remote Tools for Visual Studio 2022

**下载地址：**
```
https://visualstudio.microsoft.com/zh-hans/downloads/
→ 滚动到 "用于 Visual Studio 2022 的远程工具"
→ 下载 x64 版本
```

或直接链接：
```
https://aka.ms/vs/17/release/RemoteTools.amd64ret.enu.exe
```

### 1.2 安装 Remote Tools

在远程 Windows 11 机器上：

```
1. 双击运行 RemoteTools.amd64ret.enu.exe
2. 选择安装路径（默认即可）
3. 点击"安装"
4. 等待安装完成
```

安装位置：
```
C:\Program Files (x86)\Microsoft Visual Studio\2022\Remote Debugger\
```

### 1.3 配置防火墙（重要！）

**方法 A：使用配置向导（推荐）**

在远程机器上运行：
```
C:\Program Files (x86)\Microsoft Visual Studio\2022\Remote Debugger\rdbgwiz.exe
```

配置选项：
```
☑ 配置远程调试监视器
☑ 配置 Windows 防火墙以允许远程调试
```

**方法 B：手动配置（管理员权限的 PowerShell）**

```powershell
# 允许 Remote Debugger 通过防火墙
New-NetFirewallRule -DisplayName "Visual Studio Remote Debugger" `
    -Direction Inbound `
    -Program "C:\Program Files (x86)\Microsoft Visual Studio\2022\Remote Debugger\x64\msvsmon.exe" `
    -Action Allow
```

### 1.4 启动 Remote Debugger

**方式 1：手动启动（开发时）**

双击运行：
```
C:\Program Files (x86)\Microsoft Visual Studio\2022\Remote Debugger\x64\msvsmon.exe
```

启动后界面：
```
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  Visual Studio 2022 Remote Debugger
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  服务器名称: WIN11-PC:4024
  
  [✓] 正在运行
  [✓] 等待连接...
  
  工具 → 选项：
    ☑ 允许任何用户调试（开发环境）
    身份验证模式: 无身份验证
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
```

**重要：记下服务器名称！** 例如：`WIN11-PC:4024` 或 `192.168.1.100:4024`

**方式 2：创建快捷方式（推荐）**

在远程机器桌面创建快捷方式：
```
目标: "C:\Program Files (x86)\Microsoft Visual Studio\2022\Remote Debugger\x64\msvsmon.exe"
名称: Remote Debugger
```

每次需要远程调试时，双击运行即可。

---

## 💻 第二步：本地 Visual Studio 配置

### 2.1 打开项目

在本地 Visual Studio 中打开：
```
E:\gitcode\wx4helper\wx4helper.sln
```

### 2.2 编译项目

```
生成 → 重新生成解决方案 (Ctrl+Shift+B)

或单独编译：
- 永利系统: 右键"永利系统"项目 → 生成
- BaiShengVx3Plus: 右键"BaiShengVx3Plus"项目 → 生成
```

确认编译成功：
```
输出窗口显示：
========== 生成: 1 成功，0 失败，0 最新 ==========
```

### 2.3 部署到远程机器

**方法 A：使用网络共享（推荐）**

```powershell
# 在本地 PowerShell 中运行

# 1. 设置变量
$RemotePC = "192.168.1.100"  # 替换为远程机器 IP
$ProjectName = "永利系统"     # 或 "BaiShengVx3Plus"

# 2. 复制编译输出到远程
$Source = ".\$ProjectName\bin\Debug\net8.0-windows\*"
$Destination = "\\$RemotePC\C$\RemoteDebug\$ProjectName\"

# 创建目录
New-Item -Path $Destination -ItemType Directory -Force

# 复制文件
Copy-Item -Path $Source -Destination $Destination -Recurse -Force

Write-Host "部署完成！" -ForegroundColor Green
Write-Host "远程路径: C:\RemoteDebug\$ProjectName" -ForegroundColor Yellow
```

**方法 B：手动复制（简单）**

1. 在远程机器创建文件夹：
   ```
   C:\RemoteDebug\永利系统\
   C:\RemoteDebug\BaiShengVx3Plus\
   ```

2. 从本地复制整个文件夹：
   ```
   E:\gitcode\wx4helper\永利系统\bin\Debug\net8.0-windows\
   ```
   到远程：
   ```
   C:\RemoteDebug\永利系统\
   ```

**重要：确保复制了 .pdb 文件！**
```
必须复制的文件：
✓ 永利系统.exe          (程序)
✓ 永利系统.pdb          (符号文件，调试必需！)
✓ 永利系统.dll
✓ *.dll                 (所有依赖)
✓ config.json           (配置文件)
```

---

## 🐛 第三步：开始远程调试

### 3.1 在远程机器运行程序

在远程 Win11 上，双击运行：
```
C:\RemoteDebug\永利系统\永利系统.exe
```

或
```
C:\RemoteDebug\BaiShengVx3Plus\BaiShengVx3Plus.exe
```

### 3.2 在本地 VS 附加调试器

**步骤：**

1. **打开"附加到进程"对话框**
   ```
   调试 → 附加到进程 (Ctrl+Alt+P)
   ```

2. **配置连接**
   ```
   连接类型: 远程 (无身份验证)
   连接目标: 192.168.1.100:4024
   ```
   
   （替换为远程机器的实际 IP 或计算机名）

3. **刷新进程列表**
   ```
   点击"刷新"按钮
   ```

4. **选择要调试的进程**
   ```
   在列表中找到：
   - 永利系统.exe
   或
   - BaiShengVx3Plus.exe
   ```
   
   勾选"显示所有用户的进程"（如果看不到）

5. **附加调试器**
   ```
   点击"附加"按钮
   ```

6. **选择代码类型（如果提示）**
   ```
   ☑ 托管代码 (.NET Core, .NET 5+)
   ```

### 3.3 设置断点并调试

现在您可以：

```
✓ 在代码中设置断点（点击左边距或按 F9）
✓ 在远程程序中触发断点
✓ 单步执行（F10, F11）
✓ 查看变量值
✓ 修改变量值
✓ 查看调用堆栈
✓ 使用即时窗口
```

---

## 🔧 快速部署脚本（推荐使用）

创建 `quick-deploy.ps1` 在项目根目录：

```powershell
<#
.SYNOPSIS
    快速部署到远程机器
.EXAMPLE
    .\quick-deploy.ps1 -RemoteIP "192.168.1.100" -Project "永利系统"
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$RemoteIP,
    
    [Parameter(Mandatory=$true)]
    [ValidateSet("永利系统", "BaiShengVx3Plus")]
    [string]$Project
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "   快速部署工具" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "项目: $Project" -ForegroundColor Yellow
Write-Host "目标: $RemoteIP" -ForegroundColor Yellow
Write-Host ""

# 1. 编译
Write-Host "[1/3] 正在编译 $Project..." -ForegroundColor Green
dotnet build ".\$Project\$Project.csproj" --configuration Debug --nologo --verbosity minimal

if ($LASTEXITCODE -ne 0) {
    Write-Host "编译失败！" -ForegroundColor Red
    exit 1
}
Write-Host "  ✓ 编译成功" -ForegroundColor Green

# 2. 停止远程进程
Write-Host "[2/3] 正在停止远程进程..." -ForegroundColor Green
try {
    Invoke-Command -ComputerName $RemoteIP -ScriptBlock {
        param($ProcessName)
        $proc = Get-Process -Name $ProcessName -ErrorAction SilentlyContinue
        if ($proc) {
            $proc | Stop-Process -Force
            Start-Sleep -Seconds 1
        }
    } -ArgumentList $Project -ErrorAction SilentlyContinue
    Write-Host "  ✓ 进程已停止" -ForegroundColor Green
} catch {
    Write-Host "  ! 无法停止远程进程（可能未运行）" -ForegroundColor Yellow
}

# 3. 部署文件
Write-Host "[3/3] 正在部署文件..." -ForegroundColor Green
$Source = ".\$Project\bin\Debug\net8.0-windows\*"
$Destination = "\\$RemoteIP\C$\RemoteDebug\$Project\"

New-Item -Path $Destination -ItemType Directory -Force | Out-Null
Copy-Item -Path $Source -Destination $Destination -Recurse -Force

Write-Host "  ✓ 部署完成" -ForegroundColor Green

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "部署成功！" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "下一步操作：" -ForegroundColor Yellow
Write-Host "1. 在远程机器运行: C:\RemoteDebug\$Project\$Project.exe"
Write-Host "2. 在 VS 中: 调试 → 附加到进程 → ${RemoteIP}:4024"
Write-Host "3. 选择进程: $Project.exe"
Write-Host ""
```

**使用：**

```powershell
# 部署永利系统
.\quick-deploy.ps1 -RemoteIP "192.168.1.100" -Project "永利系统"

# 部署 BaiShengVx3Plus
.\quick-deploy.ps1 -RemoteIP "192.168.1.100" -Project "BaiShengVx3Plus"
```

---

## ❓ 常见问题（Windows 11 特定）

### Q1: 无法访问远程网络共享（\\192.168.1.100\C$）

**原因：** Windows 11 默认禁用了管理共享

**解决方案：**

在远程 Windows 11 上（管理员 PowerShell）：

```powershell
# 1. 启用管理共享
Set-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Services\LanmanServer\Parameters" `
    -Name "AutoShareWks" -Value 1

# 2. 启用文件和打印机共享
Set-NetFirewallRule -DisplayGroup "文件和打印机共享" -Enabled True -Profile Private

# 3. 重启 Server 服务
Restart-Service -Name LanmanServer -Force

# 4. 验证共享
Get-SmbShare
```

### Q2: 找不到远程进程

**检查清单：**

```
☐ 远程程序正在运行
  → 在远程机器：任务管理器 → 详细信息 → 查找 永利系统.exe

☐ Remote Debugger 正在运行
  → 窗口标题显示 "正在运行"

☐ 使用管理员权限启动 msvsmon.exe
  → 右键 → 以管理员身份运行

☐ 防火墙已配置
  → 运行防火墙配置向导

☐ 选择了正确的身份验证模式
  → msvsmon.exe → 工具 → 选项 → 无身份验证
```

### Q3: 断点显示为灰色空心圆 ⭕

**原因：** 符号文件（.pdb）未加载

**解决：**

```
1. 确认 .pdb 文件已部署
   远程机器应该有：C:\RemoteDebug\永利系统\永利系统.pdb

2. 手动加载符号
   调试 → 窗口 → 模块
   找到"永利系统.dll" → 右键 → 加载符号
   选择远程机器上的 .pdb 文件

3. 配置符号路径
   调试 → 选项 → 调试 → 符号
   添加路径: \\192.168.1.100\C$\RemoteDebug\永利系统

4. 确保版本一致
   本地代码版本必须与远程程序版本完全一致
   重新编译并部署所有文件
```

### Q4: PowerShell Remoting 连接失败

**启用 PowerShell Remoting（远程 Win11）：**

```powershell
# 以管理员身份运行
Enable-PSRemoting -Force -SkipNetworkProfileCheck

# 配置防火墙
Set-NetFirewallRule -Name "WINRM-HTTP-In-TCP" -RemoteAddress Any

# 测试连接（本地）
Test-WSMan -ComputerName 192.168.1.100
```

---

## 📝 日常使用流程

### 典型的一天

```
┌─────────────────────────────────────────┐
│ 早上 - 准备环境                          │
├─────────────────────────────────────────┤
│ 1. 在远程机器启动 Remote Debugger       │
│    (双击桌面快捷方式)                    │
│                                         │
│ 2. 在本地打开 Visual Studio            │
│    (打开 wx4helper.sln)                 │
└─────────────────────────────────────────┘

┌─────────────────────────────────────────┐
│ 开发 - 迭代循环                          │
├─────────────────────────────────────────┤
│ 1. 在本地修改代码                        │
│ 2. 编译项目 (Ctrl+Shift+B)             │
│ 3. 运行部署脚本                          │
│    .\quick-deploy.ps1                   │
│ 4. 在远程运行程序                        │
│ 5. 在 VS 附加调试器                      │
│ 6. 测试、调试                            │
│ 7. 重复 1-6                              │
└─────────────────────────────────────────┘

┌─────────────────────────────────────────┐
│ 晚上 - 清理                              │
├─────────────────────────────────────────┤
│ 1. 提交代码到 Git                        │
│ 2. 关闭远程程序和 Remote Debugger       │
│ 3. 关闭 Visual Studio                   │
└─────────────────────────────────────────┘
```

---

## 🎯 性能优化建议

### 加快部署速度

```powershell
# 使用 Robocopy 增量复制（只复制变更的文件）
$Source = ".\永利系统\bin\Debug\net8.0-windows\"
$Dest = "\\192.168.1.100\C$\RemoteDebug\永利系统\"

robocopy $Source $Dest /MIR /Z /W:5 /R:3 /NDL /NFL /NJH /NJS
```

### 加快调试速度

```
调试 → 选项 → 调试 → 常规：

☐ 启用属性求值和其他隐式函数调用
  (减少网络往返)

☐ 在调试时启用诊断工具
  (减少性能开销)

☑ 使用托管兼容模式
  (提高远程调试性能)
```

---

## 📞 获取帮助

如果遇到问题：

1. **查看 Remote Debugger 窗口的输出**
   - 显示连接状态和错误信息

2. **查看 VS 输出窗口**
   - 视图 → 输出 → 显示输出来源：调试

3. **启用详细日志**
   ```
   调试 → 选项 → 调试 → 常规
   ☑ 启用诊断模式
   ```

---

**文档版本**: 2.0 (Windows 11 专用)  
**最后更新**: 2025-12-26  
**适用项目**: 永利系统、BaiShengVx3Plus

