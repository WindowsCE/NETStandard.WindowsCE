setlocal enabledelayedexpansion

REM Capture parameters
set SolutionDir=%~1
set Project=%~2
set Framework=%~3

set dotnet="C:\Program Files\dotnet\dotnet.exe"

REM ============================================================================
REM Check tools availability
REM ============================================================================
if not exist %dotnet% (
	echo Error trying to find DotNet executable
	EXIT /B 1
)

if exist %SolutionDir%%Project%\project.json (
	set Project=%Project%\project.json
)

if not exist %SolutionDir%%Project% (
	echo The file 'project.json' was not found at: %SolutionDir%%Project%
	EXIT /B 1
)

REM ============================================================================
REM Setup variables according to framework
REM ============================================================================

if "%Framework%" == "net35" (
	set TargetDir=%Framework%
	GOTO SetupEnd
)

if "%Framework%" == "net35-client" (
	set TargetDir=%Framework%
	GOTO SetupEnd
)

if "%Framework%" == "net35-cf" (
	set TargetDir=%Framework%
	GOTO SetupEnd
)

if "%Framework%" == "sl4" (
	set TargetDir=%Framework%
	GOTO SetupEnd
)

if "%Framework%" == "net40" (
	set TargetDir=%Framework%
	GOTO SetupEnd
)

if "%Framework%" == "net45" (
	set TargetDir=%Framework%
	GOTO SetupEnd
)

if "%Framework%" == "net451" (
	set TargetDir=%Framework%
	GOTO SetupEnd
)

if "%Framework%" == "net452" (
	set TargetDir=%Framework%
	GOTO SetupEnd
)

if "%Framework%" == "net46" (
	set TargetDir=%Framework%
	GOTO SetupEnd
)

if "%Framework%" == "net461" (
	set TargetDir=%Framework%
	GOTO SetupEnd
)

if "%Framework%" == "net462" (
	set TargetDir=%Framework%
	GOTO SetupEnd
)

if "%Framework%" == "netstandard1.0" (
	set TargetDir=%Framework%
	GOTO SetupEnd
)

if "%Framework%" == "netstandard1.1" (
	set TargetDir=%Framework%
	GOTO SetupEnd
)

if "%Framework%" == "netstandard1.2" (
	set TargetDir=%Framework%
	GOTO SetupEnd
)

if "%Framework%" == "netstandard1.3" (
	set TargetDir=%Framework%
	GOTO SetupEnd
)

if "%Framework%" == "netstandard1.4" (
	set TargetDir=%Framework%
	GOTO SetupEnd
)

if "%Framework%" == "netstandard1.5" (
	set TargetDir=%Framework%
	GOTO SetupEnd
)

if "%Framework%" == "netstandard1.6" (
	set TargetDir=%Framework%
	GOTO SetupEnd
)

if "%Framework%" == "profile2" (
	set Framework=".NETPortable,Version=v4.0,Profile=Profile2"
	set TargetDir=portable-net40+win8+sl4+wp7
	GOTO SetupEnd
)

if "%Framework%" == "profile7" (
	set Framework=".NETPortable,Version=v4.5,Profile=Profile7"
	set TargetDir=portable-net45+win8
	GOTO SetupEnd
)

if "%Framework%" == "profile111" (
	set Framework=".NETPortable,Version=v4.5,Profile=Profile111"
	set TargetDir=portable-net45+win8+wpa81
	GOTO SetupEnd
)

if "%Framework%" == "profile136" (
	set Framework=".NETPortable,Version=v4.0,Profile=Profile136"
	set TargetDir=portable-net40+sl5+win8+wp8
	GOTO SetupEnd
)

if "%Framework%" == "profile44" (
	set Framework=".NETPortable,Version=v4.5.1,Profile=Profile44"
	set TargetDir=portable-net451+win81
	GOTO SetupEnd
)

if "%Framework%" == "profile259" (
	set Framework=".NETPortable,Version=v4.5,Profile=Profile259"
	set TargetDir=portable-net45+win8+wpa81+wp8
	GOTO SetupEnd
)

if "%Framework%" == "profile328" (
	set Framework=".NETPortable,Version=v4.0,Profile=Profile328"
	set TargetDir=portable-net40+sl5+win8+wpa81+wp8
	GOTO SetupEnd
)

echo Unknown target: %Framework%
exit /B 1

:SetupEnd
REM ============================================================================
REM ============================================================================

set Delim=%%3B
set OutputPath=%SolutionDir%Output\%TargetDir%
set ObjOutputPath=%SolutionDir%Output\obj
rmdir /s/q "%OutputPath%" 2> nul
rmdir /s/q "%ObjOutputPath%" 2> nul

REM Call DotNet to build project
%dotnet% build %SolutionDir%%Project% -c Release -f %Framework% -b %ObjOutputPath% -o %OutputPath%
set DOTNETERROR=%ERRORLEVEL%
rmdir /s/q "%ObjOutputPath%"

if %DOTNETERROR% == 0 (
	echo.
) else (
	EXIT /B 1
)

EXIT /B %ERRORLEVEL%
