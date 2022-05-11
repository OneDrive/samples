import { TimelinePipe } from "@pnp/core";
import { _Picker } from "../picker.js";

export function ResolveWithPicks(): TimelinePipe<_Picker> {

    return (instance: _Picker) => {

        instance.on.pick(async function (this: _Picker, data) {

            this.emit[this.InternalResolveEvent](data);
        });

        return instance;
    };
}
