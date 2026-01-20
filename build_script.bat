@echo off
chcp 65001 >nul
cd /d "%~dp0永利系统"
echo 正在恢复 NuGet 包...
dotnet restore
echo.
echo 正在编译项目...
dotnet build --no-restore
if %ERRORLEVEL% EQU 0 (
    echo.
    echo 编译成功！
) else (
    echo.
    echo 编译失败！
    exit /b 1
)
