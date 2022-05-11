import { log, enter, leave } from "../logger";
import { fetch, getHeaders, isError } from "../fetch";
import { v4 } from "uuid";

// defines the actions in our manifest
const actions = [
    {
        "type": "newFile",
        "url": "https://localhost:3000/markdown/create",
        // tslint:disable-next-line: object-literal-sort-keys
        "availableOn": {
            "file": {
                "extensions": [
                    ".md",
                ],
            },
            "web": {},
        },
    },
    {
        "type": "open",
        "url": "https://localhost:3000/markdown/edit",
        // tslint:disable-next-line: object-literal-sort-keys
        "availableOn": {
            "file": {
                "extensions": [
                    ".md",
                ],
            },
            "web": {

            },
        },
    },
    {
        "type": "preview",
        "url": "https://localhost:3000/markdown/preview",
        // tslint:disable-next-line: object-literal-sort-keys
        "availableOn": {
            "file": {
                "extensions": [
                    ".md",
                ],
            },
            "web": {

            },
        },
    },
];

// tslint:disable: object-literal-sort-keys
const fileTypeIcon = {
    svg: "https://localhost:3000/images/icons/icon.svg",
    png1x: "https://localhost:3000/images/icons/icon@1x.png",
    "png1.5x": "https://localhost:3000/images/icons/icon@1.5x.png",
    png2x: "https://localhost:3000/images/icons/icon@2x.png",
};

// tslint:disable: object-literal-sort-keys
const appIcon = {
    svg: "https://localhost:3000/images/icons/app-icon.svg",
    png1x: "https://localhost:3000/images/icons/app-icon@1x.png",
    "png1.5x": "https://localhost:3000/images/icons/app-icon@1.5x.png",
    png2x: "https://localhost:3000/images/icons/app-icon@2x.png",
};

// defines the body of the manifest
const defaultManifest = {
    "id": v4(),
    "properties": [
        {
            "key": "version",
            "value": "2",
        },
        {
            "key": "fileTypeDisplayName",
            "value": "Contoso Markdown",
        },
        {
            "key": "fileTypeIcon",
            // tslint:disable-next-line: max-line-length
            "value": JSON.stringify(fileTypeIcon),
        },
        {
            "key": "appIcon",
            // tslint:disable-next-line: max-line-length
            "value": JSON.stringify(appIcon),
        },
        {
            "key": "actions",
            // tslint:disable-next-line: max-line-length
            "value": JSON.stringify(actions),
        },
    ],
    "type": "FileHandler",
};

const InjectManifest = async (token: string, objectId: string) => {

    enter("InjectManifest");

    log(`Injecting manifest into app: ${objectId}. This operation will clear all previous manifests added for this app.`);

    await fetch(`https://graph.microsoft.com/v1.0/applications/${objectId}/addIns`,
        {
            body: JSON.stringify({
                value: [defaultManifest],
            }),
            method: "PUT",
            ...getHeaders(token),
        }).then(isError);

    log("Injected manifest successfully.");

    leave("InjectManifest");
};

export default InjectManifest;
