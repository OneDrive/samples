import { PublicClientApplication } from "@azure/msal-browser";

const client = new PublicClientApplication({
    auth: {
        authority: "https://login.microsoftonline.com/{tenant id}/",
        clientId: "{app id}",
        redirectUri: "http://localhost:8080",
    }
});

await client.initialize();

export async function applyToken(init: Partial<RequestInit>): Promise<Partial<RequestInit>> {

    const loginRequest = {
        scopes: ["Files.ReadWrite.AppFolder", "User.Read"],
    };

    let token = "";

    try {

        let tokenResponse = await client.acquireTokenSilent(loginRequest);
        token = tokenResponse.accessToken;

    } catch (e) {

        const resp = await client.loginPopup(loginRequest);

        if (resp.idToken) {

            client.setActiveAccount(resp.account);

            const resp2 = await client.acquireTokenSilent(loginRequest);
            token = resp2.accessToken;

        } else {
            // throw the error that brought us here
            throw e;
        }
    }

    if (typeof init.headers === "undefined") {
        init.headers = {};
    }

    init.headers = {
        "Authorization": `Bearer ${token}`,
        ...init.headers,
    }

    return init;
}
