Function Prepare-Package([String] $environmentName, $pathToArchive) {
	function Update-SamlConfigFile([string] $configPath) {
		if (-not (Test-Path -LiteralPath $configPath)) {
			Write-Host "Skipping SAML config update (file not found): $configPath"
			return
		}

		Write-Host "Updating SAML config in $configPath"

		$xml = New-Object System.Xml.XmlDocument
		$xml.PreserveWhitespace = $true
		$xml.Load($configPath)

		$federation = $xml.SelectSingleNode("/*[local-name()='configuration']/*[local-name()='Federation']")
		if ($federation -ne $null) {
			if ($Env:SsoCertificateThumbPrint) {
				$signingCert = $federation.SelectSingleNode(".//*[local-name()='SigningCertificate']")
				if ($signingCert -ne $null) {
					$signingCert.SetAttribute("findValue", $Env:SsoCertificateThumbPrint)
				}
			}

			if ($Env:SsoServiceProviderId) {
				$audience = $federation.SelectSingleNode(".//*[local-name()='AllowedAudienceUris']/*[local-name()='Audience']")
				if ($audience -ne $null) {
					$audience.InnerText = $Env:SsoServiceProviderId
				}
			}
		}

		$saml20Federation = $xml.SelectSingleNode("/*[local-name()='configuration']/*[local-name()='SAML20Federation']")
		if ($saml20Federation -ne $null) {
			$serviceProvider = $saml20Federation.SelectSingleNode(".//*[local-name()='ServiceProvider']")
			if ($serviceProvider -ne $null) {
				if ($Env:SsoServiceProviderId) {
					$serviceProvider.SetAttribute("id", $Env:SsoServiceProviderId)
				}
				if ($Env:SsoServiceProviderServer) {
					$serviceProvider.SetAttribute("server", $Env:SsoServiceProviderServer)
				}
			}

			if ($Env:SsoIDPEndPoints) {
				$idp = $saml20Federation.SelectSingleNode(".//*[local-name()='IDPEndPoints']/*[local-name()='add' and @default='true']")
				if ($idp -eq $null) {
					$idp = $saml20Federation.SelectSingleNode(".//*[local-name()='IDPEndPoints']/*[local-name()='add'][1]")
				}
				if ($idp -ne $null) {
					$idp.SetAttribute("id", $Env:SsoIDPEndPoints)
				}
			}
		}

		$settings = New-Object System.Xml.XmlWriterSettings
		$settings.Encoding = New-Object System.Text.UTF8Encoding($false)
		$settings.Indent = $true
		$settings.OmitXmlDeclaration = $false

		$writer = [System.Xml.XmlWriter]::Create($configPath, $settings)
		$xml.Save($writer)
		$writer.Dispose()
	}

	Write-Host "Environment is $environmentName"

	[string]$archivePath = $pathToArchive.Path
	$item = Get-Item -LiteralPath $archivePath -Force -ErrorAction Stop

	$destination = Join-Path (Get-Location) "TEMP_PresentationWeb"

	Write-Host "Resolved path: $($item.FullName)"
	Write-Host "PSIsContainer: $($item.PSIsContainer)"
	Write-Host "Destination: $destination"

	# Ensure clean destination
	if (Test-Path -LiteralPath $destination) {
		Remove-Item -LiteralPath $destination -Recurse -Force
	}

	New-Item -ItemType Directory -Path $destination | Out-Null

	if ($item.PSIsContainer) {
		Write-Host "Source is a directory → copying contents"

		Copy-Item -Path (Join-Path $item.FullName '*') `
				  -Destination $destination `
				  -Recurse -Force
	}
	else {
		Write-Host "Source is a zip → extracting"

		Expand-Archive -LiteralPath $item.FullName `
					   -DestinationPath $destination `
					   -Force
	}

	Write-Host "Files prepared in: $destination"

	if (Test-Path -LiteralPath $pathToArchive) {
		Remove-Item -LiteralPath $pathToArchive -Recurse -Force -Confirm:$false
	}

	Write-Host "Updating appsettings.json"
	$appSettingsPath = ".\TEMP_PresentationWeb\appsettings.json"
	$appSettings = Get-Content $appSettingsPath -Raw | ConvertFrom-Json
	Write-Host "Prepare-Package using KitosDbProvider='$($Env:KitosDbProvider)'"

	# Database provider (resolved in DeploymentSetup.ps1)
	$appSettings.Database.Provider  = $Env:KitosDbProvider
	$appSettings.Hangfire.Provider  = $Env:KitosDbProvider

	# Connection strings
	$appSettings.ConnectionStrings.KitosContext     = $Env:KitosDbConnectionStringForIIsApp
	$appSettings.ConnectionStrings.kitos_HangfireDB = $Env:HangfireDbConnectionStringForIIsApp

	# SMTP
	$appSettings.Smtp.From           = $Env:SmtpFromMail
	# Only overwrite DeliveryMethod if the env var is explicitly set; otherwise keep the appsettings.json default.
	# This prevents a missing/null SSM value from blanking the field and causing SmtpClient to fail.
	if ($Env:SmtpDeliveryMethod) { $appSettings.Smtp.DeliveryMethod = $Env:SmtpDeliveryMethod }
	$appSettings.Smtp.Host           = $Env:SmtpNetworkHost
	$appSettings.Smtp.Port           = $Env:SmtpNetworkPort
	$appSettings.Smtp.UserName       = $Env:SmtpUserName
	$appSettings.Smtp.Password       = $Env:SmtpPassword
	if ($Env:SmtpEnableSsl) { $appSettings.Smtp.EnableSsl = $Env:SmtpEnableSsl }

	# Serilog
	$appSettings.Serilog.MinimumLevel.Default = $Env:LogLevel

	# AppSettings
	$appSettings.AppSettings.SecurityKeyString                    = $Env:SecurityKeyString
	$appSettings.AppSettings.BaseUrl                              = "https://$Env:KitosHostName/"
	$appSettings.AppSettings.MailSuffix                           = $Env:MailSuffix
	$appSettings.AppSettings.Environment                          = $Env:KitosEnvName
	$appSettings.AppSettings.DeploymentVersion                    = $Env:BUILD_NUMBER
	$appSettings.AppSettings.ResetPasswordTTL                     = $Env:ResetPasswordTtl
	$appSettings.AppSettings.DefaultUserPassword                  = $Env:DefaultUserPassword
	$appSettings.AppSettings.UseDefaultUserPassword               = $Env:UseDefaultUserPassword
	$appSettings.AppSettings.SsoServiceProviderId                 = $Env:SsoServiceProviderId
	$appSettings.AppSettings.SsoCertificateThumbprint             = $Env:SsoCertificateThumbPrint
	$appSettings.AppSettings.StsIssuer                            = $Env:StsIssuer
	$appSettings.AppSettings.StsCertificateEndpoint               = $Env:StsCertificateEndpoint
	$appSettings.AppSettings.ServiceCertificateAliasOrg           = $Env:ServiceCertificateAliasOrg
	$appSettings.AppSettings.StsCertificateAlias                  = $Env:StsCertificateAlias
	$appSettings.AppSettings.StsCertificateThumbprint             = $Env:StsCertificateThumbprint
	$appSettings.AppSettings.OrgService6EntityId                  = $Env:OrgService6EntityId
	$appSettings.AppSettings.StsAdressePort                       = $Env:StsAdressePort
	$appSettings.AppSettings.StsBrugerPort                        = $Env:StsBrugerPort
	$appSettings.AppSettings.StsPersonPort                        = $Env:StsPersonPort
	$appSettings.AppSettings.StsVirksomhedPort                    = $Env:StsVirksomhedPort
	$appSettings.AppSettings.StsOrganisationPort                  = $Env:StsOrganisationPort
	$appSettings.AppSettings.StsOrganisationSystemPort            = $Env:StsOrganisationSystemPort
	$appSettings.AppSettings.StsOrganisationCertificateThumbprint = $Env:StsOrganisationCertificateThumbprint
	$appSettings.AppSettings.PubSubBaseUrl                        = $Env:PubSubBaseUrl

	$appSettings | ConvertTo-Json -Depth 10 | Set-Content $appSettingsPath -Encoding UTF8

	$stampedAppSettings = Get-Content $appSettingsPath -Raw | ConvertFrom-Json
	Write-Host "Stamped appsettings Database.Provider='$($stampedAppSettings.Database.Provider)' Hangfire.Provider='$($stampedAppSettings.Hangfire.Provider)'"

	Write-Host "Update of appsettings.json complete"

	# Keep SAML configuration in sync in both config files, but use App.config as the source for
	# Presentation.Web.dll.config. This matches local builds where MSBuild emits
	# Presentation.Web.dll.config from App.config, and avoids copying IIS-only sections from
	# Web.config into the ConfigurationManager-consumed config file.
	Update-SamlConfigFile ".\TEMP_PresentationWeb\App.config"
	Update-SamlConfigFile ".\TEMP_PresentationWeb\Web.config"
	Copy-Item ".\TEMP_PresentationWeb\App.config" ".\TEMP_PresentationWeb\Presentation.Web.dll.config" -Force
	Write-Host "Copied App.config → Presentation.Web.dll.config"

	# Handle environment-specific robots file (was previously done via msdeploy -replace)
	$wwwroot = ".\TEMP_PresentationWeb\wwwroot"
	$robotsSource = Join-Path $wwwroot $Env:robots
	if (Test-Path $robotsSource) {
		Copy-Item $robotsSource (Join-Path $wwwroot "Robots.txt") -Force
		Write-Host "Robots.txt set from $($Env:robots)"
	}

	Compress-Archive -Path ".\TEMP_PresentationWeb\*" -DestinationPath "$pathToArchive"
	Write-Host "Zipping file back complete"

	Remove-Item -Path ".\TEMP_PresentationWeb" -Recurse -Force
	Write-Host "Clean up done"
}