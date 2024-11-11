using _MicroserviceTemplate_.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;

namespace _MicroserviceTemplate_.WebApi.Configuration
{
    /// <summary>
    /// Database configuration source.
    /// </summary>
    public class DatabaseConfigurationSource : IConfigurationSource
    {
        private readonly Action<DbContextOptionsBuilder<_MicroserviceTemplate_DbContext>> _options;

        /// <summary>
        /// Creates database configuration source.
        /// </summary>
        /// <param name="options">Database context options.</param>
        public DatabaseConfigurationSource(Action<DbContextOptionsBuilder<_MicroserviceTemplate_DbContext>> options)
        {
            _options = options;
        }

        /// <summary>
        /// Returns configuration provider.
        /// </summary>
        /// <param name="builder">Configuration builder.</param>
        /// <returns>Configuration provider.</returns>
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new DatabaseConfigurationProvider(_options);
        }
    }
}