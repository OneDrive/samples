# OneDrive File Browser v8 Samples

This folder contains samples for using the [OneDrive/SharePoint File Browser](https://aka.ms/OneDrive/file-browser).

## Sample Integrations

|Name|Description|
|---|---|
[javascript-basic](./javascript-basic/readme.md)|Demonstrates using no SDKs/TypeScript how to interact with the file browser within a JavaScript based SPA|
[typescript-react](./typescript-react/readme.md)|Shows the using the browser within a React application.|

## Example Configurations

These are a collection of [example configuration files](./example-browser-configs/readme.md) to assist in understanding how to configure the browser. Full details can be found in [the documentation](https://aka.ms/OneDrive/file-browser) to create any configuration you need, these cover common scenarios.

## Required Setup

To run the samples you will need to create an AAD application. This is the application you will use to get tokens for reading the files. You can follow these steps:

1. Create a new AAD App Registration, note the ID of the application
2. Under authentication, create a new Single-page application registry
   1. Set the redirect uri to https://localhost (this is for testing the samples)
   2. Ensure both Access tokens and ID tokens are checked
   3. You may optionally configure this application for multitenant but this is outside the scope of this article
3. Under API permissions
   1. Add Files.Read.All, Sites.Read.All, Leave User.Read for Graph delegated permissions
   2. Add AllSites.Read, MyFiles.Read for SharePoint delegated permissions

> When connecting to OneDrive Personal, you will request the `OneDrive.ReadWrite` scope which cooresponds to `Files.ReadWrite.All` or `Files.ReadWrite`.
