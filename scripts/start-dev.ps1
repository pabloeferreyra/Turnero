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

# Load environment variables from .env
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

Write-Host "🚀 Iniciando servicios locales..." -ForegroundColor Cyan

# ============================================================
# Redis: Check if running, otherwise start via Docker
# ============================================================
$redisStarted = $false

# Check if Redis is already running locally
$redisRunning = $false
try {
    $ping = redis-cli ping 2>&1
    if ($ping -eq "PONG") {
        $redisRunning = $true
        Write-Host "✅ Redis ya está corriendo localmente" -ForegroundColor Green
    }
} catch {
    $redisRunning = $false
}

if (-not $redisRunning) {
    # Check if Docker is available
    $dockerAvailable = $false
    try {
        $dockerVersion = docker --version 2>&1
        if ($LASTEXITCODE -eq 0) {
            $dockerAvailable = $true
        }
    } catch {
        $dockerAvailable = $false
    }

    if ($dockerAvailable) {
        Write-Host "📦 Redis no encontrado, iniciando contenedor Docker..." -ForegroundColor Yellow

        # Check if container already exists
        $existing = docker ps -a --filter "name=turnero-redis-dev" --format "{{.Names}}" 2>&1
        if ($existing -match "turnero-redis-dev") {
            $status = docker ps --filter "name=turnero-redis-dev" --format "{{.Status}}" 2>&1
            if ($status -match "^Up") {
                Write-Host "✅ Contenedor turnero-redis-dev ya está corriendo" -ForegroundColor Green
                $redisRunning = $true
            } else {
                Write-Host "🔄 Iniciando contenedor existente turnero-redis-dev..." -ForegroundColor Yellow
                docker start turnero-redis-dev 2>&1 | Out-Null
                $redisRunning = $true
            }
        } else {
            Write-Host "🔄 Creando contenedor Redis (redis:7-alpine)..." -ForegroundColor Yellow
            docker run -d --name turnero-redis-dev -p 6379:6379 redis:7-alpine 2>&1 | Out-Null
            $redisStarted = $true
            $redisRunning = $true
        }

        # Wait for Redis to be ready
        if ($redisRunning) {
            Write-Host "⏳ Esperando a que Redis esté listo..." -ForegroundColor Yellow
            $ready = $false
            for ($i = 0; $i -lt 15; $i++) {
                try {
                    $ping = redis-cli ping 2>&1
                    if ($ping -eq "PONG") {
                        $ready = $true
                        break
                    }
                } catch {}
                Start-Sleep -Seconds 1
            }
            if (-not $ready) {
                # Try pinging via docker
                try {
                    $dockerPing = docker exec turnero-redis-dev redis-cli ping 2>&1
                    if ($dockerPing -eq "PONG") {
                        $ready = $true
                    }
                } catch {}
            }
            if ($ready) {
                Write-Host "✅ Redis listo en localhost:6379" -ForegroundColor Green
            } else {
                Write-Warning "⚠️  Redis no responde, la app usará solo cache en memoria (IMemoryCache)"
            }
        }
    } else {
        Write-Warning "⚠️  Docker no está disponible. Redis no se iniciará. La app usará solo cache en memoria (IMemoryCache)."
    }
}

# ============================================================
# Start .NET application
# ============================================================
Write-Host "🟢 Iniciando aplicación..." -ForegroundColor Cyan

Push-Location $root
try {
    dotnet run --project "Turnero.csproj"
}
finally {
    Pop-Location

    # Cleanup: stop Redis container if we started it
    if ($redisStarted) {
        Write-Host "🧹 Deteniendo Redis (contenedor: turnero-redis-dev)..." -ForegroundColor Yellow
        try {
            docker stop turnero-redis-dev 2>&1 | Out-Null
            docker rm turnero-redis-dev 2>&1 | Out-Null
            Write-Host "✅ Redis detenido y eliminado" -ForegroundColor Green
        } catch {
            Write-Warning "⚠️  No se pudo detener Redis automáticamente. Ejecuta: docker stop turnero-redis-dev"
        }
    }
}
