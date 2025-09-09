@description('Primary location for all resources')
param location string

@description('Name of the environment')
param environmentName string

@description('Resource token for unique naming')
param resourceToken string

@description('Resource prefix for naming')
param resourcePrefix string

@description('ASP.NET Core environment variable')
param aspNetCoreEnvironment string

@description('ASP.NET Core URLs configuration')
param aspNetCoreUrls string

// App Service Plan (FREE F1 tier)
resource appServicePlan 'Microsoft.Web/serverfarms@2022-09-01' = {
  name: 'plan-${resourcePrefix}-${resourceToken}'
  location: location
  kind: 'linux'
  properties: {
    reserved: true
  }
  sku: {
    name: 'F1'
    tier: 'Free'
  }
}

// App Service (FREE tier)
resource appService 'Microsoft.Web/sites@2022-09-01' = {
  name: 'app-${resourcePrefix}-${resourceToken}'
  location: location
  kind: 'app,linux'
  tags: {
    'azd-service-name': 'hn-backend'
  }
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|8.0'
      cors: {
        allowedOrigins: ['*']
        supportCredentials: false
      }
      appSettings: [
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: aspNetCoreEnvironment
        }
        {
          name: 'ASPNETCORE_URLS'
          value: aspNetCoreUrls
        }
      ]
    }
    httpsOnly: true
  }
}

// Outputs
output BACKEND_URI string = 'https://${appService.properties.defaultHostName}'
output APP_SERVICE_NAME string = appService.name
