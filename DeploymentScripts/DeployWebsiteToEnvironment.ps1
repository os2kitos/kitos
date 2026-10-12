param(
    [Parameter(Mandatory=$true)][string]$targetEnvironment
    )

# Stop on first error
$ErrorActionPreference = "Stop"

# Load helper libraries
.$PSScriptRoot\DeploymentSetup.ps1
.$PSScriptRoot\PreparePackage.ps1
.$PSScriptRoot\DeployWebsite.ps1

Setup-Environment -environmentName $targetEnvironment

$webPackageDir = Resolve-Path "$PSScriptRoot\..\WebPackage"
$presentationWebArchive = Join-Path $webPackageDir "Presentation.Web.zip"

Prepare-Package -environmentName $targetEnvironment -pathToArchive (Resolve-Path $presentationWebArchive)

Deploy-Website  -packageDirectory $webPackageDir `
                -msDeployUrl "$Env:MsDeployUrl" `
                -msDeployUser $Env:MsDeployUserName `
                -msDeployPassword $Env:MsDeployPassword
