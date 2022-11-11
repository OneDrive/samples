# Redirect File Handler

You may have a scenario where you want to use a file handler to redirect the opening of a file to the browser directly. This sample shows how to securely accomplish a 302 redirect to the file's webUrl.

## Setup

### AAD Configuration

You will need to create an Azure AD Application Registration, and follow these steps to configure it for testing

1. Add an authentication type of "Web" and set the redirect URL to "http://localhost:7071/api/FileHandlerPostTrigger".
2. Add API Permissions "openid", and delegated "Files.Read.Selected"
3. Configure a secret for testing, for production you should use certificates
4. Update the application's manifest to include in the "addIns" section defintion of the file handler, an example can be see in the [sample manifest](sample-manifest.json).

> [Full documentation](https://aka.ms/file-handlers) on configuring file handler

As noted in the documentation it can take up to 24 hours for the file handler to appear in the UI.

### Start Azure Function

Once the file handler entry appears in the UI of SharePoint and OneDrive, you can begin testing. In the azure-function project:

1. `npm install` to get all the dependencies
2. Using vscode, F5 to start the local debug server and set a break point in the FileHandlerPostTrigger/index.ts file
3. In the SharePoint/OneDrive UI click your file handler under "open" on a `.pdf` file
4. Your local azure function should be invoked and you can now step through the execution

## File Handler Flow

1. POST request is received from SharePoint/OneDrive
2. The select item's url is noted
3. MSAL auth code flow is executed to get a valid user+app token (this should be SSO if the app is pre-consented in the tenant)
4. The selected file's webUrl is read and a 302 redirect is issued to that url

## Code Tours

This project supports [Codetour](https://aka.ms/codetour), an extension for [vscode](https://aka.ms/vscode).

## Notes

- [Full documentation on file handlers](https://aka.ms/file-handlers)
- This project was created by following the [Azure Function JavaScript guide](https://learn.microsoft.com/azure/azure-functions/functions-reference-node)
- It makes use of the [auth code flow](https://learn.microsoft.com/azure/active-directory-b2c/authorization-code-flow)
- [Azure Functions Documentation](https://learn.microsoft.com/azure/azure-functions/)
