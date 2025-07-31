targetScope = 'subscription'
import { resourceGroupName } from './Meta/naming-scheme.bicep'

param environmentName string

resource ResourceGroup 'Microsoft.Resources/resourceGroups@2025-04-01' = {
  name: resourceGroupName(environmentName)
  location: deployment().location
  tags: {
    project: 'hintkeep'
  }
}

module Environment './environment.bicep' = {
  name: 'hintkeep-environment-${environmentName}-${deployment().location}'
  scope: ResourceGroup
  params: {
    environmentName: environmentName
  }
}
