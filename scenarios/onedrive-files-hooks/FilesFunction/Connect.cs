using FilesFunction.Services;
using IntegratedTokenCache.Stores;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.TokenCacheProviders;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FilesFunction
{
    /// <summary>
    /// Service that's called when a user signs up for using OneDrive as storage backend
    /// </summary>
    public class Connect
    {
        private readonly ITokenAcquisition _tokenAcquisition;
        private readonly GraphServiceClient _graphServiceClient;
        private static IConfiguration _config;
        private static IMsalAccountActivityStore _msalAccountActivityStore;
        private static IMsalTokenCacheProvider _msalTokenCacheProvider;

        public Connect(ITokenAcquisition tokenAcquisition, GraphServiceClient graphServiceClient, 
            IConfiguration config, IMsalAccountActivityStore msalAccountActivityStore, IMsalTokenCacheProvider msalTokenCacheProvider)
        {
            _tokenAcquisition = tokenAcquisition;
            _graphServiceClient = graphServiceClient;
            _config = config;
            _msalAccountActivityStore = msalAccountActivityStore;
            _msalTokenCacheProvider = msalTokenCacheProvider;
        }

        [FunctionName("Connect")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            #region Authentication
            // We're using Microsoft.Identity.Web
            var (authenticationStatus, authenticationResponse) = await req.HttpContext.AuthenticateAzureFunctionAsync();
            if (!authenticationStatus)
            {
                return authenticationResponse;
            }

            // Sample call to show how to get an access token, only for demo purposes
            //var token = await _tokenAcquisition.GetAccessTokenForUserAsync(scopes: Constants.BasePermissionScopes, tokenAcquisitionOptions: new TokenAcquisitionOptions());
            //log.LogInformation($"Graph access token: {token}");
            #endregion

            #region Configure a subscription to handle change notifications

            // Get the current user information
            string tid = req.HttpContext.User.FindFirstValue("http://schemas.microsoft.com/identity/claims/tenantid");
            string oid = req.HttpContext.User.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier");
            string upn = req.HttpContext.User.FindFirstValue("preferred_username");

            await SubscriptionManagement.ManageSubscription(null, oid, tid, upn, _config, _msalAccountActivityStore, _graphServiceClient, _msalTokenCacheProvider);
            #endregion

            string responseMessage = "This HTTP triggered function executed successfully";
            return new OkObjectResult(responseMessage);
        }
    }
}

