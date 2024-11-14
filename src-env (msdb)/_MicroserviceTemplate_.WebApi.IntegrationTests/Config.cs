using _MicroserviceTemplate_.EF;
using _MicroserviceTemplate_.WebApi.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;

namespace _MicroserviceTemplate_.WebApi.IntegrationTests
{
    [SetUpFixture]
    public class Config
    {
        private static IConfiguration _configuration;

        public static _MicroserviceTemplate_DbContext DBContext;
        public static HttpClient HttpClient;

        [OneTimeSetUp]
        public static void SetUpFixture()
        {
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
            string connectionString = _configuration.GetValue<string>("ConnectionStrings");
            if (!String.IsNullOrWhiteSpace(connectionString))
            {
                if (IsInjectDbCredentialsToConnectionString())
                {
                    connectionString +=
                        $";User Id={_configuration.GetValue<string>("AspNetCoreDbUserName")};Password={_configuration.GetValue<string>("AspNetCoreDbPassword")}";
                }

                builder.UseSqlServer(connectionString); 
            }
            return new _MicroserviceTemplate_DbContext(builder.Options);
        }

        private static bool IsInjectDbCredentialsToConnectionString()
        {
            return _configuration.GetValue<bool>("InjectDBCredentialFromEnvironment");
        }
    }
}