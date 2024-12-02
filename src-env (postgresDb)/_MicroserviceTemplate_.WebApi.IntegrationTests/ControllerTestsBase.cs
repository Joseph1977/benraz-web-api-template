using _MicroserviceTemplate_.EF;
using NUnit.Framework;
using System.Net.Http;
using System.Threading.Tasks;

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

            var _connectionString = Config.IsCheckConnectionStringExists();

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
            var _connectionString = Config.IsCheckConnectionStringExists();

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