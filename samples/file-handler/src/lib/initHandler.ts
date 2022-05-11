import { IncomingMessage, ServerResponse } from "http";
import { Session } from "next-iron-session";
import { ConfidentialClientApplication } from "@azure/msal-node";
import { IActivationProps } from "./types";
import { fromBase64, readRequestBody, toBase64 } from "./utils";
import { URL } from "url";

/**
 * Shape of the stored login state
 */
export interface ILoginState {
    target: string;
    activationParams: IActivationProps;
}

/**
 * Shape of the stored session state
 */
export interface SessionState {
    auth?: {
        token: string;
        expires: Date;
    };
}

export const sessionKey = "session";

/**
 * Factory to create a configured ConfidentialClientApplication
 */
export async function authClientFactory(): Promise<ConfidentialClientApplication> {
    let tenantId = "";
    let clientId = "";
    let clientSecret = "";

    tenantId = process.env.AAD_MSAL_AUTH_TENANT_ID;
    clientId = process.env.AAD_MSAL_AUTH_ID;
    clientSecret = process.env.AAD_MSAL_AUTH_SECRET;

    // for production you may target the common endpoint for multi-tenant apps
    // for dev we target a single tenant for simplicity
    const authority = `https://login.microsoftonline.com/${tenantId}`;

    // produce our configured client application
    return new ConfidentialClientApplication({
        auth: {
            authority: authority,
            clientId: clientId,
            clientSecret: clientSecret,
        },
    });
}

/**
 * Initializes the values required for the file handler to operate, including an access token at parsing the activation props
 * 
 * @param req 
 * @param res 
 */
export async function initHandler(req: IncomingMessage & { session: Session }, res: ServerResponse): Promise<[string, IActivationProps] | never> {

    // get our request query
    const query = (new URL(req.url, process.env.FILEHANDLER_SITE_HOST_URL)).searchParams;
    const token = query.get("token");

    if (token) {
        // this is a redirect from login so we need to setup our session

        // the state is created by us and passed to AAD which passes it back to help with stateless application
        // we could store it in the session, but iron session uses cookies and the activation params may exceed
        // the maximum cookie size
        const loginState: ILoginState = JSON.parse(fromBase64(query.get("state")));

        // construct and save our session data
        const sessionData: SessionState = {
            auth: {
                expires: new Date(query.get("expiresOn")),
                token,
            },
        };
        req.session.set(sessionKey, sessionData);
        await req.session.save();

        return [token, loginState.activationParams];
    }

    const session: SessionState = req.session.get(sessionKey);

    if (session === undefined || typeof session.auth === undefined || session.auth.expires < new Date()) {

        // this is the initial request to the app and we need to read the activation params from the
        // request body to properly initialize the application
        const body = await readRequestBody(req);

        const activationParams: Partial<IActivationProps> = body.split("&").map(v => v.split("=")).reduce((prev: Partial<IActivationProps>, curr: any[]) => {
            prev[curr[0]] = curr[0] === "items" ? JSON.parse(decodeURIComponent(curr[1])) : curr[1];
            return prev;
        }, {});

        // we want to reduce what we keep in the state to only what we need:

        // we will send this state to the server and get it back
        const state = toBase64(JSON.stringify(<ILoginState>{ target: req.url, activationParams: {
            items: activationParams.items,
        } }));

        const authApp = await authClientFactory();

        // send the request to get the auth code url to which we will send the user
        const authUrl = await authApp.getAuthCodeUrl({
            domainHint: activationParams.domainHint,
            loginHint: decodeURIComponent(activationParams.userId),
            redirectUri: `${process.env.FILEHANDLER_SITE_HOST_URL}/api/auth/login`,
            scopes: ["openid", "Files.ReadWrite.Selected"],
            state,
        });

        // redirect this request to the url from the authentication app
        res.writeHead(302, {
            location: authUrl,
        });
        res.end();

        return [null, null];

    } else {

        // this is the case of save or other where we just need the token
        return [session.auth.token, null];
    }
}
