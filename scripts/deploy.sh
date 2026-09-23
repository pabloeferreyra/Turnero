#!/usr/bin/env bash
set -eu
(set -o pipefail) 2>/dev/null && set -o pipefail

if [ $# -lt 1 ] || [ $# -gt 2 ]; then
  echo "Usage: ./scripts/deploy.sh <version> [firebase_credentials_file]" >&2
  echo "Example: ./scripts/deploy.sh v3.0.2 /opt/secrets/firebase.json" >&2
  exit 1
fi

VERSION="$1"
FIREBASE_FILE="${2:-${FIREBASE_CREDENTIALS_FILE:-}}"
COMPOSE_PROJECT_NAME="${COMPOSE_PROJECT_NAME:-turnero}"
COMPOSE_FILE="${COMPOSE_FILE:-docker-compose.prod.yml}"
IMAGE_REPO="${IMAGE_REPO:-turnero-app}"

if ! command -v podman >/dev/null 2>&1; then
  echo "Error: podman is not installed or not available in PATH." >&2
  exit 1
fi

if ! podman info >/dev/null 2>&1; then
  echo "Error: Podman is not available. Start Podman machine/service and retry." >&2
  exit 1
fi

if [ ! -f "${COMPOSE_FILE}" ]; then
  echo "Error: compose file not found: ${COMPOSE_FILE}" >&2
  exit 1
fi

if [ -z "${FIREBASE_FILE}" ]; then
  echo "Error: FIREBASE_CREDENTIALS_FILE is required (arg2 or env var)." >&2
  exit 1
fi

if [ ! -f "${FIREBASE_FILE}" ]; then
  echo "Error: firebase credentials file not found: ${FIREBASE_FILE}" >&2
  exit 1
fi

IMAGE_TAG="${IMAGE_REPO}:${VERSION}"

echo "[1/4] Building image ${IMAGE_TAG}"
podman build -t "${IMAGE_TAG}" .

echo "[2/4] Exporting deploy variables"
export TURNERO_IMAGE="${IMAGE_TAG}"
export FIREBASE_CREDENTIALS_FILE="${FIREBASE_FILE}"

echo "[3/4] Starting compose project ${COMPOSE_PROJECT_NAME}"
podman compose -p "${COMPOSE_PROJECT_NAME}" -f "${COMPOSE_FILE}" up -d --no-build --remove-orphans

echo "[4/4] Service status"
podman compose -p "${COMPOSE_PROJECT_NAME}" -f "${COMPOSE_FILE}" ps

echo "Deployment completed with Podman Compose."
