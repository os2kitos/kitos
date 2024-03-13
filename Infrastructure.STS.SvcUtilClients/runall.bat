@echo off

@echo NB: Run this command from a (Visual Studio) Developer Prompt prompt for access to the SvcUtil.exe utility

setlocal
:PROMPT
SET /P CONFIRM=This command deletes the existing service definitions. Are you sure (Y/[N])?
IF /I "%CONFIRM%" NEQ "Y" GOTO END

cd Organisation
call run.bat
cd ..\OrganisationSystem
call run.bat
cd ..\Virksomhed
call run.bat
cd ..\Bruger
call run.bat
cd ..\Adresse
call run.bat
cd ..\Person
call run.bat
cd ..

:END
endlocal

