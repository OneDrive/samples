import {
    Picker,
    IFilePickerOptions,
    INotificationData,
    IPickData,
    OneDriveConsumerInit,
    ODSPInit,
    IAuthenticateCommand,
} from "@pnp/picker-sdk";

import { getToken } from "./auth";

document.onreadystatechange = () => {
    if (document.readyState === "interactive") {
        const button: HTMLButtonElement = <any>document.getElementById("launchpicker");
        button.onclick = launchOneDrivePicker;
    }
};

const sharepoint_baseUrl = "https://tenant-my.sharepoint.com";

async function launchOneDrivePicker(e) {

    e.preventDefault();

    const accountType: "Consumer" | "ODSP" = "Consumer";

    const init: OneDriveConsumerInit | ODSPInit = getOneDriveInit(accountType);

    // setup the picker with the desired behaviors
    const picker = await Picker(window.open("", "Picker", "width=800,height=600"), init);

    picker.addEventListener("pickernotifiation", (e: CustomEvent<INotificationData>) => {

        console.log("picker notification: " + JSON.stringify(e.detail));
    });

    picker.addEventListener("pickerchange", (e: CustomEvent<IPickData>) => {

        document.getElementById("pickerResults").innerHTML = `<pre>${JSON.stringify(e.detail, null, 2)}</pre>`
    });
}
function getFilePickerOptions(accountType: string): IFilePickerOptions {
    if (accountType == "Consumer") {
        return {
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

    } else {
        return {
            sdk: "8.0",
            entry: {
                sharePoint: {
                    byPath: {
                        web: sharepoint_baseUrl,
                        list: "SitePages"
                    }
                }
            },
            authentication: {},
            messaging: {
                origin: "http://localhost:3000",
                channelId: "27"
            },
            typesAndSources: {
                mode: "all",
                filters: [".aspx"],
                pivots: {
                    recent: false,
                    oneDrive: false
                }
            }
        };
    }
}

function getOneDriveInit(accountType: string): OneDriveConsumerInit | ODSPInit {

    const options: IFilePickerOptions = getFilePickerOptions(accountType);
    
    let authCmd : IAuthenticateCommand = null;

    if (accountType == "Consumer") {
        authCmd = {
            command: "authenticate",
            type: "Graph",
            resource: "",
        }
        const init: OneDriveConsumerInit = {
            type: "Consumer",
            options,
            tokenFactory: () => getToken(authCmd),
        };

        return init;

    } else {

        authCmd = {
            command: "authenticate",
            type: "SharePoint",
            resource: sharepoint_baseUrl,
        }

        const init: ODSPInit = {
            type: "ODSP",
            baseUrl: sharepoint_baseUrl,
            options,
            tokenFactory: () => getToken(authCmd),
        };
        return init;
    }
}

