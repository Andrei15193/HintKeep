import { storageAccountName } from './Meta/naming-scheme.bicep'

param environmentName string

@export()
var tableNames = [
  'table1'
  'table2'
]

resource StorageAccount 'Microsoft.Storage/storageAccounts@2025-01-01' = {
  name: storageAccountName(environmentName)
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  location: resourceGroup().location
  properties: {
    accessTier: 'Hot'
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
