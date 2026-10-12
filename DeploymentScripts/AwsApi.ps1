Function Get-AwsCliPath() {
    if ($script:AwsCliPath) {
        return $script:AwsCliPath
    }

    $awsCommand = Get-Command aws -ErrorAction SilentlyContinue
    if ($awsCommand) {
        $script:AwsCliPath = $awsCommand.Source
        return $script:AwsCliPath
    }

    $candidatePaths = @(
        "$Env:ProgramFiles\\Amazon\\AWSCLIV2\\aws.exe",
        "$Env:ProgramFiles(x86)\\Amazon\\AWSCLIV2\\aws.exe"
    )

    foreach ($candidate in $candidatePaths) {
        if (Test-Path -LiteralPath $candidate) {
            $script:AwsCliPath = $candidate
            $awsDir = Split-Path -Parent $candidate

            # Ensure child scripts can call aws by name once resolved.
            if (-not (($Env:Path -split ';') -contains $awsDir)) {
                $Env:Path = "$Env:Path;$awsDir"
            }

            return $script:AwsCliPath
        }
    }

    throw "AWS CLI not found. Install AWS CLI v2 or ensure aws.exe is on PATH."
}

Function Invoke-AwsCli([string[]] $arguments) {
    $awsPath = Get-AwsCliPath
    & $awsPath @arguments
    if ($LASTEXITCODE -ne 0) {
        throw "AWS CLI command failed with exit code ${LASTEXITCODE}: aws $($arguments -join ' ')"
    }
}

Function Configure-Aws($accessKeyId, $secretAccessKey) {
    Write-Host "Configuring AWS for access key $accessKeyId"
    
    # Set defaults
    $Env:AWS_DEFAULT_REGION="eu-west-1"
    
    # Set access keys passed by caller
    $Env:AWS_ACCESS_KEY_ID=$accessKeyId
    $Env:AWS_SECRET_ACCESS_KEY=$secretAccessKey

    $resolvedAwsPath = Get-AwsCliPath
    Write-Host "Using AWS CLI at: $resolvedAwsPath"
    
    Write-Host "Finished configuring AWS. Active Key Id: $Env:AWS_ACCESS_KEY_ID"
}

Function Get-SSM-Parameter($environmentName, $parameterName) {
    Write-Host "Getting $parameterName from SSM"
    $response = Invoke-AwsCli @("ssm", "get-parameter", "--with-decryption", "--name", "/kitos/$environmentName/$parameterName")
    ($response | ConvertFrom-Json).Parameter.Value.Trim()
}

Function Get-SSM-Parameters($environmentName) {
    $prefix = "/kitos/$environmentName/"
    Write-Host "Getting all SSM Parameters from $prefix"

    $response = Invoke-AwsCli @("ssm", "get-parameters-by-path", "--with-decryption", "--path", $prefix)
    $parameters = ($response | ConvertFrom-Json).Parameters

    # Convert structure to map
    $table = new-object System.Collections.Hashtable
    for($i = 0 ; $i -lt $parameters.Length; $i++) {
        $name = $parameters[$i].Name
        $value = $parameters[$i].Value.Trim()
        $table.Add(($name).Replace($prefix,""),$value)
    }

    #return map
    $table
}