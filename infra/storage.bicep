param name string
param location string = resourceGroup().location
param tags object = {}

@allowed([
  'Cool'
  'Hot'
  'Premium' ])
param accessTier string = 'Hot'
param allowBlobPublicAccess bool = true
param allowCrossTenantReplication bool = true
param allowSharedKeyAccess bool = true
param containers array = []
param defaultToOAuthAuthentication bool = false
param deleteRetentionPolicy object = {}
@allowed([ 'AzureDnsZone', 'Standard' ])
param dnsEndpointType string = 'Standard'
param kind string = 'StorageV2'
param minimumTlsVersion string = 'TLS1_2'
param networkAcls object = {
  bypass: 'AzureServices'
  defaultAction: 'Allow'
}
@allowed([ 'Enabled', 'Disabled' ])
param publicNetworkAccess string = 'Enabled'
param sku object = { name: 'Standard_LRS' }

resource storage 'Microsoft.Storage/storageAccounts@2022-05-01' = {
  name: name
  location: location
  tags: tags
  kind: kind
  sku: sku
  properties: {
    accessTier: accessTier
    allowBlobPublicAccess: allowBlobPublicAccess
    allowCrossTenantReplication: allowCrossTenantReplication
    allowSharedKeyAccess: allowSharedKeyAccess
    defaultToOAuthAuthentication: defaultToOAuthAuthentication
    dnsEndpointType: dnsEndpointType
    minimumTlsVersion: minimumTlsVersion
    networkAcls: networkAcls
    publicNetworkAccess: publicNetworkAccess
  }

  resource blobServices 'blobServices' = if (!empty(containers)) {
    name: 'default'
    properties: {
      deleteRetentionPolicy: deleteRetentionPolicy
    }
    resource container 'containers' = [for container in containers: {
      name: container.name
      properties: {
        publicAccess: contains(container, 'publicAccess') ? container.publicAccess : 'None'
      }
    }]
  }
}

resource table_service 'Microsoft.Storage/storageAccounts/tableServices@2023-01-01' = {
  parent: storage
  name: 'default'
  properties: {
    cors: {
      corsRules: []
    }
  }
}

resource configuration_table 'Microsoft.Storage/storageAccounts/tableServices/tables@2023-01-01' = {
  parent: table_service
  name: 'configuration'
  properties: {}
}

resource authorization_table 'Microsoft.Storage/storageAccounts/tableServices/tables@2023-01-01' = {
  parent: table_service
  name: 'authorization'
  properties: {}
}

resource management_table 'Microsoft.Storage/storageAccounts/tableServices/tables@2023-01-01' = {
  parent: table_service
  name: 'management'
  properties: {}
}

// add storage queue service
resource queue_service 'Microsoft.Storage/storageAccounts/queueServices@2023-01-01' = {
  parent: storage
  name: 'default'
  properties: {
    cors: {
      corsRules: []
    }
  }
}

resource monitor_queue 'Microsoft.Storage/storageAccounts/queueServices/queues@2023-01-01' = {
  parent: queue_service
  name: 'monitor'
  properties: {}
}

resource notifications_queue 'Microsoft.Storage/storageAccounts/queueServices/queues@2023-01-01' = {
  parent: queue_service
  name: 'notifications'
  properties: {}
}


var storageAccountKeys = listKeys(storage.id, '2021-04-01')
var connectionString = 'DefaultEndpointsProtocol=https;AccountName=${name};AccountKey=${storageAccountKeys.keys[0].value};EndpointSuffix=core.windows.net'

output name string = storage.name
output primaryEndpoints object = storage.properties.primaryEndpoints
output connectionString string = connectionString
