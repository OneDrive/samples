import { AzureFunction, Context, HttpRequest,  } from "@azure/functions"
import { readFileSync } from "fs";
import { resolve } from "path";

const httpTrigger: AzureFunction = async function (context: Context, req: HttpRequest): Promise<void> {

    const buf = readFileSync(resolve(__dirname, "../../IconServer/icon.svg"));

    context.res = {
        headers: {
            "Content-Type": "image/svg+xml",
        },
        body: buf,
    };
};

export default httpTrigger;