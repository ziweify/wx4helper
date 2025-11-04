# Loader DLL 编译说明

## 方法 1：使用 Visual Studio（推荐）

这是最简单的方法：

1. **打开项目文件**
   ```
   双击打开: D:\gitcode\wx4helper\Loader\Loader.vcxproj
   ```

2. **选择配置**
   - 在顶部工具栏找到配置下拉框
   - 选择 **Release**
   - 选择 **x64**

3. **生成项目**
   - 右键点击解决方案资源管理器中的 "Loader" 项目
   - 选择 "生成"
   - 等待编译完成（约10-20秒）

4. **查看输出**
   ```
   D:\gitcode\wx4helper\Loader\x64\Release\Loader.dll
   ```

5. **验证成功**
   - 检查输出窗口显示 "0 个错误"
   - 确认 DLL 文件存在
   - DLL 应该自动复制到 BaiShengVx3Plus\bin\Release\net8.0-windows\

## 方法 2：使用批处理脚本

1. **运行脚本**
   ```
   双击运行: D:\gitcode\wx4helper\Loader\build.bat
   ```

2. **查看结果**
   - 脚本会自动查找 MSBuild
   - 编译 Release x64 配置
   - 自动复制 DLL 到输出目录

## 方法 3：使用 Developer Command Prompt

1. **打开 Developer Command Prompt**
   - 开始菜单搜索 "Developer Command Prompt for VS 2022"
   - 或 "Developer Command Prompt for VS 2019"

2. **导航到项目目录**
   ```cmd
   cd /d D:\gitcode\wx4helper\Loader
   ```

3. **编译项目**
   ```cmd
   msbuild Loader.vcxproj /p:Configuration=Release /p:Platform=x64
   ```

4. **查看输出**
   ```cmd
   dir x64\Release\Loader.dll
   ```

## 编译后的文件位置

编译成功后，DLL 会在以下位置：

```
源文件:
D:\gitcode\wx4helper\Loader\x64\Release\Loader.dll

自动复制到:
D:\gitcode\wx4helper\BaiShengVx3Plus\bin\Release\net8.0-windows\Loader.dll
D:\gitcode\wx4helper\BaiShengVx3Plus\bin\Debug\net8.0-windows\Loader.dll
```

## 验证编译成功

### 1. 检查文件存在
```cmd
dir Loader\x64\Release\Loader.dll
```

### 2. 检查文件大小
正常的 DLL 文件应该在 20-50 KB 左右

### 3. 使用 Dependencies.exe 检查
下载 [Dependencies](https://github.com/lucasg/Dependencies/releases)
```
打开 Loader.dll
查看导出函数:
- LaunchWeChatWithInjection
- InjectDllToProcess
- GetWeChatProcesses
```

## 常见问题

### Q1: 找不到 MSBuild.exe
**解决方案:**
- 确保已安装 Visual Studio 2019 或 2022
- 确保安装了 "C++ 桌面开发" 工作负载

### Q2: 缺少 Windows SDK
**解决方案:**
- 打开 Visual Studio Installer
- 修改 Visual Studio
- 勾选 "Windows 10 SDK (10.0.xxxxx.x)"
- 安装

### Q3: 缺少 v142 平台工具集
**解决方案:**
- 打开 Visual Studio Installer
- 修改 Visual Studio
- 勾选 "MSVC v142 - VS 2019 C++ x64/x86 生成工具"
- 安装

### Q4: 编译成功但找不到 DLL
**解决方案:**
检查输出路径:
```
Loader\x64\Release\Loader.dll
```
如果在其他位置，手动复制到 BaiShengVx3Plus\bin\Release\net8.0-windows\

### Q5: DLL 无法加载
**解决方案:**
1. 使用 Dependencies.exe 检查依赖项
2. 确保编译的是 x64 版本
3. 确保运行环境有 Visual C++ Redistributable

## 编译选项说明

- **Configuration: Release** - 优化的发布版本
- **Platform: x64** - 64位版本（必须与 BaiShengVx3Plus 匹配）
- **C++ Standard: C++20** - 使用 C++20 标准
- **Platform Toolset: v142** - VS 2019 工具集
- **Windows SDK: 10.0** - Windows 10 SDK

## 下一步

编译成功后：

1. ✅ 确认 Loader.dll 在输出目录
2. ✅ 确认 WeixinX.dll 在输出目录
3. ✅ 运行 BaiShengVx3Plus.exe
4. ✅ 测试"绑定"和"获取列表"功能

## 需要帮助？

如果遇到问题：
1. 检查 Visual Studio 输出窗口的详细错误信息
2. 确保所有依赖项都已安装
3. 尝试清理并重新生成解决方案
4. 查看本目录下的其他文档

祝编译顺利！🚀

