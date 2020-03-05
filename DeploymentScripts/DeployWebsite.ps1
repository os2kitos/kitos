Function Deploy-Website($packageDirectory, $msDeployUrl, $msDeployUser, $msDeployPassword, $logLevel, $esUrl, $ssoGateway, $securityKeyString, $smtpFromMail, $smtpNwHost, $resetPwTtl, $mailSuffix, $baseUrl, $kitosEnvName, $buildNumber, $kitosDbConnectionString, $hangfireConnectionString, $defaultUserPassword, $useDefaultUserPassword, $ssoServiceProviderServer, $ssoIDPEndpoints) {

    $msdeploy = "C:\Program Files\IIS\Microsoft Web Deploy V3\msdeploy.exe";
    $fullCommand=$(("`"{0}`" " +  
                    "-verb:sync " +
                    "-source:package=`"{1}\Presentation.Web.csproj.zip`" " +
                    "-dest:auto,computerName=`"{2}`",userName=`"{3}`",password=`"{4}`",authtype=`"Basic`",includeAcls=`"False`" " +
                    "-disableLink:AppPoolExtension " +
                    "-disableLink:ContentExtension " + 
                    "-disableLink:CertificateExtension " + 
                    "-skip:objectname=`"dirPath`",absolutepath=`"C:\\kitos_tmp\\app\\App_Data$`" " + 
                    "-skip:objectname=`"dirPath`",absolutepath=`"Default Web \Site\\App_Data$`" " + 
                    "-setParamFile:`"{1}\Presentation.Web.csproj.SetParameters.xml`" -allowUntrusted " + 
                    "-setParam:name=`"serilog:minimum-level`",value=`"{5}`" " + 
                    "-setParam:name=`"serilog:write-to:Elasticsearch.nodeUris`",value=`"{6}`" " + 
                    "-setParam:name=`"SSOGateway`",value=`"{7}`" " + 
                    "-setParam:name=`"SecurityKeyString`",value=`"{8}`" " + 
                    "-setParam:name=`"SmtpFromEmail`",value=`"{9}`" " + 
                    "-setParam:name=`"SmtpNetworkHost`",value=`"{10}`" " + 
                    "-setParam:name=`"ResetPasswordTTL`",value=`"{11}`" " + 
                    "-setParam:name=`"BaseUrl`",value=`"{12}`" " + 
                    "-setParam:name=`"MailSuffix`",value=`"{13}`" " + 
                    "-setParam:name=`"Environment`",value=`"{14}`" " + 
                    "-setParam:name=`"DeploymentVersion`",value=`"{15}`" " + 
                    "-setParam:name=`"KitosContext-Web.config Connection String`",value=`"{16}`" " + 
                    "-setParam:name=`"kitos_HangfireDB-Web.config Connection String`",value=`"{17}`" " +
                    "-setParam:name=`"DefaultUserPassword`",value=`"{18}`" " +
                    "-setParam:name=`"UseDefaultPassword`",value=`"{19}`" " +
                    "-setParam:name=`"ssoServiceProviderServer`",value=`"{20}`" " +
                    "-setParam:name=`"ssoIDPEndpoints`",value=`"{21}`" "
                    ) `
    -f $msdeploy, $packageDirectory, $msDeployUrl, $msDeployUser, $msDeployPassword, $logLevel, $esUrl, $ssoGateway, $securityKeyString, $smtpFromMail, $smtpNwHost, $resetPwTtl, $baseUrl, $mailSuffix, $kitosEnvName, $buildNumber, $kitosDbConnectionString, $hangfireConnectionString, $defaultUserPassword, $useDefaultUserPassword, $ssoServiceProviderServer, $ssoIDPEndpoints)
    
    & cmd.exe /C $fullCommand
    
    if($LASTEXITCODE -ne 0)	{ throw "FAILED TO DEPLOY" } 
}