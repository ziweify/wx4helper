@echo off
chcp 65001 >nul
echo ========================================
echo   查找并复制 SQLite 原生 DLL
echo ========================================
echo.

echo [1] 搜索本地 e_sqlite3.dll...
set FOUND=0

REM 搜索 NuGet 缓存
if exist "%USERPROFILE%\.nuget\packages\sqlitepclraw.lib.e_sqlite3" (
    echo 在 NuGet 缓存中找到 SQLitePCLRaw.lib.e_sqlite3
    for /r "%USERPROFILE%\.nuget\packages\sqlitepclraw.lib.e_sqlite3" %%f in (e_sqlite3.dll) do (
        if exist "%%f" (
            echo 找到: %%f
            set FOUND=1
            echo 正在复制到项目 libs 目录...
            if not exist "libs" mkdir "libs"
            copy "%%f" "libs\e_sqlite3.dll" /Y
            echo ✅ 复制成功！
            goto :copy_to_output
        )
    )
)

REM 搜索 F5BotV2 输出目录
if exist "..\Build\e_sqlite3.dll" (
    echo 在 F5BotV2 Build 目录找到 e_sqlite3.dll
    set FOUND=1
    if not exist "libs" mkdir "libs"
    copy "..\Build\e_sqlite3.dll" "libs\e_sqlite3.dll" /Y
    echo ✅ 从 F5BotV2 复制成功！
    goto :copy_to_output
)

if exist "..\F5BotV2\bin\Debug\e_sqlite3.dll" (
    echo 在 F5BotV2 Debug 目录找到 e_sqlite3.dll
    set FOUND=1
    if not exist "libs" mkdir "libs"
    copy "..\F5BotV2\bin\Debug\e_sqlite3.dll" "libs\e_sqlite3.dll" /Y
    echo ✅ 从 F5BotV2 复制成功！
    goto :copy_to_output
)

if %FOUND%==0 (
    echo.
    echo ❌ 未找到 e_sqlite3.dll
    echo.
    echo 请手动下载 SQLite DLL:
    echo 1. 访问 https://www.sqlite.org/download.html
    echo 2. 下载 sqlite-dll-win-x64-*.zip
    echo 3. 解压并将 sqlite3.dll 重命名为 e_sqlite3.dll
    echo 4. 复制到 BaiShengVx3Plus\libs\ 目录
    pause
    exit /b 1
)

:copy_to_output
echo.
echo [2] 复制到输出目录...
if not exist "bin\Debug\net8.0-windows" (
    echo 输出目录不存在，需要先编译项目
    echo 请运行 fix_and_run.bat
    pause
    exit /b 0
)

if exist "libs\e_sqlite3.dll" (
    copy "libs\e_sqlite3.dll" "bin\Debug\net8.0-windows\e_sqlite3.dll" /Y
    echo ✅ 已复制到输出目录
) else (
    echo ❌ libs\e_sqlite3.dll 不存在
)

echo.
echo ========================================
echo   完成
echo ========================================
pause

