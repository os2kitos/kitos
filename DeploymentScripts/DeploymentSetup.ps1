# Load helper
.$PSScriptRoot\AwsApi.ps1

Function Load-Environment-Secrets-From-Aws([String] $envName, [bool] $loadTcHangfireConnectionString = $true, [bool] $loadTestUsers = $true) {
    Write-Host "Loading environment configuration from SSM"
    
    $parameters = Get-SSM-Parameters -environmentName "$envName"

    $Env:KitosHostName = $parameters["HostName"]
    $Env:MsDeployUserName = $parameters["MsDeployUserName"]
    $Env:MsDeployPassword = $parameters["MsDeployPassword"]
    $Env:MsDeployUrl = $parameters["MsDeployUrl"]
    $Env:LogLevel = $parameters["LogLevel"]
    $Env:EsUrl = $parameters["EsUrl"]
    $Env:SsoGateway = $parameters["SsoGateway"]
    $Env:SecurityKeyString = $parameters["SecurityKeyString"]
    $Env:SmtpFromMail = $parameters["SmtpFromMail"]
    $Env:SmtpNetworkHost = $parameters["SmtpNetworkHost"]
    $Env:ResetPasswordTtl = $parameters["ResetPasswordTtl"]
    $Env:MailSuffix = $parameters["MailSuffix"]
    $Env:KitosEnvName = $parameters["KitosEnvName"]
    $Env:KitosDbConnectionStringForIIsApp = $parameters["KitosDbConnectionStringForIIsApp"]
    $Env:HangfireDbConnectionStringForIIsApp = $parameters["HangfireDbConnectionStringForIIsApp"]
    $Env:KitosDbConnectionStringForTeamCity = $parameters["KitosDbConnectionStringForTeamCity"]
    
    if($loadTcHangfireConnectionString -eq $true) {
        $Env:HangfireDbConnectionStringForTeamCity = $parameters["HangfireDbConnectionStringForTeamCity"]
    }
    
    if($loadTestUsers -eq $true) {
        $Env:TestUserGlobalAdmin = $parameters["TestUserGlobalAdmin"]
        $Env:TestUserGlobalAdminPw = $parameters["TestUserGlobalAdminPw"]

        $Env:TestUserLocalAdmin = $parameters["TestUserLocalAdmin"]
        $Env:TestUserLocalAdminPw = $parameters["TestUserLocalAdminPw"]

        $Env:TestUserNormalUser = $parameters["TestUserNormalUser"]
        $Env:TestUserNormalUserPw = $parameters["TestUserNormalUserPw"]
        
        $Env:TestUserApiUser = $parameters["TestUserApiUser"]
        $Env:TestUserApiUserPw = $parameters["TestUserApiUserPw"]
        
        $Env:TestUserApiGlobalAdmin = $parameters["TestUserApiGlobalAdmin"]
        $Env:TestUserApiGlobalAdminPw = $parameters["TestUserApiGlobalAdminPw"]
        $Env:DefaultUserPassword = $parameters["DefaultUserPassword"]
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

    $Env:MigrationsFolder = Resolve-Path "$PSScriptRoot\..\DataAccessApp"

    switch( $environmentName ) 
    {
        "integration" 
        {
            $Env:TestToolsPath = Resolve-Path "$PSScriptRoot\..\TestDatabaseTools\Tools.Test.Database.exe"
            $loadTcHangfireConnectionString = $true
            $loadTestUsers = $true
            $Env:UseDefaultUserPassword = "true"
            break;
        }
        "test"
        {
            $loadTcHangfireConnectionString = $false
            $loadTestUsers = $false
            $Env:UseDefaultUserPassword = "false"
            break;
        }
        "production"
        {
            $loadTcHangfireConnectionString = $false
            $loadTestUsers = $false
            $Env:UseDefaultUserPassword = "false"
            break;
        }
        default { Throw "Error: Unknown environment provided: $environmentName" }
    }
    
    Configure-Aws -accessKeyId "$Env:AwsAccessKeyId" -secretAccessKey "$Env:AwsSecretAccessKey"
    Load-Environment-Secrets-From-Aws -envName "$environmentName" -loadTcHangfireConnectionString $loadTcHangfireConnectionString -loadTestUsers $loadTestUsers
    
    Write-Host "Finished configuring $environmentName"
}