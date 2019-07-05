# Stop on first error
$ErrorActionPreference = "Stop"

# Load helper library
.$PSScriptRoot\AwsApi.ps1

Configure-Aws-From-User-Input
Load-Environment-Secrets-From-Aws -envName "integration"

# Non-secret configuration parameters
$Env:MsDeployServiceUrl="https://172.26.2.34:8172/msdeploy.axd"
$Env:LoggingLevel="verbose"
$Env:ElasticsearchNodeUrl="http://localhost:9200"
$Env:SSOGateway="https://os2sso-test.miracle.dk/.well-known/openid-configuration"
$Env:SmtpFromEmail="ci-kitos@strongminds.dk"
$Env:SmtpNetworkHost="127.0.0.1"
$Env:ResetPasswordTTL="10.00:00:00"
$Env:BaseUrl="https://kitos-integration.strongminds.dk/"
$Env:MailSuffix="(SAW CI)"
$Env:KitosEnvironmentName="dev"

# Execute the deployment script
.$PSScriptRoot\DeployWebsite.ps1 -ErrorAction Stop

exit $lastexitcode