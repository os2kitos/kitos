Function Prepare-Package([String] $environmentName, $packageDirectory) {

    Write-Host "Environment is $environmentName"

	if($environmentName -eq "integration"){

        Write-Host "Unzipping $packageDirectory to is TEMP_PresentationWeb"

        Expand-Archive -Path $packageDirectory -DestinationPath .\TEMP_PresentationWeb

        Remove-Item -Path $packageDirectory

        Write-Host "Updating Web.config"

        $filePathToTask = (Resolve-Path ".\TEMP_PresentationWeb\Content\C_C\kitos_tmp\app\Web.config")
        $xml = New-Object XML
        $xml.Load($filePathToTask)
        $element =  $xml.SelectSingleNode("//mailSettings")

        $specifiedPickupDirectoryChildNode = $xml.CreateElement("specifiedPickupDirectory")
        $specifiedPickupDirectoryChildNode.SetAttribute("pickupDirectoryLocation", "c:\temp\maildrop\")

        $smtpChildNode = $xml.CreateElement("smtp")
        $smtpChildNode.SetAttribute("from", "noreply@kitos.dk")
        $smtpChildNode.SetAttribute("deliveryMethod", "SpecifiedPickupDirectory")
        $smtpChildNode.AppendChild($specifiedPickupDirectoryChildNode)

        # Removes all childs and attributes of element mailSettings currently dont have attributes but if they are added this needs to be changed
        $element.RemoveAll()
        $element.AppendChild($smtpChildNode)

        $xml.Save($filePathToTask)

        Write-Host "Update of Web.config complete"
        
        Compress-Archive -Path (Resolve-Path ".\TEMP_PresentationWeb\*") -CompressionLevel NoCompression -DestinationPath $packageDirectory

        Remove-Item -Path (Resolve-Path ".\TEMP_PresentationWeb") -Recurse -Force      
        
        Write-Host "Zipping file back complete" 
	}

}