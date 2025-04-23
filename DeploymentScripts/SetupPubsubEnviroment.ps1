.$PSScriptRoot\AwsApi.ps1

Function Load-Pubsub-Parameters([String] $envName) {
    Configure-Aws -accessKeyId "$Env:AwsAccessKeyId" -secretAccessKey "$Env:AwsSecretAccessKey"
	$parameters = Get-SSM-Parameters -environmentName "$envName"

    if($parameters.Count -eq 0) {
        throw "No parameters found for environment $envName"
    }

    $Env:RABBIT_MQ_USER                     = $parameters["RabbitMqUsername"]
    $Env:RABBIT_MQ_PASSWORD                 = $parameters["RabbitMqPassword"]
    $Env:PUBSUB_API_KEY                     = $parameters["PubSubApiKey"]
    $Env:CERT_PASSWORD                      = $parameters["CertPassword"]
    $Env:IDP_HOST_MAPPING                   = $parameters["IdpHostMapping"]
    $Env:PUBSUB_CONNECTION_STRING           = $parameters["PubSubConnectionString"]
    $Env:PUBSUB_MIGRATION_CONNECTION_STRING = $parameters["PubSubMigrationConnectionString"]
    $Env:PUBSUB_REMOTE_TARGET_USER          = $parameters["PubSubRemoteTargetUser"]
    $Env:PUBSUB_REMOTE_TARGET_HOST          = $parameters["PubSubRemoteTargetHost"]
    $Env:PUBSUB_REMOTE_TARGET_PATH          = $parameters["PubSubRemoteTargetPath"]
}
