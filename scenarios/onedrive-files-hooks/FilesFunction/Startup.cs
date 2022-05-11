using FilesFunction.Services;
using IntegratedTokenCache;
using IntegratedTokenCache.Stores;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;
using System;
using System.Reflection;

[assembly: FunctionsStartup(typeof(FilesFunction.Startup))]
namespace FilesFunction
{
    /// <summary>
    /// Class that initializes this Azure functions host
    /// </summary>
    public class Startup : FunctionsStartup
    {
        // Override the Configure method to load configuration values
        // from the .NET user secret store
        public override void Configure(IFunctionsHostBuilder builder)
        {
            #region Configuration
            var config = new ConfigurationBuilder()
                .AddUserSecrets(Assembly.GetExecutingAssembly(), false)
                .AddEnvironmentVariables()
                .Build();

            // Make the loaded config available via dependency injection
            builder.Services.AddSingleton<IConfiguration>(config);
            var configuration = builder.GetContext().Configuration;
            #endregion

            #region Persist extra data in SQL
            // Add Sql Server as store to persist additional token data, using entity framework
            builder.Services.AddDbContextPool<IntegratedTokenCacheDbContext>(options =>
                options.UseSqlServer(config["ConnectionStrings:DefaultConnection"]));

            // Configure the dependency injection for IMsalAccountActivityStore to use a SQL Server to store the entity MsalAccountActivity.
            // You might want to customize this class, or implement our own, with logic that fits your business need.
            builder.Services.AddScoped<IMsalAccountActivityStore, SqlServerMsalAccountActivityStore>();
            #endregion

            #region MSAL token caching into SQL
            // Add custom token caching, depends on the Sql server distributed token cache
            builder.Services.AddIntegratedUserTokenCache();
            // Add Sql Server as distributed Token cache store
            builder.Services.AddDistributedSqlServerCache(options =>
            {
                /*
                    dotnet tool install --global dotnet-sql-cache
                    dotnet sql-cache create "Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=MsalTokenCacheDatabase;Integrated Security=True;" dbo TokenCache    
                */
                options.ConnectionString = config["ConnectionStrings:DefaultConnection"];
                options.SchemaName = "dbo";
                options.TableName = "TokenCache";

                if (int.TryParse(config["Files:MsalCacheLifeTime"], out int cacheLifeTimeInMinutes) && cacheLifeTimeInMinutes != 0)
                {
                    //Once expired, the cache entry is automatically deleted by Microsoft.Extensions.Caching.SqlServer library
                    options.DefaultSlidingExpiration = TimeSpan.FromMinutes(cacheLifeTimeInMinutes);
                }
                else
                {
                    options.DefaultSlidingExpiration = TimeSpan.FromHours(30*24);
                }
            });
            #endregion

            #region Configure authentication
            builder.Services.AddAuthentication(sharedOptions =>
            {
                sharedOptions.DefaultScheme = "Bearer";
                sharedOptions.DefaultChallengeScheme = "Bearer";
            })
                .AddMicrosoftIdentityWebApi(configuration)
                    .EnableTokenAcquisitionToCallDownstreamApi();
            #endregion

            #region Configure Microsoft Graph usage
            builder.Services.AddMicrosoftGraph(configuration, Constants.BasePermissionScopes);
            #endregion
        }
    }
}
