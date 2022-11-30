# typescript-sdk-odc

This sample shows how to work with the picker and the example [sdk-domevents](../sdk-domevents/readme.md) to connect to OneDrive Consumer.

> Using an SDK is optional, to see a basic example please review the [javascript-basic](../javascript-basic/readme.md) sample.

## Setup

You need a .env file with (replacing {tenant id} with the correct value):

```
CLIENT_ID = '"{AAD CLIENT ID}"'

CLIENT_AUTHORITY_OD = '"https://login.microsoftonline.com/consumers"'

CLIENT_AUTHORITY_ODSP = '"https://login.microsoftonline.com/{tenant id}"'
```

## AAD App Setup for Personal OneDrive Access

1. When you setup the AAD Application Registration you need to select the "Accounts in any organizational directory (Any Azure AD directory - Multitenant) and personal Microsoft accounts (e.g. Skype, Xbox)" option. This cannot be changed for existing applications.
2. The scope you need to request is `OneDrive.ReadWrite` - which is used behind the scenes, in the AAD application you select `Files.ReadWrite`


### If you want to support personal and org accounts you may need to update the authority at runtime to match the account's files you are currently trying to load.

Personal: https://login.microsoftonline.com/consumers
OD4B & SharePoint: https://login.microsoftonline.com/common

## Build the dependency

Before running this sample you will need to navigate to the ../sdk-domevents and run `npm run build` to build the dependency sdk.
