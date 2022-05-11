# Introduction

This sample shows the file picking and file browsing controls using an .NET 6.0 ASP.NET Core application. For getting access tokens a call to the ASP.NET Core server is made.

# Demo setup

## Setup your Azure AD application

1. Create a new AAD App Registration, note the client id of the application
2. Under authentication, configure a new Web
    1. Set the redirect uri to `https://localhost:4322/signin-oidc`
    2. You may optionally configure this application for multitenant but this is outside the scope of this article
3. Under API permissions
   1. Add `Files.Read.All`, `Sites.Read.All`, Leave `User.Read` for Graph delegated permissions
   2. Add `AllSites.Read`, `MyFiles.Read` for SharePoint delegated permissions
   3. Consent the application via "Grant admin consent for Contoso"
4. Under Certificates and secrets
   1. Add a new client secret, note the secret value

## Configure the `appsettings.json` file

Go to the sample's `appsettings.json` file and replace the following values:

- `<contoso>` with your tenant name
- `<client id>` with the client id of the Azure AD application created in the previous step
- `<client secret>` with the client secrete value added to your Azure AD application created in the previous step

```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "Domain": "<contoso>.onmicrosoft.com",
    "TenantId": "organizations",
    "ClientId": "<client id>",
    "ClientSecret": "<client secret>",
    "CallbackPath": "/signin-oidc"
  },
  "Picker": {
    "SiteUrl": "https://<contoso>-my.sharepoint.com"
  },
  "Browser": {
    "SiteUrl": "https://<contoso>.sharepoint.com"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*"
}
```
