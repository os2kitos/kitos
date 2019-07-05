.\WebPackage\Presentation.Web.csproj.deploy.cmd `
/Y `
/A:Basic `
/M:$Env:MsDeployServiceUrl `
/U:$Env:MsDeployUserName `
/P:$Env:MsDeployPassword `
-allowUntrusted `
"-setParam:name='serilog:minimum-level',value='$Env:LoggingLevel' -setParam:name='serilog:write-to:Elasticsearch.nodeUris',value='$Env:ElasticsearchNodeUrl' -setParam:name='SSOGateway',value='$Env:SSOGateway' -setParam:name='SmtpFromEmail',value='$Env:SmtpFromEmail' -setParam:name='SmtpNetworkHost',value='$Env:SmtpNetworkHost' -setParam:name='ResetPasswordTTL',value='$Env:ResetPasswordTTL' -setParam:name='BaseUrl',value='$Env:BaseUrl' -setParam:name='MailSuffix',value='$Env:MailSuffix' -setParam:name='Environment',value='$Env:KitosEnvironmentName' -setParam:name='DeploymentVersion',value='$Env:BUILD_NUMBER' -setParam:name='KitosContext-Web.config Connection String',value='$Env:KitosDbConnectionStringForIIsApp' -setParam:name='kitos_HangfireDB-Web.config Connection String',value='$Env:HangfireDbConnectionStringForIIsApp'"