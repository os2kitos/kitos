# Set EnvironmentName
$Env:EnvironmentName="integration"

# Check for missing vars
if (-Not (Test-Path 'env:AwsAccessKeyId')) { 
    throw "Error: Remember to set the AwsAccessKeyId input before starting the build"
} 
if (-Not (Test-Path 'env:AwsSecretAccessKey')) { 
    throw "Error: Remember to set the AwsSecretAccessKey input before starting the build"
} 

# Set access keys passed by user
$Env:AWS_ACCESS_KEY_ID=$Env:AwsAccessKeyId
$Env:AWS_SECRET_ACCESS_KEY=$Env:AwsSecretAccessKey

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