import { TimelinePipe } from "@pnp/core";
import { _Picker } from "../picker.js";

export function RejectOnErrors(): TimelinePipe<_Picker> {

    return (instance: _Picker) => {

        instance.on.error(function (this: _Picker, err) {

            this.emit[this.InternalRejectEvent](err || null);
        });

        return instance;
    };
}
