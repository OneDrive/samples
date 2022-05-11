using FilesFunction.Model;
using FilesFunction.Services;
using IntegratedTokenCache.Stores;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web.TokenCacheProviders.Distributed;
using System;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace FilesFunction
{
    /// <summary>
    /// Function that processes the change notifications, is triggerd by adding a message in an Azure Storage queueu
    /// </summary>
    public class ProcessChanges
    {
        private static IConfiguration _config;
        private static IMsalAccountActivityStore _msalAccountActivityStore;
        private static IServiceProvider _serviceProvider = null;

        public ProcessChanges(IConfiguration config, IMsalAccountActivityStore msalAccountActivityStore, IServiceProvider serviceProvider)
        {
            _config = config;
            _msalAccountActivityStore = msalAccountActivityStore;
            _serviceProvider = serviceProvider;
        }

        /* Manually put messages in queue:
         * MSA: {"ChangeType":"updated","ClientState":"GraphTutorialState","Resource":"me/drive/root","ResourceData":null,"SubscriptionExpirationDateTime":"2021-04-23T11:57:29.003+02:00","SubscriptionId":"74534c47-4de7-4a29-bf25-b1c42f126c2b","TenantId":""}
         * M365: {"ChangeType":"updated","ClientState":"GraphTutorialState","Resource":"me/drive/root","ResourceData":null,"SubscriptionExpirationDateTime":"2021-04-22T18:52:43.8545835+00:00","SubscriptionId":"5d0a8522-5c2e-48cc-9088-a91e7afa770f","TenantId":"d8623c9e-30c7-473a-83bc-d907df44a26e"}
         */

        [FunctionName("ProcessChanges")]
        public async Task Run([QueueTrigger(Constants.OneDriveFileNotificationsQueue)] string myQueueItem, ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");

            var notification = JsonSerializer.Deserialize<ChangeNotification>(myQueueItem);

            // Get the most recently updated msal account information for this subscription
            var account = await _msalAccountActivityStore.GetMsalAccountActivityForSubscription(notification.SubscriptionId);

            if (account != null)
            {
                // Configure the confidential client to get an access token for the needed resource
                var app = ConfidentialClientApplicationBuilder.Create(_config.GetValue<string>("AzureAd:ClientId"))
                        .WithClientSecret(_config.GetValue<string>("AzureAd:ClientSecret"))
                        .WithAuthority($"{_config.GetValue<string>("AzureAd:Instance")}{_config.GetValue<string>("AzureAd:TenantId")}")
                        .Build();

                // Initialize the MSAL cache for the specific account
                var msalCache = new BackgroundWorkerTokenCacheAdapter(account.AccountCacheKey,
                        _serviceProvider.GetService<IDistributedCache>(),
                        _serviceProvider.GetService<ILogger<MsalDistributedTokenCacheAdapter>>(),
                        _serviceProvider.GetService<IOptions<MsalDistributedTokenCacheAdapterOptions>>());

                await msalCache.InitializeAsync(app.UserTokenCache);

                // Prepare an MsalAccount instance representing the user we want to get a token for
                var hydratedAccount = new MsalAccount
                {
                    HomeAccountId = new AccountId(
                        account.AccountIdentifier,
                        account.AccountObjectId,
                        account.AccountTenantId)
                };

                try
                {
                    // Use the confidential MSAL client to get a token for the user we need to impersonate
                    var result = await app.AcquireTokenSilent(Constants.BasePermissionScopes, hydratedAccount)
                        .ExecuteAsync()
                        .ConfigureAwait(false);

                    //log.LogInformation($"Token acquired: {result.AccessToken}");

                    // Configure the Graph SDK to use an auth provider that takes the token we've just requested
                    var authenticationProvider = new DelegateAuthenticationProvider(
                    (requestMessage) =>
                    {
                        requestMessage.Headers.Authorization = new AuthenticationHeaderValue(CoreConstants.Headers.Bearer, result.AccessToken);
                        return Task.FromResult(0);
                    });
                    var graphClient = new GraphServiceClient(authenticationProvider);

                    // Retrieve the last used subscription activity information for this user+subscription
                    var subscriptionActivity = await _msalAccountActivityStore.GetSubscriptionActivityForUserSubscription(account.AccountObjectId, account.AccountTenantId, account.UserPrincipalName, notification.SubscriptionId);

                    // Make graph call on behalf of the user: do the delta query providing the last used change token so only new changes are returned
                    IDriveItemDeltaCollectionPage deltaCollection = await graphClient.Me.Drive.Root.Delta(subscriptionActivity.LastChangeToken).Request().GetAsync();
                    bool morePagesAvailable = false;
                    do
                    {
                        // If there is a NextPageRequest, there are more pages
                        morePagesAvailable = deltaCollection.NextPageRequest != null;
                        foreach (var driveItem in deltaCollection.CurrentPage)
                        {
                            await ProcessDriveItemChanges(driveItem, log);
                        }

                        if (morePagesAvailable)
                        {
                            // Get the next page of results
                            deltaCollection = await deltaCollection.NextPageRequest.GetAsync();
                        }
                    }
                    while (morePagesAvailable);

                    // Get the last used change token
                    var deltaLink = deltaCollection.AdditionalData["@odata.deltaLink"];
                    if (!string.IsNullOrEmpty(deltaLink.ToString()))
                    {
                        var token = GetChangeTokenFromUrl(deltaLink.ToString());
                        subscriptionActivity.LastChangeToken = token;
                    }

                    // Persist back the last used change token
                    await _msalAccountActivityStore.UpsertSubscriptionActivity(subscriptionActivity);
                }
                catch (MsalUiRequiredException ex)
                {
                    /*
                     * If MsalUiRequiredException is thrown for an account, it means that a user interaction is required
                     * thus the background worker wont be able to acquire a token silently for it.
                     * The user of that account will have to access the web app to perform this interaction.
                     * Examples that could cause this: MFA requirement, token expired or revoked, token cache deleted, etc
                     */
                    await _msalAccountActivityStore.HandleIntegratedTokenAcquisitionFailure(account);

                    log.LogError($"Could not acquire token for account {account.UserPrincipalName}.");
                    log.LogError($"Error: {ex.Message}");
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        private static async Task ProcessDriveItemChanges(DriveItem fileChange, ILogger log)
        {
            if (fileChange.Folder != null)
            {
                log.LogInformation($"Changed happened in folder: {fileChange.Name}");
            }
            else
            {
                if (fileChange.Deleted != null)
                {
                    log.LogInformation($"File with id {fileChange.Id} and name {fileChange.Name} was deleted");
                }
                else 
                {
                    log.LogInformation($"File with id {fileChange.Id} and name {fileChange.Name} was added or updated");
                }
            }
        }

        internal static string GetChangeTokenFromUrl(string url)
        {
            // Initial change query returned from ODB+SharePoint
            // https://graph.microsoft.com/v1.0/me/drive/root/microsoft.graph.delta(token=null)?token=MzslMjM0OyUyMzE7Mzs0NzAwODliMi0zNzk3LTQxNjktYWE0OC03ZTE1OTUwZjk5ODA7NjM3NTUwMjg2MDI0NTAwMDAwOzU1MTYzMjM0NjslMjM7JTIzOyUyMzA
            // Initial change query returned from ODC
            // https://graph.microsoft.com/v1.0/me/drive/root/microsoft.graph.delta(token='latest')?token=aTE09NjM3NTUwMzI5MDkxNjA7SUQ9RjlBMTE3MzIyMjhGNjhBNiExMDE7TFI9NjM3NTUwMzI5MjU1ODc7RVA9MjA7U0k9MTQ7RExFUD0wO1NHPTE7U089NjtQST0z
            // Subsequent change queries returned from ODB+SharePoint
            // https://graph.microsoft.com/v1.0/me/drive/root/microsoft.graph.delta(token='MzslMjM0OyUyMzE7Mzs0NzAwODliMi0zNzk3LTQxNjktYWE0OC03ZTE1OTUwZjk5ODA7NjM3NTUwMjcxNDI4OTcwMDAwOzU1MTYyNzc5MzslMjM7JTIzOyUyMzA')
            // Subsequent change queries returned from ODC
            // https://graph.microsoft.com/v1.0/me/drive/root/microsoft.graph.delta(token='aTE09NjM3NTUwMzQ3MjMyODc7SUQ9RjlBMTE3MzIyMjhGNjhBNiExMDE7TFI9NjM3NTUwMzQ5MDU0MjA7RVA9MjA7U0k9MjM7RExFUD0wO1NHPTE7U089NjtQST0z')?token=aTE09NjM3NTUwMzU0ODIxNzc7SUQ9RjlBMTE3MzIyMjhGNjhBNiExMDE7TFI9NjM3NTUwMzU0OTAyMjc7RVA9MjA7U0k9MjM7RExFUD0wO1NHPTE7U089MjtQST0z

            string token = "?token=";
            string ODBNextToken = "microsoft.graph.delta(token='";

            if (url.Contains(token, StringComparison.InvariantCultureIgnoreCase))
            {
                return url[(url.IndexOf(token) + token.Length)..];
            }
            else
            {
                string part1 = url[(url.IndexOf(ODBNextToken) + ODBNextToken.Length)..];
                return part1.Substring(0, part1.IndexOf("'"));
            }
        }
    }
}
