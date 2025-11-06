@echo off
chcp 65001 >nul
echo ========================================
echo 编译 BaiShengVx3Plus 项目
echo ========================================
echo.

cd /d "%~dp0"
echo 正在编译...
echo.

dotnet build --no-restore 2>&1

if %errorlevel% equ 0 (
    echo.
    echo ========================================
    echo 编译成功！
    echo ========================================
) else (
    echo.
    echo ========================================
    echo 编译失败，请检查错误信息
    echo ========================================
)

echo.
echo 按任意键退出...
pause >nul
