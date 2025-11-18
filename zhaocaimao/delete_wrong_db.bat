@echo off
chcp 65001 >nul
echo.
echo ====================================
echo 删除错误的群数据库文件
echo ====================================
echo.

set "target=D:\gitcode\wx4helper\BaiShengVx3Plus\bin\Debug\net8.0-windows\Data\business_27206515609@chatroom.db"

if exist "%target%" (
    echo 找到文件: %target%
    del /F /Q "%target%"
    if %errorlevel% equ 0 (
        echo ✓ 文件已删除
    ) else (
        echo ✗ 删除失败
    )
) else (
    echo ✓ 文件不存在（已经删除或从未创建）
)

echo.
echo 当前 Data 文件夹内容:
echo ------------------------------------
dir /B "D:\gitcode\wx4helper\BaiShengVx3Plus\bin\Debug\net8.0-windows\Data"
echo.
pause

