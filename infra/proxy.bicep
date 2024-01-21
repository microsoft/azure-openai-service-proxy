param name string
param location string = resourceGroup().location
param tags object = {}

param identityName string
param containerAppsEnvironmentName string
param containerRegistryName string
param serviceName string = 'proxy'
param exists bool
param postgresUser string
@secure()
param postgresPassword string
param postgresDatabase string
param postgresServer string

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
    // containerCpuCoreCount: '1.0'
    // containerMemory:'2.0Gi'
    containerMaxReplicas: 5
    env: [
      {
        name: 'POSTGRES_CONNECTION_STRING'
        value: 'postgresql://${postgresUser}:${postgresPassword}@${postgresServer}/${postgresDatabase}'
      }
    ]
  }
}

output SERVICE_PROXY_IDENTITY_PRINCIPAL_ID string = proxyIdentity.properties.principalId
output SERVICE_PROXY_NAME string = app.outputs.name
output SERVICE_PROXY_URI string = app.outputs.uri
output SERVICE_PROXY_IMAGE_NAME string = app.outputs.imageName
