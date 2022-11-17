import React, { useRef, useEffect } from "react";
import { IFileBrowserOptions, IAuthenticateCommand } from "../types";
import { combine } from "@pnp/core";

export interface BrowserProps {
    baseUrl: string;
    getToken: (message: IAuthenticateCommand) => Promise<string>;
    options: IFileBrowserOptions;
    onOpen?: (items: any[]) => void;
}

async function messageListener(getToken: (message: IAuthenticateCommand) => Promise<string>, port: MessagePort, message: any) {

    switch (message.data.type) {

        case "notification":
            console.log(`notification: ${JSON.stringify(message.data)}`);
            break;

        case "command":

            port.postMessage({
                type: "acknowledge",
                id: message.data.id,
            });

            const command = message.data.data;

            switch (command.command) {

                case "authenticate":

                    // getToken is from scripts/auth.js
                    const token = await getToken(command);

                    if (typeof token !== "undefined" && token !== null) {

                        port.postMessage({
                            type: "result",
                            id: message.data.id,
                            data: {
                                result: "token",
                                token,
                            }
                        });

                    } else {
                        console.error(`Could not get auth token for command: ${JSON.stringify(command)}`);
                    }

                    break;

                case "open":

                    port.postMessage({
                        type: "result",
                        id: message.data.id,
                        data: {
                            result: "success",
                        },
                    });

                    console.log(`OPEN >> ${JSON.stringify(command)}`);

                    alert(`Intercepted "open" command for .txt file: ${command.item.name}`);

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

        default:
            console.log("here");
            console.log(JSON.stringify(message));
            break;
    }
}

// file browser control
function Browser(props: BrowserProps) {

    const { baseUrl, getToken, options } = props;

    const iframeRef: React.MutableRefObject<HTMLIFrameElement | null> = useRef(null);

    useEffect(() => {

        (async () => {

            if (iframeRef && iframeRef.current !== null && iframeRef.current.contentWindow) {

                const { contentWindow } = iframeRef.current;

                const authToken = await getToken({
                    resource: baseUrl,
                    command: "authenticate",
                    type: "SharePoint",
                });

                const queryString = new URLSearchParams({
                    fileBrowser: JSON.stringify(options),
                });

                const url = combine(baseUrl, `_layouts/15/fileBrowser.aspx?${queryString}`);

                const form = contentWindow.document.createElement("form");
                form.setAttribute("action", url);
                form.setAttribute("method", "POST");
                contentWindow.document.body.append(form);

                const input = contentWindow.document.createElement("input");
                input.setAttribute("type", "hidden")
                input.setAttribute("name", "access_token");
                input.setAttribute("value", authToken);
                form.appendChild(input);

                form.submit();

                window.addEventListener("message", (event) => {

                    if (event.source && event.source === contentWindow) {

                        const message = event.data;

                        if (message.type === "initialize" && message.channelId === options.messaging.channelId) {

                            const port = event.ports[0];

                            port.addEventListener("message", messageListener.bind(null, getToken, port));

                            port.start();

                            port.postMessage({
                                type: "activate",
                            });
                        }
                    }
                });
            }

        })();

        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [iframeRef]);

    return (
        <iframe ref={iframeRef} title="browserFrame" id="browserFrame" width="100%" height="800"></iframe>
    );
}

export default Browser;
