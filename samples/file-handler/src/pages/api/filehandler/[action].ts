import { NextApiResponse } from "next";
import { DriveItem } from "@microsoft/microsoft-graph-types";

import { NextApiRequestWithSession } from "../../../lib/types";
import { initHandler } from "../../../lib/initHandler";
import { withSession } from "../../../lib/withSession";

// next API config (see: https://nextjs.org/docs/api-routes/api-middlewares#custom-config)
export const config = {
    api: {
        bodyParser: {
            sizeLimit: "10mb",
        },
    },
};

/**
 * Our API handler method to process incoming API requests
 * This matches in nextjs any call to /api/filehandler/[action]
 * 
 * @param req The incoming API request
 * @param res 
 */
const handler = async (req: NextApiRequestWithSession, res: NextApiResponse) => {

    const { action } = req.query;

    console.log(`In api/filehandler. Action: ${action}`);

    switch (action) {
        case "save":
            try {
                console.log("Entering handleSave");
                await handleSave(req, res);
                console.log("Leaving handleSave");
            } catch(e) {
                res.status(500).end(`Error in save action: ${e.message || e}`);
            }            
            break;
        default:
            res.status(404).end();
    }
};
export default withSession(handler);

/**
 * Handles saving the file contents back to the sever
 * 
 * @param req 
 * @param res 
 */
async function handleSave(req: NextApiRequestWithSession, res: NextApiResponse): Promise<void> {

    // get the information posted to us from the /pages/markdown/[action]
    const { content, fileUrl, requestId }: { content: string; fileUrl: string; requestId: string; } = req.body;

    console.log(`content: ${content}, fileUrl: ${fileUrl}, requestId: ${requestId}`);

    // get our token from the session
    const [token] = await initHandler(req as any, res);

    console.log(`token: ${token}`);

    // we validate our inputs
    if (typeof content === "undefined" || content.length < 1) {
        return res.status(400).end();
    }

    if (typeof fileUrl === "undefined" || fileUrl.length < 1) {
        return res.status(400).end();
    }

    if (typeof token === "undefined") {
        return res.status(500).end("No security token.");
    }

    console.log("getting item info");

    // we need to load up some details about the file to enable us to save it properly via the Microsoft Graph
    const itemInfoResponse = await fetch(fileUrl, {
        headers: {
            "authorization": `Bearer ${token}`,
        },
    });
    if (!itemInfoResponse.ok) {
        const errInfo = await itemInfoResponse.clone().text();
        console.error(errInfo);
        return res.status(500).end("Error getting item details before save.");
    }

    console.log("got item info");

    const itemInfo: DriveItem = await itemInfoResponse.clone().json();

    // construct a graph url to PUT our changes
    const contentUrl = `https://graph.microsoft.com/v1.0/drives/${itemInfo.parentReference.driveId}/items/${itemInfo.id}/content`;

    console.log(`updating: ${contentUrl}`);

    // update the file via Graph
    const updateResult = await fetch(contentUrl, {
        body: Buffer.from(content),
        headers: {
            "authorization": `Bearer ${token}`,
        },
        method: "PUT",
    });

    console.log("updated");

    // report on any errors
    if (!updateResult.ok) {
        const err = await updateResult.clone().text();
        // TODO:: log errors in telemetry
        console.error(err);
        return res.status(updateResult.status).end(`Update failed. Request Id: ${requestId}`);
    }

    // if it all went well we return a 200 from the api
    return res.status(200).end();
}
