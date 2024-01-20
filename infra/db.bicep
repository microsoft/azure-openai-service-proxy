param location string
param tags object
param postgresAdminUser string
@secure()
param postgresAdminPassword string
param postgresDatabaseName string
param name string

// Create PostgreSQL database
module postgresServer 'core/database/postgresql/flexibleserver.bicep' = {
  name: 'postgresql'
  params: {
    name: name
    location: location
    tags: tags
    sku: {
      name: 'Standard_B2s'
      tier: 'Burstable'
    }
    storage: {
      storageSizeGB: 32
    }
    version: '16'
    administratorLogin: postgresAdminUser
    administratorLoginPassword: postgresAdminPassword
    databaseNames: [ postgresDatabaseName ]
    allowAzureIPsFirewall: true
  }
}

resource sqlDeploymentScript 'Microsoft.Resources/deploymentScripts@2020-10-01' = {
  name: '${name}-deployment-script'
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
    environmentVariables: [ {
        name: 'SQL_SCRIPT'
        value: loadTextContent('../database/aoai-proxy.sql')
      }
      {
        name: 'PG_USER'
        value: postgresAdminUser
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
        value: postgresAdminPassword
      }
    ]

    scriptContent: '''
      apk add postgresql-client

      psql -U ${PG_USER} -d ${PG_DB} -h ${PG_HOST} -w <<EOF
      \x
      ${SQL_SCRIPT}
      EOF
    '''
  }
}

output DOMAIN_NAME string = postgresServer.outputs.POSTGRES_DOMAIN_NAME
