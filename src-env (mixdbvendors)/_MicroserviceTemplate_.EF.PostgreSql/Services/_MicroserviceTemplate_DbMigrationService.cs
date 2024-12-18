using System.Threading.Tasks;
using Benraz.Infrastructure.Common.DataRedundancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace _MicroserviceTemplate_.EF.PostgreSql.Services
{
    /// <summary>
    /// Database migration service.
    /// </summary>
    public class _MicroserviceTemplate_DbMigrationService : IDbMigrationService
    {
        private readonly IConfigurationRoot _configurationRoot;
        private readonly IDrChecker _drChecker;
        private readonly _MicroserviceTemplate_DbContext _context;

        /// <summary>
        /// Creates service.
        /// </summary>
        /// <param name="configuration">Configuration root.</param>
        /// <param name="drChecker">Data redundancy checker.</param>
        /// <param name="context">Database context.</param>
        public _MicroserviceTemplate_DbMigrationService(
            IConfiguration configuration,
            IDrChecker drChecker,
            _MicroserviceTemplate_DbContext context)
        {
            _configurationRoot = (IConfigurationRoot)configuration;
            _drChecker = drChecker;
            _context = context;
        }

        /// <summary>
        /// Migrates database.
        /// </summary>
        /// <returns>Task.</returns>
        public async Task MigrateAsync()
        {
            if (!_drChecker.IsActiveDR())
            {
                return;
            }

            await _context.Database.MigrateAsync();

            _configurationRoot.Reload();
        }
    }
}