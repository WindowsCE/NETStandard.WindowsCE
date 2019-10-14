@echo off

set SolutionDir=%~dp0
set PackageVersion=%~1
set AssemblyName=NETStandard.WindowsCE
set PackageName=%AssemblyName%
set SourceCodePath=%SolutionDir%\src

mkdir %SolutionDir%Output
xcopy %SourceCodePath%\NETStandard.WindowsCE\bin\Release %SolutionDir%Output /sy > nul

echo Copying source files...
CALL %SolutionDir%tools\nuget_source.bat %SolutionDir% %PackageName% %SourceCodePath% || EXIT /B 1

echo Preparing files for packaging...
CALL %SolutionDir%tools\nuget_prepare.bat %SolutionDir% net35-cf %PackageName% %AssemblyName% || EXIT /B 1
CALL %SolutionDir%tools\nuget_prepare_empty.bat %SolutionDir% net35-client %PackageName% %AssemblyName% || EXIT /B 1
CALL %SolutionDir%tools\nuget_prepare_empty.bat %SolutionDir% net35 %PackageName% %AssemblyName% || EXIT /B 1
CALL %SolutionDir%tools\nuget_prepare_empty.bat %SolutionDir% net40 %PackageName% %AssemblyName% || EXIT /B 1
CALL %SolutionDir%tools\nuget_prepare_empty.bat %SolutionDir% net45 %PackageName% %AssemblyName% || EXIT /B 1
CALL %SolutionDir%tools\nuget_prepare_empty.bat %SolutionDir% net461 %PackageName% %AssemblyName% || EXIT /B 1
CALL %SolutionDir%tools\nuget_prepare_empty.bat %SolutionDir% netstandard1.0 %PackageName% %AssemblyName% || EXIT /B 1
CALL %SolutionDir%tools\nuget_prepare_empty.bat %SolutionDir% netstandard2.0 %PackageName% %AssemblyName% || EXIT /B 1

echo Preparing build files for packaging...
CALL %SolutionDir%tools\nuget_prepare_build.bat %SolutionDir% net35-cf %PackageName% %AssemblyName% || EXIT /B 1

echo Copy complete. Starting NuGet packaging...
CALL %SolutionDir%tools\nuget_pack.bat %SolutionDir% %PackageName% %AssemblyName% %PackageVersion% || EXIT /B 1

echo Packaging complete

EXIT /B %ERRORLEVEL%
