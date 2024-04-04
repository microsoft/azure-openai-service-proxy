targetScope = 'subscription'

@minLength(1)
@maxLength(64)
@description('Name which is used to generate a short unique hash for each resource')
param name string

@minLength(1)
@description('Primary location for all resources')
param location string

param proxyAppExists bool = false
param adminAppExists bool = false

@description('Location for the Playground app resource group')
@allowed([ 'centralus', 'eastus2', 'eastasia', 'westeurope', 'westus2' ])
@metadata({
  azd: {
    type: 'location'
  }
})
param swaLocation string

@secure()
@description('PostgreSQL Server administrator password')
param postgresAdminPassword string

@secure()
@description('PostgreSQL Encryption Key')
param postgresEncryptionKey string

param authTenantId string = subscription().tenantId
param authClientId string

var resourceToken = toLower(uniqueString(subscription().id, name, location))
var tags = { 'azd-env-name': name }
var prefix = '${name}-${resourceToken}'

var postgresAdminUser = 'admin${uniqueString(resourceGroup.id)}'
var postgresDatabaseName = 'aoai-proxy'

resource resourceGroup 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: '${name}-rg'
  location: location
  tags: tags
}

// Container apps host (including container registry)
module containerApps 'core/host/container-apps.bicep' = {
  name: 'container-apps'
  scope: resourceGroup
  params: {
    name: '${prefix}-app'
    location: location
    tags: tags
    containerAppsEnvironmentName: '${prefix}-cae'
    containerRegistryName: '${replace(prefix, '-', '')}registry'
    logAnalyticsWorkspaceName: monitoring.outputs.logAnalyticsWorkspaceName
  }
}

// PostgreSQL Server
module postgresServer 'db.bicep' = {
  name: 'postgres-server'
  scope: resourceGroup
  params: {
    name: '${prefix}-postgresql'
    location: location
    tags: tags
    postgresAdminUser: postgresAdminUser
    postgresAdminPassword: postgresAdminPassword
    postgresDatabaseName: postgresDatabaseName
  }
}

// Proxy app
module proxy 'proxy.bicep' = {
  name: 'proxy'
  scope: resourceGroup
  params: {
    name: '${prefix}-proxy'
    location: location
    tags: tags
    identityName: '${prefix}-id-proxy'
    containerAppsEnvironmentName: containerApps.outputs.environmentName
    containerRegistryName: containerApps.outputs.registryName
    exists: proxyAppExists
    postgresServer: postgresServer.outputs.DOMAIN_NAME
    postgresDatabase: postgresDatabaseName
    postgresUser: postgresAdminUser
    postgresPassword: postgresAdminPassword
    postgresEncryptionKey: postgresEncryptionKey
  }
}

// The Playground frontend
module playground 'playground.bicep' = {
  name: 'playground'
  scope: resourceGroup
  params: {
    name: '${prefix}-playground'
    location: swaLocation
    tags: tags
  }
}
// link Playground to Proxy backend
module swaLinkDotnet './linkSwaResource.bicep' = {
  name: 'frontend-link-dotnet'
  scope: resourceGroup
  params: {
    swaAppName: playground.outputs.SERVICE_WEB_NAME
    backendAppName: proxy.outputs.SERVICE_PROXY_NAME
  }
}

// Monitor application with Azure Monitor
module monitoring 'core/monitor/monitoring.bicep' = {
  name: 'monitoring'
  scope: resourceGroup
  params: {
    location: location
    tags: tags
    applicationInsightsDashboardName: '${prefix}-appinsights-dashboard'
    applicationInsightsName: '${prefix}-appinsights'
    logAnalyticsName: '${take(prefix, 50)}-loganalytics' // Max 63 chars
  }
}

// Admin app
module admin 'admin.bicep' = {
  name: 'admin'
  scope: resourceGroup
  params: {
    name: '${prefix}-admin'
    location: location
    tags: tags
    identityName: '${prefix}-id-admin'
    containerAppsEnvironmentName: containerApps.outputs.environmentName
    containerRegistryName: containerApps.outputs.registryName
    exists: adminAppExists
    postgresServer: postgresServer.outputs.DOMAIN_NAME
    postgresDatabase: postgresDatabaseName
    postgresUser: postgresAdminUser
    postgresPassword: postgresAdminPassword
    postgresEncryptionKey: postgresEncryptionKey
    tenantId: authTenantId
    clientId: authClientId
    playgroundUrl: playground.outputs.SERVICE_WEB_URI
    appInsightsConnectionString: monitoring.outputs.applicationInsightsConnectionString
  }
}

output AZURE_LOCATION string = location
output AZURE_CONTAINER_ENVIRONMENT_NAME string = containerApps.outputs.environmentName
output AZURE_CONTAINER_REGISTRY_NAME string = containerApps.outputs.registryName
output AZURE_CONTAINER_REGISTRY_ENDPOINT string = containerApps.outputs.registryLoginServer
output SERVICE_PROXY_IDENTITY_PRINCIPAL_ID string = proxy.outputs.SERVICE_PROXY_IDENTITY_PRINCIPAL_ID
output SERVICE_PROXY_NAME string = proxy.outputs.SERVICE_PROXY_NAME
output SERVICE_PROXY_URI string = proxy.outputs.SERVICE_PROXY_URI
output SERVICE_PROXY_IMAGE_NAME string = proxy.outputs.SERVICE_PROXY_IMAGE_NAME
output SERVICE_PROXY_ENDPOINTS array = [ '${proxy.outputs.SERVICE_PROXY_URI}/docs' ]
output SERVICE_PLAYGROUND_URI string = playground.outputs.SERVICE_WEB_URI
output SERVICE_ADMIN_IDENTITY_PRINCIPAL_ID string = admin.outputs.principalId
output SERVICE_ADMIN_NAME string = admin.outputs.name
output SERVICE_ADMIN_URI string = admin.outputs.uri
output SERVICE_ADMIN_IMAGE_NAME string = admin.outputs.imageName
