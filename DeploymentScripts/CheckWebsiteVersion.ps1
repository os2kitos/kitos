# Stop on first error
$ErrorActionPreference = "Stop"

Function Check-Website-Version($hostName, $expectedVersion) {
	$response = Invoke-WebRequest -uri "$https://root/api/HealthCheck" –UseBasicParsing
	if($response.StatusCode -ne 200) {
		Throw "Invalid response code received $response.StatusCode"
	}

	if($response.Content.Trim('"') -ne ($expectedVersion).ToString()) {
		Throw "Invalid Deployment version received $response.Content. Expected $expectedVersion"
	}

	Write-Host "All good - website is deployed in the selected version"
}