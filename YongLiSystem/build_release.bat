@echo off
chcp 65001 >nul
echo ===================================
echo 永利系统 - Release 构建脚本
echo ===================================
echo.

cd /d "%~dp0"

echo [1/3] 清理旧的构建文件...
if exist "bin\Release" rd /s /q "bin\Release"
if exist "obj\Release" rd /s /q "obj\Release"
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

echo [3/3] 构建 Release 版本...
dotnet build --configuration Release
if errorlevel 1 (
    echo 构建失败！
    pause
    exit /b 1
)
echo.

echo ===================================
echo ✅ Release 构建成功！
echo 输出目录: bin\Release\net8.0-windows
echo ===================================
echo.

pause

