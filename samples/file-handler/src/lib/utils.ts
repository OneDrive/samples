import { IncomingMessage } from "http";
import { StringDecoder } from "string_decoder";

export async function readRequestBody(req: IncomingMessage): Promise<string> {

    const buffer = await (new Promise<string>(resolve => {
        const decoder = new StringDecoder("utf-8");
        let b = "";

        req.on("data", (chunk) => {
            b += decoder.write(chunk);
        });

        req.on("end", () => {
            b += decoder.end();
            resolve(b);
        });
    }));

    return buffer;
}

export function toBase64(data: string): string {
    return Buffer.from(data, "utf8").toString("base64");
}

export function fromBase64(data: string): string {
    return Buffer.from(data, "base64").toString("utf8");
}
