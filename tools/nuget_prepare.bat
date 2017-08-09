REM Capture parameters
set SolutionDir=%~1
set platform=%~2
set package=%~3
set assembly=%~4

set nuget_folder=%SolutionDir%.nuget\%package%
set output=%SolutionDir%Output

if not exist "%output%\%platform%\%assembly%.dll" (
	echo Could not find assemblies for %platform% platform
	EXIT /B %ERRORLEVEL%
)

echo Copying assemblies for %platform%...
rmdir /s/q "%nuget_folder%\lib\%platform%" > nul 2>&1
mkdir "%nuget_folder%\lib\%platform%" || EXIT /B 1

set nodoc=0
copy "%output%\%platform%\%assembly%.dll" "%nuget_folder%\lib\%platform%\*.*" > nul || EXIT /B 1
copy "%output%\%platform%\%assembly%.pdb" "%nuget_folder%\lib\%platform%\*.*" > nul || EXIT /B 1
copy "%output%\%platform%\%assembly%.xml" "%nuget_folder%\lib\%platform%\*.*" > nul || set nodoc=1
if %nodoc% == 1 (
    echo [WARNING] No documentation was found for %assembly%
    set ERRORLEVEL=0
)
if exist "%output%\%platform%\%assembly%.mdb" (
	copy "%output%\%platform%\%assembly%.mdb" "%nuget_folder%\lib\%platform%\*.*" > nul || EXIT /B 1
)

CALL :CopyResource pt || EXIT /B 1
CALL :CopyResource pt-BR || EXIT /B 1
CALL :CopyResource es || EXIT /B 1
CALL :CopyResource es-ES || EXIT /B 1

set platform=
set assembly=
EXIT /B %ERRORLEVEL%

:CopyResource
set lang=%~1

if exist "%output%\%platform%\%lang%\%assembly%.dll" (
    mkdir "%nuget_folder%\lib\%platform%\%lang%" > nul || EXIT /B 1
    copy "%output%\%platform%\%lang%\%assembly%.dll" "%nuget_folder%\lib\%platform%\%lang%\" > nul || EXIT /B 1
)

set lang=
EXIT /B 0