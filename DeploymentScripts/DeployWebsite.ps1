Function Deploy-Website($msDeployUrl, $msDeployUser, $msDeployPassword, $logLevel, $esUrl, $ssoGateway, $smtpFromMail, $smtpNwHost, $resetPwTtl, $mailSuffix, $baseUrl, $kitosEnvName, $buildNumber, $kitosDbConnectionString, $hangfireConnectionString) {
	.\WebPackage\Presentation.Web.csproj.deploy.cmd `
	/Y `
	/A:Basic `
	/M:$msDeployUrl `
	/U:$msDeployUser `
	/P:$msDeployPassword `
	-allowUntrusted `
	"-setParam:name='serilog:minimum-level',value='$logLevel' -setParam:name='serilog:write-to:Elasticsearch.nodeUris',value='$esUrl' -setParam:name='SSOGateway',value='$ssoGateway' -setParam:name='SmtpFromEmail',value='$smtpFromMail' -setParam:name='SmtpNetworkHost',value='$smtpNwHost' -setParam:name='ResetPasswordTTL',value='$resetPwTtl' -setParam:name='BaseUrl',value='$baseUrl' -setParam:name='MailSuffix',value='$mailSuffix' -setParam:name='Environment',value='$kitosEnvName' -setParam:name='DeploymentVersion',value='$buildNumber' -setParam:name='KitosContext-Web.config Connection String',value='$kitosDbConnectionString' -setParam:name='kitos_HangfireDB-Web.config Connection String',value='$hangfireConnectionString'"
}

#	.\WebPackage\Presentation.Web.csproj.deploy.cmd `
#	/Y `
#	/A:Basic `
#	/M:$Env:MsDeployServiceUrl `
#	/U:$Env:MsDeployUserName `
#	/P:$Env:MsDeployPassword `
#	-allowUntrusted `
#	"-setParam:name='serilog:minimum-level',value='$Env:LoggingLevel' -setParam:name='serilog:write-to:Elasticsearch.nodeUris',value='$Env:ElasticsearchNodeUrl' -setParam:name='SSOGateway',value='$Env:SSOGateway' -setParam:name='SmtpFromEmail',value='$Env:SmtpFromEmail' -setParam:name='SmtpNetworkHost',value='$Env:SmtpNetworkHost' -setParam:name='ResetPasswordTTL',value='$Env:ResetPasswordTTL' -setParam:name='BaseUrl',value='$Env:BaseUrl' -setParam:name='MailSuffix',value='$Env:MailSuffix' -setParam:name='Environment',value='$Env:KitosEnvironmentName' -setParam:name='DeploymentVersion',value='$Env:BUILD_NUMBER' -setParam:name='KitosContext-Web.config Connection String',value='$Env:KitosDbConnectionStringForIIsApp' -setParam:name='kitos_HangfireDB-Web.config Connection String',value='$Env:HangfireDbConnectionStringForIIsApp'"