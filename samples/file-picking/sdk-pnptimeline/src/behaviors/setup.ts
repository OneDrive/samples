import { TimelinePipe } from "@pnp/core";
import { _Picker } from "../picker.js";

export function Setup(): TimelinePipe<_Picker> {

    return (instance: _Picker) => {

        instance.on.init(function (this: _Picker) {

            window.addEventListener("message", (event) => {

                if (event.source && event.source === this.window) {

                    const message = event.data;

                    if (message.type === "initialize" && message.channelId === this.options.messaging.channelId) {

                        this.port = event.ports[0];

                        this.port.addEventListener("message", (event) => Reflect.apply(this.messageListener, this, [event]));

                        this.port.start();

                        this.port.postMessage({
                            type: "activate",
                        });
                    }
                }
            });
        });

        instance.on.dispose(function (this: _Picker) {

            if (this.port) {
                this.port.close();
            }
        });

        return instance;
    };
}
