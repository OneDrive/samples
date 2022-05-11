using Azure.Storage.Queues;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace FilesFunction
{
    /// <summary>
    /// Files subscription (=webhook) function. Will be called when a change happens
    /// </summary>
    public class FilesSubscription
    {
        private static IConfiguration _config;

        public FilesSubscription(IConfiguration config)
        {
            _config = config;
        }

        [FunctionName("FilesSubscription")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            #region Web hook setup (validation)
            // Is this a validation request?
            // https://docs.microsoft.com/graph/webhooks#notification-endpoint-validation
            string validationToken = req.Query["validationToken"];

            log.LogInformation($"Validation token = {validationToken}");

            if (!string.IsNullOrEmpty(validationToken))
            {
                // Because validationToken is a string, OkObjectResult
                // will return a text/plain response body, which is
                // required for validation
                return new OkObjectResult(validationToken);
            }
            #endregion

            #region Web hook is called by Microsoft 365
            // Not a validation request, process the body
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            log.LogInformation($"Change notification payload: {requestBody}");

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            // Deserialize the JSON payload into a list of ChangeNotification objects, there can be multiple notifications in a
            // single web hook call
            var notifications = JsonSerializer.Deserialize<NotificationList>(requestBody, jsonOptions);

            foreach (var notification in notifications.Value)
            {
                // Extra security mechanism
                if (notification.ClientState == Constants.FilesSubscriptionServiceClientState)
                {
                    // Process each notification, we're enqueueing the notifications as the service needs 
                    // to respond within 30 seconds
                    await ProcessNotification(notification, log);
                }
                else
                {
                    log.LogInformation($"Notification received with unexpected client state: {notification.ClientState}");
                }
            }

            // Return 202 per docs
            return new AcceptedResult();
            #endregion
        }

        private async Task ProcessNotification(ChangeNotification notification, ILogger log)
        {
            // Add request to queue
            QueueClient queue = new QueueClient(_config.GetValue<string>("AzureWebJobsStorage"), Constants.OneDriveFileNotificationsQueue, new QueueClientOptions
            {
                MessageEncoding = QueueMessageEncoding.Base64
            });
            await queue.CreateIfNotExistsAsync();

            // Send a message to our queue
            string message = JsonSerializer.Serialize(notification);

            await queue.SendMessageAsync(message);
            log.LogInformation($"The following message was added to the queue: {message}");
        }
    }
}
