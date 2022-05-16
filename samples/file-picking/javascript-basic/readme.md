# javascript-basic

A single page example showing full interaction with the picker through plain JavaScript on a single page with no external dependencies.

## Getting Started

1. Follow the guidance in the [root readme](../README.md#required-setup) for setting up the AAD application
2. Update the `baseUrl` value in index.html with a valid absolute URL to either the SharePoint or OneDrive instance the picker should surface.
3. Update the `msalParams` value in [scripts/auth.js](./scripts/auth.js) with valid configuration including authority and clientId

> Additional details on these values can be found in [the documentation](https://aka.ms/OneDrive/file-picker)

## Running this Sample

After updating the required values you need to:

1. Run `npm install` - the only dev-dependency is the express server
2. Run `npm start` - this will launch the server using the current directory as the static source
3. Navigate in a browser to `http://localhost:3000`
4. Select the `Launch Picker` button, choose some files, and select `Select`

## Next Steps

You can modify the `params` configuation in [index.html](./index.html) to try out different configuration options to see how the picker behaves. Full information on the options is available in [the documentation](https://aka.ms/OneDrive/file-picker). 
