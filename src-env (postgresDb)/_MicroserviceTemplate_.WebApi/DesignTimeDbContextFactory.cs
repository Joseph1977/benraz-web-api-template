using _MicroserviceTemplate_.EF;
using Benraz.Infrastructure.Common.CommonUtilities;
using Benraz.Infrastructure.Common.EnvironmentConfiguration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace _MicroserviceTemplate_.WebApi
{
    /// <summary>
    /// Design time db context factory.
    /// </summary>
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<_MicroserviceTemplate_DbContext>
    {
        /// <summary>
        /// Create db context.
        /// </summary>
        /// <param name="args">Args.</param>
        /// <returns>_MicroserviceTemplate_ db context.</returns>
        public _MicroserviceTemplate_DbContext CreateDbContext(string[] args)
        {
            // Load launchSettings.json
            var launchSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "Properties", "launchSettings.json");
            CommonUtilities.LoadEnvironmentVariablesFromJson(launchSettingsPath, "profiles['IIS Express'].environmentVariables");

            // Load environment for default configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables()
                .Add(new CustomEnvironmentVariableConfigurationSource())
                .Build();

            string connectionString = null;
            if (CommonUtilities.IsNeedToConnectToDB(configuration.GetValue<string>("ConnectionStrings"), configuration.GetValue<bool>("SkipDbConnectIfNoConnectionString")))
            {
                connectionString = CommonUtilities.GetConnectString(configuration);
            }
            var optionsBuilder = new DbContextOptionsBuilder<_MicroserviceTemplate_DbContext>();
            optionsBuilder.UseNpgsql(connectionString);
            return new _MicroserviceTemplate_DbContext(optionsBuilder.Options);
        }
    }
}
