###############################################
## Parameters
###############################################

# Check if versions are passed as parameter
if($args[0] -Eq $null -Or $args[0] -Eq ''){
	Print ('Version can t be null!', '-foregroundcolor red')
	exit
}

if($args[1] -Eq $null -Or $args[1] -Eq ''){
	Print ('Myth version can t be null!', '-foregroundcolor red')
	exit
}

if($args[2] -Eq $null -Or $args[2] -Eq ''){
	Print ('Name can t be null!', '-foregroundcolor red')
	exit
}

if($args[3] -Eq $null -Or $args[3] -Eq ''){
	Print ('Projects can t be null!', '-foregroundcolor red')
	exit
}

if($args[4] -Eq $null -Or $args[4] -Eq ''){
	Print ('Path can t be null!', '-foregroundcolor red')
	exit
}

## Set specified settings
$version = $args[0]
$mythVersion = $args[1]
$name = $args[2]
$projects = $args[3]
$currentPath = $args[4]
$ignore = $args[5]

###############################################
## Functions
###############################################

# Pause and wait for input to continue
Function Pause{
	Write-Host -NoNewLine 'Press any key to continue...';
	$null = $Host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown');
}

# Replace version matched to regex to the new version
Function Replace-Version {
	Param (
		$newVersion,
		$oldVersion,
		$line,
		$content
	)
	
	$temp = $line	
	$temp = $temp -replace $oldversion, $newVersion	
	$content = $content -replace $line, $temp
		
	return $content
}

# Build and create Nuget pack
Function Build-And-Pack{
	Param(
		$type
	)
		
	$current = $currentPath
	
	dotnet build "`"$($current)\$($file)`"" -v quiet -c $type
	dotnet pack $file -o $ENV:UserProfile\AppData\Roaming\NuGet\Packages -v quiet -c $type
}

# Delete files in informed path
Function Remove-Cached-Files{
	Param(
		$path,
		$isDirectory
	)
	
	$directory = "Get-ChildItem "
	if($isDirectory -eq 'true'){
		$directory = $directory + " -Directory "
	}
	
	$basePath = $ENV:UserProfile
	$path = $basePath + $path
	
	$directories = Invoke-Expression( "$($directory)" + "-Filter '$($project)*' " + '-Path "$($path)"' )

	foreach($directory in $directories){
		if( $ignore.count -Eq 0 -Or $directory -NotMatch ($ignore -join '|')){
			Remove-Item $directory.FullName -Recurse
		}
	}
}

# Write in console the informed message
Function Print{
	Param(
		$message,
		$color
	)
	
	Invoke-Expression( 'Write-Host ' + '-------------------------------------------------------------------------------' + $color)
	Invoke-Expression( 'Write-Host ' + $message + $color)
	Invoke-Expression( 'Write-Host ' + '-------------------------------------------------------------------------------' + $color)
	Write-Host ""
}

###############################################
# Main
###############################################

# Define regex patterns
$versionRegex = '<Version>(.*)<\/Version>'
$mythRegex = 'Myth.*Version="(.*)"'
$harpyRegex = 'Harpy.*Version="(.*)"'

# Set variables
$current = $currentPath
$project = $name.ToLower()

Print('UPDATING TO VERSION: $($version)...', '-foregroundcolor blue')

# Get all *.csproj files
$files = Get-ChildItem -Path $current -Include *.csproj -Recurse | % { $_.FullName }	

foreach($file in $files) {
	$content = (Get-Content -Path $file)		
	
	foreach($line in $content) {
		
		# Replace in csproj the Myth version founded
		if($line -match $mythRegex){
			$content = Replace-Version -newVersion $mythVersion -oldVersion $matches[1] -line $line -content $content
		}
		
		# Replace in csproj the version of project and harpy versions
		if($line -match $versionRegex -Or $line -match $harpyRegex){
			$content = Replace-Version -newVersion $version -oldVersion $matches[1] -line $line -content $content
		}
	}		
	
	# Save file
	Set-Content -Path $file -Value ($content)
}
Print("$($name) packages changed to the new version $($version) in all procjects", '-foregroundcolor green')

# Remove all myth packages already generated
Remove-Cached-Files -path '\AppData\Roaming\NuGet\Packages' -isDirectory 'false'
Print("$($name) packages cleaned from local Nuget Packages", '-foregroundcolor green')

# Remove all myth packages cached local
Remove-Cached-Files -path '\.nuget\packages' -isDirectory 'true'
Print("$($name) packages cleaned from cached Nuget Packages", '-foregroundcolor green')

# Build and pack projects
foreach($file in $projects) {
	$buildType = 'Release'
	if($version -Like '*stage*'){
		$buildType = 'Debug'
	}
	
	Print("Starting building package $($file) as $($buildType)...", '-foregroundcolor blue')
	Build-And-Pack($buildType)
}	
Print ("$($name) packages cleaned and generated to the new version $($version) with success", '-foregroundcolor green')


