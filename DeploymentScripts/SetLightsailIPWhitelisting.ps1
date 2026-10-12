$ErrorActionPreference = "Stop"

# Retrieve configuration from AWS Parameter Store
Write-Host "Loading allowlist configuration from AWS Parameter Store"

$configJson = aws ssm get-parameter --with-decryption --name "/kitos/lightsail/allowlist/config" | ConvertFrom-Json
if ($LASTEXITCODE -ne 0) { throw "FAILED TO LOAD allowlist configuration from AWS Parameter Store" }

$cfg = $configJson.Parameter.Value | ConvertFrom-Json

Write-Host ""
Write-Host "=== Lightsail IP Whitelisting ==="
Write-Host "Instances: $($cfg.Instances -join ', ')"
Write-Host ""

foreach ($instance in $cfg.Instances) {

    Write-Host "[$(Get-Date -Format 'HH:mm:ss')] Processing instance: $instance"

    # --- Check and enable IPv6 if not already enabled
    $instanceDetails = aws lightsail get-instance `
        --instance-name $instance | ConvertFrom-Json
    
    if ($instanceDetails.instance.isIpv6Enabled -ne $true) {
        Write-Host "  IPv6 is disabled. Enabling IPv6..."
        aws lightsail set-ip-address-type --resource-type Instance --resource-name $instance --ip-address-type dualstack | Out-Null
        Write-Host "  IPv6 enabled. Waiting 10 seconds for changes to propagate..."
        Start-Sleep -Seconds 10
    } else {
        Write-Host "  IPv6 is already enabled"
    }

    # --- Read existing firewall state
    $state = aws lightsail get-instance-port-states `
        --instance-name $instance | ConvertFrom-Json

    if ($state.portStates.Count -eq 0) {
        Write-Host "  No existing public ports"
    }

    # --- Close existing ports explicitly (safe & idempotent)
    foreach ($p in $state.portStates) {
        Write-Host "  Closing $($p.fromPort)-$($p.toPort)/$($p.protocol)"
        aws lightsail close-instance-public-ports `
            --instance-name $instance `
            --port-info "fromPort=$($p.fromPort),toPort=$($p.toPort),protocol=$($p.protocol)"
    }

    # --- Apply whitelist
    foreach ($p in $cfg.Ports) {

        if ($cfg.IPv4.Count -gt 0) {
            $cidrs4 = $cfg.IPv4 -join ","
            Write-Host "  Opening $($p.Port)/$($p.Protocol) IPv4: $cidrs4"
            aws lightsail open-instance-public-ports `
                --instance-name $instance `
                --port-info "fromPort=$($p.Port),toPort=$($p.Port),protocol=$($p.Protocol),cidrs=$cidrs4"
        }

        if ($cfg.IPv6.Count -gt 0) {
            $cidrs6 = $cfg.IPv6 -join ","
            Write-Host "  Opening $($p.Port)/$($p.Protocol) IPv6: $cidrs6"
            aws lightsail open-instance-public-ports `
                --instance-name $instance `
                --port-info "fromPort=$($p.Port),toPort=$($p.Port),protocol=$($p.Protocol),ipv6Cidrs=$cidrs6"
        }
    }

    Write-Host "  ✔ Firewall reconciled for $instance"
    Write-Host ""
}

Write-Host "=== Done ==="
