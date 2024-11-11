using _MicroserviceTemplate_.EF;
using _MicroserviceTemplate_.EF.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;

namespace _MicroserviceTemplate_.WebApi.Configuration
{
    /// <summary>
    /// Database configuration provider.
    /// </summary>
    public class DatabaseConfigurationProvider : ConfigurationProvider
    {
        private readonly Action<DbContextOptionsBuilder<_MicroserviceTemplate_DbContext>> _options;

        /// <summary>
        /// Creates database configuration provider.
        /// </summary>
        /// <param name="options">Database context options.</param>
        public DatabaseConfigurationProvider(
            Action<DbContextOptionsBuilder<_MicroserviceTemplate_DbContext>> options)
        {
            _options = options;
        }

        /// <summary>
        /// Loads configuration from database.
        /// </summary>
        public override void Load()
        {
            var builder = new DbContextOptionsBuilder<_MicroserviceTemplate_DbContext>();
            _options(builder);

            using (var context = new _MicroserviceTemplate_DbContext(builder.Options))
            {
                var repository = new SettingsEntriesRepository(context);
                try
                {
                    var settingsEntries = repository.GetAllAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                    Data = settingsEntries.ToDictionary(x => x.Id, x => x.Value);
                }
                catch (Exception)
                {
                    // Could be caused on the first migration
                }
            }
        }
    }
}