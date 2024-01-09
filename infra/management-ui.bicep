param name string
param location string = resourceGroup().location
param tags object = {}

param identityName string
param containerAppsEnvironmentName string
param containerRegistryName string
param serviceName string = 'management-ui'
param exists bool

resource managementUIIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: identityName
  location: location
}

module app 'core/host/container-app-upsert.bicep' = {
  name: '${serviceName}-container-app-module'
  params: {
    name: name
    location: location
    tags: union(tags, { 'azd-service-name': serviceName })
    identityName: managementUIIdentity.name
    exists: exists
    containerAppsEnvironmentName: containerAppsEnvironmentName
    containerRegistryName: containerRegistryName
    targetPort: 8081
    env: []
  }
}

output IDENTITY_PRINCIPAL_ID string = managementUIIdentity.properties.principalId
output NAME string = app.outputs.name
output URI string = app.outputs.uri
output IMAGE_NAME string = app.outputs.imageName
