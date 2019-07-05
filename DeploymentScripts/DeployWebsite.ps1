Function Deploy-Website($packageDirectory, $msDeployUrl, $msDeployUser, $msDeployPassword, $logLevel, $esUrl, $ssoGateway, $smtpFromMail, $smtpNwHost, $resetPwTtl, $mailSuffix, $baseUrl, $kitosEnvName, $buildNumber, $kitosDbConnectionString, $hangfireConnectionString) {


	$msdeploy = "C:\Program Files\IIS\Microsoft Web Deploy V3\msdeploy.exe";
	$fullCommand=$("`"{0}`" -verb:sync -source:package=`"{1}\Presentation.Web.csproj.zip`" -dest:auto,computerName=`"{2}`",userName=`"{3}`",password=`"{4}`",authtype=`"Basic`",includeAcls=`"False`" -disableLink:AppPoolExtension -disableLink:ContentExtension -disableLink:CertificateExtension -skip:objectname=`"dirPath`",absolutepath=`"C:\\kitos_tmp\\app\\App_Data$`" -skip:objectname=`"dirPath`",absolutepath=`"Default Web \Site\\App_Data$`" -setParamFile:`"{1}\Presentation.Web.csproj.SetParameters.xml`" -allowUntrusted -setParam:name=`"serilog:minimum-level`",value=`"{5}`" -setParam:name=`"serilog:write-to:Elasticsearch.nodeUris`",value=`"{6}`" -setParam:name=`"SSOGateway`",value=`"{7}`" -setParam:name=`"SmtpFromEmail`",value=`"{8}`" -setParam:name=`"SmtpNetworkHost`",value=`"{9}`" -setParam:name=`"ResetPasswordTTL`",value=`"{10}`" -setParam:name=`"BaseUrl`",value=`"{11}`" -setParam:name=`"MailSuffix`",value=`"{12}`" -setParam:name=`"Environment`",value=`"{13}`" -setParam:name=`"DeploymentVersion`",value=`"{14}`" -setParam:name=`"KitosContext-Web.config Connection String`",value=`"{15}`" -setParam:name=`"kitos_HangfireDB-Web.config Connection String`",value=`"{16}`"" `
	-f $msdeploy, $packageDirectory, $msDeployUrl, $msDeployUser, $msDeployPassword, $logLevel, $esUrl, $ssoGateway, $smtpFromMail, $smtpNwHost, $resetPwTtl, $baseUrl, $mailSuffix, $kitosEnvName, $buildNumber, $kitosDbConnectionString, $hangfireConnectionString )
	
	& cmd.exe /C $fullCommand
	
	if($LASTEXITCODE -ne 0)	{ throw "FAILED TO DEPLOY" } 
}