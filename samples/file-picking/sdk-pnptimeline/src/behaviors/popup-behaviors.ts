import { TimelinePipe } from "@pnp/core";
import { LogNotifications } from "./log-notifications.js";
import { _Picker } from "../picker.js";
import { ResolveWithPicks } from "./resolves.js";
import { Setup } from "./setup.js";

export function Close(): TimelinePipe<_Picker> {

    return (instance: _Picker) => {

        instance.on.close(function (this: _Picker) {

            this.emit[this.InternalResolveEvent](null);
            this.window.close();
        });

        return instance;
    };
}

export function CloseOnPick(): TimelinePipe<_Picker> {

    return (instance: _Picker) => {

        instance.on.pick(async function (this: _Picker, data) {

            this.window.close();
        });

        return instance;
    };
}

export function Popup(): TimelinePipe<_Picker> {

    return (instance: _Picker) => {

        instance.using(
            Setup(),
            Close(),
            LogNotifications(),
            ResolveWithPicks(),
            CloseOnPick(),
        );

        return instance;
    };
}
