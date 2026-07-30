#!/bin/sh
set -eu

# ── Wait for required services ────────────────────────────────────
# WAIT_FOR_SERVICES is a space-separated list of "host:port" pairs.
# Example: "redis:6379 postgres:5432"
if [ -n "${WAIT_FOR_SERVICES:-}" ]; then
    for service in $WAIT_FOR_SERVICES; do
        /app/wait-for-it.sh "$service" -t 60
    done
fi

# ── Wait for PostgreSQL from connection string (optional) ─────────
# Extracts host:port from ConnectionStrings__PostgresConnection
# if WAIT_FOR_POSTGRES is set to "true".
if [ "${WAIT_FOR_POSTGRES:-false}" = "true" ] && [ -n "${ConnectionStrings__PostgresConnection:-}" ]; then
    pg_host=$(echo "$ConnectionStrings__PostgresConnection" | sed -n 's/.*[Hh]ost=\([^;]*\).*/\1/p')
    if [ -z "$pg_host" ]; then
        pg_host=$(echo "$ConnectionStrings__PostgresConnection" | sed -n 's/.*[Ss]erver=\([^;]*\).*/\1/p')
    fi
    pg_port=$(echo "$ConnectionStrings__PostgresConnection" | sed -n 's/.*[Pp]ort=\([^;]*\).*/\1/p')
    pg_port="${pg_port:-5432}"

    if [ -n "$pg_host" ]; then
        /app/wait-for-it.sh "${pg_host}:${pg_port}" -t 60
    fi
fi

# ── Execute the application ───────────────────────────────────────
exec ./Turnero "$@"
