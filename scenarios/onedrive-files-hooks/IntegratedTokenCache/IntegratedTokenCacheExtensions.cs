using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web.TokenCacheProviders;
using System;

namespace IntegratedTokenCache
{
    public static class IntegratedTokenCacheExtensions
    {
        /// <summary>Adds an integrated per-user .NET Core distributed based token cache.</summary>
        /// <param name="services">The services collection to add to.</param>
        /// <returns>A <see cref="IServiceCollection"/> to chain.</returns>
        public static IServiceCollection AddIntegratedUserTokenCache(
            this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddDistributedMemoryCache();
            services.AddHttpContextAccessor();
            services.AddSingleton<IMsalTokenCacheProvider, IntegratedTokenCacheAdapter>();
            return services;
        }

        /// <summary>Adds an integrated per-user .NET Core distributed based token cache.</summary>
        /// <param name="builder">The Authentication builder to add to.</param>
        /// <returns>A <see cref="AuthenticationBuilder"/> to chain.</returns>
        public static AuthenticationBuilder AddIntegratedUserTokenCache(
            this AuthenticationBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Services.AddIntegratedUserTokenCache();
            return builder;
        }
    }
}
