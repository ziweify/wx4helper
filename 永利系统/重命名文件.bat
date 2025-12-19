@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion

echo Starting file rename...
cd /d "%~dp0项目说明"

set "success=0"
set "fail=0"

ren "251219-002.md" "251219-002-本地DLL引用.md" 2>nul && (set /a success+=1 && echo Success: 251219-002.md) || (set /a fail+=1 && echo Failed: 251219-002.md)
ren "251219-003.md" "251219-003-引用缺失解决.md" 2>nul && (set /a success+=1 && echo Success: 251219-003.md) || (set /a fail+=1 && echo Failed: 251219-003.md)
ren "251219-004.md" "251219-004-NetCore路径.md" 2>nul && (set /a success+=1 && echo Success: 251219-004.md) || (set /a fail+=1 && echo Failed: 251219-004.md)
ren "251219-005.md" "251219-005-编译错误修复.md" 2>nul && (set /a success+=1 && echo Success: 251219-005.md) || (set /a fail+=1 && echo Failed: 251219-005.md)
ren "251219-006.md" "251219-006-编译成功说明.md" 2>nul && (set /a success+=1 && echo Success: 251219-006.md) || (set /a fail+=1 && echo Failed: 251219-006.md)
ren "251219-007.md" "251219-007-MVVM方案对比.md" 2>nul && (set /a success+=1 && echo Success: 251219-007.md) || (set /a fail+=1 && echo Failed: 251219-007.md)
ren "251219-008.md" "251219-008-框架深度对比.md" 2>nul && (set /a success+=1 && echo Success: 251219-008.md) || (set /a fail+=1 && echo Failed: 251219-008.md)
ren "251219-009.md" "251219-009-切换操作指南.md" 2>nul && (set /a success+=1 && echo Success: 251219-009.md) || (set /a fail+=1 && echo Failed: 251219-009.md)
ren "251219-010.md" "251219-010-切换完成报告.md" 2>nul && (set /a success+=1 && echo Success: 251219-010.md) || (set /a fail+=1 && echo Failed: 251219-010.md)
ren "251219-011.md" "251219-011-Ribbon设计指南.md" 2>nul && (set /a success+=1 && echo Success: 251219-011.md) || (set /a fail+=1 && echo Failed: 251219-011.md)

echo.
echo Rename completed!
echo Success: !success! files
if !fail! gtr 0 echo Failed: !fail! files
echo.
pause

