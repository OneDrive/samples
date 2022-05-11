# Scenarios

Scenarios are larger samples that incorporate several technologies, APIs, or other capabilities to realize an end to end scenario. These are slightly more complex that the samples, but provide a fuller picture of what it takes to accomplish common integrations with OneDrive and SharePoint files.

## [List View Service Integration](./list-view-service-integration)

This example scenario demonstrates how to securely call an AAD secured remote service from an SPFx list view command set. This can be used to trigger automation, have the remote service perform actions as the user (SSO), or launch other applications to act on the file.

## [OneDrive Files Hooks](./onedrive-files-hooks)

This sample application shows how an application (desktop application) can connect with either OneDrive for Business (ODB) or the OneDrive consumer (ODC) service. Once connected a Microsoft Graph subscription is configured which calls into the application's web hook service. The web hook service will queue the change, allowing it to be picked up by a background service which processes the change notification by performing a delta query and getting the actual changes that happened on the user's OneDrive.

