@echo off
chcp 65001 >nul
cd /d "%~dp0"
echo ========================================
echo   开始编译 BaiShengVx3Plus
echo ========================================
dotnet build --no-incremental
echo.
echo ========================================
echo   编译完成
echo ========================================
pause

