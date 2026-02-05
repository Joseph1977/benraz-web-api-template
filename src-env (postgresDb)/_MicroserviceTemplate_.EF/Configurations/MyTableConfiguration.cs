using _MicroserviceTemplate_.Domain.MyTables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _MicroserviceTemplate_.EF.Configurations
{
    public class MyTableConfiguration : IEntityTypeConfiguration<MyTable>
    {
        public void Configure(EntityTypeBuilder<MyTable> builder)
        {
            builder.ToTable("MyTables");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id).HasColumnName("MyTableId").HasColumnType("uuid");
            builder.Property(x => x.CreateTimeUtc).HasDefaultValueSql("CURRENT_TIMESTAMP AT TIME ZONE 'UTC'");
            builder.Property(x => x.UpdateTimeUtc).HasDefaultValueSql("CURRENT_TIMESTAMP AT TIME ZONE 'UTC'");
        }
    }
}
