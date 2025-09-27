import { buildInRoles } from './Meta/build-in-roles.bicep'
import { resourceTags } from './Meta/resource-tags.bicep'
import { storageAccountName, functionsAppName } from './Meta/naming-scheme.bicep'

param name string
param type EnvironmentType
param publishUrls string[]

@secure()
param signingKey string
@secure()
param hashAlgoritmKey string

@export()
type EnvironmentType = 'development' | 'production'

@export()
var tableNames = [
  'Users'
  'UserSessions'
]

resource PersonalAppPlan 'Microsoft.Web/serverfarms@2024-11-01' existing = {
  name: 'Personal'
  scope: resourceGroup('Personal')
}

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
  tags: resourceTags

  resource TableServices 'tableServices' = {
    name: 'default'

    resource Tables 'tables' = [
      for tableName in tableNames: {
        name: tableName
      }
    ]
  }
}

resource FunctionsApp 'Microsoft.Web/sites@2024-11-01' = {
  name: functionsAppName(name)
  location: resourceGroup().location
  tags: resourceTags
  kind: 'functionapp'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: PersonalAppPlan.id
    httpsOnly: true
    siteConfig: {
      alwaysOn: true
      http20Enabled: true
      minTlsVersion: '1.2'
      acrUseManagedIdentityCreds: false
      ftpsState: 'Disabled'
      scmType: 'None'
      linuxFxVersion: 'DOTNET-ISOLATED|9.0'

      defaultDocuments: ['index.html']
      cors: {
        allowedOrigins: publishUrls
      }

      appSettings: [
        // HintKeep Config
        {
          name: 'DOTNET_ENVIRONMENT'
          value: type == 'development' ? 'Development' : 'Production'
        }
        {
          name: 'HINTKEEP_SIGNING_KEY'
          value: signingKey
        }
        {
          name: 'HINTKEEP_HASH_KEY'
          value: hashAlgoritmKey
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
        {
          name: 'AzureWebJobsDisableHomepage'
          value: 'true'
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

  resource LogsConfiguration 'config' = {
    name: 'logs'
    properties: {
      httpLogs: {
        fileSystem: {
          enabled: true
          retentionInDays: 7
          retentionInMb: 100
        }
      }
    }
  }
}

resource ManagedIdentityRoleAssignments 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = [
  for roleId in [
    buildInRoles.storage.storageAccount.blob.dataContributor
    buildInRoles.storage.storageAccount.table.dataContributor
    buildInRoles.storage.storageAccount.queue.dataContributor
  ]: {
    name: guid(resourceGroup().name, StorageAccount.name, roleId)
    scope: StorageAccount
    properties: {
      roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', roleId)
      principalId: FunctionsApp.identity.principalId
      principalType: 'ServicePrincipal'
    }
  }
]

output storageAccountName string = StorageAccount.name
output functionsAppName string = FunctionsApp.name
output functionsAppUrl string = 'https://${FunctionsApp.name}.azurewebsites.net'
