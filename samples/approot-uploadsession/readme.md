# AppRoot Upload Session

This sample shows how to create a basic upload session for large files using an application's [special approot folder](https://learn.microsoft.com/graph/api/drive-get-specialfolder).

## Setup

### Create a new Application Registration in Entra ID

1. Creat a new application registration
2. Add a SPA authentication method
    - Ensure you allow Access Tokens and ID Tokens
    - Redirect: https://localhost:8080
3. Consent to the `Files.ReadWrite` or `Files.ReadWrite.AppFolder` (least privledge) delegated scope
4. Create a file

### Update application info

1. In [security.ts](./src/security.ts) update the `authority` and `clientId` to match those of the application registration
2. In [security.ts](./src/security.ts) ensure that the scopes listed in the `loginRequest` object match the scopes consented for your application

## Run

1. Ensure all dependencies are installed `npm install`
2. Start the application `npm start`
3. A browser window should auto-launch, if not open a browser and load `http://localhost:8080/`
4. Using the UX select a file and then select "Do Upload"
