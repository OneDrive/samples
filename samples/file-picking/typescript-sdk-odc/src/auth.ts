import { PublicClientApplication, Configuration, SilentRequest } from "@azure/msal-browser";
import { combine } from "@pnp/core";
import { IAuthenticateCommand } from "@pnp/picker-sdk";
import { get } from "env-var";

let app : PublicClientApplication = null;

export async function getToken(command: IAuthenticateCommand): Promise<string> {
    let authParams = { scopes:[] };

    switch (command.type) {
        case "SharePoint":
            authParams = { scopes: [`${combine(command.resource, ".default")}`] };
            break;
        case "Graph":
                authParams = { scopes: ["OneDrive.ReadWrite"] };
                break;
        default:
            break;
    }

    return getTokenWithScopes(authParams.scopes, command.type);
}

export async function getTokenWithScopes(scopes: string[], type: string, additionalAuthParams?: Omit<SilentRequest, "scopes">): Promise<string> {

    const clientId = get("CLIENT_ID").required().asString();
    let authority = get("CLIENT_AUTHORITY_OD").required().asString();

    if(type == "SharePoint")
    {
        authority = get("CLIENT_AUTHORITY_ODSP").required().asString();
    }

    const msalParams = {
        auth: {
            authority,
            clientId,
            redirectUri: "http://localhost:3000",
        },
    }

    app = new PublicClientApplication(msalParams);

    let accessToken = "";
    const authParams = { scopes, ...additionalAuthParams };

    try {

        // see if we have already the idtoken saved
        const resp = await app.acquireTokenSilent(authParams!);
        accessToken = resp.accessToken;

    } catch (e) {

        // per examples we fall back to popup
        const resp = await app.loginPopup(authParams!);
        app.setActiveAccount(resp.account);

        if (resp.idToken) {

            const resp2 = await app.acquireTokenSilent(authParams!);
            accessToken = resp2.accessToken;

        } else {

            // throw the error that brought us here
            throw e;
        }
    }

    return accessToken;
}
