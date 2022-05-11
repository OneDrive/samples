import { log, enter, leave } from "../logger";
import { createInterface } from "readline";
import { PublicClientApplication } from "@azure/msal-node";

const GetToken = async (): Promise<{ appId: string, tenantId: string, token: string }> => {

    enter("GetToken");

    log("Beginning device-code flow.");

    const readline = createInterface({
        input: process.stdin,
        output: process.stdout,
    });

    const appId = await new Promise<string>(resolve => {
        readline.question("What is the client id for the setup application? ", id => {
            resolve(id);
        });
    });

    const tenantId = await new Promise<string>(resolve => {
        readline.question("What is the tenant id for the setup application? ", id => {
            resolve(id);
        });
    });

    log(`Using appId: ${appId} in tenant id: ${tenantId}`);

    let token = "";

    log("Requesting token");

    if (token === "") {

        // authenticate
        const pca = new PublicClientApplication({
            auth: {
                authority: `https://login.microsoftonline.com/${tenantId}`,
                clientId: appId,
            },
        });

        const tokenResponse = await pca.acquireTokenByDeviceCode({
            deviceCodeCallback: (response) => (console.log(response.message)),
            scopes: ["Directory.AccessAsUser.All"],
        });

        token = tokenResponse.accessToken;
    }

    log("Token acquired");

    leave("GetToken");

    return { appId, tenantId, token };
};

export default GetToken;
