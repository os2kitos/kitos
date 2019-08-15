
param(
    [Parameter(Mandatory=$true)][string]$usrname,
    [Parameter(Mandatory=$true)][string]$pwd,
    [Parameter(Mandatory=$true)][string]$url,
	[Parameter(Mandatory=$true)][string]$testType,
	[string]$testToRun
    )
    
    Try 
    {
        $oldSeleniumServer = Get-Process -Id (Get-NetTCPConnection -LocalPort 4444 -ErrorAction Stop).OwningProcess | Where-Object {$_.ProcessName.Equals("java")}
    
        if ($oldSeleniumServer){
            Write-Host "Found unexpected java process on port 4444. Trying to shut down"
            Stop-Process -Id $oldSeleniumServer.Id
            $maxWait = 0
            while(-Not $oldSeleniumServer.HasExited){
                Write-Host "Shutting down..."
                Start-Sleep 1
                $maxWait += 1
                if($maxWait -ge 10){
                    throw "Taking too long to close selenium server on port 4444. Please try again or shutdown manually."
                }
            }
            Write-Host "Succesfully shutdown"
        }
    }
    Catch [Microsoft.PowerShell.Cmdletization.Cim.CimJobException]
    {
        Write-Host "Found nothing using port 4444 as expected. Moving on to starting selenium server"
    }
    Catch
    {
        throw "An unexpected error ocurred. Is your selenium server shut-down correctly?"
    }
    
    try {
        Write-Host "Starting selenium server"

        $app = Start-Process powershell.exe -ArgumentList "webdriver-manager start" -PassThru -WindowStyle Hidden
    
        Write-Host "Starting E2E test. This might take a while..."
		switch($testType){
			"headless" {
				gulp "e2e:headless" --params.login.email="$usrname" --params.login.pwd="$pwd" --baseUrl="$url"
			}
			"local" {
				gulp "e2e:local" --params.login.email="$usrname" --params.login.pwd="$pwd" --baseUrl="$url"
			}
			"single" {
				gulp "e2e:single" --params.login.email="$usrname" --params.login.pwd="$pwd" --baseUrl="$url" --testToRun="$testToRun"
			}
		}
		
        
        
        if($LASTEXITCODE -ne 0)	{ Throw "Integration tests failed" }
    }
    catch{
		Write-Host $_.Exception
        throw $_.Exception
    }
    finally{
        Write-Host "Testing done, shutting down selenium server as clean up"
		try{
			Stop-Process $app.Id
		}catch{
			Write-Host "Failed to stop process that started selenium server, trying to stop the selenium server now"
			Write-Host $_.Exception
		}
        
		try{
			$seleniumServer = Get-Process -Id (Get-NetTCPConnection -LocalPort 4444).OwningProcess | Where-Object {$_.ProcessName.Equals("java")}
			Stop-Process -Id $seleniumServer.Id
		}catch{
			Write-Host "Failed to stop selenium server"
			Write-Host $_.Exception
		}
		
    }
    
    
    
    
    
    