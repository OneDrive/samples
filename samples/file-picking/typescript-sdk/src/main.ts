import { get } from "env-var";

// this makes use of the local example sdk, see ../
import {
    Picker,
    MSALAuthenticate,
    IFilePickerOptions,
    IPicker,
    Popup,
} from "@pnp/picker-api";

document.onreadystatechange = () => {
    if (document.readyState === "interactive") {
        const button: HTMLButtonElement = <any>document.getElementById("launchpicker");
        button.onclick = createWindow;
    }
};

const options: IFilePickerOptions = {
    sdk: "8.0",
    entry: {
        oneDrive: {},
    },
    authentication: {},
    messaging: {
        origin: "http://localhost:3000",
        channelId: "27"
    },
    selection: {
        mode: "multiple",
        maxCount: 5,
    },
    typesAndSources: {
        mode: "all",
        pivots: {
            recent: true,
            oneDrive: true,
        },
    },
};

const clientId = get("CLIENT_ID").required().asString();
const clientAuthority = get("CLIENT_AUTHORITY").required().asString();
const baseUrl = get("BASE_URL").required().asString();

const msalParams = {
    auth: {
        authority: clientAuthority,
        clientId,
        redirectUri: "http://localhost:3000"
    },
}

async function createWindow(e) {

    e.preventDefault();

    // setup the picker with the desired behaviors
    const picker = Picker(window.open("", "Picker", "width=800,height=600")).using(
        Popup(),
        MSALAuthenticate(msalParams),
    );

    // optionally log notifications to the console
    picker.on.notification(function (this: IPicker, message) {
        console.log("notification: " + JSON.stringify(message));
    });

    // optionially log any logging from the library itself to the console
    picker.on.log(function (this: IPicker, message, level) {
        console.log(`log: [${level}] ${message}`);
    });

    // activate the picker with our baseUrl and options object
    const results = await picker.activate({
        baseUrl,
        options,
    });

    document.getElementById("pickedFiles").innerHTML = `<pre>${JSON.stringify(results, null, 2)}</pre>`;
}
