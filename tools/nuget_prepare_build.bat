REM Capture parameters
set SolutionDir=%~1
set platform=%~2
set package=%~3
set assembly=%~4

set nuget_folder=%SolutionDir%.nuget\%package%
set output=%SolutionDir%Output

if not "%platform%" == "" (
    set platform=\%platform%
)

rmdir /s/q "%nuget_folder%\build%platform%" > nul 2>&1
mkdir "%nuget_folder%\build%platform%" || EXIT /B 1

copy %SolutionDir%%package%.props "%nuget_folder%\build%platform%\*.*" > NUL

set platform=
set package=
set assembly=
EXIT /B %ERRORLEVEL%