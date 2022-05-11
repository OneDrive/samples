import { ConfidentialClientApplication, OnBehalfOfRequest } from "@azure/msal-node";

export async function getOnBehalfToken(tenantId: string, messageToken: string): Promise<string> {

    const cca = new ConfidentialClientApplication({
        auth: {
            clientId: process.env["MICROSOFT_PROVIDER_AUTHENTICATION_APPID"],
            clientSecret: process.env["MICROSOFT_PROVIDER_AUTHENTICATION_SECRET"],
            authority: `https://login.microsoftonline.com/${tenantId}`
        }
    });

    const oboRequest: OnBehalfOfRequest = {
        oboAssertion: messageToken,
        scopes: ["files.readwrite.all", "offline_access", "user.read"],
    }

    const response = await cca.acquireTokenOnBehalfOf(oboRequest);

    return response.accessToken;
}