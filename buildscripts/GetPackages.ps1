# in msbuild <Exec Command="powershell -Command &quot;&amp;{ .\Deploy.ps1 scriptArg1 scriptArg2 }&quot;" />

Param($packagePath, $packageName)

Function GetPackagePath
{
	Param($packagePath, $packageName)
	Get-ChildItem $packagePath\*\$packageName -Recurse | Format-Table FullName
}