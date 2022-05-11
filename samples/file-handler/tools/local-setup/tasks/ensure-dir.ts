import { existsSync, mkdirSync } from "fs";
import { resolve } from "path";
import { log, enter, leave } from "../logger";

export const dirName = ".local-dev-secrets";

const EnsureDevSecretsFolder = (root: string): string => {

    enter("EnsureDevSecretsFolder");

    const resolvedPath = resolve(root, dirName);

    log(`Resolved Path: ${resolvedPath}`);

    if (!existsSync(resolvedPath)) {
        log("Folder does not exist, creating...");
        mkdirSync(resolvedPath);
        log(`Folder created at ${resolvedPath}.`);
    } else {
        log("Folder exists, no action taken.");
    }

    leave("EnsureDevSecretsFolder");

    return resolvedPath;
};

export default EnsureDevSecretsFolder;
