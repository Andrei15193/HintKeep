using '../environment-group.bicep'

param name = 'test'
param type = 'development'
param publishUrls = [
  'https://test.hintkeep.com'
]

param hashAlgoritmKey = readEnvironmentVariable('HINTKEEP_HASH_KEY', '')
