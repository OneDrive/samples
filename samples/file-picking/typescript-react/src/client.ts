import { combine } from "@pnp/core";
import { getTokenWithScopes } from "./auth";

export async function request<T = any>(path: string, init?: RequestInit): Promise<T> {

    path = combine("https://graph.microsoft.com/v1.0/", path);

    const token = await getTokenWithScopes(["https://graph.microsoft.com/.default"]);

    const ini = {
        method: "GET",
        headers: {
            "Authorization": `Bearer ${token}`,
            "Content-Type": "application/json",
        },
        ...init,
    }

    if (typeof init !== "undefined" && init?.headers) {
        ini.headers = { ...ini.headers, ...init.headers };
    }

    const response = await fetch(path, ini);

    if (!response.ok) {
        throw Error(`[${response.status}] ${response.statusText}`);
    }

    if (response.status !== 204) {

        return response.json();
    }
}