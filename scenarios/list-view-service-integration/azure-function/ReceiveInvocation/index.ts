import { AzureFunction, Context } from "@azure/functions"
import { ServiceBusClient } from "@azure/service-bus";
import { InvocationHttpRequest, QueueMessage } from "../types";

const httpTrigger: AzureFunction = async function (context: Context, req: InvocationHttpRequest): Promise<void> {

    context.log('HTTP trigger function processing a request.');

    const client = new ServiceBusClient(process.env["ServiceBusConnection"]);
    const sender = client.createSender(process.env["ServiceBusQueueName"]);

    try {

        // context.log("InvocationHttpRequest: ", req);

        // Here we create our message to add to the queue
        // for simplicity we include the auth information in the message
        // alternatives would be writing the values to a database or other store
        const queueMessage: QueueMessage = {
            auth: {
                requestBearerToken: req.headers["authorization"].split(" ")[1],
            },
            request: req.body,
        }

        await sender.sendMessages({
            body: JSON.stringify(queueMessage),
        });

        context.res = {
            status: 200,
            body: "Operation successfully added to queue."
        };

    } catch (e) {

        context.log("Error executing httpTrigger");

        // for production likely you would not want to return actual error information, but this can help with debugging while learning
        context.res = {
            status: 500,
            body: `There was an error adding the message to the queue: ${e.message ? e.message : e}`,
        };

        context.done(e);

    } finally {

        await sender.close();
        await client.close();
    }
};

export default httpTrigger;
