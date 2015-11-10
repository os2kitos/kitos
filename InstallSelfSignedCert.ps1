$cert = New-SelfSignedCertificate -DnsName ("localhost") -CertStoreLocation cert:\LocalMachine\My
$rootStore = Get-Item cert:\LocalMachine\Root
$rootStore.Open("ReadWrite")
$rootStore.Add($cert)
$rootStore.Close();
# Import-Module WebAdministration
# Set-Location IIS:\SslBindings
# New-WebBinding -Name "Kitos" -IP "*" -Port 44300 -Protocol https
# $cert | New-Item 0.0.0.0!44300
