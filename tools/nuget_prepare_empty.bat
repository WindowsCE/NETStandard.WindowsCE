REM Capture parameters
set SolutionDir=%~1
set platform=%~2
set package=%~3
set assembly=%~4

set nuget_folder=%SolutionDir%.nuget\%package%
set output=%SolutionDir%Output

rmdir /s/q "%nuget_folder%\lib\%platform%" > nul 2>&1
mkdir "%nuget_folder%\lib\%platform%" || EXIT /B 1

copy NUL "%nuget_folder%\lib\%platform%\_._" > nul

set platform=
set package=
set assembly=
EXIT /B %ERRORLEVEL%