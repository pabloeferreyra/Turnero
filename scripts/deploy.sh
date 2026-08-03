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
STACK_NAME="${STACK_NAME:-turnero}"
STACK_FILE="${STACK_FILE:-docker-stack.prod.yml}"
IMAGE_REPO="${IMAGE_REPO:-turnero-app}"

if ! command -v docker >/dev/null 2>&1; then
  echo "Error: docker is not installed or not available in PATH." >&2
  exit 1
fi

if [ ! -f "${STACK_FILE}" ]; then
  echo "Error: stack file not found: ${STACK_FILE}" >&2
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

SWARM_STATE="$(docker info --format '{{.Swarm.LocalNodeState}}' 2>/dev/null || true)"
if [ "${SWARM_STATE}" != "active" ]; then
  echo "Error: Docker Swarm is not active. Run: docker swarm init" >&2
  exit 1
fi

IMAGE_TAG="${IMAGE_REPO}:${VERSION}"

echo "[1/4] Building image ${IMAGE_TAG}"
docker build -t "${IMAGE_TAG}" .

echo "[2/4] Exporting deploy variables"
export TURNERO_IMAGE="${IMAGE_TAG}"
export FIREBASE_CREDENTIALS_FILE="${FIREBASE_FILE}"

echo "[3/4] Deploying stack ${STACK_NAME}"
docker stack deploy -c "${STACK_FILE}" "${STACK_NAME}"

echo "[4/4] Service status"
docker service ls | grep "${STACK_NAME}_turnero-app" || true
docker service ps "${STACK_NAME}_turnero-app" --no-trunc || true

echo "Deploy request sent. Rolling update is handled by Swarm (start-first + rollback)."
