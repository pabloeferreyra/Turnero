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

cd "${ROOT_DIR}"
dotnet run --project "Turnero.csproj"
