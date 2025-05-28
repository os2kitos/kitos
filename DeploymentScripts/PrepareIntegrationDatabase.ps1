param(
    [Parameter(Mandatory=$true)][string]$targetEnvironment
    )
#-------------------------------------------------------------
# Stop on first error
#-------------------------------------------------------------
$ErrorActionPreference = "Stop"

#-------------------------------------------------------------
# Load helper libraries
#-------------------------------------------------------------
.$PSScriptRoot\DeploymentSetup.ps1
.$PSScriptRoot\DbMigrations.ps1

switch( $targetEnvironment ) 
    {
        "integration" 
        {
            #All good
            break;
        }
        "dev" 
        {
            #All good
            break;
        }
        default { Throw "Error: Unsupported environment provided: $targetEnvironment" }
    }

Setup-Environment -environmentName $targetEnvironment

.$PSScriptRoot\PrepareCleanDeveloperDatabase.ps1 `
                -testToolsExePath "$Env:TestToolsPath" `
                -migrationsFolderPath "$Env:MigrationsFolder" `
                -kitosDbConnectionString "$Env:KitosDbConnectionStringForTeamCity" `
                -hangfireDbConnectionString "$Env:HangfireDbConnectionStringForTeamCity" `
                -globalAdminUserName "$Env:TestUserGlobalAdmin" `
                -globalAdminPw "$Env:TestUserGlobalAdminPw" `
                -localAdminUserName "$Env:TestUserLocalAdmin" `
                -localAdminPw "$Env:TestUserLocalAdminPw" `
                -normalUserUserName "$Env:TestUserNormalUser" `
                -normalUserPw "$Env:TestUserNormalUserPw" `
                -apiUserUserName "$Env:TestUserApiUser" `
                -apiUserPw "$Env:TestUserApiUserPw" `
                -apiGlobalAdminUserName "$Env:TestUserApiGlobalAdmin" `
                -apiGlobalAdminPw "$Env:TestUserApiGlobalAdminPw" `
                -systemIntegratorEmail "$Env:TestUserSystemIntegrator" `
                -systemIntegratorPw "$Env:TestUserSystemIntegratorPw"
                