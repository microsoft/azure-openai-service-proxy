param name string
param location string = resourceGroup().location
param tags object = {}

@description('The name of the user assigned identity that the container app will used to connect to the container registraty')
param identityName string
param containerAppsEnvironmentName string
param containerRegistryName string
param serviceName string = 'admin'
param exists bool
param postgresDatabase string
param postgresServer string
@secure()
param postgresEncryptionKey string
param clientId string
param tenantId string
param playgroundUrl string
@secure()
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
      {
        name: 'postgres-encryption-key'
        value: postgresEncryptionKey
      }
      {
        name: 'postgres-connection-string'
        value: 'Server=${postgresServer};Port=5432;User Id=${name};Database=${postgresDatabase};Ssl Mode=Require;'
      }
    ]
    env: [
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
      {
        name: 'PostgresEncryptionKey'
        secretRef: 'postgres-encryption-key'
      }
      {
        name: 'ConnectionStrings__AoaiProxyContext'
        secretRef: 'postgres-connection-string'
      }
    ]
  }
}

output SERVICE_ADMIN_IDENTITY_PRINCIPAL_ID string = adminIdentity.properties.principalId
output SERVICE_ADMIN_NAME string = app.outputs.name
output SERVICE_ADMIN_URI string = app.outputs.uri
output SERVICE_ADMIN_IMAGE_NAME string = app.outputs.imageName
