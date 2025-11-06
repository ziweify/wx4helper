@echo off
chcp 65001 >nul
cd /d "%~dp0"

echo ========================================
echo   恢复 NuGet 包和编译项目
echo ========================================
echo.

echo [1/3] 清理旧的编译输出...
dotnet clean --configuration Debug
echo.

echo [2/3] 恢复 NuGet 包 (包括 sqlite3.dll)...
dotnet restore
echo.

echo [3/3] 编译项目...
dotnet build --configuration Debug
echo.

if %errorlevel% equ 0 (
    echo ========================================
    echo   ✅ 编译成功！
    echo ========================================
    echo.
    echo 检查 SQLite 原生库是否存在:
    if exist "bin\Debug\net8.0-windows\runtimes\win-x64\native\sqlite3.dll" (
        echo ✅ sqlite3.dll 已复制到输出目录
    ) else (
        echo ❌ 警告: sqlite3.dll 未找到
    )
    echo.
    echo 输出目录: bin\Debug\net8.0-windows\
) else (
    echo ========================================
    echo   ❌ 编译失败！
    echo ========================================
)

echo.
pause
