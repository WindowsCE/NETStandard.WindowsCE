@echo off

set SolutionDir=%~dp0
set AssemblyName=NETStandard
set PackageName=%AssemblyName%.WindowsCE
set SourceCodePath=%SolutionDir%\src

echo Copying source files...
CALL %SolutionDir%tools\nuget_source.bat %SolutionDir% %PackageName% %SourceCodePath% || EXIT /B 1

echo Preparing files for packaging...
CALL %SolutionDir%tools\nuget_prepare.bat %SolutionDir% net35-cf %PackageName% %AssemblyName% || EXIT /B 1

echo Copy complete. Starting NuGet packaging...
CALL %SolutionDir%tools\nuget_pack.bat %SolutionDir% %PackageName% %AssemblyName% || EXIT /B 1

echo Packaging complete

EXIT /B %ERRORLEVEL%
