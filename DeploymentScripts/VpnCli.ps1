Function Disconnect-VPN() {
    Write-Host "Disconnecting VPN sessions"
    
    $AppFolder = Resolve-Path "$PSScriptRoot\..\CiscoAnyConnectTool"
    
    & "$AppFolder\CiscoAnyConnectTool.exe" "disconnect"
    
    if($LASTEXITCODE -ne 0)	{ Throw "FAILED TO DISCONNECT VPN" }
}

Function Connect-VPN([string]$remoteHost, [string]$username, [string]$pwd) {
    Write-Host "Connecting VPN to `"$remoteHost`""
    
    $AppFolder = Resolve-Path "$PSScriptRoot\..\CiscoAnyConnectTool"
    
    $output = & "$AppFolder\CiscoAnyConnectTool.exe" "connect" "$remoteHost" "$username" "$pwd" 2>&1 | Out-String
    Write-Host $output
    
    if($LASTEXITCODE -ne 0)	{ Throw "FAILED CONNECTING TO VPN at `"$remoteHost`"" }
    
    # Verify connection actually succeeded by checking output for error messages
    if($output -match "error:|unable to|failed|not able to establish") {
        Throw "VPN connection failed: Connection command completed but VPN did not establish. Check credentials and VPN server requirements."
    }
    
    Write-Host "VPN connection established successfully"
}