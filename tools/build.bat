setlocal enabledelayedexpansion
set ScriptDir=%~dp0

REM Capture parameters
set SolutionDir=%~1
set SolutionName=%~2
set Project=%~3
set TargetFX=%~4
set Configuration=%~5
set TargetDir=%~6

if "%Configuration%" == "" (
    set Configuration=Release
)
if "%TargetDir%" == "" (
    set TargetDir=%TargetFX%
)

echo.
echo --------------------------------------------------------------------------------
echo  Build %Project% for %TargetFX% platform
echo --------------------------------------------------------------------------------
echo.

set msbuild="C:\Program Files (x86)\Microsoft Visual Studio\2017\BuildTools\MSBuild\15.0\Bin\MSBuild.exe"
if not exist %msbuild% (
    set msbuild="C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe"
)
if not exist %msbuild% (
	echo Error trying to find MSBuild executable
	EXIT /B 1
)

set Delim=%%3B
set SolutionFile=%SolutionName%.sln
set OutputPath=%SolutionDir%Output\%TargetDir%
set Project=%Project:.=_%

if not "%TargetFX%" == "" (
    set TargetFXConfig=;TargetFramework=%TargetFX%
)

dotnet restore %SolutionDir%%SolutionFile%
%msbuild% %SolutionDir%%SolutionFile% /t:"%Project%:Rebuild" /verbosity:minimal /p:Configuration=%Configuration%;OutputPath=%OutputPath%%TargetFXConfig%

echo.
echo --------------------------------------------------------------------------------
echo.

EXIT /B %ERRORLEVEL%
