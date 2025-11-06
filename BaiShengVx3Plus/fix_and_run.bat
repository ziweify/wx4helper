@echo off
chcp 65001 >nul
cd /d "%~dp0"

echo ========================================
echo   SQLite 原生 DLL 问题修复
echo ========================================
echo.

echo [1/5] 清理旧的编译输出...
if exist "bin" rd /s /q "bin"
if exist "obj" rd /s /q "obj"
echo ✅ 清理完成
echo.

echo [2/5] 恢复 NuGet 包...
dotnet restore --force
if %errorlevel% neq 0 (
    echo ❌ NuGet 恢复失败！
    pause
    exit /b 1
)
echo ✅ NuGet 恢复完成
echo.

echo [3/5] 编译项目...
dotnet build --configuration Debug
if %errorlevel% neq 0 (
    echo ❌ 编译失败！
    pause
    exit /b 1
)
echo ✅ 编译成功
echo.

echo [4/5] 检查 SQLite 原生 DLL...
set OUTPUT_DIR=bin\Debug\net8.0-windows

if exist "%OUTPUT_DIR%\e_sqlite3.dll" (
    echo ✅ 找到 e_sqlite3.dll
) else if exist "%OUTPUT_DIR%\x64\e_sqlite3.dll" (
    echo ✅ 找到 x64\e_sqlite3.dll
    copy "%OUTPUT_DIR%\x64\e_sqlite3.dll" "%OUTPUT_DIR%\" >nul 2>&1
    echo ✅ 已复制到主目录
) else if exist "%OUTPUT_DIR%\runtimes\win-x64\native\e_sqlite3.dll" (
    echo ✅ 找到 runtimes\win-x64\native\e_sqlite3.dll
    copy "%OUTPUT_DIR%\runtimes\win-x64\native\e_sqlite3.dll" "%OUTPUT_DIR%\" >nul 2>&1
    echo ✅ 已复制到主目录
) else if exist "%OUTPUT_DIR%\SQLite.Interop.dll" (
    echo ✅ 找到 SQLite.Interop.dll (System.Data.SQLite)
) else if exist "%OUTPUT_DIR%\x64\SQLite.Interop.dll" (
    echo ✅ 找到 x64\SQLite.Interop.dll
) else (
    echo ⚠️  警告: 未找到任何 SQLite 原生 DLL
    echo.
    echo 正在搜索所有可能的位置...
    dir /s /b "%OUTPUT_DIR%\*.dll" | findstr /i "sqlite"
    echo.
)
echo.

echo [5/5] 运行程序...
echo ========================================
echo   🚀 启动 BaiShengVx3Plus
echo ========================================
echo.

dotnet run --configuration Debug --no-build

echo.
echo ========================================
echo   程序已退出
echo ========================================
pause

