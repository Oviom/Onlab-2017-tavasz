@echo off

set ROOT=..
set FAILED_BUILDS_LOG=failed-builds.log

rem ----------------------------------------------------------------------------
rem Run compilation
rem ----------------------------------------------------------------------------
call rebuild.cmd ..\SnappyDL.sln x86
call rebuild.cmd ..\SnappyDL.sln x64
call rebuild.cmd ..\Snappy.sln x86
call rebuild.cmd ..\Snappy.sln x64

rem ----------------------------------------------------------------------------
rem Copy files to target folders
rem ----------------------------------------------------------------------------
xcopy /y /d ..\bin\Win32\Release\*.dll any\
xcopy /y /d ..\bin\x64\Release\*.dll any\
xcopy /y /d ..\SnappyPI\bin\Release\*.dll any\
exit /b

:end

