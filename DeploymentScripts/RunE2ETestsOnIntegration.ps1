# Stop on first error
$ErrorActionPreference = "Stop"

# Load helper libraries
.$PSScriptRoot\DeploymentSetup.ps1

Setup-Environment -environmentName "integration"

.$PSScriptRoot\RunE2ETests.ps1 `
                            -url "https://$Env:KitosHostName" `
                            -usrname "[$Env:TestUserGlobalAdmin, $Env:TestUserLocalAdmin, $Env:TestUserNormalUser, $Env:TestUserApiUser]" `
                            -pwd "[$Env:TestUserGlobalAdminPw, $Env:TestUserLocalAdminPw, $Env:TestUserNormalUserPw, $Env:TestUserApiUserPw]" `
							-testType "headless"