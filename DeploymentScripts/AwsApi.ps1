Function Configure-Aws-From-User-Input() {
    # Check for missing vars
	if (-Not (Test-Path 'env:AwsAccessKeyId')) { 
		throw "Error: Remember to set the AwsAccessKeyId input before starting the build"
	} 
	if (-Not (Test-Path 'env:AwsSecretAccessKey')) { 
		throw "Error: Remember to set the AwsSecretAccessKey input before starting the build"
	} 

	# Set defaults
	$Env:AWS_DEFAULT_REGION="eu-west-1"
	
	# Set access keys passed by user
	$Env:AWS_ACCESS_KEY_ID=$Env:AwsAccessKeyId
	$Env:AWS_SECRET_ACCESS_KEY=$Env:AwsSecretAccessKey
}

Function Get-SSM-Parameter($environmentName, $parameterName) {
    (aws ssm get-parameter --with-decryption --name "/kitos/$environmentName/$parameterName" | ConvertFrom-Json).Parameter.Value
}

Function Load-Environment-Secrets-From-Aws($envName) {
	# Load Secrets from parameterstore
	$Env:MsDeployUserName = Get-SSM-Parameter -environmentName "$envName" -parameterName "MsDeployUserName"
	$Env:MsDeployPassword = Get-SSM-Parameter -environmentName "$envName" -parameterName "MsDeployPassword"
	$Env:KitosDbConnectionStringForIIsApp = Get-SSM-Parameter -environmentName "$envName" -parameterName "KitosDbConnectionStringForIIsApp"
	$Env:HangfireDbConnectionStringForIIsApp = Get-SSM-Parameter -environmentName "$envName" -parameterName "HangfireDbConnectionStringForIIsApp"
}