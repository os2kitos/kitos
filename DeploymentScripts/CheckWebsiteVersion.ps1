Function Check-Website-Version($hostName, $expectedVersion) {
    $response = Invoke-WebRequest -uri "https://$hostName/api/HealthCheck" -UseBasicParsing
	
	if($response.StatusCode -ne 200) { 
		Throw "Invalid response code: $response.StatusCode"
	}
	
	if($response.Content.Trim('"') -ne ($expectedVersion).ToString()) {
		Throw "Invalid version detected. Expected $expectedVersion but got $response.Content"
	}
	
	Write-Host "All good - expected version ($expectedVersion) was found at $hostName"
}