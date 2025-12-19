@echo off
chcp 65001 >nul
cd /d "E:\gitcode\wx4helper\永利系统\项目说明"

echo 开始重命名文件...
echo.

ren "251219-002.md" "251219-002-本地DLL引用.md"
if %errorlevel%==0 (echo ✓ 251219-002.md) else (echo ✗ 251219-002.md 失败)

ren "251219-003.md" "251219-003-引用缺失解决.md"
if %errorlevel%==0 (echo ✓ 251219-003.md) else (echo ✗ 251219-003.md 失败)

ren "251219-004.md" "251219-004-NetCore路径.md"
if %errorlevel%==0 (echo ✓ 251219-004.md) else (echo ✗ 251219-004.md 失败)

ren "251219-005.md" "251219-005-编译错误修复.md"
if %errorlevel%==0 (echo ✓ 251219-005.md) else (echo ✗ 251219-005.md 失败)

ren "251219-006.md" "251219-006-编译成功说明.md"
if %errorlevel%==0 (echo ✓ 251219-006.md) else (echo ✗ 251219-006.md 失败)

ren "251219-007.md" "251219-007-MVVM方案对比.md"
if %errorlevel%==0 (echo ✓ 251219-007.md) else (echo ✗ 251219-007.md 失败)

ren "251219-008.md" "251219-008-框架深度对比.md"
if %errorlevel%==0 (echo ✓ 251219-008.md) else (echo ✗ 251219-008.md 失败)

ren "251219-009.md" "251219-009-切换操作指南.md"
if %errorlevel%==0 (echo ✓ 251219-009.md) else (echo ✗ 251219-009.md 失败)

ren "251219-010.md" "251219-010-切换完成报告.md"
if %errorlevel%==0 (echo ✓ 251219-010.md) else (echo ✗ 251219-010.md 失败)

ren "251219-011.md" "251219-011-Ribbon设计指南.md"
if %errorlevel%==0 (echo ✓ 251219-011.md) else (echo ✗ 251219-011.md 失败)

echo.
echo 重命名完成！
echo.
pause

