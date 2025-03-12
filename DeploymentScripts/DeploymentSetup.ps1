# Load helper
.$PSScriptRoot\AwsApi.ps1

Function Load-Environment-Secrets-From-Aws([String] $envName, [bool] $loadTcHangfireConnectionString = $true, [bool] $loadTestUsers = $true) {
    Write-Host "Loading environment configuration from SSM"
    
    $parameters = Get-SSM-Parameters -environmentName "$envName"

    if($parameters.Count -eq 0) {
        throw "No parameters found for environment $envName"
    }

    $Env:KitosHostName = $parameters["HostName"]
    $Env:MsDeployUserName = $parameters["MsDeployUserName"]
    $Env:MsDeployPassword = $parameters["MsDeployPassword"]
    $Env:MsDeployUrl = $parameters["MsDeployUrl"]
    $Env:LogLevel = $parameters["LogLevel"]
    $Env:EsUrl = $parameters["EsUrl"]
    $Env:SecurityKeyString = $parameters["SecurityKeyString"]
    $Env:SmtpFromMail = $parameters["SmtpFromMail"]
    $Env:SmtpNetworkHost = $parameters["SmtpNetworkHost"]
    $Env:SmtpNetworkPort = $parameters["SmtpNetworkPort"]
    $Env:SmtpUserName = $parameters["SmtpUserName"]
    $Env:SmtpPassword = $parameters["SmtpPassword"]
    $Env:ResetPasswordTtl = $parameters["ResetPasswordTtl"]
    $Env:MailSuffix = $parameters["MailSuffix"]
    $Env:KitosEnvName = $parameters["KitosEnvName"]
    $Env:KitosDbConnectionStringForIIsApp = $parameters["KitosDbConnectionStringForIIsApp"]
    $Env:HangfireDbConnectionStringForIIsApp = $parameters["HangfireDbConnectionStringForIIsApp"]
    $Env:KitosDbConnectionStringForTeamCity = $parameters["KitosDbConnectionStringForTeamCity"]
    $Env:SsoServiceProviderServer = $parameters["SsoServiceProviderServer"]
    $Env:SsoIDPEndPoints = $parameters["SsoIDPEndPoints"]
    $Env:SsoServiceProviderId = $parameters["SsoServiceProviderId"]
    $Env:SsoCertificateThumbPrint = $parameters["SsoCertificateThumbPrint"]
    $Env:StsOrganisationEndpointHost = $parameters["StsOrganisationEndpointHost"]
    $Env:StsIssuer = $parameters["StsIssuer"]
    $Env:StsCertificateEndpoint = $parameters["StsCertificateEndpoint"]
    $Env:ServiceCertificateAliasOrg = $parameters["ServiceCertificateAliasOrg"]
    $Env:StsCertificateAlias = $parameters["StsCertificateAlias"]
    $Env:StsCertificateThumbprint = $parameters["StsCertificateThumbprint"]
    $Env:OrgService6EntityId = $parameters["OrgService6EntityId"]
    $Env:StsBrugerPort = $parameters["StsBrugerPort"]
    $Env:StsAdressePort = $parameters["StsAdressePort"]
    $Env:StsPersonPort = $parameters["StsPersonPort"]
    $Env:StsVirksomhedPort = $parameters["StsVirksomhedPort"]
    $Env:StsOrganisationPort = $parameters["StsOrganisationPort"]
    $Env:StsOrganisationSystemPort = $parameters["StsOrganisationSystemPort"]
    $Env:StsOrganisationCertificateThumbprint = $parameters["StsOrganisationCertificateThumbprint"]
    $Env:PubSubBaseUrl = $parameters["PubSubBaseUrl"]
    
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
            $Env:Robots = ".*Robots\.Test\.Txt"
            break;
        }
        "dev" 
        {
            $Env:TestToolsPath = Resolve-Path "$PSScriptRoot\..\TestDatabaseTools\Tools.Test.Database.exe"
            $loadTcHangfireConnectionString = $true
            $loadTestUsers = $true
            $Env:UseDefaultUserPassword = "true"
            $Env:Robots = ".*Robots\.Test\.Txt"
            break;
        }
         "staging"
        {
            $loadTcHangfireConnectionString = $false
            $loadTestUsers = $false
            $Env:UseDefaultUserPassword = "false"
            $Env:Robots = ".*Robots\.Test\.Txt"
            break;
        }
        "production"
        {
            $loadTcHangfireConnectionString = $false
            $loadTestUsers = $false
            $Env:UseDefaultUserPassword = "false"
            $Env:Robots = ".*Robots\.Prod\.Txt"
            break;
        }
        default { Throw "Error: Unknown environment provided: $environmentName" }
    }
    
    Configure-Aws -accessKeyId "$Env:AwsAccessKeyId" -secretAccessKey "$Env:AwsSecretAccessKey"
    Load-Environment-Secrets-From-Aws -envName "$environmentName" -loadTcHangfireConnectionString $loadTcHangfireConnectionString -loadTestUsers $loadTestUsers
    
    Write-Host "Finished configuring $environmentName"
}