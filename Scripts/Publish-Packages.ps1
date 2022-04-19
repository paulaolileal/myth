$path = "$ENV:UserProfile\AppData\Roaming\NuGet\Packages"
$files = Get-ChildItem -Path $path -Name -Exclude *dev* -Include *.nupkg -ErrorAction SilentlyContinue | Sort-Object
$key = 'oy2j4fxvuo7bjhqj5ngqe3rjy62jhqzuvjlws4f4gvtdyi'
foreach($file in $files){
	$package = "$($path)\$($file)"
	Write-Host "Publicando pacote: $($file)" -fore blue
	dotnet nuget push $package -k $key -s https://api.nuget.org/v3/index.json --skip-duplicate
	Remove-Item $package
}
