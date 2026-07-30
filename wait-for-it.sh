#!/bin/sh
# wait-for-it.sh: Wait for a TCP service to become available before executing a command.
#
# Usage: wait-for-it.sh host:port [-t timeout] [-- command args...]
#   host:port    The TCP host and port to wait for
#   -t timeout   Timeout in seconds (default: 60)
#   -- command   Command to execute after the service is ready
#
# Example:
#   wait-for-it.sh redis:6379 -t 30 -- ./Turnero

set -eu

TIMEOUT=60
HOST=""
PORT=""
CMD=""

usage() {
    echo "Usage: $0 host:port [-t timeout] [-- command args...]"
    exit 1
}

# Parse arguments
while [ $# -gt 0 ]; do
    case "$1" in
        *:*)
            HOST="${1%:*}"
            PORT="${1##*:}"
            ;;
        -t)
            if [ $# -lt 2 ]; then
                echo "Error: -t requires a value"
                usage
            fi
            TIMEOUT="$2"
            shift
            ;;
        --)
            shift
            CMD="$@"
            break
            ;;
        -*)
            echo "Unknown option: $1"
            usage
            ;;
        *)
            echo "Unexpected argument: $1"
            usage
            ;;
    esac
    shift
done

if [ -z "$HOST" ] || [ -z "$PORT" ]; then
    echo "Error: host:port is required."
    usage
fi

if [ "$TIMEOUT" -le 0 ] 2>/dev/null; then
    echo "Error: timeout must be a positive integer."
    exit 1
fi

echo "wait-for-it: waiting for $HOST:$PORT (timeout: ${TIMEOUT}s)..."

END_TIME=$(( $(date +%s) + TIMEOUT ))

while [ "$(date +%s)" -lt "$END_TIME" ]; do
    if nc -z "$HOST" "$PORT" 2>/dev/null; then
        echo "wait-for-it: $HOST:$PORT is ready!"
        if [ -n "$CMD" ]; then
            exec $CMD
        fi
        exit 0
    fi
    sleep 2
done

echo "wait-for-it: timeout reached (${TIMEOUT}s) waiting for $HOST:$PORT"
exit 1
