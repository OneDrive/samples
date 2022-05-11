using IntegratedTokenCache.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IntegratedTokenCache.Stores
{
    /// <summary>
    /// Storing activity data next to the MSAL token cache
    /// </summary>
    public interface IMsalAccountActivityStore
    {
        Task UpsertMsalAccountActivity(MsalAccountActivity msalAccountActivity);

        Task<IEnumerable<MsalAccountActivity>> GetMsalAccountActivitesSince(DateTime lastActivityDate);

        Task<MsalAccountActivity> GetMsalAccountActivityForUser(string userPrincipalName);

        Task HandleIntegratedTokenAcquisitionFailure(MsalAccountActivity failedAccountActivity);

        Task RemoveMsalAccountActivity(string accountCacheKey);

        Task<List<string>> UpdateSubscriptionId(string subscriptionId, string accountObjectId, string accountTenantId, string userPrincipalName);

        Task<MsalAccountActivity> GetMsalAccountActivityForSubscription(string subscriptionId);

        Task UpsertSubscriptionActivity(SubscriptionActivity subscriptionActivity);

        Task<SubscriptionActivity> GetSubscriptionActivityForUserSubscription(string accountObjectId, string accountTenantId, string userPrincipalName, string subscriptionId);

        Task<List<SubscriptionActivity>> GetSubscriptionActivities();
    }
}