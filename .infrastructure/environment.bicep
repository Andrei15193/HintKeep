import { buildInRoles } from './Meta/build-in-roles.bicep'
import { storageAccountName, functionsAppName } from './Meta/naming-scheme.bicep'

param name string
param type EnvironmentType

@export()
type EnvironmentType = 'development' | 'production'

@export()
var tableNames = [
  'Users'
]

resource StorageAccount 'Microsoft.Storage/storageAccounts@2025-01-01' = {
  name: storageAccountName(name)
  location: resourceGroup().location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    accessTier: 'Hot'
    allowBlobPublicAccess: false
    supportsHttpsTrafficOnly: true
    minimumTlsVersion: 'TLS1_2'
  }
  tags: {
    project: 'hintkeep'
  }

  resource Identifier 'tableServices' = {
    name: 'default'

    resource Tables 'tables' = [
      for tableName in tableNames: {
        name: tableName
      }
    ]
  }
}

resource PersonalAppPlan 'Microsoft.Web/serverfarms@2024-11-01' existing = {
  name: 'Personal'
  scope: resourceGroup('Personal')
}

resource FunctionsApp 'Microsoft.Web/sites@2024-11-01' = {
  name: functionsAppName(name)
  location: resourceGroup().location
  tags: {
    project: 'HintKeep'
  }
  kind: 'functionapp'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: PersonalAppPlan.id
    httpsOnly: true
    siteConfig: {
      alwaysOn: true
      http20Enabled: false
      acrUseManagedIdentityCreds: false
      minTlsVersion: '1.2'
      linuxFxVersion: 'DOTNET-ISOLATED|9.0'
      functionAppScaleLimit: 0
      minimumElasticInstanceCount: 1

      appSettings: [
        // HintKeep Config
        {
          name: 'DOTNET_ENVIRONMENT'
          value: type == 'development' ? 'Development' : 'Production'
        }

        // Functions App Config
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet-isolated'
        }
        {
          name: 'WEBSITE_USE_PLACEHOLDER_DOTNETISOLATED'
          value: '1'
        }

        // Managed Identity
        {
          name: 'AzureWebJobsStorage__blobServiceUri'
          value: 'https://${StorageAccount.name}.blob.${environment().suffixes.storage}'
        }
        {
          name: 'AzureWebJobsStorage__credential'
          value: 'managedidentity'
        }
        {
          name: 'AzureWebJobsStorage__queueServiceUri'
          value: 'https://${StorageAccount.name}.queue.${environment().suffixes.storage}'
        }
        {
          name: 'AzureWebJobsStorage__tableServiceUri'
          value: 'https://${StorageAccount.name}.table.${environment().suffixes.storage}'
        }
      ]
    }
  }
}

resource ManagedIdentityRoleAssignments 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = [
  for roleGuid in [
    buildInRoles.storage.storageAccount.blob.dataContributor
    buildInRoles.storage.storageAccount.table.dataContributor
  ]: {
    name: guid(resourceGroup().name, FunctionsApp.name, roleGuid)
    scope: StorageAccount
    properties: {
      roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', roleGuid)
      principalId: FunctionsApp.identity.principalId
      principalType: 'ServicePrincipal'
    }
  }
]

output storageAccountName string = StorageAccount.name
output functionsAppName string = FunctionsApp.name
