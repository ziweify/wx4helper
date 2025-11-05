@echo off
chcp 65001 >nul
cd /d "%~dp0"

echo ========================================
echo  ORM 重构 - 编译测试
echo ========================================
echo.

echo [1/3] 正在恢复 NuGet 包...
dotnet restore
if %errorlevel% neq 0 (
    echo.
    echo ❌ 恢复失败！
    pause
    exit /b 1
)

echo.
echo [2/3] 正在编译项目...
dotnet build --configuration Debug --no-restore
if %errorlevel% neq 0 (
    echo.
    echo ❌ 编译失败！请查看上面的错误信息
    pause
    exit /b 1
)

echo.
echo [3/3] 检查输出文件...
if exist "bin\Debug\net8.0-windows\BaiShengVx3Plus.exe" (
    echo ✅ 编译成功！
    echo.
    echo 输出文件: bin\Debug\net8.0-windows\BaiShengVx3Plus.exe
) else (
    echo ❌ 编译成功但未找到输出文件
)

echo.
echo ========================================
echo  编译完成
echo ========================================
pause

