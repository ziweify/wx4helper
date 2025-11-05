@echo off
chcp 65001
echo ========================================
echo 恢复被删除的服务文件
echo ========================================
echo.

cd /d D:\gitcode\wx4helper

echo 正在恢复文件...
echo.

REM 恢复 ContactDataService.cs
git checkout HEAD~1 -- BaiShengVx3Plus/Services/ContactDataService.cs
if %errorlevel% == 0 (
    echo [OK] ContactDataService.cs 已恢复
) else (
    echo [ERROR] ContactDataService.cs 恢复失败
)

REM 恢复 UserInfoService.cs
git checkout HEAD~1 -- BaiShengVx3Plus/Services/UserInfoService.cs
if %errorlevel% == 0 (
    echo [OK] UserInfoService.cs 已恢复
) else (
    echo [ERROR] UserInfoService.cs 恢复失败
)

REM 恢复 WeChatService.cs
git checkout HEAD~1 -- BaiShengVx3Plus/Services/WeChatService.cs
if %errorlevel% == 0 (
    echo [OK] WeChatService.cs 已恢复
) else (
    echo [ERROR] WeChatService.cs 恢复失败
)

REM 恢复 WeChatLoaderService.cs
git checkout HEAD~1 -- BaiShengVx3Plus/Services/WeChatLoaderService.cs
if %errorlevel% == 0 (
    echo [OK] WeChatLoaderService.cs 已恢复
) else (
    echo [ERROR] WeChatLoaderService.cs 恢复失败
)

REM 恢复 WeixinSocketClient.cs
git checkout HEAD~1 -- BaiShengVx3Plus/Services/WeixinSocketClient.cs
if %errorlevel% == 0 (
    echo [OK] WeixinSocketClient.cs 已恢复
) else (
    echo [ERROR] WeixinSocketClient.cs 恢复失败
)

echo.
echo ========================================
echo 恢复完成！
echo ========================================
echo.
echo 接下来需要：
echo 1. 移动文件到对应的子文件夹
echo 2. 更新命名空间
echo 3. 在 Visual Studio 中重新加载项目
echo.
pause

