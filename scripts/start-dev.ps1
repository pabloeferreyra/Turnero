Param(
    [string]$EnvFile = ".env"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$root = Resolve-Path (Join-Path $PSScriptRoot "..")
$envPath = Join-Path $root $EnvFile

if (-not (Test-Path $envPath)) {
    Write-Error "No se encontro el archivo de entorno: $envPath. Copia .env.example a .env y completa los valores."
}

Get-Content $envPath | ForEach-Object {
    $line = $_.Trim()

    if ([string]::IsNullOrWhiteSpace($line)) { return }
    if ($line.StartsWith("#")) { return }

    $pair = $line.Split("=", 2)
    if ($pair.Count -ne 2) { return }

    $name = $pair[0].Trim()
    $value = $pair[1]

    if (-not [string]::IsNullOrWhiteSpace($name)) {
        [System.Environment]::SetEnvironmentVariable($name, $value, "Process")
    }
}

Push-Location $root
try {
    dotnet run --project "Turnero.csproj"
}
finally {
    Pop-Location
}
