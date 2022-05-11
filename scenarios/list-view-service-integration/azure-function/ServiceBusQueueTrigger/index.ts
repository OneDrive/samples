import { AzureFunction, Context } from "@azure/functions"
import { QueueMessage } from "../types";
import { getOnBehalfToken } from "../auth";
import { encodeSharingUrl, requestBinder, getRandomString } from "../utils";

const serviceBusQueueTrigger: AzureFunction = async function (context: Context, msgStr: string): Promise<void> {

    try {

        // parse our passed in message
        const message: QueueMessage = JSON.parse(msgStr);

        // we need to exchange our token for one to call the graph
        const onBehalfToken = await getOnBehalfToken(message.request.aadInfo.tenantId, message.auth.requestBearerToken);

        // create a request engine bound to our token
        const request = requestBinder(onBehalfToken);

        // we need to get the absolute path to the document library from which we were called
        const libAbsPath = (new URL(message.request.listUrl, message.request.webAbsUrl)).toString();

        // we encode the absolute path using the sharing url trick (https://docs.microsoft.com/en-us/graph/api/shares-get?view=graph-rest-1.0&tabs=http#encoding-sharing-urls)
        const shareUrl = encodeSharingUrl(libAbsPath);

        // now we get some information about the document library, namely its id using the parent information of the root folder of our share
        const docLibInfo = await request<{ parentReference: { driveId: string } }>(`https://graph.microsoft.com/v1.0/shares/${shareUrl}/root`);

        // grab the doc lib id from which the button was invoked
        const docLibGraphId = docLibInfo.parentReference.driveId;

        // add a file to the doc lib as the user
        await request(`https://graph.microsoft.com/v1.0/drives/${docLibGraphId}/root:/${getRandomString(6)}.txt:/content`, {
            method: "PUT",
            headers: {
                "Content-Type": "text/plain",
            },
            body: `Here is our content! And some random text so we know it is new "${getRandomString(10)}".`
        });

        // context.log("Upload Result", upoloadResultInfo);

        // production: resumable upload: https://docs.microsoft.com/en-us/graph/api/driveitem-createuploadsession?view=graph-rest-1.0

    } catch (e) {
        context.log("Error running serviceBusQueueTrigger");
        context.done(e);
    }
};

export default serviceBusQueueTrigger;
