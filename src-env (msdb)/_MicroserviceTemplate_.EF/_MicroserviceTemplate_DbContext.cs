using _MicroserviceTemplate_.Domain.MyTables;
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
        /// My tables.
        /// </summary>
        public DbSet<MyTable> MyTables { get; set; }

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