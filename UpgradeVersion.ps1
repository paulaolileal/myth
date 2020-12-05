if($args[0] -ne $null -And $args[0] -ne ''){

	$version = $args[0]
	$current = $PSScriptRoot # Get the current script path

	# Replace all the versions of Harpy reference to the new one
	$files = Get-ChildItem -Path $current -Include *.csproj -Recurse | % { $_.FullName }	
	foreach($file in $files) {
		$content = (Get-Content -Path $file -Raw)		
		
		$vFoundReferences = $content -match 'Myth.*Version="(.*)"'
		if($vFoundReferences){
			$content = $content -Replace $matches[1], $version
		}
		
		$vFoundVersion = $content -match '<Version>(.*)<\/Version>'
		if($vFoundVersion){
			$content = $content -Replace $matches[1], $version
		}
		
		Set-Content -Path $file -Value ($content)
	}
	
	# Remove all harpy packages generated
	$packages =  Get-ChildItem -Filter "myth*" -Path $ENV:UserProfile\AppData\Roaming\NuGet\Packages
	foreach($pack in $packages){
		Remove-Item $pack.FullName -Recurse
	}

	# Remove all harpy packages cached
	$harpyFolders = Get-ChildItem -Directory -Filter "myth*" -Path $ENV:UserProfile\.nuget\packages
	foreach($folder in $harpyFolders){
		Remove-Item $folder.FullName -Recurse
	}
	
	$projects = @(	
		"$($current)\Myth.Commons\Myth.csproj")
	
	foreach($file in $projects) {
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