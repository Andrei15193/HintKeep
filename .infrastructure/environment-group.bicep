targetScope = 'subscription'
import { resourceGroupName } from './Meta/naming-scheme.bicep'
import { EnvironmentType } from 'environment.bicep'

param name string
param type EnvironmentType

resource ResourceGroup 'Microsoft.Resources/resourceGroups@2025-04-01' = {
  name: resourceGroupName(name)
  location: deployment().location
  tags: {
    project: 'hintkeep'
  }
}

module Environment './environment.bicep' = {
  name: 'hintkeep-environment-${name}-${deployment().location}'
  scope: ResourceGroup
  params: {
    name: name
    type: type
  }
}

output resourceGroupName string = ResourceGroup.name
output storageAccountName string = Environment.outputs.storageAccountName
output functionsAppName string = Environment.outputs.functionsAppName
