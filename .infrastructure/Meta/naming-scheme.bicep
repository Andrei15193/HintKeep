@export()
func resourceGroupName(environmentName string) string => toLower('hintkeep-${environmentName}')

@export()
func storageAccountName(environmentName string) string => toLower('hintkeepstorage${environmentName}')

@export()
func functionsAppName(environmentName string) string => toLower('hintkeep-webapp-${environmentName}')
