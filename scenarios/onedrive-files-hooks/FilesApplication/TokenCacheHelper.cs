using System.IO;
using System.Security.Cryptography;
using Microsoft.Identity.Client;

namespace FilesApplication
{
    static class TokenCacheHelper
    { 
        /// <summary>
        /// Path to the token cache
        /// </summary>
        private static readonly string CacheFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location + ".msalcache.bin";

        private static readonly object FileLock = new object();

        private static void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {

            //debug
            //var exchangeTokenCacheV3Bytes = args.TokenCache.SerializeMsalV3();
            //string jsonString = System.Text.Encoding.UTF8.GetString(exchangeTokenCacheV3Bytes);            

            lock (FileLock)
            {
                args.TokenCache.DeserializeMsalV3(File.Exists(CacheFilePath)
                    ? ProtectedData.Unprotect(File.ReadAllBytes(CacheFilePath),
                                              null,
                                              DataProtectionScope.CurrentUser)
                    : null);
            }
        }

        private static void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            // if the access operation resulted in a cache update
            if (args.HasStateChanged)
            {
                lock (FileLock)
                {
                    // reflect changes in the persistent store
                    File.WriteAllBytes(CacheFilePath,
                                       ProtectedData.Protect(args.TokenCache.SerializeMsalV3(), 
                                                             null, 
                                                             DataProtectionScope.CurrentUser)
                                      );
                }
            }
        }
        internal static void EnableSerialization(ITokenCache tokenCache)
        {
            tokenCache.SetBeforeAccess(BeforeAccessNotification);
            tokenCache.SetAfterAccess(AfterAccessNotification);
        }
    }
}
