import { AzureFunction, Context, HttpRequest } from "@azure/functions"
import { ConfidentialClientApplication } from "@azure/msal-node";
import fetch from "node-fetch";

// the local testing url of the file handler
const azureFunctionLocalUrl = "http://localhost:7071/api/FileHandlerPostTrigger";

// the scopes we will request for authorization
const scopes = ["openid", "Files.Read.Selected"];

// this represents the properties you get when invoked by a file handler
export interface IActivationProps {
    appId: string;
    client: string;
    cultureName: string;
    domainHint: string;
    /**
     * array of urls, 1 if single select, potentially multiple if multiple select
     */
    items: string[];
    userId: string;
}

// this is the azure function itself
const httpTrigger: AzureFunction = async function (context: Context, req: HttpRequest): Promise<void> {

    // this gets our auth app from the function below
    const authApp = await getAuthApp();

    // here we test if the url as a "code" query param, which is our clue it is a request returning from the auth call
    // this is the auth code flow https://learn.microsoft.com/azure/active-directory-b2c/authorization-code-flow
    if (/code=/i.test(req.url)) {

        // using the code returned from the server we acquire an access token
        const tokenResponse = await authApp.acquireTokenByCode({
            code: req.query.code as string,
            redirectUri: azureFunctionLocalUrl,
            scopes,
        });

        // now we make a request with an auth token to the special url supplied to us by the file handler and captured below
        // this gets us the file details, we select the webUrl as that is what this example users, but all driveItem properties are available
        const docInfoResponse = await fetch(req.query.state + "?select=webUrl", {
            headers: {
                "authorization": `${tokenResponse.tokenType} ${tokenResponse.accessToken}`,
            }
        });

        // convert the response into json
        // in production this should include proper error handling and UX to inform the user of any problems
        const docInfo = await docInfoResponse.json();

        // do a 302 redirect directly to the file, which allows the browser to open the file directly
        context.res = {
            status: 302,
            headers: {
                "Location": docInfo.webUrl,
            },
        }

    } else {

        // this code path represents the initial request to the azure function from the file handler
        // the request body will contain form encoded values representing the activation params which we decode as shown below
        const body = req.body;

        // some logic to decode the body into params, lots of ways this could be done
        const activationParams: Partial<IActivationProps> = body.split("&").map(v => v.split("=")).reduce((prev: Partial<IActivationProps>, curr: any[]) => {
            prev[curr[0]] = curr[0] === "items" ? JSON.parse(decodeURIComponent(curr[1])) : curr[1];
            return prev;
        }, {});

        // we use the item url as the state, this could be a key and all of the activation params stored in a db, file store, in-memory storage, distributed cache, etc.
        // the state is returned to us by the auth flow so we can use it as a reference to rehydrate our request.
        const state = activationParams.items[0];

        // send the request to get the auth code url to which we will send the user
        const authUrl = await authApp.getAuthCodeUrl({
            domainHint: activationParams.domainHint,
            loginHint: decodeURIComponent(activationParams.userId),
            redirectUri: azureFunctionLocalUrl,
            scopes,
            state,
        });

        // redirect this request to the url from the authentication app
        // the user will then have the option to consent to the scopes and continue
        context.res = {
            status: 302,
            headers: {
                "Location": authUrl,
            },
        };
    }
};

export default httpTrigger;

function getAuthApp() {

    // this is the tenant id, should be stored in environment/azure configuration
    const tenantId = "{tenant id}";
    // this is the application id from AAD
    const clientId = "{client id}";
    // this is the secret, appropriate for testing, for prod certifcates should be used
    const clientSecret = "{client secret}";

    // for production you may target the common endpoint for multi-tenant apps
    // for dev we target a single tenant for simplicity
    const authority = `https://login.microsoftonline.com/${tenantId}`;

    // produce our configured client application
    return new ConfidentialClientApplication({
        auth: {
            authority,
            clientId,
            clientSecret,
        },
    });
}
