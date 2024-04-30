param location string
param tags object

param postgresDatabaseName string
param name string
param authType string = 'Password'

param adminSystemAssignedIdentity string
param proxySystemAssignedIdentity string

@secure()
param entraAuthorizationToken string

@description('Entra admin role name')
param entraAdministratorName string = ''

@description('Entra admin role object ID (in Entra)')
param entraAdministratorObjectId string = ''

@description('Entra admin user type')
@allowed([
  'User'
  'Group'
  'ServicePrincipal'
])
param entraAdministratorType string = 'User'

// Create PostgreSQL database
module postgresServer 'core/database/postgresql/flexibleserver.bicep' = {
  name: 'postgresql'
  params: {
    name: name
    location: location
    tags: tags
    sku: {
      name: 'Standard_D2ds_v5'
      tier: 'GeneralPurpose'
    }
    storage: {
      iops: 120
      tier: 'P4'
      storageSizeGB: 32
    }
    version: '16'
    // databaseNames: [ postgresDatabaseName ]
    allowAzureIPsFirewall: true
    entraAdministratorName: entraAdministratorName
    entraAdministratorObjectId: entraAdministratorObjectId
    entraAdministratorType: entraAdministratorType
    authType: authType
  }
}

resource postgresConfig 'Microsoft.DBforPostgreSQL/flexibleServers/configurations@2022-12-01' = {
  dependsOn: [
    postgresServer
  ]
  name: '${name}/azure.extensions'
  properties: {
    value: 'PGCRYPTO'
    source: 'user-override'
  }
}

resource sqlDeploymentScriptSetup 'Microsoft.Resources/deploymentScripts@2020-10-01' = {
  name: '${name}-deployment-script-setup'
  dependsOn: [
    postgresConfig
  ]
  location: location
  kind: 'AzureCLI'
  properties: {
    azCliVersion: '2.9.1'
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
        value: postgresServer.outputs.POSTGRES_DOMAIN_NAME
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

      echo "$SQL_SETUP_SCRIPT" > ./setup.sql
      cat ./setup.sql
      echo "Executing setup script"

      psql -a -U "$PG_USER" -d "postgres" -h "$PG_HOST" -v PG_USER="$PG_USER" -v ADMIN_SYSTEM_ASSIGNED_IDENTITY="$ADMIN_SYSTEM_ASSIGNED_IDENTITY" -v PROXY_SYSTEM_ASSIGNED_IDENTITY="$PROXY_SYSTEM_ASSIGNED_IDENTITY" -w -f ./setup.sql

    '''
  }
}

resource sqlDeploymentScript 'Microsoft.Resources/deploymentScripts@2020-10-01' = {
  name: '${name}-deployment-script'
  dependsOn: [
    sqlDeploymentScriptSetup
  ]
  location: location
  kind: 'AzureCLI'
  properties: {
    azCliVersion: '2.9.1'
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
        value: postgresServer.outputs.POSTGRES_DOMAIN_NAME
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
  
        echo -e "<<EOF\n\x\n${SQL_SCRIPT}\nexit\nEOF" > ./script.sql
        psql -U ${PG_USER} -d "aoai-proxy" -h ${PG_HOST} -w -f ./script.sql
      '''
  }
}

output DOMAIN_NAME string = postgresServer.outputs.POSTGRES_DOMAIN_NAME
