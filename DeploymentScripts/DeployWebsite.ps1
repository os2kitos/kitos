Function Deploy-Website($msDeployUrl, $msDeployUser, $msDeployPassword, $logLevel, $esUrl, $ssoGateway, $smtpFromMail, $smtpNwHost, $resetPwTtl, $mailSuffix, $baseUrl, $kitosEnvName, $buildNumber, $kitosDbConnectionString, $hangfireConnectionString) {

	& "C:\Program Files\IIS\Microsoft Web Deploy V3\msdeploy.exe" `
		-source:package="C:\TeamCity\buildAgent\work\d1df6864f98d2599\WebPackage\Presentation.Web.csproj.zip" `
		-dest:auto,computerName="$msDeployUrl",userName="$msDeployUser",password="$msDeployPassword",authtype="Basic",includeAcls="False" `
		-verb:sync `
		-disableLink:AppPoolExtension `
		-disableLink:ContentExtension `
		-disableLink:CertificateExtension `
		-skip:objectname="dirPath",absolutepath="C:\kitos_tmp\app\App_Data$" `
		-skip:objectname="dirPath",absolutepath="Default Web Site\App_Data$" `
		-setParamFile:"$PSScriptRoot\Presentation.Web.csproj.SetParameters.xml"  `
		-allowUntrusted `
		-setParam:name="serilog:minimum-level",value="$logLevel" `
		-setParam:name="serilog:write-to:Elasticsearch.nodeUris",value="$esUrl" `
		-setParam:name="SSOGateway",value="$ssoGateway" `
		-setParam:name="SmtpFromEmail",value="$smtpFromMail" `
		-setParam:name="SmtpNetworkHost",value="$smtpNwHost" `
		-setParam:name="ResetPasswordTTL",value="$resetPwTtl" `
		-setParam:name="BaseUrl",value="$baseUrl" `
		-setParam:name="MailSuffix",value="$mailSuffix" `
		-setParam:name="Environment",value="$kitosEnvName" `
		-setParam:name="DeploymentVersion",value="$buildNumber" `
		-setParam:name="KitosContext-Web.config Connection String",value="$kitosDbConnectionString" `
		-setParam:name="kitos_HangfireDB-Web.config Connection String",value="$hangfireConnectionString"
}