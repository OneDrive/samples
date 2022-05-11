using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace FilesApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly string AadInstance = ConfigurationManager.AppSettings["ida:AADInstance"];
        private static readonly string Tenant = ConfigurationManager.AppSettings["ida:Tenant"];
        private static readonly string ClientId = ConfigurationManager.AppSettings["ida:ClientId"];

        private static readonly string Authority = string.Format(CultureInfo.InvariantCulture, AadInstance, Tenant);

        // To authenticate to the To Do list service, the client needs to know the service's App ID URI and URL
        private static readonly string FilesServiceScope = ConfigurationManager.AppSettings["files:ServiceScope"];
        private static readonly string[] Scopes = { FilesServiceScope };
        private static readonly string[] BasePermissionScopes = { "user.read", "files.readwrite.all" };
        private static readonly string MSGraphURL = "https://graph.microsoft.com/v1.0/";

        private readonly HttpClient _httpClient = new HttpClient();
        private readonly IPublicClientApplication _app;

        // Button content
        const string SignInString = "Connect OneDrive";
        const string ClearCacheString = "Disconnect OneDrive";

        public MainWindow()
        {
            InitializeComponent();

            _app = PublicClientApplicationBuilder.Create(ClientId)
                .WithAuthority(Authority)
                .WithDefaultRedirectUri()
                .Build();

            TokenCacheHelper.EnableSerialization(_app.UserTokenCache);

            Init(SignInButton.Content.ToString() != ClearCacheString).GetAwaiter().GetResult();
        }

        private async Task Init(bool isAppStarting)
        {
            var accounts = (await _app.GetAccountsAsync()).ToList();
            if (!accounts.Any())
            {
                SignInButton.Content = SignInString;
                return;
            }

            AuthenticationResult result = null;
            try
            {
                result = await _app.AcquireTokenSilent(Scopes, accounts.FirstOrDefault())
                    .ExecuteAsync()
                    .ConfigureAwait(false);

                Dispatcher.Invoke(
                    () =>
                    {
                        SignInButton.Content = ClearCacheString;
                        SetUserName(result.Account);
                    });
            }
            // There is no access token in the cache, so prompt the user to sign-in.
            catch (MsalUiRequiredException)
            {
                if (!isAppStarting)
                {
                    SignInButton.Content = SignInString;
                }
            }
            catch (MsalException ex)
            {
                // An unexpected error occurred.
                string message = ex.Message;
                if (ex.InnerException != null)
                {
                    message += "Error Code: " + ex.ErrorCode + "Inner Exception : " + ex.InnerException.Message;
                }
                MessageBox.Show(message);

                UserName.Content = FilesApplication.Resources.UserNotSignedIn;
                return;
            }
        }

        private async Task SignUp()
        {
            var accounts = (await _app.GetAccountsAsync()).ToList();
            if (!accounts.Any())
            {
                SignInButton.Content = SignInString;
                return;
            }

            // Get an access token to call the Connect service.
            AuthenticationResult result = null;
            try
            {
                result = await _app.AcquireTokenSilent(Scopes, accounts.FirstOrDefault())
                    .ExecuteAsync()
                    .ConfigureAwait(false);

            }
            // There is no access token in the cache, so prompt the user to sign-in.
            catch (MsalUiRequiredException)
            {
            }
            catch (MsalException ex)
            {
                // An unexpected error occurred.
                string message = ex.Message;
                if (ex.InnerException != null)
                {
                    message += "Error Code: " + ex.ErrorCode + "Inner Exception : " + ex.InnerException.Message;
                }
                MessageBox.Show(message);

                UserName.Content = FilesApplication.Resources.UserNotSignedIn;
                return;
            }

            // Once the token has been returned by MSAL, add it to the http authorization header, before making the call to access the Connect service.
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);

            // Call the Connect list service.
            HttpResponseMessage response = await _httpClient.PostAsync(ConfigurationManager.AppSettings["files:Connect"], null);

            if (response.IsSuccessStatusCode)
            {
                // do something
            }
            else
            {
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden && response.Headers.WwwAuthenticate.Any())
                {
                    await HandleChallengeFromWebApi(response, result.Account);
                }
                else
                {
                    await DisplayErrorMessage(response);
                }
            }
        }

        private static async Task DisplayErrorMessage(HttpResponseMessage httpResponse)
        {
            string failureDescription = await httpResponse.Content.ReadAsStringAsync();
            if (failureDescription.StartsWith("<!DOCTYPE html>"))
            {
                string path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                string errorFilePath = Path.Combine(path, "error.html");
                File.WriteAllText(errorFilePath, failureDescription);
                Process.Start(errorFilePath);
            }
            else
            {
                MessageBox.Show($"{httpResponse.ReasonPhrase}\n {failureDescription}", "An error occurred", MessageBoxButton.OK);
            }
        }

        /// <summary>
        /// When the Web API needs consent, it can sent a 403 with information in the WWW-Authenticate header in 
        /// order to challenge the user
        /// </summary>
        /// <param name="response">HttpResonse received from the service</param>
        /// <returns></returns>
        private async Task HandleChallengeFromWebApi(HttpResponseMessage response, IAccount account)
        {
            AuthenticationHeaderValue bearer = response.Headers.WwwAuthenticate.First(v => v.Scheme == "Bearer");
            IEnumerable<string> parameters = bearer.Parameter.Split(',').Select(v => v.Trim()).ToList();
            string clientId = GetParameter(parameters, "clientId");
            string claims = GetParameter(parameters, "claims");
            string[] scopes = GetParameter(parameters, "scopes")?.Split(',');
            string proposedAction = GetParameter(parameters, "proposedAction");
            string consentUri = GetParameter(parameters, "consentUri");

            string loginHint = account?.Username;
            string domainHint = IsConsumerAccount(account) ? "consumers" : "organizations";
            string extraQueryParameters = $"claims={claims}&domainHint={domainHint}";

            if (proposedAction == "forceRefresh")
            {
                // Removes the account, but then re-signs-in
                await _app.RemoveAsync(account);
                await _app.AcquireTokenInteractive(scopes)
                    .WithPrompt(Prompt.Consent)
                    .WithLoginHint(loginHint)
                    .WithExtraQueryParameters(extraQueryParameters)
                    .WithAuthority(_app.Authority)
                    .ExecuteAsync()
                    .ConfigureAwait(false);
            }
            else if (proposedAction == "consent")
            {
                if (MessageBox.Show("You need to consent to the Web API. If you press Ok, you'll be redirected to a browser page to consent",
                                    "Consent needed for the Web API",
                                    MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    Process.Start(consentUri);
                }
            }
        }

        /// <summary>
        /// Tells if the account is a consumer account
        /// </summary>
        /// <param name="account">Account</param>
        /// <returns><c>true</c> if the application supports MSA+AAD and the home tenant id of the account is the MSA tenant. <c>false</c>
        /// otherwise (in particular if the app is a single-tenant app, returning <c>false</c> enables MSA accounts which are guest
        /// of a directory</returns>
        private static bool IsConsumerAccount(IAccount account)
        {
            const string msaTenantId = "9188040d-6c67-4c5b-b112-36a304b66dad";
            return (Tenant == "common" || Tenant == "consumers") && account?.HomeAccountId.TenantId == msaTenantId;
        }

        private static string GetParameter(IEnumerable<string> parameters, string parameterName)
        {
            int offset = parameterName.Length + 1;
            return parameters.FirstOrDefault(p => p.StartsWith($"{parameterName}="))?.Substring(offset)?.Trim('"');
        }

        private async void UploadFile(object sender, RoutedEventArgs e)
        {
            var accounts = (await _app.GetAccountsAsync()).ToList();

            if (!accounts.Any())
            {
                MessageBox.Show("Please sign in first");
                return;
            }

            System.Windows.Forms.OpenFileDialog fileDialog = new System.Windows.Forms.OpenFileDialog();

            if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                using (var fileStream = File.OpenRead(fileDialog.FileName))
                {
                    AuthenticationResult result = null;
                    try
                    {
                        result = await _app.AcquireTokenSilent(BasePermissionScopes, accounts.FirstOrDefault())
                            .ExecuteAsync()
                            .ConfigureAwait(false);

                        await Dispatcher.Invoke(async () =>
                        {

                            Microsoft.Graph.GraphServiceClient graphClient = new Microsoft.Graph.GraphServiceClient(MSGraphURL,
                                new Microsoft.Graph.DelegateAuthenticationProvider(async (requestMessage) =>
                                {
                                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", result.AccessToken);
                                }));

                            // Use properties to specify the conflict behavior
                            // in this case, replace
                            var uploadProps = new Microsoft.Graph.DriveItemUploadableProperties
                            {
                                ODataType = null,
                                AdditionalData = new Dictionary<string, object>
                                    {
                                        { "@microsoft.graph.conflictBehavior", "replace" }
                                    }
                            };

                            string itemPath = Path.GetFileName(fileDialog.FileName);

                            // Create the upload session
                            // itemPath does not need to be a path to an existing item
                            var uploadSession = await graphClient.Me.Drive.Root
                                .ItemWithPath(itemPath)
                                .CreateUploadSession(uploadProps)
                                .Request()
                                .PostAsync();

                            // Max slice size must be a multiple of 320 KiB
                            int maxSliceSize = 10 * 320 * 1024;
                            var fileUploadTask = new Microsoft.Graph.LargeFileUploadTask<Microsoft.Graph.DriveItem>(uploadSession, fileStream, maxSliceSize);

                            // Create a callback that is invoked after each slice is uploaded
                            IProgress<long> progress = new Progress<long>(prog =>
                            {
                                ProgressLabel.Content = $"Uploaded {prog} bytes of {fileStream.Length} bytes";
                            });

                            try
                            {
                                // Upload the file
                                var uploadResult = await fileUploadTask.UploadAsync(progress);

                                if (uploadResult.UploadSucceeded)
                                {
                                    // The ItemResponse object in the result represents the created item.
                                    MessageBox.Show($"Added file {uploadResult.ItemResponse.Name} to the signed in user's OneDrive");
                                }
                                else
                                {
                                    MessageBox.Show("Upload failed", "Error");
                                }
                            }
                            catch (Microsoft.Graph.ServiceException ex)
                            {
                                MessageBox.Show($"Error uploading: {ex} ", "Error");
                            }

                            ProgressLabel.Content = "";

                            // Approach for uploading files guaranteed below 4MB in size
                            //var addedItem = await graphClient.Me.Drive.Root.ItemWithPath(Path.GetFileName(fileDialog.FileName)).Content
                            //.Request()
                            //.PutAsync<Microsoft.Graph.DriveItem>(fileStream);
                            //MessageBox.Show($"Added file {addedItem.Name} to the signed in user's OneDrive");
                        });
                    }
                    // There is no access token in the cache, so prompt the user to sign-in.
                    catch (MsalUiRequiredException)
                    {
                        MessageBox.Show("Please re-sign");
                        SignInButton.Content = SignInString;
                    }
                    catch (MsalException ex)
                    {
                        // An unexpected error occurred.
                        string message = ex.Message;
                        if (ex.InnerException != null)
                        {
                            message += "Error Code: " + ex.ErrorCode + "Inner Exception : " + ex.InnerException.Message;
                        }

                        Dispatcher.Invoke(() =>
                        {
                            UserName.Content = FilesApplication.Resources.UserNotSignedIn;
                            MessageBox.Show("Unexpected error: " + message);
                        });

                        return;
                    }
                }
            }
        }

        private async void SignIn(object sender = null, RoutedEventArgs args = null)
        {
            var accounts = (await _app.GetAccountsAsync()).ToList();
            // If there is already a token in the cache, clear the cache and update the label on the button.
            if (SignInButton.Content.ToString() == ClearCacheString)
            {
                // clear the cache
                while (accounts.Any())
                {
                    await _app.RemoveAsync(accounts.First());
                    accounts = (await _app.GetAccountsAsync()).ToList();
                }
                // Also clear cookies from the browser control.
                SignInButton.Content = SignInString;
                UserName.Content = FilesApplication.Resources.UserNotSignedIn;
                return;
            }

            // Get an access token to call the To Do list service.
            try
            {
                var result = await _app.AcquireTokenSilent(Scopes, accounts.FirstOrDefault())
                    .ExecuteAsync()
                    .ConfigureAwait(false);

                await Dispatcher.Invoke(async () =>
                {
                    SignInButton.Content = ClearCacheString;
                    SetUserName(result.Account);
                    await SignUp();
                }
                );
            }
            catch (MsalUiRequiredException)
            {
                try
                {
                    // Force a sign-in (Prompt.SelectAccount), as the MSAL web browser might contain cookies for the current user
                    // and we don't necessarily want to re-sign-in the same user
                    var result = await _app.AcquireTokenInteractive(Scopes)
                        .WithAccount(accounts.FirstOrDefault())
                        .WithPrompt(Prompt.SelectAccount)
                        .WithExtraScopesToConsent(BasePermissionScopes)
                        .ExecuteAsync()
                        .ConfigureAwait(false);

                    await Dispatcher.Invoke(async () =>
                    {
                        SignInButton.Content = ClearCacheString;
                        SetUserName(result.Account);
                        await SignUp();
                    }
                    );
                }
                catch (MsalException ex)
                {
                    if (ex.ErrorCode == "access_denied")
                    {
                        // The user canceled sign in, take no action.
                    }
                    else
                    {
                        // An unexpected error occurred.
                        string message = ex.Message;
                        if (ex.InnerException != null)
                        {
                            message += "Error Code: " + ex.ErrorCode + "Inner Exception : " + ex.InnerException.Message;
                        }

                        MessageBox.Show(message);
                    }

                    Dispatcher.Invoke(() =>
                    {
                        UserName.Content = FilesApplication.Resources.UserNotSignedIn;
                    });
                }
            }
        }

        // Set user name to text box
        private void SetUserName(IAccount userInfo)
        {
            string userName = null;

            if (userInfo != null)
            {
                userName = userInfo.Username;
            }

            if (userName == null)
                userName = FilesApplication.Resources.UserNotIdentified;

            UserName.Content = userName;
        }

    }
}
