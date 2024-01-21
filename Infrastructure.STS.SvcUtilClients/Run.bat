@echo off
setlocal

REM ------------------------------------------------------------
REM Get svcutil.exe as: >dotnet tool install --global dotnet-svcutil
REM
REM https://learn.microsoft.com/en-us/dotnet/framework/wcf/servicemodel-metadata-utility-tool-svcutil-exe
REM
REM NB: This is the .NET Core Framework version, but we target net45
REM ------------------------------------------------------------

:PROMPT
SET /P SURE=NB: This command overwrites the old service files. Are you sure (Y/[N])?
IF /I "%SURE%" NEQ "Y" GOTO END

del OrganisationService.cs /Q
del VirksomhedService.cs /Q
del OrganisationSystemService.cs /Q

dotnet-svcutil ^
 schemas\Organisation.wsdl ^
 --noLogo --sync --namespace "*","Kombit.InfrastructureSamples.OrganisationService" --outputDir . --outputFile "OrganisationService.cs"

dotnet-svcutil ^
 schemas\Virksomhed.wsdl ^
 --noLogo --sync --namespace "*","Kombit.InfrastructureSamples.VirksomhedService" --outputDir . --outputFile "VirksomhedService.cs"

dotnet-svcutil ^
 schemas\OrganisationSystem.wsdl ^
 --noLogo --sync --namespace "*","Kombit.InfrastructureSamples.OrganisationSystemService" --outputDir . --outputFile "OrganisationSystemService.cs"


:END
endlocal

