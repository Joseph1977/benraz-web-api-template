Param (
	[Parameter(Mandatory = $true)][string] $ProjectName
)

Get-ChildItem -Recurse -Filter "*_MicroserviceTemplate_*" | % { Rename-Item -Path $_.PSPath -NewName $_.Name.replace("_MicroserviceTemplate_",$ProjectName) }
Get-ChildItem -Recurse -File -Exclude "Rename.ps1" | % { ((Get-Content -Path $_.PSPath -Raw) -Replace "_MicroserviceTemplate_",$ProjectName) | Set-Content -Path $_.PSPath }