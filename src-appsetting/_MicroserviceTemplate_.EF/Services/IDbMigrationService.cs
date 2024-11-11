using System.Threading.Tasks;

namespace _MicroserviceTemplate_.EF.Services
{
    /// <summary>
    /// Database migration service.
    /// </summary>
    public interface IDbMigrationService
    {
        /// <summary>
        /// Migrates database.
        /// </summary>
        /// <returns>Task.</returns>
        Task MigrateAsync();
    }
}