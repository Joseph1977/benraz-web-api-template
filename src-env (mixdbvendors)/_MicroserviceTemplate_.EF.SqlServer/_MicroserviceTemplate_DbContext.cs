using Microsoft.EntityFrameworkCore;
using _MicroserviceTemplate_.EF.Data;

namespace _MicroserviceTemplate_.EF.SqlServer
{
    /// <summary>
    /// Database context.
    /// </summary>
    public class _MicroserviceTemplate_DbContext : DbSets<_MicroserviceTemplate_DbContext>
    {
        /// <summary>
        /// Creates context.
        /// </summary>
        /// <param name="options">Context options.</param>
        public _MicroserviceTemplate_DbContext(DbContextOptions<_MicroserviceTemplate_DbContext> options)
            : base(options)
        {
        }
    }
}