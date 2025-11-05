@echo off
chcp 65001 >nul
cd /d %~dp0
echo 编译 BaiShengVx3Plus 项目...
dotnet build --configuration Debug
echo 编译完成！
pause

