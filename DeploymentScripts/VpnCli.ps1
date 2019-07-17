Function Disconnect-VPN() {
    Write-Host "Disconnecting VPN sessions"
    
    $AppFolder = Resolve-Path "$PSScriptRoot\..\CiscoAnyConnectTool"
    
    & "$AppFolder\CiscoAnyConnectTool.exe" "disconnect"
    
    if($LASTEXITCODE -ne 0)	{ Throw "FAILED TO DISCONNECT VPN" }
}

Function Connect-VPN([string]$remoteHost, [string]$username, [string]$pwd) {
    Write-Host "Connecting VPN to `"$remoteHost`""
    
    $AppFolder = Resolve-Path "$PSScriptRoot\..\CiscoAnyConnectTool"
    
    & "$AppFolder\CiscoAnyConnectTool.exe" "connect" "$remoteHost" "$username" "$pwd"
    
    if($LASTEXITCODE -ne 0)	{ Throw "FAILED CONNECTING TO VPN at `"$remoteHost`"" }
}