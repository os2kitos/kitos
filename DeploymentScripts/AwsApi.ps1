Function Configure-Aws($accessKeyId, $secretAccessKey) {
    Write-Host "Configuring AWS for access key $accessKeyId"
    
    # Set defaults
    $Env:AWS_DEFAULT_REGION="eu-west-1"
    
    # Set access keys passed by caller
    $Env:AWS_ACCESS_KEY_ID=$accessKeyId
    $Env:AWS_SECRET_ACCESS_KEY=$secretAccessKey
    
    Write-Host "Finished configuring AWS. Active Key Id: $Env:AWS_ACCESS_KEY_ID"
}

Function Get-SSM-Parameter($environmentName, $parameterName) {
    Write-Host "Getting $parameterName from SSM"
    (aws ssm get-parameter --with-decryption --name "/kitos/$environmentName/$parameterName" | ConvertFrom-Json).Parameter.Value
    if($LASTEXITCODE -ne 0)	{ Throw "FAILED TO LOAD $parameterName from $environmentName" }
}

Function Get-SSM-Parameters($environmentName) {
    $prefix = "/kitos/$environmentName/"
    Write-Host "Getting all SSM Parameters from $prefix"

    $parameters = (aws ssm get-parameters-by-path --with-decryption --path "$prefix" | ConvertFrom-Json).Parameters
    
    if($LASTEXITCODE -ne 0)	{ Throw "FAILED TO LOAD SSM parameters from $environmentName" }

    # Convert structure to map
    $table = new-object System.Collections.Hashtable
    for($i = 0 ; $i -lt $parameters.Length; $i++) {
        $name = $parameters[$i].Name
        $value = $parameters[$i].Value
        $table.Add(($name).Replace($prefix,""),$value)
    }

    #return map
    $table
}