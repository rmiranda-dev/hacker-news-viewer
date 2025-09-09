targetScope = 'subscription'

@minLength(1)
@maxLength(64)
@description('Name of the environment that can be used as part of naming resource convention')
param environmentName string

@minLength(1)
@description('Primary location for all resources')
param location string

@description('Name of the resource group')
param resourceGroupName string = 'rg-${environmentName}'

@description('ASP.NET Core environment variable')
param aspNetCoreEnvironment string = 'Production'

@description('ASP.NET Core URLs configuration')
param aspNetCoreUrls string = 'http://+:80'

// Generate a unique suffix for resource names
var resourceToken = uniqueString(subscription().id, location, environmentName)
var resourcePrefix = 'hn'

// Create resource group
resource rg 'Microsoft.Resources/resourceGroups@2023-07-01' = {
  name: resourceGroupName
  location: location
  tags: {
    'azd-env-name': environmentName
  }
}

// Deploy resources to the resource group
module resources 'resources.bicep' = {
  name: 'resources'
  scope: rg
  params: {
    location: location
    environmentName: environmentName
    resourceToken: resourceToken
    resourcePrefix: resourcePrefix
    aspNetCoreEnvironment: aspNetCoreEnvironment
    aspNetCoreUrls: aspNetCoreUrls
  }
}

// Outputs
output RESOURCE_GROUP_ID string = rg.id
output AZURE_LOCATION string = location
output AZURE_TENANT_ID string = tenant().tenantId
output BACKEND_URI string = resources.outputs.BACKEND_URI
