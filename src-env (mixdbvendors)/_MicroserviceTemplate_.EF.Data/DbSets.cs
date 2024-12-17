using System.Reflection;
using _MicroserviceTemplate_.Domain.MyTables;
using Microsoft.EntityFrameworkCore;

namespace _MicroserviceTemplate_.EF.Data;

public abstract class DbSets<TContext> : DbContext where TContext : DbContext
{
    /// <summary>
    /// My tables.
    /// </summary>
    public DbSet<MyTable> MyTables { get; set; }
    
    /// <summary>
    /// Creates context.
    /// </summary>
    /// <param name="options">Context options.</param>
    protected DbSets(DbContextOptions<TContext> options)
        : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var providerName = this.Database.ProviderName;

        if (providerName == "Microsoft.EntityFrameworkCore.SqlServer")
        {
            GlobalConfig.DatabaseProvider = DatabaseProvider.SqlServer;
        }
        else if (providerName == "Npgsql.EntityFrameworkCore.PostgreSQL")
        {
            GlobalConfig.DatabaseProvider = DatabaseProvider.PostgreSql;
        }
        
        modelBuilder.ApplyConfigurationsFromAssembly(
            Assembly.GetAssembly(typeof(DbSets<TContext>)));
    }
}