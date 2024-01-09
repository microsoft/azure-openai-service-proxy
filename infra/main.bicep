targetScope = 'subscription'

@minLength(1)
@maxLength(64)
@description('Name which is used to generate a short unique hash for each resource')
param name string

@minLength(1)
@description('Primary location for all resources')
param location string

param proxyAppExists bool = false

param playgroundServiceName string = ''
@description('Location for the Playground app resource group')
@allowed([ 'centralus', 'eastus2', 'eastasia', 'westeurope', 'westus2' ])
@metadata({
  azd: {
    type: 'location'
  }
})
param swaLocation string

@secure()
@description('PostGreSQL Server administrator password')
param postgresAdminPassword string

var resourceToken = toLower(uniqueString(subscription().id, name, location))
var tags = { 'azd-env-name': name }

resource resourceGroup 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: '${name}-rg'
  location: location
  tags: tags
}

var prefix = '${name}-${resourceToken}'

var postgresServerName = '${prefix}-postgresql'
var postgresAdminUser = 'admin${uniqueString(resourceGroup.id)}'
var postgresDatabaseName = 'aoai-proxy'

// Create storage account for the service
module storageAccount 'storage.bicep' = {
  name: 'storage'
  scope: resourceGroup
  params: {
    name: 'storage${resourceToken}'
    location: location
  }
}

// Create PostgreSQL database
module postgresServer 'core/database/postgresql/flexibleserver.bicep' = {
  name: 'postgresql'
  scope: resourceGroup
  params: {
    name: postgresServerName
    location: location
    tags: tags
    sku: {
      name: 'Standard_B1ms'
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

// Container apps host (including container registry)
module containerApps 'core/host/container-apps.bicep' = {
  name: 'container-apps'
  scope: resourceGroup
  params: {
    name: 'app'
    location: location
    tags: tags
    containerAppsEnvironmentName: '${prefix}-containerapps-env'
    containerRegistryName: '${replace(prefix, '-', '')}registry'
    logAnalyticsWorkspaceName: monitoring.outputs.logAnalyticsWorkspaceName
  }
}

// Proxy app
module proxy 'proxy.bicep' = {
  name: 'proxy'
  scope: resourceGroup
  params: {
    name: replace('${take(prefix, 19)}-ca', '--', '-')
    location: location
    tags: tags
    identityName: '${prefix}-id-proxy'
    containerAppsEnvironmentName: containerApps.outputs.environmentName
    containerRegistryName: containerApps.outputs.registryName
    exists: proxyAppExists
    azure_storage_connection_string: storageAccount.outputs.connectionString
    postgresServer: postgresServer.outputs.POSTGRES_DOMAIN_NAME
    postgresDatabase: postgresDatabaseName
    postgresUser: postgresAdminUser
    postgresPassword: postgresAdminPassword
  }
}

// The Playground frontend
module playground 'playground.bicep' = {
  name: 'playground'
  scope: resourceGroup
  params: {
    name: !empty(playgroundServiceName) ? playgroundServiceName : 'swaplayground-${resourceToken}'
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

// create an app service plan for the Monitor Azure Function
module appServicePlan 'core/host/appserviceplan.bicep' = {
  name: 'appServicePlan'
  scope: resourceGroup
  params: {
    name: 'openai-proxy-${resourceToken}-app-service-plan'
    location: location
    tags: tags
    sku: {
      name: 'Y1'
    }
  }
}

module MonitorFunction 'core/host/functions.bicep' = {
  name: 'azureFunctions'
  scope: resourceGroup
  params: {
    name: 'openai-proxy-${resourceToken}-monitor-function'
    location: location
    tags: tags
    runtimeVersion: '~4'
    runtimeName: 'dotnet'
    appServicePlanId: appServicePlan.outputs.id
    storageAccountName: storageAccount.outputs.name
    alwaysOn: false
    appSettings: {
      AzureProxyStorageAccount: storageAccount.outputs.connectionString
    }
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
