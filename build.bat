@echo off

set SolutionDir=%~dp0
set MSBuild=%SolutionDir%tools\MSBuild.cmd
set MainProject=%SolutionDir%src\NETStandard.WindowsCE\NETStandard.WindowsCE.csproj

call %MSBuild% /t:Restore %MainProject% || EXIT /B 1
call %MSBuild% /p:Configuration=Debug %MainProject% || EXIT /B 1
call %MSBuild% /p:Configuration=Release %MainProject% || EXIT /B 1
