Function Deploy-Website($packageDirectory, $msDeployUrl, $msDeployUser, $msDeployPassword) {

    $msdeploy = "C:\Program Files\IIS\Microsoft Web Deploy V3\msdeploy.exe"

    # All environment-specific configuration is now baked into appsettings.json by Prepare-Package.
    # This function only handles the file sync to the remote IIS server.
    # The backend always deploys to the Default Web Site root (C:\inetpub\wwwroot) across all environments.
    $fullCommand = ("`"{0}`" " +
                    "-verb:sync " +
                    "-source:package=`"{1}\Presentation.Web.zip`" " +
                    "-dest:contentPath=`"Default Web Site`",computerName=`"{2}`",userName=`"{3}`",password=`"{4}`",authtype=`"Basic`",includeAcls=`"False`" " +
                    "-enableRule:AppOffline " +
                    "-disableLink:AppPoolExtension " +
                    "-disableLink:ContentExtension " +
                    "-disableLink:CertificateExtension " +
                    "-skip:objectname=`"dirPath`",absolutepath=`"App_Data$`" " +
                    "-skip:objectname=`"dirPath`",absolutepath=`".*\\runtimes($|\\.*)`" " +
                    "-skip:objectname=`"filePath`",absolutepath=`".*\\runtimes\\.*`" " +
                    "-allowUntrusted") `
    -f $msdeploy, $packageDirectory, $msDeployUrl, $msDeployUser, $msDeployPassword

    & cmd.exe /C $fullCommand

    if($LASTEXITCODE -ne 0) { throw "FAILED TO DEPLOY" }
}