$cert = New-SelfSignedCertificate -DnsName ("localtest.me","*.localtest.me") -CertStoreLocation cert:\LocalMachine\My
$rootStore = Get-Item cert:\LocalMachine\Root
$rootStore.Open("ReadWrite")
$rootStore.Add($cert)
$rootStore.Close();
Import-Module WebAdministration
Set-Location IIS:\SslBindings
New-WebBinding -Name "Default Web Site" -IP "*" -Port 443 -Protocol https
$cert | New-Item 0.0.0.0!443
