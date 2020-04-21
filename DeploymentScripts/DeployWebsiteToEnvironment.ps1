param(
    [Parameter(Mandatory=$true)][string]$targetEnvironment
    )

# Stop on first error
$ErrorActionPreference = "Stop"

# Load helper libraries
.$PSScriptRoot\DeploymentSetup.ps1
.$PSScriptRoot\DeployWebsite.ps1

Setup-Environment -environmentName $targetEnvironment

Deploy-Website  -packageDirectory (Resolve-Path "$PSScriptRoot\..\WebPackage") `
                -msDeployUrl "$Env:MsDeployUrl" `
                -msDeployUser $Env:MsDeployUserName `
                -msDeployPassword $Env:MsDeployPassword `
                -logLevel "$Env:LogLevel" `
                -esUrl "$Env:EsUrl" `
                -ssoGateway "$Env:SsoGateway" `
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
                -robotsFileName "$Env:robots" `
                -smtpNetworkPort "$Env:SmtpNetworkPort" `
                -smtpNetworkUsername "$Env:SmtpUserName" `
                -smtpNetworkPassword "$Env:SmtpPassword"
