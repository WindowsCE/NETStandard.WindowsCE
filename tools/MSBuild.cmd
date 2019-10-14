@echo off

set ScriptDir=%~dp0

for /f "usebackq tokens=*" %%i in (`%ScriptDir%vswhere -latest -products * -requires Microsoft.Component.MSBuild -property installationPath`) do (
  set InstallDir=%%i
)

if exist "%InstallDir%\MSBuild\Current\Bin\MSBuild.exe" (
  "%InstallDir%\MSBuild\Current\Bin\MSBuild.exe" %*
)

if exist "%InstallDir%\MSBuild\15.0\Bin\MSBuild.exe" (
  "%InstallDir%\MSBuild\15.0\Bin\MSBuild.exe" %*
)