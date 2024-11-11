using _MicroserviceTemplate_.Domain.Settings;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace _MicroserviceTemplate_.EF
{
    /// <summary>
    /// Database context.
    /// </summary>
    public class _MicroserviceTemplate_DbContext : DbContext
    {
        /// <summary>
        /// Settings entries.
        /// </summary>
        public DbSet<SettingsEntry> SettingsEntries { get; set; }

        /// <summary>
        /// Creates context.
        /// </summary>
        /// <param name="options">Context options.</param>
        public _MicroserviceTemplate_DbContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(
                Assembly.GetAssembly(typeof(_MicroserviceTemplate_DbContext)));
        }
    }
}