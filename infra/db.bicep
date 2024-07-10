param location string
param tags object

param name string
param authType string = 'Password'

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
      name: 'Standard_B2s'
      tier: 'Burstable'
    }
    storage: {
      iops: 120
      tier: 'P4'
      storageSizeGB: 32
    }
    version: '16'
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

output DOMAIN_NAME string = postgresServer.outputs.POSTGRES_DOMAIN_NAME
output RESOURCE_NAME string = name
