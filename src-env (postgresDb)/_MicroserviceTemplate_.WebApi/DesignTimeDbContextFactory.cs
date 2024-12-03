using _MicroserviceTemplate_.EF;
using Benraz.Infrastructure.Common.EnvironmentConfiguration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
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

            if (File.Exists(launchSettingsPath))
            {
                var jsonContent = File.ReadAllText(launchSettingsPath);
                var jsonReader = new JsonTextReader(new StringReader(jsonContent));
                var launchSettings = JObject.Load(jsonReader);

                var environmentVariables = launchSettings["profiles"]?["IIS Express"]?["environmentVariables"];
                if (environmentVariables != null)
                {
                    foreach (JProperty variable in environmentVariables)
                    {
                        Environment.SetEnvironmentVariable(variable.Name, variable.Value.ToString());
                    }
                }
            }

            // Load environment for default configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables()
                .Add(new CustomEnvironmentVariableConfigurationSource())
                .Build();

            if (IsCheckConnectionStringExists())
            {
                string connectionString = Environment.GetEnvironmentVariable("ConnectionStrings");

                if (IsInjectDbCredentialsToConnectionString())
                {
                    connectionString +=
                         $";Username={Environment.GetEnvironmentVariable("AspNetCoreDbUserName")};Password='{Environment.GetEnvironmentVariable("AspNetCoreDbPassword")}'";
                }

                var optionsBuilder = new DbContextOptionsBuilder<_MicroserviceTemplate_DbContext>();
                optionsBuilder.UseNpgsql(connectionString);
                return new _MicroserviceTemplate_DbContext(optionsBuilder.Options);
            }
            else
            {
                var optionsBuilder = new DbContextOptionsBuilder<_MicroserviceTemplate_DbContext>();
                return new _MicroserviceTemplate_DbContext(optionsBuilder.Options);
            }
        }

        private bool IsInjectDbCredentialsToConnectionString()
        {
            bool result;
            bool.TryParse(Environment.GetEnvironmentVariable("InjectDBCredentialFromEnvironment"), out result);
            return result;
        }

        private bool IsCheckConnectionStringExists()
        {
            bool isExists = true;
            string connectionString = Environment.GetEnvironmentVariable("ConnectionStrings");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                isExists = false;
            }
            return isExists;
        }
    }
}
