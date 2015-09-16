REM This file is used to package projects and publish them to MyGet
REM This file should sit in the same directory as the csproj file you want to package 
REM nuget.exe should be in a directory called ".nuget" one directory up
REM You can get nuget.exe to install by turning on nuget package restore

set config=%1
set PackageVersion=%2
if "%config%" == "" (
   set config=Release
)

set version=
if not "%PackageVersion%" == "" (
   set version=-Version %PackageVersion%
)

set nuget=nuget.exe

REM Make sure there is only one file in the package directory because we're going to push everything to myget
del /F /Q bin\nuget_build 
mkdir bin\nuget_build

REM ** Pack the Project **
REM Changing package title/id/description can be done by modifying [AssemblyTitle] and [AssemblyDescription]
REM in the AssemblyInfo.cs file in the project (see: http://stackoverflow.com/questions/22208542/nuget-pack-someproject-csproj-wont-let-me-change-title-or-description/22208543#22208543)

cmd /c %nuget% pack "Svg.csproj" -IncludeReferencedProjects -o bin\nuget_build -p Configuration=%config% %version% 

REM ** Push the file to myget ** 
REM There should only be a single file in the -Source https://www.nuget.org/
for /f %%l in ('dir /b /s bin\nuget_build\*.nupkg') do (
    cmd /c %nuget% push %%l 
)