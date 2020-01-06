param(	
    [string]$testToRun
    )
# Stop on first error
$ErrorActionPreference = "Stop"

.$PSScriptRoot\RunE2ETests.ps1 `
                            -url "https://localhost:44300" `
                            -usrname "[local-global-admin-user@kitos.dk, local-local-admin-user@kitos.dk, local-regular-user@kitos.dk, local-api-user@kitos.dk]" `
                            -pwd "[localNoSecret, localNoSecret, localNoSecret, localNoSecret]" `
							-testType "single" `
                            -testToRun "$testToRun"
