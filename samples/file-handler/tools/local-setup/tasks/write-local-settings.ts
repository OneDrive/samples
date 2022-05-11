import { log, enter, leave } from "../logger";
import { writeFileSync } from "fs";
import {resolve } from "path";

const WriteLocalSettings = async (path: string, tenantId: string, appId: string, appSecret: string) => {

    enter("InjectManifest");

    log(`Writing settings file.`);

    const file = [];
    file.push(`IRON_SESSION_PASSWORD='password'`);
    file.push(`NODE_ENV='development'`);
    file.push(`AAD_MSAL_AUTH_TENANT_ID="${tenantId}"`);
    file.push(`AAD_MSAL_AUTH_ID="${appId}"`);
    file.push(`AAD_MSAL_AUTH_SECRET="${appSecret}"`);
    file.push(`FILEHANDLER_SITE_HOST_URL="https://localhost:3000"`);

    writeFileSync(resolve(path, ".env.local"), file.join("\n") + "\n");

    log(`Wrote settings file.`);

    leave("InjectManifest");
};

export default WriteLocalSettings;
