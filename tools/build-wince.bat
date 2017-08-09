setlocal enabledelayedexpansion

REM Capture parameters
set SolutionDir=%~1
set SolutionName=%~2
set Project=%~3
set Configuration=%~4

set net35path="C:\Windows\Microsoft.NET\Framework\v3.5"
set msbuild35="%net35path%\MSBuild.exe"
set targetscf="%net35path%\Microsoft.CompactFramework.CSharp.targets"

set msbuild="%ProgramFiles(x86)%\MSBuild\14.0\bin\msbuild"
if not exist %msbuild% (
    set msbuild="%ProgramFiles%\MSBuild\14.0\bin\msbuild"
)
if not exist %msbuild% (
    echo Falling back to MSBuild 12.0
    set msbuild="%ProgramFiles(x86)%\MSBuild\12.0\bin\msbuild"
)
if not exist %msbuild% (
    set msbuild=%ProgramFiles%\MSBuild\12.0\bin\msbuild"
)
if not exist %msbuild% (
    echo Falling back to .NET Framework 3.5 MSBuild
    set msbuild=%msbuild35%
)

echo Compiling %Project% for .NETFramework,Version=v3.5,Profile=CompactFramework
echo.

REM ============================================================================
REM Check tools availability
REM ============================================================================
if not exist %msbuild% (
    echo Error trying to find MSBuild executable
    EXIT /B 1
)

if not exist %targetscf% (
    echo Error trying to find Compact Framework targets
    echo.
    echo Install '.NET Compact Framework Redistributable'
    echo and 'Power Toys for .NET Compact Framework 3.5'
    exit /B 1
)

REM ============================================================================
REM Setup variables
REM ============================================================================

set WinCEDir=%SolutionDir%WindowsCE
set SolutionFile=%WinCEDir%\%SolutionName%.sln
set TargetDir=net35-cf
REM Normalize project name for MSBuild
set Project=%Project:.=_%

if "%Configuration%" == "" (
    set Configuration=Release
)

if not exist %SolutionFile% (
    echo Missing solution for Compact Framework target
    exit /B 1
)

REM ============================================================================
REM ============================================================================

set OutputPath=%SolutionDir%Output\%TargetDir%
set ObjOutputPath=%SolutionDir%Output\obj
rmdir /s/q "%OutputPath%" 2> nul
rmdir /s/q "%ObjOutputPath%" 2> nul

REM Restore packages from packages.config
if exist %WinCEDir%\packages.config (
    set PackagesDir=%WinCEDir%\packages
    set NuGetCommand=%SolutionDir%tools\NuGet.exe
)
if exist %WinCEDir%\packages.config (
    echo Restoring NuGet packages
    rmdir /s/q %PackagesDir% > nul 2>&1
    mkdir %PackagesDir% > nul
    %NuGetCommand% install -ExcludeVersion -outputDirectory %PackagesDir% %SolutionDir%WindowsCE\packages.config > nul
    echo.
)

REM Call MSBuild to build library
%msbuild% %SolutionFile% /target:%Project% /verbosity:minimal /property:Configuration=%Configuration%;OutputPath=%OutputPath%\;BaseIntermediateOutputPath=%ObjOutputPath%\ > output_rel.log
rmdir /s/q "%ObjOutputPath%"

if %ERRORLEVEL% == 0 (
    echo Compilation succeeded
) else (
    echo Compilation failed
    type output_rel.log
    EXIT /B 1
)

echo.
echo.
echo.
EXIT /B %ERRORLEVEL%

set Configuration=
set SolutionDir=
set SolutionFile=
set Project=
