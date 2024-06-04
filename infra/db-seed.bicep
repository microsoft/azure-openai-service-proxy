param name string
param location string

param postgresDatabaseName string
param adminSystemAssignedIdentity string
param proxySystemAssignedIdentity string

@description('Entra admin role name')
param entraAdministratorName string = ''

@secure()
param entraAuthorizationToken string

param postgresServerName string

resource postgresServer 'Microsoft.DBforPostgreSQL/flexibleServers@2022-12-01' existing = {
  name: postgresServerName
}

resource sqlDeploymentScriptSetup 'Microsoft.Resources/deploymentScripts@2020-10-01' = {
  name: '${name}-deployment-script-setup'
  dependsOn: [
    postgresServer
  ]
  location: location
  kind: 'AzureCLI'
  properties: {
    azCliVersion: '2.37.0'
    retentionInterval: 'PT1H' // Retain the script resource for 1 hour after it ends running
    timeout: 'PT5M' // Five minutes
    cleanupPreference: 'OnSuccess'
    environmentVariables: [
      {
        name: 'SQL_SETUP_SCRIPT'
        value: loadTextContent('../database/setup.sql')
      }
      {
        name: 'SQL_SCRIPT'
        value: loadTextContent('../database/aoai-proxy.sql')
      }
      {
        name: 'PG_USER'
        value: entraAdministratorName
      }
      {
        name: 'PG_DB'
        value: postgresDatabaseName
      }
      {
        name: 'PG_HOST'
        value: postgresServer.properties.fullyQualifiedDomainName
      }
      {
        name: 'PGPASSWORD'
        value: entraAuthorizationToken
      }
      {
        name: 'ADMIN_SYSTEM_ASSIGNED_IDENTITY'
        value: adminSystemAssignedIdentity
      }
      {
        name: 'PROXY_SYSTEM_ASSIGNED_IDENTITY'
        value: proxySystemAssignedIdentity
      }
    ]

    scriptContent: '''
      #!/bin/bash

      apk add postgresql-client

      echo "Executing permissions setup script starting"
      echo "$SQL_SETUP_SCRIPT" > ./setup.sql
      cat ./setup.sql

      psql -a -U "$PG_USER" -d "postgres" -h "$PG_HOST" -v PG_USER="$PG_USER" -v ADMIN_SYSTEM_ASSIGNED_IDENTITY="$ADMIN_SYSTEM_ASSIGNED_IDENTITY" -v PROXY_SYSTEM_ASSIGNED_IDENTITY="$PROXY_SYSTEM_ASSIGNED_IDENTITY" -w -f ./setup.sql
      echo "Executing permissions setup script ended"

      echo "Executing database setup script starting"
      echo "$SQL_SCRIPT" > ./setup.sql
      cat ./setup.sql

      psql -a -U "$PG_USER" -d "aoai-proxy" -h "$PG_HOST" -w -f ./setup.sql
      echo "Executing database setup script ended"

      echo "Creating database schema aoai with permissions"
      psql -a -U "$PG_USER" -d "aoai-proxy" -h "$PG_HOST" -c "REVOKE ALL ON ALL TABLES IN SCHEMA aoai FROM aoai_proxy_app; GRANT DELETE, INSERT, UPDATE, SELECT ON ALL TABLES IN SCHEMA aoai TO aoai_proxy_app; GRANT ALL ON SCHEMA aoai TO azure_pg_admin; GRANT USAGE ON SCHEMA aoai TO azuresu;"

    '''
  }
}
