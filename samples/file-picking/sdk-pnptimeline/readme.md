# Example SDK

This package demonstrates a way to wrap and interact with the picker control. There is no requirement to use this wrapper, but it shows setting up and using the picker in a reusable way across solutions.

It relies on the [@pnp/core Timeline](https://pnp.github.io/pnpjs/core/timeline/) to orchestrate the messaging. There is also [a sample sdk with no dependencies](../sdk-domevents/readme.md).

## Overview

This package provides a wrapper for the picker and exposes a way to register various behaviors to control how the interaction occurs. These [behaviors are described below](#behaviors), and you are encouraged to create your own behaviors as needed.

The picker provides eight subscribable events, listed below in the table. Generally you will use behaviors, but directly registering is always an option.

|name|desacription|
|---|---|
|authenticate|emits any time a token is required by the picker|
|pick|emits when a user picks items and selects the Select button|
|close|emits when the user selects the Cancel button|
|notification|emits when the picker sends a notification|
|log|emits when there is a log message|
|error|emits when there is an error within the picker|
|init|emits as the first event when the picker is being established|
|dispose|emits as the final event in the picker lifecycle|

All events are registered through the "on" property:

```TypeScript
picker.on.close(...);

picker.on.authenticate(...);
```

## Build Example SDK

To reference this library from another sample solution:

1. In this repository run `npm run watch` to start up tsc in watch mode or `npm run build` to transpile the source
2. In the solution where you want to reference this package, use `npm install file://path/to/picker-ts-api --save`

## Initialize

The picker is initialized by specifying the window into which the picker should be rendered, and optionally an app and scenario.

```TypeScript
import {
    Picker,
    MSALAuthenticate,
    LogNotifications,
    IFilePickerOptions,
    IPicker,
    Popup,
} from "@pnp/picker-api";

const msalParams = {
    auth: {
        authority: "{client id authority}",
        clientId: "{client id}",
        redirectUri: "http://localhost:3000",
    },
}

const pickerInitParams = {
    // represents the set of init params as discussed in the main docs article
}

// setup the picker with the desired behaviors
// here we use the built-in behaviors "Popup" and "MSALAuthenticate" described below
const picker = Picker(window.open("", "Picker", "width=800,height=600")).using(
    Popup(),
    LogNotifications(),
    MSALAuthenticate(msalParams),
);

// optionially log any logging to the console (or any log sink)
picker.on.log(function (this: IPicker, message, level) {
    console.log(`log: [${level}] ${message}`);
});


// activate the picker with our baseUrl and options object
// because we used the "Popup" behavior the activate promise will resolve once the user picks
// items or cancels.
const results = await picker.activate({
    baseUrl: "https://tenant.sharepoint.com/sites/dev",
    options: pickerInitParams,
});
```

## Behaviors

The picker wrapper comes with a set of behaviors included to illustrate various ways to interact with the picker.

> Some behaviors compose multiple behaviors to make registration easier. These are tagged below with (composed)

### Embed (composed)

This behavior is used when establishing the picker within an iframe. It is composed of the Setup behavior and the supplied pick handler. When running in embed mode the activate promise never resolves by default.

> Note that you must provide an authentication behavior in addition to these defaults.

```TypeScript
import {
    Embed,
    IPickData,
} from "@pnp/picker-api";

picker.using(Embed((pickedItems: IPickData) => {

    console.log(`picked: ${JSON.stringify(pickedItems)}`);
}));
```

### RejectOnErrors

Used as part of the "Popup" composed behavior. Causes the activate promise to reject on any errors within the picker.

### LamdaAuthenticate

Provides a behavior to use when you already have an authentication system and do not want to make use of the `MSALAuthenticate` behavior. This behavior allows you to inject your funcitonality into the wrapper through a function which asyncronously returns a string, representing a valid token.

```TypeScript
import {
    IAuthenticateCommand,
} from "@pnp/picker-api";

function async getToken(command: IAuthenticateCommand): Promise<string> {

    const { resource } = command;

    const token = await {your code to get token};

    return token;
}

picker.using(LamdaAuthenticate(getToken));
```

### LogNotifications

Allows you to automatically log any notifications to the picker's log event. You can optionally register an observer on the notification event yourself.

> This is included in both the Embed and Popup composed behaviors.

```TypeScript
import {
    LogNotifications,
} from "@pnp/picker-api";

picker.using(LogNotifications());
```

### MSALAuthenticate

This behavior sets up an MSAL instance to provide tokens to the picker. It requires the standard MSAL init configuration, and is a thin wrapper around the `@azure/msal-browser` library.

> If you are already creating tokens for your application, consider using the LamdaAuthenticate behavior and wrapping your existing functionality.

```TypeScript
import {
    MSALAuthenticate,
} from "@pnp/picker-api";

const msalParams = {
    auth: {
        authority: "{client id authority}",
        clientId: "{client id}",
        redirectUri: "http://localhost:3000",
    },
}

picker.using(MSALAuthenticate(msalParams));
```

### Close

This behavior is used as part of the Popup composed behavior to close the window when a user selects the Cancel button.

```TypeScript
import {
    Close,
} from "@pnp/picker-api";

picker.using(Close());
```

### CloseOnPick

This behavior is used as part of the Popup composed behavior to close the window when a user selects items.

```TypeScript
import {
    CloseOnPick,
} from "@pnp/picker-api";

picker.using(CloseOnPick());
```

### Popup (composed)

This behavior is used to easily configure a scenario where a popup is used for the picker. It is composed of the Setup, Close, LogNotifications, ResolveWithPicks, and CloseOnPick behaviors.

> Note that you must provide an authentication behavior in addition to these defaults.

```TypeScript
import {
    Popup,
} from "@pnp/picker-api";

picker.using(Popup());
```

### ResolveWithPicks

This behavior resolves the picker's activate promise with the picks made by the user. Once the promise is resolved the picker is done and no further interaction is possible.

```TypeScript
import {
    ResolveWithPicks,
} from "@pnp/picker-api";

picker.using(ResolveWithPicks());
```
