# Load helper
.$PSScriptRoot\AwsApi.ps1

Function Load-Environment-Secrets-From-Aws([String] $envName, [bool] $loadTcHangfireConnectionString = $true, [bool] $loadTestUsers = $true) {
    Write-Host "Loading environment configuration from SSM"
    
    $Env:KitosHostName = Get-SSM-Parameter -environmentName "$envName" -parameterName "HostName"
    $Env:MsDeployUserName = Get-SSM-Parameter -environmentName "$envName" -parameterName "MsDeployUserName"
    $Env:MsDeployPassword = Get-SSM-Parameter -environmentName "$envName" -parameterName "MsDeployPassword"
    $Env:KitosDbConnectionStringForIIsApp = Get-SSM-Parameter -environmentName "$envName" -parameterName "KitosDbConnectionStringForIIsApp"
    $Env:HangfireDbConnectionStringForIIsApp = Get-SSM-Parameter -environmentName "$envName" -parameterName "HangfireDbConnectionStringForIIsApp"
    $Env:KitosDbConnectionStringForTeamCity = Get-SSM-Parameter -environmentName "$envName" -parameterName "KitosDbConnectionStringForTeamCity"
    
    if($loadTcHangfireConnectionString -eq $true) {
        $Env:HangfireDbConnectionStringForTeamCity = Get-SSM-Parameter -environmentName "$envName" -parameterName "HangfireDbConnectionStringForTeamCity"
    }
    
    if($loadTestUsers -eq $true) {
        $Env:TestUserGlobalAdmin = Get-SSM-Parameter -environmentName "$envName" -parameterName "TestUserGlobalAdmin"
        $Env:TestUserGlobalAdminPw = Get-SSM-Parameter -environmentName "$envName" -parameterName "TestUserGlobalAdminPw"

        $Env:TestUserLocalAdmin = Get-SSM-Parameter -environmentName "$envName" -parameterName "TestUserLocalAdmin"
        $Env:TestUserLocalAdminPw = Get-SSM-Parameter -environmentName "$envName" -parameterName "TestUserLocalAdminPw"

        $Env:TestUserNormalUser = Get-SSM-Parameter -environmentName "$envName" -parameterName "TestUserNormalUser"
        $Env:TestUserNormalUserPw = Get-SSM-Parameter -environmentName "$envName" -parameterName "TestUserNormalUserPw"
    }
    
    
    Write-Host "Finished loading environment configuration from SSM"
}

Function Setup-Environment([String] $environmentName) {
    Write-Host "Configuring Deployment Environment $environmentName"
    
    if (-Not (Test-Path 'env:AwsAccessKeyId')) { 
    	throw "Error: Remember to set the AwsAccessKeyId input before starting the build"
    } 
    if (-Not (Test-Path 'env:AwsSecretAccessKey')) { 
    	throw "Error: Remember to set the AwsSecretAccessKey input before starting the build"
    }
    
    switch( $environmentName ) 
    {
        "integration" 
        {
            $loadTcHangfireConnectionString = $true
            $loadTestUsers = $true
            break;
        }
        "test"
        {
            $loadTcHangfireConnectionString = $false
            $loadTestUsers = $false
            break;
        }
        "production"
        {
            $loadTcHangfireConnectionString = $false
            $loadTestUsers = $false
            break;
        }
        default { Throw "Error: Unknnown environment provided: $environmentName" }
    }
    
    Configure-Aws -accessKeyId "$Env:AwsAccessKeyId" -secretAccessKey "$Env:AwsSecretAccessKey"
    Load-Environment-Secrets-From-Aws -envName "$environmentName" -loadTcHangfireConnectionString $loadTcHangfireConnectionString -loadTestUsers $loadTestUsers
    
    Write-Host "Finished configuring $environmentName"
}