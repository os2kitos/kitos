@ECHO OFF

@echo off
setlocal
:PROMPT
SET /P CONFIRM=This command deletes the existing service definitions. Are you sure (Y/[N])?
IF /I "%CONFIRM%" NEQ "Y" GOTO END

cd Organisation
call run.bat
cd ..\Virksomhed
call run.bat
cd ..

:END
endlocal

