# Stop on first error
$ErrorActionPreference = "Stop"

# Load helper libraries
.$PSScriptRoot\DeploymentSetup.ps1
.$PSScriptRoot\DeployWebsite.ps1

Setup-Environment -environmentName "production"

Deploy-Website  -packageDirectory (Resolve-Path "$PSScriptRoot\..\WebPackage") `
                -msDeployUrl "https://10.7.23.10:8172/msdeploy.axd" `
                -msDeployUser $Env:MsDeployUserName `
                -msDeployPassword $Env:MsDeployPassword `
                -logLevel "verbose" `
                -esUrl "http://10.2.23.21:9200/" `
                -ssoGateway "https://os2sso.miracle.dk/.well-known/openid-configuration" `
                -smtpFromMail "noreply@kitos.dk" `
                -smtpNwHost "10.7.99.81" `
                -resetPwTtl "10.00:00:00" `
                -mailSuffix "" `
                -baseUrl "https://$Env:KitosHostName/" `
                -kitosEnvName "Prod" `
                -buildNumber $Env:BUILD_NUMBER `
                -kitosDbConnectionString $Env:KitosDbConnectionStringForIIsApp `
                -hangfireConnectionString $Env:HangfireDbConnectionStringForIIsApp