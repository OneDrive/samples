using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web.TokenCacheProviders.Distributed;
using System.Threading.Tasks;

namespace FilesFunction.Services
{
    /// <summary>
    /// MSAL distributed token cache adapter that operates on a given cache key. Is used
    /// to load the correct MSAL token from cache when running in a background service
    /// </summary>
    public class BackgroundWorkerTokenCacheAdapter : MsalDistributedTokenCacheAdapter
    {
        /// <summary>
        /// .NET Core distributed cache.
        /// </summary>
        private readonly IDistributedCache _distributedCache;

        /// <summary>
        /// MSAL distributed token cache options.
        /// </summary>
        private readonly MsalDistributedTokenCacheAdapterOptions _cacheOptions;

        private readonly string _cacheKey;

        public BackgroundWorkerTokenCacheAdapter(string cacheKey,
            IDistributedCache distributedCache,
            ILogger<MsalDistributedTokenCacheAdapter> logger,
            IOptions<MsalDistributedTokenCacheAdapterOptions> cacheOptions)
            : base(distributedCache, cacheOptions, logger)
        {
            _cacheKey = cacheKey;
            _distributedCache = distributedCache;
            _cacheOptions = cacheOptions?.Value;
        }

        protected override async Task RemoveKeyAsync(string cacheKey)
        {
            await _distributedCache.RemoveAsync(_cacheKey).ConfigureAwait(false);
        }

        protected override async Task<byte[]> ReadCacheBytesAsync(string cacheKey)
        {
            return await _distributedCache.GetAsync(_cacheKey).ConfigureAwait(false);
        }

        protected override async Task WriteCacheBytesAsync(string cacheKey, byte[] bytes)
        {
            await _distributedCache.SetAsync(_cacheKey, bytes, _cacheOptions).ConfigureAwait(false);
        }
    }
}
