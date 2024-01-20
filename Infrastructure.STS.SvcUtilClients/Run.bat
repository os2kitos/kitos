@echo off
setlocal

REM ------------------------------------------------------------
REM Get svcutil.exe via Visual Studio Installer/Individual components/SDK's, libraries and frameworks/Windows 11 SDK
REM
REM https://learn.microsoft.com/en-us/dotnet/framework/wcf/servicemodel-metadata-utility-tool-svcutil-exe
REM
REM NB: This is the .NET Framework version -- there is an dotnet-svcutil.exe also
REM ------------------------------------------------------------

:PROMPT
SET /P SURE=NB: This command overwrites the old service files. Are you sure (Y/[N])?
IF /I "%SURE%" NEQ "Y" GOTO END

del OrganisationService.cs
del VirksomhedService.cs

dotnet-svcutil ^
 schemas\Organisation.wsdl ^
 --noLogo --sync --namespace "*","Kombit.InfrastructureSamples.OrganisationService" --outputDir . --outputFile "OrganisationService.cs"

dotnet-svcutil ^
 schemas\Virksomhed.wsdl ^
 --noLogo --sync --namespace "*","Kombit.InfrastructureSamples.VirksomhedService" --outputDir . --outputFile "VirksomhedService.cs"

:END
endlocal

