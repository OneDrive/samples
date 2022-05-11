import { TimelinePipe } from "@pnp/core";
import { Setup } from "./setup.js";
import { PickObserver, _Picker } from "../picker.js";
import { LogNotifications } from "./log-notifications.js";

export function Embed(onPick: PickObserver): TimelinePipe<_Picker> {

    return (instance: _Picker) => {

        instance.using(
            Setup(),
            LogNotifications(),
        );

        instance.on.pick(onPick);

        return instance;
    };
}
