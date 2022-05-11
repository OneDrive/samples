import { _Picker } from "./picker.js";

export * from "./behaviors/popup-behaviors.js";
export * from "./behaviors/setup.js";
export * from "./behaviors/msal-authenticate.js";
export * from "./behaviors/lamda-authenticate.js";
export * from "./behaviors/resolves.js";
export * from "./behaviors/errors.js";
export * from "./behaviors/embed-behaviors.js";
export * from "./behaviors/log-notifications.js";

export {
    IFilePickerOptions,
    IAuthenticateCommand,
    IPickData,
    SPItem,
} from "./types.js";

export type PickerInit = [];

export function Picker(window: Window): _Picker {

    if (typeof window === "undefined") {
        throw Error("You must supply a valid Window for the picker to render within.");
    }

    return new _Picker(window);
}

export type IPicker = ReturnType<typeof Picker>;
