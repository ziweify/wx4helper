@echo off
chcp 65001 >nul
cd /d "%~dp0"

echo ========================================
echo  清理空文件夹
echo ========================================
echo.

echo [1/3] 清理 Services\Member...
if exist "Services\Member" (
    rmdir "Services\Member" 2>nul
    if %errorlevel% equ 0 (
        echo ✓ 已删除 Services\Member
    ) else (
        echo ⚠ Services\Member 不是空文件夹或不存在
    )
) else (
    echo ✓ Services\Member 不存在
)

echo.
echo [2/3] 清理 Services\Order...
if exist "Services\Order" (
    rmdir "Services\Order" 2>nul
    if %errorlevel% equ 0 (
        echo ✓ 已删除 Services\Order
    ) else (
        echo ⚠ Services\Order 不是空文件夹或不存在
    )
) else (
    echo ✓ Services\Order 不存在
)

echo.
echo [3/3] 清理 Services\Database...
if exist "Services\Database" (
    rmdir "Services\Database" 2>nul
    if %errorlevel% equ 0 (
        echo ✓ 已删除 Services\Database
    ) else (
        echo ⚠ Services\Database 不是空文件夹或不存在
    )
) else (
    echo ✓ Services\Database 不存在
)

echo.
echo ========================================
echo  清理完成
echo ========================================
pause

