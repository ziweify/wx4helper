@echo off
chcp 65001 >nul
echo ===================================
echo 永利系统 - 构建和运行脚本
echo ===================================
echo.

cd /d "%~dp0"

echo [1/3] 清理旧的构建文件...
if exist "bin\Debug" rd /s /q "bin\Debug"
if exist "obj\Debug" rd /s /q "obj\Debug"
echo 清理完成。
echo.

echo [2/3] 还原 NuGet 包...
dotnet restore
if errorlevel 1 (
    echo 还原失败！
    pause
    exit /b 1
)
echo 还原完成。
echo.

echo [3/3] 构建项目...
dotnet build --configuration Debug
if errorlevel 1 (
    echo 构建失败！
    pause
    exit /b 1
)
echo 构建完成。
echo.

echo ===================================
echo 构建成功！正在启动应用程序...
echo ===================================
echo.

dotnet run --no-build --configuration Debug

pause

