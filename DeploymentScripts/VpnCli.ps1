Function Disconnect() {
    Write-Host "Disconnecting VPN sessions"
    
    $AppFolder = Resolve-Path "$PSScriptRoot\..\CiscoAnyConnectTool"
    
    & "$AppFolder\CiscoAnyConnectTool.exe" "disconnect"
    
    if($LASTEXITCODE -ne 0)	{ Throw "FAILED DISCONNECT VPN" }
}

Function Connect([string]$host, [string]$username, [string]$pwd) {
    Write-Host "Connecting VPN to $host"
    
    $AppFolder = Resolve-Path "$PSScriptRoot\..\CiscoAnyConnectTool"
    
    & "$AppFolder\CiscoAnyConnectTool.exe" "connect" "$host" "$username" "$pwd"
    
    if($LASTEXITCODE -ne 0)	{ Throw "FAILED CONNECTING TO VPN $host" }
}