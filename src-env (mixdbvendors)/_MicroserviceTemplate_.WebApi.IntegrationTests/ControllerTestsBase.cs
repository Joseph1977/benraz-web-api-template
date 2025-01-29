using Benraz.Infrastructure.Common.CommonUtilities;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using System.Net.Http;
using System.Threading.Tasks;
#if SQLSERVER
using _MicroserviceTemplate_.EF.SqlServer;
#else
using _MicroserviceTemplate_.EF.PostgreSql;
#endif

namespace _MicroserviceTemplate_.WebApi.IntegrationTests
{
    [TestFixture]
    public abstract class ControllerTestsBase
    {
        protected HttpClient HttpClient;
        protected _MicroserviceTemplate_DbContext DBContext;

        [SetUp]
        public virtual async Task SetUpAsync()
        {
            HttpClient = Config.HttpClient;
            DBContext = Config.DBContext;

            var _connectionString = CommonUtilities.IsNeedToConnectToDB(Config._configuration.GetValue<string>("ConnectionStrings"), Config._configuration.GetValue<bool>("SkipDbConnectIfNoConnectionString"));

            if (!_connectionString)
            {
                Assert.Ignore("Connection string is missing. Skipping test.");
            }
            else
            {
                await ClearDataAsync();
            }
        }

        [TearDown]
        public virtual async Task TearDownAsync()
        {
            var _connectionString = CommonUtilities.IsNeedToConnectToDB(Config._configuration.GetValue<string>("ConnectionStrings"), Config._configuration.GetValue<bool>("SkipDbConnectIfNoConnectionString"));

            if (_connectionString)
            {
                await ClearDataAsync();
            }
        }

        protected virtual Task ClearDataAsync()
        {
            return Task.CompletedTask;
        }

        protected _MicroserviceTemplate_DbContext CreateDbContext()
        {
            return Config.CreateDbContext();
        }
    }
}