-- Postgres database setup script
-- This script creates the aoai-proxy db and sets up the roles and permissions for the application

CREATE DATABASE "aoai-proxy" WITH OWNER azure_pg_admin;
CREATE ROLE aoai_proxy_app WITH NOLOGIN NOSUPERUSER INHERIT NOCREATEDB NOCREATEROLE NOREPLICATION NOBYPASSRLS;
CREATE ROLE aoai_proxy_reporting WITH NOLOGIN NOSUPERUSER INHERIT NOCREATEDB NOCREATEROLE NOREPLICATION NOBYPASSRLS;

GRANT aoai_proxy_app TO :"PG_USER";
GRANT aoai_proxy_reporting TO :"PG_USER";

select * from pgaadauth_create_principal(:'ADMIN_SYSTEM_ASSIGNED_IDENTITY', false, false);
select * from pgaadauth_create_principal(:'PROXY_SYSTEM_ASSIGNED_IDENTITY', false, false);
GRANT aoai_proxy_app TO :"PROXY_SYSTEM_ASSIGNED_IDENTITY";
GRANT aoai_proxy_app TO :"ADMIN_SYSTEM_ASSIGNED_IDENTITY";
