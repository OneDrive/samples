# Contoso: Markdown FileHandler

This file handler sample uses Nextjs, MSAL, the Monaco editor, and React to demonstrate a Markdown [file handler](https://docs.microsoft.com/en-us/onedrive/developer/file-handlers). You can use the code tours for introductions to various pieces of the functionality, or follow the dev setup guide below to begin testing.

## Dev Setup

To work locally you need to setup a file handler app registration and load it with a manifest. We have included a tool and associated npm script to accomplish this.

You must first setup an application that will be used to sign you in and modify the directory within your tenant. To do this you must be a tenant admin.

> It currently takes 24-48 hours before your file handler will appear

1. Create new application registration in AAD and give it an easy to find name, something like "filehandler localdev setup"

2. Add permissions:
  - Graph: Delegated: openid, Directory.AccessAsUser.All
  - consent to the permissions

3. Under Authentication:
  - Add a Platform
    - Add Mobile and desktop applications
    - Select (MSAL only) option in list of redirect uris
  - Set "Default client type" to treat app as public client "Yes"

-> note client id of app:
-> note tenant id of app:

4. Run: "npm run dev-setup" 
  - supply client id and secret when prompted
  - follow the on screen prompts to complete the device-code auth flow

> It currently takes 24-48 hours for new file handlers to appear in the UI. You can [follow these steps for clearing the cache](https://docs.microsoft.com/en-us/onedrive/developer/file-handlers/reset-cache) to speed this up for development.

5. Once setup you can optionally delete the registration helper application, or leave it in place for future use.

6. Running the `dev-setup` command in step 4 generates an ".env.local" file in the root of the filehandler directory. Review the values and ensure it was created correctly. It should appear similar to below:

```
IRON_SESSION_PASSWORD='{anything you want}'
NODE_ENV='production'
AAD_MSAL_AUTH_TENANT_ID="{app tenant id from step 4}"
AAD_MSAL_AUTH_ID="{app client id from step 4}"
AAD_MSAL_AUTH_SECRET="{app secret from step 4}"
FILEHANDLER_SITE_HOST_URL="https://localhost:3000"
```

> This file is excluded from git via .gitignore so it is safe for your local testing secrets

7. Run `npm run dev` and launch the file handler from within OneDrive or a SharePoint document library on a __.md__ file.
