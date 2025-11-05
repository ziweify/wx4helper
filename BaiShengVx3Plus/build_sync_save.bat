@echo off
chcp 65001 >nul
echo ========================================
echo 编译 BaiShengVx3Plus (同步立即保存功能)
echo ========================================
echo.

cd /d "%~dp0"

echo [1/2] 清理项目...
dotnet clean --configuration Debug

echo.
echo [2/2] 编译项目...
dotnet build --configuration Debug

echo.
if %ERRORLEVEL% EQU 0 (
    echo ✓ 编译成功！
    echo.
    echo 输出目录: bin\Debug\net8.0-windows\
    echo.
) else (
    echo ✗ 编译失败！
    echo.
    pause
    exit /b 1
)

pause

