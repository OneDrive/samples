using FilePickerDemo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace FilePickerDemo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> logger;
        private readonly ITokenAcquisition tokenAcquisition;
        private readonly IConfiguration configuration;
        private readonly Uri pickerSite;

        public HomeController(ILogger<HomeController> logger, ITokenAcquisition tokenAcquisition, IConfiguration config)
        {
            this.logger = logger;
            this.tokenAcquisition = tokenAcquisition;
            configuration = config;
            pickerSite = new Uri(configuration["Picker:SiteUrl"]);
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Browser()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel(Activity.Current?.Id ?? HttpContext.TraceIdentifier));
        }

        [HttpGet]
        [Authorize]
        public async Task<string> GetPickerAccessToken()
        {
            string[] scopes = new string[] { $"https://{pickerSite.DnsSafeHost}/.default" };
            string accessToken = await tokenAcquisition.GetAccessTokenForUserAsync(scopes);

            return accessToken;
        }

        [HttpPost]
        [Authorize]
        public async Task<string> Index(SelectedFileViewModel model)
        {
            string[] scopes = new string[] { "files.read.all" };
            string accessToken = await tokenAcquisition.GetAccessTokenForUserAsync(scopes);

            if (string.IsNullOrEmpty(model.AccessToken))
            {
                model.AccessToken = accessToken;
            }

            var graphClient = GetGraphClient(model.AccessToken);

            var result = await graphClient.Drives[model.DriveId].Items[model.FileId].Request().Expand("thumbnails").GetAsync();

            return result.Thumbnails.First().Large.Url;
        }

        private static GraphServiceClient GetGraphClient(string accessToken)
        {
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            return new GraphServiceClient(new DelegateAuthenticationProvider(
                async (requestMessage) =>
                {
                    requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", accessToken);
                })
            );
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        }

    }
}
