# Check if the parameter are informed
if($args[0] -Eq $null -Or $args[1] -Eq ''){
	Print ('Version can t be null!', '-foregroundcolor red')
	exit
}

# Version of the csproj and Harpy packages
$version = $args[0]

# Version of Myth packages
$mythVersion = $version

# Check if the Myth Version are informed (not required)
if($args[1] -ne $null -And $args[1] -ne ''){
	$mythVersion = $args[1]
}

# Name of the current project
$name = 'Myth'

# Projects to build (in order of dependency)
$projects = @(	
	"Myth.Commons\Myth.Commons.csproj",
	"Myth.Specification\Myth.Specification.csproj",		
	"Myth.Repository\Myth.Repository.csproj",		
	"Myth.Repository.EntityFramework\Myth.Repository.EntityFramework.csproj",		
	"Myth.Rest\Myth.Rest.csproj",		
	"Myth.Odata\Myth.Odata.csproj",	
	"Myth.DependencyInjection\Myth.DependencyInjection.csproj"	
)	

# Projects to ignore on delete cached packages
$ignore = @(

)

# Define the current path
$currentPath = $PSScriptRoot # Get the current script path

# Run the script
& ..\Set-Myth-Version.ps1 $version $mythVersion $name $projects $currentPath $ignore