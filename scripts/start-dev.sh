#!/usr/bin/env bash
set -euo pipefail

ENV_FILE="${1:-.env}"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "${SCRIPT_DIR}/.." && pwd)"
ENV_PATH="${ROOT_DIR}/${ENV_FILE}"

if [ ! -f "${ENV_PATH}" ]; then
  echo "No se encontro el archivo de entorno: ${ENV_PATH}. Copia .env.example a .env y completa los valores." >&2
  exit 1
fi

# Load environment variables from .env
while IFS= read -r line || [ -n "$line" ]; do
  line="${line%$'\r'}"

  if [ -z "${line// }" ]; then
    continue
  fi

  if [[ "${line}" == \#* ]]; then
    continue
  fi

  if [[ "${line}" != *=* ]]; then
    continue
  fi

  name="${line%%=*}"
  value="${line#*=}"

  name="${name## }"
  name="${name%% }"

  if [ -n "${name}" ]; then
    export "${name}=${value}"
  fi
done < "${ENV_PATH}"

echo "🚀 Iniciando servicios locales..."

# ============================================================
# Redis: Check if running, otherwise start via Docker
# ============================================================
REDIS_STARTED=false
REDIS_RUNNING=false

# Check if Redis is already running locally
if command -v redis-cli &> /dev/null; then
  if redis-cli ping 2>/dev/null | grep -q "PONG"; then
    REDIS_RUNNING=true
    echo "✅ Redis ya está corriendo localmente"
  fi
fi

if [ "$REDIS_RUNNING" = false ]; then
  # Check if Docker is available
  if command -v docker &> /dev/null; then
    echo "📦 Redis no encontrado, iniciando contenedor Docker..."

    # Check if container already exists
    if docker ps -a --filter "name=turnero-redis-dev" --format "{{.Names}}" 2>/dev/null | grep -q "turnero-redis-dev"; then
      STATUS=$(docker ps --filter "name=turnero-redis-dev" --format "{{.Status}}" 2>/dev/null)
      if echo "$STATUS" | grep -q "^Up"; then
        echo "✅ Contenedor turnero-redis-dev ya está corriendo"
        REDIS_RUNNING=true
      else
        echo "🔄 Iniciando contenedor existente turnero-redis-dev..."
        docker start turnero-redis-dev > /dev/null 2>&1
        REDIS_RUNNING=true
      fi
    else
      echo "🔄 Creando contenedor Redis (redis:7-alpine)..."
      docker run -d --name turnero-redis-dev -p 6379:6379 redis:7-alpine > /dev/null 2>&1
      REDIS_STARTED=true
      REDIS_RUNNING=true
    fi

    # Wait for Redis to be ready
    if [ "$REDIS_RUNNING" = true ]; then
      echo "⏳ Esperando a que Redis esté listo..."
      READY=false
      for i in $(seq 1 15); do
        if redis-cli ping 2>/dev/null | grep -q "PONG"; then
          READY=true
          break
        fi
        # Try pinging via docker directly
        if docker exec turnero-redis-dev redis-cli ping 2>/dev/null | grep -q "PONG"; then
          READY=true
          break
        fi
        sleep 1
      done
      if [ "$READY" = true ]; then
        echo "✅ Redis listo en localhost:6379"
      else
        echo "⚠️  Redis no responde, la app usará solo cache en memoria (IMemoryCache)" >&2
      fi
    fi
  else
    echo "⚠️  Docker no está disponible. Redis no se iniciará. La app usará solo cache en memoria (IMemoryCache)." >&2
  fi
fi

# ============================================================
# Start .NET application
# ============================================================
echo "🟢 Iniciando aplicación..."
cd "${ROOT_DIR}"

# Trap to cleanup Redis on exit
cleanup() {
  if [ "$REDIS_STARTED" = true ]; then
    echo "🧹 Deteniendo Redis (contenedor: turnero-redis-dev)..."
    docker stop turnero-redis-dev > /dev/null 2>&1
    docker rm turnero-redis-dev > /dev/null 2>&1
    echo "✅ Redis detenido y eliminado"
  fi
}
trap cleanup EXIT

dotnet run --project "Turnero.csproj"
