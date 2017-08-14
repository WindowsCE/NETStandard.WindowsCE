REM Capture parameters
set ScriptDir=%~dp0
set SolutionDir=%~1
set PackageName=%~2
set AssemblyName=%~3
set AssemblyVersion=%~4

set NuGetCommand=%ScriptDir%NuGet.exe
set VersionInfoCommand=%ScriptDir%VersionInfo.vbs
set ProdVersionInfoCommand=%ScriptDir%ProductVersionInfo.vbs
set output=%SolutionDir%Output
set nuget_nuspec=%SolutionDir%%PackageName%.nuspec
set nuget_folder=%SolutionDir%.nuget\%PackageName%
set AssemblyDir=%output%\netstandard1.3
set AssemblyFile=%AssemblyName%.dll
if not exist %AssemblyDir%\%AssemblyFile% (
    set AssemblyDir=%output%\net46
)
if not exist %AssemblyDir%\%AssemblyFile% (
    set AssemblyDir=%output%\net45
)
if not exist %AssemblyDir%\%AssemblyFile% (
    set AssemblyDir=%output%\net40
)
if not exist %AssemblyDir%\%AssemblyFile% (
    set AssemblyDir=%output%\net35
)
if not exist %AssemblyDir%\%AssemblyFile% (
    set AssemblyDir=%output%\net35-cf
)
if not exist %AssemblyDir%\%AssemblyFile% (
    echo The assembly file could not be found at: %AssemblyDir%
    EXIT /B 1
)

if not exist %nuget_nuspec% (
    echo The nuspec file could not be found at: %nuget_nuspec%
    EXIT /B 1
)

if "%AssemblyVersion%" == "" (
    REM for /f %%i in ('cscript //nologo %VersionInfoCommand% %AssemblyDir%\%AssemblyFile%') do set AssemblyVersion=%%i
    for /f %%i in ('cscript //nologo %ProdVersionInfoCommand% %AssemblyDir% %AssemblyFile%') do set AssemblyVersion=%%i
)

mkdir "%nuget_folder%" > nul 2>&1
del "%nuget_folder%\%PackageName%.nuspec" > nul 2>&1
copy "%nuget_nuspec%" "%nuget_folder%" > nul

%NuGetCommand% pack %nuget_folder%\%PackageName%.nuspec -Version %AssemblyVersion% -Symbols -OutputDirectory %output%
REM %NuGetCommand% pack %nuget_folder%\%PackageName%.nuspec -Version %AssemblyVersion% -OutputDirectory %output%

rmdir /s/q "%SolutionDir%.nuget" > nul 2>&1
