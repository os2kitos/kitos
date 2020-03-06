# Stop on first error
$ErrorActionPreference = "Stop"

# Load helper libraries
.\DeploymentSetup.ps1

Setup-Environment -environmentName "integration"