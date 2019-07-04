# Load helper library
.$PSScriptRoot\AwsApi.ps1

# Load Secrets from parameterstore
$Env:MsDeployUserName = Get-SSM-Parameter -environmentName "$Env:EnvironmentName" -parameterName "MsDeployUserName"
$Env:MsDeployPassword = Get-SSM-Parameter -environmentName "$Env:EnvironmentName" -parameterName "MsDeployPassword"
$Env:KitosDbConnectionStringForIIsApp = Get-SSM-Parameter -environmentName "$Env:EnvironmentName" -parameterName "KitosDbConnectionStringForIIsApp"
$Env:HangfireDbConnectionStringForIIsApp = Get-SSM-Parameter -environmentName "$Env:EnvironmentName" -parameterName "HangfireDbConnectionStringForIIsApp"

.\Presentation.Web.csproj.deploy.cmd `
/Y `
/A:Basic `
/M:$Env:MsDeployServiceUrl `
/U:$Env:MsDeployUserName `
/P:$Env:MsDeployPassword `
-allowUntrusted `
"-setParam:name='serilog:minimum-level',value='$Env:LoggingLevel' `
 -setParam:name='serilog:write-to:Elasticsearch.nodeUris',value='$Env:ElasticsearchNodeUrl' `
 -setParam:name='SSOGateway',value='$Env:SSOGateway' `
 -setParam:name='SmtpFromEmail',value='$Env:SmtpFromEmail' `
 -setParam:name='SmtpNetworkHost',value='$Env:SmtpNetworkHost' `
 -setParam:name='ResetPasswordTTL',value='$Env:ResetPasswordTTL' `
 -setParam:name='BaseUrl',value='$Env:BaseUrl' `
 -setParam:name='MailSuffix',value='$Env:MailSuffix' `
 -setParam:name='Environment',value='$Env:KitosEnvironmentName' `
 -setParam:name='DeploymentVersion',value='$Env:BUILD_NUMBER' `
 -setParam:name='KitosContext-Web.config Connection String',value='$Env:KitosDbConnectionString' `
 -setParam:name='kitos_HangfireDB-Web.config Connection String',value='$Env:HangfireDbConnectionStringForIIsApp'"