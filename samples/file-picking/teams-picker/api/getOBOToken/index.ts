import { AzureFunction, Context, HttpRequest } from "@azure/functions"
import {
    OnBehalfOfCredentialAuthConfig,
    OnBehalfOfUserCredential
} from "@microsoft/teamsfx";

const httpTrigger: AzureFunction = async function (context: Context, req: HttpRequest, teamsfxContext: { [key: string]: any }): Promise<void> {

    const accessToken = teamsfxContext["AccessToken"];

    const { resource } = req.query;

    const oboAuthConfig: OnBehalfOfCredentialAuthConfig = {
        authorityHost: process.env.M365_AUTHORITY_HOST,
        tenantId: process.env.M365_TENANT_ID,
        clientId: process.env.M365_CLIENT_ID,
        clientSecret: process.env.M365_CLIENT_SECRET,
    };

    const oboCredential = new OnBehalfOfUserCredential(accessToken, oboAuthConfig);

    const token = await oboCredential.getToken(`${resource}/.default`);

    context.res = {
        body: token.token
    };
};

export default httpTrigger;