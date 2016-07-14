# in msbuild <Exec Command="powershell -Command &quot;&amp;{ .\Deploy.ps1 scriptArg1 scriptArg2 }&quot;" />
# ps cmd line: ./GetPackages.ps1 -packagePath "C:\os2kitos\b1\packages" -packageName "GitVersionCore.dll"

Param([cmdletbinding()]$packagePath, [cmdletbinding()]$packageName)

function Get-PackagePath
{   
	Param($packagePath, $packageName)
	Get-ChildItem $packagePath\*\$packageName -Recurse | Format-Table FullName
}

Get-PackagePath -packagePath $packagePath -packageName $packageName
