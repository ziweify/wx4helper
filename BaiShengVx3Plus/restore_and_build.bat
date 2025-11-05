@echo off
chcp 65001 >nul
cd /d "%~dp0"
echo 正在恢复 NuGet 包...
dotnet restore
if %errorlevel% neq 0 (
    echo 恢复失败！
    pause
    exit /b 1
)

echo.
echo 正在编译项目...
dotnet build --configuration Debug
if %errorlevel% neq 0 (
    echo 编译失败！
    pause
    exit /b 1
)

echo.
echo ✅ 编译成功！
pause

