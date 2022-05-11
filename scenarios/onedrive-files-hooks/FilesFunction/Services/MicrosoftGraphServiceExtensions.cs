using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using System.Collections.Generic;

namespace FilesFunction.Services
{
    public static class MicrosoftGraphServiceExtensions
    {
        /// <summary>
        /// Adds the Microsoft Graph client as a singleton.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="configuration">Configuration for Microsoft Graph.</param>
        /// <param name="initialScopes">Initial scopes.</param>
        /// <param name="graphBaseUrlKey">Base URL for Microsoft graph. This can be
        /// changed for instance for applications running in national clouds</param>
        public static void AddMicrosoftGraph(this IServiceCollection services,
                                             IConfiguration configuration,
                                             IEnumerable<string> initialScopes,
                                             string graphBaseUrlKey = "MicrosoftGraphBaseUrl")
        {
            // Graph base URL
            string graphBaseUrl = configuration.GetValue<string>(graphBaseUrlKey);

            services.AddTokenAcquisition(true);
            services.AddSingleton<GraphServiceClient, GraphServiceClient>(serviceProvider =>
            {
                var tokenAquisitionService = serviceProvider.GetService<ITokenAcquisition>();
                GraphServiceClient client = string.IsNullOrWhiteSpace(graphBaseUrl) ?
                            new GraphServiceClient(new TokenAcquisitionCredentialProvider(tokenAquisitionService, initialScopes)) :
                            new GraphServiceClient(graphBaseUrl, new TokenAcquisitionCredentialProvider(tokenAquisitionService, initialScopes));
                return client;
            });
        }
    }
}
