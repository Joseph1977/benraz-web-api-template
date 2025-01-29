using _MicroserviceTemplate_.EF;
using Benraz.Infrastructure.Common.CommonUtilities;
using Benraz.Infrastructure.Common.EnvironmentConfiguration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;

namespace _MicroserviceTemplate_.WebApi.IntegrationTests
{
    [SetUpFixture]
    public class Config
    {
        public static IConfiguration _configuration;
        public static _MicroserviceTemplate_DbContext DBContext;
        public static HttpClient HttpClient;

        [OneTimeSetUp]
        public static void SetUpFixture()
        {
            // Load EnvironmentVariables.json
            var launchSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "Properties", "EnvironmentVariables.json");
            CommonUtilities.LoadEnvironmentVariablesFromJson(launchSettingsPath);

            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("Properties/EnvironmentVariables.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Add(new CustomEnvironmentVariableConfigurationSource())
                .Build();

            DBContext = CreateDbContext();

            var server = new TestServer(new WebHostBuilder()
                .UseConfiguration(_configuration)
                .UseStartup<StartupStub>());

            HttpClient = server.CreateClient();

            var token = _configuration.GetValue<string>("AccessToken");
            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public static _MicroserviceTemplate_DbContext CreateDbContext()
        {
            var builder = new DbContextOptionsBuilder<_MicroserviceTemplate_DbContext>().EnableSensitiveDataLogging();
            if (CommonUtilities.IsNeedToConnectToDB(_configuration.GetValue<string>("ConnectionStrings"), _configuration.GetValue<bool>("SkipDbConnectIfNoConnectionString")))
            {
                string connectionString = CommonUtilities.GetConnectString(_configuration);
                builder.UseNpgsql(connectionString);
            }
            return new _MicroserviceTemplate_DbContext(builder.Options);
        }
    }
}