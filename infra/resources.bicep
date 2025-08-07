param location string
param resourceToken string
param tags object

@description('The SKU of App Service Plan.')
param sku string = 'P0V3'

@description('The CIPP API base URL')
param cippApiBaseUrl string = 'https://cippmboqc.azurewebsites.net'

@description('The CIPP Static Web App URL')
param cippSwaUrl string = 'https://lemon-hill-0df49860f.3.azurestaticapps.net'

resource appServicePlan 'Microsoft.Web/serverfarms@2022-03-01' = {
  name: 'plan-${resourceToken}'
  location: location
  sku: {
    name: sku
    capacity: 1
  }
  properties: {
    reserved: false
  }
  tags: tags
}

resource webApp 'Microsoft.Web/sites@2024-04-01' = {
  name: 'app-${resourceToken}'
  location: location
  tags: union(tags, { 'azd-service-name': 'web' })
  kind: 'app'
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    clientAffinityEnabled: true
    siteConfig: {
      minTlsVersion: '1.2'
      http20Enabled: true
      alwaysOn: true
      windowsFxVersion: 'DOTNET|9.0'
      metadata: [
        {
          name: 'CURRENT_STACK'
          value: 'dotnet'
        }
      ]
    }
  }
  
  resource appSettings 'config' = {
    name: 'appsettings'
    properties: {
      SCM_DO_BUILD_DURING_DEPLOYMENT: 'true'
      WEBSITE_HTTPLOGGING_RETENTION_DAYS: '3'
      CIPP_API_BASE_URL: cippApiBaseUrl
      CIPP_SWA_URL: cippSwaUrl
      ASPNETCORE_ENVIRONMENT: 'Production'
    }
  }
}

output WEB_URI string = 'https://${webApp.properties.defaultHostName}'
output WEB_NAME string = webApp.name
