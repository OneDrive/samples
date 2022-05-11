using IntegratedTokenCache.Entities;
using IntegratedTokenCache.Stores;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web.TokenCacheProviders.Distributed;
using System.Threading.Tasks;

namespace IntegratedTokenCache
{
    /// <summary>
    /// An extension of MsalDistributedTokenCacheAdapter, that will update/insert the entity MsalAccountActivity 
    /// before MSAL writes an entry in the token cache
    /// </summary>
    public class IntegratedTokenCacheAdapter : MsalDistributedTokenCacheAdapter
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public IntegratedTokenCacheAdapter(
            IServiceScopeFactory scopeFactory,
            IDistributedCache memoryCache,
            ILogger<MsalDistributedTokenCacheAdapter> logger,
            IOptions<MsalDistributedTokenCacheAdapterOptions> cacheOptions) : base(memoryCache, cacheOptions, logger)
        {
            _scopeFactory = scopeFactory;
        }

        // Overriding OnBeforeWriteAsync to upsert the entity MsalAccountActivity
        // before MSAL writes an entry in the token cache
        protected override async Task OnBeforeWriteAsync(TokenCacheNotificationArgs args)
        {
            var accountActivity = new MsalAccountActivity(args.SuggestedCacheKey, args.Account);            

            await UpsertActivity(accountActivity);

            await Task.FromResult(base.OnBeforeWriteAsync(args));
        }

        protected override Task WriteCacheBytesAsync(string cacheKey, byte[] bytes)
        {
            // get's the json representation of the token info stored in the cache, for debug purposes
            // string jsonString = System.Text.Encoding.UTF8.GetString(bytes);

            return base.WriteCacheBytesAsync(cacheKey, bytes);
        }

        public async Task RemoveKeyFromCache(string cacheKey)
        {
            await Task.FromResult(RemoveKeyAsync(cacheKey));
        }

        //protected override async Task RemoveKeyAsync(string cacheKey)
        //{
        //    // Also drop the respective MsalAccountActivity
        //    using (var scope = _scopeFactory.CreateScope())
        //    {
        //        var _integratedTokenCacheStore = scope.ServiceProvider.GetRequiredService<IMsalAccountActivityStore>();
                
        //        await _integratedTokenCacheStore.RemoveMsalAccountActivity(cacheKey);
        //    }

        //    await Task.FromResult(base.RemoveKeyAsync(cacheKey));
        //}

        // Call the upsert method of the class that implements IMsalAccountActivityStore
        private async Task<MsalAccountActivity> UpsertActivity(MsalAccountActivity accountActivity)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var _integratedTokenCacheStore = scope.ServiceProvider.GetRequiredService<IMsalAccountActivityStore>();

                await _integratedTokenCacheStore.UpsertMsalAccountActivity(accountActivity);

                return accountActivity;
            }
        }
    }
}