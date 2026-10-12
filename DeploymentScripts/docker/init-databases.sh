#!/bin/bash
set -e

# This script is mounted into the PostgreSQL container as an init script.
# It creates the required databases and a shared application user.

POSTGRES_USER="${POSTGRES_USER:-postgres}"
APP_USER="${APP_USER:-kitos}"
APP_PASSWORD="${APP_PASSWORD:-kitos}"

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "postgres" <<-EOSQL
    -- Create application user
    DO \$\$
    BEGIN
        IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = '${APP_USER}') THEN
            -- CREATEDB is required by local reset/migration scripts that recreate databases.
            CREATE ROLE ${APP_USER} WITH LOGIN PASSWORD '${APP_PASSWORD}' CREATEDB;
        END IF;
    END
    \$\$;

    -- Create databases
    SELECT 'CREATE DATABASE kitos OWNER ${APP_USER}'
        WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'kitos')\gexec

    SELECT 'CREATE DATABASE kitos_hangfiredb OWNER ${APP_USER}'
        WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'kitos_hangfiredb')\gexec

    SELECT 'CREATE DATABASE kitos_pubsub OWNER ${APP_USER}'
        WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'kitos_pubsub')\gexec
EOSQL
