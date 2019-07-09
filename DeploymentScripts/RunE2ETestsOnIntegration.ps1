# Stop on first error
$ErrorActionPreference = "Stop"

# Load helper libraries
.$PSScriptRoot\DeploymentSetup.ps1

Setup-Environment -environmentName "integration"

.$PSScriptRoot\RunE2ETests.ps1 `
                            -url "https://kitos-integration.strongminds.dk" `
                            -usrname "[kitos-integration-globaladmin@strongminds.dk, kitos-integration-localadmin@strongminds.dk, kitos-integration-normal-user@strongminds.dk]" `
                            -pwd "[$Env:TestUserGlobalAdminPw, $Env:TestUserLocalAdminPw, $Env:TestUserNormalUserPw]"