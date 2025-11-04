@echo off
setlocal

echo ========================================
echo Building WeixinX Project (Release x64)
echo ========================================
echo.

:: Try to find MSBuild using vswhere
set "VSWHERE=%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe"

if not exist "%VSWHERE%" (
    echo ERROR: vswhere.exe not found
    echo Please install Visual Studio 2019 or later
    pause
    exit /b 1
)

for /f "usebackq tokens=*" %%i in (`"%VSWHERE%" -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe`) do (
    set "MSBUILD=%%i"
)

if not exist "%MSBUILD%" (
    echo ERROR: MSBuild.exe not found
    pause
    exit /b 1
)

echo Found MSBuild: %MSBUILD%
echo.

:: Change to script directory
cd /d "%~dp0"

:: Build WeixinX
"%MSBUILD%" WeixinX.sln /p:Configuration=Release /p:Platform=x64 /t:WeixinX /m

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ========================================
    echo Build Successful!
    echo ========================================
    echo Output: WeixinX\x64\Release\WeixinX.dll
) else (
    echo.
    echo ========================================
    echo Build Failed!
    echo ========================================
    pause
    exit /b %ERRORLEVEL%
)

endlocal

