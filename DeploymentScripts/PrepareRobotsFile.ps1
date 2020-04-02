Function CopyAndRenameRobots([String] $robotsFileName)
{
	Write-Host "Extracting zip file"
	Expand-Archive "$PSScriptRoot\..\WebPackage\Presentation.Web.csproj.zip" -DestinationPath "$PSScriptRoot\..\WebPackage\files-to-edit\"
	
	Write-Host "Copying Robots.txt"
	Copy-Item "$PSScriptRoot\Robots\$($robotsFileName).txt" -Destination "$PSScriptRoot\..\WebPackage\files-to-edit\Content\C_C\kitos_tmp\app\Robots.txt"
	
	Write-Host "Rezipping"
	Compress-Archive "$PSScriptRoot\..\WebPackage\files-to-edit\**" -DestinationPath "$PSScriptRoot\..\WebPackage\Presentation.Web.csproj.zip" -Force
	
	Write-Host "Robots import complete waiting"
	Start-Sleep -s 15
	Write-Host "Wait finish"
}

Function Prepare-RobotsFile([String] $targetEnvironment)
{
    switch( $targetEnvironment ) 
    {
        "integration" 			
			{
				CopyAndRenameRobots -robotsFileName "Test_Robots"
            break;
        }
        "test"
        {
			CopyAndRenameRobots -robotsFileName "Test_Robots"
            break;
        }
        "production"
        {
			CopyAndRenameRobots -robotsFileName "Prod_Robots"
            break;
        }
        default { Throw "Error: Unknown environment provided: $targetEnvironment" }
    }
}