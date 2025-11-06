@echo off
chcp 65001 >nul
cd /d "%~dp0"

echo ========================================
echo   BaiShengVx3Plus 诊断编译和运行
echo ========================================
echo.

echo [步骤1] 清理旧文件...
if exist "bin" rd /s /q "bin"
if exist "obj" rd /s /q "obj"
echo ✅ 清理完成
echo.

echo [步骤2] 恢复 NuGet 包...
dotnet restore --force
if %errorlevel% neq 0 (
    echo ❌ NuGet 恢复失败
    pause
    exit /b 1
)
echo ✅ NuGet 恢复完成
echo.

echo [步骤3] 编译项目...
dotnet build --configuration Debug
if %errorlevel% neq 0 (
    echo ❌ 编译失败
    pause
    exit /b 1
)
echo ✅ 编译成功
echo.

echo [步骤4] 检查 SQLite DLL...
set OUTPUT_DIR=bin\Debug\net8.0-windows

echo 检查主目录...
if exist "%OUTPUT_DIR%\e_sqlite3.dll" (
    echo ✅ 找到 e_sqlite3.dll
    goto :run
)

if exist "%OUTPUT_DIR%\SQLite.Interop.dll" (
    echo ✅ 找到 SQLite.Interop.dll
    goto :run
)

echo 检查 x64 子目录...
if exist "%OUTPUT_DIR%\x64\e_sqlite3.dll" (
    echo ✅ 找到 x64\e_sqlite3.dll，正在复制...
    copy "%OUTPUT_DIR%\x64\e_sqlite3.dll" "%OUTPUT_DIR%\" >nul 2>&1
    goto :run
)

if exist "%OUTPUT_DIR%\x64\SQLite.Interop.dll" (
    echo ✅ 找到 x64\SQLite.Interop.dll，正在复制...
    copy "%OUTPUT_DIR%\x64\SQLite.Interop.dll" "%OUTPUT_DIR%\" >nul 2>&1
    goto :run
)

echo 检查 runtimes 子目录...
if exist "%OUTPUT_DIR%\runtimes\win-x64\native\e_sqlite3.dll" (
    echo ✅ 找到 runtimes\win-x64\native\e_sqlite3.dll，正在复制...
    copy "%OUTPUT_DIR%\runtimes\win-x64\native\e_sqlite3.dll" "%OUTPUT_DIR%\" >nul 2>&1
    goto :run
)

if exist "%OUTPUT_DIR%\runtimes\win-x64\native\sqlite3.dll" (
    echo ✅ 找到 runtimes\win-x64\native\sqlite3.dll，正在复制为 e_sqlite3.dll...
    copy "%OUTPUT_DIR%\runtimes\win-x64\native\sqlite3.dll" "%OUTPUT_DIR%\e_sqlite3.dll" >nul 2>&1
    goto :run
)

echo.
echo ⚠️  警告: 未找到 SQLite 原生 DLL
echo.
echo 正在搜索所有 SQLite 相关的 DLL...
echo.
for /r "%OUTPUT_DIR%" %%f in (*.dll) do (
    echo %%f | findstr /i "sqlite" >nul 2>&1
    if not errorlevel 1 echo %%f
)
echo.
echo 尝试从 libs 目录复制...
if exist "libs\e_sqlite3.dll" (
    copy "libs\e_sqlite3.dll" "%OUTPUT_DIR%\" >nul 2>&1
    echo ✅ 已从 libs 复制 e_sqlite3.dll
) else (
    echo ❌ libs\e_sqlite3.dll 不存在
    echo.
    echo 请先运行: find_and_copy_sqlite_dll.bat
    pause
    exit /b 1
)

:run
echo.
echo [步骤5] 运行程序（带诊断）...
echo ========================================
echo   🚀 启动 BaiShengVx3Plus (诊断模式)
echo ========================================
echo.
echo 注意: 程序会显示多个诊断对话框，请依次点击"确定"
echo.

dotnet run --configuration Debug --no-build

echo.
echo ========================================
echo   程序已退出
echo ========================================
echo.
echo 如果看到错误对话框，请记录错误信息。
echo.
pause

