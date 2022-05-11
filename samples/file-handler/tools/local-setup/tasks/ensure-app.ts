import { log, enter, leave } from "../logger";
import { fetch, getHeaders, isError, toJson } from "../fetch";
import {
    Application as IApplication,
    ServicePrincipal as IServicePrincipal,
    OAuth2PermissionGrant as IOAuth2PermissionGrant,
    PasswordCredential as IPasswordCredential,
} from "@microsoft/microsoft-graph-types";

const appDisplayName = "Contoso-FileHandler-Markdown";

const EnsureApp = async (token: string): Promise<{ displayName: string, appId: string, appSecret?: string, id: string, created: boolean }> => {

    enter("CreateApp");

    const headers = getHeaders(token);

    const apps = await fetch(`https://graph.microsoft.com/v1.0/applications?$filter=displayName eq '${appDisplayName}'&$select=displayName,id,appId`,
        {
            ...headers,
        }).then(isError).then(toJson<Pick<IApplication, "displayName" | "id" | "appId">[]>());

    if (apps.length > 0) {

        // the task already exists
        log(`Application ${appDisplayName} already exists with id ${apps[0].id}. No action taken.`);

        return { displayName: appDisplayName, appId: apps[0].appId, id: apps[0].id, created: false };
    }

    log(`Application ${appDisplayName} not found in directory. Creating app...`);

    // create the application
    const createResponseJson = await fetch("https://graph.microsoft.com/v1.0/applications", {
        body: JSON.stringify(<IApplication>{
            api: {
                "acceptMappedClaims": null,
                "knownClientApplications": [],
                "oauth2PermissionScopes": [],
                "preAuthorizedApplications": [],
                "requestedAccessTokenVersion": 2,
            },
            displayName: appDisplayName,
            isFallbackPublicClient: true,
            signInAudience: "AzureADMyOrg",
            web: {
                redirectUris: ["https://localhost:3000/api/auth/login"],
                logoutUrl: "https://localhost:3000/api/auth/logout",
                implicitGrantSettings: {
                    enableIdTokenIssuance: false,
                    enableAccessTokenIssuance: false,
                },
            },
        }),
        method: "POST",
        ...headers,
    }).then(isError).then(toJson<IApplication>());

    log(`Created new application with name ${createResponseJson.displayName} and appId ${createResponseJson.appId}.`);

    log(`Creating service principal for appId: ${createResponseJson.appId}.`);

    // then we need to create a service principal
    const createServicePrincipalResponseJson = await fetch(`https://graph.microsoft.com/v1.0/serviceprincipals`, {
        body: JSON.stringify(<IServicePrincipal>{
            appId: createResponseJson.appId,
        }),
        method: "POST",
        ...headers,
    }).then(isError).then(toJson<IServicePrincipal>());

    log(`Created service principal for appId: ${createResponseJson.appId}.`);

    log(`Adding permission scopes...`);

    // lookup the id for the graph resource
    const principalInfos = await fetch("https://graph.microsoft.com/v1.0/servicePrincipals?$filter=displayName eq 'Microsoft Graph'&$select=id",
        {
            ...headers,
        }).then(isError).then(toJson<Pick<IServicePrincipal, "id">[]>());

    // make sure we got one
    if (principalInfos.length < 1) {
        throw Error("Could not locate 'Microsoft Graph' service principal");
    }

    // now we need to assign scopes to the application so it can do what we need (sign in users and read their files)
    await fetch(`https://graph.microsoft.com/v1.0/oauth2PermissionGrants`, {
        body: JSON.stringify(<IOAuth2PermissionGrant>{
            clientId: createServicePrincipalResponseJson.id,
            consentType: "AllPrincipals",
            endTime: "9000-01-01T00:00:00",
            principalId: null,
            resourceId: principalInfos[0].id,
            scope: "openid Files.ReadWrite.All",
            startTime: "0001-01-01T00:00:00",
        }),
        method: "POST",
        ...headers,
    }).then(isError);

    log(`Added permission scopes.`);

    log(`Adding app secret...`);

    const addPasswordResponseJson = await fetch(`https://graph.microsoft.com/v1.0/applications/${createResponseJson.id}/addPassword`, {
        body: JSON.stringify(<{ displayName: string }>{
            displayName: "local-testing-secret",
        }),
        method: "POST",
        ...headers,
    }).then(isError).then(toJson<IPasswordCredential>());

    const appSecret = addPasswordResponseJson.secretText;

    log(`Added app secret.`);

    leave("CreateApp");

    return { displayName: appDisplayName, appId: createResponseJson.appId, id: createResponseJson.id, appSecret, created: true };
};

export default EnsureApp;

