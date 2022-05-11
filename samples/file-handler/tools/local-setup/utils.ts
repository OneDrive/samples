import { randomBytes } from "crypto";

export function randomString(length: number): string {
    return randomBytes(length).toString("base64").slice(0, length);
}
