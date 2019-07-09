# Stop on first error
$ErrorActionPreference = "Stop"

# Load helper libraries
.$PSScriptRoot\DeploymentSetup.ps1
.$PSScriptRoot\DeployWebsite.ps1

Setup-Environment -environmentName "integration"

Deploy-Website  -packageDirectory (Resolve-Path "$PSScriptRoot\..\WebPackage") `
                -msDeployUrl "https://172.26.2.34:8172/msdeploy.axd" `
                -msDeployUser $Env:MsDeployUserName `
                -msDeployPassword $Env:MsDeployPassword `
                -logLevel "verbose" `
                -esUrl "http://localhost:9200" `
                -ssoGateway "https://os2sso-test.miracle.dk/.well-known/openid-configuration" `
                -smtpFromMail "ci-kitos@strongminds.dk" `
                -smtpNwHost "127.0.0.1" `
                -resetPwTtl "10.00:00:00" `
                -mailSuffix "(SAW CI)" `
                -baseUrl "https://kitos-integration.strongminds.dk/" `
                -kitosEnvName "Dev" `
                -buildNumber $Env:BUILD_NUMBER `
                -kitosDbConnectionString $Env:KitosDbConnectionStringForIIsApp `
                -hangfireConnectionString $Env:HangfireDbConnectionStringForIIsApp