$version = $args[0]
$mythVersion = $version

if($args[1] -ne $null -And $args[1] -ne ''){
	$mythVersion = $args[1]
}

Write-Host "Starting Myth packages..." -fore DarkYellow
cd myth-commons
& .\Set-Myth-Version.ps1 $version $mythVersion

Write-Host "Starting Harpy packages..." -fore DarkYellow
cd ..\harpy
& .\Set-Harpy-Version.ps1 $version $mythVersion

Write-Host "Starting SQLite package..." -fore DarkYellow
cd ..\harpy-sqlite
& .\Set-Harpy-Version.ps1 $version $mythVersion

Write-Host "Starting Oracle package..." -fore DarkYellow
cd ..\harpy-oracle
& .\Set-Harpy-Version.ps1 $version $mythVersion

Write-Host "Starting LiteDB package..." -fore DarkYellow
cd ..\harpy-litedb
& .\Set-Harpy-Version.ps1 $version $mythVersion

Write-Host "Starting PostgreSQL package..." -fore DarkYellow
cd ..\harpy-postgresql
& .\Set-Harpy-Version.ps1 $version $mythVersion

Write-Host "Starting CLI package..." -fore DarkYellow
cd ..\harpy-cli
& .\Set-Harpy-Version.ps1 $version $mythVersion

cd ..

Write-Host "Myth packages changed with success!" -fore green
