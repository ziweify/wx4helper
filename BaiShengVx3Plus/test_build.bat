@echo off
chcp 65001 >nul
cd /d %~dp0
echo 正在编译项目...
dotnet build --no-incremental
echo.
echo 编译完成！

