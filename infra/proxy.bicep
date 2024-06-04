param name string
param location string = resourceGroup().location
param tags object = {}

@description('The name of the user assigned identity that the container app will used to connect to the container registraty')
param identityName string
param containerAppsEnvironmentName string
param containerRegistryName string
param serviceName string = 'proxy'
param exists bool
param postgresDatabase string
param postgresServer string
@secure()
param postgresEncryptionKey string
@secure()
param appInsightsConnectionString string

resource proxyIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: identityName
  location: location
}

module app 'core/host/container-app-upsert.bicep' = {
  name: '${serviceName}-container-app-module'
  params: {
    name: name
    location: location
    tags: union(tags, { 'azd-service-name': serviceName })
    identityName: proxyIdentity.name
    exists: exists
    containerAppsEnvironmentName: containerAppsEnvironmentName
    containerRegistryName: containerRegistryName
    targetPort: 3100
    containerCpuCoreCount: '0.75'
    containerMemory: '1.5Gi'
    containerMaxReplicas: 2
    secrets: [
      {
        name: 'postgres-encryption-key'
        value: postgresEncryptionKey
      }
      {
        name: 'postgres-user'
        value: name
      }
      {
        name: 'postgres-database'
        value: postgresDatabase
      }
      {
        name: 'postgres-server'
        value: postgresServer
      }
      {
        name: 'app-insights-connection-string'
        value: appInsightsConnectionString
      }
    ]
    env: [
      {
        name: 'POSTGRES_ENCRYPTION_KEY'
        secretRef: 'postgres-encryption-key'
      }
      {
        name: 'POSTGRES_USER'
        secretRef: 'postgres-user'
      }
      {
        name: 'POSTGRES_DATABASE'
        secretRef: 'postgres-database'
      }
      {
        name: 'POSTGRES_SERVER'
        secretRef: 'postgres-server'
      }
      {
        name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
        secretRef: 'app-insights-connection-string'
      }
    ]
  }
}

output SERVICE_PROXY_IDENTITY_PRINCIPAL_ID string = proxyIdentity.properties.principalId
output SERVICE_PROXY_NAME string = app.outputs.name
output SERVICE_PROXY_URI string = app.outputs.uri
output SERVICE_PROXY_IMAGE_NAME string = app.outputs.imageName
