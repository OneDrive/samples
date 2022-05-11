using IntegratedTokenCache.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntegratedTokenCache.Stores
{
    /// <summary>
    /// Storing activity data next to the MSAL token cache
    /// </summary>
    public class SqlServerMsalAccountActivityStore : IMsalAccountActivityStore
    {
        private IntegratedTokenCacheDbContext _dbContext;

        public SqlServerMsalAccountActivityStore(IntegratedTokenCacheDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Retrieve MsalAccountActivites that happened before a certain time ago
        public async Task<IEnumerable<MsalAccountActivity>> GetMsalAccountActivitesSince(DateTime lastActivityDate)
        {
            return await _dbContext.MsalAccountActivities
                .Where(x => x.FailedToAcquireToken == false
                    && x.LastActivity <= lastActivityDate)
                .ToListAsync();
        }

        public async Task<MsalAccountActivity> GetMsalAccountActivityForSubscription(string subscriptionId)
        {
            return await _dbContext.MsalAccountActivities
                            .Where(x => x.SubscriptionId == subscriptionId)
                            .OrderByDescending(x=>x.LastActivity)
                            .FirstOrDefaultAsync();
        }

        // Retireve a specific user MsalAccountActivity
        public async Task<MsalAccountActivity> GetMsalAccountActivityForUser(string userPrincipalName)
        {
            return await _dbContext.MsalAccountActivities
                            .Where(x => x.FailedToAcquireToken == false
                                && x.UserPrincipalName == userPrincipalName)
                            .FirstOrDefaultAsync();
        }

        public async Task<SubscriptionActivity> GetSubscriptionActivityForUserSubscription(string accountObjectId, string accountTenantId, string userPrincipalName, string subscriptionId)
        {
            return await _dbContext.SubscriptionActivities
                .Where(x => x.AccountObjectId == accountObjectId && 
                            x.AccountTenantId == accountTenantId && 
                            x.UserPrincipalName == userPrincipalName && 
                            x.SubscriptionId == subscriptionId)
                .FirstOrDefaultAsync();
        }

        public async Task<List<SubscriptionActivity>> GetSubscriptionActivities()
        {
            return await _dbContext.SubscriptionActivities.ToListAsync();
        }

        // Setting the flag FailedToAcquireToken to true
        public async Task HandleIntegratedTokenAcquisitionFailure(MsalAccountActivity failedAccountActivity)
        {
            //failedAccountActivity.FailedToAcquireToken = true;
            //_dbContext.MsalAccountActivities.Update(failedAccountActivity);
            _dbContext.MsalAccountActivities.Remove(failedAccountActivity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task RemoveMsalAccountActivity(string accountCacheKey)
        {
            var rowsToRemove = await _dbContext.MsalAccountActivities
                .Where(x => x.AccountCacheKey == accountCacheKey)
                .ToListAsync();

            _dbContext.MsalAccountActivities.RemoveRange(rowsToRemove);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpsertSubscriptionActivity(SubscriptionActivity subscriptionActivity)
        {
            if (_dbContext.SubscriptionActivities.Count(x => x.AccountObjectId == subscriptionActivity.AccountObjectId &&
                            x.AccountTenantId == subscriptionActivity.AccountTenantId &&
                            x.UserPrincipalName == subscriptionActivity.UserPrincipalName) != 0)
            {
                _dbContext.Update(subscriptionActivity);
            }
            else
            {
                _dbContext.Add(subscriptionActivity);
            }

            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<List<string>> UpdateSubscriptionId(string subscriptionId, string accountObjectId, string accountTenantId, string userPrincipalName)
        {
            var rowsToUpdate = await _dbContext.MsalAccountActivities
                .Where(x => x.AccountObjectId == accountObjectId && x.AccountTenantId == accountTenantId && x.UserPrincipalName == userPrincipalName)
                .OrderByDescending(x => x.LastActivity)
                .ToListAsync();

            List<string> cacheEntriesToRemove = new List<string>();
            bool first = true;
            foreach(var row in rowsToUpdate)
            {
                if (first)
                {
                    // Update the last used row
                    row.SubscriptionId = subscriptionId;
                    first = false;
                    _dbContext.MsalAccountActivities.Update(row);
                }
                else
                {
                    // Delete all "old" rows as the corresponding msal cache item is not valid anymore
                    cacheEntriesToRemove.Add(row.AccountCacheKey);
                    _dbContext.MsalAccountActivities.Remove(row);
                }
            }
            
            await _dbContext.SaveChangesAsync();

            return cacheEntriesToRemove;
        }

        // Insert a new MsalAccountActivity case it doesnt exist, otherwise update the existing entry
        public async Task UpsertMsalAccountActivity(MsalAccountActivity msalAccountActivity)
        {
            if (_dbContext.MsalAccountActivities.Count(x => x.AccountCacheKey == msalAccountActivity.AccountCacheKey) != 0)
                _dbContext.Update(msalAccountActivity);
            else
                _dbContext.MsalAccountActivities.Add(msalAccountActivity);

            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}