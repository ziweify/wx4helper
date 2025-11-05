@echo off
chcp 65001
echo ========================================
echo 恢复缺失的 WeChat 和 Contact 服务文件
echo ========================================
echo.

cd /d D:\gitcode\wx4helper

echo 正在恢复 ContactDataService.cs...
git checkout HEAD~1 -- BaiShengVx3Plus/Services/ContactDataService.cs
if %errorlevel% == 0 (
    echo [OK] ContactDataService.cs 已恢复
) else (
    echo [ERROR] ContactDataService.cs 恢复失败，尝试更早的版本...
    git checkout HEAD~2 -- BaiShengVx3Plus/Services/ContactDataService.cs
)

echo.
echo 正在恢复 WeChatService.cs...
git checkout HEAD~1 -- BaiShengVx3Plus/Services/WeChatService.cs
if %errorlevel% == 0 (
    echo [OK] WeChatService.cs 已恢复
) else (
    echo [ERROR] WeChatService.cs 恢复失败，尝试更早的版本...
    git checkout HEAD~2 -- BaiShengVx3Plus/Services/WeChatService.cs
)

echo.
echo 正在恢复 WeChatLoaderService.cs...
git checkout HEAD~1 -- BaiShengVx3Plus/Services/WeChatLoaderService.cs
if %errorlevel% == 0 (
    echo [OK] WeChatLoaderService.cs 已恢复
) else (
    echo [ERROR] WeChatLoaderService.cs 恢复失败，尝试更早的版本...
    git checkout HEAD~2 -- BaiShengVx3Plus/Services/WeChatLoaderService.cs
)

echo.
echo 正在恢复 WeixinSocketClient.cs...
git checkout HEAD~1 -- BaiShengVx3Plus/Services/WeixinSocketClient.cs
if %errorlevel% == 0 (
    echo [OK] WeixinSocketClient.cs 已恢复
) else (
    echo [ERROR] WeixinSocketClient.cs 恢复失败，尝试更早的版本...
    git checkout HEAD~2 -- BaiShengVx3Plus/Services/WeixinSocketClient.cs
)

echo.
echo ========================================
echo 恢复完成！
echo ========================================
echo.
echo 文件已恢复到 BaiShengVx3Plus\Services\ 目录
echo.
echo 接下来需要：
echo 1. 在 Visual Studio 中查看这些文件
echo 2. 移动 ContactDataService.cs 到 Contact 文件夹
echo 3. 创建 WeChat 文件夹并移动 3 个文件
echo 4. 更新命名空间
echo.
pause

