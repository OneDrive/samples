import React from "react";
import { authentication } from "@microsoft/teams-js";
import { combine } from "@pnp/core";
import { MouseEvent, useEffect, useState, useContext } from "react";
import { useData } from "@microsoft/teamsfx-react";
import { TeamsFxContext } from "./Context";
import { TeamsUserCredential } from "@microsoft/teamsfx";

/**
 * These could be passed in to PickerDialog as props, or otherwise made dynamic. For
 * this sample we make them static for simplicity.
 */
const params = {
    sdk: "8.0",
    entry: {
        oneDrive: {
            files: {},
        }
    },
    authentication: {},
    messaging: {
        origin: "http://localhost:53000",
        channelId: "27"
    },
    typesAndSources: {
        mode: "files",
        pivots: {
            oneDrive: true,
            recent: true,
        },
    },
}

// For this sample we hard code it, you can get it various ways in your application
const baseUrl = "https://318studios-my.sharepoint.com";

export function PickerDialog() {

    const [contentWindow, setContentWindow] = useState<Window | null>(null);

    // const submit = () => {
    //     dialog.url.submit("hello");
    // }

    const { teamsUserCredential } = useContext(TeamsFxContext);

    useEffect(() => {

        const frame = (document.getElementById("frame") as HTMLIFrameElement);
        setContentWindow(frame.contentWindow);

    }, []);

    useEffect(() => {

        if (teamsUserCredential && contentWindow) {

            const queryString = new URLSearchParams({
                filePicker: JSON.stringify(params),
            });

            const url = combine(baseUrl, `_layouts/15/FilePicker.aspx?${queryString}`);

            (async () => {

                const { token } = await teamsUserCredential.getToken([]);

                const form = contentWindow.document.createElement("form");
                form.setAttribute("action", url);
                form.setAttribute("method", "POST");
                contentWindow.document.body.append(form);

                const input = contentWindow.document.createElement("input");
                input.setAttribute("type", "hidden")
                input.setAttribute("name", "access_token");
                input.setAttribute("value", token);
                form.appendChild(input);

                form.submit();

            })();
        }

    }, [contentWindow, teamsUserCredential]);

    return (<div>
        <h1>Picker Render</h1>

        <iframe title="File Picker" id="frame"></iframe>

    </div>);
}
