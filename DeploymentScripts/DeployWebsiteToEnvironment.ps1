param(
    [Parameter(Mandatory=$true)][string]$targetEnvironment
    )

# Stop on first error
$ErrorActionPreference = "Stop"

# Load helper libraries
.$PSScriptRoot\DeploymentSetup.ps1
.$PSScriptRoot\PreparePackage.ps1
.$PSScriptRoot\DeployWebsite.ps1

Setup-Environment -environmentName $targetEnvironment

Prepare-Package -environmentName $targetEnvironment -pathToArchive (Resolve-Path "$PSScriptRoot\..\WebPackage\Presentation.Web.zip")

Deploy-Website  -packageDirectory (Resolve-Path "$PSScriptRoot\..\WebPackage") `
                -msDeployUrl "$Env:MsDeployUrl" `
                -msDeployUser $Env:MsDeployUserName `
                -msDeployPassword $Env:MsDeployPassword `
                -logLevel "$Env:LogLevel" `
                -esUrl "$Env:EsUrl" `
                -securityKeyString "$Env:SecurityKeyString" `
                -smtpFromMail "$Env:SmtpFromMail" `
                -smtpNwHost "$Env:SmtpNetworkHost" `
                -resetPwTtl "$Env:ResetPasswordTtl" `
                -mailSuffix "$Env:MailSuffix" `
                -baseUrl "https://$Env:KitosHostName/" `
                -kitosEnvName "$Env:KitosEnvName" `
                -buildNumber $Env:BUILD_NUMBER `
                -kitosDbConnectionString $Env:KitosDbConnectionStringForIIsApp `
                -hangfireConnectionString $Env:HangfireDbConnectionStringForIIsApp `
                -defaultUserPassword $Env:DefaultUserPassword `
                -useDefaultUserPassword $Env:UseDefaultUserPassword `
                -ssoServiceProviderServer "$Env:SsoServiceProviderServer" `
                -ssoIDPEndPoints "$Env:SsoIDPEndPoints" `
                -ssoServiceProviderId "$Env:SsoServiceProviderId" `
                -ssoCertificateThumbPrint "$Env:SsoCertificateThumbPrint" `
                -stsOrganisationEndpointHost "$Env:StsOrganisationEndpointHost" `
                -stsIssuer "$Env:StsIssuer" `
                -stsCertificateEndpoint "$Env:StsCertificateEndpoint" `
                -serviceCertificateAliasOrg "$Env:ServiceCertificateAliasOrg" `
                -stsCertificateAlias "$Env:StsCertificateAlias" `
                -stsCertificateThumbprint "$Env:StsCertificateThumbprint" `
                -orgService6EntityId "$Env:OrgService6EntityId" `
                -stsAdressePort "$Env:StsAdressePort" `
                -stsBrugerPort "$Env:StsBrugerPort" `
                -stsPersonPort "$Env:StsPersonPort" `
                -stsVirksomhedPort "$Env:StsVirksomhedPort" `
                -stsOrganisationPort "$Env:StsOrganisationPort" `
                -stsOrganisationSystemPort "$Env:StsOrganisationSystemPort" `
                -stsOrganisationCertificateThumbprint "$Env:StsOrganisationCertificateThumbprint" `
                -robotsFileName "$Env:robots" `
                -smtpNetworkPort "$Env:SmtpNetworkPort" `
                -smtpNetworkUsername "$Env:SmtpUserName" `
                -smtpNetworkPassword "$Env:SmtpPassword" `
                -pubSubBaseUrl "$Env:PubSubBaseUrl"
