import {
    Picker,
    IFilePickerOptions,
    INotificationData,
    IPickData,
} from "@pnp/picker-sdk";

import { getTokenWithScopes } from "./auth";

document.onreadystatechange = () => {
    if (document.readyState === "interactive") {
        const button: HTMLButtonElement = <any>document.getElementById("launchpicker");
        button.onclick = createWindow;
    }
};

const options: IFilePickerOptions = {
    sdk: "8.0",
    entry: {
        oneDrive: {}
    },
    authentication: {},
    messaging: {
        origin: "http://localhost:3000",
        channelId: "27"
    },
};

async function createWindow(e) {

    e.preventDefault();

    // setup the picker with the desired behaviors
    const picker = await Picker(window.open("", "Picker", "width=800,height=600"), {
        type: "Consumer",
        options,
        tokenFactory: () => getTokenWithScopes(["OneDrive.ReadWrite"]),
    });

    picker.addEventListener("pickernotifiation", (e: CustomEvent<INotificationData>) => {

        console.log("picker notification: " + JSON.stringify(e.detail));
    });

    picker.addEventListener("pickerchange", (e: CustomEvent<IPickData>) => {

        document.getElementById("pickerResults").innerHTML = `<pre>${JSON.stringify(e.detail, null, 2)}</pre>`
    });
}
