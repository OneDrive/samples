import fetch, { Response } from "node-fetch";
import { log } from "./logger";

export { fetch };

export const getHeaders = (token) => ({
    headers: {
        "authorization": `Bearer ${token}`,
        "content-type": "application/json",
    },
});

export async function isError(response: Response): Promise<Response | never> {
    if (!response.ok) {
        const txt = await response.clone().text();
        log(`Error: ${txt}`);
        throw Error(txt);
    }
    return response;
}

export function toJson<T = any>(parser?: (json: any) => T) {
    return async (response: Response): Promise<T> => {
        const json: { value: T } = await response.clone().json();
        return parser ? parser(json) : json.value ? json.value : <T><unknown>json;
    };
}
