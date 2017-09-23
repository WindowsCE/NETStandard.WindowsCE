@echo off

REM Capture parameters
set SolutionDir=%~dp0

REM Restore packages from packages.config
if exist %SolutionDir%packages.config (
    set PackagesDir=%SolutionDir%packages
    set NuGetCommand=%SolutionDir%..\tools\NuGet.exe
) else (
	echo Package configuration file was not found
)

if exist %SolutionDir%packages.config (
    echo Restoring NuGet packages
    rmdir /s/q %PackagesDir% > nul 2>&1
    mkdir %PackagesDir% > nul
    %NuGetCommand% install -ExcludeVersion -outputDirectory %PackagesDir% %SolutionDir%packages.config > nul
    echo.
)
