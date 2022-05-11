# Example SDK - no dependencies

This example SDK uses no dependencies at run time to illustrate another pattern to wrap the picker. There is no requirement to use this wrapper, but it shows setting up and using the picker in a reusable way across solutions.

## Install

This package is not published, to use it with the local samples you need to execute `npm build` in this project.


## Usage


// setup the picker with the desired behaviors
    const picker = await LoadPicker(window.open("", "Picker", "width=800,height=600"), {
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

## Configurations

There are two ways to configure this example SDK

### OneDrive / SharePoint



### OneDrive Consumer/Personal




