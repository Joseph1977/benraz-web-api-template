using _MicroserviceTemplate_.Domain.MyTables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _MicroserviceTemplate_.EF.DataConfigurations
{
    public class MyTableDataConfiguration : IEntityTypeConfiguration<MyTable>
    {
        public void Configure(EntityTypeBuilder<MyTable> builder)
        {
            builder.HasData(
                Create("Operation:Add", "Add", "Add a new record."),
                Create("Operation:Edit", "Edit", "Edit existing record."),
                Create("Operation:Remove", "Remove", "Remove existing record."));
        }

        private MyTable Create(string id, object value, string description = null)
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
