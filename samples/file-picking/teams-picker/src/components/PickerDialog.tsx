import React, { useEffect, useState, useContext } from "react";
import { dialog } from "@microsoft/teams-js";
import { combine, getGUID } from "@pnp/core";
import { TeamsFxContext } from "./Context";
import { BearerTokenAuthProvider, createApiClient } from "@microsoft/teamsfx";

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
        origin: "https://localhost:53000",
        channelId: getGUID(),
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
const baseUrl = "https://{tenant}-my.sharepoint.com";

export function PickerDialog() {

    const [contentWindow, setContentWindow] = useState<Window | null>(null);

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

            const functionName = process.env.REACT_APP_FUNC_NAME;
            const functionEndpoint = process.env.REACT_APP_FUNC_ENDPOINT;
            const apiClient = createApiClient(`${functionEndpoint}/api/`, new BearerTokenAuthProvider(async () => (await teamsUserCredential.getToken(""))!.token));
            const url = combine(baseUrl, `_layouts/15/FilePicker.aspx?${queryString}`);

            contentWindow.parent.addEventListener("message", (event) => {

                if (event.source && event.source === contentWindow) {

                    const message = event.data;

                    if (message.type === "initialize" && message.channelId === params.messaging.channelId) {

                        const port = event.ports[0];

                        port.addEventListener("message", async (message: any) => {

                            console.log("message" + JSON.stringify(message));

                            switch (message.data.type) {

                                case "notification":
                                    console.log(`notification: ${message.data}`);
                                    break;

                                case "command":

                                    port.postMessage({
                                        type: "acknowledge",
                                        id: message.data.id,
                                    });

                                    const command = message.data.data;

                                    switch (command.command) {

                                        case "authenticate":

                                            const { data: token2 } = await apiClient.get(`${functionName}?resource=${command.resource}`);

                                            if (typeof token2 !== "undefined" && token2 !== null) {

                                                port.postMessage({
                                                    type: "result",
                                                    id: message.data.id,
                                                    data: {
                                                        result: "token",
                                                        token: token2,
                                                    }
                                                });

                                            } else {
                                                console.error(`Could not get auth token for command: ${JSON.stringify(command)}`);
                                            }

                                            break;

                                        case "close":

                                            dialog.url.submit("");
                                            break;

                                        case "pick":

                                            port.postMessage({
                                                type: "result",
                                                id: message.data.id,
                                                data: {
                                                    result: "success",
                                                },
                                            });

                                            dialog.url.submit(JSON.stringify(command));

                                            break;

                                        default:

                                            console.warn(`Unsupported command: ${JSON.stringify(command)}`, 2);

                                            port.postMessage({
                                                result: "error",
                                                error: {
                                                    code: "unsupportedCommand",
                                                    message: command.command
                                                },
                                                isExpected: true,
                                            });
                                            break;
                                    }

                                    break;
                            }
                        });

                        port.start();

                        port.postMessage({
                            type: "activate",
                        });
                    }
                }
            });

            (async () => {

                const { data: token } = await apiClient.get(`${functionName}?resource=${baseUrl}`);

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

        <iframe title="File Picker" id="frame" width="100%" height="100%" sandbox="allow-same-origin allow-top-navigation allow-forms allow-scripts"></iframe>

    </div>);
}
