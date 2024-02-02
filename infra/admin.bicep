param name string
param location string = resourceGroup().location
param tags object = {}

param identityName string
param containerAppsEnvironmentName string
param containerRegistryName string
param serviceName string = 'admin'
param exists bool
param postgresUser string
@secure()
param postgresPassword string
param postgresDatabase string
param postgresServer string
param clientId string
param tenantId string
param playgroundUrl string
param appInsightsConnectionString string

resource adminIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: identityName
  location: location
}

module app 'core/host/container-app-upsert.bicep' = {
  name: '${serviceName}-container-app-module'
  params: {
    name: name
    location: location
    tags: union(tags, { 'azd-service-name': serviceName })
    identityName: adminIdentity.name
    exists: exists
    containerAppsEnvironmentName: containerAppsEnvironmentName
    containerRegistryName: containerRegistryName
    targetPort: 8080
    secrets: [
      {
        name: 'postconstr'
        value: 'Server=${postgresServer};Port=5432;User Id=${postgresUser};Password=${postgresPassword};Database=${postgresDatabase};Ssl Mode=Require;'
      }
      {
        name: 'tenant-id'
        value: tenantId
      }
      {
        name: 'client-id'
        value: clientId
      }
      {
        name: 'app-insights-connection-string'
        value: appInsightsConnectionString
      }
    ]
    env: [
      {
        name: 'ConnectionStrings__AoaiProxyContext'
        secretRef: 'postconstr'
      }
      {
        name: 'AzureAd__TenantId'
        secretRef: 'tenant-id'
      }
      {
        name: 'AzureAd__ClientId'
        secretRef: 'client-id'
      }
      {
        name: 'ASPNETCORE_FORWARDEDHEADERS_ENABLED'
        value: 'true'
      }
      {
        name: 'PlaygroundUrl'
        value: playgroundUrl
      }
      {
        name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
        secretRef: 'app-insights-connection-string'
      }
    ]
  }
}

output principalId string = adminIdentity.properties.principalId
output name string = app.outputs.name
output uri string = app.outputs.uri
output imageName string = app.outputs.imageName
