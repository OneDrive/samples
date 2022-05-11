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
using Microsoft.Identity.Web.TokenCacheProviders;
using Microsoft.Identity.Web.TokenCacheProviders.Distributed;
using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace FilesFunction
{
    /// <summary>
    /// Function that is triggered by a timer and that will perform maintenance on the subscriptions (e.g. renew them)
    /// </summary>
    public class ManageSubscriptions
    {
        private static IConfiguration _config;
        private static IMsalAccountActivityStore _msalAccountActivityStore;
        private static IServiceProvider _serviceProvider = null;
        private static IMsalTokenCacheProvider _msalTokenCacheProvider;

        public ManageSubscriptions(IConfiguration config, IMsalAccountActivityStore msalAccountActivityStore, IServiceProvider serviceProvider, IMsalTokenCacheProvider msalTokenCacheProvider)
        {
            _config = config;
            _msalAccountActivityStore = msalAccountActivityStore;
            _serviceProvider = serviceProvider;
            _msalTokenCacheProvider = msalTokenCacheProvider;
        }

        [FunctionName("ManageSubscriptions")]
        public async Task Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            if (myTimer.IsPastDue)
            {
                log.LogInformation("Skipping past due invocation, waiting for next scheduled run");
                return;
            }

            var subscriptions = await _msalAccountActivityStore.GetSubscriptionActivities();

            foreach(var subscription in subscriptions)
            {
                // Get the most recently updated msal account information for this subscription
                var account = await _msalAccountActivityStore.GetMsalAccountActivityForSubscription(subscription.SubscriptionId);

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

                        // Configure the Graph SDK to use an auth provider that takes the token we've just requested
                        var authenticationProvider = new DelegateAuthenticationProvider(
                        (requestMessage) =>
                        {
                            requestMessage.Headers.Authorization = new AuthenticationHeaderValue(CoreConstants.Headers.Bearer, result.AccessToken);
                            return Task.FromResult(0);
                        });
                        var graphClient = new GraphServiceClient(authenticationProvider);

                        // Add/renew the subscriptions
                        await SubscriptionManagement.ManageSubscription(subscription, account.AccountObjectId, account.AccountTenantId, account.UserPrincipalName,
                            _config, _msalAccountActivityStore, graphClient, _msalTokenCacheProvider);

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
        }
    }
}
