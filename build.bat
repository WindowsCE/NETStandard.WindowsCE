@echo off

set SolutionDir=%~dp0
set SolutionName=netstandard-net35cf
set Project=src\NETStandard.WindowsCE

REM Cleanup output directory
rmdir /s/q "%SolutionDir%Output" 2> nul
mkdir "%SolutionDir%Output"

CALL %SolutionDir%tools\build.bat %SolutionDir% %SolutionName% %Project% net35-cf || EXIT /B 1
CALL %SolutionDir%tools\build.bat %SolutionDir% %SolutionName% %Project% net35 || EXIT /B 1

EXIT /B %ERRORLEVEL%
