Function CopyAndRenameRobots([String] $robotsFileName)
{
	Write-Debug "Extracting files"
	Expand-Archive "$PSScriptRoot\..\WebPackage\Presentation.Web.csproj.zip" -DestinationPath "$PSScriptRoot\..\WebPackage\files-to-edit\"
	
	Write-Debug "Copying Robot file"
	Copy-Item "$PSScriptRoot\Robots\$($robotsFileName).txt" -Destination "$PSScriptRoot\..\WebPackage\files-to-edit\Content\C_C\kitos_tmp\app\Robots.txt"
	
	Write-Debug "Zipping file"
	Compress-Archive "$PSScriptRoot\..\WebPackage\files-to-edit\**" -DestinationPath "$PSScriptRoot\..\WebPackage\Presentation.Web.csproj.zip" -Force
	
	Write-Debug "Removing temp files"
	Remove-Item "$PSScriptRoot\..\WebPackage\files-to-edit" -recurse
	
	Write-Debug "Robots.txt filetransfer complete"
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