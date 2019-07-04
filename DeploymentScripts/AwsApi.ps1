$Env:AWS_DEFAULT_REGION="eu-west-1"

Function Get-SSM-Parameter($environmentName, $parameterName) {
    (aws ssm get-parameter --with-decryption --name "/kitos/$environmentName/$parameterName" | ConvertFrom-Json).Parameter.Value
}