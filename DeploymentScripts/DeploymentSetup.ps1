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
    $Env:KitosDbProvider = if ([string]::IsNullOrWhiteSpace($parameters["KitosDbProvider"])) { "PostgreSql" } else { $parameters["KitosDbProvider"] }
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
    $Env:SmtpDeliveryMethod = $parameters["SmtpDeliveryMethod"]

    if($loadTcHangfireConnectionString -eq $true) {
        $Env:HangfireDbConnectionStringForTeamCity = $parameters["HangfireDbConnectionStringForTeamCity"]
    }

    # When the DB provider is PostgreSQL, override connection strings with values from
    # the PostgreSQL-compatible environment SSM path (same environment name, e.g. /kitos/integration/).
    if ($Env:KitosDbProvider -and (
        $Env:KitosDbProvider.Equals("Postgres", [System.StringComparison]::OrdinalIgnoreCase) -or
        $Env:KitosDbProvider.Equals("PostgreSql", [System.StringComparison]::OrdinalIgnoreCase) -or
        $Env:KitosDbProvider.Equals("Npgsql", [System.StringComparison]::OrdinalIgnoreCase)
    )) {
        $pgEnvName = $envName
        Write-Host "KitosDbProvider is PostgreSQL - loading DB connection strings from SSM path: /kitos/$pgEnvName/"
        $pgParameters = Get-SSM-Parameters -environmentName "$pgEnvName"

        if ($pgParameters.Count -eq 0) {
            throw "No parameters found for Postgres environment $pgEnvName"
        }

        $Env:KitosHostName = $pgParameters["HostName"]
        $Env:MsDeployUrl    = $pgParameters["MsDeployUrl"]
        $Env:KitosDbConnectionStringForIIsApp    = $pgParameters["KitosDbConnectionStringForIIsApp"]
        $Env:HangfireDbConnectionStringForIIsApp = $pgParameters["HangfireDbConnectionStringForIIsApp"]
        $Env:KitosDbConnectionStringForTeamCity  = $pgParameters["KitosDbConnectionStringForTeamCity"]
        $Env:KitosDbProvider = "PostgreSql"

        if ($loadTcHangfireConnectionString -eq $true) {
            $Env:HangfireDbConnectionStringForTeamCity = $pgParameters["HangfireDbConnectionStringForTeamCity"]
        }
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

        $Env:TestUserSystemIntegrator = $parameters["TestUserSystemIntegrator"]
        $Env:TestUserSystemIntegratorPw = $parameters["TestUserSystemIntegratorPw"]

        $Env:DefaultUserPassword = $parameters["DefaultUserPassword"]
    }
    
    
    Write-Host "Finished loading environment configuration from SSM"
}

# Emits ##teamcity[setParameter] messages for every env: variable currently set in this process,
# making them available to all subsequent TeamCity build steps as environment variables.
Function Publish-TeamCityParameters() {
    if (-not $Env:TEAMCITY_VERSION) { return }  # no-op outside TeamCity

    # These TeamCity parameters are marked readOnly and must not be re-published.
    $variablesNotToPublish = @("AwsAccessKeyId", "AwsSecretAccessKey")

    Get-ChildItem Env: | ForEach-Object {
        if ($variablesNotToPublish -contains $_.Name) { return }
        if ([string]::IsNullOrEmpty($_.Value)) { return }

        $escaped = $_.Value -replace '\|', '||' -replace "'", "|'" -replace '\[', '|[' -replace '\]', '|]' -replace "`n", '|n' -replace "`r", '|r'
        Write-Host "##teamcity[setParameter name='env.$($_.Name)' value='$escaped']"
    }
}

Function Setup-Environment([String] $environmentName) {
    Write-Host "Configuring Deployment Environment $environmentName"
    
    if (-Not (Test-Path 'env:AwsAccessKeyId')) { 
    	throw "Error: Remember to set the AwsAccessKeyId input before starting the build"
    } 
    if (-Not (Test-Path 'env:AwsSecretAccessKey')) { 
    	throw "Error: Remember to set the AwsSecretAccessKey input before starting the build"
    }

    $forcePickupDirectorySmtp = $false

    switch( $environmentName )
    {
        "integration" 
        {
            $Env:TestToolsPath = Resolve-Path "$PSScriptRoot\..\TestDatabaseTools\Tools.Test.Database.exe"
            $loadTcHangfireConnectionString = $true
            $loadTestUsers = $true
            $Env:UseDefaultUserPassword = "true"
            $Env:Robots = ".*Robots\.Test\.Txt"
            $forcePickupDirectorySmtp = $true
            break;
        }
        "dev" 
        {
            $Env:TestToolsPath = Resolve-Path "$PSScriptRoot\..\TestDatabaseTools\Tools.Test.Database.exe"
            $loadTcHangfireConnectionString = $true
            $loadTestUsers = $true
            $Env:UseDefaultUserPassword = "true"
            $Env:Robots = ".*Robots\.Test\.Txt"
            $forcePickupDirectorySmtp = $true
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

    # Apply after SSM load so these cannot be overwritten by SSM parameters
    if ($forcePickupDirectorySmtp) {
        $Env:SmtpDeliveryMethod = "SpecifiedPickupDirectory"
    } else {
        $Env:SmtpDeliveryMethod = "Network"
    }

    # Emit TeamCity service messages after all overrides are applied so subsequent
    # build steps receive the final values (not the pre-override SSM values).
    Publish-TeamCityParameters

    Write-Host "Finished configuring $environmentName"
}