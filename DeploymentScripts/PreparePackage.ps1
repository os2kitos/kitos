Function Prepare-Package([String] $environmentName, $pathToArchive) {

    Write-Host "Environment is $environmentName"

	if($environmentName -eq "integration"){

        Write-Host "Unzipping $pathToArchive to is TEMP_PresentationWeb"

        Expand-Archive -Path $pathToArchive -DestinationPath .\TEMP_PresentationWeb

        Remove-Item -Path $pathToArchive

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

        $7zipPath = "C:\Program Files\7-Zip\7z.exe"

        if (-not (Test-Path -Path $7zipPath -PathType Leaf)) {
            throw "7 zip file '$7zipPath' not found"
        }

        Set-Alias 7zip $7zipPath

        $Source = (Resolve-Path ".\TEMP_PresentationWeb\*")
        $Target = $pathToArchive

        7zip a -mx=9 $Target $Source

        Write-Host "Zipping file back complete"
        
        Remove-Item -Path (Resolve-Path ".\TEMP_PresentationWeb") -Recurse -Force      
        
        Write-Host "Clean up done"
	}

}