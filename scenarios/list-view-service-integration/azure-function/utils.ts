import fetch, { RequestInit } from "node-fetch";

export function encodeSharingUrl(url: string): string {
    return "u!" + Buffer.from(url, "utf8").toString("base64").replace(/=$/i, "").replace("/", "_").replace("+", "-");
}

export function requestBinder(token: string): <T = any>(url: string, init?: RequestInit) => Promise<T> {

    return async function <T>(url: string, init: RequestInit = {}): Promise<T> {

        if (!init.headers) {
            init.headers = {};
        }

        init.headers["authorization"] = `Bearer ${token}`;

        const result = await fetch(url, init);

        if (!result.ok) {
            const errorBody = await result.text();
            throw Error(`[${result.status}] ${result.statusText} ${errorBody}`);
        }

        return result.json();
    }
}

export function getRandomString(chars: number): string {
    const text = new Array(chars);
    for (let i = 0; i < chars; i++) {
        text[i] = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789".charAt(Math.floor(Math.random() * 62));
    }
    return text.join("");
}
