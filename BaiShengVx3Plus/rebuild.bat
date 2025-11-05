@echo off
chcp 65001 >nul
echo 正在清理...
rd /s /q bin 2>nul
rd /s /q obj 2>nul

echo 正在编译...
dotnet build --configuration Debug --no-incremental

echo.
echo 编译完成！
pause

