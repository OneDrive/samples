# Scenarios

Scenarios are larger samples that incorporate several technologies, APIs, or other capabilities to realize an end to end scenario. These are slightly more complex that the samples, but provide a fuller picture of what it takes to accomplish common integrations with OneDrive and SharePoint files.

## [List View Service Integration](./list-view-service-integration)

This example scenario demonstrates how to securely call an AAD secured remote service from an SPFx list view command set. This can be used to trigger automation, have the remote service perform actions as the user (SSO), or launch other applications to act on the file.

## [OneDrive Files Hooks](./onedrive-files-hooks)

This sample application shows how an application (desktop application) can connect with either OneDrive for Business (ODB) or the OneDrive consumer (ODC) service. Once connected a Microsoft Graph subscription is configured which calls into the application's web hook service. The web hook service will queue the change, allowing it to be picked up by a background service which processes the change notification by performing a delta query and getting the actual changes that happened on the user's OneDrive.

## [Avoid getting throttled by using RateLimit response headers](./throttling-ratelimit-handling/)

[SharePoint Online uses throttling](https://learn.microsoft.com/en-us/sharepoint/dev/general-development/how-to-avoid-getting-throttled-or-blocked-in-sharepoint-online) to maintain optimal performance and reliability of the SharePoint Online service. Throttling limits the number of API calls or operations within a time window to prevent overuse of resources. When your application gets throttled SharePoint Online returns a HTTP status code 429 ("Too many requests") or 503 ("Server Too Busy") and the requests will fail. In both cases, a Retry-After header is included in the response indicating how long the calling application should wait before retrying or making a new request.

In addition to the Retry-After header in the response of throttled requests, SharePoint Online also returns the [IETF RateLimit headers](https://github.com/ietf-wg-httpapi/ratelimit-headers) for selected limits in certain conditions to help applications manage rate limiting. We recommend applications to take advantage of these headers to avoid getting throttled and therefore achieve a better overall throughput for your application.
