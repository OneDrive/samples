using IntegratedTokenCache;
using IntegratedTokenCache.Entities;
using IntegratedTokenCache.Stores;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Identity.Web.TokenCacheProviders;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FilesFunction.Services
{
    /// <summary>
    /// Manages the Microsoft Graph subscriptions on OneDrive
    /// </summary>
    internal static class SubscriptionManagement
    {
        internal static async Task ManageSubscription(SubscriptionActivity currentSubscriptionActivity, string oid, string tid, string upn, IConfiguration config, IMsalAccountActivityStore msalAccountActivityStore, GraphServiceClient _graphServiceClient, IMsalTokenCacheProvider msalTokenCacheProvider)
        {
            string subscriptionId = null;
            string changeToken = null;            
            string notiticationUrl = config.GetValue<string>("Files:SubscriptionService");
            int subscriptionLifeTimeInMinutes = config.GetValue<int>("Files:SubscriptionLifeTime");
            if (subscriptionLifeTimeInMinutes == 0)
            {
                subscriptionLifeTimeInMinutes = 15;
            }

            // Load the current subscription (if any)
            var currentSubscriptions = await _graphServiceClient.Subscriptions.Request().GetAsync();
            var currentOneDriveSubscription = currentSubscriptions.FirstOrDefault(p => p.Resource == "me/drive/root");

            // If present and still using the same subscription host then update the subscription expiration date
            if (currentOneDriveSubscription != null && currentOneDriveSubscription.NotificationUrl.Equals(notiticationUrl, StringComparison.InvariantCultureIgnoreCase))
            {
                // Extend the expiration of the current subscription
                subscriptionId = currentOneDriveSubscription.Id;
                currentOneDriveSubscription.ExpirationDateTime = DateTimeOffset.UtcNow.AddMinutes(subscriptionLifeTimeInMinutes);
                currentOneDriveSubscription.ClientState = Constants.FilesSubscriptionServiceClientState;
                await _graphServiceClient.Subscriptions[currentOneDriveSubscription.Id].Request().UpdateAsync(currentOneDriveSubscription);

                // Check if the last change token was populated
                if (currentSubscriptionActivity == null)
                {
                    currentSubscriptionActivity = await msalAccountActivityStore.GetSubscriptionActivityForUserSubscription(oid, tid, upn, subscriptionId);
                }

                if (currentSubscriptionActivity != null)
                {
                    changeToken = currentSubscriptionActivity.LastChangeToken;
                }
            }
            else
            {
                // Add a new subscription
                var newSubscription = await _graphServiceClient.Subscriptions.Request().AddAsync(new Subscription()
                {
                    ChangeType = "updated",
                    NotificationUrl = notiticationUrl,
                    Resource = "me/drive/root",
                    ExpirationDateTime = DateTimeOffset.UtcNow.AddMinutes(subscriptionLifeTimeInMinutes),
                    ClientState = Constants.FilesSubscriptionServiceClientState,
                    LatestSupportedTlsVersion = "v1_2"
                });
                subscriptionId = newSubscription.Id;
            }

            // Store the user principal name with the subscriptionid as that's the mechanism needed to connect change event with tenant/user
            var cacheEntriesToRemove = await msalAccountActivityStore.UpdateSubscriptionId(subscriptionId, oid, tid, upn);

            // If we've found old MSAL cache entries for which we've removed the respective MsalActivity records we should also 
            // drop these from the MSAL cache itself
            if (cacheEntriesToRemove.Any())
            {
                foreach (var cacheEntry in cacheEntriesToRemove)
                {
                    await (msalTokenCacheProvider as IntegratedTokenCacheAdapter).RemoveKeyFromCache(cacheEntry);
                }
            }

            if (changeToken == null)
            {
                // Initialize the subscription and get the latest change token, to avoid getting back all the historical changes
                IDriveItemDeltaCollectionPage deltaCollection = await _graphServiceClient.Me.Drive.Root.Delta("latest").Request().GetAsync();
                var deltaLink = deltaCollection.AdditionalData["@odata.deltaLink"];
                if (!string.IsNullOrEmpty(deltaLink.ToString()))
                {
                    changeToken = ProcessChanges.GetChangeTokenFromUrl(deltaLink.ToString());
                }
            }

            // Store a record per user/subscription to track future delta queries
            if (currentSubscriptionActivity == null)
            {
                currentSubscriptionActivity = new SubscriptionActivity(oid, tid, upn)
                {
                    SubscriptionId = subscriptionId,
                    LastChangeToken = changeToken
                };
            }

            currentSubscriptionActivity.LastChangeToken = changeToken;
            currentSubscriptionActivity.SubscriptionId = subscriptionId;

            await msalAccountActivityStore.UpsertSubscriptionActivity(currentSubscriptionActivity);
        }


    }
}
