using _MicroserviceTemplate_.Domain.MyTables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _MicroserviceTemplate_.EF.Data.DataConfigurations
{
    public class MyTableDataConfiguration : IEntityTypeConfiguration<MyTable>
    {
        public void Configure(EntityTypeBuilder<MyTable> builder)
        {
            builder.HasData(
                Create(new Guid("c209d3b5-6482-439a-be59-3471c170cae9"), "Add", "Add a new record."),
                Create(new Guid("e5ecdbee-834f-42e7-a25d-b0aa00d3fcf1"), "Edit", "Edit existing record."),
                Create(new Guid("8ec418ae-69cf-44f0-a1f3-ba26142b3563"), "Remove", "Remove existing record."));
        }

        private MyTable Create(Guid id, object value, string description = null)
        {
            return new MyTable
            {
                Id = id,
                Value = value?.ToString(),
                Description = description
            };
        }
    }
}
