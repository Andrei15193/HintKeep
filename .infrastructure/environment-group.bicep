targetScope = 'subscription'
import { resourceGroupName } from './Meta/naming-scheme.bicep'
import { resourceTags } from './Meta/resource-tags.bicep'
import { EnvironmentType } from 'environment.bicep'

param name string
param type EnvironmentType
param publishUrls string[]

resource ResourceGroup 'Microsoft.Resources/resourceGroups@2025-04-01' = {
  name: resourceGroupName(name)
  location: deployment().location
  tags: resourceTags
}

module Environment './environment.bicep' = {
  name: 'hintkeep-environment-${name}-${deployment().location}'
  scope: ResourceGroup
  params: {
    name: name
    type: type
    publishUrls: publishUrls
  }
}

output resourceGroupName string = ResourceGroup.name
output storageAccountName string = Environment.outputs.storageAccountName
output functionsAppName string = Environment.outputs.functionsAppName
output functionsAppUrl string = Environment.outputs.functionsAppUrl
