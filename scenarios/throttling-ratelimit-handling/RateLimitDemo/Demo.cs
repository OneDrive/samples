using Microsoft.Identity.Client;
using Spectre.Console;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace RateLimitDemo
{
    internal class Demo
    {
        // Set the client ID of your Azure AD application (GUID)
        private static string ClientId = "c545f9ce-1c11-440b-812b-0b35217d9e83";

        // Set the tenant ID of you Azure AD tenant (GUID, load https://aad.portal.azure.com, and click on "Azure Active Directory")
        private static string TenantId = "d8623c9e-30c7-473a-83bc-d907df44a26e";

        // Provide the host name of SharePoint Online tenant (e.g. contoso.sharepoint.com)
        private static string TenantName = "bertonline.sharepoint.com";

        // Provide path to locally stored certificate, e.g. My|CurrentUser|b133d1cb4d19ce539986c7ac67de005481084c84
        // for a certificate stored in the local store (certmgr.msc), Personal node (=My) using certificate thumbprint b133...
        // You can get the thumbprint by looking up your certificate via certmgr, opening it to the Details tab
        // and scrolling down to the Thumbprint field
        private static string CertificatePath = "My|CurrentUser|b133d1cb4d19ce539986c7ac67de005481084c84";

        // Provide path to PFX file containing the cert to use
        private static string CertificatePFXPath = "";

        // Provide password used to protect the PFX file
        private static string CertificatePFXPassword = "";

        
        private static readonly ReaderWriterLockSlim readerWriterLock = new();
        private static int callsDone = 0;
        private static Stopwatch timer = null;
#pragma warning disable IDE0052 // Remove unread private members
        private static Timer timerThread;
#pragma warning restore IDE0052 // Remove unread private members
        private static readonly ConcurrentDictionary<int, int> requestCountEvolution = new();

        internal async Task InitializeAndRunAsync(int threads = 10, bool useRateLimiter = true)
        {
            // Use Microsoft.Identity.Client to create and configure a confidential client application
            IConfidentialClientApplication confidentialClientApplication = InitializeAuthentication(ClientId, TenantId, CertificatePath, CertificatePFXPath, CertificatePFXPassword);

            // Acquire a SharePoint access token
            var uri = new Uri(GetSiteFromTenant(TenantName));
            var scopes = new[] { $"{uri.Scheme}://{uri.Authority}/.default" };
            var sharePointAccessToken = await GetAccessTokenAsync(confidentialClientApplication, scopes);

            // Acquire a Graph access token
            uri = new Uri($"https://graph.microsoft.com");
            scopes = new[] { $"{uri.Scheme}://{uri.Authority}/.default" };
            var graphAccessToken = await GetAccessTokenAsync(confidentialClientApplication, scopes);

            // Configure the HttpClient with a handler that deals with throttling and the RateLimit headers
            // This HttpClient will be used for all Graph, SharePoint REST and SharePoint CSOM calls
            var client = new HttpClient(new ThrottlingHandler(useRateLimiter)
            {
                InnerHandler = new HttpClientHandler()
            });

            AnsiConsole.MarkupLine("Starting a loop that issues Graph, SharePoint REST and SharePoint CSOM calls");

            // Start stopwatch to capture run time and a timer to display progress every 10 seconds
            timer = Stopwatch.StartNew();
            timerThread = new Timer(new TimerCallback(TickTimer), null, 0, 10000);

            // Kick off a number of parallel threads which each will send requests
            var parallelOps = new List<Task>();

            for (int j = 0; j < threads; j++)
            {
                parallelOps.Add(DoWork(j, client, sharePointAccessToken, graphAccessToken));
            }

            await Task.WhenAll(parallelOps);
        }
        
        private async Task DoWork(int instance, HttpClient client, string sharePointAccessToken, string graphAccessToken)
        {
            // Create a context per thread as ClientContext is not thread safe
            Microsoft.SharePoint.Client.ClientContext clientContext = CreateClientContext(client, sharePointAccessToken);

            // Send requests to SharePoint via SharePoint REST, Microsoft Graph and SharePoint CSOM API calls
            for (int i = 0; i <= 5000; i++)
            {
                if (i % 25 == 0)
                {
                    AnsiConsole.MarkupLine($"[green]Instance {instance}, iteration {i}[/]");
                }

                // Test a SharePoint Rest call
                await SharePointRestCallAsync(client, sharePointAccessToken);

                // Test a Graph Rest call
                await GraphRestCallAsync(client, graphAccessToken);

                // Test a CSOM call
                await SharePointCSOMCallAsync(clientContext);
            }            
        }

        private static void TickTimer(object state)
        {
            // Visualize the progess (requests done in the given timeframe)
            int callsDoneSnapshot = CallsDone();
            int secondsPassed = (int)new TimeSpan(timer.ElapsedTicks).TotalSeconds;
            AnsiConsole.MarkupLine($"[blue]{callsDoneSnapshot} calls done in {secondsPassed} seconds[/]");
            requestCountEvolution.TryAdd(callsDoneSnapshot, secondsPassed);

            // At the five minute mark export the results
            if (secondsPassed == 5*60)
            {
                ExportResults();
            }
        }

        private static void ExportResults()
        {
            using (var writer = new StreamWriter("requestCountEvolution.csv"))
            {
                writer.WriteLine("Requests,Seconds");
                foreach (var kvp in requestCountEvolution)
                {
                    writer.WriteLine($"{kvp.Key},{kvp.Value}");
                }
            }
        }

        private async Task SharePointCSOMCallAsync(Microsoft.SharePoint.Client.ClientContext clientContext)
        {
            // Make a CSOM call
            clientContext.Load(clientContext.Web);
            await clientContext.ExecuteQueryAsync();
            IncrementCallsDone();

            //Console.WriteLine(clientContext.Web.Title);
        }

        private static Microsoft.SharePoint.Client.ClientContext CreateClientContext(HttpClient httpClient, string accessToken)
        {
            var clientContext = new Microsoft.SharePoint.Client.ClientContext(new Uri($"{GetSiteFromTenant(TenantName)}"));

            // Hookup event handler to insert the access token
            clientContext.ExecutingWebRequest += (sender, args) =>
            {
                args.WebRequestExecutor.RequestHeaders["Authorization"] = $"Bearer {accessToken}";
            };

            // Hookup custom WebRequestExecutorFactory 
            clientContext.WebRequestExecutorFactory = new HttpClientWebRequestExecutorFactory(httpClient);
            return clientContext;
        }

        private async Task GraphRestCallAsync(HttpClient httpClient, string accessToken)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, $"https://graph.microsoft.com/v1.0/sites"))
            {
                // Add the access token
                request.Headers.Add("Authorization", $"Bearer {accessToken}");

                // Configure request
                request.Headers.Add("Accept", "application/json;odata.metadata=minimal;odata.streaming=true;IEEE754Compatible=false");

                // Send the request
                HttpResponseMessage response = await httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    // Get the response string
                    Stream requestResponseStream = await response.Content.ReadAsStreamAsync();
                    using (var streamReader = new StreamReader(requestResponseStream))
                    {
                        requestResponseStream.Seek(0, SeekOrigin.Begin);
                        string requestResponse = await streamReader.ReadToEndAsync();
                        IncrementCallsDone();
                        //Console.WriteLine(requestResponse);
                    }
                }
                else
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception(errorContent);
                }
            }
        }

        private async Task SharePointRestCallAsync(HttpClient httpClient, string accessToken)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, $"{GetSiteFromTenant(TenantName)}/_api/web"))
            {
                // Add the access token
                request.Headers.Add("Authorization", $"Bearer {accessToken}");

                // Configure request
                request.Headers.Add("Accept", "application/json;odata=nometadata");

                // Send the request
                HttpResponseMessage response = await httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    // Get the response string
                    Stream requestResponseStream = await response.Content.ReadAsStreamAsync();
                    using (var streamReader = new StreamReader(requestResponseStream))
                    {
                        requestResponseStream.Seek(0, SeekOrigin.Begin);
                        string requestResponse = await streamReader.ReadToEndAsync();
                        IncrementCallsDone();
                        //Console.WriteLine(requestResponse);
                    }
                }
                else
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception(errorContent);
                }
            }
        }

        private static async Task<string> GetAccessTokenAsync(IConfidentialClientApplication confidentialClientApplication, string[] scopes)
        {
            var builder = confidentialClientApplication.AcquireTokenForClient(scopes);
            AuthenticationResult result = await builder.ExecuteAsync();
            return result.AccessToken;
        }

        private static IConfidentialClientApplication InitializeAuthentication(string applicationId, string tenantId, string certPath, string certFile, string certPassword)
        {
            var certificate = LoadCertificate(certPath, certFile, certPassword);

            var builder = ConfidentialClientApplicationBuilder.Create(applicationId).WithCertificate(certificate);

            builder = builder.WithAuthority($"https://login.microsoftonline.com/organizations", tenantId);

            return builder.Build();
        }

        private static X509Certificate2 LoadCertificate(string certPathLocation, string certFile, string certPassword)
        {
            if (!string.IsNullOrEmpty(certPathLocation))
            {
                // Did we get a three part certificate path (= local stored cert)
                var certPath = certPathLocation.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (certPath.Length == 3 && (certPath[1].Equals("CurrentUser", StringComparison.InvariantCultureIgnoreCase) || certPath[1].Equals("LocalMachine", StringComparison.InvariantCultureIgnoreCase)))
                {
                    // Load the Cert based upon this
                    string certThumbPrint = certPath[2].ToUpper();

                    _ = Enum.TryParse(certPath[0], out StoreName storeName);
                    _ = Enum.TryParse(certPath[1], out StoreLocation storeLocation);

                    var store = new X509Store(storeName, storeLocation);
                    store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
                    var certificateCollection = store.Certificates.Find(X509FindType.FindByThumbprint, certThumbPrint, false);

                    store.Close();

                    foreach (var certificate in certificateCollection)
                    {
                        if (certificate.Thumbprint == certThumbPrint)
                        {
                            return certificate;
                        }
                    }
                }

                throw new Exception($"Certificate could not be loaded using this path information {certPathLocation}");
            }
            else
            {
                if (!File.Exists(certFile))
                {
                    throw new FileNotFoundException($"Certificate file {certFile} does not exist");
                }

                using (var certfile = File.OpenRead(certFile))
                {
                    var certificateBytes = new byte[certfile.Length];
                    certfile.Read(certificateBytes, 0, (int)certfile.Length);
                    // Don't dispose the cert as that will lead to "m_safeCertContext is an invalid handle" errors when the confidential client actually uses the cert
                    return new X509Certificate2(certificateBytes,
                                                certPassword,
                                                X509KeyStorageFlags.Exportable |
                                                X509KeyStorageFlags.MachineKeySet |
                                                X509KeyStorageFlags.PersistKeySet);
                }
            }
        }

        private static string GetSiteFromTenant(string tenantName)
        {
            if (Uri.TryCreate(tenantName, UriKind.Absolute, out var uri))
            {
                return $"https://{uri.DnsSafeHost}";
            }
            else
            {
                return $"https://{tenantName}";
            }
        }

        private static void IncrementCallsDone()
        {
            readerWriterLock.EnterWriteLock();
            try
            {
                _ = Interlocked.Increment(ref callsDone);
            }
            finally
            {
                readerWriterLock.ExitWriteLock();
            }
        }

        private static int CallsDone()
        {
            readerWriterLock.EnterReadLock();
            try
            {
                return callsDone;
            }
            finally
            {
                readerWriterLock.ExitReadLock();
            }
        }
    }
}
