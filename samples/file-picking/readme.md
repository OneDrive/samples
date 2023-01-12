# OneDrive File Picker v8 Samples

This folder contains samples for using the [OneDrive/SharePoint File Picker](https://aka.ms/OneDrive/file-picker).

## Sample Integrations

|Name|Description|
|---|---|
[csharp-basic](./csharp-basic/readme.md)|Shows the file picking and file browsing controls using an .NET 6.0 ASP.NET Core application. For getting access tokens a call to the ASP.NET Core server is made.|
[javascript-basic](./javascript-basic/readme.md)|Demonstrates using no SDKs/TypeScript how to interact with the file picker within a JavaScript based SPA|
[typescript-react](./typescript-react/readme.md)|Samples using the [example sdk](./sdk-/readme.md) within a React application. Includes examples of acting on the picked information to preview delete, download, or copy a link to the picked files.|
[typescript-sdk](./typescript-sdk/readme.md)|Sample using TypeScript along with the [pnp timeline based sdk](./sdk-timeline/readme.md).|
[typescript-sdk-odc](./typescript-sdk-odc/readme.md)|Sample using TypeScript along with the [example no-dependencies sdk](./sdk-domevents/readme.md) to pick OneDrive Consumer (Personal) files.|

## Sample SDKs

While it is not required to use an SDK when working with the File Picker, it can help to wrap the logic behind abstractions. These examples show techniques you can use in your application to make the picker interactions reusable across solutions.

|Name|Description|
|---|---|
[sdk-domevents](./sdk-domevents/readme.md)|SDK with no external dependencies written in TypeScript using native dom events to provide updates on picked files|
[sdk-timeline](./sdk-timeline/readme.md)|Uses the PnPjs Timeline to wrap the picker and respond to messages in a more robust/pluggable way, but with added complexity|

## Example Configurations

These are a collection of [example configuration files](./example-picker-configs/readme.md) to assist in understanding how to configure the picker. Full details can be found in [the documentation](https://aka.ms/OneDrive/file-picker) to create any configuration you need, these cover common scenarios.

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

## FAQ

<details>
  <summary>How can I determine the base URL?</summary>

  - To get the url of a user's main OneDrive you can use `https://graph.microsoft.com/v1.0/me/drive` and parse the webUrl property.

  - For referencing a SharePoint site, you'd need to know what site/library you wanted to explorer. You could possibly use the https://graph.microsoft.com/v1.0/me/followedSites call for sites the current user follows.

</details>

<details>
  <summary>Can I show the "Shared" menu item?</summary>

  Currently no, due to internal limitations with the control setting the "sharedLibraries" pivot the Quick Access section is shown, but not shared. This is a known gap between the service and 3P capabilities for which there is not currently a timeline to address.

</details>
