#!/bin/sh
set -eu

# Creates Keycloak's DB user + database on the same Postgres instance as the app.
# Runs only when the data volume is first initialized (see postgres image docs).

psql -v ON_ERROR_STOP=1 \
  --username "$POSTGRES_USER" \
  --dbname "$POSTGRES_DB" \
  -v kc_user="$KEYCLOAK_DB_USER" \
  -v kc_pass="$KEYCLOAK_DB_PASSWORD" \
  -v kc_db="$KEYCLOAK_DB_NAME" <<'EOSQL'
CREATE ROLE :"kc_user" WITH LOGIN PASSWORD :'kc_pass';
CREATE DATABASE :"kc_db" OWNER :"kc_user";
EOSQL
