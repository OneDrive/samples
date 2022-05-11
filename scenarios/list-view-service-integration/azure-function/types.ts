import { HttpRequest } from "@azure/functions"

export interface SPFxUserInfo {
    /**
    * The display name for the current user.
    *
    * @remarks
    * Example: `"John Doe"`
    */
    readonly displayName: string;
    /**
     * The email address for the current user.
     *
     * @remarks
     * Example: `"example@contoso.com"`
     */
    readonly email: string;
    /**
     * Returns if the current user is an anonymous guest.
     */
    readonly isAnonymousGuestUser: boolean;
    /**
     * Returns true if the current user is an external guest.
     */
    readonly isExternalGuestUser: boolean;
    /**
     * The login name for current user.
     *
     * @remarks
     * Example: on-premise user: `"domain\user"`, online user: `"user@domain.com"`
     */
    readonly loginName: string;
    /**
     * This boolean represents if a the user or web's time zone settings should be used
     * to display the current time.
     */
    readonly preferUserTimeZone: boolean;
    /* Excluded from this release type: timeZoneInfo */
    /* Excluded from this release type: firstDayOfWeek */
    /* Excluded from this release type: __constructor */
}

export interface InvocationHttpRequest extends HttpRequest {
    body: InvocationRequestBody;
    headers: {
        "x-ms-client-principal-name": string;
        "x-ms-client-principal-id": string,
        "x-ms-token-aad-id-token": string;
        "x-ms-token-aad-access-token": string;
        "x-ms-token-aad-expires-on": string;
        "x-ms-token-aad-refresh-token": string;
    }
}

export interface AADInfo {
    instanceUrl: string;
    tenantId: string;
    userId: string;
}

export interface InvocationRequestBody {
    user: SPFxUserInfo;
    siteId: string;
    siteUrl: string;
    webId: string;
    webRelUrl: string;
    webAbsUrl: string;
    listId: string;
    listUrl: string;
    listTitle: string;
    aadInfo: AADInfo;
}

export interface QueueMessage {

    request: InvocationRequestBody

    auth: {
        requestBearerToken: string;
    }
}

