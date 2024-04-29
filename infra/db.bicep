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

resource sqlDeploymentScript 'Microsoft.Resources/deploymentScripts@2020-10-01' = {
  name: '${name}-deployment-script'
  dependsOn: [
    postgresConfig
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
      apk add postgresql-client

      psql -U ${PG_USER} -d postgres -h ${PG_HOST} -w -c 'CREATE DATABASE "aoai-proxy";'

      psql -U ${PG_USER} -d postgres -h ${PG_HOST} -w -c 'CREATE ROLE aoai_proxy_app WITH NOLOGIN NOSUPERUSER INHERIT NOCREATEDB NOCREATEROLE NOREPLICATION NOBYPASSRLS;'
      psql -U ${PG_USER} -d postgres -h ${PG_HOST} -w -c 'CREATE ROLE aoai_proxy_reporting WITH NOLOGIN NOSUPERUSER INHERIT NOCREATEDB NOCREATEROLE NOREPLICATION NOBYPASSRLS;'

      psql -U ${PG_USER} -d postgres -h ${PG_HOST} -w -c 'GRANT aoai_proxy_app TO ${PG_USER};'
      psql -U ${PG_USER} -d postgres -h ${PG_HOST} -w -c 'GRANT aoai_proxy_reporting TO ${PG_USER};'

      psql -U ${PG_USER} -d postgres -h ${PG_HOST} -w -c "select * from pgaadauth_create_principal('${ADMIN_SYSTEM_ASSIGNED_IDENTITY}', false, false);"
      psql -U ${PG_USER} -d postgres -h ${PG_HOST} -w -c "select * from pgaadauth_create_principal('${PROXY_SYSTEM_ASSIGNED_IDENTITY}', false, false);"

      psql -U ${PG_USER} -d postgres -h ${PG_HOST} -w -c 'GRANT aoai_proxy_app TO ${ADMIN_SYSTEM_ASSIGNED_IDENTITY};'
      psql -U ${PG_USER} -d postgres -h ${PG_HOST} -w -c 'GRANT aoai_proxy_app TO ${PROXY_SYSTEM_ASSIGNED_IDENTITY};'


      psql -U ${PG_USER} -d ${PG_DB} -h ${PG_HOST} -w <<EOF
      \x
      ${SQL_SCRIPT}
      EOF
    '''
  }
}

resource postgres_setup 'Microsoft.Resources/deploymentScripts@2020-10-01' = {
  name: '${name}-deployment-script'
  dependsOn: [
    sqlDeploymentScript
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
      apk add postgresql-client

      psql -U ${PG_USER} -d postgres -h ${PG_HOST} -w -c 'CREATE DATABASE "aoai-proxy";'

      psql -U ${PG_USER} -d postgres -h ${PG_HOST} -w -c 'CREATE ROLE aoai_proxy_app WITH NOLOGIN NOSUPERUSER INHERIT NOCREATEDB NOCREATEROLE NOREPLICATION NOBYPASSRLS;'
      psql -U ${PG_USER} -d postgres -h ${PG_HOST} -w -c 'CREATE ROLE aoai_proxy_reporting WITH NOLOGIN NOSUPERUSER INHERIT NOCREATEDB NOCREATEROLE NOREPLICATION NOBYPASSRLS;'

      psql -U ${PG_USER} -d postgres -h ${PG_HOST} -w -c 'GRANT aoai_proxy_app TO ${PG_USER};'
      psql -U ${PG_USER} -d postgres -h ${PG_HOST} -w -c 'GRANT aoai_proxy_reporting TO ${PG_USER};'

      psql -U ${PG_USER} -d postgres -h ${PG_HOST} -w -c "select * from pgaadauth_create_principal('${ADMIN_SYSTEM_ASSIGNED_IDENTITY}', false, false);"
      psql -U ${PG_USER} -d postgres -h ${PG_HOST} -w -c "select * from pgaadauth_create_principal('${PROXY_SYSTEM_ASSIGNED_IDENTITY}', false, false);"

      psql -U ${PG_USER} -d postgres -h ${PG_HOST} -w -c 'GRANT aoai_proxy_app TO ${ADMIN_SYSTEM_ASSIGNED_IDENTITY};'
      psql -U ${PG_USER} -d postgres -h ${PG_HOST} -w -c 'GRANT aoai_proxy_app TO ${PROXY_SYSTEM_ASSIGNED_IDENTITY};'


      psql -U ${PG_USER} -d ${PG_DB} -h ${PG_HOST} -w <<EOF
      \x
      ${SQL_SCRIPT}
      EOF
    '''
  }
}


output DOMAIN_NAME string = postgresServer.outputs.POSTGRES_DOMAIN_NAME
