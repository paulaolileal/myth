if($args[0] -ne $null -And $args[0] -ne ''){

	$version = $args[0]
	$mythVersion = $version
	$current = $PSScriptRoot # Get the current script path

	if($args[1] -ne $null -And $args[1] -ne ''){
		$mythVersion = $args[1]
	}

	$versionRegex = '<Version>(.*)<\/Version>'
	$mythRegex = 'Myth.*Version="(.*)"'

	# Replace all the versions of Myth reference to the new one
	$files = Get-ChildItem -Path $current -Include *.csproj -Recurse | % { $_.FullName }	
	
	foreach($file in $files) {
		$content = (Get-Content -Path $file)		
		
		foreach($line in $content) {
			if($line -match $mythRegex){
				$temp = $line
				$temp = $temp -Replace $matches[1], $mythVersion
				$content = $content -Replace $line, $temp
			}
			
			if($line -match $versionRegex){
				$temp = $line
				$temp = $temp -Replace $matches[1], $version
				$content = $content -Replace $line, $temp
			}
		}		
		
		Set-Content -Path $file -Value ($content.Trim())
	}
	Write-Host "Myth packages set to $($version) in all procjects" -fore green
	
	# Remove all myth packages generated
	$packages =  Get-ChildItem -Filter "myth*" -Path $ENV:UserProfile\AppData\Roaming\NuGet\Packages
	foreach($pack in $packages){
		Remove-Item $pack.FullName -Recurse
	}
	Write-Host "Myth packages cleaned from local Nuget Packages" -fore green

	# Remove all myth packages cached
	$harpyFolders = Get-ChildItem -Directory -Filter "myth*" -Path $ENV:UserProfile\.nuget\packages
	foreach($folder in $harpyFolders){
		Remove-Item $folder.FullName -Recurse
	}
	Write-Host "Myth packages cleaned from cached Nuget Packages" -fore green
	
	$projects = @(	
		"$($current)\Myth.Commons\Myth.Commons.csproj",
		"$($current)\Myth.Specification\Myth.Specification.csproj",		
		"$($current)\Myth.Repository\Myth.Repository.csproj",		
		"$($current)\Myth.Repository.EntityFramework\Myth.Repository.EntityFramework.csproj",		
		"$($current)\Myth.Rest\Myth.Rest.csproj",		
		"$($current)\Myth.Odata\Myth.Odata.csproj"	
		)
	
	foreach($file in $projects) {
		Write-Host "Starting build package $($file)..." -fore blue
		
		if($version -Like '*stage*'){
			dotnet build $file -v quiet -c Debug 
			dotnet pack $file -o $ENV:UserProfile\AppData\Roaming\NuGet\Packages -v quiet -c Debug
		}else{
			dotnet build $file -v quiet -c Release
			dotnet pack $file -o $ENV:UserProfile\AppData\Roaming\NuGet\Packages -v quiet -c Release
		}
	}
	
	Write-Host "Myth packages cleaned and generated to the new version $($version) with success" -fore green
	
}else{
	Write-Host 'Version can t be null!' -fore red
	exit
}