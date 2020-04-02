Function CopyAndRenameRobots([String] $robotsFileName)
{	
	Write-Host "Copying $($robotsFileName).txt"
	Copy-Item "$PSScriptRoot\Robots\$($robotsFileName).txt" -Destination "C:\kitos_tmp\app\Robots.txt"
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